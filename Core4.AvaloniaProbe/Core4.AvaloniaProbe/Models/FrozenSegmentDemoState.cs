using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Models;

/// <summary>
/// One fused directed segment built from an opposed pair. The span is shared, while both aligned values remain
/// independently movable and can cross the origin because the pair is frozen together.
/// </summary>
public readonly record struct FrozenSegmentDemoState(
    Measure Origin,
    Measure Span,
    Measure DominantValueOffset,
    Measure RecessiveValueOffset)
{
    private static readonly Carrier AxisCarrier = new(1);

    public static FrozenSegmentDemoState Default => new(
        Measure.Zero,
        new Measure(4),
        new Measure(10),
        new Measure(8));

    public Carrier Carrier => AxisCarrier;

    public Site OriginSite => Site.Open(Carrier, Origin);

    public Site DominantUnitSite => Site.Open(Carrier, DominantUnitLocation);

    public Site RecessiveUnitSite => Site.Open(Carrier, RecessiveUnitLocation);

    public Site DominantValueSite => Site.Open(Carrier, DominantValueLocation);

    public Site RecessiveValueSite => Site.Open(Carrier, RecessiveValueLocation);

    public Basis DominantBasis => Basis.Freeze(OriginSite, DominantUnitSite);

    public Basis RecessiveBasis => Basis.Freeze(OriginSite, RecessiveUnitSite);

    public Basis SegmentBasis => Basis.Fuse(RecessiveBasis, DominantBasis);

    public Measure DominantUnitLocation => Origin + Span;

    public Measure RecessiveUnitLocation => Origin - Span;

    public Measure DominantValueLocation => Origin + DominantValueOffset;

    public Measure RecessiveValueLocation => Origin - RecessiveValueOffset;

    public Ratio DominantReading => DominantBasis.Project(DominantValueSite).Normalize();

    public Ratio RecessiveReading => RecessiveBasis.Project(RecessiveValueSite).Normalize();

    public string ComplexReading => $"{RecessiveReading}r + {DominantReading}d";

    public string ApproximateReading =>
        $"({RecessiveReading.Approximate:0.0}i + {DominantReading.Approximate:0.0})";

    public FrozenSegmentDemoState WithOrigin(Measure origin) => this with { Origin = origin };

    public FrozenSegmentDemoState WithSpan(Measure span)
    {
        var normalized = span.Absolute;
        return this with { Span = normalized.Raw == 0 ? Measure.One : normalized };
    }

    public FrozenSegmentDemoState WithDominantValueOffset(Measure offset) => this with { DominantValueOffset = offset };

    public FrozenSegmentDemoState WithRecessiveValueOffset(Measure offset) => this with { RecessiveValueOffset = offset };
}
