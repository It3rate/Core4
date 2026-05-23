using Gojo.ConceptSurface;
using Gojo.Core;
using Gojo.Structures;

namespace Gojo.Tests.ConceptSurface;

public sealed class BoundaryFlowLaneBasisTests
{
    [Theory]
    [MemberData(nameof(SelectedLaneBasisStates))]
    public void BoundaryFlowExposureDefinesLegalLaneBasis(
        BoundaryExposure boundary,
        FlowExposure flow,
        string expectedLabels)
    {
        var lanes = BoundaryFlowLaneBasis.LegalLanes(boundary, flow);

        Assert.Equal(expectedLabels, string.Join("|", lanes.Select(lane => lane.Label)));
    }

    [Fact]
    public void SingleLaneSurfacesCanInferTheOnlyOccupiedLane()
    {
        Assert.True(BoundaryFlowLaneBasis.TryInferSingleLane(
            BoundaryExposure.Positive,
            FlowExposure.Forward,
            out var positiveForward));
        Assert.Equal(BoundaryFlowLane.PositiveForward, positiveForward);

        Assert.True(BoundaryFlowLaneBasis.TryInferSingleLane(
            BoundaryExposure.Negative,
            FlowExposure.Inverse,
            out var negativeInverse));
        Assert.Equal(BoundaryFlowLane.NegativeInverse, negativeInverse);
    }

    [Fact]
    public void MultiLaneSurfaceDoesNotPretendAValueLaneWasChosen()
    {
        Assert.False(BoundaryFlowLaneBasis.TryInferSingleLane(
            BoundaryExposure.FullSigned,
            FlowExposure.Forward,
            out _));
        Assert.False(BoundaryFlowLaneBasis.TryInferSingleLane(
            BoundaryExposure.Negative,
            FlowExposure.Both,
            out _));
    }

    [Fact]
    public void LaneBasisSurfaceKeepsExplicitLegalLaneMarkers()
    {
        var selected = Relation.Segment(new Carrier(), -4, 4);
        var basis = BoundaryFlowLaneBasis.Create(
            new Carrier(),
            BoundaryExposure.FullSigned,
            FlowExposure.Both,
            selected);

        Assert.True(BoundaryFlowLaneBasis.TryRead(
            basis,
            out var exposure,
            out var readSelected,
            out var lanes));
        Assert.True(exposure.Boundary.IsFullSigned);
        Assert.True(exposure.Flow.IsBoth);
        Assert.Equal(selected, readSelected);
        Assert.Equal("PF|PI|NF|NI", string.Join("|", lanes.Select(lane => lane.Label)));
    }

    [Fact]
    public void CarrierAxisSurfaceCanExposeLegalLanesWithoutDuplicatingValues()
    {
        var carrier = new Carrier();
        var axis = CarrierAxisSurface.Create(
            new Carrier(),
            carrier,
            BoundaryExposure.Negative,
            FlowExposure.Both);

        Assert.True(CarrierAxisSurface.TryReadLegalLanes(axis, out var lanes));
        Assert.Equal("NF|NI", string.Join("|", lanes.Select(lane => lane.Label)));
    }

    [Fact]
    public void ValueSurfaceKeepsMagnitudeSeparateFromOccupiedLane()
    {
        var carrier = new Carrier();
        var axis = CarrierAxisSurface.Create(
            new Carrier(),
            carrier,
            BoundaryExposure.FullSigned,
            FlowExposure.Both);
        var value = BoundaryFlowValueSurface.Create(
            new Carrier(),
            carrier,
            5,
            BoundaryFlowLane.NegativeInverse,
            axis);

        Assert.True(BoundaryFlowValueSurface.TryRead(
            value,
            out var magnitude,
            out var lane,
            out _,
            out var basis,
            out _));
        Assert.Equal(new CarrierLocation(carrier, 5), magnitude);
        Assert.Equal(BoundaryFlowLane.NegativeInverse, lane);
        Assert.Equal(axis, basis);

        Assert.True(BoundaryFlowValueSurface.TryReadOccupation(
            value,
            axis,
            out _,
            out _,
            out var isLegal));
        Assert.True(isLegal);
    }

