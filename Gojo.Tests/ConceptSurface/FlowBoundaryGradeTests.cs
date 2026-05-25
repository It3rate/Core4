using Gojo.ConceptSurface;
using Gojo.Core;
using Gojo.Structures;

namespace Gojo.Tests.ConceptSurface;

public sealed class FlowBoundaryGradeTests
{
    [Fact]
    public void FlowFaceSurfaceReadsPrimitiveFaces()
    {
        var selected = Relation.Point(new Carrier(), 9);
        var evidence = Relation.Segment(new Carrier(), 0, 3);
        var forward = FlowFaceSurface.Forward(new Carrier(), selected, evidence);
        var inverse = FlowFaceSurface.Inverse(new Carrier(), selected, evidence);

        Assert.True(FlowFaceSurface.TryRead(forward, out var forwardFace, out var forwardSelected, out var forwardEvidence));
        Assert.Equal(FlowFace.Forward, forwardFace);
        Assert.Equal(selected, forwardSelected);
        Assert.Equal(evidence, forwardEvidence);

        Assert.True(FlowFaceSurface.TryRead(inverse, out var inverseFace, out var inverseSelected, out var inverseEvidence));
        Assert.Equal(FlowFace.Inverse, inverseFace);
        Assert.Equal(selected, inverseSelected);
        Assert.Equal(evidence, inverseEvidence);
        Assert.Equal(FlowFace.Forward, inverseFace.Dual());
    }

    [Fact]
    public void FlowExposureReadsAbsentAndComposedFaces()
    {
        var selected = Relation.Point(new Carrier(), 1);
        var empty = Relation.OrderedGroup(new Carrier());
        var forward = FlowFaceSurface.Forward(new Carrier(), selected);
        var inverse = FlowFaceSurface.Inverse(new Carrier(), selected);
        var both = Relation.OrderedGroup(new Carrier(), forward, inverse);

        Assert.Equal(FlowExposure.None, FlowFaceSurface.ReadExposure(empty));
        Assert.Equal(FlowExposure.Forward, FlowFaceSurface.ReadExposure(forward));
        Assert.Equal(FlowExposure.Inverse, FlowFaceSurface.ReadExposure(inverse));
        Assert.Equal(FlowExposure.Both, FlowFaceSurface.ReadExposure(both));
    }

    [Fact]
    public void BoundarySideSurfaceRequiresPrimitiveFlowFace()
    {
        var selected = Relation.Point(new Carrier(), 2);
        var flowFace = FlowFaceSurface.Forward(new Carrier(), selected);
        var cutEvidence = Relation.Point(new Carrier(), 0);
        var side = BoundarySideSurface.Positive(new Carrier(), flowFace, cutEvidence);

        Assert.True(BoundarySideSurface.TryRead(side, out var readSide, out var readFlowFace, out var readCutEvidence));
        Assert.Equal(BoundarySide.Positive, readSide);
        Assert.Equal(flowFace, readFlowFace);
        Assert.Equal(cutEvidence, readCutEvidence);

        var nonFlowRelation = Relation.Point(new Carrier(), 0);
        var invalidSide = BoundarySideSurface.Positive(new Carrier(), nonFlowRelation);

        Assert.False(BoundarySideSurface.TryRead(invalidSide, out _, out _, out _));
    }

    [Fact]
    public void BoundaryExposureReadsComposedSidesOverFlow()
    {
        var selected = Relation.Point(new Carrier(), 4);
        var flowFace = FlowFaceSurface.Forward(new Carrier(), selected);
        var positive = BoundarySideSurface.Positive(new Carrier(), flowFace);
        var negative = BoundarySideSurface.Negative(new Carrier(), flowFace);
        var both = Relation.OrderedGroup(new Carrier(), positive, negative);

        Assert.True(BoundarySideSurface.TryReadExposure(positive, out var positiveExposure));
        Assert.Equal(BoundaryExposure.Positive, positiveExposure);

        Assert.True(BoundarySideSurface.TryReadExposure(negative, out var negativeExposure));
        Assert.Equal(BoundaryExposure.Negative, negativeExposure);

        Assert.True(BoundarySideSurface.TryReadExposure(both, out var fullExposure));
        Assert.Equal(BoundaryExposure.FullSigned, fullExposure);

        var empty = Relation.OrderedGroup(new Carrier());
        Assert.False(BoundarySideSurface.TryReadExposure(empty, out _));
    }

    [Fact]
    public void FlowBoundaryGradeLaneSurfaceMapsBornSidesToOccupiedLanes()
    {
        var selected = Relation.Point(new Carrier(), 5);
        var positiveForward = FlowBoundaryGradeLaneSurface.Create(
            new Carrier(),
            FlowFace.Forward,
            BoundarySide.Positive,
            selected);
        var negativeInverse = FlowBoundaryGradeLaneSurface.Create(
            new Carrier(),
            FlowFace.Inverse,
            BoundarySide.Negative,
            selected);

        Assert.True(FlowBoundaryGradeLaneSurface.TryRead(
            positiveForward,
            out var firstLane,
            out var firstFlowFace,
            out var firstBoundarySide,
            out var firstSelected));
        Assert.Equal(BoundaryFlowLane.PositiveForward, firstLane);
        Assert.True(FlowFaceSurface.TryRead(firstFlowFace, out var firstFlow, out _, out _));
        Assert.Equal(FlowFace.Forward, firstFlow);
        Assert.True(BoundarySideSurface.TryRead(firstBoundarySide, out var firstSide, out _, out _));
        Assert.Equal(BoundarySide.Positive, firstSide);
        Assert.Equal(selected, firstSelected);

        Assert.True(FlowBoundaryGradeLaneSurface.TryRead(
            negativeInverse,
            out var secondLane,
            out _,
            out _,
            out _));
        Assert.Equal(BoundaryFlowLane.NegativeInverse, secondLane);
    }
}
