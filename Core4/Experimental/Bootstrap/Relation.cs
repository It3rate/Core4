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

    public override string ToString() => $"({Left} -> {Right})";
}
