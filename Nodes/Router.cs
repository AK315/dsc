using System.Net;

class Router : INode
{
    /// <summary>
    /// Describes one ARP record
    /// </summary>
    public record ArpRecord
    {
        public ArpRecord(UInt32 mac, IPAddress ip)
        {
            Mac = mac;
            Ip = ip;
        }

        public UInt32 Mac { get; private set; }
        public IPAddress? Ip { get; private set; }   
    }

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
        if(Arp.ToList<ArpRecord>().Find(arp => arp.Mac == Mac && arp.Ip == Ip) == null)
            Arp.Append<ArpRecord> (new ArpRecord(Mac, Ip));
    }

    /// <summary>
    /// Removes ARP record with the specified MAC and IP from the collection of ARP records of the node
    /// </summary>
    /// <param name="Mac"></param>
    /// <param name="Ip"></param>
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
    /// Describes one IP Interface
    /// </summary>
    public record IpInterface
    {
        public IPAddress Ip { get; private set; }
        public uint Mask { get; private set; }

        public IpInterface(IPAddress ip, uint mask)
        {
            Ip = ip;
            Mask = mask;
        }
    }

    /// <summary>
    /// Collection of IP Interfaces of the node
    /// </summary>
    public ICollection<IpInterface>? IpInt;

    /// <summary>
    /// Adds new IP Interface to the collection of IP Interfaces of the node
    /// </summary>
    /// <param name="Ip"></param>
    /// <param name="Mask"></param>
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
}