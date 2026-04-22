namespace Core4.Experimental.Bootstrap;

public sealed class Relation : IElement
{
    public Relation(IElement left, IElement right)
    {
        Left = left;
        Right = right;
    }

    public IElement Left { get; }
    public IElement Right { get; }

    public IElement Numerator => new RelationFold(Left, Right).Numerator;

    public IElement Denominator => new RelationFold(Left, Right).Denominator;

    public long PartitionCount => new RelationFold(Left, Right).PartitionCount;

    public IEnumerable<PartitionStep> Partitions() => new RelationFold(Left, Right).Partitions();

    public override string ToString() => $"({Left} -> {Right})";
}