    [Fact]
    public void ValueSurfaceCanShowLaneTensionAgainstItsBasis()
    {
        var carrier = new Carrier();
        var positiveForwardAxis = CarrierAxisSurface.PositiveUnit(new Carrier(), carrier);
        var illegalValue = BoundaryFlowValueSurface.Create(
            new Carrier(),
            carrier,
            5,
            BoundaryFlowLane.NegativeInverse,
            positiveForwardAxis);

        Assert.True(BoundaryFlowValueSurface.TryReadOccupation(
            illegalValue,
            positiveForwardAxis,
            out var magnitude,
            out var lane,
            out var isLegal));
        Assert.Equal(new CarrierLocation(carrier, 5), magnitude);
        Assert.Equal(BoundaryFlowLane.NegativeInverse, lane);
        Assert.False(isLegal);
    }

    [Fact]
    public void ValueReadProjectsLaneIntoSignedFlowNotation()
    {
        var carrier = new Carrier();
        var axis = CarrierAxisSurface.SignedBothFlows(new Carrier(), carrier);
        var value = BoundaryFlowValueSurface.Create(
            new Carrier(),
            carrier,
            5,
            BoundaryFlowLane.NegativeInverse,
            axis);

        Assert.True(BoundaryFlowValueSurface.TryReadValue(value, out var read));
        Assert.True(read.IsLegal);
        Assert.True(read.IsInverseFlow);
        Assert.False(read.IsForwardFlow);
        Assert.Equal(-5, read.SignedTicks);
        Assert.Equal("-5i", read.Notation);
    }

    [Fact]
    public void SingleLaneBasisCanCreateInferredValueSurface()
    {
        var carrier = new Carrier();
        var axis = CarrierAxisSurface.PositiveUnit(new Carrier(), carrier);
        var magnitude = Relation.Point(carrier, 7);

        Assert.True(BoundaryFlowValueSurface.TryCreateInferred(
            new Carrier(),
            magnitude,
            axis,
            out var value));
        Assert.True(BoundaryFlowValueSurface.TryReadValue(value, out var read));
        Assert.Equal(BoundaryFlowLane.PositiveForward, read.Lane);
        Assert.Equal("7f", read.Notation);
    }

    [Fact]
    public void MultiLaneBasisDoesNotInferValueLane()
    {
        var carrier = new Carrier();
        var axis = CarrierAxisSurface.SignedForward(new Carrier(), carrier);
        var magnitude = Relation.Point(carrier, 7);

        Assert.False(BoundaryFlowValueSurface.TryCreateInferred(
            new Carrier(),
            magnitude,
            axis,
            out _));
    }

    [Fact]
    public void SignedForwardSurfaceCanSettleAcrossPositiveAndNegativeLanes()
    {
        var carrier = new Carrier();
        var axis = CarrierAxisSurface.SignedForward(new Carrier(), carrier);
        var value = BoundaryFlowValueSurface.Create(
            new Carrier(),
            carrier,
            5,
            BoundaryFlowLane.PositiveForward,
            axis);

        Assert.True(BoundaryFlowValueSettlement.TryApplySignedDelta(
            value,
            axis,
            -8,
            out var settled));
        Assert.True(BoundaryFlowValueSurface.TryReadValue(settled, out var read));
        Assert.True(read.IsLegal);
        Assert.Equal(BoundaryFlowLane.NegativeForward, read.Lane);
        Assert.Equal(3, read.Magnitude.Ticks);
        Assert.Equal(-3, read.SignedTicks);
        Assert.Equal("-3f", read.Notation);
        Assert.True(ConceptSurfaceReaderForTests.TryReadPoint(read.ResidueOrTension, out var residue));
        Assert.Equal(0, residue.Ticks);
    }

    [Fact]
    public void PositiveForwardSurfaceLeavesResidueWhenDeltaWouldNeedNegativeLane()
    {
        var carrier = new Carrier();
        var axis = CarrierAxisSurface.PositiveUnit(new Carrier(), carrier);
        var value = BoundaryFlowValueSurface.Create(
            new Carrier(),
            carrier,
            5,
            BoundaryFlowLane.PositiveForward,
            axis);

        Assert.True(BoundaryFlowValueSettlement.TryApplySignedDelta(
            value,
            axis,
            -8,
            out var settled));
        Assert.True(BoundaryFlowValueSurface.TryReadValue(settled, out var read));
        Assert.True(read.IsLegal);
        Assert.Equal(BoundaryFlowLane.PositiveForward, read.Lane);
        Assert.Equal(0, read.Magnitude.Ticks);
        Assert.True(ConceptSurfaceReaderForTests.TryReadPoint(read.ResidueOrTension, out var residue));
        Assert.Equal(-3, residue.Ticks);
    }

