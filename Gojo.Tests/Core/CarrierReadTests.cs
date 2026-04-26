using Gojo.Core;

namespace Gojo.Tests.Core;

public sealed class CarrierReadTests
{
    [Fact]
    public void SameCarrierReadSeesPositiveTicks()
    {
        var x = new Carrier(10);
        var location = new CarrierLocation(x, 5);
        var read = location.ReadFrom(x);

        Assert.Equal(5UL, location.Ticks);
        Assert.True(read.IsVisible);
        Assert.Equal(1, read.Direction);
        Assert.Equal(5, read.ProjectedTicks);
    }

    [Fact]
    public void OrthogonalCarrierReadReturnsZero()
    {
        var x = new Carrier(10);
        var y = new Carrier(20);
        var location = new CarrierLocation(y, 5);
        var read = location.ReadFrom(x);

        Assert.Equal(0, read.Direction);
        Assert.False(read.IsVisible);
        Assert.Equal(0, read.ProjectedTicks);
    }

    [Fact]
    public void OpposedCarrierReadProjectsNegativeWithoutNegativeStorage()
    {
        var forward = new Carrier(10);
        var inverse = forward.Inverse();
        var location = new CarrierLocation(inverse, 5);

        var readFromForward = location.ReadFrom(forward);
        var readFromInverse = location.ReadFrom(inverse);

        Assert.Equal(5UL, location.Ticks);
        Assert.Equal(-1, readFromForward.Direction);
        Assert.Equal(-5, readFromForward.ProjectedTicks);
        Assert.Equal(1, readFromInverse.Direction);
        Assert.Equal(5, readFromInverse.ProjectedTicks);
    }

    [Fact]
    public void CarrierLocationCarriesTransformWhenReadFromTransformSlot()
    {
        var transformReader = Carrier.Unknown;
        var expansive = new CarrierLocation(transformReader, 3);
        var contractive = new CarrierLocation(transformReader.Inverse(), 10);
        var additive = new CarrierLocation(transformReader, 0);

        Assert.Equal(1, expansive.ReadFrom(transformReader).Direction);
        Assert.Equal(3UL, expansive.Ticks);
        Assert.Equal(-1, contractive.ReadFrom(transformReader).Direction);
        Assert.Equal(10UL, contractive.Ticks);
        Assert.False(additive.ReadFrom(transformReader).IsVisible);
        Assert.Equal(0UL, additive.Ticks);
    }

    [Fact]
    public void BoundaryReadPreservesBothCarrierLayersInsteadOfDecimalProjection()
    {
        var celsius = new Carrier(10);
        var tenth = new Carrier(11);
        var boundary = new CarrierLocation(Carrier.Unknown.Inverse(), 10);
        var read = new CarrierLocation(tenth, 234).ReadThroughBoundary(celsius, boundary);

        Assert.Equal(
            new Relation(
                new CarrierLocation(celsius, 23),
                new CarrierLocation(tenth, 4)),
            read);
        Assert.Equal("23 4|10", $"{read.Left.Ticks} {read.Right.Ticks}|{boundary.Ticks}");
    }

    [Fact]
    public void BoundarySideAndTravelCanBePlainCarrierLocations()
    {
        var side = new Carrier(101);
        var travel = new Carrier(102);

        var positiveOverflow = Alignment(new CarrierLocation(side, 1), new CarrierLocation(travel, 1));
        var positiveRefill = Alignment(new CarrierLocation(side, 1), new CarrierLocation(travel.Inverse(), 1));
        var negativeRefill = Alignment(new CarrierLocation(side.Inverse(), 1), new CarrierLocation(travel, 1));
        var negativeOverflow = Alignment(new CarrierLocation(side.Inverse(), 1), new CarrierLocation(travel.Inverse(), 1));

        Assert.Equal(1, positiveOverflow);
        Assert.Equal(-1, positiveRefill);
        Assert.Equal(-1, negativeRefill);
        Assert.Equal(1, negativeOverflow);

        long Alignment(CarrierLocation sideLocation, CarrierLocation travelLocation) =>
            Math.Sign(side.ReadSignOf(sideLocation.Carrier) * travel.ReadSignOf(travelLocation.Carrier));
    }

    [Fact]
    public void NewTensionAppearsOnlyPastAccountedCarrierRegion()
    {
        var carrier = new Carrier(10);
        var accounted = new CarrierLocation(carrier, 20);

        var inside = new CarrierLocation(carrier, 11).NewTensionAfter(accounted, carrier);
        var boundary = new CarrierLocation(carrier, 20).NewTensionAfter(accounted, carrier);
        var beyond = new CarrierLocation(carrier, 21).NewTensionAfter(accounted, carrier);

        Assert.Equal(new CarrierLocation(carrier, 0), inside);
        Assert.Equal(new CarrierLocation(carrier, 0), boundary);
        Assert.Equal(new CarrierLocation(carrier, 1), beyond);
    }

