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

    public ulong Overflow { get; private set; }

    public ulong Underflow { get; private set; }

    public bool HasState => Value != 0 || Overflow != 0 || Underflow != 0;

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
        Receive(ticks);
        SettleWorld();
    }

    public void Remove(ulong ticks)
    {
        Draw(ticks);
        SettleWorld();
    }

    public void SetValue(ulong ticks)
    {
        Value = ticks;
        SettleWorld();
    }

    internal void Receive(ulong ticks)
    {
        if (ticks == 0)
        {
            return;
        }

        if (Underflow >= ticks)
        {
            Underflow -= ticks;
            return;
        }

        ticks -= Underflow;
        Underflow = 0;
        Value = checked(Value + ticks);
    }

    internal void Draw(ulong ticks)
    {
        if (ticks == 0)
        {
            return;
        }

        if (Value >= ticks)
        {
            Value -= ticks;
            return;
        }

        ticks -= Value;
        Value = 0;
        Underflow = checked(Underflow + ticks);
    }

    internal bool AccountOverflow(ulong value, ulong overflow)
    {
        if (Value == value && overflow == 0)
        {
            return false;
        }

        Value = value;
        AddOverflow(overflow);
        return true;
    }

    internal bool KeepOverflowRemainder(ulong ticks)
    {
        if (Overflow == ticks)
        {
            return false;
        }

        Overflow = ticks;
        return true;
    }

    internal bool KeepUnderflowRemainder(ulong ticks)
    {
        if (Underflow == ticks)
        {
            return false;
        }

        Underflow = ticks;
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

    internal void AddOverflow(ulong ticks)
    {
        if (ticks != 0)
        {
            Overflow = checked(Overflow + ticks);
        }
    }

    internal void SetOverflow(ulong ticks) => Overflow = ticks;

    internal void AddUnderflow(ulong ticks)
    {
        if (ticks != 0)
        {
            Underflow = checked(Underflow + ticks);
        }
    }

    internal void SetUnderflow(ulong ticks) => Underflow = ticks;

    public override string ToString()
    {
        var orientation = IsInverse ? "-" : "+";
        return HasState
            ? $"{Id}{orientation}[{Value}|{Overflow}|{Underflow}]"
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
        .Select(carrier => $"{carrier.Id}:{carrier.Value}:{carrier.Overflow}:{carrier.Underflow}")
        .Order()];

    private static ulong NextId() => idCounter++;
}
