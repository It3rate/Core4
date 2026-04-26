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
        Assert.Equal("23 4|10", $"{read.LeftLocation.Ticks} {read.RightLocation.Ticks}|{boundary.Ticks}");
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
    public void UnaccountedVisibleParticipationIsAllNewTension()
    {
        var carrier = new Carrier(10);
        var incoming = new CarrierLocation(carrier, 9);

        var read = incoming.AccountedBy(carrier);

        Assert.Equal(
            new Relation(
                new CarrierLocation(carrier, 0),
                incoming),
            read);
    }

    [Fact]
    public void ZeroAccountingSurfaceDoesNotConsumeVisibleTension()
    {
        var carrier = new Carrier(10);
        var incoming = new CarrierLocation(carrier, 9);

        var read = incoming.AccountedBy(carrier, new CarrierLocation(carrier, 0));

        Assert.Equal(
            new Relation(
                new CarrierLocation(carrier, 0),
                incoming),
            read);
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
    public void RelationCanActAsTwoAccountingSurfacesInReaderContext()
    {
        var carrier = new Carrier(10);
        var incoming = new CarrierLocation(carrier, 15);
        var routeCapacity = new Relation(
            new CarrierLocation(carrier, 7),
            new CarrierLocation(carrier, 6));

        var tension = incoming.NewTensionAfter(carrier, routeCapacity);
        var read = incoming.AccountedBy(carrier, routeCapacity);

        Assert.Equal(new CarrierLocation(carrier, 2), tension);
        Assert.Equal(
            new Relation(
                new CarrierLocation(carrier, 13),
                new CarrierLocation(carrier, 2)),
            read);
    }

    [Fact]
    public void MultipleRelationsCanActAsAccountingSurfacesInReaderContext()
    {
        var carrier = new Carrier(10);
        var incoming = new CarrierLocation(carrier, 15);
        var first = new Relation(
            new CarrierLocation(carrier, 4),
            new CarrierLocation(carrier, 3));
        var second = new Relation(
            new CarrierLocation(carrier, 2),
            new CarrierLocation(carrier, 5));

        var tension = incoming.NewTensionAfter(carrier, first, second);

        Assert.Equal(new CarrierLocation(carrier, 1), tension);
    }

    [Fact]
    public void RelationAccountingStillIgnoresIncompatibleSlots()
    {
        var x = new Carrier(10);
        var y = new Carrier(20);
        var incoming = new CarrierLocation(x, 15);
        var capacity = new Relation(
            new CarrierLocation(x, 4),
            new CarrierLocation(y, 100));

        var read = incoming.AccountedBy(x, capacity);

        Assert.Equal(
            new Relation(
                new CarrierLocation(x, 4),
                new CarrierLocation(x, 11)),
            read);
    }

    [Fact]
    public void RelationCanBeReadAsOrderedBoundariesInCarrierContext()
    {
        var carrier = new Carrier(10);
        var segment = new Relation(
            new CarrierLocation(carrier, 8),
            new CarrierLocation(carrier, 2));

        var boundaries = segment.BoundariesOn(carrier);
        var span = segment.SpanOn(carrier);

        Assert.Equal(
            new Relation(
                new CarrierLocation(carrier, 2),
                new CarrierLocation(carrier, 8)),
            boundaries);
        Assert.Equal(new CarrierLocation(carrier, 6), span);
    }

    [Fact]
    public void RelationBoundaryReadReturnsZeroWhenReaderCannotSeeBothEndpoints()
    {
        var x = new Carrier(10);
        var y = new Carrier(20);
        var segment = new Relation(
            new CarrierLocation(x, 2),
            new CarrierLocation(y, 8));

        Assert.Equal(
            new Relation(
                new CarrierLocation(x, 0),
                new CarrierLocation(x, 0)),
            segment.BoundariesOn(x));
        Assert.Equal(new CarrierLocation(x, 0), segment.SpanOn(x));
    }

    [Fact]
    public void RelationBoundaryReadPreservesDuplicateCarrierLineage()
    {
        var source = new Carrier(10);
        var duplicate = source.Duplicate(11);
        var segment = new Relation(
            new CarrierLocation(source, 2),
            new CarrierLocation(duplicate, 8));

        var boundaries = segment.BoundariesOn(duplicate);

        Assert.Equal(
            new Relation(
                new CarrierLocation(duplicate, 2),
                new CarrierLocation(duplicate, 8)),
            boundaries);
    }

    [Fact]
    public void RelationBoundaryReadUsesOpposedCarrierWhenReadSeesOpposedEndpoints()
    {
        var forward = new Carrier(10);
        var inverse = forward.Inverse();
        var segment = new Relation(
            new CarrierLocation(inverse, 8),
            new CarrierLocation(inverse, 2));

        var boundaries = segment.BoundariesOn(forward);
        var span = segment.SpanOn(forward);

        Assert.Equal(
            new Relation(
                new CarrierLocation(inverse, 2),
                new CarrierLocation(inverse, 8)),
            boundaries);
        Assert.Equal(new CarrierLocation(inverse, 6), span);
    }

    [Fact]
    public void RelationFlowReadPreservesTravelDirectionAsCarrierParticipation()
    {
        var carrier = new Carrier(10);

        var forward = new Relation(
            new CarrierLocation(carrier, 2),
            new CarrierLocation(carrier, 8));
        var reverse = new Relation(
            new CarrierLocation(carrier, 8),
            new CarrierLocation(carrier, 2));

        Assert.Equal(new CarrierLocation(carrier, 6), forward.FlowOn(carrier));
        Assert.Equal(new CarrierLocation(carrier.Inverse(), 6), reverse.FlowOn(carrier));
    }

    [Fact]
    public void RelationFlowReadTreatsIncreasingOpposedCarrierAsReverseFlow()
    {
        var forward = new Carrier(10);
        var inverse = forward.Inverse();
        var segment = new Relation(
            new CarrierLocation(inverse, 2),
            new CarrierLocation(inverse, 8));

        Assert.Equal(new CarrierLocation(inverse, 6), segment.FlowOn(forward));
    }

    [Fact]
    public void RelationBoundaryPointsSplitWhereCarrierParticipationCanChange()
    {
        var carrier = new Carrier(10);
        var outer = new Relation(
            new CarrierLocation(carrier, 0),
            new CarrierLocation(carrier, 8));
        var inner = new Relation(
            new CarrierLocation(carrier, 2),
            new CarrierLocation(carrier, 5));

        var points = Relation.BoundaryPointsOn(carrier, outer, inner);

        Assert.Equal(
            new[]
            {
                new CarrierLocation(carrier, 0),
                new CarrierLocation(carrier, 2),
                new CarrierLocation(carrier, 5),
                new CarrierLocation(carrier, 8)
            },
            points);
    }

    [Fact]
    public void RelationBoundaryPointsIgnoreInvisibleAndZeroSpans()
    {
        var x = new Carrier(10);
        var y = new Carrier(20);
        var visible = new Relation(
            new CarrierLocation(x, 2),
            new CarrierLocation(x, 5));
        var invisible = new Relation(
            new CarrierLocation(x, 7),
            new CarrierLocation(y, 10));
        var zero = new Relation(
            new CarrierLocation(x, 3),
            new CarrierLocation(x, 3));

        var points = Relation.BoundaryPointsOn(x, visible, invisible, zero);

        Assert.Equal(
            new[]
            {
                new CarrierLocation(x, 2),
                new CarrierLocation(x, 5)
            },
            points);
    }

    [Fact]
    public void RelationBoundaryPointsPreserveOpposedCarrierSurface()
    {
        var forward = new Carrier(10);
        var inverse = forward.Inverse();
        var segment = new Relation(
            new CarrierLocation(inverse, 0),
            new CarrierLocation(inverse, 8));

        var points = Relation.BoundaryPointsOn(forward, segment);

        Assert.Equal(
            new[]
            {
                new CarrierLocation(inverse, 0),
                new CarrierLocation(inverse, 8)
            },
            points);
    }

    [Fact]
    public void RelationCoveredSpansSplitWhereParticipationCanChange()
    {
        var carrier = new Carrier(10);
        var outer = new Relation(
            new CarrierLocation(carrier, 0),
            new CarrierLocation(carrier, 8));
        var inner = new Relation(
            new CarrierLocation(carrier, 2),
            new CarrierLocation(carrier, 5));

        var spans = Relation.CoveredSpansOn(carrier, outer, inner);

        Assert.Equal(
            new[]
            {
                new Relation(new CarrierLocation(carrier, 0), new CarrierLocation(carrier, 2)),
                new Relation(new CarrierLocation(carrier, 2), new CarrierLocation(carrier, 5)),
                new Relation(new CarrierLocation(carrier, 5), new CarrierLocation(carrier, 8))
            },
            spans);
    }

    [Fact]
    public void RelationCoveredSpansPreserveOpposedCarrierSurface()
    {
        var forward = new Carrier(10);
        var inverse = forward.Inverse();
        var segment = new Relation(
            new CarrierLocation(inverse, 0),
            new CarrierLocation(inverse, 8));

        var spans = Relation.CoveredSpansOn(forward, segment);

        Assert.Equal(
            new[]
            {
                new Relation(new CarrierLocation(inverse, 0), new CarrierLocation(inverse, 8))
            },
            spans);
    }

    [Fact]
    public void RelationCoveredSpansSkipUncoveredGaps()
    {
        var carrier = new Carrier(10);
        var first = new Relation(
            new CarrierLocation(carrier, 0),
            new CarrierLocation(carrier, 2));
        var second = new Relation(
            new CarrierLocation(carrier, 5),
            new CarrierLocation(carrier, 8));

        var spans = Relation.CoveredSpansOn(carrier, first, second);

        Assert.Equal(
            new[]
            {
                new Relation(new CarrierLocation(carrier, 0), new CarrierLocation(carrier, 2)),
                new Relation(new CarrierLocation(carrier, 5), new CarrierLocation(carrier, 8))
            },
            spans);
    }

    [Fact]
    public void RelationCoveredSpansIgnoreInvisibleSegments()
    {
        var x = new Carrier(10);
        var y = new Carrier(20);
        var invisible = new Relation(
            new CarrierLocation(y, 0),
            new CarrierLocation(y, 8));

        Assert.Empty(Relation.CoveredSpansOn(x, invisible));
    }

    [Fact]
    public void RelationSupportStatesReadReverseAndForwardParticipationBySpan()
    {
        var carrier = new Carrier(10);
        var forward = new Relation(
            new CarrierLocation(carrier, 0),
            new CarrierLocation(carrier, 8));
        var reverse = new Relation(
            new CarrierLocation(carrier, 2),
            new CarrierLocation(carrier, 0));

        var states = Relation.SupportStatesOn(carrier, forward, reverse);

        Assert.Equal(
            new[]
            {
                new Relation(
                    new Relation(new CarrierLocation(carrier, 0), new CarrierLocation(carrier, 2)),
                    new Relation(new CarrierLocation(carrier.Inverse(), 1), new CarrierLocation(carrier, 1))),
                new Relation(
                    new Relation(new CarrierLocation(carrier, 2), new CarrierLocation(carrier, 8)),
                    new Relation(new CarrierLocation(carrier.Inverse(), 0), new CarrierLocation(carrier, 1)))
            },
            states);
    }

    [Fact]
    public void RelationSupportStatesSkipUncoveredGaps()
    {
        var carrier = new Carrier(10);
        var first = new Relation(
            new CarrierLocation(carrier, 0),
            new CarrierLocation(carrier, 2));
        var second = new Relation(
            new CarrierLocation(carrier, 5),
            new CarrierLocation(carrier, 8));

        var states = Relation.SupportStatesOn(carrier, first, second);

        Assert.Equal(
            new[]
            {
                new Relation(
                    new Relation(new CarrierLocation(carrier, 0), new CarrierLocation(carrier, 2)),
                    new Relation(new CarrierLocation(carrier.Inverse(), 0), new CarrierLocation(carrier, 1))),
                new Relation(
                    new Relation(new CarrierLocation(carrier, 5), new CarrierLocation(carrier, 8)),
                    new Relation(new CarrierLocation(carrier.Inverse(), 0), new CarrierLocation(carrier, 1)))
            },
            states);
    }

    [Fact]
    public void RelationSupportStatesUseOpposedSlotForOpposedCarrierFlow()
    {
        var forward = new Carrier(10);
        var inverse = forward.Inverse();
        var opposed = new Relation(
            new CarrierLocation(inverse, 0),
            new CarrierLocation(inverse, 8));

        var states = Relation.SupportStatesOn(forward, opposed);

        Assert.Equal(
            new[]
            {
                new Relation(
                    new Relation(new CarrierLocation(inverse, 0), new CarrierLocation(inverse, 8)),
                    new Relation(new CarrierLocation(inverse, 1), new CarrierLocation(forward, 0)))
            },
            states);
    }

    [Fact]
    public void RelationSupportStatesMergeMultipleCompatibleParticipations()
    {
        var carrier = new Carrier(10);
        var firstForward = new Relation(
            new CarrierLocation(carrier, 0),
            new CarrierLocation(carrier, 8));
        var secondForward = new Relation(
            new CarrierLocation(carrier, 1),
            new CarrierLocation(carrier, 9));
        var reverse = new Relation(
            new CarrierLocation(carrier, 10),
            new CarrierLocation(carrier, 2));

        var states = Relation.SupportStatesOn(carrier, firstForward, secondForward, reverse);

        Assert.Contains(
            new Relation(
                new Relation(new CarrierLocation(carrier, 2), new CarrierLocation(carrier, 8)),
                new Relation(new CarrierLocation(carrier.Inverse(), 1), new CarrierLocation(carrier, 2))),
            states);
    }

    [Fact]
    public void RelationSupportStatesCanBeScopedToCarrierInterval()
    {
        var carrier = new Carrier(10);
        var scope = new Relation(
            new CarrierLocation(carrier, 2),
            new CarrierLocation(carrier, 6));
        var forward = new Relation(
            new CarrierLocation(carrier, 0),
            new CarrierLocation(carrier, 8));
        var reverse = new Relation(
            new CarrierLocation(carrier, 10),
            new CarrierLocation(carrier, 4));

        var states = Relation.SupportStatesWithinOn(carrier, scope, forward, reverse);

        Assert.Equal(
            new[]
            {
                new Relation(
                    new Relation(new CarrierLocation(carrier, 2), new CarrierLocation(carrier, 4)),
                    new Relation(new CarrierLocation(carrier.Inverse(), 0), new CarrierLocation(carrier, 1))),
                new Relation(
                    new Relation(new CarrierLocation(carrier, 4), new CarrierLocation(carrier, 6)),
                    new Relation(new CarrierLocation(carrier.Inverse(), 1), new CarrierLocation(carrier, 1)))
            },
            states);
    }

    [Fact]
    public void RelationScopedSupportProjectionsPreserveOnlyScopedParticipation()
    {
        var carrier = new Carrier(10);
        var scope = new Relation(
            new CarrierLocation(carrier, 2),
            new CarrierLocation(carrier, 6));
        var forward = new Relation(
            new CarrierLocation(carrier, 0),
            new CarrierLocation(carrier, 8));
        var reverse = new Relation(
            new CarrierLocation(carrier, 10),
            new CarrierLocation(carrier, 4));

        var support = Relation.SupportWithinOn(carrier, scope, forward, reverse);
        var tension = Relation.TensionWithinOn(carrier, scope, forward, reverse);
        var exclusive = Relation.ExclusiveWithinOn(carrier, scope, forward, reverse);

        Assert.Equal(
            new[]
            {
                new Relation(
                    new Relation(new CarrierLocation(carrier, 2), new CarrierLocation(carrier, 4)),
                    new Relation(new CarrierLocation(carrier.Inverse(), 0), new CarrierLocation(carrier, 1))),
                new Relation(
                    new Relation(new CarrierLocation(carrier, 4), new CarrierLocation(carrier, 6)),
                    new Relation(new CarrierLocation(carrier.Inverse(), 1), new CarrierLocation(carrier, 1)))
            },
            support);
        Assert.Equal(
            new[]
            {
                new Relation(
                    new Relation(new CarrierLocation(carrier, 4), new CarrierLocation(carrier, 6)),
                    new Relation(new CarrierLocation(carrier.Inverse(), 1), new CarrierLocation(carrier, 1)))
            },
            tension);
        Assert.Equal(
            new[]
            {
                new Relation(
                    new Relation(new CarrierLocation(carrier, 2), new CarrierLocation(carrier, 4)),
                    new Relation(new CarrierLocation(carrier.Inverse(), 0), new CarrierLocation(carrier, 1)))
            },
            exclusive);
    }

    [Fact]
    public void RelationSupportProjectionPreservesReverseAndForwardSlots()
    {
        var carrier = new Carrier(10);
        var state = new Relation(
            new CarrierLocation(carrier.Inverse(), 2),
            new CarrierLocation(carrier, 3));

        Assert.Equal(
            new Relation(
                new CarrierLocation(carrier.Inverse(), 2),
                new CarrierLocation(carrier, 3)),
            state.SupportOn(carrier));
    }

    [Fact]
    public void RelationTensionProjectionReadsSharedReverseAndForwardParticipation()
    {
        var carrier = new Carrier(10);
        var state = new Relation(
            new CarrierLocation(carrier.Inverse(), 2),
            new CarrierLocation(carrier, 3));

        Assert.Equal(
            new Relation(
                new CarrierLocation(carrier.Inverse(), 2),
                new CarrierLocation(carrier, 2)),
            state.TensionOn(carrier));
    }

    [Fact]
    public void RelationExclusiveProjectionKeepsOnlyLargerFlowSide()
    {
        var carrier = new Carrier(10);
        var forwardLarger = new Relation(
            new CarrierLocation(carrier.Inverse(), 2),
            new CarrierLocation(carrier, 5));
        var reverseLarger = new Relation(
            new CarrierLocation(carrier.Inverse(), 5),
            new CarrierLocation(carrier, 2));

        Assert.Equal(
            new Relation(
                new CarrierLocation(carrier.Inverse(), 0),
                new CarrierLocation(carrier, 3)),
            forwardLarger.ExclusiveOn(carrier));
        Assert.Equal(
            new Relation(
                new CarrierLocation(carrier.Inverse(), 3),
                new CarrierLocation(carrier, 0)),
            reverseLarger.ExclusiveOn(carrier));
    }

    [Fact]
    public void RelationSupportProjectionsCanReadNestedSupportState()
    {
        var carrier = new Carrier(10);
        var state = new Relation(
            new Relation(new CarrierLocation(carrier, 0), new CarrierLocation(carrier, 2)),
            new Relation(new CarrierLocation(carrier.Inverse(), 1), new CarrierLocation(carrier, 1)));

        Assert.Equal(
            new Relation(
                new CarrierLocation(carrier.Inverse(), 1),
                new CarrierLocation(carrier, 1)),
            state.SupportOn(carrier));
        Assert.Equal(
            new Relation(
                new CarrierLocation(carrier.Inverse(), 1),
                new CarrierLocation(carrier, 1)),
            state.TensionOn(carrier));
        Assert.Equal(
            new Relation(
                new CarrierLocation(carrier.Inverse(), 0),
                new CarrierLocation(carrier, 0)),
            state.ExclusiveOn(carrier));
    }

    [Fact]
    public void RelationSupportSpanProjectionPreservesIntervalsWithAnyParticipation()
    {
        var carrier = new Carrier(10);
        var forward = new Relation(
            new CarrierLocation(carrier, 0),
            new CarrierLocation(carrier, 8));
        var reverse = new Relation(
            new CarrierLocation(carrier, 2),
            new CarrierLocation(carrier, 0));

        var support = Relation.SupportOn(carrier, forward, reverse);

        Assert.Equal(
            new[]
            {
                new Relation(
                    new Relation(new CarrierLocation(carrier, 0), new CarrierLocation(carrier, 2)),
                    new Relation(new CarrierLocation(carrier.Inverse(), 1), new CarrierLocation(carrier, 1))),
                new Relation(
                    new Relation(new CarrierLocation(carrier, 2), new CarrierLocation(carrier, 8)),
                    new Relation(new CarrierLocation(carrier.Inverse(), 0), new CarrierLocation(carrier, 1)))
            },
            support);
    }

    [Fact]
    public void RelationTensionSpanProjectionPreservesOnlyIntervalsWithSharedOpposedParticipation()
    {
        var carrier = new Carrier(10);
        var forward = new Relation(
            new CarrierLocation(carrier, 0),
            new CarrierLocation(carrier, 8));
        var reverse = new Relation(
            new CarrierLocation(carrier, 2),
            new CarrierLocation(carrier, 0));

        var tension = Relation.TensionOn(carrier, forward, reverse);

        Assert.Equal(
            new[]
            {
                new Relation(
                    new Relation(new CarrierLocation(carrier, 0), new CarrierLocation(carrier, 2)),
                    new Relation(new CarrierLocation(carrier.Inverse(), 1), new CarrierLocation(carrier, 1)))
            },
            tension);
    }

    [Fact]
    public void RelationExclusiveSpanProjectionPreservesOnlyUnopposedIntervals()
    {
        var carrier = new Carrier(10);
        var forward = new Relation(
            new CarrierLocation(carrier, 0),
            new CarrierLocation(carrier, 8));
        var reverse = new Relation(
            new CarrierLocation(carrier, 2),
            new CarrierLocation(carrier, 0));

        var exclusive = Relation.ExclusiveOn(carrier, forward, reverse);

        Assert.Equal(
            new[]
            {
                new Relation(
                    new Relation(new CarrierLocation(carrier, 2), new CarrierLocation(carrier, 8)),
                    new Relation(new CarrierLocation(carrier.Inverse(), 0), new CarrierLocation(carrier, 1)))
            },
            exclusive);
    }

    [Fact]
    public void RelationTensionAndExclusiveSpanProjectionsReturnNoZeroStates()
    {
        var carrier = new Carrier(10);
        var forward = new Relation(
            new CarrierLocation(carrier, 0),
            new CarrierLocation(carrier, 8));

        Assert.Empty(Relation.TensionOn(carrier, forward));
        Assert.Equal(Relation.SupportOn(carrier, forward), Relation.ExclusiveOn(carrier, forward));
    }

    [Fact]
    public void BooleanStyleSpanReadsAreSupportProjections()
    {
        var carrier = new Carrier(10);
        var forward = new Relation(
            new CarrierLocation(carrier, 0),
            new CarrierLocation(carrier, 8));
        var reverse = new Relation(
            new CarrierLocation(carrier, 2),
            new CarrierLocation(carrier, 0));

        Assert.Equal(Relation.SupportOn(carrier, forward, reverse), Relation.OrOn(carrier, forward, reverse));
        Assert.Equal(Relation.TensionOn(carrier, forward, reverse), Relation.AndOn(carrier, forward, reverse));
        Assert.Equal(Relation.ExclusiveOn(carrier, forward, reverse), Relation.XorOn(carrier, forward, reverse));
    }

    [Fact]
    public void RelationUncoveredSpansReadScopedAbsenceBetweenCoveredIntervals()
    {
        var carrier = new Carrier(10);
        var scope = new Relation(
            new CarrierLocation(carrier, 0),
            new CarrierLocation(carrier, 10));
        var blocked = new Relation(
            new CarrierLocation(carrier, 2),
            new CarrierLocation(carrier, 5));

        var uncovered = Relation.UncoveredSpansOn(carrier, scope, blocked);

        Assert.Equal(
            new[]
            {
                new Relation(new CarrierLocation(carrier, 0), new CarrierLocation(carrier, 2)),
                new Relation(new CarrierLocation(carrier, 5), new CarrierLocation(carrier, 10))
            },
            uncovered);
    }

    [Fact]
    public void RelationUncoveredSpansReturnEmptyWhenScopeIsFullyCovered()
    {
        var carrier = new Carrier(10);
        var scope = new Relation(
            new CarrierLocation(carrier, 2),
            new CarrierLocation(carrier, 8));
        var blocked = new Relation(
            new CarrierLocation(carrier, 0),
            new CarrierLocation(carrier, 10));

        var uncovered = Relation.UncoveredSpansOn(carrier, scope, blocked);

        Assert.Empty(uncovered);
    }

    [Fact]
    public void RelationUncoveredSpansIgnoreInvisibleCoverage()
    {
        var carrier = new Carrier(10);
        var other = new Carrier(20);
        var scope = new Relation(
            new CarrierLocation(carrier, 0),
            new CarrierLocation(carrier, 10));
        var invisible = new Relation(
            new CarrierLocation(other, 2),
            new CarrierLocation(other, 5));

        var uncovered = Relation.UncoveredSpansOn(carrier, scope, invisible);

        Assert.Equal(
            new[]
            {
                scope
            },
            uncovered);
    }

    [Fact]
    public void RelationUncoveredSpansClipCoverageToScope()
    {
        var carrier = new Carrier(10);
        var scope = new Relation(
            new CarrierLocation(carrier, 2),
            new CarrierLocation(carrier, 8));
        var leftBlocked = new Relation(
            new CarrierLocation(carrier, 0),
            new CarrierLocation(carrier, 5));
        var rightBlocked = new Relation(
            new CarrierLocation(carrier, 7),
            new CarrierLocation(carrier, 10));

        var uncovered = Relation.UncoveredSpansOn(carrier, scope, leftBlocked, rightBlocked);

        Assert.Equal(
            new[]
            {
                new Relation(new CarrierLocation(carrier, 5), new CarrierLocation(carrier, 7))
            },
            uncovered);
    }

    [Fact]
    public void RelationCoveredSpansCanReadCoverageInsideScope()
    {
        var carrier = new Carrier(10);
        var scope = new Relation(
            new CarrierLocation(carrier, 2),
            new CarrierLocation(carrier, 8));
        var leftSupported = new Relation(
            new CarrierLocation(carrier, 0),
            new CarrierLocation(carrier, 5));
        var rightSupported = new Relation(
            new CarrierLocation(carrier, 7),
            new CarrierLocation(carrier, 10));

        var covered = Relation.CoveredSpansWithinOn(carrier, scope, leftSupported, rightSupported);

        Assert.Equal(
            new[]
            {
                new Relation(new CarrierLocation(carrier, 2), new CarrierLocation(carrier, 5)),
                new Relation(new CarrierLocation(carrier, 7), new CarrierLocation(carrier, 8))
            },
            covered);
    }

    [Fact]
    public void RelationPassAndBlockAreScopedFilterProjections()
    {
        var carrier = new Carrier(10);
        var scope = new Relation(
            new CarrierLocation(carrier, 0),
            new CarrierLocation(carrier, 10));
        var supported = new Relation(
            new CarrierLocation(carrier, 2),
            new CarrierLocation(carrier, 5));

        Assert.Equal(
            Relation.CoveredSpansWithinOn(carrier, scope, supported),
            Relation.PassOn(carrier, scope, supported));
        Assert.Equal(
            Relation.UncoveredSpansOn(carrier, scope, supported),
            Relation.BlockOn(carrier, scope, supported));
    }

    [Fact]
    public void RelationNotIsScopedAbsenceProjection()
    {
        var carrier = new Carrier(10);
        var scope = new Relation(
            new CarrierLocation(carrier, 0),
            new CarrierLocation(carrier, 10));
        var blocked = new Relation(
            new CarrierLocation(carrier, 2),
            new CarrierLocation(carrier, 5));

        Assert.Equal(
            Relation.UncoveredSpansOn(carrier, scope, blocked),
            Relation.NotOn(carrier, scope, blocked));
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
