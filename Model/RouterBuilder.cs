using System.Net;

namespace dsc.Model;

public class RouterBuilder
{
    private IPAddress _router_ip;
    private IRouterDataSource _source;

    public RouterBuilder(IPAddress ip, IRouterDataSource source)
    {
        if(source == null)
            throw new ArgumentNullException(nameof(source));

        _router_ip = ip;
        _source = source;
    }

    public async Task<IRouter> Build()
    {
        var result = new Router();

        // Addint router system name
        result.Name = await _source.GetRouterSystemName(_router_ip);

        // Adding router interfaces
        result.AddIpInterfaces(await _source.GetRouterInterfacesAsync(_router_ip));
        
        return result;
    }
}