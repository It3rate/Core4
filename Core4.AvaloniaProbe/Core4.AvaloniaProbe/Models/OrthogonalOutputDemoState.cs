using Core4.Dynamics;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Models;

/// <summary>
/// A fused directed segment that is also publishing outward on one orthogonal carrier while holding residual
/// orthogonal tension at the same site.
/// </summary>
public readonly record struct OrthogonalOutputDemoState(
    Measure Origin,
    Measure Span,
    Measure DominantValueOffset,
    Measure RecessiveValueOffset,
    Measure TopOutput,
    Measure TopTension)
{
    private static readonly Carrier AxisCarrier = new(1);
    private static readonly Carrier OutputCarrier = new(2);

    public static OrthogonalOutputDemoState Default => new(
        Measure.Zero,
        new Measure(4),
        new Measure(10),
        new Measure(8),
        new Measure(6),
        new Measure(6));

    public FrozenSegmentDemoState Segment => new(Origin, Span, DominantValueOffset, RecessiveValueOffset);

    public Carrier Carrier => AxisCarrier;

    public Carrier OrthogonalCarrier => OutputCarrier;

    public Site OriginSite => Segment.OriginSite;

    public Site DominantValueSite => Segment.DominantValueSite;

    public Site RecessiveValueSite => Segment.RecessiveValueSite;

    public Basis DominantBasis => Segment.DominantBasis;

    public Basis RecessiveBasis => Segment.RecessiveBasis;

    public Basis SegmentBasis => Segment.SegmentBasis;

    public Measure DominantUnitLocation => Segment.DominantUnitLocation;

    public Measure RecessiveUnitLocation => Segment.RecessiveUnitLocation;

    public Measure DominantValueLocation => Segment.DominantValueLocation;

    public Measure RecessiveValueLocation => Segment.RecessiveValueLocation;

    public Ratio DominantReading => Segment.DominantReading;

    public Ratio RecessiveReading => Segment.RecessiveReading;

    public Ratio ComplexProjection => RecessiveReading.Add(DominantReading).Normalize();

    public string ComplexReading => Segment.ComplexReading;

    public string ApproximateReading => Segment.ApproximateReading;

    public Ratio OutputRatio => Ratio.Tension(TopOutput).Normalize();

    public Ratio TensionRatio => Ratio.Tension(TopTension).Normalize();

    public Flow OutputFlow => new(OrthogonalCarrier, Ratio.Tension(TopOutput), Provenance.SelfDriven(TopOutput.Absolute));

    public Site SourceSite => Site.Open(Carrier, Origin)
        .Anchor(new Ratio(Measure.Zero, Measure.One))
        .Resolve(ComplexProjection)
        .WithResidual(TensionRatio)
        .Connect(OutputFlow);

    public SiteSettlement Settlement => SourceSite.Settle();

    public OrthogonalReservoir Orthogonal => SourceSite.Orthogonal;

    public OrthogonalOutputDemoState WithOrigin(Measure origin) => this with { Origin = origin };

    public OrthogonalOutputDemoState WithSpan(Measure span)
    {
        var normalized = span.Absolute;
        return this with { Span = normalized.Raw == 0 ? Measure.One : normalized };
    }

    public OrthogonalOutputDemoState WithDominantValueOffset(Measure offset) => this with { DominantValueOffset = offset };

    public OrthogonalOutputDemoState WithRecessiveValueOffset(Measure offset) => this with { RecessiveValueOffset = offset };

    public OrthogonalOutputDemoState WithTopOutput(Measure output) => this with { TopOutput = output.Absolute };

    public OrthogonalOutputDemoState WithTopTension(Measure tension) => this with { TopTension = tension };
}
