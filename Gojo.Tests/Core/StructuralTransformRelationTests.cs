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
    public void SameCarrierSegmentDoesNotBehaveAsPointTransform()
    {
        var carrier = new Carrier(10);
        var structure = Structure(Point(carrier, 7), Segment(carrier, 2, 5));

        var settled = structure.AddAndSettle(new CarrierLocation(carrier, 0));

        Assert.Equal(new CarrierLocation(carrier, 7), settled.PositionOn(carrier));
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
    public void PointToOriginRelationSettlesCarryAndResidue()
    {
        var lower = new Carrier(10);
        var upper = new Carrier(20);
        var transform = new Relation(Point(lower, 10), Point(upper, 0));
        var structure = Structure(
            Surface(Point(lower, 0), Point(upper, 0)),
            transform);

        var settled = structure.AddAndSettle(new CarrierLocation(lower, 27));

        Assert.Equal(new CarrierLocation(lower, 7), settled.PositionOn(lower));
        Assert.Equal(new CarrierLocation(upper, 2), settled.PositionOn(upper));
    }

    [Fact]
    public void SegmentToSegmentRelationSettlesStretchedExtent()
    {
        var lower = new Carrier(10);
        var upper = new Carrier(20);
        var transform = new Relation(Segment(lower, 0, 1), Segment(upper, 0, 1));
        var structure = Structure(
            Surface(Point(lower, 0), Point(upper, 0)),
            transform);

        var settled = structure.AddAndSettle(new CarrierLocation(lower, 7));

        Assert.Equal(new CarrierLocation(lower, 7), settled.PositionOn(lower));
        Assert.Equal(new CarrierLocation(upper, 7), settled.PositionOn(upper));
    }

    [Fact]
    public void RepeatedSegmentRelationsSettleAsPowers()
    {
        var a = new Carrier(10);
        var b = new Carrier(20);
        var c = new Carrier(30);
        var first = new Relation(Segment(a, 0, 1), Segment(b, 0, 1));
        var second = new Relation(Segment(b, 0, 1), Segment(c, 0, 1));
        var structure = Structure(
            Surface(Point(a, 0), Point(b, 0), Point(c, 0)),
            Surface(first, second));

        var settled = structure.AddAndSettle(new CarrierLocation(a, 3));

        Assert.Equal(new CarrierLocation(a, 3), settled.PositionOn(a));
        Assert.Equal(new CarrierLocation(b, 3), settled.PositionOn(b));
        Assert.Equal(new CarrierLocation(c, 9), settled.PositionOn(c));
    }

    [Fact]
    public void OriginToOriginAttachmentSettlesWithoutAccumulating()
    {
        var lower = new Carrier(10);
        var upper = new Carrier(20);
        var transform = new Relation(Point(lower, 0), Point(upper, 0));
        var structure = Structure(
            Surface(Point(lower, 0), Point(upper, 0)),
            transform);

        var settled = structure.AddAndSettle(new CarrierLocation(lower, 7));

        Assert.Equal(new CarrierLocation(lower, 7), settled.PositionOn(lower));
        Assert.Equal(new CarrierLocation(upper, 0), settled.PositionOn(upper));
    }

    [Fact]
    public void PointRelationsCanCarryThroughBaseTenChain()
    {
        var ones = new Carrier(10);
        var tens = new Carrier(20);
        var hundreds = new Carrier(30);
        var onesToTens = new Relation(Point(ones, 10), Point(tens, 0));
        var tensToHundreds = new Relation(Point(tens, 10), Point(hundreds, 0));
        var structure = Structure(
            Surface(Point(ones, 0), Point(tens, 0), Point(hundreds, 0)),
            Surface(onesToTens, tensToHundreds));

        var settled = structure.AddAndSettle(new CarrierLocation(ones, 375));

        Assert.Equal(new CarrierLocation(ones, 5), settled.PositionOn(ones));
        Assert.Equal(new CarrierLocation(tens, 7), settled.PositionOn(tens));
        Assert.Equal(new CarrierLocation(hundreds, 3), settled.PositionOn(hundreds));
    }

    [Fact]
    public void PointRelationCanUseBinaryBoundary()
    {
        var bit = new Carrier(10);
        var next = new Carrier(20);
        var transform = new Relation(Point(bit, 2), Point(next, 0));
        var structure = Structure(
            Surface(Point(bit, 0), Point(next, 0)),
            transform);

        var settled = structure.AddAndSettle(new CarrierLocation(bit, 13));

        Assert.Equal(new CarrierLocation(bit, 1), settled.PositionOn(bit));
        Assert.Equal(new CarrierLocation(next, 6), settled.PositionOn(next));
    }

    [Fact]
    public void SegmentRelationSettlementUsesBothSegmentLengths()
    {
        var lower = new Carrier(10);
        var upper = new Carrier(20);
        var transform = new Relation(Segment(lower, 0, 2), Segment(upper, 0, 5));
        var structure = Structure(
            Surface(Point(lower, 0), Point(upper, 0)),
            transform);

        var settled = structure.AddAndSettle(new CarrierLocation(lower, 4));

        Assert.Equal(new CarrierLocation(lower, 4), settled.PositionOn(lower));
        Assert.Equal(new CarrierLocation(upper, 10), settled.PositionOn(upper));
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
    public void CarryLayerResidueIsModuloRead()
    {
        var ones = new Carrier(10);
        var tens = new Carrier(20);
        var transform = new Relation(Point(ones, 10), Point(tens, 0));
        var structure = Structure(
            Surface(Point(ones, 0), Point(tens, 0)),
            transform);

        var settled = structure.AddAndSettle(new CarrierLocation(ones, 37));

        Assert.Equal(new CarrierLocation(ones, 7), settled.PositionOn(ones));
        Assert.Equal(new CarrierLocation(tens, 3), settled.PositionOn(tens));
        Assert.Equal(
            Surface(
                Contribution(
                    new Relation(transform, new CarrierLocation(ones, 10)),
                    new CarrierLocation(ones, 7),
                    0),
                Contribution(
                    transform,
                    new CarrierLocation(tens, 3))),
            settled.Left);
    }

    [Fact]
    public void RepeatedAdditiveLayersCarrySameLoadWithoutScaling()
    {
        var layers = Enumerable
            .Range(0, 4)
            .Select(index => new Carrier((ulong)(100 + index)))
            .ToArray();
        var structure = Structure(
            Surface(layers.Select(layer => Point(layer, 0)).Cast<IElement>().ToArray()),
            Surface(Enumerable
                .Range(0, layers.Length - 1)
                .Select(index => new Relation(Point(layers[index], 1), Point(layers[index + 1], 0)))
                .Cast<IElement>()
                .ToArray()));

        var settled = structure.AddAndSettle(new CarrierLocation(layers[0], 15));

        Assert.Equal(new CarrierLocation(layers[0], 0), settled.PositionOn(layers[0]));
        Assert.Equal(new CarrierLocation(layers[1], 0), settled.PositionOn(layers[1]));
        Assert.Equal(new CarrierLocation(layers[2], 0), settled.PositionOn(layers[2]));
        Assert.Equal(new CarrierLocation(layers[3], 15), settled.PositionOn(layers[3]));
    }

    [Fact]
    public void PointCarryIntoExistingCarrierPositionActsAsShift()
    {
        var source = new Carrier(10);
        var shifted = new Carrier(20);
        var structure = Structure(
            Surface(Point(source, 0), Point(shifted, 5)),
            new Relation(Point(source, 1), Point(shifted, 0)));

        var settled = structure.AddAndSettle(new CarrierLocation(source, 7));

        Assert.Equal(new CarrierLocation(source, 0), settled.PositionOn(source));
        Assert.Equal(new CarrierLocation(shifted, 12), settled.PositionOn(shifted));
    }

    [Fact]
    public void EightDigitBaseTenCarryStackSettlesPositiveInteger()
    {
        var digits = Enumerable
            .Range(0, 8)
            .Select(index => new Carrier((ulong)(200 + index)))
            .ToArray();
        var structure = Structure(
            Surface(digits.Select(digit => Point(digit, 0)).Cast<IElement>().ToArray()),
            Surface(Enumerable
                .Range(0, digits.Length - 1)
                .Select(index => new Relation(Point(digits[index], 10), Point(digits[index + 1], 0)))
                .Cast<IElement>()
                .ToArray()));

        var settled = structure.AddAndSettle(new CarrierLocation(digits[0], 12_345_678));
        var expectedDigits = new ulong[] { 8, 7, 6, 5, 4, 3, 2, 1 };

        for (var index = 0; index < digits.Length; index++)
        {
            Assert.Equal(new CarrierLocation(digits[index], expectedDigits[index]), settled.PositionOn(digits[index]));
        }
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
    public void SettledContributionSurfaceCanFeedNextFrame()
    {
        var ones = new Carrier(10);
        var tens = new Carrier(20);
        var structure = Structure(
            Surface(Point(ones, 0), Point(tens, 0)),
            new Relation(Point(ones, 10), Point(tens, 0)));

        var first = structure.AddAndSettle(new CarrierLocation(ones, 7));
        var second = first.AddAndSettle(new CarrierLocation(ones, 5));

        Assert.Equal(new CarrierLocation(ones, 2), second.PositionOn(ones));
        Assert.Equal(new CarrierLocation(tens, 1), second.PositionOn(tens));
    }

    [Fact]
    public void IncomingRelationSurfaceCanCarryMultiplePointsIntoSettlement()
    {
        var ones = new Carrier(10);
        var tens = new Carrier(20);
        var structure = Structure(
            Surface(Point(ones, 0), Point(tens, 0)),
            new Relation(Point(ones, 10), Point(tens, 0)));
        var incoming = Surface(Point(ones, 7), Point(ones, 5));

        var settled = structure.AddAndSettle(incoming);

        Assert.Equal(new CarrierLocation(ones, 2), settled.PositionOn(ones));
        Assert.Equal(new CarrierLocation(tens, 1), settled.PositionOn(tens));
    }

    [Fact]
    public void IncomingRelationSurfaceCanProvideIndependentSegmentParticipation()
    {
        var lower = new Carrier(10);
        var upper = new Carrier(20);
        var transform = new Relation(Segment(lower, 0, 1), Segment(upper, 0, 1));
        var structure = Structure(
            Surface(Point(lower, 0), Point(upper, 0)),
            transform);
        var incoming = Surface(Point(lower, 7), Point(upper, 3));

        var settled = structure.AddAndSettle(incoming);

        Assert.Equal(new CarrierLocation(lower, 7), settled.PositionOn(lower));
        Assert.Equal(new CarrierLocation(upper, 21), settled.PositionOn(upper));
        Assert.Equal(
            Surface(
                Contribution(
                    Point(lower, 7),
                    new CarrierLocation(lower, 7)),
                Contribution(
                    Point(upper, 3),
                    new CarrierLocation(upper, 3)),
                Contribution(
                    transform,
                    new CarrierLocation(upper, 18),
                    7)),
            settled.Left);
    }

    [Fact]
    public void ContributionReadsLiftPlainPointsIntoVisibleContributions()
    {
        var lower = new Carrier(10);
        var upper = new Carrier(20);
        var surface = Surface(Point(lower, 2), Point(upper, 5));

        Assert.Equal(
            new[]
            {
                Contribution(Point(lower, 2), new CarrierLocation(lower, 2)),
                Contribution(Point(upper, 5), new CarrierLocation(upper, 5))
            },
            surface.Contributions());
        Assert.Equal(new CarrierLocation(lower, 2), surface.ContributionTotalOn(lower));
        Assert.Equal(new CarrierLocation(upper, 5), surface.ContributionTotalOn(upper));
    }

    [Fact]
    public void ContributionReadsKeepBareLocationsContextual()
    {
        var carrier = new Carrier(10);
        var location = new CarrierLocation(carrier, 7);
        var span = new Relation(
            new CarrierLocation(carrier, 2),
            new CarrierLocation(carrier, 5));

        Assert.Equal(
            new[]
            {
                Contribution(
                    new Relation(Point(carrier, 0), location),
                    location)
            },
            location.Contributions());
        Assert.Empty(span.Contributions());
        Assert.Equal(new CarrierLocation(carrier, 0), span.ContributionTotalOn(carrier));
    }

    [Fact]
    public void ContributionReadsExposeSettledContributionRelations()
    {
        var lower = new Carrier(10);
        var upper = new Carrier(20);
        var transform = new Relation(Segment(lower, 0, 1), Segment(upper, 0, 1));
        var structure = Structure(
            Surface(Point(lower, 0), Point(upper, 3)),
            transform);

        var settled = structure.AddAndSettle(new CarrierLocation(lower, 7));

        Assert.Equal(
            new[]
            {
                Contribution(
                    new Relation(Point(lower, 0), new CarrierLocation(lower, 7)),
                    new CarrierLocation(lower, 7)),
                Contribution(
                    Point(upper, 3),
                    new CarrierLocation(upper, 3)),
                Contribution(
                    transform,
                    new CarrierLocation(upper, 18),
                    7)
            },
            settled.Left.Contributions());
        Assert.Equal(
            new[]
            {
                Contribution(
                    Point(upper, 3),
                    new CarrierLocation(upper, 3)),
                Contribution(
                    transform,
                    new CarrierLocation(upper, 18),
                    7)
            },
            settled.Left.ContributionsOn(upper));
    }

    [Fact]
    public void ContributionReadsProjectTotalsAndScaleMarksFromVisibleStructure()
    {
        var lower = new Carrier(10);
        var upper = new Carrier(20);
        var transform = new Relation(Segment(lower, 0, 1), Segment(upper, 0, 1));
        var structure = Structure(
            Surface(Point(lower, 0), Point(upper, 3)),
            transform);

        var settled = structure.AddAndSettle(new CarrierLocation(lower, 7));

        Assert.Equal(new CarrierLocation(upper, 21), settled.Left.ContributionTotalOn(upper));
        Assert.Equal(
            new[]
            {
                new CarrierLocation(upper, 1),
                new CarrierLocation(upper, 7)
            },
            settled.Left.ContributionScalesOn(upper));
        Assert.Equal(new CarrierLocation(upper, 7), settled.Left.ContributionScaleOn(upper));
    }

    [Fact]
    public void SettledSegmentContributionSurfaceCanFeedNextFrameWithoutRestretching()
    {
        var lower = new Carrier(10);
        var upper = new Carrier(20);
        var transform = new Relation(Segment(lower, 0, 1), Segment(upper, 0, 1));
        var structure = Structure(
            Surface(Point(lower, 0), Point(upper, 3)),
            transform);

        var first = structure.AddAndSettle(new CarrierLocation(lower, 7));
        var second = first.AddAndSettle(new CarrierLocation(lower, 0));

        Assert.Equal(new CarrierLocation(lower, 7), second.PositionOn(lower));
        Assert.Equal(new CarrierLocation(upper, 21), second.PositionOn(upper));
        Assert.Equal(first.Left, second.Left);
    }

    [Fact]
    public void PointCarryCanFlowIntoOpposedCarrier()
    {
        var forward = new Carrier(10);
        var opposed = forward.Inverse();
        var structure = Structure(
            Surface(Point(forward, 0), Point(opposed, 0)),
            new Relation(Point(forward, 10), Point(opposed, 0)));

        var settled = structure.AddAndSettle(new CarrierLocation(forward, 27));

        Assert.Equal(new CarrierLocation(forward, 7), settled.PositionOn(forward));
        Assert.Equal(new CarrierLocation(opposed, 2), settled.PositionOn(opposed));
    }

    [Fact]
    public void OpposedPointCarryCanReturnThroughSecondRelation()
    {
        var forward = new Carrier(10);
        var opposed = forward.Inverse();
        var structure = Structure(
            Surface(Point(forward, 0), Point(opposed, 0)),
            Surface(
                new Relation(Point(forward, 10), Point(opposed, 0)),
                new Relation(Point(opposed, 10), Point(forward, 0))));

        var settled = structure.AddAndSettle(new CarrierLocation(forward, 120));

        Assert.Equal(new CarrierLocation(forward, 1), settled.PositionOn(forward));
        Assert.Equal(new CarrierLocation(opposed, 2), settled.PositionOn(opposed));
    }

    [Fact]
    public void LosslessPointLoopLeavesLoadVisibleWhenNoSettlementOccurs()
    {
        var first = new Carrier(10);
        var second = new Carrier(20);
        var structure = Structure(
            Surface(Point(first, 0), Point(second, 0)),
            Surface(
                new Relation(Point(first, 1), Point(second, 0)),
                new Relation(Point(second, 1), Point(first, 0))));

        var settled = structure.AddAndSettle(new CarrierLocation(first, 7));

        Assert.Equal(new CarrierLocation(first, 7), settled.PositionOn(first));
        Assert.Equal(new CarrierLocation(second, 0), settled.PositionOn(second));
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

    [Fact]
    public void StructureSettlesSegmentTransformInternallyAfterAddition()
    {
        var lower = new Carrier(10);
        var upper = new Carrier(20);
        var transform = new Relation(Segment(lower, 0, 1), Segment(upper, 0, 1));
        var structure = Structure(
            Surface(Point(lower, 0), Point(upper, 0)),
            transform);

        var settled = structure.AddAndSettle(new CarrierLocation(lower, 7));

        Assert.Equal(new CarrierLocation(lower, 7), settled.PositionOn(lower));
        Assert.Equal(new CarrierLocation(upper, 7), settled.PositionOn(upper));
        Assert.Equal(
            Surface(
                Contribution(
                    new Relation(Point(lower, 0), new CarrierLocation(lower, 7)),
                    new CarrierLocation(lower, 7)),
                Contribution(
                    transform,
                    new CarrierLocation(upper, 7),
                    7)),
            settled.Left);
    }

    [Fact]
    public void InternalSegmentTransformStretchesExistingUpperPosition()
    {
        var lower = new Carrier(10);
        var upper = new Carrier(20);
        var transform = new Relation(Segment(lower, 0, 1), Segment(upper, 0, 1));
        var structure = Structure(
            Surface(Point(lower, 0), Point(upper, 3)),
            transform);

        var settled = structure.AddAndSettle(new CarrierLocation(lower, 7));

        Assert.Equal(new CarrierLocation(lower, 7), settled.PositionOn(lower));
        Assert.Equal(new CarrierLocation(upper, 21), settled.PositionOn(upper));
        Assert.Equal(
            Surface(
                Contribution(
                    new Relation(Point(lower, 0), new CarrierLocation(lower, 7)),
                    new CarrierLocation(lower, 7)),
                Contribution(
                    Point(upper, 3),
                    new CarrierLocation(upper, 3)),
                Contribution(
                    transform,
                    new CarrierLocation(upper, 18),
                    7)),
            settled.Left);
    }

    [Fact]
    public void InternalSegmentTransformUsesBothSegmentLengths()
    {
        var lower = new Carrier(10);
        var upper = new Carrier(20);
        var structure = Structure(
            Surface(Point(lower, 0), Point(upper, 0)),
            new Relation(Segment(lower, 0, 2), Segment(upper, 0, 5)));

        var settled = structure.AddAndSettle(new CarrierLocation(lower, 4));

        Assert.Equal(new CarrierLocation(lower, 4), settled.PositionOn(lower));
        Assert.Equal(new CarrierLocation(upper, 10), settled.PositionOn(upper));
    }

    [Fact]
    public void InternalSegmentChainSettlesPowerLikePositions()
    {
        var a = new Carrier(10);
        var b = new Carrier(20);
        var c = new Carrier(30);
        var structure = Structure(
            Surface(Point(a, 0), Point(b, 0), Point(c, 0)),
            Surface(
                new Relation(Segment(a, 0, 1), Segment(b, 0, 1)),
                new Relation(Segment(b, 0, 1), Segment(c, 0, 1))));

        var settled = structure.AddAndSettle(new CarrierLocation(a, 3));

        Assert.Equal(new CarrierLocation(a, 3), settled.PositionOn(a));
        Assert.Equal(new CarrierLocation(b, 3), settled.PositionOn(b));
        Assert.Equal(new CarrierLocation(c, 9), settled.PositionOn(c));
    }

    [Fact]
    public void InternalSegmentSettlingCanFeedPointCarry()
    {
        var lower = new Carrier(10);
        var upper = new Carrier(20);
        var carried = new Carrier(30);
        var structure = Structure(
            Surface(Point(lower, 0), Point(upper, 3), Point(carried, 0)),
            Surface(
                new Relation(Segment(lower, 0, 1), Segment(upper, 0, 1)),
                new Relation(Point(upper, 10), Point(carried, 0))));

        var settled = structure.AddAndSettle(new CarrierLocation(lower, 7));

        Assert.Equal(new CarrierLocation(lower, 7), settled.PositionOn(lower));
        Assert.Equal(new CarrierLocation(upper, 1), settled.PositionOn(upper));
        Assert.Equal(new CarrierLocation(carried, 2), settled.PositionOn(carried));
    }

    [Fact]
    public void SegmentSettlingCanFeedSegmentToPointMovementInSameFrame()
    {
        var source = new Carrier(10);
        var middle = new Carrier(20);
        var target = new Carrier(30);
        var sourceSegment = Segment(source, 0, 1);
        var middleSegment = Segment(middle, 0, 1);
        var targetSegment = Segment(target, 0, 1);
        var structure = Structure(
            Surface(Point(source, 0), Point(middle, 0), Point(target, 0)),
            Surface(
                targetSegment,
                new Relation(sourceSegment, middleSegment),
                new Relation(middleSegment, Point(target, 1))));

        var settled = structure.AddAndSettle(new CarrierLocation(source, 7));

        Assert.Equal(new CarrierLocation(source, 7), settled.PositionOn(source));
        Assert.Equal(new CarrierLocation(middle, 7), settled.PositionOn(middle));
        Assert.Equal(new CarrierLocation(target, 0), settled.PositionOn(target));
        Assert.Equal(
            Surface(
                Segment(target, 0, 7),
                new Relation(sourceSegment, middleSegment),
                new Relation(middleSegment, Point(target, 7))),
            settled.Right);
    }

    [Fact]
    public void SegmentToPointTransformMovesEndpointAndPreservesSegmentTopology()
    {
        var lower = new Carrier(10);
        var upper = new Carrier(20);
        var lowerSegment = Segment(lower, 0, 1);
        var upperSegment = Segment(upper, 0, 1);
        var structure = Structure(
            Surface(Point(lower, 0), Point(upper, 0)),
            Surface(
                upperSegment,
                new Relation(lowerSegment, Point(upper, 1))));

        var settled = structure.AddAndSettle(new CarrierLocation(lower, 7));

        Assert.Equal(new CarrierLocation(lower, 7), settled.PositionOn(lower));
        Assert.Equal(new CarrierLocation(upper, 0), settled.PositionOn(upper));
        Assert.Equal(
            Surface(
                Segment(upper, 0, 7),
                new Relation(lowerSegment, Point(upper, 7))),
            settled.Right);
    }

    [Fact]
    public void SegmentToPointTransformUsesSourceSegmentLength()
    {
        var lower = new Carrier(10);
        var upper = new Carrier(20);
        var lowerSegment = Segment(lower, 0, 5);
        var structure = Structure(
            Surface(Point(lower, 0), Point(upper, 0)),
            Surface(
                Segment(upper, 0, 5),
                new Relation(lowerSegment, Point(upper, 5))));

        var settled = structure.AddAndSettle(new CarrierLocation(lower, 10));

        Assert.Equal(
            Surface(
                Segment(upper, 0, 10),
                new Relation(lowerSegment, Point(upper, 10))),
            settled.Right);
    }

    private static Relation Point(Carrier carrier, ulong ticks) =>
        new(new CarrierLocation(carrier, ticks), carrier);

    private static Relation Segment(Carrier carrier, ulong start, ulong end) =>
        new(Point(carrier, start), Point(carrier, end));

    private static Relation Contribution(IElement source, CarrierLocation location, ulong scale = 1) =>
        new(source, new Relation(location, new CarrierLocation(location.Carrier, scale)));

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
