using System.Collections.Generic;
using System.Net;
using System.Text;
using SnmpSharpNet;

namespace dsc.Snmp;

/// <summary>
/// Class for requesting device parameters over SNMP
/// </summary>
public class SnmpDeviceService
{
    private readonly IPAddress _ip;
    private readonly string _communityName;

    private readonly int _port;

    private readonly int _timeout;

    private readonly int _retry;

    public SnmpDeviceService(IPAddress ip, int port = 161, string communityName = "public", int timeout = 2000, int retry = 0)
    {
        _ip = ip;
        _communityName = communityName;
        _port = port;
        _timeout = timeout;
        _retry = retry;
    }

    /// <summary>
    /// Returns value of a parameter with the specified OID
    /// </summary>
    /// <param name="oid">Device parameter OID</param>
    /// <returns>Device parameter value</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    protected AsnType? GetPatameterValue(string oid)
    {        
        if(string.IsNullOrEmpty(oid))
            throw new ArgumentNullException(nameof(oid));

        AsnType? result = null;

        // Define agent parameters class
        AgentParameters param = new AgentParameters(new OctetString(_communityName));
        
        // Set SNMP version to 1
        param.Version = SnmpVersion.Ver1;
        
        // Construct target
        UdpTarget target = new UdpTarget(_ip, _port, _timeout, _retry);

        // Define Oid that is the root of the MIB tree you wish to retrieve
        Oid rootOid = new Oid(oid); 

        // Pdu class used for all requests
        Pdu pdu = new Pdu(PduType.Get);
        pdu.VbList.Clear();
        pdu.VbList.Add(rootOid);

        try
        {
            // Make SNMP request
            SnmpV1Packet response = (SnmpV1Packet)target.Request(pdu, param);
            if (response != null)
            {
                if (response.Pdu.ErrorStatus != 0)
                    throw new Exception($"ANMP agent returned error {response.Pdu.ErrorStatus} for request index {response.Pdu.ErrorIndex}");

                if(response.Pdu.VbList.Count() > 0)
                    result = response.Pdu.VbList[0].Value;
            }
        }
        catch(SnmpException e)
        {
            throw new ApplicationException($"An error occurred while sending SNMP request to [{ _ip.ToString() }]: { e.Message } ", e);
        }
        catch
        {
            throw;
        }
        finally
        {
            target.Close();
        }

        return result;
    }

    /// <summary>
    /// Returns value of type int of a parameter with the specified OID
    /// </summary>
    /// <param name="oid">Device parameter OID</param>
    /// <returns>Device parameter value of type int</returns>
    public async Task<int?> GetIntValueAsync(string oid)
    {
        return await Task.Run<int?>(() => 
        {
            int result;

            AsnType? value = GetPatameterValue(oid);
            if(value != null && int.TryParse(value.ToString(), out result))
                return result;

            return null;
        });
    }

    /// <summary>
    /// Returns value of type string of a parameter with the specified OID
    /// </summary>
    /// <param name="oid">Device parameter OID</param>
    /// <returns>Device parameter value of type string</returns>
    public async Task<string?> GetStringValueAsync(string oid)
    {
        return await Task.Run<string?>(() => 
        {
            AsnType? value = GetPatameterValue(oid);
            if(value != null)
                return value.ToString();

            return null;
        });
    }

    /// <summary>
    /// Returns table of values of a parameter with the specified OID
    /// </summary>
    /// <param name="oid">Device parameter OID</param>
    /// <returns>Device parameter value in table representation</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task<IDictionary<string, IDictionary<uint, AsnType>>?> GetTableAsync(string oid)
    {
        if(string.IsNullOrEmpty(oid))
            throw new ArgumentNullException(nameof(oid));

        Oid startOid = new Oid(oid);

        return await Task.Run<IDictionary<string, IDictionary<uint, AsnType>>?>(() =>
        {
            IDictionary<string, IDictionary<uint, AsnType>>? result = new Dictionary<string, IDictionary<uint, AsnType>>();

            AgentParameters agentParameters = new AgentParameters(SnmpVersion.Ver2, new OctetString(_communityName));

            UdpTarget target = new UdpTarget(_ip, _port, _timeout, _retry);

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