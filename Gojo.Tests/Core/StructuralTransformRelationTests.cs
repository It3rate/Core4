using Gojo.Core;

namespace Gojo.Tests.Core;

public sealed class StructuralTransformRelationTests
{
    [Fact]
    public void PointIsRelationFromLocationToCarrier()
    {
        var carrier = new Carrier(10);
        var point = Point(carrier, 7);

        Assert.Equal(new CarrierLocation(carrier, 7), point.Left);
        Assert.Equal(carrier, point.Right);
        Assert.Equal(new[] { new CarrierLocation(carrier, 7) }, point.CarrierLocations());
    }

    [Fact]
    public void RelationsHaveIdentitySeparateFromShapeEquality()
    {
        var carrier = new Carrier(10);
        var first = Point(carrier, 7);
        var second = Point(carrier, 7);

        Assert.NotSame(first, second);
        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(first, second);
        Assert.False(first.SameIdentityAs(second));
    }

    [Fact]
    public void SharedPointReferenceMovesWhereItParticipates()
    {
        var carrier = new Carrier(10);
        var start = Point(carrier, 5);
        var end = Point(carrier, 11);
        var segment = new Relation(start, end);

        start.Perturb(new CarrierLocation(carrier, 3));

        Assert.Same(start, segment.Left);
        Assert.Equal(new CarrierLocation(carrier, 8), start.Left);
        Assert.Equal(new CarrierLocation(carrier, 11), end.Left);
    }

    [Fact]
    public void FreshCoincidentPointDoesNotMoveWithUnsharedSegmentPoint()
    {
        var carrier = new Carrier(10);
        var start = Point(carrier, 5);
        var end = Point(carrier, 11);
        var segment = new Relation(start, end);
        var coincident = Point(carrier, 5);

        segment.Perturb(new CarrierLocation(carrier, 3));

        Assert.Equal(new CarrierLocation(carrier, 8), start.Left);
        Assert.Equal(new CarrierLocation(carrier, 14), end.Left);
        Assert.Equal(new CarrierLocation(carrier, 5), coincident.Left);
    }

    [Fact]
    public void TargetedPointTranslationChangesOnlyThatPoint()
    {
        var carrier = new Carrier(10);
        var start = Point(carrier, 5);
        var end = Point(carrier, 11);
        _ = new Relation(start, end);

        end.Perturb(new CarrierLocation(carrier, 3));

        Assert.Equal(new CarrierLocation(carrier, 5), start.Left);
        Assert.Equal(new CarrierLocation(carrier, 14), end.Left);
    }

    [Fact]
    public void TargetedRelationTranslationMovesParticipatingPointsTogether()
    {
        var carrier = new Carrier(10);
        var start = Point(carrier, 5);
        var end = Point(carrier, 11);
        var segment = new Relation(start, end);

        segment.Perturb(new CarrierLocation(carrier, 3));

        Assert.Equal(new CarrierLocation(carrier, 8), start.Left);
        Assert.Equal(new CarrierLocation(carrier, 14), end.Left);
    }

    [Fact]
    public void RelationScalePerturbationPreservesCrossCarrierRatio()
    {
        var source = new Carrier(10);
        var target = new Carrier(20);
        var sourcePoint = Point(source, 2);
        var targetPoint = Point(target, 4);
        var coupling = new Relation(sourcePoint, targetPoint);
        var scale = new Relation(Point(source, 2), Point(source, 3));

        coupling.Perturb(scale);

        Assert.Equal(new CarrierLocation(source, 3), sourcePoint.Left);
        Assert.Equal(new CarrierLocation(target, 6), targetPoint.Left);
    }

    [Fact]
    public void InexactRelationScaleLeavesStructureUnchanged()
    {
        var carrier = new Carrier(10);
        var start = Point(carrier, 1);
        var end = Point(carrier, 2);
        var segment = new Relation(start, end);
        var scale = new Relation(Point(carrier, 2), Point(carrier, 3));

        Assert.False(segment.Perturb(scale));
        Assert.Equal(new CarrierLocation(carrier, 1), start.Left);
        Assert.Equal(new CarrierLocation(carrier, 2), end.Left);
    }

    [Fact]
    public void CarrierWithoutStructureKeepsInputAsLocalValue()
    {
        var carrier = new Carrier(10);

        carrier.Add(11);

        Assert.Equal(11UL, carrier.Value);
        Assert.Equal(0, carrier.Tension);
    }

    [Fact]
    public void ForwardCarrierLocationRelationDoesNotCreateTension()
    {
        var carrier = new Carrier(10);
        carrier.Attach(new Relation(new CarrierLocation(carrier, 0), new CarrierLocation(carrier, 10)));

        carrier.Add(11);

        Assert.Equal(11UL, carrier.Value);
        Assert.Equal(0, carrier.Tension);
    }

