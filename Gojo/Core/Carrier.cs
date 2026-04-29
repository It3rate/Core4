namespace Gojo.Core;

public readonly record struct Carrier : IElement
{
    public Carrier(ulong id)
        : this(id, id, 1, 0, 0)
    {
    }

    public Carrier(ulong id, ulong lineage, long flow)
        : this(id, lineage, flow, 0, 0)
    {
    }

    public Carrier(ulong id, ulong lineage, long flow, ulong value, ulong tension)
    {
        Id = id;
        Lineage = lineage;
        Flow = Math.Sign(flow);
        Value = value;
        Tension = tension;
    }

    public static Carrier Unknown => new(0, 0, 1);

    public static Carrier Unbound => Unknown;

    public static Carrier Primary => new(1);

    public ulong Id { get; }

    public ulong Lineage { get; }

    public long Flow { get; }

    public ulong Value { get; }

    public ulong Tension { get; }

    public Carrier Inverse() => new(Id, Lineage == 0 ? Id : Lineage, -ReadableFlow);

    public Carrier Duplicate(ulong id) => new(id, Lineage == 0 ? Id : Lineage, ReadableFlow);

    public Carrier WithValue(ulong value) => new(Id, Lineage, ReadableFlow, value, Tension);

    public Carrier WithTension(ulong tension) => new(Id, Lineage, ReadableFlow, Value, tension);

    public Carrier WithState(ulong value, ulong tension) => new(Id, Lineage, ReadableFlow, value, tension);

    public bool HasState => Value != 0 || Tension != 0;

    public bool SameIdentityAs(Carrier other) =>
        Id == other.Id &&
        Lineage == other.Lineage &&
        ReadableFlow == other.ReadableFlow;

    public long ReadSignOf(Carrier locationCarrier)
    {
        if (Id == locationCarrier.Id && Lineage == locationCarrier.Lineage)
        {
            return Math.Sign(ReadableFlow * locationCarrier.ReadableFlow);
        }

        if (Lineage == 0 || locationCarrier.Lineage == 0)
        {
            return SameIdentityAs(locationCarrier) ? 1 : 0;
        }

        if (Lineage != locationCarrier.Lineage)
        {
            return 0;
        }

        return Math.Sign(ReadableFlow * locationCarrier.ReadableFlow);
    }

    public override string ToString() => Flow switch
    {
        < 0 => $"{Lineage}:{Id}-{StateSuffix}",
        > 0 when Lineage != Id => $"{Lineage}:{Id}+{StateSuffix}",
        > 0 => $"{Id}+{StateSuffix}",
        _ => $"{Id}{StateSuffix}"
    };

    private long ReadableFlow => Flow == 0 ? 1 : Flow;

    private string StateSuffix => HasState ? $"[{Value}|{Tension}]" : string.Empty;
}
