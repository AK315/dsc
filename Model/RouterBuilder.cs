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
        var interfaces = await _source.GetRouterInterfacesAsync(_router_ip);

        var result = new Router();
        
        // Adding router interfaces
        result.AddIpInterfaces(interfaces);
        
        return result;
    }
}