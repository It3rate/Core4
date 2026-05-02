using Gojo.Core;

namespace Gojo.Tests.Core;

public sealed class StructuralPortTests
{
    [Fact]
    public void StructuralPointPortRetargetsExistingTickReference()
    {
        var input = new Carrier();
        var carrier = new Carrier();
        var tick = new TickRef(10);
        var point = Point(carrier, tick);
        var port = new Relation(Point(input, 10), point);

        Assert.True(port.TrySetStructureFrom(new CarrierLocation(input, 12)));

        Assert.Equal(new CarrierLocation(carrier, 12), point.Left);
        Assert.Equal(0UL, carrier.Value);
    }

    [Fact]
    public void StructuralSegmentPortRetargetsExistingEndpoint()
    {
        var input = new Carrier();
        var carrier = new Carrier();
        var origin = new TickRef(0);
        var extent = new TickRef(10);
        var segment = Segment(carrier, origin, extent);
        var port = new Relation(Point(input, 10), segment);

        Assert.True(port.TrySetStructureFrom(new CarrierLocation(input, 12)));

        Assert.Equal(new CarrierLocation(carrier, 0), PointLocation((Relation)segment.Left));
        Assert.Equal(new CarrierLocation(carrier, 12), PointLocation((Relation)segment.Right));
    }

    [Fact]
    public void StructuralPortMovesEveryCarrierFaceSharingTargetTickReference()
    {
        var input = new Carrier();
        var carrier = new Carrier();
        var inverse = carrier.Inverse();
        var origin = new TickRef(0);
        var extent = new TickRef(10);
        var segment = Segment(carrier, origin, extent);
        var inverseSegment = Segment(inverse, origin, extent);
        var port = new Relation(Point(input, 10), segment);

        Assert.True(port.TrySetStructureFrom(new CarrierLocation(input, 12)));

        Assert.Equal(new CarrierLocation(carrier, 12), PointLocation((Relation)segment.Right));
        Assert.Equal(new CarrierLocation(inverse, 12), PointLocation((Relation)inverseSegment.Right));
    }

    [Fact]
    public void StructuralPortPreservesProjectionResidueAsSourceTension()
    {
        var input = new Carrier();
        var carrier = new Carrier();
        var tick = new TickRef(10);
        var point = Point(carrier, tick);
        var port = new Relation(Point(input, 3), point);

        Assert.True(port.TrySetStructureFrom(new CarrierLocation(input, 1)));

        Assert.Equal(new CarrierLocation(carrier, 3), point.Left);
        Assert.Equal(new CarrierLocation(input, 1), input.TensionSlot);
    }

    [Fact]
    public void MultiSlotStructuralPortRetargetsMatchingRelationShapedInputThroughEachSurface()
    {
        var inputX = new Carrier();
        var inputY = new Carrier();
        var carrierX = new Carrier();
        var carrierY = new Carrier();
        var x = new TickRef(10);
        var y = new TickRef(20);
        var sourceSurface = new Relation(Point(inputX, 10), Point(inputY, 10));
        var targetSurface = new Relation(Point(carrierX, x), Point(carrierY, y));
        var port = new Relation(sourceSurface, targetSurface);

        Assert.True(port.TrySetStructureFrom(new Relation(
            new CarrierLocation(inputX, 12),
            new CarrierLocation(inputY, 15))));

        Assert.Equal(12, x.Ticks);
        Assert.Equal(30, y.Ticks);
    }

    [Fact]
    public void MultiSlotStructuralPortRejectsMismatchWithoutPartialRetarget()
    {
        var inputX = new Carrier();
        var inputY = new Carrier();
        var other = new Carrier();
        var carrierX = new Carrier();
        var carrierY = new Carrier();
        var x = new TickRef(10);
        var y = new TickRef(20);
        var sourceSurface = new Relation(Point(inputX, 10), Point(inputY, 10));
        var targetSurface = new Relation(Point(carrierX, x), Point(carrierY, y));
        var port = new Relation(sourceSurface, targetSurface);

        Assert.False(port.TrySetStructureFrom(new Relation(
            new CarrierLocation(inputX, 12),
            new CarrierLocation(other, 15))));

        Assert.Equal(10, x.Ticks);
        Assert.Equal(20, y.Ticks);
    }

    private static Relation Point(Carrier carrier, long ticks) =>
        new(new CarrierLocation(carrier, ticks), carrier);

    private static Relation Point(Carrier carrier, TickRef tick) =>
        new(new CarrierLocation(carrier, tick), carrier);

    private static Relation Segment(Carrier carrier, TickRef left, TickRef right) =>
        new(Point(carrier, left), Point(carrier, right));

    private static CarrierLocation PointLocation(Relation point) => (CarrierLocation)point.Left;
}
