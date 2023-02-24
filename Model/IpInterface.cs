namespace dsc.Model;

using System.Net;

/// <summary>
/// Describes one IP Interface
/// </summary>
public record IpInterface
{
    public int Index { get; private set; }
    public string? Description { get; private set; }
    public IPAddress? Ip { get; set; }
    public IPAddress? Mask { get; set; }
    public UInt64 Mac { get; private set; }

    public IpInterface(int index, string? description, UInt64 mac, IPAddress? ip = null, IPAddress? mask = null)
    {
        Index = index;
        Description = description;
        Mac = mac;
        Ip = ip;
        Mask = mask;
    }
}