    [Fact]
    public void ReverseCarrierLocationRelationWrapsInsteadOfClamping()
    {
        var carrier = new Carrier(10);
        carrier.Attach(new Relation(new CarrierLocation(carrier, 10), new CarrierLocation(carrier, 0)));

        carrier.Add(13);

        Assert.Equal(3UL, carrier.Value);
        Assert.Equal(10, carrier.Tension);
    }

    [Fact]
    public void SameCarrierResetBoundaryAccountsResidueAndPositiveTension()
    {
        var carrier = new Carrier(10);
        carrier.Attach(new Relation(Point(carrier, 10), Point(carrier, 0)));

        carrier.Add(23);

        Assert.Equal(3UL, carrier.Value);
        Assert.Equal(20, carrier.Tension);
    }

    [Fact]
    public void SameCarrierClampBoundaryAccountsLimitAndPositiveTension()
    {
        var carrier = new Carrier(10);
        carrier.Attach(new Relation(Point(carrier, 10), Point(carrier, 10)));

        carrier.Add(23);

        Assert.Equal(10UL, carrier.Value);
        Assert.Equal(13, carrier.Tension);
    }

    [Fact]
    public void CrossCarrierPointRelationRoutesPositiveTension()
    {
        var residue = new Carrier(10);
        var whole = new Carrier(20);
        residue.Attach(new Relation(Point(residue, 10), Point(whole, 0)));

        residue.Add(37);

        Assert.Equal(7UL, residue.Value);
        Assert.Equal(0, residue.Tension);
        Assert.Equal(3UL, whole.Value);
    }

    [Fact]
    public void CrossCarrierNonzeroPointRelationDoesNotAbsorbAsCarry()
    {
        var source = new Carrier(10);
        var projection = new Carrier(20);
        source.Attach(new Relation(Point(source, 2), Point(projection, 3)));

        source.Add(6);

        Assert.Equal(6UL, source.Value);
        Assert.Equal(0, source.Tension);
        Assert.Equal(0UL, projection.Value);
        Assert.Equal(0, projection.Tension);
    }

    [Fact]
    public void CrossCarrierNonzeroPointRelationProjectsExactRatio()
    {
        var source = new Carrier(10);
        var target = new Carrier(20);
        var ratio = new Relation(Point(source, 2), Point(target, 3));

        var projected = ratio.Project(new CarrierLocation(source, 6), target);

        Assert.Equal(
            new Relation(
                new CarrierLocation(target, 9),
                new CarrierLocation(source, 0)),
            projected);
    }

    [Fact]
    public void RatioProjectionCanBeReadBackwardsAsDivision()
    {
        var source = new Carrier(10);
        var target = new Carrier(20);
        var ratio = new Relation(Point(source, 2), Point(target, 3));

        var projected = ratio.Project(new CarrierLocation(target, 9), source);

        Assert.Equal(
            new Relation(
                new CarrierLocation(source, 6),
                new CarrierLocation(target, 0)),
            projected);
    }

    [Fact]
    public void ContractiveRatioLeavesResidueOnOriginalCarrier()
    {
        var source = new Carrier(10);
        var target = new Carrier(20);
        var ratio = new Relation(Point(source, 3), Point(target, 1));

        var projected = ratio.Project(new CarrierLocation(source, 10), target);

        Assert.Equal(
            new Relation(
                new CarrierLocation(target, 3),
                new CarrierLocation(source, 1)),
            projected);
    }

    [Fact]
    public void ExpansiveRatioCanRepresentSubSourceTicksWithoutResidue()
    {
        var source = new Carrier(10);
        var target = new Carrier(20);
        var ratio = new Relation(Point(source, 2), Point(target, 4));

        var projected = ratio.Project(new CarrierLocation(source, 1), target);

        Assert.Equal(
            new Relation(
                new CarrierLocation(target, 2),
                new CarrierLocation(source, 0)),
            projected);
    }

    [Fact]
    public void PointToNegativePointProjectsSubtractionShape()
    {
        var source = new Carrier(10);
        var target = new Carrier(20);
        var subtract = new Relation(Point(source, 1), Point(target, -1));

        var projected = subtract.Project(new CarrierLocation(source, 7), target);

        Assert.Equal(
            new Relation(
                new CarrierLocation(target, -7),
                new CarrierLocation(source, 0)),
            projected);
    }

    [Fact]
    public void NegativePointToPositivePointAlsoProjectsNegativeRatio()
    {
        var source = new Carrier(10);
        var target = new Carrier(20);
        var negativeRatio = new Relation(Point(source, -2), Point(target, 3));

        var projected = negativeRatio.Project(new CarrierLocation(source, 4), target);

        Assert.Equal(
            new Relation(
                new CarrierLocation(target, -6),
                new CarrierLocation(source, 0)),
            projected);
    }

