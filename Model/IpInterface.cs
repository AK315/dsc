namespace dsc.Model;

using System.Net;

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

