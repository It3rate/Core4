namespace Core4.Experimental.Bootstrap;

public sealed class Measure : IElement
{
    private readonly long raw;
    private readonly bool usesOffset;

    public Measure(long raw)
        : this(raw, usesOffset: true)
    {
    }

    private Measure(long raw, bool usesOffset)
    {
        this.raw = raw;
        this.usesOffset = usesOffset;
    }

    public static long Offset { get; set; }

    public long Raw => raw + (usesOffset ? Offset : 0);

    public long PartitionCount => ElementProjection.Of(this).PartitionCount;

    public IEnumerable<PartitionStep> Partitions() => ElementProjection.Of(this).Partitions();

    internal static Measure Fixed(long raw) => new(raw, usesOffset: false);

    public override string ToString() => Raw.ToString();
}
