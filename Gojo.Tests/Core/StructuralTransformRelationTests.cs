using Gojo.Core;
using Gojo.Reads;

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

    [Fact]
    public void PointToOriginTransformCarriesAndLeavesResidue()
    {
        var lower = new Carrier(10);
        var upper = new Carrier(20);
        var transform = new Relation(Point(lower, 10), Point(upper, 0));
        var incoming = new CarrierLocation(lower, 27);

        var read = incoming.ReadThroughStructuralTransform(transform);

        Assert.Equal(
            new Relation(new CarrierLocation(upper, 2), new CarrierLocation(lower, 7)),
            read);
    }

    [Fact]
    public void SegmentToSegmentTransformStretchesContinuousExtent()
    {
        var lower = new Carrier(10);
        var upper = new Carrier(20);
        var transform = new Relation(Segment(lower, 0, 1), Segment(upper, 0, 1));
        var incoming = new CarrierLocation(lower, 7);

        var read = incoming.ReadThroughStructuralTransform(transform);

        Assert.Equal(Segment(upper, 0, 7), read);
    }

    [Fact]
    public void RepeatedSegmentRelationsComposeAsPowers()
    {
        var a = new Carrier(10);
        var b = new Carrier(20);
        var c = new Carrier(30);
        var first = new Relation(Segment(a, 0, 1), Segment(b, 0, 1));
        var second = new Relation(Segment(b, 0, 1), Segment(c, 0, 1));

        var reads = new CarrierLocation(a, 3).ReadThroughStructuralTransforms(first, second);

        Assert.Equal(Segment(b, 0, 3), reads[0]);
        Assert.Equal(Segment(c, 0, 9), reads[1]);
    }

    [Fact]
    public void OriginToOriginAttachmentDoesNotAccumulate()
    {
        var lower = new Carrier(10);
        var upper = new Carrier(20);
        var transform = new Relation(Point(lower, 0), Point(upper, 0));
        var incoming = new CarrierLocation(lower, 7);

        var read = incoming.ReadThroughStructuralTransform(transform);

        Assert.Equal(
            new Relation(new CarrierLocation(upper, 0), incoming),
            read);
    }

    [Fact]
    public void PointTransformsCanCarryThroughBaseTenChain()
    {
        var ones = new Carrier(10);
        var tens = new Carrier(20);
        var hundreds = new Carrier(30);
        var onesToTens = new Relation(Point(ones, 10), Point(tens, 0));
        var tensToHundreds = new Relation(Point(tens, 10), Point(hundreds, 0));

        var reads = new CarrierLocation(ones, 375).ReadThroughStructuralTransforms(onesToTens, tensToHundreds);

        Assert.Equal(
            new Relation(new CarrierLocation(tens, 37), new CarrierLocation(ones, 5)),
            reads[0]);
        Assert.Equal(
            new Relation(new CarrierLocation(hundreds, 3), new CarrierLocation(tens, 7)),
            reads[1]);
    }

    [Fact]
    public void PointTransformCanUseBinaryBoundary()
    {
        var bit = new Carrier(10);
        var next = new Carrier(20);
        var transform = new Relation(Point(bit, 2), Point(next, 0));

        var read = new CarrierLocation(bit, 13).ReadThroughStructuralTransform(transform);

        Assert.Equal(
            new Relation(new CarrierLocation(next, 6), new CarrierLocation(bit, 1)),
            read);
    }

    [Fact]
    public void SegmentTransformUsesBothSegmentLengths()
    {
        var lower = new Carrier(10);
        var upper = new Carrier(20);
        var transform = new Relation(Segment(lower, 0, 2), Segment(upper, 0, 5));

        var read = new CarrierLocation(lower, 4).ReadThroughStructuralTransform(transform);

        Assert.Equal(Segment(upper, 0, 10), read);
    }

    [Fact]
    public void StructureSettlesPointTransformInternallyAfterAddition()
    {
        var lower = new Carrier(10);
        var upper = new Carrier(20);
        var structure = Structure(
            Surface(Point(lower, 0), Point(upper, 0)),
            new Relation(Point(lower, 10), Point(upper, 0)));

        var settled = structure.AddAndSettle(new CarrierLocation(lower, 11));

        Assert.Equal(new CarrierLocation(lower, 1), settled.PositionOn(lower));
        Assert.Equal(new CarrierLocation(upper, 1), settled.PositionOn(upper));
    }

    [Fact]
    public void SettledStructureDoesNotLeaveWholeCrossingOnLowerCarrier()
    {
        var lower = new Carrier(10);
        var upper = new Carrier(20);
        var structure = Structure(
            Surface(Point(lower, 0), Point(upper, 0)),
            new Relation(Point(lower, 10), Point(upper, 0)));

        var settled = structure.AddAndSettle(new CarrierLocation(lower, 27));

        Assert.NotEqual(new CarrierLocation(lower, 27), settled.PositionOn(lower));
        Assert.Equal(new CarrierLocation(lower, 7), settled.PositionOn(lower));
        Assert.Equal(new CarrierLocation(upper, 2), settled.PositionOn(upper));
    }

    [Fact]
    public void StructureCarriesThroughInternalBaseTenRelationsUntilSettled()
    {
        var ones = new Carrier(10);
        var tens = new Carrier(20);
        var hundreds = new Carrier(30);
        var structure = Structure(
            Surface(Point(ones, 0), Point(tens, 0), Point(hundreds, 0)),
            Surface(
                new Relation(Point(ones, 10), Point(tens, 0)),
                new Relation(Point(tens, 10), Point(hundreds, 0))));

        var settled = structure.AddAndSettle(new CarrierLocation(ones, 375));

        Assert.Equal(new CarrierLocation(ones, 5), settled.PositionOn(ones));
        Assert.Equal(new CarrierLocation(tens, 7), settled.PositionOn(tens));
        Assert.Equal(new CarrierLocation(hundreds, 3), settled.PositionOn(hundreds));
    }

    [Fact]
    public void InternalCarryComposesWithExistingCarrierPositions()
    {
        var ones = new Carrier(10);
        var tens = new Carrier(20);
        var structure = Structure(
            Surface(Point(ones, 9), Point(tens, 2)),
            new Relation(Point(ones, 10), Point(tens, 0)));

        var settled = structure.AddAndSettle(new CarrierLocation(ones, 2));

        Assert.Equal(new CarrierLocation(ones, 1), settled.PositionOn(ones));
        Assert.Equal(new CarrierLocation(tens, 3), settled.PositionOn(tens));
    }

    [Fact]
    public void OriginAttachmentDoesNotMoveAddedValueIntoUpperCarrier()
    {
        var lower = new Carrier(10);
        var upper = new Carrier(20);
        var structure = Structure(
            Surface(Point(lower, 0), Point(upper, 0)),
            new Relation(Point(lower, 0), Point(upper, 0)));

        var settled = structure.AddAndSettle(new CarrierLocation(lower, 11));

        Assert.Equal(new CarrierLocation(lower, 11), settled.PositionOn(lower));
        Assert.Equal(new CarrierLocation(upper, 0), settled.PositionOn(upper));
    }

    [Fact]
    public void UnconnectedCarrierKeepsAddedValueAsLocalPosition()
    {
        var carrier = new Carrier(10);
        var structure = Structure(Point(carrier, 0), Point(Carrier.Unknown, 0));

        var settled = structure.AddAndSettle(new CarrierLocation(carrier, 11));

        Assert.Equal(new CarrierLocation(carrier, 11), settled.PositionOn(carrier));
    }

    private static Relation Point(Carrier carrier, ulong ticks) =>
        new(new CarrierLocation(carrier, ticks), carrier);

    private static Relation Segment(Carrier carrier, ulong start, ulong end) =>
        new(Point(carrier, start), Point(carrier, end));

    private static Relation Structure(IElement positions, IElement relations) =>
        new(positions, relations);

    private static IElement Surface(params IElement[] elements)
    {
        return elements.Length switch
        {
            0 => Point(Carrier.Unknown, 0),
            1 => elements[0],
            _ => new Relation(elements[0], Surface(elements[1..]))
        };
    }
}
