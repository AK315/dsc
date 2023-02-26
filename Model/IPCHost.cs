using System.Net;

namespace dsc.Model;

public interface IPCHost : INode
{
    public IPAddress GetIP();
    public UInt64 GetMac();
}