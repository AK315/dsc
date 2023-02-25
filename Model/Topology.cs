namespace dsc.Model;

public class Topology
{
    public ICollection<IRouter> L3Routers { get; private set; }

    public ICollection<IPCHost> Hosts { get; private set; }

    public ICollection<NodesLink<INode, INode>> Links { get; private set; }

    public Topology()
    {
        L3Routers = new HashSet<IRouter>();
        Hosts = new HashSet<IPCHost>();
        Links = new HashSet<NodesLink<INode, INode>>();
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

    public void AddLink(INode node1, INode node2)
    {
        if(node1 == null)
            throw new ArgumentNullException(nameof(node1));

        if(node2 == null)
            throw new ArgumentNullException(nameof(node2));

        Links.Add(new NodesLink<INode, INode>(node1, node2));
    }
}
