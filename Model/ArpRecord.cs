namespace dsc.Model;

using System.Net;

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