    [Fact]
    public void RouteProjectsSignedForwardValueAcrossCarrierBases()
    {
        var source = new Carrier();
        var target = new Carrier();
        var sourceAxis = CarrierAxisSurface.SignedForward(new Carrier(), source);
        var targetAxis = CarrierAxisSurface.SignedForward(new Carrier(), target);
        var sourceValue = BoundaryFlowValueSurface.Create(
            new Carrier(),
            source,
            5,
            BoundaryFlowLane.NegativeForward,
            sourceAxis);
        var route = new Relation(
            Relation.Segment(source, -10, 10),
            Relation.Segment(target, -100, 100));

        Assert.True(BoundaryFlowValueRoute.TryProject(
            sourceValue,
            route,
            targetAxis,
            target,
            out var routeSurface));
        Assert.True(BoundaryFlowValueRoute.TryRead(
            routeSurface,
            out var readSourceValue,
            out var readRoute,
            out var readTargetAxis,
            out var targetValue,
            out var routeResidue));
        Assert.Equal(sourceValue, readSourceValue);
        Assert.Equal(route, readRoute);
        Assert.Equal(targetAxis, readTargetAxis);
        Assert.True(BoundaryFlowValueSurface.TryReadValue(targetValue, out var targetRead));
        Assert.True(targetRead.IsLegal);
        Assert.Equal(BoundaryFlowLane.NegativeForward, targetRead.Lane);
        Assert.Equal(50, targetRead.Magnitude.Ticks);
        Assert.Equal(-50, targetRead.SignedTicks);
        Assert.Equal("-50f", targetRead.Notation);
        Assert.Equal(targetRead.ResidueOrTension, routeResidue);
        Assert.True(ConceptSurfaceReaderForTests.TryReadPoint(routeResidue, out var residue));
        Assert.Equal(0, residue.Ticks);
    }

    [Fact]
    public void RouteKeepsRejectedInverseFlowVisibleOnForwardOnlyTarget()
    {
        var source = new Carrier();
        var target = new Carrier();
        var sourceAxis = CarrierAxisSurface.SignedBothFlows(new Carrier(), source);
        var targetAxis = CarrierAxisSurface.SignedForward(new Carrier(), target);
        var sourceValue = BoundaryFlowValueSurface.Create(
            new Carrier(),
            source,
            5,
            BoundaryFlowLane.NegativeInverse,
            sourceAxis);
        var route = new Relation(
            Relation.Segment(source, -10, 10),
            Relation.Segment(target, -10, 10));

        Assert.True(BoundaryFlowValueRoute.TryProject(
            sourceValue,
            route,
            targetAxis,
            target,
            out var routeSurface));
        Assert.True(BoundaryFlowValueRoute.TryReadTargetValue(routeSurface, out var targetRead));
        Assert.False(targetRead.IsLegal);
        Assert.Equal(BoundaryFlowLane.NegativeInverse, targetRead.Lane);
        Assert.Equal(5, targetRead.Magnitude.Ticks);
        Assert.Equal("-5i", targetRead.Notation);
        Assert.True(ConceptSurfaceReaderForTests.TryReadPoint(targetRead.ResidueOrTension, out var residue));
        Assert.Equal(-5, residue.Ticks);
    }

    [Fact]
    public void RouteScalesPositiveForwardValueIntoPositiveForwardTarget()
    {
        var source = new Carrier();
        var target = new Carrier();
        var sourceAxis = CarrierAxisSurface.PositiveUnit(new Carrier(), source);
        var targetAxis = CarrierAxisSurface.PositiveUnit(new Carrier(), target);
        var sourceValue = BoundaryFlowValueSurface.Create(
            new Carrier(),
            source,
            5,
            BoundaryFlowLane.PositiveForward,
            sourceAxis);
        var route = new Relation(
            Relation.Segment(source, 0, 10),
            Relation.Segment(target, 0, 100));

        Assert.True(BoundaryFlowValueRoute.TryProject(
            sourceValue,
            route,
            targetAxis,
            target,
            out var routeSurface));
        Assert.True(BoundaryFlowValueRoute.TryReadTargetValue(routeSurface, out var targetRead));
        Assert.True(targetRead.IsLegal);
        Assert.Equal(BoundaryFlowLane.PositiveForward, targetRead.Lane);
        Assert.Equal(50, targetRead.Magnitude.Ticks);
        Assert.Equal("50f", targetRead.Notation);
    }

