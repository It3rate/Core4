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
    public void LayerTransformCarriesOneNumericRelation()
    {
        var expansive = LayerTransform.Expansive(3);
        var contractive = LayerTransform.Contractive(10);
        var additive = LayerTransform.Additive;

        Assert.Equal(Carrier.Unknown, expansive.Value.Carrier);
        Assert.True(expansive.IsExpansive);
        Assert.Equal(3UL, expansive.Magnitude);
        Assert.Equal(Carrier.Unknown.Inverse(), contractive.Value.Carrier);
        Assert.True(contractive.IsContractive);
        Assert.Equal(10UL, contractive.Magnitude);
        Assert.Equal(Carrier.Unknown, additive.Value.Carrier);
        Assert.True(additive.IsAdditive);
        Assert.Equal(0UL, additive.Magnitude);
    }

    [Fact]
    public void LayeredReadPreservesBothCarrierLayersInsteadOfDecimalProjection()
    {
        var celsius = new Carrier(10);
        var tenth = new Carrier(11);
        var read = LayeredRead.FromBoundary(celsius, tenth, 234, LayerTransform.Contractive(10));

        Assert.Equal(new CarrierLocation(celsius, 23), read.Earlier);
        Assert.Equal(new CarrierLocation(tenth, 4), read.Later);
        Assert.Equal(10UL, read.Boundary.Magnitude);
        Assert.Equal("23 4|10", read.ToString());
    }

    [Fact]
    public void BoundaryEventsUseCarrierNumbersForSideAndTravelDirection()
    {
        Assert.Equal(new CarrierLocation(BoundaryEvent.SideCarrier, 1), BoundaryEvent.PositiveOverflow.Side);
        Assert.Equal(new CarrierLocation(BoundaryEvent.TravelCarrier, 1), BoundaryEvent.PositiveOverflow.Travel);
        Assert.Equal(new CarrierLocation(BoundaryEvent.SideCarrier, 1), BoundaryEvent.PositiveRefill.Side);
        Assert.Equal(new CarrierLocation(BoundaryEvent.TravelCarrier.Inverse(), 1), BoundaryEvent.PositiveRefill.Travel);
        Assert.Equal(new CarrierLocation(BoundaryEvent.SideCarrier.Inverse(), 1), BoundaryEvent.NegativeRefill.Side);
        Assert.Equal(new CarrierLocation(BoundaryEvent.TravelCarrier, 1), BoundaryEvent.NegativeRefill.Travel);
        Assert.Equal(new CarrierLocation(BoundaryEvent.SideCarrier.Inverse(), 1), BoundaryEvent.NegativeOverflow.Side);
        Assert.Equal(new CarrierLocation(BoundaryEvent.TravelCarrier.Inverse(), 1), BoundaryEvent.NegativeOverflow.Travel);

        Assert.Equal(1, BoundaryEvent.PositiveOverflow.Alignment);
        Assert.Equal(-1, BoundaryEvent.PositiveRefill.Alignment);
        Assert.Equal(-1, BoundaryEvent.NegativeRefill.Alignment);
        Assert.Equal(1, BoundaryEvent.NegativeOverflow.Alignment);
    }
}
