using System.Collections;

namespace dsc.Model;

public class NodesLink<T1, T2> where T1 : INode where T2 : INode
{
    public T1 Node1 { get; init; }

    public T2 Node2 { get; init; }

    public NodesLink(T1 node1, T2 node2)
    {
        Node1 = node1;
        Node2 = node2;
    }

    public override int GetHashCode()
    {
        var hashCodes = new List<int>();
        hashCodes.Add(Node1.GetHashCode());
        hashCodes.Add(Node2.GetHashCode());
        hashCodes.Sort();

        return ((IStructuralEquatable)hashCodes.ToArray()).GetHashCode(EqualityComparer<int>.Default);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;
        
        if(obj is NodesLink<INode, INode>)
            return GetHashCode() == obj.GetHashCode();

        return base.Equals (obj);
    }
}