    [Fact]
    public void MultipleCompatibleAccountingSurfacesAccumulateBeforeNewTensionAppears()
    {
        var carrier = new Carrier(10);
        var incoming = new CarrierLocation(carrier, 15);

        var partial = incoming.NewTensionAfter(
            carrier,
            new CarrierLocation(carrier, 7),
            new CarrierLocation(carrier, 6));

        var covered = incoming.NewTensionAfter(
            carrier,
            new CarrierLocation(carrier, 7),
            new CarrierLocation(carrier, 8));

        Assert.Equal(new CarrierLocation(carrier, 2), partial);
        Assert.Equal(new CarrierLocation(carrier, 0), covered);
    }

    [Fact]
    public void AccountingReadReturnsAccountedParticipationAndNewTension()
    {
        var carrier = new Carrier(10);
        var incoming = new CarrierLocation(carrier, 15);

        var read = incoming.AccountedBy(
            carrier,
            new CarrierLocation(carrier, 7),
            new CarrierLocation(carrier, 6));

        Assert.Equal(
            new Relation(
                new CarrierLocation(carrier, 13),
                new CarrierLocation(carrier, 2)),
            read);
    }

    [Fact]
    public void AccountingReadReturnsNoParticipationWhenReaderCannotSeeIncomingCarrier()
    {
        var x = new Carrier(10);
        var y = new Carrier(20);
        var incoming = new CarrierLocation(y, 15);

        var read = incoming.AccountedBy(x, new CarrierLocation(y, 15));

        Assert.Equal(
            new Relation(
                new CarrierLocation(y, 0),
                new CarrierLocation(y, 0)),
            read);
    }

    [Fact]
    public void MultipleAccountingSurfacesIgnoreOrthogonalAndOpposedParticipation()
    {
        var x = new Carrier(10);
        var y = new Carrier(20);
        var incoming = new CarrierLocation(x, 10);

        var tension = incoming.NewTensionAfter(
            x,
            new CarrierLocation(x, 3),
            new CarrierLocation(y, 100),
            new CarrierLocation(x.Inverse(), 100),
            new CarrierLocation(x, 4));

        Assert.Equal(new CarrierLocation(x, 3), tension);
    }

    [Fact]
    public void SameLineageCarrierAccountingCanAbsorbAcrossDuplicateCarrier()
    {
        var source = new Carrier(10);
        var duplicate = source.Duplicate(11);
        var incoming = new CarrierLocation(duplicate, 10);

        var tension = incoming.NewTensionAfter(
            duplicate,
            new CarrierLocation(source, 6));

        Assert.Equal(new CarrierLocation(duplicate, 4), tension);
    }

    [Fact]
    public void IndependentCarrierAccountingDoesNotAbsorbCoincidentTicks()
    {
        var source = new Carrier(10);
        var independent = new Carrier(11);
        var incoming = new CarrierLocation(source, 10);

        var tension = incoming.NewTensionAfter(
            source,
            new CarrierLocation(independent, 10));

        Assert.Equal(incoming, tension);
    }

    [Fact]
    public void OpposedCarrierCanBeAccountedWithinTheSameOpposedFlow()
    {
        var forward = new Carrier(10);
        var inverse = forward.Inverse();
        var incoming = new CarrierLocation(inverse, 10);

        var tension = incoming.NewTensionAfter(
            forward,
            new CarrierLocation(inverse, 6));

        Assert.Equal(new CarrierLocation(inverse, 4), tension);
    }

    [Fact]
    public void OrthogonalAccountingDoesNotAbsorbCarrierTension()
    {
        var x = new Carrier(10);
        var y = new Carrier(20);
        var incoming = new CarrierLocation(x, 5);
        var orthogonalAccounted = new CarrierLocation(y, 100);

        var tension = incoming.NewTensionAfter(orthogonalAccounted, x);

        Assert.Equal(incoming, tension);
    }

    [Fact]
    public void OpposedAccountingDoesNotAbsorbCarrierTensionWithoutRoute()
    {
        var forward = new Carrier(10);
        var incoming = new CarrierLocation(forward, 5);
        var opposedAccounted = new CarrierLocation(forward.Inverse(), 100);

        var tension = incoming.NewTensionAfter(opposedAccounted, forward);

        Assert.Equal(incoming, tension);
    }

    [Fact]
    public void InvisibleCarrierDoesNotCreateTensionInReaderFrame()
    {
        var x = new Carrier(10);
        var y = new Carrier(20);
        var incoming = new CarrierLocation(y, 5);
        var accounted = new CarrierLocation(y, 0);

        var tension = incoming.NewTensionAfter(accounted, x);

        Assert.Equal(new CarrierLocation(y, 0), tension);
    }
}
