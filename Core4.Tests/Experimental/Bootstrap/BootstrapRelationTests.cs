using BootstrapMeasure = Core4.Experimental.Bootstrap.Measure;
using BootstrapRelationFold = Core4.Experimental.Bootstrap.RelationFold;
using BootstrapRelation = Core4.Experimental.Bootstrap.Relation;
using BootstrapCarrier = Core4.Experimental.Bootstrap.Carrier;
using BootstrapPartitionStep = Core4.Experimental.Bootstrap.PartitionStep;
using BootstrapSupportProjection = Core4.Experimental.Bootstrap.SupportProjection;
using BootstrapSupportSpanUtilities = Core4.Experimental.Bootstrap.SupportSpanUtilities;
using BootstrapDirectedSupportSpan = Core4.Experimental.Bootstrap.DirectedSupportSpan;
using BootstrapSupportStateSpan = Core4.Experimental.Bootstrap.SupportStateSpan;
using BootstrapSupportReadEquations = Core4.Experimental.Bootstrap.SupportReadEquations;

namespace Core4.Tests.Experimental.Bootstrap;

public sealed class BootstrapRelationTests
{
    [Fact]
    public void RawMeasuresFoldToUnitFlowPair()
    {
        BootstrapMeasure.Offset = 0;
        try
        {
            var start = new BootstrapMeasure(10);
            var end = new BootstrapMeasure(14);
            var fold = new BootstrapRelationFold(start, end);

            Assert.Equal((4L, 1L), Read(fold));

            BootstrapMeasure.Offset = 100;

            Assert.Equal((4L, 1L), Read(fold));
        }
        finally
        {
            BootstrapMeasure.Offset = 0;
        }
    }

    [Fact]
    public void CoincidentRawMeasuresFoldToOpenFraction()
    {
        var point = new BootstrapMeasure(10);

        Assert.Equal((0L, 0L), Read(new BootstrapRelationFold(point, point)));
    }

    [Fact]
    public void RawMeasureLoopPreservesDirectionAcrossAllTicks()
    {
        var dominantStart = new BootstrapMeasure(10);
        var dominantEnd = new BootstrapMeasure(20);
        var recessiveStart = new BootstrapMeasure(20);
        var recessiveEnd = new BootstrapMeasure(10);

        Assert.Equal((10L, 1L), Read(new BootstrapRelationFold(dominantStart, dominantEnd)));
        Assert.Equal((-10L, -1L), Read(new BootstrapRelationFold(recessiveStart, recessiveEnd)));
    }

    [Fact]
    public void ZeroPartitionPointClaimsAppendWithoutMixing()
    {
        var open = BootstrapRelation.Open();
        var leftPoint = new BootstrapRelation(new BootstrapMeasure(8), open);
        var rightPoint = new BootstrapRelation(new BootstrapMeasure(12), open);

        Assert.Equal((8L, 0L), Fold(leftPoint));
        Assert.Equal((12L, 0L), Fold(rightPoint));
        Assert.Equal((20L, 0L), Read(new BootstrapRelationFold(leftPoint, rightPoint)));
    }

    [Fact]
    public void PointClaimCanBeResolvedThroughSharedUnit()
    {
        var open = BootstrapRelation.Open();
        var point = new BootstrapRelation(new BootstrapMeasure(8), open);
        var unit = new BootstrapRelation(new BootstrapMeasure(10), new BootstrapMeasure(14));

        Assert.Equal((8L, 0L), Fold(point));
        Assert.Equal((4L, 1L), Fold(unit));
        Assert.Equal((8L, 4L), Read(new BootstrapRelationFold(point, unit)));
    }

    [Fact]
    public void SharedBasisSmearingProducesMultiplicationLikeResult()
    {
        var open = BootstrapRelation.Open();
        var unit = new BootstrapRelation(new BootstrapMeasure(10), new BootstrapMeasure(14));
        var two = new BootstrapRelation(new BootstrapRelation(new BootstrapMeasure(8), open), unit);
        var three = new BootstrapRelation(new BootstrapRelation(new BootstrapMeasure(12), open), unit);

        Assert.Equal((8L, 4L), Fold(two));
        Assert.Equal((12L, 4L), Fold(three));
        Assert.Equal((24L, 4L), Read(new BootstrapRelationFold(two, three)));
    }

