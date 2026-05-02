using Gojo.Core;

namespace Gojo.Tests.Core;

public sealed class CarrierSlotTests
{
    [Fact]
    public void SetFeedsDifferenceFromCurrentVisibleValue()
    {
        var carrier = new Carrier();
        carrier.Add(4);

        carrier.Set(10);

        Assert.Equal(10UL, carrier.Value);
        Assert.Equal(0, carrier.Tension);
    }

    [Fact]
    public void SetCanMoveVisibleValueDownByFeedingNegativeTension()
    {
        var carrier = new Carrier();
        carrier.Add(10);

        carrier.Set(4);

        Assert.Equal(4UL, carrier.Value);
        Assert.Equal(0, carrier.Tension);
    }

    [Fact]
    public void SetBelowOriginLeavesUnaccountedNegativeTension()
    {
        var carrier = new Carrier();
        carrier.Add(4);

        carrier.Set(-3);

        Assert.Equal(0UL, carrier.Value);
        Assert.Equal(-3, carrier.Tension);
    }

    [Fact]
    public void SetSettlesThroughAttachedStructureInsteadOfForcingValue()
    {
        var carrier = new Carrier();
        carrier.Attach(new Relation(Point(carrier, 10), Point(carrier, 0)));

        carrier.Set(23);

        Assert.Equal(3UL, carrier.Value);
        Assert.Equal(20, carrier.Tension);
    }

    [Fact]
    public void SetChangesVisibleResidueWithoutDiscardingExistingTension()
    {
        var carrier = new Carrier();
        carrier.Attach(new Relation(Point(carrier, 10), Point(carrier, 0)));
        carrier.Add(37);

        carrier.Set(8);

        Assert.Equal(8UL, carrier.Value);
        Assert.Equal(30, carrier.Tension);
    }

    private static Relation Point(Carrier carrier, long ticks) =>
        new(new CarrierLocation(carrier, ticks), carrier);
}
