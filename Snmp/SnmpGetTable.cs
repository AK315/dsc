using System.Collections.Generic;
using System.Net;
using System.Text;
using SnmpSharpNet;

namespace dsc.Snmp;

public class SnmpGetTable
{
    private readonly IPAddress _ip;
    private readonly Oid _startOid;

    private readonly string _communityName;

    public SnmpGetTable(IPAddress ip, string oid, string communityName = "public")
    {
        if(string.IsNullOrEmpty(oid))
            throw new ArgumentNullException(nameof(oid));

        _ip = ip;
        _startOid = new Oid(oid);
        _communityName = communityName;
    }

    public Task<IDictionary<string, IDictionary<uint, AsnType>>?> GetTableAsync()
    {
        return Task.Factory.StartNew<IDictionary<string, IDictionary<uint, AsnType>>?>(() =>
        {
            IDictionary<string, IDictionary<uint, AsnType>>? result = new Dictionary<string, IDictionary<uint, AsnType>>();

            // Not every row has a value for every column so keep track of all columns available in the table
            //        List<uint> tableColumns = new List<uint>();

            AgentParameters agentParameters = new AgentParameters(SnmpVersion.Ver2, new OctetString("public"));
            // IpAddress peer = new IpAddress(_ip);

            // if(!peer.Valid)
            //     throw new FormatException($"Name or IP address of the node is in invalid format: {peer}");

            UdpTarget target = new UdpTarget(_ip);

            // This is the table OID
            //Oid startOid = new Oid(".1.3.6.1.2.1.4.22");
            //        Oid startOid = new Oid(".1.3.6.1.2.1.2.2");
            Oid startOid = new Oid(_startOid);

            // Each table OID is followed by .1 for the entry OID. Add it to the table OID
            startOid.Add(1); // Add Entry OID to the end of the table OID

            // Prepare the request PDU
            Pdu bulkPdu = Pdu.GetBulkPdu();
            bulkPdu.VbList.Add(startOid);

            // We don't need any NonRepeaters
            bulkPdu.NonRepeaters = 0;

            // Tune MaxRepetitions to the number best suited to retrive the data
            bulkPdu.MaxRepetitions = 100;

            // Current OID will keep track of the last retrieved OID and be used as indication that we have reached end of table
            Oid curOid = (Oid)startOid.Clone();

            try
            {
                // Keep looping through results until end of table
                while (startOid.IsRootOf(curOid))
                {
                    SnmpPacket? res = null;

                    res = target.Request(bulkPdu, agentParameters);

                    if (res.Version != SnmpVersion.Ver2)
                        throw new Exception("Wrong version of SNMP response. Expected version 2.");

                    if (res.Pdu.ErrorStatus != 0)
                        throw new Exception($"ANMP agent returned error {res.Pdu.ErrorStatus} for request index {res.Pdu.ErrorIndex}");


                    // Go through the VbList and check all replies
                    foreach (Vb v in res.Pdu.VbList)
                    {
                        curOid = (Oid)v.Oid.Clone();

                        // VbList could contain items that are past the end of the requested table.
                        // Make sure we are dealing with an OID that is part of the table
                        if (startOid.IsRootOf(v.Oid))
                        {
                            // Get child Id's from the OID (past the table.entry sequence)
                            uint[] childOids = Oid.GetChildIdentifiers(startOid, v.Oid);

                            // Get the value instance and converted it to a dotted decimal string to use as key in result dictionary
                            uint[] instance = new uint[childOids.Length - 1];
                            Array.Copy(childOids, 1, instance, 0, childOids.Length - 1);

                            String strInst = InstanceToString(instance);

                            // Column id is the first value passed to <table oid>.entry in the response OID
                            uint column = childOids[0];

                            // if(!tableColumns.Contains(column))
                            //     tableColumns.Add(column);

                            if (result.ContainsKey(strInst))
                                result[strInst][column] = (AsnType)v.Value.Clone();
                            else
                            {
                                result[strInst] = new Dictionary<uint, AsnType>();
                                result[strInst][column] = (AsnType)v.Value.Clone();
                            }
                        }
                        else
                        {
                            // We've reached the end of the table. No point continuing the loop
                            break;
                        }
                    }
                    // If last received OID is within the table, build next request
                    if (startOid.IsRootOf(curOid))
                    {
                        bulkPdu.VbList.Clear();
                        bulkPdu.VbList.Add(curOid);

                        bulkPdu.NonRepeaters = 0;
                        bulkPdu.MaxRepetitions = 100;
                    }
                }
            }
            finally
            {
                target.Close();
            }

            return result;
        });
    }
    protected string InstanceToString(uint[] instance)
    {
        StringBuilder str = new StringBuilder();
    
        foreach( uint v in instance ) 
        {
            if( str.Length == 0 )
                str.Append(v);
            else
                str.AppendFormat(".{0}", v);
        }
        return str.ToString();
    }        
}