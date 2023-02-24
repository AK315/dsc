using System.Net;

namespace dsc.Model;

public interface IRouterDataSource
{
    public Task<string?> GetRouterSystemName(IPAddress ip);
    public Task<ICollection<IpInterface>> GetRouterInterfacesAsync(IPAddress ip);
}