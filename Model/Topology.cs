using System.Net;
using Microsoft.Extensions.Logging;

namespace dsc.Model;

public class Topology
{
    public ICollection<IRouter> L3Routers { get; private set; }

    public ICollection<IPCHost> Hosts { get; private set; }

    public ICollection<NodesLink<INode, INode>> Links { get; private set; }

    private ILogger? _logger;
    private IRouterDataSource _source;

    public Topology(IRouterDataSource source, ILogger? logger)
    {
        L3Routers = new HashSet<IRouter>();
        Hosts = new HashSet<IPCHost>();
        Links = new HashSet<NodesLink<INode, INode>>();

        _logger = logger;
        _source = source;
    }

    public async Task BuildAsync(IPAddress startAddress)
    {
        await DiscoverRouterAsync(startAddress);
    }

    protected async Task<IRouter?> DiscoverRouterAsync(IPAddress address)
    {
        var routerBuilder = new RouterBuilder(address, _source);
        var router = (Router) await routerBuilder.BuildAsync();

        if(router != null && !NodeExists(router))
        {
            AddNode(router);
            DiscoverRouterPCs(router);
            await DiscoverRouterHopsAsync(router);
        }

        return router;
    }

    protected void DiscoverRouterPCs(IRouter router)
    {
        if(router == null)
            throw new ArgumentNullException(nameof(router));

        ICollection<ArpRecord> arpRecords = router.GetArpRecords();
        foreach(ArpRecord arp in arpRecords)
        {
            PCHost pcHost = new PCHost(arp.Mac, arp.Ip);
            AddNode(pcHost);
            AddLink(router, pcHost);
        }
    }

    protected async Task DiscoverRouterHopsAsync(IRouter router)
    {
        if(router == null)
            throw new ArgumentNullException(nameof(router));

        ICollection<IPAddress> nextHops = router.GetNextHops();
        var routerTasks = new List<Task<IRouter?>>();

        foreach(var address in nextHops)
            routerTasks.Add(DiscoverRouterAsync(address));

        await Task.WhenAll(routerTasks.ToArray());

        foreach(var task in routerTasks)
        {
            var nextRouter = task.Result;
            if (nextRouter != null)
                AddLink(router, nextRouter);
        }
    }

    public void AddNode(INode node)
    {
        switch (node)
        {
            case IRouter router:
                L3Routers.Add(router);
                break;
            case IPCHost host:
                Hosts.Add(host);
                break;
        }
    }

    public bool NodeExists(INode node)
    {
        switch (node)
        {
            case IRouter router:
                return L3Routers.Contains(router);
            case IPCHost host:
                return Hosts.Contains(host);
        }
        
        return false;
    }

    public void AddLink(INode node1, INode node2)
    {
        if(node1 == null)
            throw new ArgumentNullException(nameof(node1));

        if(node2 == null)
            throw new ArgumentNullException(nameof(node2));

        Links.Add(new NodesLink<INode, INode>(node1, node2));
    }
}
