namespace Gojo.Core;

public readonly record struct Carrier
{
    public Carrier(ulong id)
        : this(id, id, 1)
    {
    }

    public Carrier(ulong id, ulong lineage, long flow)
    {
        Id = id;
        Lineage = lineage;
        Flow = Math.Sign(flow);
    }

    public static Carrier Unbound => new(0, 0, 0);

    public static Carrier Primary => new(1);

    public ulong Id { get; }

    public ulong Lineage { get; }

    public long Flow { get; }

    public Carrier Inverse() => new(Id, Lineage == 0 ? Id : Lineage, -ReadableFlow);

    public Carrier Duplicate(ulong id) => new(id, Lineage == 0 ? Id : Lineage, ReadableFlow);

    public long ReadSignOf(Carrier locationCarrier)
    {
        if (Lineage == 0 || locationCarrier.Lineage == 0)
        {
            return this == locationCarrier ? 1 : 0;
        }

        if (Lineage != locationCarrier.Lineage)
        {
            return 0;
        }

        return Math.Sign(ReadableFlow * locationCarrier.ReadableFlow);
    }

    public override string ToString() => Flow switch
    {
        < 0 => $"{Lineage}:{Id}-",
        > 0 when Lineage != Id => $"{Lineage}:{Id}+",
        > 0 => $"{Id}+",
        _ => $"{Id}"
    };

    private long ReadableFlow => Flow == 0 ? 1 : Flow;
}
