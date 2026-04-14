using System.Collections.Immutable;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.Calibration;

/// <summary>
/// Minimal rolling three-measure calibrater. The first sample becomes origin, the second sample becomes the unit
/// marker, and the third sample is projected as the current value through that live relation. Later samples slide
/// the window left so the structure stays origin-unit-value.
/// Example: <c>var state = Calibrater.Start(1).Input(62).Input(79).Input(101);</c>
/// </summary>
public sealed class Calibrater
{
    private readonly ImmutableArray<Measure> _samples;

    private Calibrater(Carrier carrier, ImmutableArray<Measure> samples)
    {
        Carrier = carrier;
        _samples = samples;
    }

    public Carrier Carrier { get; }

    public ImmutableArray<Measure> Samples => _samples;

    public Measure SampleCount => new(_samples.Length);

    public bool HasOrigin => _samples.Length >= 1;

    public bool HasUnit => _samples.Length >= 2;

    public bool HasValue => _samples.Length >= 3;

    public Measure OriginMeasure => HasOrigin ? _samples[0] : Measure.Zero;

    public Measure UnitMeasure => HasUnit ? _samples[1] : Measure.Zero;

    public Measure ValueMeasure => HasValue ? _samples[2] : Measure.Zero;

    public Site OriginSite => Site.Open(Carrier, OriginMeasure);

    public Site UnitSite => Site.Open(Carrier, UnitMeasure);

    public Site ValueSite => Site.Open(Carrier, ValueMeasure);

    public Measure TickSize => HasUnit ? (UnitMeasure - OriginMeasure).Absolute : Measure.Zero;

    public Ratio ValueProjection => TryGetUnitRelation(out var relation) && HasValue
        ? relation.Project(ValueSite).Normalize()
        : Ratio.Zero(Measure.Zero);

    public static Calibrater Start(Carrier carrier) => new(carrier, ImmutableArray<Measure>.Empty);

    public static Calibrater Start(long carrier) => Start(new Carrier(carrier));

    public Calibrater Input(Measure value)
    {
        if (_samples.Length < 3)
        {
            return new Calibrater(Carrier, _samples.Add(value));
        }

        return new Calibrater(
            Carrier,
            ImmutableArray.Create(
                _samples[1],
                _samples[2],
                value));
    }

    public Calibrater InputShifted(Measure value, Ratio follow, Ratio pressure)
    {
        if (_samples.Length < 3)
        {
            return Input(value);
        }

        var nextOrigin = CanApply(follow)
            ? Nudge(_samples[0], _samples[1], follow)
            : _samples[0];
        var nextUnit = CanApply(follow)
            ? Nudge(_samples[1], _samples[2], follow)
            : _samples[1];
        nextUnit = ApplyPressure(nextOrigin, nextUnit, value, pressure);

        return new Calibrater(
            Carrier,
            ImmutableArray.Create(
                nextOrigin,
                nextUnit,
                value));
    }

    public Calibrater WithValue(Measure value)
    {
        if (!HasUnit)
        {
            return Input(value);
        }

        return new Calibrater(
            Carrier,
            ImmutableArray.Create(
                OriginMeasure,
                UnitMeasure,
                value));
    }

    public bool TryGetUnitRelation(out Relation relation)
    {
        if (HasUnit)
        {
            relation = OriginSite.Relate(UnitSite);
            return true;
        }

        relation = default;
        return false;
    }

    private static Measure Nudge(Measure from, Measure toward, Ratio shift)
    {
        var delta = toward.Raw - from.Raw;
        var moved = delta * shift.Recessive.Raw / shift.Dominant.Raw;
        return new Measure(from.Raw + moved);
    }

    private static Measure ApplyPressure(Measure origin, Measure unit, Measure value, Ratio pressure)
    {
        if (!CanApply(pressure))
        {
            return unit;
        }

        var span = unit.Raw - origin.Raw;
        var magnitude = Math.Abs(span);
        if (magnitude == 0)
        {
            return unit;
        }

        var projectedMagnitude = Math.Abs(value.Raw - origin.Raw) / (double)magnitude;
        if (projectedMagnitude == 0d)
        {
            return ResizeUnit(origin, span, pressure, expand: false);
        }

        if (projectedMagnitude > (magnitude * 8d))
        {
            return ResizeUnit(origin, span, pressure, expand: true);
        }

        if (magnitude > (projectedMagnitude * 8d))
        {
            return ResizeUnit(origin, span, pressure, expand: false);
        }

        return unit;
    }

    private static Measure ResizeUnit(Measure origin, long signedSpan, Ratio pressure, bool expand)
    {
        var sign = Math.Sign(signedSpan);
        if (sign == 0)
        {
            sign = 1;
        }

        var magnitude = Math.Abs(signedSpan);
        var delta = Math.Max(1, magnitude * pressure.Recessive.Raw / pressure.Dominant.Raw);
        var nextMagnitude = expand
            ? magnitude + delta
            : Math.Max(1, magnitude - delta);

        return new Measure(origin.Raw + (sign * nextMagnitude));
    }

    private static bool CanApply(Ratio ratio) => !ratio.IsTension && ratio.Dominant != Measure.Zero;
}
