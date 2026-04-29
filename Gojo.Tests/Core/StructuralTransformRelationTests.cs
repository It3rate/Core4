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
    public void CarrierWithoutStructureKeepsInputAsLocalValue()
    {
        var carrier = new Carrier(10);

        carrier.SetValue(11);

        Assert.Equal(11UL, carrier.Value);
        Assert.Equal(0UL, carrier.Overflow);
    }

    [Fact]
    public void CarrierLocalCapacityCreatesOverflowWithoutExternalSettlement()
    {
        var carrier = new Carrier(10);
        carrier.Attach(new Relation(new CarrierLocation(carrier, 0), new CarrierLocation(carrier, 10)));

        carrier.SetValue(11);

        Assert.Equal(10UL, carrier.Value);
        Assert.Equal(1UL, carrier.Overflow);
    }

    [Fact]
    public void SameCarrierResetBoundaryAccountsResidueAndOverflow()
    {
        var carrier = new Carrier(10);
        carrier.Attach(new Relation(Point(carrier, 10), Point(carrier, 0)));

        carrier.SetValue(23);

        Assert.Equal(3UL, carrier.Value);
        Assert.Equal(20UL, carrier.Overflow);
    }

    [Fact]
    public void SameCarrierClampBoundaryAccountsLimitAndOverflow()
    {
        var carrier = new Carrier(10);
        carrier.Attach(new Relation(Point(carrier, 10), Point(carrier, 10)));

        carrier.SetValue(23);

        Assert.Equal(10UL, carrier.Value);
        Assert.Equal(13UL, carrier.Overflow);
    }

    [Fact]
    public void CrossCarrierPointRelationRoutesOverflow()
    {
        var residue = new Carrier(10);
        var whole = new Carrier(20);
        residue.Attach(new Relation(Point(residue, 10), Point(whole, 0)));

        residue.SetValue(37);

        Assert.Equal(7UL, residue.Value);
        Assert.Equal(0UL, residue.Overflow);
        Assert.Equal(3UL, whole.Value);
    }

    [Fact]
    public void ExplicitBoundaryRelationRoutesPersistedOverflowWhenAddedLater()
    {
        var residue = new Carrier(10);
        var whole = new Carrier(20);
        var boundary = residue.Attach(new Relation(Point(residue, 10), Point(residue, 0)));
        residue.SetValue(23);

        whole.Attach(new Relation(boundary, Point(whole, 0)));

        Assert.Equal(3UL, residue.Value);
        Assert.Equal(0UL, residue.Overflow);
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

        ones.SetValue(375);

        Assert.Equal(5UL, ones.Value);
        Assert.Equal(7UL, tens.Value);
        Assert.Equal(3UL, hundreds.Value);
    }

    [Fact]
    public void UnderflowStaysOnCarrierWithoutRelatedInverseReceiver()
    {
        var source = new Carrier(10);
        _ = new Carrier(isInverse: true);

        source.Remove(23);

        Assert.Equal(23UL, source.Underflow);
    }

    [Fact]
    public void UnderflowRoutesThroughRelatedInversePointReceiver()
    {
        var source = new Carrier(10);
        var inverse = new Carrier(isInverse: true);
        source.Attach(new Relation(Point(source, 10), Point(inverse, 0)));

        source.Remove(23);

        Assert.Equal(3UL, source.Underflow);
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

    private static Relation Point(Carrier carrier, ulong ticks) =>
        new(new CarrierLocation(carrier, ticks), carrier);
}
