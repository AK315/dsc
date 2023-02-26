using System.Net;
using System.Text;
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

    /// <summary>
    /// Builds topology
    /// </summary>
    /// <param name="startAddress">IP address of the first router node to discover</param>
    /// <returns>Task</returns>
    public async Task BuildAsync(IPAddress startAddress)
    {
        await DiscoverRouterAsync(startAddress);
    }

    /// <summary>
    /// Discovers router with the specified IP address
    /// </summary>
    /// <param name="address">IP address of the router to be discovered</param>
    /// <returns>Task</returns>
    protected async Task<IRouter?> DiscoverRouterAsync(IPAddress address)
    {
        _logger?.LogInformation($"Discovering router with IP { address.ToString() }");

        Router? router = null;

        var routerBuilder = new RouterBuilder(address, _source);

        try
        {
            router = (Router)await routerBuilder.BuildAsync();
            if(router != null && !NodeExists(router))
            {
                _logger?.LogDebug($"Router with IP { address.ToString() } descovered successfully.");

                AddNode(router);

                _logger?.LogDebug($"Doscovering PC Hosts connected to router with IP { address.ToString() }");
                DiscoverRouterPCs(router);

                _logger?.LogDebug($"Doscovering other routers connected to router with IP { address.ToString() }");
                await DiscoverRouterHopsAsync(router);
            }
        }
        catch(AggregateException ex)
        {
            if(ex.InnerExceptions != null)
                foreach(var e in ex.InnerExceptions)
                {
                    _logger?.LogError($"An error occurred while discovering router with IP { address.ToString() } : { e.Message }");
                }
        }
        catch(Exception e)
        {
            _logger?.LogError($"An error occurred while discovering router with IP { address.ToString() } : { e.Message }");
        }

        return router;
    }

    /// <summary>
    /// Discovers PCs connected to the router
    /// </summary>
    /// <param name="router">Router that PCs are connected to</param>
    /// <exception cref="ArgumentNullException">router must not be null</exception>
    protected void DiscoverRouterPCs(IRouter router)
    {
        if(router == null)
            throw new ArgumentNullException(nameof(router));

        ICollection<ArpRecord> arpRecords = router.GetArpRecords();
        foreach(ArpRecord arp in arpRecords)
        {
            PCHost pcHost = new PCHost( arp.Mac, arp.Ip );
            _logger?.LogDebug($"Discovered PC Host with IP { arp?.Ip?.ToString() ?? "unknown" }");

            AddNode(pcHost);
            AddLink(router, pcHost);
        }
    }

    /// <summary>
    /// Discovers other routers connected to the router as next hops
    /// </summary>
    /// <param name="router">Router which next hops to be discovered</param>
    /// <returns>Task</returns>
    /// <exception cref="ArgumentNullException">router must not be null</exception>
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

    /// <summary>
    /// Adds node to topology
    /// </summary>
    /// <param name="node">Node to be added to topology</param>
    public void AddNode(INode node)
    {
        switch (node)
        {
            case IRouter router:
                L3Routers.Add(router);
                _logger?.LogDebug($"{ router.ToString() } added to topology.");
                break;
            case IPCHost host:
                Hosts.Add(host);
                _logger?.LogDebug($"{ host.ToString() } added to topology.");
                break;
        }
    }

    /// <summary>
    /// Checks if node exists in disvocered topology
    /// </summary>
    /// <param name="node">Node to be checked</param>
    /// <returns>True if node exists in topology</returns>
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

    /// <summary>
    /// Adds link between two nodes to topology
    /// </summary>
    /// <param name="node1">First node</param>
    /// <param name="node2">Second node</param>
    /// <exception cref="ArgumentNullException">node1 and node2 must not be null</exception>
    public void AddLink(INode node1, INode node2)
    {
        if(node1 == null)
            throw new ArgumentNullException(nameof(node1));

        if(node2 == null)
            throw new ArgumentNullException(nameof(node2));

        Links.Add(new NodesLink<INode, INode>(node1, node2));
        
        _logger?.LogDebug($"Link between { node1.ToString() } and { node2.ToString() } added to topology");
    }

    /// <summary>
    /// Returns topology string description
    /// </summary>
    /// <returns>Topology string description</returns>
    public override string ToString()
    {
        string result = "Topology not discovered";

        if(L3Routers != null && L3Routers.Count() > 0)
        {
            var sb = new StringBuilder();
            sb.Append("Routers: ");
            foreach(IRouter router in L3Routers)
                sb.Append($"{ router?.GetDescription() ?? "Unknown router"}  ");

            result = sb.ToString() + "\r\n\r\n";
        }

        if(Hosts != null && Hosts.Count() > 0)
        {
            var sb = new StringBuilder();
            sb.Append("PC Hosts: ");
            foreach(IPCHost host in Hosts)
                sb.Append($"{ host.GetIP().ToString() ?? "Unknown host"}  ");

            result += sb.ToString() + "\r\n\r\n";
        }

        if (Links != null && Links.Count() > 0)
        {
            var sb = new StringBuilder();
            foreach (var link in Links)
            {
                var link2 = link;
                sb.Append(link?.Node1?.ToString() ?? "This host");
                sb.Append(" connected to ");
                sb.Append(link?.Node2?.ToString() ?? "this host");
                sb.Append("\r\n");
            }

            result += sb.ToString();
        }

        return result;
    }

}
