namespace Core4.Kernel.Core;

public sealed class Measure : IElement
{
    private readonly long raw;
    private readonly Carrier carrier;
    private readonly bool usesOffset;

    public Measure(long raw)
        : this(raw, Carrier.Unbound)
    {
    }

    public Measure(long raw, Carrier carrier)
        : this(raw, carrier, usesOffset: true)
    {
    }

    private Measure(long raw, Carrier carrier, bool usesOffset)
    {
        this.raw = raw;
        this.carrier = carrier;
        this.usesOffset = usesOffset;
    }

    public static long Offset { get; set; }

    public static Measure Unbound(long raw) => new(raw, Carrier.Unbound);

    public static Measure On(Carrier carrier, long raw) => new(raw, carrier);

    public long Raw => raw + (usesOffset ? Offset : 0);

    public Carrier Carrier => carrier;

    public long PartitionCount => 0;

    public IEnumerable<PartitionStep> Partitions() => Array.Empty<PartitionStep>();

    internal static Measure Fixed(long raw, Carrier? carrier = null) => new(raw, carrier ?? Carrier.Unbound, usesOffset: false);

    public override string ToString() => carrier == Carrier.Primary || carrier == Carrier.Unbound
        ? Raw.ToString()
        : $"{carrier}:{Raw}";
}
