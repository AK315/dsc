using System.Net;

namespace dsc.Model;

public static class RouterFactory
{
    private static IPAddress _router_ip;

    public static IRouter Create(IPAddress router_ip)
    {
        _router_ip = router_ip;

        var result = new Router();

        return result;
    }
}