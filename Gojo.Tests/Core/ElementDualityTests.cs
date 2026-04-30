using Gojo.Core;

namespace Gojo.Tests.Core;

public sealed class ElementDualityTests
{
    [Fact]
    public void CarrierDualIsStableOpposedFace()
    {
        var carrier = new Carrier(10);

        var dual = carrier.Dual();

        Assert.NotSame(carrier, dual);
        Assert.True(dual.IsInverse);
        Assert.Same(carrier, dual.Dual());
    }

    [Fact]
    public void CarrierLocationDualPreservesMagnitudeOnOpposedFace()
    {
        var carrier = new Carrier(10);
        var location = new CarrierLocation(carrier, 5);

        var dual = location.Dual();

        Assert.Equal(new CarrierLocation(carrier.Dual(), 5), dual);
        Assert.Equal(-1, dual.ReadFrom(carrier).Direction);
    }

    [Fact]
    public void RelationDualRecursesThroughPointAndSegmentShape()
    {
        var carrier = new Carrier(10);
        var segment = new Relation(Point(carrier, 2), Point(carrier, 5));

        var dual = segment.Dual();

        Assert.Equal(
            new Relation(Point(carrier.Dual(), 2), Point(carrier.Dual(), 5)),
            dual);
    }

    [Fact]
    public void RelationReverseSwapsTraversalWithoutChangingFaces()
    {
        var carrier = new Carrier(10);
        var segment = new Relation(Point(carrier, 2), Point(carrier, 5));

        var reverse = segment.Reverse();

        Assert.Equal(new Relation(Point(carrier, 5), Point(carrier, 2)), reverse);
        Assert.Equal(segment, reverse.Reverse());
    }

    [Fact]
    public void DualAndReverseAreIndependentStructuralMoves()
    {
        var carrier = new Carrier(10);
        var segment = new Relation(Point(carrier, 2), Point(carrier, 5));

        Assert.Equal(segment.Dual().Reverse(), segment.Reverse().Dual());
    }

    private static Relation Point(Carrier carrier, ulong ticks) =>
        new(new CarrierLocation(carrier, ticks), carrier);
}
