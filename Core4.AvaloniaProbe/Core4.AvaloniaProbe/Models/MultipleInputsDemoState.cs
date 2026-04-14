using System.Collections.Immutable;
using Core4.Dynamics;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Models;

/// <summary>
/// A fused directed segment with one top publication and two bottom input branches attached to the same site.
/// </summary>
public readonly record struct MultipleInputsDemoState(
    Measure Origin,
    Measure Span,
    Measure DominantValueOffset,
    Measure RecessiveValueOffset,
    Measure TopOutput,
    Measure TopTension,
    Measure BottomInputA,
    Measure BottomTensionA,
    Measure BottomInputB,
    Measure BottomTensionB)
{
    private static readonly Carrier AxisCarrier = new(1);
    private static readonly Carrier TopCarrier = new(2);
    private static readonly Carrier BottomCarrierA = new(3);
    private static readonly Carrier BottomCarrierB = new(4);

    public static MultipleInputsDemoState Default => new(
        Measure.Zero,
        new Measure(4),
        new Measure(10),
        new Measure(8),
        new Measure(6),
        new Measure(8),
        new Measure(4),
        new Measure(4),
        new Measure(6),
        new Measure(6));

    public FrozenSegmentDemoState Segment => new(Origin, Span, DominantValueOffset, RecessiveValueOffset);

    public Ratio DominantReading => Segment.DominantReading;

    public Ratio RecessiveReading => Segment.RecessiveReading;

    public string ComplexReading => Segment.ComplexReading;

    public string ApproximateReading => Segment.ApproximateReading;

    public OrthogonalBranchModel TopBranch => new(
        new Flow(TopCarrier, Ratio.Tension(TopOutput), Provenance.SelfDriven(TopOutput.Absolute)),
        Ratio.Tension(TopTension));

    public OrthogonalBranchModel BottomBranchA => new(
        new Flow(BottomCarrierA, Ratio.Tension(BottomInputA), Provenance.OtherDriven(BottomInputA.Absolute)),
        Ratio.Tension(BottomTensionA));

    public OrthogonalBranchModel BottomBranchB => new(
        new Flow(BottomCarrierB, Ratio.Tension(BottomInputB), Provenance.OtherDriven(BottomInputB.Absolute)),
        Ratio.Tension(BottomTensionB));

    public ImmutableArray<OrthogonalBranchModel> BottomBranches => [BottomBranchA, BottomBranchB];

    public Site SourceSite => Site.Open(AxisCarrier, Origin)
        .Anchor(new Ratio(Measure.Zero, Measure.One))
        .Resolve(RecessiveReading.Add(DominantReading).Normalize())
        .WithResidual(AggregateTension)
        .Connect(TopBranch.Signal)
        .Connect(BottomBranchA.Signal)
        .Connect(BottomBranchB.Signal);

    public SiteSettlement Settlement => SiteDynamics.Settle(SourceSite);

    public OrthogonalReservoir Orthogonal => SourceSite.Orthogonal;

    public Ratio AggregateTension => Ratio.Tension(new Measure(checked(TopTension.Raw + BottomTensionA.Raw + BottomTensionB.Raw)));

    public MultipleInputsDemoState WithOrigin(Measure origin) => this with { Origin = origin };

    public MultipleInputsDemoState WithSpan(Measure span)
    {
        var normalized = span.Absolute;
        return this with { Span = normalized.Raw == 0 ? Measure.One : normalized };
    }

    public MultipleInputsDemoState WithDominantValueOffset(Measure offset) => this with { DominantValueOffset = offset };

    public MultipleInputsDemoState WithRecessiveValueOffset(Measure offset) => this with { RecessiveValueOffset = offset };

    public MultipleInputsDemoState WithTopOutput(Measure output) => this with { TopOutput = output.Absolute };

    public MultipleInputsDemoState WithTopTension(Measure tension) => this with { TopTension = tension };

    public MultipleInputsDemoState WithBottomInputA(Measure input) => this with { BottomInputA = input.Absolute };

    public MultipleInputsDemoState WithBottomTensionA(Measure tension) => this with { BottomTensionA = tension };

    public MultipleInputsDemoState WithBottomInputB(Measure input) => this with { BottomInputB = input.Absolute };

    public MultipleInputsDemoState WithBottomTensionB(Measure tension) => this with { BottomTensionB = tension };
}
