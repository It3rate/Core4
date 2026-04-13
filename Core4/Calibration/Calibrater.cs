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
}
