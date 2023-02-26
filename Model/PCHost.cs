using System.Collections;
using System.Net;

namespace dsc.Model;

public class PCHost : IPCHost
{
    public IPAddress? Ip { get; set; }
    
    public UInt64 Mac { get; private set; }

    public IPAddress GetIP() => Ip ?? new IPAddress(0);

    public UInt64 GetMac() => Mac;

    public PCHost(UInt64 mac, IPAddress? ip = null)
    {
        Mac = mac;
        Ip = ip;
    }

    public override int GetHashCode()
    {
        var hashCodes = new List<UInt64>();
        hashCodes.Add(Mac);
        
        hashCodes.Sort();

        return ((IStructuralEquatable)hashCodes.ToArray()).GetHashCode(EqualityComparer<UInt64>.Default);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;
        
        if(obj is IPCHost)
            return GetHashCode() == obj.GetHashCode();

        return base.Equals (obj);
    }

    public override string ToString()
    {
        return $"PC Host [{ Ip?.ToString() ?? "Unknown IP" }]";
    }
}