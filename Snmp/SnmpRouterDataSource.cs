using System.Net;
using SnmpSharpNet;
using dsc.Model;

namespace dsc.Snmp;

public class SnmpRouterDataSource : IRouterDataSource
{
    /// <summary>
    /// Returns router system name
    /// </summary>
    /// <param name="ip">Router's IP address</param>
    /// <returns>Router's system name</returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<string?> GetRouterSystemName(IPAddress ip)
    {
        SnmpDeviceService deviceService = new SnmpDeviceService(ip);
        string? name = await deviceService.GetStringValueAsync(SnmpOid.RouterSystemName);

        return name;
    }

    /// <summary>
    /// Returns router's interface collection
    /// </summary>
    /// <param name="ip">Router's IP address</param>
    /// <returns>Router's interface collection</returns>
    public async Task<ICollection<IpInterface>> GetRouterInterfacesAsync(IPAddress ip)
    {
        ICollection<IpInterface> result = new List<IpInterface>();

        SnmpDeviceService deviceService = new SnmpDeviceService(ip);
        
        // Extracting interfaces data
        var interfaceTable = await deviceService.GetTableAsync(SnmpOid.RouterInterfaceTable);

        if (interfaceTable != null)
        {
            foreach (KeyValuePair<string, IDictionary<uint, AsnType>>? p in interfaceTable)
            {
                // Extracting interface index
                int index = default(int);
                int.TryParse(p?.Value[1]?.ToString(), out index);

                // Extracting interface description
                string? description = p?.Value[2]?.ToString();

                // Extracting interface physical address (MAC)
                string? macString = new string(p?.Value[6]?.ToString()?.ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray());
                
                UInt64 mac;
                if(UInt64.TryParse(macString, System.Globalization.NumberStyles.HexNumber, null, out mac))
                    result.Add(new IpInterface(index, description, mac));
            }
        }

        // Extracting IP addresses data
        var addressTable = await deviceService.GetTableAsync(SnmpOid.RouterInterfaceIpAddressTable);
        if (addressTable != null)
        {
            foreach (KeyValuePair<string, IDictionary<uint, AsnType>>? p in addressTable)
            {
                // Extracting interface index
                int index = default(int);
                if(int.TryParse(p?.Value[2]?.ToString(), out index))
                {
                    // Extracting IP address
                    string? ipAddress = p?.Value[1]?.ToString();
                    IPAddress? Ip;

                    // Extractin Net mask
                    string? netMask = p?.Value[3]?.ToString();
                    IPAddress? Mask;
                    if(!string.IsNullOrEmpty(ipAddress) 
                        && !string.IsNullOrEmpty(netMask)
                        && IPAddress.TryParse(ipAddress, out Ip)
                        && IPAddress.TryParse(netMask, out Mask))
                    {
                        var ifaces = result.Where(i => i.Index == index);
                        foreach(var iface in ifaces)
                            if (iface != null)
                            {
                                iface.Ip = Ip;
                                iface.Mask = Mask;
                            }
                    }
                }
            }
        }


        return result;
    }
}