    [Fact]
    public void TransferFullyAcceptsLegalProjectedLane()
    {
        var source = new Carrier();
        var target = new Carrier();
        var sourceAxis = CarrierAxisSurface.SignedForward(new Carrier(), source);
        var targetAxis = CarrierAxisSurface.SignedForward(new Carrier(), target);
        var sourceValue = BoundaryFlowValueSurface.Create(
            new Carrier(),
            source,
            5,
            BoundaryFlowLane.NegativeForward,
            sourceAxis);
        var route = new Relation(
            Relation.Segment(source, -10, 10),
            Relation.Segment(target, -100, 100));

        Assert.True(BoundaryFlowValueRoute.TryProject(
            sourceValue,
            route,
            targetAxis,
            target,
            out var routeSurface));
        Assert.True(BoundaryFlowValueTransfer.TryAccept(routeSurface, out var transfer));
        Assert.True(BoundaryFlowValueTransfer.TryReadSummary(transfer, out var read));
        Assert.True(read.IsFullyAccepted);
        Assert.False(read.IsPartiallyAccepted);
        Assert.Equal(BoundaryFlowLane.NegativeForward, read.Accepted.Lane);
        Assert.Equal(50, read.Accepted.Magnitude.Ticks);
        Assert.Equal("-50f", read.Accepted.Notation);
        Assert.Equal(0, read.Blocked.Magnitude.Ticks);
    }

    [Fact]
    public void TransferBlocksIllegalProjectedLaneWithoutInventingAcceptedValue()
    {
        var source = new Carrier();
        var target = new Carrier();
        var sourceAxis = CarrierAxisSurface.SignedBothFlows(new Carrier(), source);
        var targetAxis = CarrierAxisSurface.SignedForward(new Carrier(), target);
        var sourceValue = BoundaryFlowValueSurface.Create(
            new Carrier(),
            source,
            5,
            BoundaryFlowLane.NegativeInverse,
            sourceAxis);
        var route = new Relation(
            Relation.Segment(source, -10, 10),
            Relation.Segment(target, -10, 10));

        Assert.True(BoundaryFlowValueRoute.TryProject(
            sourceValue,
            route,
            targetAxis,
            target,
            out var routeSurface));
        Assert.True(BoundaryFlowValueTransfer.TryAccept(routeSurface, out var transfer));
        Assert.False(BoundaryFlowValueTransfer.TryReadAcceptedValue(transfer, out _));
        Assert.True(BoundaryFlowValueTransfer.TryReadSummary(transfer, out var read));
        Assert.True(read.IsFullyBlocked);
        Assert.False(read.HasAccepted);
        Assert.False(read.Blocked.IsLegal);
        Assert.Equal(BoundaryFlowLane.NegativeInverse, read.Blocked.Lane);
        Assert.Equal(5, read.Blocked.Magnitude.Ticks);
        Assert.Equal("-5i", read.Blocked.Notation);
        Assert.True(ConceptSurfaceReaderForTests.TryReadPoint(read.Blocked.ResidueOrTension, out var residue));
        Assert.Equal(-5, residue.Ticks);
    }

