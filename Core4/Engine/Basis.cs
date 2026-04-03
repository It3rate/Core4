using System.Collections.Immutable;

namespace Core4.Engine;

public sealed class Basis
{
    private readonly ImmutableArray<Basis> _inputs;
    private readonly ImmutableArray<Pin> _pins;

    private Basis(Pin pin)
    {
        _inputs = ImmutableArray<Basis>.Empty;
        _pins = ImmutableArray.Create(pin);
    }

    private Basis(Basis recessive, Basis dominant)
    {
        _inputs = ImmutableArray.Create(recessive, dominant);

        var pins = ImmutableArray.CreateBuilder<Pin>(recessive._pins.Length + dominant._pins.Length);
        pins.AddRange(recessive._pins);
        pins.AddRange(dominant._pins);
        _pins = pins.MoveToImmutable();
    }

    public bool IsAtomic => _inputs.IsEmpty;

    public bool IsComposite => !IsAtomic;

    public bool IsAtomicPair => IsComposite && Recessive.IsAtomic && Dominant.IsAtomic;

    public Measure Grade => IsAtomic ? Measure.Zero : new Measure(Math.Max(Recessive.Grade.Raw, Dominant.Grade.Raw) + 1);

    public Measure PinCount => new(_pins.Length);

    public Measure Arity => PinCount;

    public Measure CarrierCount => new(SelectCarriers().Length);

    public ImmutableArray<Basis> Inputs => _inputs;

    public ImmutableArray<Pin> Pins => _pins;

    public Basis Recessive => IsComposite ? _inputs[0] : this;

    public Basis Dominant => IsComposite ? _inputs[1] : this;

    public Measure Potential => IsAtomic
        ? Absolute(AtomicPin.Delta)
        : IsSameCarrierPair() && IsOpposedPair() && HasSharedOriginPerCarrier()
            ? Recessive.Potential + Dominant.Potential
            : new Measure(checked(Recessive.Potential.Raw * Dominant.Potential.Raw));

    public Ratio Ratio => IsComposite
        ? new Ratio(Recessive.Potential, Dominant.Potential).Normalize()
        : Ratio.Tension(Potential);

    public Measure Orientation => IsAtomic ? AtomicPin.Orientation : Measure.Zero;

    public bool IsRecessive => IsAtomic && Orientation.Raw < 0;

    public bool IsDominant => IsAtomic && Orientation.Raw > 0;

    internal Pin AtomicPin => _pins[0];

    public static Basis Freeze(Pin pin) => new(pin);

    public static Basis Freeze(Carrier carrier, Measure origin, Measure anchor) => new(new Pin(carrier, origin, anchor));

    public static Basis Fuse(Pin recessive, Pin dominant, params Pin[] remaining)
    {
        var bases = new Basis[remaining.Length + 2];
        bases[0] = Freeze(recessive);
        bases[1] = Freeze(dominant);

        for (var index = 0; index < remaining.Length; index++)
        {
            bases[index + 2] = Freeze(remaining[index]);
        }

        var rest = new Basis[Math.Max(0, bases.Length - 2)];

        for (var index = 2; index < bases.Length; index++)
        {
            rest[index - 2] = bases[index];
        }

        return Fuse(bases[0], bases[1], rest);
    }

    public static Basis Fuse(Basis recessive, Basis dominant, params Basis[] remaining)
    {
        var fused = new Basis(recessive, dominant);

        for (var index = 0; index < remaining.Length; index++)
        {
            fused = new Basis(fused, remaining[index]);
        }

        return fused;
    }

    public Basis Flip()
    {
        if (IsAtomic)
        {
            return Freeze(AtomicPin.Flip());
        }

        return Fuse(Recessive.Flip(), Dominant.Flip());
    }

    public Ratio Project(Measure measure) => IsAtomic
        ? AtomicPin.Project(measure).Normalize()
        : Ratio.Tension(measure);

    public Attachment Attach(params Measure[] measures) => Attachment.Attach(this, measures);

    public ImmutableArray<Pin> SelectPins() => _pins;

    public ImmutableArray<Pin> SelectPins(Carrier carrier)
    {
        var pins = ImmutableArray.CreateBuilder<Pin>();

        foreach (var pin in _pins)
        {
            if (pin.Carrier == carrier)
            {
                pins.Add(pin);
            }
        }

        return pins.ToImmutable();
    }

    public ImmutableArray<Carrier> SelectCarriers()
    {
        var carriers = ImmutableArray.CreateBuilder<Carrier>();

        foreach (var pin in _pins)
        {
            if (!carriers.Contains(pin.Carrier))
            {
                carriers.Add(pin.Carrier);
            }
        }

        return carriers.ToImmutable();
    }

    public bool HasSharedOriginPerCarrier()
    {
        foreach (var carrier in SelectCarriers())
        {
            var origin = default(Measure);
            var hasOrigin = false;

            foreach (var pin in SelectPins(carrier))
            {
                if (!hasOrigin)
                {
                    origin = pin.Origin;
                    hasOrigin = true;
                    continue;
                }

                if (pin.Origin != origin)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public bool IsSameCarrierPair()
    {
        if (!IsAtomicPair)
        {
            return false;
        }

        return Recessive.AtomicPin.Carrier == Dominant.AtomicPin.Carrier;
    }

    public bool IsAlignedPair()
    {
        if (!IsAtomicPair)
        {
            return false;
        }

        return Recessive.AtomicPin.Orientation.Raw != 0 &&
               Recessive.AtomicPin.Orientation == Dominant.AtomicPin.Orientation;
    }

    public bool IsOpposedPair()
    {
        if (!IsAtomicPair)
        {
            return false;
        }

        return Recessive.AtomicPin.Orientation.Raw != 0 &&
               Recessive.AtomicPin.Orientation == -Dominant.AtomicPin.Orientation;
    }

    public Measure KernelCode()
    {
        if (!IsAtomicPair)
        {
            return Measure.Zero;
        }

        var recessive = SideCode(Recessive.AtomicPin);
        var dominant = SideCode(Dominant.AtomicPin);

        if (recessive < 0 || dominant < 0)
        {
            return Measure.Zero;
        }

        return new Measure((recessive << 1) | dominant);
    }

    private static Measure Absolute(Measure value) => value.Raw < 0 ? -value : value;

    private static long SideCode(Pin pin)
    {
        if (pin.Orientation.Raw < 0)
        {
            return 0L;
        }

        if (pin.Orientation.Raw > 0)
        {
            return 1L;
        }

        return -1L;
    }
}
