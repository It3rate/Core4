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
    public void SegmentIsRelationBetweenTwoPointsOnOneCarrier()
    {
        var carrier = new Carrier(10);
        var segment = Segment(carrier, 2, 9);

        Assert.Equal(Point(carrier, 2), segment.Left);
        Assert.Equal(Point(carrier, 9), segment.Right);
        Assert.Equal(
            new[] { new CarrierLocation(carrier, 2), new CarrierLocation(carrier, 9) },
            segment.CarrierLocations());
    }

    [Fact]
    public void PointAndSegmentTransformsAreBothRelationToRelationShapes()
    {
        var lower = new Carrier(10);
        var upper = new Carrier(20);

        var pointTransform = new Relation(Point(lower, 10), Point(upper, 0));
        var segmentTransform = new Relation(Segment(lower, 0, 1), Segment(upper, 0, 1));

        Assert.IsType<Relation>(pointTransform.Left);
        Assert.IsType<Relation>(pointTransform.Right);
        Assert.IsType<Relation>(segmentTransform.Left);
        Assert.IsType<Relation>(segmentTransform.Right);
    }

    [Fact(Skip = "Target behavior: structural point transforms should carry/reset by relation shape, not external route methods.")]
    public void PointToOriginTransformCarriesAndLeavesResidue()
    {
        var lower = new Carrier(10);
        var upper = new Carrier(20);
        var transform = new Relation(Point(lower, 10), Point(upper, 0));
        var incoming = new CarrierLocation(lower, 27);

        var read = ReadThroughStructuralTransform(incoming, transform);

        Assert.Equal(
            new Relation(new CarrierLocation(upper, 2), new CarrierLocation(lower, 7)),
            read);
    }

    [Fact(Skip = "Target behavior: structural segment transforms should stretch, not clear like point transforms.")]
    public void SegmentToSegmentTransformStretchesContinuousExtent()
    {
        var lower = new Carrier(10);
        var upper = new Carrier(20);
        var transform = new Relation(Segment(lower, 0, 1), Segment(upper, 0, 1));
        var incoming = new CarrierLocation(lower, 7);

        var read = ReadThroughStructuralTransform(incoming, transform);

        Assert.Equal(Segment(upper, 0, 7), read);
    }

    [Fact(Skip = "Target behavior: repeated segment-to-segment relations should compose as power-like scaling.")]
    public void RepeatedSegmentRelationsComposeAsPowers()
    {
        var a = new Carrier(10);
        var b = new Carrier(20);
        var c = new Carrier(30);
        var first = new Relation(Segment(a, 0, 1), Segment(b, 0, 1));
        var second = new Relation(Segment(b, 0, 1), Segment(c, 0, 1));

        var firstRead = ReadThroughStructuralTransform(new CarrierLocation(a, 3), first);
        var secondRead = ReadThroughStructuralTransform(new CarrierLocation(b, 3), second);

        Assert.Equal(Segment(b, 0, 3), firstRead);
        Assert.Equal(Segment(c, 0, 9), secondRead);
    }

    private static Relation Point(Carrier carrier, ulong ticks) =>
        new(new CarrierLocation(carrier, ticks), carrier);

    private static Relation Segment(Carrier carrier, ulong start, ulong end) =>
        new(Point(carrier, start), Point(carrier, end));

    private static Relation ReadThroughStructuralTransform(CarrierLocation incoming, Relation transform)
    {
        _ = incoming;
        _ = transform;
        throw new NotImplementedException();
    }
}
