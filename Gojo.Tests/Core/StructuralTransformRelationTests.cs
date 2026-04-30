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
        Assert.Equal(0, carrier.Tension);
    }

    [Fact]
    public void ForwardCarrierLocationRelationDoesNotCreateTension()
    {
        var carrier = new Carrier(10);
        carrier.Attach(new Relation(new CarrierLocation(carrier, 0), new CarrierLocation(carrier, 10)));

        carrier.SetValue(11);

        Assert.Equal(11UL, carrier.Value);
        Assert.Equal(0, carrier.Tension);
    }

    [Fact]
    public void ReverseCarrierLocationRelationWrapsInsteadOfClamping()
    {
        var carrier = new Carrier(10);
        carrier.Attach(new Relation(new CarrierLocation(carrier, 10), new CarrierLocation(carrier, 0)));

        carrier.SetValue(13);

        Assert.Equal(3UL, carrier.Value);
        Assert.Equal(10, carrier.Tension);
    }

    [Fact]
    public void SameCarrierResetBoundaryAccountsResidueAndPositiveTension()
    {
        var carrier = new Carrier(10);
        carrier.Attach(new Relation(Point(carrier, 10), Point(carrier, 0)));

        carrier.SetValue(23);

        Assert.Equal(3UL, carrier.Value);
        Assert.Equal(20, carrier.Tension);
    }

    [Fact]
    public void SameCarrierClampBoundaryAccountsLimitAndPositiveTension()
    {
        var carrier = new Carrier(10);
        carrier.Attach(new Relation(Point(carrier, 10), Point(carrier, 10)));

        carrier.SetValue(23);

        Assert.Equal(10UL, carrier.Value);
        Assert.Equal(13, carrier.Tension);
    }

    [Fact]
    public void CrossCarrierPointRelationRoutesPositiveTension()
    {
        var residue = new Carrier(10);
        var whole = new Carrier(20);
        residue.Attach(new Relation(Point(residue, 10), Point(whole, 0)));

        residue.SetValue(37);

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

        source.SetValue(6);

        Assert.Equal(6UL, source.Value);
        Assert.Equal(0, source.Tension);
        Assert.Equal(0UL, projection.Value);
        Assert.Equal(0, projection.Tension);
    }

    [Fact]
    public void ExplicitBoundaryRelationRoutesPersistedPositiveTensionWhenAddedLater()
    {
        var residue = new Carrier(10);
        var whole = new Carrier(20);
        var boundary = residue.Attach(new Relation(Point(residue, 10), Point(residue, 0)));
        residue.SetValue(23);

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

        ones.SetValue(375);

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

    private static Relation Point(Carrier carrier, ulong ticks) =>
        new(new CarrierLocation(carrier, ticks), carrier);
}
