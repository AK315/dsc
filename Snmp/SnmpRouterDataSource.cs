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
        
        // Collecting interfaces data
        var interfaceTable = await deviceService.GetTableAsync(SnmpOid.RouterInterfaceTable);

        if (interfaceTable != null)
        {
            ParseInterfaceTable(interfaceTable, ref result);

            // Collecting IP addresses data
            var addressTable = await deviceService.GetTableAsync(SnmpOid.RouterInterfaceIpAddressTable);
           
            if (addressTable != null)
                ParseIpAddressesTable(addressTable, ref result);
        }

        return result;
    }

    /// <summary>
    /// Returns router's next hops collection
    /// </summary>
    /// <param name="ip">Router's IP address</param>
    /// <returns></returns>
    public async Task<ICollection<IPAddress>> GetRouterNextHopsAsync(IPAddress ip)
    {
        ICollection<IPAddress> result = new List<IPAddress>();

        SnmpDeviceService deviceService = new SnmpDeviceService(ip);
        
        // Collecting interfaces data
        var routingTable = await deviceService.GetTableAsync(SnmpOid.RouterRoutingTable);

        if (routingTable != null)
            ParseRoutingTable(routingTable, ref result);

        return result;
    }

    /// <summary>
    /// Parses SNMP table of router IP interfaces to a collection of IpInterface objects
    /// </summary>
    /// <param name="interfaceTable">SNMP table of router IP interfaces</param>
    /// <param name="ipInterfaces">Resulting collection of IpInterface objects</param>
    /// <exception cref="ArgumentNullException">interfaceTable must not be null</exception>
    protected void ParseInterfaceTable(IDictionary<string, IDictionary<uint, AsnType>> interfaceTable, ref ICollection<IpInterface> ipInterfaces)
    {
        if(interfaceTable == null)
            throw new ArgumentNullException(nameof(interfaceTable));

        ipInterfaces = ipInterfaces ?? new List<IpInterface>();

        foreach (KeyValuePair<string, IDictionary<uint, AsnType>>? p in interfaceTable)
        {
            // Extracting interface index
            int index = default(int);
            string? strIndex = p?.Value[1]?.ToString();
            int.TryParse(strIndex, out index);

            // Extracting interface description
            string? description = p?.Value[2]?.ToString();

            // Extracting interface physical address (MAC)
            string? strMac = new string(p?.Value[6]?.ToString()?.ToCharArray().Where(c => !Char.IsWhiteSpace(c))?.ToArray());
            
            UInt64 mac;
            if(index != default(int) && UInt64.TryParse(strMac, System.Globalization.NumberStyles.HexNumber, null, out mac))
                ipInterfaces.Add(new IpInterface(index, description, mac));
        }
    }

    /// <summary>
    /// Parses SNMP table of router IP addresses to an existing collection of IpInterfae objects updating them
    /// </summary>
    /// <param name="ipAddressesTable">SNMP table or router IP addresses</param>
    /// <param name="ipInterfaces">Existing not empty collection of IpInterface objects</param>
    /// <exception cref="ArgumentNullException">ipAddressesTable and ipInterfaces must not be null</exception>
    /// <exception cref="ArgumentOutOfRangeException">ipInterfaces must not be empty collection</exception>
    protected void ParseIpAddressesTable(IDictionary<string, IDictionary<uint, AsnType>> ipAddressesTable, ref ICollection<IpInterface> ipInterfaces)
    {
        if(ipAddressesTable == null)
            throw new ArgumentNullException(nameof(ipAddressesTable));

        if(ipInterfaces == null)
            throw new ArgumentNullException(nameof(ipInterfaces));

        if(ipInterfaces.Count() == 0)
            throw new ArgumentOutOfRangeException(nameof(ipInterfaces));

        foreach (KeyValuePair<string, IDictionary<uint, AsnType>>? p in ipAddressesTable)
        {
            // Extracting interface index
            int index = default(int);
            string? strIndex = p?.Value[2]?.ToString();
            if(!string.IsNullOrEmpty(strIndex) 
                && int.TryParse(strIndex, out index))
            {
                // Extracting IP address
                string? strIpAddress = p?.Value[1]?.ToString();
                IPAddress? Ip;

                // Extractin Net mask
                string? strNetMask = p?.Value[3]?.ToString();
                IPAddress? Mask;
                if(!string.IsNullOrEmpty(strIpAddress) 
                    && !string.IsNullOrEmpty(strNetMask)
                    && IPAddress.TryParse(strIpAddress, out Ip)
                    && IPAddress.TryParse(strNetMask, out Mask))
                {
                    var ifaces = ipInterfaces.Where(i => i.Index == index);
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

    /// <summary>
    /// Parses SNMP table of router routes for collection of IP addresses of next hops
    /// </summary>
    /// <param name="routingTable">SNMP table or router routes</param>
    /// <param name="nextHops">Resulting collection of IP addresses of next hops</param>
    /// <exception cref="ArgumentNullException">routingTable must not be null</exception>
    protected void ParseRoutingTable(IDictionary<string, IDictionary<uint, AsnType>> routingTable, ref ICollection<IPAddress> nextHops)
    {
        if(routingTable == null)
            throw new ArgumentNullException(nameof(routingTable));

        nextHops = nextHops ?? new List<IPAddress>();

        foreach (KeyValuePair<string, IDictionary<uint, AsnType>>? p in routingTable)
        {
            string? strIpAddress = p?.Value[4]?.ToString();
            IPAddress? Ip;

            if(!string.IsNullOrEmpty(strIpAddress) && IPAddress.TryParse(strIpAddress, out Ip))
            {
    
                if(nextHops.Where(addr => addr.Equals(Ip)).Count() == 0)
                    nextHops.Add(Ip);
            }
        }

    }
}