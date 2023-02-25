using System.Net;

namespace dsc.Model;

public class RouterBuilder
{
    private IPAddress _router_ip;
    private IRouterDataSource _source;

    public RouterBuilder(IPAddress ip, IRouterDataSource source)
    {
        if(source == null)
            throw new ArgumentNullException(nameof(source));

        _router_ip = ip;
        _source = source;
    }

    public async Task<Router> Build()
    {
        var result = new Router();

        // Adding router system name
        result.Name = await _source.GetRouterSystemName(_router_ip);

        // Adding router interfaces
        result.AddIpInterfaces(await _source.GetRouterInterfacesAsync(_router_ip));

        // Adding router next hops
        result.AddNextHops(await _source.GetRouterNextHopsAsync(_router_ip));

        ICollection<ArpRecord> arpRecords = await _source.GetRouterArpRecordsAsync(_router_ip);
        if(arpRecords != null)
        {
            foreach(var arpRecord in arpRecords)
                if(isPCMac(arpRecord.Mac) && isIpLocal(arpRecord.Ip, result.IpInt))
                    result.AddArp(arpRecord);
        }

        return result;
    }

    protected bool isIpLocal(IPAddress ip, ICollection<IpInterface> ipInterfaces)
    {
        if(ip == null)
            throw new ArgumentNullException(nameof(ip));

        if(ipInterfaces == null)
            throw new ArgumentNullException(nameof(ipInterfaces));

        foreach(IpInterface iface in ipInterfaces)
        {
            if(iface == null || iface.Ip == null || iface.Mask == null)
                continue;

            if((IpToInt(ip) & IpToInt(iface.Mask)) == (IpToInt(iface.Ip) & IpToInt(iface.Mask)))
                return true;
        }

        return false;
    }

    protected bool isPCMac(UInt64 mac)
    {
        // Assert that all routers have MAC starting from not 00. All others are not routers.
        return (mac & 0xff0000000000) == 0;
    }

    public static uint IpToInt(IPAddress address)
    {
        byte[] bytes = address.GetAddressBytes();

        // flip big-endian(network order) to little-endian
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);

        return BitConverter.ToUInt32(bytes, 0);
    }

    public static IPAddress IntToIp(uint ipAddress)
    {
        byte[] bytes = BitConverter.GetBytes(ipAddress);

        // flip little-endian to big-endian(network order)
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);

        return new IPAddress(bytes);
    }    
}