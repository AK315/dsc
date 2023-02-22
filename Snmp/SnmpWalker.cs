namespace dsc.Snmp;

using System.Collections.Generic;
using System.Net;
using System.Text;
using SnmpSharpNet;

public static class SnmpWalker
{
    public static void v1GetNext()
    {
        // SNMP community name
        OctetString community = new OctetString("public");

        // Define agent parameters class
        AgentParameters param = new AgentParameters(community);
        // Set SNMP version to 1
        param.Version = SnmpVersion.Ver1;
        // Construct the agent address object
        // IpAddress class is easy to use here because
        //  it will try to resolve constructor parameter if it doesn't
        //  parse to an IP address
        IpAddress agent = new IpAddress("100.0.0.100");

        // Construct target
        UdpTarget target = new UdpTarget((IPAddress)agent, 161, 2000, 1);

        // Define Oid that is the root of the MIB
        //  tree you wish to retrieve
        Oid rootOid = new Oid("1.3.6.1.2.1.2.2.1.2"); // ifDescr

        // This Oid represents last Oid returned by
        //  the SNMP agent
        Oid lastOid = (Oid)rootOid.Clone();

        // Pdu class used for all requests
        Pdu pdu = new Pdu(PduType.GetNext);

        // Loop through results
        while (lastOid != null)
        {
            // When Pdu class is first constructed, RequestId is set to a random value
            // that needs to be incremented on subsequent requests made using the
            // same instance of the Pdu class.
            if (pdu.RequestId != 0)
            {
                pdu.RequestId += 1;
            }
            // Clear Oids from the Pdu class.
            pdu.VbList.Clear();
            // Initialize request PDU with the last retrieved Oid
            pdu.VbList.Add(lastOid);
            // Make SNMP request
            SnmpV1Packet result = (SnmpV1Packet)target.Request(pdu, param);
            // You should catch exceptions in the Request if using in real application.

            // If result is null then agent didn't reply or we couldn't parse the reply.
            if (result != null)
            {
                // ErrorStatus other then 0 is an error returned by 
                // the Agent - see SnmpConstants for error definitions
                if (result.Pdu.ErrorStatus != 0)
                {
                    // agent reported an error with the request
                    Console.WriteLine("Error in SNMP reply. Error {0} index {1}", 
                        result.Pdu.ErrorStatus,
                        result.Pdu.ErrorIndex);
                    lastOid = null;
                    break;
                }
                else
                {
                    // Walk through returned variable bindings
                    foreach (Vb v in result.Pdu.VbList)
                    {
                        // Check that retrieved Oid is "child" of the root OID
                        if (rootOid.IsRootOf(v.Oid))
                        {
                            Console.WriteLine("{0} ({1}): {2}",
                                v.Oid.ToString(), 
                                SnmpConstants.GetTypeName(v.Value.Type), 
                                v.Value.ToString());
                            lastOid = v.Oid;
                        }
                        else
                        {
                            // we have reached the end of the requested
                            // MIB tree. Set lastOid to null and exit loop
                            lastOid = null;
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("No response received from SNMP agent.");
            }
        }
        target.Close();
    }

    public static void v2GetBulk()
    {
        // SNMP community name
        OctetString community = new OctetString("public");

        // Define agent parameters class
        AgentParameters param = new AgentParameters(community);
        // Set SNMP version to 2 (GET-BULK only works with SNMP ver 2 and 3)
        param.Version = SnmpVersion.Ver2;
        // Construct the agent address object
        // IpAddress class is easy to use here because
        //  it will try to resolve constructor parameter if it doesn't
        //  parse to an IP address
        IpAddress agent = new IpAddress("172.0.0.1");

        // Construct target
        UdpTarget target = new UdpTarget((IPAddress)agent, 161, 2000, 1);

        // Define Oid that is the root of the MIB
        //  tree you wish to retrieve
        Oid rootOid = new Oid("1.3.6.1.2.1.2.2.1.2"); // ifDescr

        // This Oid represents last Oid returned by
        //  the SNMP agent
        Oid lastOid = (Oid)rootOid.Clone();

        // Pdu class used for all requests
        Pdu pdu = new Pdu(PduType.GetBulk);

        // In this example, set NonRepeaters value to 0
        pdu.NonRepeaters = 0;
        // MaxRepetitions tells the agent how many Oid/Value pairs to return
        // in the response.
        pdu.MaxRepetitions = 5;

        // Loop through results
        while (lastOid != null)
        {
            // When Pdu class is first constructed, RequestId is set to 0
            // and during encoding id will be set to the random value
            // for subsequent requests, id will be set to a value that
            // needs to be incremented to have unique request ids for each
            // packet
            if (pdu.RequestId != 0)
            {
                pdu.RequestId += 1;
            }
            // Clear Oids from the Pdu class.
            pdu.VbList.Clear();
            // Initialize request PDU with the last retrieved Oid
            pdu.VbList.Add(lastOid);
            // Make SNMP request
            SnmpV2Packet result = (SnmpV2Packet)target.Request(pdu, param);
            // You should catch exceptions in the Request if using in real application.

            // If result is null then agent didn't reply or we couldn't parse the reply.
            if (result != null)
            {
                // ErrorStatus other then 0 is an error returned by 
                // the Agent - see SnmpConstants for error definitions
                if (result.Pdu.ErrorStatus != 0)
                {
                    // agent reported an error with the request
                    Console.WriteLine("Error in SNMP reply. Error {0} index {1}", 
                        result.Pdu.ErrorStatus,
                        result.Pdu.ErrorIndex);
                    lastOid = null;
                    break;
                }
                else
                {
                    // Walk through returned variable bindings
                    foreach (Vb v in result.Pdu.VbList)
                    {
                        // Check that retrieved Oid is "child" of the root OID
                        if (rootOid.IsRootOf(v.Oid))
                        {
                            Console.WriteLine("{0} ({1}): {2}",
                                v.Oid.ToString(), 
                                SnmpConstants.GetTypeName(v.Value.Type), 
                                v.Value.ToString());
                            if (v.Value.Type == SnmpConstants.SMI_ENDOFMIBVIEW)
                                lastOid = null;
                            else
                                lastOid = v.Oid;
                        }
                        else
                        {
                            // we have reached the end of the requested
                            // MIB tree. Set lastOid to null and exit loop
                            lastOid = null;
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("No response received from SNMP agent.");
            }
        }
        target.Close();
    }

    public static void v2GetTable(string ip, string oid)
    {
        Dictionary<string, Dictionary<uint, AsnType>> result = new Dictionary<string, Dictionary<uint, AsnType>>();
        
        // Not every row has a value for every column so keep track of all columns available in the table
        List<uint> tableColumns = new List<uint>();
        
        // Prepare agent information
        AgentParameters param = new AgentParameters(SnmpVersion.Ver2, new OctetString("public"));

//        IpAddress peer = new IpAddress("100.0.0.100");
        IpAddress peer = new IpAddress(ip);
        if( ! peer.Valid ) {
            Console.WriteLine("Unable to resolve name or error in address for peer: {0}", "100.0.0.100");
            return;
        }

        UdpTarget target = new UdpTarget((IPAddress)peer);
        
        // This is the table OID
//        Oid startOid = new Oid(".1.3.6.1.2.1.4.22");
//        Oid startOid = new Oid(".1.3.6.1.2.1.2.2");
//        Oid startOid = new Oid(".1.3.6.1.2.1.2.2.1.6.1");
        Oid startOid = new Oid(oid);
        
        
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
        
        // Keep looping through results until end of table
        while( startOid.IsRootOf(curOid) )
        {
            SnmpPacket? res = null;
            try 
            {
                res = target.Request(bulkPdu, param);
            }
            catch( Exception ex ) 
            {
                Console.WriteLine("Request failed: {0}", ex.Message);
                target.Close();
                return;
            }

            // For GetBulk request response has to be version 2
            if( res.Version != SnmpVersion.Ver2 ) 
            {
                Console.WriteLine("Received wrong SNMP version response packet.");
                target.Close();
                return;
            }

            // Check if there is an agent error returned in the reply
            if( res.Pdu.ErrorStatus != 0 ) 
            {
                Console.WriteLine("SNMP agent returned error {0} for request Vb index {1}", res.Pdu.ErrorStatus, res.Pdu.ErrorIndex);
                target.Close();
                return;
            }
            
            // Go through the VbList and check all replies
            foreach( Vb v in res.Pdu.VbList ) 
            {
                curOid = (Oid)v.Oid.Clone();
            
                // VbList could contain items that are past the end of the requested table.
                // Make sure we are dealing with an OID that is part of the table
                if(startOid.IsRootOf(v.Oid)) 
                {
                    // Get child Id's from the OID (past the table.entry sequence)
                    uint[] childOids = Oid.GetChildIdentifiers(startOid, v.Oid);
                    
                    // Get the value instance and converted it to a dotted decimal string to use as key in result dictionary
                    uint[] instance = new uint[childOids.Length-1];
                    Array.Copy(childOids, 1, instance, 0, childOids.Length-1 );
                    
                    String strInst = InstanceToString(instance);
                    
                    // Column id is the first value passed to <table oid>.entry in the response OID
                    uint column = childOids[0];
            
                    if(!tableColumns.Contains(column))
                        tableColumns.Add(column);

                    if(result.ContainsKey(strInst)) 
                    {
                        result[strInst][column] = (AsnType)v.Value.Clone();
                    } 
                    else 
                    {
                        result[strInst] = new Dictionary<uint, AsnType>();
                        result[strInst][column] = (AsnType)v.Value.Clone();
                    }
                } else {
                    // We've reached the end of the table. No point continuing the loop
                    break;
                }
            }
            // If last received OID is within the table, build next request
            if( startOid.IsRootOf(curOid) ) {
                bulkPdu.VbList.Clear();
                bulkPdu.VbList.Add(curOid);
                bulkPdu.NonRepeaters = 0;
                bulkPdu.MaxRepetitions = 100;
            }
        }
        target.Close();

        if(result.Count <= 0)
        {
            Console.WriteLine("No results returned.\n");
        } 
        else 
        {
            Console.Write("Instance");
            
            foreach( uint column in tableColumns ) 
            {
                Console.Write("\tColumn id {0} |", column);
            }

            Console.WriteLine("");
            
            foreach( KeyValuePair<string, Dictionary<uint, AsnType>> kvp in result ) 
            {
                Console.Write("{0}", kvp.Key);
            
                foreach(uint column in tableColumns) 
                {
                    if( kvp.Value.ContainsKey(column) ) 
                    {
                        Console.Write("\t{0} ({1}) |", kvp.Value[column].ToString(), SnmpConstants.GetTypeName(kvp.Value[column].Type));
                    } 
                    else 
                    {
                        Console.Write("\t|");
                    }
                }
                Console.WriteLine("");
            }
        }
    }
    public static string InstanceToString(uint[] instance) {
        StringBuilder str = new StringBuilder();
        foreach( uint v in instance ) {
            if( str.Length == 0 )
                str.Append(v);
            else
                str.AppendFormat(".{0}", v);
        }
        return str.ToString();
    }        
}