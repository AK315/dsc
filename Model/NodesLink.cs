namespace dsc.Model;

public record NodesLink<T1, T2> where T1 : INode where T2 : INode
{
    public T1 Node1 { get; init; }

    public T2 Node2 { get; init; }

    public NodesLink(T1 node1, T2 node2)
    {
        Node1 = node1;
        Node2 = node2;
    }
}