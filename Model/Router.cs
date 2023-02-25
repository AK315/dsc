namespace dsc.Model;

using System.Net;

/// <summary>
/// Router entity.
/// </summary>
public class Router : IRouter
{

    public Router()
    {
        
    }

    /// <summary>
    /// System name of the router
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// A collection or ARP records of the node
    /// </summary>
    public ICollection<ArpRecord>? Arp { get; private set; }

    /// <summary>
    /// Adds ARP record to the collection of ARP records of the node
    /// </summary>
    /// <param name="Mac">MAC address</param>
    /// <param name="Ip">IP address</param>
    public void AddArp(UInt32 Mac, IPAddress Ip)
    {
        Arp = Arp ?? new List<ArpRecord>();
        if (Arp.ToList<ArpRecord>().Find(arp => arp.Mac == Mac && arp.Ip == Ip) == null)
            Arp.Append<ArpRecord>(new ArpRecord(Mac, Ip));
    }

    /// <summary>
    /// Removes ARP record with the specified MAC and IP from the collection of ARP records of the node
    /// </summary>
    /// <param name="Mac">MAC address</param>
    /// <param name="Ip">IP address</param>
    public void DelArp(UInt32 Mac, IPAddress Ip)
    {
        Arp?.ToList().RemoveAll(arp => arp.Mac == Mac && arp.Ip == Ip);
    }

    /// <summary>
    /// Removes all records from the collection of ARP records of the node.
    /// </summary>
    public void ClearArp()
    {
        Arp?.Clear();
    }

    /// <summary>
    /// Collection of IP Interfaces of the node
    /// </summary>
    public ICollection<IpInterface>? IpInt { get; private set; }

    /// <summary>
    /// Adds new IP Interface to the collection of IP Interfaces of the node
    /// </summary>
    /// <param name="ipInterface">An IP interface object</param>
    public void AddIpInterface(IpInterface ipInterface)
    {
        if(ipInterface == null)
            return;

        DelIpInterface(ipInterface);

        IpInt = IpInt ?? new List<IpInterface>();
        IpInt.Add(ipInterface);
    }

    /// <summary>
    /// Adds several IP Interfaces to the collection of IP Interfaces of the node
    /// </summary>
    /// <param name="ipInterfaces">A number of IP interface objects to add</param>
    public void AddIpInterfaces(ICollection<IpInterface> ipInterfaces)
    {
        if(ipInterfaces == null)
            return;

        DelIpInterfaces(ipInterfaces);

        IpInt = IpInt ?? new List<IpInterface>();
        foreach (var iface in ipInterfaces)
            IpInt.Add(iface);
    }

    /// <summary>
    /// Deletes IP Interface from the collection of IP Interfaces of the node
    /// </summary>
    /// <param name="ipInterface">IP Interface to delete</param>
    public void DelIpInterface(IpInterface ipInterface)
    {
        if(IpInt == null)
            return;

        if(ipInterface == null)
            return;

        foreach (var oldInterface in IpInt.Where(@if => @if.Index == ipInterface.Index).ToList())
            if (oldInterface != null)
                IpInt.Remove(oldInterface);
    }

    /// <summary>
    /// Deletes IP Interfaces from the collection of IP Interfaces of the node
    /// </summary>
    /// <param name="ipInterfaces">Collectin of IP Interfaces to delete</param>
    public void DelIpInterfaces(ICollection<IpInterface> ipInterfaces)
    {
        if(IpInt == null)
            return;

        if(ipInterfaces == null)
            return;

        foreach (var iface in ipInterfaces)
        {
            foreach (var oldInterface in IpInt.Where(@if => @if.Index == iface.Index).ToList())
                if (oldInterface != null)
                    IpInt.Remove(oldInterface);
        }
    }

    /// <summary>
    /// Removes all IP Interfaces from the collection of IP Interfaces of the node
    /// </summary>
    public void ClearIpInt()
    {
        IpInt?.Clear();
    }

    /// <summary>
    /// Collection of IP addresses of next hops
    /// </summary>
    public ICollection<IPAddress>? NextHops { get; private set; }

    /// <summary>
    /// Adds new next hop to the collection of next hops of the node
    /// </summary>
    /// <param name="Ip">IP address of the next hop to add</param>
    public void AddNextHop(IPAddress ip)
    {
        if(ip == null)
            throw new ArgumentNullException(nameof(ip));

        // No need to add zero-address hops 
        if(BitConverter.ToInt32(ip.GetAddressBytes().Reverse().ToArray()) == 0)
            return;

        NextHops = NextHops ?? new List<IPAddress>();
        if(NextHops.Where(addr => addr.Equals(ip)).Count() == 0)
            NextHops.Add(ip);
    }

    /// <summary>
    /// Adds a number of next hops to the collection of next hops of the node
    /// </summary>
    /// <param name="ips">Collection of IP addresses of next hops</param>
    public void AddNextHops(ICollection<IPAddress> ips)
    {
        NextHops = NextHops ?? new List<IPAddress>();

        foreach(var ip in ips)
            AddNextHop(ip);
    }

    /// <summary>
    /// Removes next hop with specified IP from the collection of next hops of the node  
    /// </summary>
    /// <param name="Ip">IP addres of the hop to remove </param>
    public void DelNextHop(IPAddress Ip)
    {
        NextHops?.ToList<IPAddress>().RemoveAll(addr => addr.Equals(Ip));
    }

    /// <summary>
    /// Removes all next hops from the collection of next hops of the node
    /// </summary>
    public void ClearNextHops()
    {
        NextHops?.Clear();
    }
}