using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Models;

/// <summary>
/// Two opposed one-dimensional bases sharing one origin. The page stores only movable measures and rebuilds the
/// dominant and recessive bases plus their live value sites each frame.
/// </summary>
public readonly record struct InvertedPairDemoState(
    Measure Origin,
    Measure DominantSpan,
    Measure RecessiveSpan,
    Measure DominantValueOffset,
    Measure RecessiveValueOffset)
{
    private static readonly Carrier AxisCarrier = new(1);

    public static InvertedPairDemoState Default => new(
        Measure.Zero,
        new Measure(4),
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

    public Measure DominantUnitLocation => Origin + DominantSpan;

    public Measure RecessiveUnitLocation => Origin - RecessiveSpan;

    public Measure DominantValueLocation => Origin + DominantValueOffset;

    public Measure RecessiveValueLocation => Origin - RecessiveValueOffset;

    public bool IsDominantHidden => DominantValueOffset.Raw < 0;

    public bool IsRecessiveHidden => RecessiveValueOffset.Raw < 0;

    public Ratio DominantReading => DominantBasis.Project(DominantValueSite).Normalize();

    public Ratio RecessiveReading => RecessiveBasis.Project(RecessiveValueSite).Normalize();

    public Ratio DisplayDominantReading => IsDominantHidden
        ? Ratio.Tension(DominantValueOffset.Absolute)
        : DominantReading;

    public Ratio DisplayRecessiveReading => IsRecessiveHidden
        ? Ratio.Tension(RecessiveValueOffset.Absolute)
        : RecessiveReading;

    public string ComplexReading => $"{DisplayRecessiveReading}r + {DisplayDominantReading}d";

    public string ApproximateReading =>
        $"({RecessiveReading.Approximate:0.0}i + {DominantReading.Approximate:0.0})";

    public InvertedPairDemoState WithOrigin(Measure origin) => this with { Origin = origin };

    public InvertedPairDemoState WithDominantSpan(Measure span)
    {
        var normalized = span.Absolute;
        return this with { DominantSpan = normalized.Raw == 0 ? Measure.One : normalized };
    }

    public InvertedPairDemoState WithRecessiveSpan(Measure span)
    {
        var normalized = span.Absolute;
        return this with { RecessiveSpan = normalized.Raw == 0 ? Measure.One : normalized };
    }

    public InvertedPairDemoState WithDominantValueOffset(Measure offset) => this with { DominantValueOffset = offset };

    public InvertedPairDemoState WithRecessiveValueOffset(Measure offset) => this with { RecessiveValueOffset = offset };
}
