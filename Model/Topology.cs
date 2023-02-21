namespace dsc.Model;

public class Topology
{
    public ICollection<NodesLink<INode, INode>> Links { get; private set; }
}