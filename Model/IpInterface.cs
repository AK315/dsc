namespace dsc.Model;

using System.Net;

/// <summary>
/// Describes one IP Interface
/// </summary>
public record IpInterface
{
    public string? Description { get; private set; }
    public IPAddress? Ip { get; private set; }
    public uint Mask { get; private set; }

    public UInt64 Mac { get; private set; }

    public IpInterface(string? description, UInt64 mac, IPAddress? ip = null, uint mask = 0)
    {
        Description = description;
        Mac = mac;
        Ip = ip;
        Mask = mask;
    }
}

