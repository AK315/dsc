using System.Net;

namespace dsc.Model;

public interface IRouterDataSource
{
    public Task<ICollection<IpInterface>> GetRouterInterfacesAsync(IPAddress ip);
}