    [Fact]
    public void NegativePointToNegativePointProjectsPositiveRatio()
    {
        var source = new Carrier(10);
        var target = new Carrier(20);
        var positiveRatio = new Relation(Point(source, -2), Point(target, -3));

        var projected = positiveRatio.Project(new CarrierLocation(source, 4), target);

        Assert.Equal(
            new Relation(
                new CarrierLocation(target, 6),
                new CarrierLocation(source, 0)),
            projected);
    }

    [Fact]
    public void NegativeRatioCanBeReadBackwardsAsSignedDivision()
    {
        var source = new Carrier(10);
        var target = new Carrier(20);
        var negativeRatio = new Relation(Point(source, 2), Point(target, -3));

        var projected = negativeRatio.Project(new CarrierLocation(target, -9), source);

        Assert.Equal(
            new Relation(
                new CarrierLocation(source, 6),
                new CarrierLocation(target, 0)),
            projected);
    }

    [Fact]
    public void SegmentToSegmentRelationProjectsByOrderedBasisLengths()
    {
        var source = new Carrier(10);
        var target = new Carrier(20);
        var ratio = new Relation(Segment(source, 0, 2), Segment(target, 0, 3));

        var projected = ratio.Project(new CarrierLocation(source, 6), target);

        Assert.Equal(
            new Relation(
                new CarrierLocation(target, 9),
                new CarrierLocation(source, 0)),
            projected);
    }

    [Fact]
    public void ReversedSourceSegmentProjectsNegativeRatio()
    {
        var source = new Carrier(10);
        var target = new Carrier(20);
        var ratio = new Relation(Segment(source, 2, 0), Segment(target, 0, 3));

        var projected = ratio.Project(new CarrierLocation(source, 4), target);

        Assert.Equal(
            new Relation(
                new CarrierLocation(target, -6),
                new CarrierLocation(source, 0)),
            projected);
    }

    [Fact]
    public void ExplicitBoundaryRelationRoutesPersistedPositiveTensionWhenAddedLater()
    {
        var residue = new Carrier(10);
        var whole = new Carrier(20);
        var boundary = residue.Attach(new Relation(Point(residue, 10), Point(residue, 0)));
        residue.Add(23);

        whole.Attach(new Relation(boundary, Point(whole, 0)));

        Assert.Equal(3UL, residue.Value);
        Assert.Equal(0, residue.Tension);
        Assert.Equal(2UL, whole.Value);
    }

    [Fact]
    public void PointRelationsSettleThroughBaseTenCarryChain()
    {
        var ones = new Carrier(10);
        var tens = new Carrier(20);
        var hundreds = new Carrier(30);
        ones.Attach(new Relation(Point(ones, 10), Point(tens, 0)));
        tens.Attach(new Relation(Point(tens, 10), Point(hundreds, 0)));

        ones.Add(375);

        Assert.Equal(5UL, ones.Value);
        Assert.Equal(7UL, tens.Value);
        Assert.Equal(3UL, hundreds.Value);
    }

    [Fact]
    public void NegativeTensionStaysOnCarrierWithoutRelatedInverseReceiver()
    {
        var source = new Carrier(10);
        _ = new Carrier(isInverse: true);

        source.Remove(23);

        Assert.Equal(-23, source.Tension);
    }

    [Fact]
    public void NegativeTensionRoutesThroughRelatedInversePointReceiver()
    {
        var source = new Carrier(10);
        var inverse = new Carrier(isInverse: true);
        source.Attach(new Relation(Point(source, 10), Point(inverse, 0)));

        source.Remove(23);

        Assert.Equal(-3, source.Tension);
        Assert.Equal(2UL, inverse.Value);
    }

    [Fact]
    public void RelatedInverseCarrierBalancesOpposedVisibleValues()
    {
        var source = new Carrier(10);
        var inverse = new Carrier(isInverse: true);
        source.Attach(new Relation(Point(source, 1), Point(inverse, 0)));
        source.Add(5);

        inverse.Add(7);

        Assert.Equal(0UL, source.Value);
        Assert.Equal(2UL, inverse.Value);
    }

    [Fact]
    public void OpposedResidualCanFeedLaterRelationsInSameWorld()
    {
        var source = new Carrier(10);
        var inverse = new Carrier(isInverse: true);
        var target = new Carrier(isInverse: true);
        source.Attach(new Relation(Point(source, 1), Point(inverse, 0)));
        inverse.Attach(new Relation(Point(inverse, 1), Point(target, 0)));
        source.Add(5);

        inverse.Add(7);

        Assert.Equal(0UL, source.Value);
        Assert.Equal(0UL, inverse.Value);
        Assert.Equal(2UL, target.Value);
    }

    private static Relation Point(Carrier carrier, long ticks) =>
        new(new CarrierLocation(carrier, ticks), carrier);

    private static Relation Segment(Carrier carrier, long left, long right) =>
        new(Point(carrier, left), Point(carrier, right));
}
