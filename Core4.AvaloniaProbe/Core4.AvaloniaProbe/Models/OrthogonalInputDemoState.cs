using Core4.Dynamics;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Models;

/// <summary>
/// A fused directed segment with one top publication and one bottom input attached at the same site.
/// </summary>
public readonly record struct OrthogonalInputDemoState(
    Measure Origin,
    Measure Span,
    Measure DominantValueOffset,
    Measure RecessiveValueOffset,
    Measure TopOutput,
    Measure TopTension,
    Measure BottomInput,
    Measure BottomTension)
{
    private static readonly Carrier AxisCarrier = new(1);
    private static readonly Carrier TopCarrier = new(2);
    private static readonly Carrier BottomCarrier = new(3);

    public static OrthogonalInputDemoState Default => new(
        Measure.Zero,
        new Measure(4),
        new Measure(10),
        new Measure(8),
        new Measure(6),
        new Measure(6),
        new Measure(5),
        new Measure(5));

    public FrozenSegmentDemoState Segment => new(Origin, Span, DominantValueOffset, RecessiveValueOffset);

    public Ratio DominantReading => Segment.DominantReading;

    public Ratio RecessiveReading => Segment.RecessiveReading;

    public string ComplexReading => Segment.ComplexReading;

    public string ApproximateReading => Segment.ApproximateReading;

    public OrthogonalBranchModel TopBranch => new(
        new Flow(TopCarrier, Ratio.Tension(TopOutput), Provenance.SelfDriven(TopOutput.Absolute)),
        Ratio.Tension(TopTension));

    public OrthogonalBranchModel BottomBranch => new(
        new Flow(BottomCarrier, Ratio.Tension(BottomInput), Provenance.OtherDriven(BottomInput.Absolute)),
        Ratio.Tension(BottomTension));

    public Site SourceSite => Site.Open(AxisCarrier, Origin)
        .Anchor(new Ratio(Measure.Zero, Measure.One))
        .Resolve(RecessiveReading.Add(DominantReading).Normalize())
        .WithResidual(AggregateTension)
        .Connect(TopBranch.Signal)
        .Connect(BottomBranch.Signal);

    public SiteSettlement Settlement => SourceSite.Settle();

    public OrthogonalReservoir Orthogonal => SourceSite.Orthogonal;

    public Ratio AggregateTension => Ratio.Tension(new Measure(checked(TopTension.Raw + BottomTension.Raw)));

    public OrthogonalInputDemoState WithOrigin(Measure origin) => this with { Origin = origin };

    public OrthogonalInputDemoState WithSpan(Measure span)
    {
        var normalized = span.Absolute;
        return this with { Span = normalized.Raw == 0 ? Measure.One : normalized };
    }

    public OrthogonalInputDemoState WithDominantValueOffset(Measure offset) => this with { DominantValueOffset = offset };

    public OrthogonalInputDemoState WithRecessiveValueOffset(Measure offset) => this with { RecessiveValueOffset = offset };

    public OrthogonalInputDemoState WithTopOutput(Measure output) => this with { TopOutput = output.Absolute };

    public OrthogonalInputDemoState WithTopTension(Measure tension) => this with { TopTension = tension };

    public OrthogonalInputDemoState WithBottomInput(Measure input) => this with { BottomInput = input.Absolute };

    public OrthogonalInputDemoState WithBottomTension(Measure tension) => this with { BottomTension = tension };
}