    [Fact]
    public void TransferPartiallyAcceptsCapacityLimitedProjection()
    {
        var source = new Carrier();
        var target = new Carrier();
        var sourceAxis = CarrierAxisSurface.PositiveUnit(new Carrier(), source);
        var targetAxis = CarrierAxisSurface.PositiveUnit(new Carrier(), target);
        var sourceValue = BoundaryFlowValueSurface.Create(
            new Carrier(),
            source,
            5,
            BoundaryFlowLane.PositiveForward,
            sourceAxis);
        var route = new Relation(
            Relation.Segment(source, 0, 10),
            Relation.Segment(target, 0, 100));
        var capacity = Relation.Point(target, 30);

        Assert.True(BoundaryFlowValueRoute.TryProject(
            sourceValue,
            route,
            targetAxis,
            target,
            out var routeSurface));
        Assert.True(BoundaryFlowValueTransfer.TryAccept(
            routeSurface,
            capacity,
            out var transfer));
        Assert.True(BoundaryFlowValueTransfer.TryReadSummary(transfer, out var read));
        Assert.True(read.IsPartiallyAccepted);
        Assert.Equal(BoundaryFlowLane.PositiveForward, read.Accepted.Lane);
        Assert.Equal(30, read.Accepted.Magnitude.Ticks);
        Assert.Equal("30f", read.Accepted.Notation);
        Assert.Equal(BoundaryFlowLane.PositiveForward, read.Blocked.Lane);
        Assert.Equal(20, read.Blocked.Magnitude.Ticks);
        Assert.Equal("20f", read.Blocked.Notation);
        Assert.True(ConceptSurfaceReaderForTests.TryReadPoint(read.Blocked.ResidueOrTension, out var residue));
        Assert.Equal(20, residue.Ticks);
    }

    [Fact]
    public void TargetSettlementAccumulatesAcceptedTransferAndWritesTimeline()
    {
        var source = new Carrier();
        var target = new Carrier();
        var sourceAxis = CarrierAxisSurface.PositiveUnit(new Carrier(), source);
        var targetAxis = CarrierAxisSurface.PositiveUnit(new Carrier(), target);
        var sourceValue = BoundaryFlowValueSurface.Create(
            new Carrier(),
            source,
            3,
            BoundaryFlowLane.PositiveForward,
            sourceAxis);
        var targetBefore = BoundaryFlowValueSurface.Create(
            new Carrier(),
            target,
            20,
            BoundaryFlowLane.PositiveForward,
            targetAxis);
        var route = new Relation(
            Relation.Segment(source, 0, 10),
            Relation.Segment(target, 0, 100));

        Assert.True(BoundaryFlowValueRoute.TryProject(
            sourceValue,
            route,
            targetAxis,
            target,
            out var routeSurface));
        Assert.True(BoundaryFlowValueTransfer.TryAccept(routeSurface, out var transfer));
        Assert.True(BoundaryFlowTargetSettlement.TrySettle(
            targetBefore,
            transfer,
            out var settlement));
        Assert.True(BoundaryFlowTargetSettlement.TryRead(
            settlement,
            out var readBefore,
            out var readTransfer,
            out var readAfter,
            out var residue,
            out var timeline));
        Assert.Equal(targetBefore, readBefore);
        Assert.Equal(transfer, readTransfer);
        Assert.True(BoundaryFlowValueSurface.TryReadValue(readAfter, out var afterRead));
        Assert.True(afterRead.IsLegal);
        Assert.Equal(BoundaryFlowLane.PositiveForward, afterRead.Lane);
        Assert.Equal(50, afterRead.Magnitude.Ticks);
        Assert.Equal("50f", afterRead.Notation);
        Assert.True(ConceptSurfaceReaderForTests.TryReadPoint(residue, out var residuePoint));
        Assert.Equal(0, residuePoint.Ticks);

        Assert.True(TimeStepSequence.TryRead(timeline, out _, out var initial, out var steps));
        Assert.Equal(targetBefore, initial);
        Assert.Single(steps);
        Assert.True(TimeStepSequence.TryReadStep(
            steps[0],
            out _,
            out var start,
            out var end,
            out var stepBefore,
            out var transition,
            out var stepAfter,
            out var stepResidue));
        Assert.Equal(0, start.Ticks);
        Assert.Equal(1, end.Ticks);
        Assert.Equal(targetBefore, stepBefore);
        Assert.Equal(transfer, transition);
        Assert.Equal(readAfter, stepAfter);
        Assert.Equal(residue, stepResidue);
    }

