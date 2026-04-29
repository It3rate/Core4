namespace Gojo.Core;

public sealed class Carrier : IElement
{
    private static ulong idCounter = 1;

    private Carrier? inverse;

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

    public void Add(ulong ticks)
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

    public void Remove(ulong ticks)
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

    public void SetValue(ulong ticks) => Value = ticks;

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

    private static ulong NextId() => idCounter++;
}
