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

    public ulong Id { get; }

    public bool IsInverse { get; }

    public ulong Value { get; private set; }

    public long Tension { get; private set; }

    public bool HasState => Value != 0 || Tension != 0;

    public CarrierLocation ValueSlot => new(this, Value);

    public CarrierLocation TensionSlot => new(this, Tension);

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

    public bool SameIdentityAs(Carrier other) => ReferenceEquals(this, other);

    public long ReadSignOf(Carrier locationCarrier)
    {
        return SameIdentityAs(locationCarrier) ? 1 : 0;
    }

    public Relation Attach(Relation relation)
    {
        foreach (var carrier in relation.Carriers().Distinct())
        {
            if (!carrier.relations.Any(existing => existing.SameIdentityAs(relation)))
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
        // TODO: Replace immediate quiet settlement with a time-step scheduler
        // so several perturbations can enter the same connected world before
        // relations lower tension. Sequential settlement should remain an
        // explicit higher-level operation when ordered changes matter.
        SettleWorld();
    }

    public void Remove(ulong ticks)
    {
        Receive(checked(-checked((long)ticks)));
        // TODO: See Add. Public perturbations settle synchronously for the
        // current executable foothold, but the dynamic model wants batched
        // per-step settlement across independent structures.
        SettleWorld();
    }

    public void Set(long ticks)
    {
        Receive(checked(ticks - checked((long)Value)));
        // TODO: See Add. This is slot-style delta input, not direct value
        // assignment; a future time-step scheduler should batch it with other
        // same-frame perturbations before settlement.
        SettleWorld();
    }

    public void Set(ulong ticks) => Set(checked((long)ticks));

    public bool TrySetFrom(Relation port, Carrier source) =>
        TrySetFrom(port, source.ValueSlot);

    public bool TrySetFrom(Relation port, CarrierLocation input)
    {
        var projected = port.Project(input, this);
        if (!projected.TryGetCarrierLocations(out var target, out var residue) ||
            !target.Carrier.SameIdentityAs(this))
        {
            return false;
        }

        Set(target.Ticks);
        residue.Carrier.HoldResidueTension(residue.Ticks);
        return true;
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

    internal void HoldResidueTension(long ticks)
    {
        AddResidualTension(ticks);
        SettleWorld();
    }

    public override string ToString()
    {
        var orientation = IsInverse ? "-" : "+";
        return HasState
            ? $"{Id}{orientation}[{Value}|{Tension}]"
            : $"{Id}{orientation}";
    }

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
                foreach (var relation in carriers.SelectMany(carrier => carrier.relations)
                    .GroupBy(relation => relation.Id)
                    .Select(group => group.First()))
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