    [Fact]
    public void PartitionIteratorUsesNaturalFlowAndPlacesPartialWidthFirst()
    {
        var open = BootstrapRelation.Open();
        var unit = new BootstrapRelation(new BootstrapMeasure(10), new BootstrapMeasure(14));
        var value = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(26), open), unit);
        var partitions = value.Partitions().ToArray();

        Assert.Equal(7, value.PartitionCount);
        Assert.Equal(
            new[]
            {
                new BootstrapPartitionStep(0, 2),
                new BootstrapPartitionStep(2, 4),
                new BootstrapPartitionStep(6, 4),
                new BootstrapPartitionStep(10, 4),
                new BootstrapPartitionStep(14, 4),
                new BootstrapPartitionStep(18, 4),
                new BootstrapPartitionStep(22, 4),
            },
            partitions);
    }

    [Fact]
    public void PositiveUnitStartsAtLeftmostSideOfNegativeRun()
    {
        var open = BootstrapRelation.Open();
        var unit = new BootstrapRelation(new BootstrapMeasure(10), new BootstrapMeasure(14));
        var value = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(-10), open), unit);

        Assert.Equal(
            new[]
            {
                new BootstrapPartitionStep(-10, 2),
                new BootstrapPartitionStep(-8, 4),
                new BootstrapPartitionStep(-4, 4),
            },
            value.Partitions().ToArray());
    }

    [Fact]
    public void NegativeUnitStartsAtRightmostSideAndFlowsLeft()
    {
        var open = BootstrapRelation.Open();
        var inverseUnit = new BootstrapRelation(new BootstrapMeasure(14), new BootstrapMeasure(10));
        var value = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(10), open), inverseUnit);

        Assert.Equal(
            new[]
            {
                new BootstrapPartitionStep(10, -2),
                new BootstrapPartitionStep(8, -4),
                new BootstrapPartitionStep(4, -4),
            },
            value.Partitions().ToArray());
    }

    [Fact]
    public void OpenSiteCountIsDerivedFromStructure()
    {
        var open = BootstrapRelation.Open();
        var unit = new BootstrapRelation(new BootstrapMeasure(10), new BootstrapMeasure(14));
        var nested = new BootstrapRelation(new BootstrapRelation(open, unit), new BootstrapRelation(open, open));

        Assert.Equal(1, BootstrapRelation.OpenSiteCount(open));
        Assert.Equal(0, BootstrapRelation.OpenSiteCount(unit));
        Assert.Equal(3, BootstrapRelation.OpenSiteCount(nested));
    }

    [Fact]
    public void ApplyFillsTheFirstOpenSiteInsteadOfCreatingPeerComposition()
    {
        var open = BootstrapRelation.Open();
        var unit = new BootstrapRelation(new BootstrapMeasure(10), new BootstrapMeasure(14));
        var transform = new BootstrapRelation(open, unit);
        var applied = BootstrapRelation.Apply(transform, new BootstrapMeasure(8));

        var appliedRelation = Assert.IsType<BootstrapRelation>(applied);
        Assert.Equal((8L, 4L), Fold(appliedRelation));
        Assert.Equal(0, BootstrapRelation.OpenSiteCount(appliedRelation));
        Assert.Equal(8, Assert.IsType<BootstrapMeasure>(appliedRelation.Left).Raw);
    }

    [Fact]
    public void ComposeAlsoAutoDetectsAndFillsAnOpenSite()
    {
        var open = BootstrapRelation.Open();
        var unit = new BootstrapRelation(new BootstrapMeasure(10), new BootstrapMeasure(14));
        var transform = new BootstrapRelation(open, unit);
        var composed = BootstrapRelation.Compose(transform, new BootstrapMeasure(8));

        var composedRelation = Assert.IsType<BootstrapRelation>(composed);
        Assert.Equal((8L, 4L), Fold(composedRelation));
        Assert.Equal(0, BootstrapRelation.OpenSiteCount(composedRelation));
    }

    [Fact]
    public void ApplyFallsBackToPeerCompositionWhenNoOpenSiteExists()
    {
        var left = new BootstrapMeasure(10);
        var right = new BootstrapMeasure(14);
        var applied = BootstrapRelation.Apply(left, right);

        var relation = Assert.IsType<BootstrapRelation>(applied);
        Assert.Same(left, relation.Left);
        Assert.Same(right, relation.Right);
        Assert.Equal((4L, 1L), Fold(relation));
    }

    [Fact]
    public void ApplyCanFillNestedGradeTwoOpenStructure()
    {
        var open = BootstrapRelation.Open();
        var unit = new BootstrapRelation(new BootstrapMeasure(10), new BootstrapMeasure(14));
        var transform = new BootstrapRelation(new BootstrapRelation(open, unit), unit);
        var applied = BootstrapRelation.Apply(transform, new BootstrapMeasure(8));

        var outer = Assert.IsType<BootstrapRelation>(applied);
        var inner = Assert.IsType<BootstrapRelation>(outer.Left);

        Assert.Equal(0, BootstrapRelation.OpenSiteCount(applied));
        Assert.Equal((32L, 4L), Fold(outer));
        Assert.Equal(8, Assert.IsType<BootstrapMeasure>(inner.Left).Raw);
        Assert.Same(unit, outer.Right);
    }

    [Fact]
    public void ReusedNestedRelationsStillFoldByReadingTheirCurrentLeaves()
    {
        BootstrapMeasure.Offset = 0;
        try
        {
            var start = new BootstrapMeasure(10);
            var end = new BootstrapMeasure(14);
            var unit = new BootstrapRelation(start, end);
            var reused = new BootstrapRelationFold(unit, unit);

            Assert.Equal((16L, 1L), Read(reused));

            BootstrapMeasure.Offset = 50;

            var reusedLeft = Assert.IsType<BootstrapRelation>(reused.Left);
            var reusedRight = Assert.IsType<BootstrapRelation>(reused.Right);

            Assert.Same(reusedLeft, reusedRight);
            Assert.Equal((4L, 1L), Fold(reusedLeft));
            Assert.Equal((16L, 1L), Read(reused));
            Assert.Equal(60, Assert.IsType<BootstrapMeasure>(reusedLeft.Left).Raw);
            Assert.Equal(64, Assert.IsType<BootstrapMeasure>(reusedLeft.Right).Raw);
        }
        finally
        {
            BootstrapMeasure.Offset = 0;
        }
    }

    [Fact]
    public void NestedRelationsCanStillBeStoredWithoutAnyGradeFlags()
    {
        var left = new BootstrapRelation(new BootstrapMeasure(10), new BootstrapMeasure(14));
        var right = new BootstrapRelation(new BootstrapMeasure(20), new BootstrapMeasure(24));
        var parent = new BootstrapRelation(left, right);

        Assert.Same(left, parent.Left);
        Assert.Same(right, parent.Right);
    }

    [Fact]
    public void FoldResultIsItselfAnElementWithOrderedPairSurface()
    {
        var fold = new BootstrapRelationFold(new BootstrapMeasure(10), new BootstrapMeasure(14));

        Assert.IsAssignableFrom<Core4.Experimental.Bootstrap.IElement>(fold);
        Assert.Equal((4L, 1L), Read(fold));
        Assert.Equal((4L, 0L), ReadElement(fold.First));
        Assert.Equal((1L, 0L), ReadElement(fold.Second));
        var reduced = Assert.IsType<BootstrapRelation>(fold.Reduced);
        Assert.Equal(4, Assert.IsType<BootstrapMeasure>(reduced.Left).Raw);
        Assert.Equal(1, Assert.IsType<BootstrapMeasure>(reduced.Right).Raw);
    }

    [Fact]
    public void FoldKeepsOpenStructureAsReducedInsteadOfFlatteningItAway()
    {
        var open = BootstrapRelation.Open();
        var unit = new BootstrapRelation(new BootstrapMeasure(10), new BootstrapMeasure(14));
        var fold = new BootstrapRelationFold(open, unit);

        var reduced = Assert.IsType<BootstrapRelation>(fold.Reduced);
        var reducedLeft = Assert.IsType<BootstrapRelationFold>(reduced.Left);
        var reducedRight = Assert.IsType<BootstrapRelationFold>(reduced.Right);

        Assert.Equal(1, BootstrapRelation.OpenSiteCount(reduced));
        Assert.Equal((0L, 0L), Read(reducedLeft));
        Assert.Equal((4L, 1L), Read(reducedRight));
        Assert.Equal((0L, 4L), Read(fold));
    }

    [Fact]
    public void OpenTransformTraversesByItsClosedBasisInsteadOfItsProjectedZeroMagnitude()
    {
        var open = BootstrapRelation.Open();
        var unit = new BootstrapRelation(new BootstrapMeasure(10), new BootstrapMeasure(14));
        var transform = new BootstrapRelation(open, unit);
        var fold = new BootstrapRelationFold(open, unit);

        Assert.Equal(
            new[]
            {
                new BootstrapPartitionStep(0, 1),
                new BootstrapPartitionStep(1, 1),
                new BootstrapPartitionStep(2, 1),
                new BootstrapPartitionStep(3, 1),
            },
            transform.Partitions().ToArray());

        Assert.Equal(
            new[]
            {
                new BootstrapPartitionStep(0, 1),
                new BootstrapPartitionStep(1, 1),
                new BootstrapPartitionStep(2, 1),
                new BootstrapPartitionStep(3, 1),
            },
            fold.Partitions().ToArray());
    }

    [Fact]
    public void PartialApplicationBehavesLikeANewTransformWithFewerOpenSites()
    {
        var open = BootstrapRelation.Open();
        var unit = new BootstrapRelation(new BootstrapMeasure(10), new BootstrapMeasure(14));
        var transform = new BootstrapRelation(new BootstrapRelation(open, unit), open);
        var partiallyApplied = BootstrapRelation.Apply(transform, new BootstrapMeasure(8));
        var outer = Assert.IsType<BootstrapRelation>(partiallyApplied);
        var fold = new BootstrapRelationFold(outer.Left, outer.Right);

        var reduced = Assert.IsType<BootstrapRelation>(fold.Reduced);
        var reducedLeft = Assert.IsType<BootstrapRelationFold>(reduced.Left);

        Assert.Equal(1, BootstrapRelation.OpenSiteCount(reduced));
        Assert.Equal(1, BootstrapRelation.OpenSiteCount(partiallyApplied));
        Assert.Equal((8L, 4L), Read(reducedLeft));
    }

    [Fact]
    public void PartiallyAppliedTransformTraversesByItsClosedStructuralSide()
    {
        var open = BootstrapRelation.Open();
        var unit = new BootstrapRelation(new BootstrapMeasure(10), new BootstrapMeasure(14));
        var transform = new BootstrapRelation(new BootstrapRelation(open, unit), open);
        var partiallyApplied = BootstrapRelation.Apply(transform, new BootstrapMeasure(8));

        Assert.Equal(
            new[]
            {
                new BootstrapPartitionStep(0, 4),
                new BootstrapPartitionStep(4, 4),
            },
            partiallyApplied.Partitions().ToArray());
    }

    [Fact]
    public void ClosedGradeTwoFoldKeepsReducedChildStructure()
    {
        var open = BootstrapRelation.Open();
        var unit = new BootstrapRelation(new BootstrapMeasure(10), new BootstrapMeasure(14));
        var two = new BootstrapRelation(new BootstrapRelation(new BootstrapMeasure(8), open), unit);
        var three = new BootstrapRelation(new BootstrapRelation(new BootstrapMeasure(12), open), unit);
        var fold = new BootstrapRelationFold(two, three);

        var reduced = Assert.IsType<BootstrapRelation>(fold.Reduced);
        var reducedLeft = Assert.IsType<BootstrapRelationFold>(reduced.Left);
        var reducedRight = Assert.IsType<BootstrapRelationFold>(reduced.Right);

        Assert.Equal((24L, 4L), Read(fold));
        Assert.Equal(0, BootstrapRelation.OpenSiteCount(reduced));
        Assert.Equal((8L, 4L), Read(reducedLeft));
        Assert.Equal((12L, 4L), Read(reducedRight));
    }

    [Fact]
    public void ClosedGradeTwoFoldTraversesByItsReducedChildStructure()
    {
        var open = BootstrapRelation.Open();
        var unit = new BootstrapRelation(new BootstrapMeasure(10), new BootstrapMeasure(14));
        var two = new BootstrapRelation(new BootstrapRelation(new BootstrapMeasure(8), open), unit);
        var three = new BootstrapRelation(new BootstrapRelation(new BootstrapMeasure(12), open), unit);
        var fold = new BootstrapRelationFold(two, three);
        var reduced = Assert.IsType<BootstrapRelation>(fold.Reduced);

        var expected = new[]
        {
            new BootstrapPartitionStep(0, 4),
            new BootstrapPartitionStep(4, 4),
            new BootstrapPartitionStep(0, 4),
            new BootstrapPartitionStep(4, 4),
            new BootstrapPartitionStep(8, 4),
        };

        Assert.Equal(5, fold.PartitionCount);
        Assert.Equal(expected, fold.Partitions().ToArray());
        Assert.Equal(expected, reduced.Partitions().ToArray());
    }

    [Fact]
    public void ClosedGradeThreeFoldKeepsNestedReducedStructure()
    {
        var open = BootstrapRelation.Open();
        var unit = new BootstrapRelation(new BootstrapMeasure(10), new BootstrapMeasure(14));
        var two = new BootstrapRelation(new BootstrapRelation(new BootstrapMeasure(8), open), unit);
        var three = new BootstrapRelation(new BootstrapRelation(new BootstrapMeasure(12), open), unit);
        var four = new BootstrapRelation(new BootstrapRelation(new BootstrapMeasure(16), open), unit);
        var six = new BootstrapRelationFold(two, three);
        var twentyFour = new BootstrapRelationFold(six, four);

        var reduced = Assert.IsType<BootstrapRelation>(twentyFour.Reduced);
        var reducedLeft = Assert.IsType<BootstrapRelationFold>(reduced.Left);
        var reducedRight = Assert.IsType<BootstrapRelationFold>(reduced.Right);
        var reducedLeftStructure = Assert.IsType<BootstrapRelation>(reducedLeft.Reduced);

        Assert.Equal((96L, 4L), Read(twentyFour));
        Assert.Equal((24L, 4L), Read(reducedLeft));
        Assert.Equal((16L, 4L), Read(reducedRight));
        Assert.Equal(0, BootstrapRelation.OpenSiteCount(reduced));
        Assert.IsType<BootstrapRelationFold>(reducedLeftStructure.Left);
        Assert.IsType<BootstrapRelationFold>(reducedLeftStructure.Right);
    }

    [Fact]
    public void ClosedGradeThreeFoldTraversesByAllNestedResolvedChildren()
    {
        var open = BootstrapRelation.Open();
        var unit = new BootstrapRelation(new BootstrapMeasure(10), new BootstrapMeasure(14));
        var two = new BootstrapRelation(new BootstrapRelation(new BootstrapMeasure(8), open), unit);
        var three = new BootstrapRelation(new BootstrapRelation(new BootstrapMeasure(12), open), unit);
        var four = new BootstrapRelation(new BootstrapRelation(new BootstrapMeasure(16), open), unit);
        var six = new BootstrapRelationFold(two, three);
        var twentyFour = new BootstrapRelationFold(six, four);
        var reduced = Assert.IsType<BootstrapRelation>(twentyFour.Reduced);

        var expected = new[]
        {
            new BootstrapPartitionStep(0, 4),
            new BootstrapPartitionStep(4, 4),
            new BootstrapPartitionStep(0, 4),
            new BootstrapPartitionStep(4, 4),
            new BootstrapPartitionStep(8, 4),
            new BootstrapPartitionStep(0, 4),
            new BootstrapPartitionStep(4, 4),
            new BootstrapPartitionStep(8, 4),
            new BootstrapPartitionStep(12, 4),
        };

        Assert.Equal(9, twentyFour.PartitionCount);
        Assert.Equal(expected, twentyFour.Partitions().ToArray());
        Assert.Equal(expected, reduced.Partitions().ToArray());
    }

    [Fact]
    public void HigherGradePartialApplicationKeepsOneOpenSiteBeforeTopLevelFoldClosesIt()
    {
        var open = BootstrapRelation.Open();
        var unit = new BootstrapRelation(new BootstrapMeasure(10), new BootstrapMeasure(14));
        var transform = new BootstrapRelation(
            new BootstrapRelation(new BootstrapRelation(open, unit), unit),
            new BootstrapRelation(new BootstrapRelation(open, unit), unit));

        var partiallyApplied = BootstrapRelation.Apply(transform, new BootstrapMeasure(8));
        var outer = Assert.IsType<BootstrapRelation>(partiallyApplied);
        var left = Assert.IsType<BootstrapRelation>(outer.Left);
        var fold = new BootstrapRelationFold(outer.Left, outer.Right);
        var reduced = Assert.IsType<BootstrapRelation>(fold.Reduced);
        var partitions = partiallyApplied.Partitions().ToArray();

        Assert.Equal(1, BootstrapRelation.OpenSiteCount(partiallyApplied));
        Assert.Equal(0, BootstrapRelation.OpenSiteCount(reduced));
        Assert.Equal((32L, 4L), Fold(left));
        Assert.True(partitions.Length >= 8);
        Assert.All(partitions, partition => Assert.NotEqual(0, partition.Width));
        Assert.Contains(new BootstrapPartitionStep(0, 1), partitions);
        Assert.Contains(new BootstrapPartitionStep(0, 4), partitions);
    }

    [Fact]
    public void MeasureCanCarryBootstrapCarrierIdentity()
    {
        var horizontal = new BootstrapCarrier(10);
        var vertical = new BootstrapCarrier(20);
        var x = new BootstrapMeasure(8, horizontal);
        var y = new BootstrapMeasure(8, vertical);

        Assert.Equal(horizontal, x.Carrier);
        Assert.Equal(vertical, y.Carrier);
        Assert.NotEqual(x.Carrier, y.Carrier);
    }

    [Fact]
    public void CarrierCanBeCrystallizedFromStructureAndRecoverItsSource()
    {
        var miles = new BootstrapCarrier(10);
        var hours = new BootstrapCarrier(20);
        var speedShape = new BootstrapRelation(
            new BootstrapMeasure(60, miles),
            new BootstrapMeasure(2, hours));

        var speed = BootstrapCarrier.Crystallize(speedShape);

        Assert.NotEqual(miles, speed);
        Assert.NotEqual(hours, speed);
        Assert.Equal(miles.Id, speed.Domain);
        Assert.True(BootstrapCarrier.TryGetSource(speed, out var source));
        Assert.Same(speedShape, source);
    }

    [Fact]
    public void CrystallizedCarrierCanBeUsedAsItsOwnStartingCarrier()
    {
        var miles = new BootstrapCarrier(10);
        var hours = new BootstrapCarrier(20);
        var speedShape = new BootstrapRelation(
            new BootstrapMeasure(60, miles),
            new BootstrapMeasure(2, hours));
        var speed = BootstrapCarrier.Crystallize(speedShape);
        var speedReading = new BootstrapMeasure(30, speed);

        Assert.Equal(speed, speedReading.Carrier);
        Assert.True(BootstrapCarrier.TryGetSource(speedReading.Carrier, out var source));
        Assert.Same(speedShape, source);
    }

    [Fact]
    public void CrystallizedCarrierKeepsFirstValueVariableAndRestFrozen()
    {
        var miles = new BootstrapCarrier(10);
        var hours = new BootstrapCarrier(20);
        var speedShape = new BootstrapRelation(
            new BootstrapMeasure(60, miles),
            new BootstrapRelation(
                new BootstrapMeasure(2, hours),
                new BootstrapMeasure(1, hours)));
        var speed = BootstrapCarrier.Crystallize(speedShape);

        Assert.True(BootstrapCarrier.TryGetVariableSource(speed, out var variable));
        Assert.True(BootstrapCarrier.TryGetFrozenSource(speed, out var frozen));
        Assert.Same(speedShape.Left, variable);
        Assert.Same(speedShape.Right, frozen);
    }

    [Fact]
    public void SingleValueCrystallizationHasVariableButNoFrozenSurface()
    {
        var speed = new BootstrapMeasure(30, new BootstrapCarrier(10));
        var crystallized = BootstrapCarrier.Crystallize(speed);

        Assert.True(BootstrapCarrier.TryGetVariableSource(crystallized, out var variable));
        Assert.False(BootstrapCarrier.TryGetFrozenSource(crystallized, out _));
        Assert.Same(speed, variable);
    }

    [Fact]
    public void CrystallizedCarrierExposesTickSurfaceByCarrierReference()
    {
        var miles = new BootstrapCarrier(10);
        var hours = new BootstrapCarrier(20);
        var speedShape = new BootstrapRelation(
            new BootstrapMeasure(60, miles),
            new BootstrapMeasure(2, hours));
        var speed = BootstrapCarrier.Crystallize(speedShape);

        Assert.True(BootstrapCarrier.TryGetTickSurface(speed, out var surface));
        Assert.Equal(
            new[]
            {
                new Core4.Experimental.Bootstrap.TickSurfaceEntry(miles, 1),
                new Core4.Experimental.Bootstrap.TickSurfaceEntry(hours, 1),
            },
            surface);
    }

    [Fact]
    public void AutomaticCrystallizationPreservesRepeatedCarrierOccurrences()
    {
        var meters = new BootstrapCarrier(10);
        var seconds = new BootstrapCarrier(20);
        var accelerationShape = new BootstrapRelation(
            new BootstrapMeasure(8, meters),
            new BootstrapRelation(
                new BootstrapMeasure(2, seconds),
                new BootstrapMeasure(3, seconds)));
        var acceleration = BootstrapCarrier.Crystallize(accelerationShape);

        Assert.True(BootstrapCarrier.TryGetTickSurface(acceleration, out var surface));
        Assert.Equal(
            new[]
            {
                new Core4.Experimental.Bootstrap.TickSurfaceEntry(meters, 1),
                new Core4.Experimental.Bootstrap.TickSurfaceEntry(seconds, 1),
                new Core4.Experimental.Bootstrap.TickSurfaceEntry(seconds, 1),
            },
            surface);
    }

    [Fact]
    public void CrystallizedCarrierCanCarryExplicitSignedTickSurface()
    {
        var miles = new BootstrapCarrier(10);
        var hours = new BootstrapCarrier(20);
        var speedShape = new BootstrapRelation(
            new BootstrapMeasure(60, miles),
            new BootstrapMeasure(2, hours));
        var speed = BootstrapCarrier.Crystallize(
            speedShape,
            new Core4.Experimental.Bootstrap.TickSurfaceEntry(miles, 1),
            new Core4.Experimental.Bootstrap.TickSurfaceEntry(hours, -1));

        Assert.True(BootstrapCarrier.TryGetTickSurface(speed, out var surface));
        Assert.Equal(
            new[]
            {
                new Core4.Experimental.Bootstrap.TickSurfaceEntry(miles, 1),
                new Core4.Experimental.Bootstrap.TickSurfaceEntry(hours, -1),
            },
            surface);
    }

    [Fact]
    public void ExplicitTickSurfaceCanExpressRepeatedContractiveParticipation()
    {
        var meters = new BootstrapCarrier(10);
        var seconds = new BootstrapCarrier(20);
        var accelerationShape = new BootstrapRelation(
            new BootstrapMeasure(8, meters),
            new BootstrapRelation(
                new BootstrapMeasure(2, seconds),
                new BootstrapMeasure(3, seconds)));
        var acceleration = BootstrapCarrier.Crystallize(
            accelerationShape,
            new Core4.Experimental.Bootstrap.TickSurfaceEntry(meters, 1),
            new Core4.Experimental.Bootstrap.TickSurfaceEntry(seconds, -2));

        Assert.True(BootstrapCarrier.TryGetTickSurface(acceleration, out var surface));
        Assert.Equal(
            new[]
            {
                new Core4.Experimental.Bootstrap.TickSurfaceEntry(meters, 1),
                new Core4.Experimental.Bootstrap.TickSurfaceEntry(seconds, -2),
            },
            surface);
    }

    [Fact]
    public void ExplicitTickSurfaceCanKeepOpposedSameCarrierParticipationsSeparate()
    {
        var meters = new BootstrapCarrier(10);
        var seconds = new BootstrapCarrier(20);
        var mixedShape = new BootstrapRelation(
            new BootstrapMeasure(8, meters),
            new BootstrapRelation(
                new BootstrapMeasure(2, seconds),
                new BootstrapMeasure(4, seconds)));
        var mixed = BootstrapCarrier.Crystallize(
            mixedShape,
            new Core4.Experimental.Bootstrap.TickSurfaceEntry(meters, 1),
            new Core4.Experimental.Bootstrap.TickSurfaceEntry(seconds, 1),
            new Core4.Experimental.Bootstrap.TickSurfaceEntry(seconds, -1));

        Assert.True(BootstrapCarrier.TryGetTickSurface(mixed, out var surface));
        Assert.Equal(
            new[]
            {
                new Core4.Experimental.Bootstrap.TickSurfaceEntry(meters, 1),
                new Core4.Experimental.Bootstrap.TickSurfaceEntry(seconds, 1),
                new Core4.Experimental.Bootstrap.TickSurfaceEntry(seconds, -1),
            },
            surface);
    }

    [Fact]
    public void ExplicitCrystallizationKeepsSignedVariableAndFrozenSurfaces()
    {
        var miles = new BootstrapCarrier(10);
        var hours = new BootstrapCarrier(20);
        var distance = new BootstrapMeasure(60, miles);
        var time = new BootstrapMeasure(2, hours);
        var speed = BootstrapCarrier.Crystallize(
            new BootstrapRelation(distance, time),
            distance,
            time,
            new Core4.Experimental.Bootstrap.TickSurfaceEntry(miles, 1),
            new Core4.Experimental.Bootstrap.TickSurfaceEntry(hours, -1));

        Assert.True(BootstrapCarrier.TryGetVariableTickSurface(speed, out var variableSurface));
        Assert.True(BootstrapCarrier.TryGetFrozenTickSurface(speed, out var frozenSurface));
        Assert.Equal(
            new[]
            {
                new Core4.Experimental.Bootstrap.TickSurfaceEntry(miles, 1),
            },
            variableSurface);
        Assert.Equal(
            new[]
            {
                new Core4.Experimental.Bootstrap.TickSurfaceEntry(hours, -1),
            },
            frozenSurface);
    }

    [Fact]
    public void ExplicitCrystallizationKeepsRepeatedContractiveFrozenSurface()
    {
        var meters = new BootstrapCarrier(10);
        var seconds = new BootstrapCarrier(20);
        var distance = new BootstrapMeasure(8, meters);
        var timeBasis = new BootstrapRelation(
            new BootstrapMeasure(2, seconds),
            new BootstrapMeasure(3, seconds));
        var acceleration = BootstrapCarrier.Crystallize(
            new BootstrapRelation(distance, timeBasis),
            distance,
            timeBasis,
            new Core4.Experimental.Bootstrap.TickSurfaceEntry(meters, 1),
            new Core4.Experimental.Bootstrap.TickSurfaceEntry(seconds, -2));

        Assert.True(BootstrapCarrier.TryGetFrozenTickSurface(acceleration, out var frozenSurface));
        Assert.Equal(
            new[]
            {
                new Core4.Experimental.Bootstrap.TickSurfaceEntry(seconds, -2),
            },
            frozenSurface);
    }

    [Fact]
    public void ExplicitCrystallizationCanKeepWholeSourceVariable()
    {
        var kilowatts = new BootstrapCarrier(10);
        var hours = new BootstrapCarrier(20);
        var energyShape = new BootstrapRelation(
            new BootstrapMeasure(30, kilowatts),
            new BootstrapMeasure(2, hours));
        var energy = BootstrapCarrier.Crystallize(
            energyShape,
            energyShape,
            null,
            new Core4.Experimental.Bootstrap.TickSurfaceEntry(kilowatts, 1),
            new Core4.Experimental.Bootstrap.TickSurfaceEntry(hours, 1));

        Assert.True(BootstrapCarrier.TryGetVariableSource(energy, out var variable));
        Assert.False(BootstrapCarrier.TryGetFrozenSource(energy, out _));
        Assert.True(BootstrapCarrier.TryGetVariableTickSurface(energy, out var variableSurface));
        Assert.Same(energyShape, variable);
        Assert.Equal(
            new[]
            {
                new Core4.Experimental.Bootstrap.TickSurfaceEntry(kilowatts, 1),
                new Core4.Experimental.Bootstrap.TickSurfaceEntry(hours, 1),
            },
            variableSurface);
    }

    [Fact]
    public void CanonicalProjectorCanProjectCompatibleQuotientInput()
    {
        var miles = new BootstrapCarrier(10);
        var hours = new BootstrapCarrier(20);
        var speedShape = new BootstrapRelation(
            new BootstrapMeasure(60, miles),
            new BootstrapMeasure(2, hours));
        var speed = BootstrapCarrier.Crystallize(
            speedShape,
            new Core4.Experimental.Bootstrap.TickSurfaceEntry(miles, 1),
            new Core4.Experimental.Bootstrap.TickSurfaceEntry(hours, -1));
        var input = new BootstrapRelation(
            new BootstrapMeasure(60, miles),
            new BootstrapMeasure(2, hours));

        Assert.True(BootstrapCarrier.TryProject(speed, input, out var projected));
        Assert.Equal(speed, projected.Carrier);
        Assert.Equal(30, projected.Raw);
    }

    [Fact]
    public void CanonicalProjectorCanProjectCompatibleProductInput()
    {
        var pounds = new BootstrapCarrier(10);
        var feet = new BootstrapCarrier(20);
        var footPounds = BootstrapCarrier.Crystallize(
            new BootstrapRelation(new BootstrapMeasure(1, pounds), new BootstrapMeasure(1, feet)),
            new Core4.Experimental.Bootstrap.TickSurfaceEntry(pounds, 1),
            new Core4.Experimental.Bootstrap.TickSurfaceEntry(feet, 1));
        var input = new BootstrapRelation(
            new BootstrapMeasure(30, pounds),
            new BootstrapMeasure(2, feet));

        Assert.True(BootstrapCarrier.TryProject(footPounds, input, out var projected));
        Assert.Equal(footPounds, projected.Carrier);
        Assert.Equal(60, projected.Raw);
    }

    [Fact]
    public void CanonicalProjectorCanProjectRepeatedContractiveInput()
    {
        var meters = new BootstrapCarrier(10);
        var seconds = new BootstrapCarrier(20);
        var acceleration = BootstrapCarrier.Crystallize(
            new BootstrapRelation(
                new BootstrapMeasure(1, meters),
                new BootstrapRelation(new BootstrapMeasure(1, seconds), new BootstrapMeasure(1, seconds))),
            new Core4.Experimental.Bootstrap.TickSurfaceEntry(meters, 1),
            new Core4.Experimental.Bootstrap.TickSurfaceEntry(seconds, -2));
        var input = new BootstrapRelation(
            new BootstrapMeasure(8, meters),
            new BootstrapRelation(new BootstrapMeasure(2, seconds), new BootstrapMeasure(4, seconds)));

        Assert.True(BootstrapCarrier.TryProject(acceleration, input, out var projected));
        Assert.Equal(acceleration, projected.Carrier);
        Assert.Equal(1, projected.Raw);
    }

    [Fact]
    public void CanonicalProjectorCanProjectMixedSameCarrierSurfaceInOrder()
    {
        var meters = new BootstrapCarrier(10);
        var seconds = new BootstrapCarrier(20);
        var mixed = BootstrapCarrier.Crystallize(
            new BootstrapRelation(
                new BootstrapMeasure(1, meters),
                new BootstrapRelation(new BootstrapMeasure(1, seconds), new BootstrapMeasure(1, seconds))),
            new Core4.Experimental.Bootstrap.TickSurfaceEntry(meters, 1),
            new Core4.Experimental.Bootstrap.TickSurfaceEntry(seconds, 1),
            new Core4.Experimental.Bootstrap.TickSurfaceEntry(seconds, -1));
        var input = new BootstrapRelation(
            new BootstrapMeasure(8, meters),
            new BootstrapRelation(new BootstrapMeasure(2, seconds), new BootstrapMeasure(4, seconds)));

        Assert.True(BootstrapCarrier.TryProject(mixed, input, out var projected));
        Assert.Equal(mixed, projected.Carrier);
        Assert.Equal(4, projected.Raw);
    }

    [Fact]
    public void CanonicalProjectorRejectsMismatchedSurface()
    {
        var miles = new BootstrapCarrier(10);
        var hours = new BootstrapCarrier(20);
        var speed = BootstrapCarrier.Crystallize(
            new BootstrapRelation(new BootstrapMeasure(1, miles), new BootstrapMeasure(1, hours)),
            new Core4.Experimental.Bootstrap.TickSurfaceEntry(miles, 1),
            new Core4.Experimental.Bootstrap.TickSurfaceEntry(hours, -1));
        var incompatible = new BootstrapRelation(
            new BootstrapMeasure(60, miles),
            new BootstrapMeasure(5, new BootstrapCarrier(30)));

        Assert.False(BootstrapCarrier.TryProject(speed, incompatible, out _));
    }

    [Fact]
    public void ClosedDirectedSegmentLikeFoldPreservesAllFourBranchContributions()
    {
        var carrier = new BootstrapCarrier(10);
        var open = BootstrapRelation.Open();
        var expansiveUnit = new BootstrapRelation(new BootstrapMeasure(10, carrier), new BootstrapMeasure(14, carrier));
        var contractiveUnit = new BootstrapRelation(new BootstrapMeasure(14, carrier), new BootstrapMeasure(10, carrier));

        var a = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(8, carrier), open), expansiveUnit);
        var b = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(12, carrier), open), contractiveUnit);
        var c = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(16, carrier), open), expansiveUnit);
        var d = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(20, carrier), open), contractiveUnit);
        var left = new BootstrapRelation(a, b);
        var right = new BootstrapRelation(c, d);
        var fold = new BootstrapRelationFold(left, right);

        var reduced = Assert.IsType<BootstrapRelation>(fold.Reduced);
        var aligned = Assert.IsType<BootstrapRelation>(reduced.Left);
        var orthogonal = Assert.IsType<BootstrapRelation>(reduced.Right);

        Assert.Equal((32L, 4L), Read(Assert.IsType<BootstrapRelationFold>(aligned.Left)));
        Assert.Equal((60L, -4L), Read(Assert.IsType<BootstrapRelationFold>(aligned.Right)));
        Assert.Equal((-40L, 4L), Read(Assert.IsType<BootstrapRelationFold>(orthogonal.Left)));
        Assert.Equal((-48L, -4L), Read(Assert.IsType<BootstrapRelationFold>(orthogonal.Right)));
    }

    [Fact]
    public void ClosedDirectedSegmentLikeFoldCanCollapseRepeatedlyBackThroughLowerGrades()
    {
        var carrier = new BootstrapCarrier(10);
        var open = BootstrapRelation.Open();
        var expansiveUnit = new BootstrapRelation(new BootstrapMeasure(10, carrier), new BootstrapMeasure(14, carrier));
        var contractiveUnit = new BootstrapRelation(new BootstrapMeasure(14, carrier), new BootstrapMeasure(10, carrier));

        var a = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(8, carrier), open), expansiveUnit);
        var b = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(12, carrier), open), contractiveUnit);
        var c = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(16, carrier), open), expansiveUnit);
        var d = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(20, carrier), open), contractiveUnit);
        var fold = new BootstrapRelationFold(new BootstrapRelation(a, b), new BootstrapRelation(c, d));

        var collapsed = Assert.IsType<BootstrapRelation>(fold.Collapsed);
        var aligned = Assert.IsType<BootstrapRelation>(collapsed.Left);
        var orthogonal = Assert.IsType<BootstrapRelation>(collapsed.Right);
        Assert.Equal((24L, 4L), ReadElement(aligned.Left));
        Assert.Equal((-32L, 4L), ReadElement(aligned.Right));
        Assert.Equal((-12L, 4L), ReadElement(orthogonal.Left));
        Assert.Equal((4L, 4L), ReadElement(orthogonal.Right));
        Assert.Equal((-16L, 4L), ReadElement(fold.Projected));
    }

    [Fact]
    public void ClosedOrthogonalLikeFoldPreservesAlignedAndCrossCarrierContributions()
    {
        var horizontal = new BootstrapCarrier(10);
        var vertical = new BootstrapCarrier(20);
        var open = BootstrapRelation.Open();
        var horizontalUnit = new BootstrapRelation(new BootstrapMeasure(10, horizontal), new BootstrapMeasure(14, horizontal));
        var verticalUnit = new BootstrapRelation(new BootstrapMeasure(30, vertical), new BootstrapMeasure(34, vertical));

        var a = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(8, horizontal), open), horizontalUnit);
        var b = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(12, vertical), open), verticalUnit);
        var c = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(16, horizontal), open), horizontalUnit);
        var d = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(20, vertical), open), verticalUnit);
        var left = new BootstrapRelation(a, b);
        var right = new BootstrapRelation(c, d);
        var fold = new BootstrapRelationFold(left, right);

        var reduced = Assert.IsType<BootstrapRelation>(fold.Reduced);
        var aligned = Assert.IsType<BootstrapRelation>(reduced.Left);
        var orthogonal = Assert.IsType<BootstrapRelation>(reduced.Right);

        Assert.Equal((32L, 4L), Read(Assert.IsType<BootstrapRelationFold>(aligned.Left)));
        Assert.Equal((60L, 4L), Read(Assert.IsType<BootstrapRelationFold>(aligned.Right)));
        Assert.Equal((40L, 4L), Read(Assert.IsType<BootstrapRelationFold>(orthogonal.Left)));
        Assert.Equal((48L, 4L), Read(Assert.IsType<BootstrapRelationFold>(orthogonal.Right)));
    }

    [Fact]
    public void ClosedOrthogonalLikeFoldCanCollapseRepeatedlyBackThroughLowerGrades()
    {
        var horizontal = new BootstrapCarrier(10);
        var vertical = new BootstrapCarrier(20);
        var open = BootstrapRelation.Open();
        var horizontalUnit = new BootstrapRelation(new BootstrapMeasure(10, horizontal), new BootstrapMeasure(14, horizontal));
        var verticalUnit = new BootstrapRelation(new BootstrapMeasure(30, vertical), new BootstrapMeasure(34, vertical));

        var a = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(8, horizontal), open), horizontalUnit);
        var b = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(12, vertical), open), verticalUnit);
        var c = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(16, horizontal), open), horizontalUnit);
        var d = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(20, vertical), open), verticalUnit);
        var fold = new BootstrapRelationFold(new BootstrapRelation(a, b), new BootstrapRelation(c, d));

        var collapsed = Assert.IsType<BootstrapRelation>(fold.Collapsed);
        var aligned = Assert.IsType<BootstrapRelation>(collapsed.Left);
        var orthogonal = Assert.IsType<BootstrapRelation>(collapsed.Right);
        Assert.Equal((24L, 4L), ReadElement(aligned.Left));
        Assert.Equal((32L, 4L), ReadElement(aligned.Right));
        Assert.Equal((28L, 4L), ReadElement(orthogonal.Left));
        Assert.Equal((28L, 4L), ReadElement(orthogonal.Right));
        Assert.Equal((112L, 4L), ReadElement(fold.Projected));
    }

    [Fact]
    public void SupportProjectionCanReadSupportTensionAndResidueFromDirectedSegmentLikePair()
    {
        var carrier = new BootstrapCarrier(10);
        var open = BootstrapRelation.Open();
        var expansiveUnit = new BootstrapRelation(new BootstrapMeasure(10, carrier), new BootstrapMeasure(14, carrier));
        var contractiveUnit = new BootstrapRelation(new BootstrapMeasure(14, carrier), new BootstrapMeasure(10, carrier));
        var forward = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(8, carrier), open), expansiveUnit);
        var reverse = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(2, carrier), open), contractiveUnit);
        var segment = new BootstrapRelation(forward, reverse);

        Assert.Equal(new[] { (0L, 8L) }, ReadSupport(BootstrapSupportProjection.SupportOn(segment, carrier)));
        Assert.Equal(new[] { (0L, 2L) }, ReadSupport(BootstrapSupportProjection.TensionOn(segment, carrier)));
        Assert.Equal(new[] { (2L, 8L) }, ReadSupport(BootstrapSupportProjection.ExclusiveSupportOn(segment, carrier)));
        Assert.Equal(new[] { (2L, 8L) }, ReadSupport(BootstrapSupportProjection.ForwardResidueOn(segment, carrier)));
        Assert.Empty(ReadSupport(BootstrapSupportProjection.ReverseResidueOn(segment, carrier)));
    }

    [Fact]
    public void SupportProjectionCanPreserveReverseResidueWhenReverseSupportExtendsFurther()
    {
        var carrier = new BootstrapCarrier(10);
        var open = BootstrapRelation.Open();
        var expansiveUnit = new BootstrapRelation(new BootstrapMeasure(10, carrier), new BootstrapMeasure(14, carrier));
        var contractiveUnit = new BootstrapRelation(new BootstrapMeasure(14, carrier), new BootstrapMeasure(10, carrier));
        var forward = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(8, carrier), open), expansiveUnit);
        var reverse = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(12, carrier), open), contractiveUnit);
        var segment = new BootstrapRelation(forward, reverse);

        Assert.Equal(new[] { (0L, 12L) }, ReadSupport(BootstrapSupportProjection.SupportOn(segment, carrier)));
        Assert.Equal(new[] { (0L, 8L) }, ReadSupport(BootstrapSupportProjection.TensionOn(segment, carrier)));
        Assert.Equal(new[] { (8L, 12L) }, ReadSupport(BootstrapSupportProjection.ExclusiveSupportOn(segment, carrier)));
        Assert.Empty(ReadSupport(BootstrapSupportProjection.ForwardResidueOn(segment, carrier)));
        Assert.Equal(new[] { (12L, 8L) }, ReadSupport(BootstrapSupportProjection.ReverseResidueOn(segment, carrier)));
    }

    [Fact]
    public void SupportReadEquationsExposeNamedAndUncommittedNetSupportReads()
    {
        var overlap = new BootstrapRelation(new BootstrapMeasure(1), new BootstrapMeasure(1));
        var forwardResidue = new BootstrapRelation(new BootstrapMeasure(0), new BootstrapMeasure(1));

        Assert.Equal((0L, 0L), ReadFlowPair(BootstrapSupportReadEquations.Net(overlap)));
        Assert.Equal((1L, 1L), ReadFlowPair(BootstrapSupportReadEquations.Support(overlap)));
        Assert.Equal((0L, 1L), ReadFlowPair(BootstrapSupportReadEquations.ForwardOccupancy(overlap)));
        Assert.Equal((1L, 0L), ReadFlowPair(BootstrapSupportReadEquations.ReverseOccupancy(overlap)));
        Assert.Equal((1L, 1L), ReadFlowPair(BootstrapSupportReadEquations.Tension(overlap)));
        Assert.Equal((0L, 0L), ReadFlowPair(BootstrapSupportReadEquations.Exclusive(overlap)));
        Assert.Equal((0L, 1L), ReadFlowPair(BootstrapSupportReadEquations.LargerFlow(overlap)));
        Assert.Equal((1L, 1L), ReadFlowPair(BootstrapSupportReadEquations.CanceledSupport(overlap)));

        Assert.Equal((0L, 1L), ReadFlowPair(BootstrapSupportReadEquations.Net(forwardResidue)));
        Assert.Equal((0L, 1L), ReadFlowPair(BootstrapSupportReadEquations.Support(forwardResidue)));
        Assert.Equal((0L, 1L), ReadFlowPair(BootstrapSupportReadEquations.ForwardOccupancy(forwardResidue)));
        Assert.Equal((0L, 0L), ReadFlowPair(BootstrapSupportReadEquations.ReverseOccupancy(forwardResidue)));
        Assert.Equal((0L, 0L), ReadFlowPair(BootstrapSupportReadEquations.Tension(forwardResidue)));
        Assert.Equal((0L, 1L), ReadFlowPair(BootstrapSupportReadEquations.Exclusive(forwardResidue)));
        Assert.Equal((0L, 1L), ReadFlowPair(BootstrapSupportReadEquations.ForwardResidue(forwardResidue)));
        Assert.Equal((0L, 0L), ReadFlowPair(BootstrapSupportReadEquations.ReverseResidue(forwardResidue)));

        Assert.Equal(0, BootstrapSupportReadEquations.NetMagnitude(overlap));
        Assert.Equal(2, BootstrapSupportReadEquations.SupportMagnitude(overlap));
        Assert.Equal(1, BootstrapSupportReadEquations.NetMagnitude(forwardResidue));
        Assert.Equal(1, BootstrapSupportReadEquations.SupportMagnitude(forwardResidue));
    }

    [Fact]
    public void SupportReadEquationsExposeExplicitFourStateTruthTables()
    {
        var empty = new BootstrapRelation(new BootstrapMeasure(0), new BootstrapMeasure(0));
        var reverseOnly = new BootstrapRelation(new BootstrapMeasure(1), new BootstrapMeasure(0));
        var forwardOnly = new BootstrapRelation(new BootstrapMeasure(0), new BootstrapMeasure(1));
        var both = new BootstrapRelation(new BootstrapMeasure(1), new BootstrapMeasure(1));

        Assert.Equal((0L, 0L), ReadFlowPair(BootstrapSupportReadEquations.Support(empty)));
        Assert.Equal((1L, 0L), ReadFlowPair(BootstrapSupportReadEquations.Support(reverseOnly)));
        Assert.Equal((0L, 1L), ReadFlowPair(BootstrapSupportReadEquations.Support(forwardOnly)));
        Assert.Equal((1L, 1L), ReadFlowPair(BootstrapSupportReadEquations.Support(both)));

        Assert.Equal((0L, 0L), ReadFlowPair(BootstrapSupportReadEquations.ForwardOccupancy(empty)));
        Assert.Equal((0L, 0L), ReadFlowPair(BootstrapSupportReadEquations.ForwardOccupancy(reverseOnly)));
        Assert.Equal((0L, 1L), ReadFlowPair(BootstrapSupportReadEquations.ForwardOccupancy(forwardOnly)));
        Assert.Equal((0L, 1L), ReadFlowPair(BootstrapSupportReadEquations.ForwardOccupancy(both)));

        Assert.Equal((0L, 0L), ReadFlowPair(BootstrapSupportReadEquations.ReverseOccupancy(empty)));
        Assert.Equal((1L, 0L), ReadFlowPair(BootstrapSupportReadEquations.ReverseOccupancy(reverseOnly)));
        Assert.Equal((0L, 0L), ReadFlowPair(BootstrapSupportReadEquations.ReverseOccupancy(forwardOnly)));
        Assert.Equal((1L, 0L), ReadFlowPair(BootstrapSupportReadEquations.ReverseOccupancy(both)));

        Assert.Equal((0L, 0L), ReadFlowPair(BootstrapSupportReadEquations.Tension(empty)));
        Assert.Equal((0L, 0L), ReadFlowPair(BootstrapSupportReadEquations.Tension(reverseOnly)));
        Assert.Equal((0L, 0L), ReadFlowPair(BootstrapSupportReadEquations.Tension(forwardOnly)));
        Assert.Equal((1L, 1L), ReadFlowPair(BootstrapSupportReadEquations.Tension(both)));
    }

    [Fact]
    public void SupportProjectionCanUseUnnamedNetSupportEquationReads()
    {
        var carrier = new BootstrapCarrier(10);
        var open = BootstrapRelation.Open();
        var expansiveUnit = new BootstrapRelation(new BootstrapMeasure(10, carrier), new BootstrapMeasure(14, carrier));
        var contractiveUnit = new BootstrapRelation(new BootstrapMeasure(14, carrier), new BootstrapMeasure(10, carrier));
        var forward = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(8, carrier), open), expansiveUnit);
        var reverse = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(2, carrier), open), contractiveUnit);
        var segment = new BootstrapRelation(forward, reverse);

        Assert.Equal(new[] { (0L, 8L) }, ReadSupport(BootstrapSupportProjection.ProjectOn(segment, carrier, BootstrapSupportReadEquations.ForwardOccupancy)));
        Assert.Equal(new[] { (2L, 0L) }, ReadSupport(BootstrapSupportProjection.ProjectOn(segment, carrier, BootstrapSupportReadEquations.ReverseOccupancy)));
        Assert.Equal(new[] { (0L, 8L) }, ReadSupport(BootstrapSupportProjection.ProjectOn(segment, carrier, BootstrapSupportReadEquations.LargerFlow)));
    }

    [Fact]
    public void SupportSpanUtilitiesDiscoverDirectedCarrierSpansWithoutCollapsingOccurrences()
    {
        var carrier = new BootstrapCarrier(10);
        var open = BootstrapRelation.Open();
        var expansiveUnit = new BootstrapRelation(new BootstrapMeasure(10, carrier), new BootstrapMeasure(14, carrier));
        var contractiveUnit = new BootstrapRelation(new BootstrapMeasure(14, carrier), new BootstrapMeasure(10, carrier));
        var forward = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(8, carrier), open), expansiveUnit);
        var reverse = new BootstrapRelationFold(new BootstrapRelation(new BootstrapMeasure(2, carrier), open), contractiveUnit);
        var segment = new BootstrapRelation(forward, reverse);

        Assert.Equal(
            new[]
            {
                new BootstrapDirectedSupportSpan(0, 8, 1),
                new BootstrapDirectedSupportSpan(0, 2, -1),
            },
            BootstrapSupportSpanUtilities.DiscoverDirectedSpans(segment, carrier));
    }

    [Fact]
    public void SupportSpanUtilitiesBuildSupportStateSpansWithReverseAndForwardMagnitudes()
    {
        var states = BootstrapSupportSpanUtilities.BuildStateSpans(
            new[]
            {
                new BootstrapDirectedSupportSpan(0, 8, 1),
                new BootstrapDirectedSupportSpan(0, 2, -1),
            });

        Assert.Equal(
            new[]
            {
                new BootstrapSupportStateSpan(0, 2, 1, 1),
                new BootstrapSupportStateSpan(2, 8, 0, 1),
            },
            states);
    }

    private static (long First, long Second) Fold(BootstrapRelation relation) =>
        Read(new BootstrapRelationFold(relation.Left, relation.Right));

    private static (long First, long Second) Read(BootstrapRelationFold fold) =>
        (Assert.IsType<BootstrapMeasure>(fold.First).Raw, Assert.IsType<BootstrapMeasure>(fold.Second).Raw);

    private static (long First, long Second) ReadElement(Core4.Experimental.Bootstrap.IElement element) =>
        element switch
        {
            BootstrapRelationFold fold => Read(fold),
            BootstrapRelation relation => Fold(relation),
            _ => (Assert.IsType<BootstrapMeasure>(element).Raw, 0)
        };

    private static (long Reverse, long Forward) ReadFlowPair(Core4.Experimental.Bootstrap.IElement element)
    {
        var relation = Assert.IsType<BootstrapRelation>(element);
        return (Assert.IsType<BootstrapMeasure>(relation.Left).Raw, Assert.IsType<BootstrapMeasure>(relation.Right).Raw);
    }

    private static (long Start, long End)[] ReadSupport(Core4.Experimental.Bootstrap.IElement element) => element switch
    {
        BootstrapMeasure measure when measure.Raw == 0 => [],
        BootstrapRelation relation when relation.Left is BootstrapMeasure start && relation.Right is BootstrapMeasure end
            => [(start.Raw, end.Raw)],
        BootstrapRelation relation => ReadSupport(relation.Left).Concat(ReadSupport(relation.Right)).ToArray(),
        _ => throw new Xunit.Sdk.XunitException($"Unsupported support projection element: {element.GetType().Name}")
    };
}
