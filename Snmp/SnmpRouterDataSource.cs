using System.Net;
using SnmpSharpNet;
using dsc.Model;

namespace dsc.Snmp;

public class SnmpRouterDataSource : IRouterDataSource
{
    public async Task<ICollection<IpInterface>> GetRouterInterfacesAsync(IPAddress ip)
    {
        ICollection<IpInterface> result = new List<IpInterface>();

        SnmpDeviceService deviceService = new SnmpDeviceService(ip);
        var interfaceTable = await deviceService.GetTableAsync(SnmpOid.InterfaceTableRoot);

        if (interfaceTable != null)
        {
            foreach (KeyValuePair<string, IDictionary<uint, AsnType>>? p in interfaceTable)
            {
                string? description = p?.Value[2]?.ToString();

                string? macString = new string(p?.Value[6]?.ToString().ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray());
                UInt64 mac;
                if(UInt64.TryParse(macString, System.Globalization.NumberStyles.HexNumber, null, out mac))
                    result.Add(new IpInterface(description, mac));
            }
        }

        return result;
    }
}