namespace dsc.Model;

using System.Net;

/// <summary>
/// Router entity.
/// </summary>
class Router : INode
{
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
    /// <param name="Ip">IP address</param>
    /// <param name="Mask">Net mask cardinality</param>
    public void AddIpInt(IPAddress Ip, uint Mask)
    {
        IpInt = IpInt ?? new List<IpInterface>();
        if(IpInt.ToList<IpInterface>().Find(iface => iface.Ip == Ip && iface.Mask == Mask) == null)
            IpInt.Add(new IpInterface(Ip, Mask));
    }

    /// <summary>
    /// Removes IP Interface with specified IP and Mask from the collection of the node  
    /// </summary>
    /// <param name="Ip"></param>
    /// <param name="Mask"></param>
    public void DelIpInt(IPAddress Ip, uint Mask)
    {
        IpInt?.ToList<IpInterface>().RemoveAll(iface => iface.Ip == Ip && iface.Mask == Mask);
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
    /// <param name="Ip">IP address</param>
    public void AddNextHop(IPAddress Ip)
    {
        NextHops = NextHops ?? new List<IPAddress>();
        if(NextHops.ToList<IPAddress>().Find(iface => iface.ToString() == Ip.ToString()) == null)
            NextHops.Add(Ip);
    }

    /// <summary>
    /// Removes next hop with specified IP from the collection of next hops of the node  
    /// </summary>
    /// <param name="Ip"></param>
    /// <param name="Mask"></param>
    public void DelNextHop(IPAddress Ip)
    {
        NextHops?.ToList<IPAddress>().RemoveAll(iface => iface.ToString() == Ip.ToString());
    }

    /// <summary>
    /// Removes all next hops from the collection of next hops of the node
    /// </summary>
    public void ClearNextHops()
    {
        NextHops?.Clear();
    }
}