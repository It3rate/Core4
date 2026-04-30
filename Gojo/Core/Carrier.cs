namespace Gojo.Core;

public sealed class Carrier : IElement
{
    private static ulong idCounter = 1;

    private Carrier? inverse;
    private readonly List<Relation> relations = [];
    private bool settling;

    public Carrier()
        : this(isInverse: false)
    {
    }

    public Carrier(ulong _)
        : this(isInverse: false)
    {
    }

    public Carrier(bool isInverse)
        : this(NextId(), isInverse)
    {
    }

    private Carrier(ulong id, bool isInverse)
    {
        Id = id;
        IsInverse = isInverse;
    }

    public static Carrier Unknown { get; } = new(0, isInverse: false);

    public static Carrier Unbound => Unknown;

    public static Carrier Primary => new();

    public ulong Id { get; }

    public bool IsInverse { get; }

    public ulong Value { get; private set; }

    public long Tension { get; private set; }

    public bool HasState => Value != 0 || Tension != 0;

    public Carrier Dual() => Inverse();

    public Carrier Inverse()
    {
        if (inverse is not null)
        {
            return inverse;
        }

        inverse = new Carrier(!IsInverse)
        {
            inverse = this
        };
        return inverse;
    }

    IElement IElement.Dual() => Dual();

    public Carrier Duplicate(ulong _) => new(IsInverse);

    public bool SameIdentityAs(Carrier other) => ReferenceEquals(this, other);

    public long ReadSignOf(Carrier locationCarrier)
    {
        if (SameIdentityAs(locationCarrier))
        {
            return 1;
        }

        return IsInverseOf(locationCarrier) ? -1 : 0;
    }

    public Relation Attach(Relation relation)
    {
        foreach (var carrier in relation.Carriers().Distinct())
        {
            if (!carrier.relations.Contains(relation))
            {
                carrier.relations.Add(relation);
            }
        }

        SettleWorld();
        return relation;
    }

    public void Add(ulong ticks)
    {
        Receive(checked((long)ticks));
        SettleWorld();
    }

    public void Remove(ulong ticks)
    {
        Receive(checked(-checked((long)ticks)));
        SettleWorld();
    }

    public void SetValue(ulong ticks)
    {
        Value = ticks;
        Tension = 0;
        SettleWorld();
    }

    internal void Receive(long ticks)
    {
        if (ticks == 0)
        {
            return;
        }

        Tension = checked(Tension + ticks);
        AbsorbTension();
    }

    internal bool LeaveTension(ulong value, long tension)
    {
        if (Value == value && tension == 0)
        {
            return false;
        }

        Value = value;
        AddResidualTension(tension);
        return true;
    }

    internal bool KeepTensionRemainder(long ticks)
    {
        if (Tension == ticks)
        {
            return false;
        }

        Tension = ticks;
        return true;
    }

    internal bool BalanceOpposedValueWith(Carrier other)
    {
        var shared = Math.Min(Value, other.Value);
        if (shared == 0)
        {
            return false;
        }

        Value -= shared;
        other.Value -= shared;
        return true;
    }

    internal void AddResidualTension(long ticks)
    {
        if (ticks != 0)
        {
            Tension = checked(Tension + ticks);
        }
    }

    public override string ToString()
    {
        var orientation = IsInverse ? "-" : "+";
        return HasState
            ? $"{Id}{orientation}[{Value}|{Tension}]"
            : $"{Id}{orientation}";
    }

    private bool IsInverseOf(Carrier other) =>
        ReferenceEquals(inverse, other) ||
        ReferenceEquals(other.inverse, this);

    private void SettleWorld()
    {
        if (settling)
        {
            return;
        }

        settling = true;
        try
        {
            var carriers = ConnectedCarriers();
            for (var pass = 0; pass < 256; pass++)
            {
                var before = Snapshot(carriers);
                foreach (var relation in carriers.SelectMany(carrier => carrier.relations).Distinct())
                {
                    relation.Apply();
                }

                if (Snapshot(carriers).SequenceEqual(before))
                {
                    return;
                }

                carriers = ConnectedCarriers();
            }
        }
        finally
        {
            settling = false;
        }
    }

    private Carrier[] ConnectedCarriers()
    {
        var found = new HashSet<Carrier>();
        var pending = new Queue<Carrier>();
        pending.Enqueue(this);

        while (pending.Count != 0)
        {
            var carrier = pending.Dequeue();
            if (!found.Add(carrier))
            {
                continue;
            }

            foreach (var relation in carrier.relations)
            {
                foreach (var related in relation.Carriers())
                {
                    pending.Enqueue(related);
                }
            }
        }

        return [.. found.OrderBy(carrier => carrier.Id)];
    }

    private static string[] Snapshot(Carrier[] carriers) => [.. carriers
        .Select(carrier => $"{carrier.Id}:{carrier.Value}:{carrier.Tension}")
        .Order()];

    private static ulong NextId() => idCounter++;

    private void AbsorbTension()
    {
        if (Tension > 0)
        {
            Value = checked(Value + checked((ulong)Tension));
            Tension = 0;
            return;
        }

        if (Tension == 0)
        {
            return;
        }

        var draw = checked((ulong)-Tension);
        if (Value >= draw)
        {
            Value -= draw;
            Tension = 0;
            return;
        }

        Tension = -checked((long)(draw - Value));
        Value = 0;
    }
}