    [Fact]
    public void TargetSettlementKeepsBeforeValueWhenTransferIsBlocked()
    {
        var source = new Carrier();
        var target = new Carrier();
        var sourceAxis = CarrierAxisSurface.SignedBothFlows(new Carrier(), source);
        var targetAxis = CarrierAxisSurface.SignedForward(new Carrier(), target);
        var sourceValue = BoundaryFlowValueSurface.Create(
            new Carrier(),
            source,
            5,
            BoundaryFlowLane.NegativeInverse,
            sourceAxis);
        var targetBefore = BoundaryFlowValueSurface.Create(
            new Carrier(),
            target,
            20,
            BoundaryFlowLane.PositiveForward,
            targetAxis);
        var route = new Relation(
            Relation.Segment(source, -10, 10),
            Relation.Segment(target, -10, 10));

        Assert.True(BoundaryFlowValueRoute.TryProject(
            sourceValue,
            route,
            targetAxis,
            target,
            out var routeSurface));
        Assert.True(BoundaryFlowValueTransfer.TryAccept(routeSurface, out var transfer));
        Assert.True(BoundaryFlowTargetSettlement.TrySettle(
            targetBefore,
            transfer,
            out var settlement));
        Assert.True(BoundaryFlowTargetSettlement.TryRead(
            settlement,
            out _,
            out _,
            out var after,
            out var residue,
            out _));
        Assert.Equal(targetBefore, after);
        Assert.True(BoundaryFlowValueSurface.TryReadValue(after, out var afterRead));
        Assert.Equal(20, afterRead.Magnitude.Ticks);
        Assert.Equal("20f", afterRead.Notation);
        Assert.True(BoundaryFlowValueSurface.TryReadValue(Assert.IsType<Relation>(residue), out var blockedRead));
        Assert.False(blockedRead.IsLegal);
        Assert.Equal(BoundaryFlowLane.NegativeInverse, blockedRead.Lane);
        Assert.Equal(5, blockedRead.Magnitude.Ticks);
    }

    [Fact]
    public void TargetSettlementCanCrossSignedLaneWhenAcceptedTransferOpposesBefore()
    {
        var source = new Carrier();
        var target = new Carrier();
        var sourceAxis = CarrierAxisSurface.SignedForward(new Carrier(), source);
        var targetAxis = CarrierAxisSurface.SignedForward(new Carrier(), target);
        var sourceValue = BoundaryFlowValueSurface.Create(
            new Carrier(),
            source,
            5,
            BoundaryFlowLane.NegativeForward,
            sourceAxis);
        var targetBefore = BoundaryFlowValueSurface.Create(
            new Carrier(),
            target,
            20,
            BoundaryFlowLane.PositiveForward,
            targetAxis);
        var route = new Relation(
            Relation.Segment(source, -10, 10),
            Relation.Segment(target, -100, 100));

        Assert.True(BoundaryFlowValueRoute.TryProject(
            sourceValue,
            route,
            targetAxis,
            target,
            out var routeSurface));
        Assert.True(BoundaryFlowValueTransfer.TryAccept(routeSurface, out var transfer));
        Assert.True(BoundaryFlowTargetSettlement.TrySettle(
            targetBefore,
            transfer,
            out var settlement));
        Assert.True(BoundaryFlowTargetSettlement.TryReadAfter(settlement, out var afterRead));
        Assert.True(afterRead.IsLegal);
        Assert.Equal(BoundaryFlowLane.NegativeForward, afterRead.Lane);
        Assert.Equal(30, afterRead.Magnitude.Ticks);
        Assert.Equal(-30, afterRead.SignedTicks);
        Assert.Equal("-30f", afterRead.Notation);
    }

    public static IEnumerable<object[]> SelectedLaneBasisStates()
    {
        yield return [BoundaryExposure.Positive, FlowExposure.Forward, "PF"];
        yield return [BoundaryExposure.Negative, FlowExposure.Inverse, "NI"];
        yield return [BoundaryExposure.Negative, FlowExposure.Both, "NF|NI"];
        yield return [BoundaryExposure.FullSigned, FlowExposure.Forward, "PF|NF"];
        yield return [BoundaryExposure.FullSigned, FlowExposure.Inverse, "PI|NI"];
        yield return [BoundaryExposure.FullSigned, FlowExposure.Both, "PF|PI|NF|NI"];
        yield return [BoundaryExposure.BoundedWhole, FlowExposure.None, "S:None"];
        yield return [BoundaryExposure.BoundedWhole, FlowExposure.Both, "SF|SI"];
    }
}

internal static class ConceptSurfaceReaderForTests
{
    public static bool TryReadPoint(IElement element, out CarrierLocation location)
    {
        if (element is Relation point &&
            point.Left is CarrierLocation pointLocation &&
            point.Right is Carrier pointCarrier &&
            pointLocation.Carrier.SameIdentityAs(pointCarrier))
        {
            location = pointLocation;
            return true;
        }

        location = default;
        return false;
    }
}
