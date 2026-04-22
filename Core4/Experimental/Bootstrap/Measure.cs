namespace Core4.Experimental.Bootstrap;

public sealed class Measure : IElement
{
    private readonly long raw;
    private readonly bool usesOffset;
    private static readonly Measure fixedZero = new(0, usesOffset: false);

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

    public IElement Numerator => this;

    public IElement Denominator => fixedZero;

    public long PartitionCount => RelationFold.PartitionCountOf(Raw, 0);

    public IEnumerable<PartitionStep> Partitions() => RelationFold.PartitionsOf(Raw, 0);

    internal static Measure Fixed(long raw) => new(raw, usesOffset: false);

    public override string ToString() => Raw.ToString();
}
