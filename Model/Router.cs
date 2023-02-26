namespace dsc.Model;

using System.Collections;
using System.Net;

/// <summary>
/// Router entity.
/// </summary>
public class Router : IRouter
{

    public Router()
    {
        
    }

    public override int GetHashCode()
    {
        if(IpInt == null || IpInt.Count() == 0)
            return base.GetHashCode();

        var hashCodes = new List<UInt64>();
        foreach(var iface in IpInt)
            hashCodes.Add(iface.Mac);
        
        hashCodes.Sort();

        return ((IStructuralEquatable)hashCodes.ToArray()).GetHashCode(EqualityComparer<UInt64>.Default);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;
        
        if(obj is IRouter)
            return GetHashCode() == obj.GetHashCode();

        return base.Equals (obj);
    }

    /// <summary>
    /// System name of the router
    /// </summary>
    public string? Name { get; set; }

    public string GetDescription() => Name ?? "";

    /// <summary>
    /// A collection or ARP records of the node
    /// </summary>
    public ICollection<ArpRecord>? Arp { get; private set; }

    public ICollection<ArpRecord> GetArpRecords() => Arp ?? new List<ArpRecord>();

    /// <summary>
    /// Adds ARP record to the collection of ARP records of the node
    /// </summary>
    /// <param name="Mac">ARP record to add</param>
    public void AddArp(ArpRecord arpRecord)
    {
        if(arpRecord == null)
            return;

        DelArp(arpRecord);

        Arp = Arp ?? new List<ArpRecord>();
        Arp.Add(arpRecord);
    }

    /// <summary>
    /// Adds several ARP recordsto the collection of ARP records of the node
    /// </summary>
    /// <param name="arpRecords">A number of ARP records to add</param>
    public void AddArps(ICollection<ArpRecord> arpRecords)
    {
        if(arpRecords == null)
            return;

        DelArps(arpRecords);

        Arp = Arp ?? new List<ArpRecord>();
        foreach (var arp in arpRecords)
            Arp.Add(arp);
    }

    /// <summary>
    /// Removes ARP record from the collection of ARP records of the node
    /// </summary>
    /// <param name="arpRecord">ARP record to remove</param>
    public void DelArp(ArpRecord arpRecord)
    {
        if(Arp == null)
            return;

        if(arpRecord == null)
            return;

        foreach (var oldArp in Arp.Where(arp => arp.Ip.Equals(arpRecord.Ip) && arp.Mac == arpRecord.Mac).ToList())
            if (oldArp!= null)
                Arp.Remove(oldArp);
    }

    /// <summary>
    /// Deletes ARP records from collection of ARP records of the node
    /// </summary>
    /// <param name="ipInterfaces">Collectin of ARP records to delete</param>
    public void DelArps(ICollection<ArpRecord> arpRecords)
    {
        if(Arp == null)
            return;

        if(arpRecords == null)
            return;

        foreach (var arp in arpRecords)
        {
            foreach (var oldArpRecord in Arp.Where(rec => rec.Ip.Equals(arp.Ip) && rec.Mac == arp.Mac).ToList())
                if (oldArpRecord != null)
                    Arp.Remove(oldArpRecord);
        }
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

    public ICollection<IpInterface> GetIpInterfaces() => IpInt ?? new List<IpInterface>();

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

    public ICollection<IPAddress> GetNextHops() => NextHops ?? new List<IPAddress>();

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