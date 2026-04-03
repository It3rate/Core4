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

    private Basis(Basis first, Basis second)
    {
        _inputs = ImmutableArray.Create(first, second);

        var pins = ImmutableArray.CreateBuilder<Pin>(first._pins.Length + second._pins.Length);
        pins.AddRange(first._pins);
        pins.AddRange(second._pins);
        _pins = pins.MoveToImmutable();
    }

    public bool IsAtomic => _inputs.IsEmpty;

    public bool IsComposite => !IsAtomic;

    public Measure Grade => IsAtomic ? Measure.Zero : new Measure(Math.Max(First.Grade.Raw, Second.Grade.Raw) + 1);

    public Measure PinCount => new(_pins.Length);

    public Measure Arity => PinCount;

    public Measure CarrierCount => new(SelectCarriers().Length);

    public ImmutableArray<Basis> Inputs => _inputs;

    public ImmutableArray<Pin> Pins => _pins;

    public Pin Pin => IsAtomic ? _pins[0] : throw new InvalidOperationException("Only atomic bases expose a pinned interval.");

    public Carrier Carrier => Pin.Carrier;

    public Measure Origin => Pin.Origin;

    public Measure Anchor => Pin.Anchor;

    public Measure Span => Pin.Delta;

    public Measure Orientation => Pin.Orientation;

    public Basis First => IsComposite ? _inputs[0] : throw new InvalidOperationException("Only composite bases expose a first child.");

    public Basis Second => IsComposite ? _inputs[1] : throw new InvalidOperationException("Only composite bases expose a second child.");

    public bool IsRecessive => IsAtomic && Orientation.Raw < 0;

    public bool IsDominant => IsAtomic && Orientation.Raw > 0;

    public static Basis Freeze(Pin pin) => new(pin);

    public static Basis Freeze(Carrier carrier, Measure origin, Measure anchor) => new(new Pin(carrier, origin, anchor));

    public static Basis Fuse(params Pin[] pins)
    {
        ArgumentNullException.ThrowIfNull(pins);

        if (pins.Length == 0)
        {
            throw new ArgumentException("At least one pin is required.", nameof(pins));
        }

        if (pins.Length == 1)
        {
            return Freeze(pins[0]);
        }

        var bases = new Basis[pins.Length];

        for (var index = 0; index < pins.Length; index++)
        {
            bases[index] = Freeze(pins[index]);
        }

        return Fuse(bases);
    }

    public static Basis Fuse(params Basis[] inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (inputs.Length == 0)
        {
            throw new ArgumentException("At least one basis is required.", nameof(inputs));
        }

        if (inputs.Length == 1)
        {
            return inputs[0];
        }

        if (inputs.Length == 2)
        {
            return new(inputs[0], inputs[1]);
        }

        var next = new List<Basis>((inputs.Length + 1) / 2);

        for (var index = 0; index < inputs.Length; index += 2)
        {
            next.Add(index + 1 < inputs.Length
                ? new Basis(inputs[index], inputs[index + 1])
                : inputs[index]);
        }

        return Fuse(next.ToArray());
    }

    public Basis Flip()
    {
        if (IsAtomic)
        {
            return Freeze(Pin.Flip());
        }

        var inputs = new Basis[_inputs.Length];

        for (var index = 0; index < _inputs.Length; index++)
        {
            inputs[index] = _inputs[index].Flip();
        }

        return Fuse(inputs);
    }

    public Ratio Attach(Measure measure)
    {
        if (!IsAtomic)
        {
            throw new InvalidOperationException("Only atomic bases attach a single measure directly.");
        }

        return Pin.Attach(measure);
    }

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
        var pair = AsLeafPinPair();
        return pair.first.Carrier == pair.second.Carrier;
    }

    public bool IsAlignedPair()
    {
        var pair = AsLeafPinPair();
        return pair.first.Orientation.Raw != 0 && pair.first.Orientation == pair.second.Orientation;
    }

    public bool IsOpposedPair()
    {
        var pair = AsLeafPinPair();
        return pair.first.Orientation.Raw != 0 && pair.first.Orientation == -pair.second.Orientation;
    }

    public Measure KernelCode()
    {
        var pair = AsLeafPinPair();

        return new Measure((SideCode(pair.first) << 1) | SideCode(pair.second));
    }

    private (Pin first, Pin second) AsLeafPinPair()
    {
        if (!IsComposite || !First.IsAtomic || !Second.IsAtomic)
        {
            throw new InvalidOperationException("This view only applies to a composite pair of atomic bases.");
        }

        return (First.Pin, Second.Pin);
    }

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

        throw new InvalidOperationException("A zero-span basis does not resolve to a recessive or dominant side.");
    }
}
