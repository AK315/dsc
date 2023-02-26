using System.Net;

namespace dsc.Model;

public interface IRouter : INode
{
    public int GetHashCode();
    public string GetDescription();
    public ICollection<ArpRecord> GetArpRecords();

    public ICollection<IpInterface> GetIpInterfaces();

    public ICollection<IPAddress> GetNextHops();

}