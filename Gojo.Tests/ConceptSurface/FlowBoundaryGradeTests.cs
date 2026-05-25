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
    public void FlowSurfaceCanSummarizePrimitiveFaceEvidence()
    {
        var selected = Relation.Point(new Carrier(), 1);
        var forward = FlowFaceSurface.Forward(new Carrier(), selected);
        var inverse = FlowFaceSurface.Inverse(new Carrier(), selected);
        var primitiveGroup = Relation.OrderedGroup(new Carrier(), forward, inverse);

        Assert.True(FlowSurface.TryRead(
            primitiveGroup,
            out var exposure,
            out var readSelected,
            out var forwardRead,
            out var inverseRead));
        Assert.Equal(FlowExposure.Both, exposure);
        Assert.Equal(selected, readSelected);
        Assert.Equal(forward, forwardRead);
        Assert.Equal(inverse, inverseRead);

        Assert.True(FlowSurface.TryCreateFromPrimitiveFaces(
            new Carrier(),
            selected,
            [forward, inverse],
            out var summary));
        Assert.True(FlowSurface.TryRead(summary, out var summaryExposure, out _, out _, out _));
        Assert.Equal(FlowExposure.Both, summaryExposure);
    }

    [Fact]
    public void CarrierFlowFaceSurfaceAdaptsMaterializedCarrierFaces()
    {
        var forwardCarrier = new Carrier();
        var inverseCarrier = forwardCarrier.Inverse();

        Assert.True(CarrierFlowFaceSurface.TryCreate(
            new Carrier(),
            forwardCarrier,
            out var forwardSurface));
        Assert.True(CarrierFlowFaceSurface.TryRead(
            forwardSurface,
            out var readForwardCarrier,
            out var forwardFace,
            out var forwardEvidence));
        Assert.Same(forwardCarrier, readForwardCarrier);
        Assert.Equal(FlowFace.Forward, forwardFace);
        Assert.Same(forwardCarrier, forwardEvidence);

        Assert.True(CarrierFlowFaceSurface.TryCreate(
            new Carrier(),
            inverseCarrier,
            out var inverseSurface));
        Assert.True(CarrierFlowFaceSurface.TryRead(
            inverseSurface,
            out var readInverseCarrier,
            out var inverseFace,
            out _));
        Assert.Same(inverseCarrier, readInverseCarrier);
        Assert.Equal(FlowFace.Inverse, inverseFace);
        Assert.False(forwardCarrier.IsInverse);
        Assert.True(inverseCarrier.IsInverse);
    }

    [Fact]
    public void CarrierFlowFaceEvidenceCanFeedSummaryFlowRead()
    {
        var carrier = new Carrier();
        Assert.True(CarrierFlowFaceSurface.TryCreate(new Carrier(), carrier, out var forward));
        Assert.True(CarrierFlowFaceSurface.TryCreate(new Carrier(), carrier.Inverse(), out var inverse));
        var group = Relation.OrderedGroup(new Carrier(), forward, inverse);

        Assert.True(FlowSurface.TryRead(
            group,
            out var exposure,
            out var selected,
            out var forwardRead,
            out var inverseRead));
        Assert.Equal(FlowExposure.Both, exposure);
        Assert.Same(carrier, selected);
        Assert.Equal(forward, forwardRead);
        Assert.Equal(inverse, inverseRead);
    }

    [Fact]
    public void CarrierFlowFaceSurfaceDoesNotInventFaceForUnknownCarrier()
    {
        Assert.False(CarrierFlowFaceSurface.TryReadFace(Carrier.Unknown, out _));
        Assert.False(CarrierFlowFaceSurface.TryCreate(new Carrier(), Carrier.Unknown, out _));
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
    public void BoundarySurfaceCanSummarizePrimitiveSideEvidence()
    {
        var selected = Relation.Point(new Carrier(), 4);
        var flowFace = FlowFaceSurface.Forward(new Carrier(), selected);
        var positive = BoundarySideSurface.Positive(new Carrier(), flowFace);
        var negative = BoundarySideSurface.Negative(new Carrier(), flowFace);
        var primitiveGroup = Relation.OrderedGroup(new Carrier(), positive, negative);

        Assert.True(BoundarySurface.TryRead(
            primitiveGroup,
            out var exposure,
            out var readSelected,
            out var positiveRead,
            out var negativeRead));
        Assert.Equal(BoundaryExposure.FullSigned, exposure);
        Assert.Equal(selected, readSelected);
        Assert.Equal(positive, positiveRead);
        Assert.Equal(negative, negativeRead);

        Assert.True(BoundarySurface.TryCreateFromPrimitiveSides(
            new Carrier(),
            selected,
            [positive, negative],
            out var summary));
        Assert.True(BoundarySurface.TryRead(summary, out var summaryExposure, out _, out _, out _));
        Assert.Equal(BoundaryExposure.FullSigned, summaryExposure);
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

    [Fact]
    public void BoundaryFlowSurfaceCanPairPrimitiveBoundaryAndFlowEvidence()
    {
        var selected = Relation.Point(new Carrier(), 8);
        var forward = FlowFaceSurface.Forward(new Carrier(), selected);
        var inverse = FlowFaceSurface.Inverse(new Carrier(), selected);
        var positive = BoundarySideSurface.Positive(new Carrier(), forward);
        var negative = BoundarySideSurface.Negative(new Carrier(), forward);
        var boundaryEvidence = Relation.OrderedGroup(new Carrier(), positive, negative);
        var flowEvidence = Relation.OrderedGroup(new Carrier(), forward, inverse);
        var surface = BoundaryFlowSurface.Create(new Carrier(), boundaryEvidence, flowEvidence);

        Assert.True(BoundaryFlowSurface.TryRead(
            surface,
            out var boundary,
            out var flow,
            out var boundaryRead,
            out var flowRead,
            out _));
        Assert.Equal(BoundaryExposure.FullSigned, boundary);
        Assert.Equal(FlowExposure.Both, flow);
        Assert.Equal(boundaryEvidence, boundaryRead);
        Assert.Equal(flowEvidence, flowRead);
    }
}
