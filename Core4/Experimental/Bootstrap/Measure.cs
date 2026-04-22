namespace Core4.Experimental.Bootstrap;

public sealed class Measure : IElement
{
    private readonly long raw;

    public Measure(long raw)
    {
        this.raw = raw;
    }

    public static long Offset { get; set; }

    public long Raw => raw + Offset;

    public override string ToString() => Raw.ToString();
}
