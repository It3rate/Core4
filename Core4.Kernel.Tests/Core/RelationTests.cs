using KernelMeasure = Core4.Kernel.Core.Measure;
using KernelRelationFold = Core4.Kernel.Core.RelationFold;
using KernelRelation = Core4.Kernel.Core.Relation;
using KernelCarrier = Core4.Kernel.Core.Carrier;
using KernelPartitionStep = Core4.Kernel.Core.PartitionStep;
using KernelSupportProjection = Core4.Kernel.Core.SupportProjection;
using KernelSupportSpanUtilities = Core4.Kernel.Core.SupportSpanUtilities;
using KernelDirectedSupportSpan = Core4.Kernel.Core.DirectedSupportSpan;
using KernelSupportStateSpan = Core4.Kernel.Core.SupportStateSpan;
using KernelSupportReadEquations = Core4.Kernel.Core.SupportReadEquations;

namespace Core4.Kernel.Tests.Core;

public sealed class RelationTests
{
    [Fact]
    public void RawMeasuresFoldToUnitFlowPair()
    {
        KernelMeasure.Offset = 0;
        try
        {
            var start = new KernelMeasure(10);
            var end = new KernelMeasure(14);
            var fold = new KernelRelationFold(start, end);

            Assert.Equal((4L, 1L), Read(fold));

            KernelMeasure.Offset = 100;

            Assert.Equal((4L, 1L), Read(fold));
            Assert.Equal(110, start.Raw);
            Assert.Equal(114, end.Raw);
            Assert.Equal((4L, 1L), Read(new KernelRelationFold(start, end)));
        }
        finally
        {
            KernelMeasure.Offset = 0;
        }
    }

    [Fact]
    public void CoincidentRawMeasuresFoldToOpenFraction()
    {
        var point = new KernelMeasure(10);

        Assert.Equal((0L, 0L), Read(new KernelRelationFold(point, point)));
    }

    [Fact]
    public void IsolatedRawMeasureDoesNotImplyOriginFlowOrPartitions()
    {
        var point = new KernelMeasure(-10);

        Assert.Equal(0, point.PartitionCount);
        Assert.Empty(point.Partitions());
    }

    [Fact]
    public void RawMeasureLoopPreservesDirectionAcrossAllTicks()
    {
        var dominantStart = new KernelMeasure(10);
        var dominantEnd = new KernelMeasure(20);
        var recessiveStart = new KernelMeasure(20);
        var recessiveEnd = new KernelMeasure(10);

        Assert.Equal((10L, 1L), Read(new KernelRelationFold(dominantStart, dominantEnd)));
        Assert.Equal((10L, -1L), Read(new KernelRelationFold(recessiveStart, recessiveEnd)));
    }

    [Fact]
    public void ZeroPartitionPointClaimsAppendWithoutMixing()
    {
        var leftPoint = KernelRelation.Claim(new KernelMeasure(8));
        var rightPoint = KernelRelation.Claim(new KernelMeasure(12));

        Assert.Equal((8L, 0L), Fold(Assert.IsType<KernelRelation>(leftPoint)));
        Assert.Equal((12L, 0L), Fold(Assert.IsType<KernelRelation>(rightPoint)));
        Assert.Equal((20L, 0L), Read(new KernelRelationFold(leftPoint, rightPoint)));
    }

    [Fact]
    public void PointClaimCanBeResolvedThroughSharedUnit()
    {
        var point = KernelRelation.Claim(new KernelMeasure(8));
        var unit = new KernelRelation(new KernelMeasure(10), new KernelMeasure(14));

        Assert.Equal((8L, 0L), Fold(Assert.IsType<KernelRelation>(point)));
        Assert.Equal((4L, 1L), Fold(unit));
        Assert.Equal((8L, 4L), Read(new KernelRelationFold(point, unit)));
    }

    [Fact]
    public void PointClaimUsesStructuralForwardSideInsteadOfRawMeasureSign()
    {
        var point = Assert.IsType<KernelRelation>(KernelRelation.Claim(new KernelMeasure(-8)));

        Assert.Equal((8L, 0L), Fold(point));
        Assert.Equal(
            new[]
            {
                new KernelPartitionStep(0, 1),
                new KernelPartitionStep(1, 1),
                new KernelPartitionStep(2, 1),
                new KernelPartitionStep(3, 1),
                new KernelPartitionStep(4, 1),
                new KernelPartitionStep(5, 1),
                new KernelPartitionStep(6, 1),
                new KernelPartitionStep(7, 1),
            },
            point.Partitions().ToArray());
    }

    [Fact]
    public void SharedBasisSmearingProducesMultiplicationLikeResult()
    {
        var unit = new KernelRelation(new KernelMeasure(10), new KernelMeasure(14));
        var two = new KernelRelation(KernelRelation.Claim(new KernelMeasure(8)), unit);
        var three = new KernelRelation(KernelRelation.Claim(new KernelMeasure(12)), unit);

        Assert.Equal((8L, 4L), Fold(two));
        Assert.Equal((12L, 4L), Fold(three));
        Assert.Equal((24L, 4L), Read(new KernelRelationFold(two, three)));
    }

    [Fact]
    public void PartitionIteratorUsesNaturalFlowAndPlacesPartialWidthFirst()
    {
        var unit = new KernelRelation(new KernelMeasure(10), new KernelMeasure(14));
        var value = new KernelRelationFold(KernelRelation.Claim(new KernelMeasure(26)), unit);
        var partitions = value.Partitions().ToArray();

        Assert.Equal(7, value.PartitionCount);
        Assert.Equal(
            new[]
            {
                new KernelPartitionStep(0, 2),
                new KernelPartitionStep(2, 4),
                new KernelPartitionStep(6, 4),
                new KernelPartitionStep(10, 4),
                new KernelPartitionStep(14, 4),
                new KernelPartitionStep(18, 4),
                new KernelPartitionStep(22, 4),
            },
            partitions);
    }

    [Fact]
    public void OpposedSidePointClaimOnPositiveUnitStartsAtLeftmostSide()
    {
        var unit = new KernelRelation(new KernelMeasure(10), new KernelMeasure(14));
        var opposedPoint = Assert.IsType<KernelRelation>(KernelRelation.ClaimOpposed(new KernelMeasure(10)));
        var value = new KernelRelationFold(opposedPoint, unit);

        Assert.Equal(
            new[]
            {
                new KernelPartitionStep(-10, 2),
                new KernelPartitionStep(-8, 4),
                new KernelPartitionStep(-4, 4),
            },
            value.Partitions().ToArray());
    }

    [Fact]
    public void OpposedSidePointClaimPreservesMagnitudeAndTracksItsOwnSide()
    {
        var opposedPoint = Assert.IsType<KernelRelation>(KernelRelation.ClaimOpposed(new KernelMeasure(10)));

        Assert.Equal((10L, 0L), Fold(opposedPoint));
        Assert.Equal(
            new[]
            {
                new KernelPartitionStep(-10, 1),
                new KernelPartitionStep(-9, 1),
                new KernelPartitionStep(-8, 1),
                new KernelPartitionStep(-7, 1),
                new KernelPartitionStep(-6, 1),
                new KernelPartitionStep(-5, 1),
                new KernelPartitionStep(-4, 1),
                new KernelPartitionStep(-3, 1),
                new KernelPartitionStep(-2, 1),
                new KernelPartitionStep(-1, 1),
            },
            opposedPoint.Partitions().ToArray());
    }

    [Fact]
    public void OpposedSidePointClaimUsesStructuralOpposedSideInsteadOfRawMeasureSign()
    {
        var opposedPoint = Assert.IsType<KernelRelation>(KernelRelation.ClaimOpposed(new KernelMeasure(-8)));

        Assert.Equal((8L, 0L), Fold(opposedPoint));
        Assert.Equal(
            new[]
            {
                new KernelPartitionStep(-8, 1),
                new KernelPartitionStep(-7, 1),
                new KernelPartitionStep(-6, 1),
                new KernelPartitionStep(-5, 1),
                new KernelPartitionStep(-4, 1),
                new KernelPartitionStep(-3, 1),
                new KernelPartitionStep(-2, 1),
                new KernelPartitionStep(-1, 1),
            },
            opposedPoint.Partitions().ToArray());
    }

    [Fact]
    public void NegativeUnitStartsAtRightmostSideAndFlowsLeft()
    {
        var unit = new KernelRelation(new KernelMeasure(10), new KernelMeasure(14));
        var inverseUnit = KernelRelation.Oppose(unit);
        var value = new KernelRelationFold(KernelRelation.Claim(new KernelMeasure(10)), inverseUnit);

        Assert.Equal(
            new[]
            {
                new KernelPartitionStep(10, -2),
                new KernelPartitionStep(8, -4),
                new KernelPartitionStep(4, -4),
            },
            value.Partitions().ToArray());
    }

    [Fact]
    public void ResolvedValueCanBeInvertedThroughOpposedBasis()
    {
        var point = KernelRelation.Claim(new KernelMeasure(8));
        var unit = new KernelRelation(new KernelMeasure(10), new KernelMeasure(14));
        var value = new KernelRelationFold(point, unit);
        var inverted = Assert.IsType<KernelRelationFold>(KernelRelation.Invert(value));

        Assert.Equal((8L, -4L), Read(inverted));
    }

    [Fact]
    public void OpenSiteCountIsDerivedFromStructure()
    {
        var open = KernelRelation.Open();
        var unit = new KernelRelation(new KernelMeasure(10), new KernelMeasure(14));
        var nested = new KernelRelation(new KernelRelation(open, unit), new KernelRelation(open, open));

        Assert.Equal(1, KernelRelation.OpenSiteCount(open));
        Assert.Equal(0, KernelRelation.OpenSiteCount(unit));
        Assert.Equal(3, KernelRelation.OpenSiteCount(nested));
    }

    [Fact]
    public void ApplyFillsTheFirstOpenSiteInsteadOfCreatingPeerComposition()
    {
        var open = KernelRelation.Open();
        var unit = new KernelRelation(new KernelMeasure(10), new KernelMeasure(14));
        var transform = new KernelRelation(open, unit);
        var applied = KernelRelation.Apply(transform, new KernelMeasure(8));

        var appliedRelation = Assert.IsType<KernelRelation>(applied);
        Assert.Equal((8L, 4L), Fold(appliedRelation));
        Assert.Equal(0, KernelRelation.OpenSiteCount(appliedRelation));
        Assert.Equal(8, Assert.IsType<KernelMeasure>(appliedRelation.Left).Raw);
    }

    [Fact]
    public void ComposeAlsoAutoDetectsAndFillsAnOpenSite()
    {
        var open = KernelRelation.Open();
        var unit = new KernelRelation(new KernelMeasure(10), new KernelMeasure(14));
        var transform = new KernelRelation(open, unit);
        var composed = KernelRelation.Compose(transform, new KernelMeasure(8));

        var composedRelation = Assert.IsType<KernelRelation>(composed);
        Assert.Equal((8L, 4L), Fold(composedRelation));
        Assert.Equal(0, KernelRelation.OpenSiteCount(composedRelation));
    }

    [Fact]
    public void ApplyFallsBackToPeerCompositionWhenNoOpenSiteExists()
    {
        var left = new KernelMeasure(10);
        var right = new KernelMeasure(14);
        var applied = KernelRelation.Apply(left, right);

        var relation = Assert.IsType<KernelRelation>(applied);
        Assert.Same(left, relation.Left);
        Assert.Same(right, relation.Right);
        Assert.Equal((4L, 1L), Fold(relation));
    }

    [Fact]
    public void ApplyCanFillNestedGradeTwoOpenStructure()
    {
        var open = KernelRelation.Open();
        var unit = new KernelRelation(new KernelMeasure(10), new KernelMeasure(14));
        var transform = new KernelRelation(new KernelRelation(open, unit), unit);
        var applied = KernelRelation.Apply(transform, new KernelMeasure(8));

        var outer = Assert.IsType<KernelRelation>(applied);
        var inner = Assert.IsType<KernelRelation>(outer.Left);

        Assert.Equal(0, KernelRelation.OpenSiteCount(applied));
        Assert.Equal((32L, 4L), Fold(outer));
        Assert.Equal(8, Assert.IsType<KernelMeasure>(inner.Left).Raw);
        Assert.Same(unit, outer.Right);
    }

    [Fact]
    public void ReusedNestedRelationsStillFoldByReadingTheirCurrentLeaves()
    {
        KernelMeasure.Offset = 0;
        try
        {
            var start = new KernelMeasure(10);
            var end = new KernelMeasure(14);
            var unit = new KernelRelation(start, end);
            var reused = new KernelRelationFold(unit, unit);

            Assert.Equal((16L, 1L), Read(reused));

            KernelMeasure.Offset = 50;

            var reusedLeft = Assert.IsType<KernelRelation>(reused.Left);
            var reusedRight = Assert.IsType<KernelRelation>(reused.Right);

            Assert.Same(reusedLeft, reusedRight);
            Assert.Equal((4L, 1L), Fold(reusedLeft));
            Assert.Equal((16L, 1L), Read(reused));
            Assert.Equal(60, Assert.IsType<KernelMeasure>(reusedLeft.Left).Raw);
            Assert.Equal(64, Assert.IsType<KernelMeasure>(reusedLeft.Right).Raw);
        }
        finally
        {
            KernelMeasure.Offset = 0;
        }
    }

    [Fact]
    public void NestedRelationsCanStillBeStoredWithoutAnyGradeFlags()
    {
        var left = new KernelRelation(new KernelMeasure(10), new KernelMeasure(14));
        var right = new KernelRelation(new KernelMeasure(20), new KernelMeasure(24));
        var parent = new KernelRelation(left, right);

        Assert.Same(left, parent.Left);
        Assert.Same(right, parent.Right);
    }

    [Fact]
    public void FoldResultIsItselfAnElementWithOrderedPairSurface()
    {
        var fold = new KernelRelationFold(new KernelMeasure(10), new KernelMeasure(14));

        Assert.IsAssignableFrom<Core4.Kernel.Core.IElement>(fold);
        Assert.Equal((4L, 1L), Read(fold));
        Assert.Equal((4L, 0L), ReadElement(fold.First));
        Assert.Equal((1L, 0L), ReadElement(fold.Second));
        var reduced = Assert.IsType<KernelRelation>(fold.Reduced);
        Assert.Equal(4, Assert.IsType<KernelMeasure>(reduced.Left).Raw);
        Assert.Equal(1, Assert.IsType<KernelMeasure>(reduced.Right).Raw);
    }

    [Fact]
    public void FoldKeepsOpenStructureAsReducedInsteadOfFlatteningItAway()
    {
        var open = KernelRelation.Open();
        var unit = new KernelRelation(new KernelMeasure(10), new KernelMeasure(14));
        var fold = new KernelRelationFold(open, unit);

        var reduced = Assert.IsType<KernelRelation>(fold.Reduced);
        var reducedLeft = Assert.IsType<KernelRelationFold>(reduced.Left);
        var reducedRight = Assert.IsType<KernelRelationFold>(reduced.Right);

        Assert.Equal(1, KernelRelation.OpenSiteCount(reduced));
        Assert.Equal((0L, 0L), Read(reducedLeft));
        Assert.Equal((4L, 1L), Read(reducedRight));
        Assert.Equal((0L, 4L), Read(fold));
    }

    [Fact]
    public void OpenTransformTraversesByItsClosedBasisInsteadOfItsProjectedZeroMagnitude()
    {
        var open = KernelRelation.Open();
        var unit = new KernelRelation(new KernelMeasure(10), new KernelMeasure(14));
        var transform = new KernelRelation(open, unit);
        var fold = new KernelRelationFold(open, unit);

        Assert.Equal(
            new[]
            {
                new KernelPartitionStep(0, 1),
                new KernelPartitionStep(1, 1),
                new KernelPartitionStep(2, 1),
                new KernelPartitionStep(3, 1),
            },
            transform.Partitions().ToArray());

        Assert.Equal(
            new[]
            {
                new KernelPartitionStep(0, 1),
                new KernelPartitionStep(1, 1),
                new KernelPartitionStep(2, 1),
                new KernelPartitionStep(3, 1),
            },
            fold.Partitions().ToArray());
    }

    [Fact]
    public void PartialApplicationBehavesLikeANewTransformWithFewerOpenSites()
    {
        var open = KernelRelation.Open();
        var unit = new KernelRelation(new KernelMeasure(10), new KernelMeasure(14));
        var transform = new KernelRelation(new KernelRelation(open, unit), open);
        var partiallyApplied = KernelRelation.Apply(transform, new KernelMeasure(8));
        var outer = Assert.IsType<KernelRelation>(partiallyApplied);
        var fold = new KernelRelationFold(outer.Left, outer.Right);

        var reduced = Assert.IsType<KernelRelation>(fold.Reduced);
        var reducedLeft = Assert.IsType<KernelRelationFold>(reduced.Left);

        Assert.Equal(1, KernelRelation.OpenSiteCount(reduced));
        Assert.Equal(1, KernelRelation.OpenSiteCount(partiallyApplied));
        Assert.Equal((8L, 4L), Read(reducedLeft));
    }

    [Fact]
    public void PartiallyAppliedTransformTraversesByItsClosedStructuralSide()
    {
        var open = KernelRelation.Open();
        var unit = new KernelRelation(new KernelMeasure(10), new KernelMeasure(14));
        var transform = new KernelRelation(new KernelRelation(open, unit), open);
        var partiallyApplied = KernelRelation.Apply(transform, new KernelMeasure(8));

        Assert.Equal(
            new[]
            {
                new KernelPartitionStep(0, 4),
                new KernelPartitionStep(4, 4),
            },
            partiallyApplied.Partitions().ToArray());
    }

    [Fact]
    public void ClosedGradeTwoFoldKeepsReducedChildStructure()
    {
        var open = KernelRelation.Open();
        var unit = new KernelRelation(new KernelMeasure(10), new KernelMeasure(14));
        var two = new KernelRelation(new KernelRelation(new KernelMeasure(8), open), unit);
        var three = new KernelRelation(new KernelRelation(new KernelMeasure(12), open), unit);
        var fold = new KernelRelationFold(two, three);

        var reduced = Assert.IsType<KernelRelation>(fold.Reduced);
        var reducedLeft = Assert.IsType<KernelRelationFold>(reduced.Left);
        var reducedRight = Assert.IsType<KernelRelationFold>(reduced.Right);

        Assert.Equal((24L, 4L), Read(fold));
        Assert.Equal(0, KernelRelation.OpenSiteCount(reduced));
        Assert.Equal((8L, 4L), Read(reducedLeft));
        Assert.Equal((12L, 4L), Read(reducedRight));
    }

    [Fact]
    public void ClosedGradeTwoFoldTraversesByItsReducedChildStructure()
    {
        var open = KernelRelation.Open();
        var unit = new KernelRelation(new KernelMeasure(10), new KernelMeasure(14));
        var two = new KernelRelation(new KernelRelation(new KernelMeasure(8), open), unit);
        var three = new KernelRelation(new KernelRelation(new KernelMeasure(12), open), unit);
        var fold = new KernelRelationFold(two, three);
        var reduced = Assert.IsType<KernelRelation>(fold.Reduced);

        var expected = new[]
        {
            new KernelPartitionStep(0, 4),
            new KernelPartitionStep(4, 4),
            new KernelPartitionStep(0, 4),
            new KernelPartitionStep(4, 4),
            new KernelPartitionStep(8, 4),
        };

        Assert.Equal(5, fold.PartitionCount);
        Assert.Equal(expected, fold.Partitions().ToArray());
        Assert.Equal(expected, reduced.Partitions().ToArray());
    }

    [Fact]
    public void ClosedGradeThreeFoldKeepsNestedReducedStructure()
    {
        var open = KernelRelation.Open();
        var unit = new KernelRelation(new KernelMeasure(10), new KernelMeasure(14));
        var two = new KernelRelation(new KernelRelation(new KernelMeasure(8), open), unit);
        var three = new KernelRelation(new KernelRelation(new KernelMeasure(12), open), unit);
        var four = new KernelRelation(new KernelRelation(new KernelMeasure(16), open), unit);
        var six = new KernelRelationFold(two, three);
        var twentyFour = new KernelRelationFold(six, four);

        var reduced = Assert.IsType<KernelRelation>(twentyFour.Reduced);
        var reducedLeft = Assert.IsType<KernelRelationFold>(reduced.Left);
        var reducedRight = Assert.IsType<KernelRelationFold>(reduced.Right);
        var reducedLeftStructure = Assert.IsType<KernelRelation>(reducedLeft.Reduced);

        Assert.Equal((96L, 4L), Read(twentyFour));
        Assert.Equal((24L, 4L), Read(reducedLeft));
        Assert.Equal((16L, 4L), Read(reducedRight));
        Assert.Equal(0, KernelRelation.OpenSiteCount(reduced));
        Assert.IsType<KernelRelationFold>(reducedLeftStructure.Left);
        Assert.IsType<KernelRelationFold>(reducedLeftStructure.Right);
    }

    [Fact]
    public void ClosedGradeThreeFoldTraversesByAllNestedResolvedChildren()
    {
        var open = KernelRelation.Open();
        var unit = new KernelRelation(new KernelMeasure(10), new KernelMeasure(14));
        var two = new KernelRelation(new KernelRelation(new KernelMeasure(8), open), unit);
        var three = new KernelRelation(new KernelRelation(new KernelMeasure(12), open), unit);
        var four = new KernelRelation(new KernelRelation(new KernelMeasure(16), open), unit);
        var six = new KernelRelationFold(two, three);
        var twentyFour = new KernelRelationFold(six, four);
        var reduced = Assert.IsType<KernelRelation>(twentyFour.Reduced);

        var expected = new[]
        {
            new KernelPartitionStep(0, 4),
            new KernelPartitionStep(4, 4),
            new KernelPartitionStep(0, 4),
            new KernelPartitionStep(4, 4),
            new KernelPartitionStep(8, 4),
            new KernelPartitionStep(0, 4),
            new KernelPartitionStep(4, 4),
            new KernelPartitionStep(8, 4),
            new KernelPartitionStep(12, 4),
        };

        Assert.Equal(9, twentyFour.PartitionCount);
        Assert.Equal(expected, twentyFour.Partitions().ToArray());
        Assert.Equal(expected, reduced.Partitions().ToArray());
    }

    [Fact]
    public void HigherGradePartialApplicationKeepsOneOpenSiteBeforeTopLevelFoldClosesIt()
    {
        var open = KernelRelation.Open();
        var unit = new KernelRelation(new KernelMeasure(10), new KernelMeasure(14));
        var transform = new KernelRelation(
            new KernelRelation(new KernelRelation(open, unit), unit),
            new KernelRelation(new KernelRelation(open, unit), unit));

        var partiallyApplied = KernelRelation.Apply(transform, new KernelMeasure(8));
        var outer = Assert.IsType<KernelRelation>(partiallyApplied);
        var left = Assert.IsType<KernelRelation>(outer.Left);
        var fold = new KernelRelationFold(outer.Left, outer.Right);
        var reduced = Assert.IsType<KernelRelation>(fold.Reduced);
        var partitions = partiallyApplied.Partitions().ToArray();

        Assert.Equal(1, KernelRelation.OpenSiteCount(partiallyApplied));
        Assert.Equal(0, KernelRelation.OpenSiteCount(reduced));
        Assert.Equal((32L, 4L), Fold(left));
        Assert.True(partitions.Length >= 8);
        Assert.All(partitions, partition => Assert.NotEqual(0, partition.Width));
        Assert.Contains(new KernelPartitionStep(0, 1), partitions);
        Assert.Contains(new KernelPartitionStep(0, 4), partitions);
    }

    [Fact]
    public void BareMeasureDefaultsToUnboundCarrier()
    {
        var measure = new KernelMeasure(8);

        Assert.Equal(KernelCarrier.Unbound, measure.Carrier);
        Assert.Equal(KernelMeasure.Unbound(8).Carrier, measure.Carrier);
    }

    [Fact]
    public void CarrierAttachedMeasureDoesNotSlideWithOffset()
    {
        KernelMeasure.Offset = 0;
        try
        {
            var carrier = new KernelCarrier(10);
            var attached = KernelMeasure.On(carrier, 8);
            var constructed = new KernelMeasure(9, carrier);

            KernelMeasure.Offset = 100;

            Assert.Equal(8, attached.Raw);
            Assert.Equal(9, constructed.Raw);
        }
        finally
        {
            KernelMeasure.Offset = 0;
        }
    }

    [Fact]
    public void MeasureCanAttachToCarrierIdentity()
    {
        var horizontal = new KernelCarrier(10);
        var vertical = new KernelCarrier(20);
        var x = KernelMeasure.On(horizontal, 8);
        var y = KernelMeasure.On(vertical, 8);

        Assert.Equal(horizontal, x.Carrier);
        Assert.Equal(vertical, y.Carrier);
        Assert.NotEqual(x.Carrier, y.Carrier);
    }

    [Fact]
    public void CarrierCanBeCrystallizedFromStructureAndRecoverItsSource()
    {
        var miles = new KernelCarrier(10);
        var hours = new KernelCarrier(20);
        var speedShape = new KernelRelation(
            new KernelMeasure(60, miles),
            new KernelMeasure(2, hours));

        var speed = KernelCarrier.Crystallize(speedShape);

        Assert.NotEqual(miles, speed);
        Assert.NotEqual(hours, speed);
        Assert.Equal(miles.Id, speed.Domain);
        Assert.True(KernelCarrier.TryGetSource(speed, out var source));
        Assert.Same(speedShape, source);
    }

    [Fact]
    public void CrystallizedCarrierCanBeUsedAsItsOwnStartingCarrier()
    {
        var miles = new KernelCarrier(10);
        var hours = new KernelCarrier(20);
        var speedShape = new KernelRelation(
            new KernelMeasure(60, miles),
            new KernelMeasure(2, hours));
        var speed = KernelCarrier.Crystallize(speedShape);
        var speedReading = new KernelMeasure(30, speed);

        Assert.Equal(speed, speedReading.Carrier);
        Assert.True(KernelCarrier.TryGetSource(speedReading.Carrier, out var source));
        Assert.Same(speedShape, source);
    }

    [Fact]
    public void CrystallizedCarrierKeepsFirstValueVariableAndRestFrozen()
    {
        var miles = new KernelCarrier(10);
        var hours = new KernelCarrier(20);
        var speedShape = new KernelRelation(
            new KernelMeasure(60, miles),
            new KernelRelation(
                new KernelMeasure(2, hours),
                new KernelMeasure(1, hours)));
        var speed = KernelCarrier.Crystallize(speedShape);

        Assert.True(KernelCarrier.TryGetVariableSource(speed, out var variable));
        Assert.True(KernelCarrier.TryGetFrozenSource(speed, out var frozen));
        Assert.Same(speedShape.Left, variable);
        Assert.Same(speedShape.Right, frozen);
    }

    [Fact]
    public void SingleValueCrystallizationHasVariableButNoFrozenSurface()
    {
        var speed = new KernelMeasure(30, new KernelCarrier(10));
        var crystallized = KernelCarrier.Crystallize(speed);

        Assert.True(KernelCarrier.TryGetVariableSource(crystallized, out var variable));
        Assert.False(KernelCarrier.TryGetFrozenSource(crystallized, out _));
        Assert.Same(speed, variable);
    }

    [Fact]
    public void CrystallizedCarrierExposesTickSurfaceByCarrierReference()
    {
        var miles = new KernelCarrier(10);
        var hours = new KernelCarrier(20);
        var speedShape = new KernelRelation(
            new KernelMeasure(60, miles),
            new KernelMeasure(2, hours));
        var speed = KernelCarrier.Crystallize(speedShape);

        Assert.True(KernelCarrier.TryGetTickSurface(speed, out var surface));
        Assert.Equal(
            new[]
            {
                new Core4.Kernel.Core.TickSurfaceEntry(miles, 1),
                new Core4.Kernel.Core.TickSurfaceEntry(hours, 1),
            },
            surface);
    }

    [Fact]
    public void AutomaticCrystallizationPreservesRepeatedCarrierOccurrences()
    {
        var meters = new KernelCarrier(10);
        var seconds = new KernelCarrier(20);
        var accelerationShape = new KernelRelation(
            new KernelMeasure(8, meters),
            new KernelRelation(
                new KernelMeasure(2, seconds),
                new KernelMeasure(3, seconds)));
        var acceleration = KernelCarrier.Crystallize(accelerationShape);

        Assert.True(KernelCarrier.TryGetTickSurface(acceleration, out var surface));
        Assert.Equal(
            new[]
            {
                new Core4.Kernel.Core.TickSurfaceEntry(meters, 1),
                new Core4.Kernel.Core.TickSurfaceEntry(seconds, 1),
                new Core4.Kernel.Core.TickSurfaceEntry(seconds, 1),
            },
            surface);
    }

    [Fact]
    public void CrystallizedCarrierCanCarryExplicitSignedTickSurface()
    {
        var miles = new KernelCarrier(10);
        var hours = new KernelCarrier(20);
        var speedShape = new KernelRelation(
            new KernelMeasure(60, miles),
            new KernelMeasure(2, hours));
        var speed = KernelCarrier.Crystallize(
            speedShape,
            new Core4.Kernel.Core.TickSurfaceEntry(miles, 1),
            new Core4.Kernel.Core.TickSurfaceEntry(hours, -1));

        Assert.True(KernelCarrier.TryGetTickSurface(speed, out var surface));
        Assert.Equal(
            new[]
            {
                new Core4.Kernel.Core.TickSurfaceEntry(miles, 1),
                new Core4.Kernel.Core.TickSurfaceEntry(hours, -1),
            },
            surface);
    }

    [Fact]
    public void ExplicitTickSurfaceCanExpressRepeatedContractiveParticipation()
    {
        var meters = new KernelCarrier(10);
        var seconds = new KernelCarrier(20);
        var accelerationShape = new KernelRelation(
            new KernelMeasure(8, meters),
            new KernelRelation(
                new KernelMeasure(2, seconds),
                new KernelMeasure(3, seconds)));
        var acceleration = KernelCarrier.Crystallize(
            accelerationShape,
            new Core4.Kernel.Core.TickSurfaceEntry(meters, 1),
            new Core4.Kernel.Core.TickSurfaceEntry(seconds, -2));

        Assert.True(KernelCarrier.TryGetTickSurface(acceleration, out var surface));
        Assert.Equal(
            new[]
            {
                new Core4.Kernel.Core.TickSurfaceEntry(meters, 1),
                new Core4.Kernel.Core.TickSurfaceEntry(seconds, -2),
            },
            surface);
    }

    [Fact]
    public void ExplicitTickSurfaceCanKeepOpposedSameCarrierParticipationsSeparate()
    {
        var meters = new KernelCarrier(10);
        var seconds = new KernelCarrier(20);
        var mixedShape = new KernelRelation(
            new KernelMeasure(8, meters),
            new KernelRelation(
                new KernelMeasure(2, seconds),
                new KernelMeasure(4, seconds)));
        var mixed = KernelCarrier.Crystallize(
            mixedShape,
            new Core4.Kernel.Core.TickSurfaceEntry(meters, 1),
            new Core4.Kernel.Core.TickSurfaceEntry(seconds, 1),
            new Core4.Kernel.Core.TickSurfaceEntry(seconds, -1));

        Assert.True(KernelCarrier.TryGetTickSurface(mixed, out var surface));
        Assert.Equal(
            new[]
            {
                new Core4.Kernel.Core.TickSurfaceEntry(meters, 1),
                new Core4.Kernel.Core.TickSurfaceEntry(seconds, 1),
                new Core4.Kernel.Core.TickSurfaceEntry(seconds, -1),
            },
            surface);
    }

    [Fact]
    public void ExplicitCrystallizationKeepsSignedVariableAndFrozenSurfaces()
    {
        var miles = new KernelCarrier(10);
        var hours = new KernelCarrier(20);
        var distance = new KernelMeasure(60, miles);
        var time = new KernelMeasure(2, hours);
        var speed = KernelCarrier.Crystallize(
            new KernelRelation(distance, time),
            distance,
            time,
            new Core4.Kernel.Core.TickSurfaceEntry(miles, 1),
            new Core4.Kernel.Core.TickSurfaceEntry(hours, -1));

        Assert.True(KernelCarrier.TryGetVariableTickSurface(speed, out var variableSurface));
        Assert.True(KernelCarrier.TryGetFrozenTickSurface(speed, out var frozenSurface));
        Assert.Equal(
            new[]
            {
                new Core4.Kernel.Core.TickSurfaceEntry(miles, 1),
            },
            variableSurface);
        Assert.Equal(
            new[]
            {
                new Core4.Kernel.Core.TickSurfaceEntry(hours, -1),
            },
            frozenSurface);
    }

    [Fact]
    public void ExplicitCrystallizationKeepsRepeatedContractiveFrozenSurface()
    {
        var meters = new KernelCarrier(10);
        var seconds = new KernelCarrier(20);
        var distance = new KernelMeasure(8, meters);
        var timeBasis = new KernelRelation(
            new KernelMeasure(2, seconds),
            new KernelMeasure(3, seconds));
        var acceleration = KernelCarrier.Crystallize(
            new KernelRelation(distance, timeBasis),
            distance,
            timeBasis,
            new Core4.Kernel.Core.TickSurfaceEntry(meters, 1),
            new Core4.Kernel.Core.TickSurfaceEntry(seconds, -2));

        Assert.True(KernelCarrier.TryGetFrozenTickSurface(acceleration, out var frozenSurface));
        Assert.Equal(
            new[]
            {
                new Core4.Kernel.Core.TickSurfaceEntry(seconds, -2),
            },
            frozenSurface);
    }

    [Fact]
    public void ExplicitCrystallizationCanKeepWholeSourceVariable()
    {
        var kilowatts = new KernelCarrier(10);
        var hours = new KernelCarrier(20);
        var energyShape = new KernelRelation(
            new KernelMeasure(30, kilowatts),
            new KernelMeasure(2, hours));
        var energy = KernelCarrier.Crystallize(
            energyShape,
            energyShape,
            null,
            new Core4.Kernel.Core.TickSurfaceEntry(kilowatts, 1),
            new Core4.Kernel.Core.TickSurfaceEntry(hours, 1));

        Assert.True(KernelCarrier.TryGetVariableSource(energy, out var variable));
        Assert.False(KernelCarrier.TryGetFrozenSource(energy, out _));
        Assert.True(KernelCarrier.TryGetVariableTickSurface(energy, out var variableSurface));
        Assert.Same(energyShape, variable);
        Assert.Equal(
            new[]
            {
                new Core4.Kernel.Core.TickSurfaceEntry(kilowatts, 1),
                new Core4.Kernel.Core.TickSurfaceEntry(hours, 1),
            },
            variableSurface);
    }

    [Fact]
    public void CanonicalProjectorCanProjectCompatibleQuotientInput()
    {
        var miles = new KernelCarrier(10);
        var hours = new KernelCarrier(20);
        var speedShape = new KernelRelation(
            new KernelMeasure(60, miles),
            new KernelMeasure(2, hours));
        var speed = KernelCarrier.Crystallize(
            speedShape,
            new Core4.Kernel.Core.TickSurfaceEntry(miles, 1),
            new Core4.Kernel.Core.TickSurfaceEntry(hours, -1));
        var input = new KernelRelation(
            new KernelMeasure(60, miles),
            new KernelMeasure(2, hours));

        Assert.True(KernelCarrier.TryProject(speed, input, out var projected));
        Assert.Equal(speed, projected.Carrier);
        Assert.Equal(30, projected.Raw);
    }

    [Fact]
    public void CanonicalProjectorCanProjectCompatibleProductInput()
    {
        var pounds = new KernelCarrier(10);
        var feet = new KernelCarrier(20);
        var footPounds = KernelCarrier.Crystallize(
            new KernelRelation(new KernelMeasure(1, pounds), new KernelMeasure(1, feet)),
            new Core4.Kernel.Core.TickSurfaceEntry(pounds, 1),
            new Core4.Kernel.Core.TickSurfaceEntry(feet, 1));
        var input = new KernelRelation(
            new KernelMeasure(30, pounds),
            new KernelMeasure(2, feet));

        Assert.True(KernelCarrier.TryProject(footPounds, input, out var projected));
        Assert.Equal(footPounds, projected.Carrier);
        Assert.Equal(60, projected.Raw);
    }

    [Fact]
    public void CanonicalProjectorCanProjectRepeatedContractiveInput()
    {
        var meters = new KernelCarrier(10);
        var seconds = new KernelCarrier(20);
        var acceleration = KernelCarrier.Crystallize(
            new KernelRelation(
                new KernelMeasure(1, meters),
                new KernelRelation(new KernelMeasure(1, seconds), new KernelMeasure(1, seconds))),
            new Core4.Kernel.Core.TickSurfaceEntry(meters, 1),
            new Core4.Kernel.Core.TickSurfaceEntry(seconds, -2));
        var input = new KernelRelation(
            new KernelMeasure(8, meters),
            new KernelRelation(new KernelMeasure(2, seconds), new KernelMeasure(4, seconds)));

        Assert.True(KernelCarrier.TryProject(acceleration, input, out var projected));
        Assert.Equal(acceleration, projected.Carrier);
        Assert.Equal(1, projected.Raw);
    }

    [Fact]
    public void CanonicalProjectorCanProjectMixedSameCarrierSurfaceInOrder()
    {
        var meters = new KernelCarrier(10);
        var seconds = new KernelCarrier(20);
        var mixed = KernelCarrier.Crystallize(
            new KernelRelation(
                new KernelMeasure(1, meters),
                new KernelRelation(new KernelMeasure(1, seconds), new KernelMeasure(1, seconds))),
            new Core4.Kernel.Core.TickSurfaceEntry(meters, 1),
            new Core4.Kernel.Core.TickSurfaceEntry(seconds, 1),
            new Core4.Kernel.Core.TickSurfaceEntry(seconds, -1));
        var input = new KernelRelation(
            new KernelMeasure(8, meters),
            new KernelRelation(new KernelMeasure(2, seconds), new KernelMeasure(4, seconds)));

        Assert.True(KernelCarrier.TryProject(mixed, input, out var projected));
        Assert.Equal(mixed, projected.Carrier);
        Assert.Equal(4, projected.Raw);
    }

    [Fact]
    public void CanonicalProjectorRejectsMismatchedSurface()
    {
        var miles = new KernelCarrier(10);
        var hours = new KernelCarrier(20);
        var speed = KernelCarrier.Crystallize(
            new KernelRelation(new KernelMeasure(1, miles), new KernelMeasure(1, hours)),
            new Core4.Kernel.Core.TickSurfaceEntry(miles, 1),
            new Core4.Kernel.Core.TickSurfaceEntry(hours, -1));
        var incompatible = new KernelRelation(
            new KernelMeasure(60, miles),
            new KernelMeasure(5, new KernelCarrier(30)));

        Assert.False(KernelCarrier.TryProject(speed, incompatible, out _));
    }

    [Fact]
    public void ClosedDirectedSegmentLikeFoldPreservesAllFourBranchContributions()
    {
        var carrier = new KernelCarrier(10);
        var open = KernelRelation.Open();
        var expansiveUnit = new KernelRelation(new KernelMeasure(10, carrier), new KernelMeasure(14, carrier));
        var contractiveUnit = KernelRelation.Oppose(expansiveUnit);

        var a = new KernelRelationFold(new KernelRelation(new KernelMeasure(8, carrier), open), expansiveUnit);
        var b = new KernelRelationFold(new KernelRelation(new KernelMeasure(12, carrier), open), contractiveUnit);
        var c = new KernelRelationFold(new KernelRelation(new KernelMeasure(16, carrier), open), expansiveUnit);
        var d = new KernelRelationFold(new KernelRelation(new KernelMeasure(20, carrier), open), contractiveUnit);
        var left = new KernelRelation(a, b);
        var right = new KernelRelation(c, d);
        var fold = new KernelRelationFold(left, right);

        var reduced = Assert.IsType<KernelRelation>(fold.Reduced);
        var aligned = Assert.IsType<KernelRelation>(reduced.Left);
        var orthogonal = Assert.IsType<KernelRelation>(reduced.Right);

        Assert.Equal((32L, 4L), Read(Assert.IsType<KernelRelationFold>(aligned.Left)));
        Assert.Equal((60L, -4L), Read(Assert.IsType<KernelRelationFold>(aligned.Right)));
        Assert.Equal((-40L, 4L), Read(Assert.IsType<KernelRelationFold>(orthogonal.Left)));
        Assert.Equal((-48L, -4L), Read(Assert.IsType<KernelRelationFold>(orthogonal.Right)));
    }

    [Fact]
    public void ClosedDirectedSegmentLikeFoldCanCollapseRepeatedlyBackThroughLowerGrades()
    {
        var carrier = new KernelCarrier(10);
        var open = KernelRelation.Open();
        var expansiveUnit = new KernelRelation(new KernelMeasure(10, carrier), new KernelMeasure(14, carrier));
        var contractiveUnit = KernelRelation.Oppose(expansiveUnit);

        var a = new KernelRelationFold(new KernelRelation(new KernelMeasure(8, carrier), open), expansiveUnit);
        var b = new KernelRelationFold(new KernelRelation(new KernelMeasure(12, carrier), open), contractiveUnit);
        var c = new KernelRelationFold(new KernelRelation(new KernelMeasure(16, carrier), open), expansiveUnit);
        var d = new KernelRelationFold(new KernelRelation(new KernelMeasure(20, carrier), open), contractiveUnit);
        var fold = new KernelRelationFold(new KernelRelation(a, b), new KernelRelation(c, d));

        var collapsed = Assert.IsType<KernelRelation>(fold.Collapsed);
        var aligned = Assert.IsType<KernelRelation>(collapsed.Left);
        var orthogonal = Assert.IsType<KernelRelation>(collapsed.Right);
        Assert.Equal((24L, 4L), ReadElement(aligned.Left));
        Assert.Equal((-32L, 4L), ReadElement(aligned.Right));
        Assert.Equal((-12L, 4L), ReadElement(orthogonal.Left));
        Assert.Equal((4L, 4L), ReadElement(orthogonal.Right));
        Assert.Equal((-16L, 4L), ReadElement(fold.Projected));
    }

    [Fact]
    public void ClosedOrthogonalLikeFoldPreservesAlignedAndCrossCarrierContributions()
    {
        var horizontal = new KernelCarrier(10);
        var vertical = new KernelCarrier(20);
        var open = KernelRelation.Open();
        var horizontalUnit = new KernelRelation(new KernelMeasure(10, horizontal), new KernelMeasure(14, horizontal));
        var verticalUnit = new KernelRelation(new KernelMeasure(30, vertical), new KernelMeasure(34, vertical));

        var a = new KernelRelationFold(new KernelRelation(new KernelMeasure(8, horizontal), open), horizontalUnit);
        var b = new KernelRelationFold(new KernelRelation(new KernelMeasure(12, vertical), open), verticalUnit);
        var c = new KernelRelationFold(new KernelRelation(new KernelMeasure(16, horizontal), open), horizontalUnit);
        var d = new KernelRelationFold(new KernelRelation(new KernelMeasure(20, vertical), open), verticalUnit);
        var left = new KernelRelation(a, b);
        var right = new KernelRelation(c, d);
        var fold = new KernelRelationFold(left, right);

        var reduced = Assert.IsType<KernelRelation>(fold.Reduced);
        var aligned = Assert.IsType<KernelRelation>(reduced.Left);
        var orthogonal = Assert.IsType<KernelRelation>(reduced.Right);

        Assert.Equal((32L, 4L), Read(Assert.IsType<KernelRelationFold>(aligned.Left)));
        Assert.Equal((60L, 4L), Read(Assert.IsType<KernelRelationFold>(aligned.Right)));
        Assert.Equal((40L, 4L), Read(Assert.IsType<KernelRelationFold>(orthogonal.Left)));
        Assert.Equal((48L, 4L), Read(Assert.IsType<KernelRelationFold>(orthogonal.Right)));
    }

    [Fact]
    public void ClosedOrthogonalLikeFoldCanCollapseRepeatedlyBackThroughLowerGrades()
    {
        var horizontal = new KernelCarrier(10);
        var vertical = new KernelCarrier(20);
        var open = KernelRelation.Open();
        var horizontalUnit = new KernelRelation(new KernelMeasure(10, horizontal), new KernelMeasure(14, horizontal));
        var verticalUnit = new KernelRelation(new KernelMeasure(30, vertical), new KernelMeasure(34, vertical));

        var a = new KernelRelationFold(new KernelRelation(new KernelMeasure(8, horizontal), open), horizontalUnit);
        var b = new KernelRelationFold(new KernelRelation(new KernelMeasure(12, vertical), open), verticalUnit);
        var c = new KernelRelationFold(new KernelRelation(new KernelMeasure(16, horizontal), open), horizontalUnit);
        var d = new KernelRelationFold(new KernelRelation(new KernelMeasure(20, vertical), open), verticalUnit);
        var fold = new KernelRelationFold(new KernelRelation(a, b), new KernelRelation(c, d));

        var collapsed = Assert.IsType<KernelRelation>(fold.Collapsed);
        var aligned = Assert.IsType<KernelRelation>(collapsed.Left);
        var orthogonal = Assert.IsType<KernelRelation>(collapsed.Right);
        Assert.Equal((24L, 4L), ReadElement(aligned.Left));
        Assert.Equal((32L, 4L), ReadElement(aligned.Right));
        Assert.Equal((28L, 4L), ReadElement(orthogonal.Left));
        Assert.Equal((28L, 4L), ReadElement(orthogonal.Right));
        Assert.Equal((112L, 4L), ReadElement(fold.Projected));
    }

    [Fact]
    public void SupportProjectionCanReadSupportTensionAndResidueFromDirectedSegmentLikePair()
    {
        var carrier = new KernelCarrier(10);
        var open = KernelRelation.Open();
        var expansiveUnit = new KernelRelation(new KernelMeasure(10, carrier), new KernelMeasure(14, carrier));
        var contractiveUnit = KernelRelation.Oppose(expansiveUnit);
        var forward = new KernelRelationFold(new KernelRelation(new KernelMeasure(8, carrier), open), expansiveUnit);
        var reverse = new KernelRelationFold(new KernelRelation(new KernelMeasure(2, carrier), open), contractiveUnit);
        var segment = new KernelRelation(forward, reverse);

        Assert.Equal(new[] { (0L, 8L) }, ReadSupport(KernelSupportProjection.SupportOn(segment, carrier)));
        Assert.Equal(new[] { (0L, 2L) }, ReadSupport(KernelSupportProjection.TensionOn(segment, carrier)));
        Assert.Equal(new[] { (2L, 8L) }, ReadSupport(KernelSupportProjection.ExclusiveSupportOn(segment, carrier)));
        Assert.Equal(new[] { (2L, 8L) }, ReadSupport(KernelSupportProjection.ForwardResidueOn(segment, carrier)));
        Assert.Empty(ReadSupport(KernelSupportProjection.ReverseResidueOn(segment, carrier)));
    }

    [Fact]
    public void SupportProjectionCanPreserveReverseResidueWhenReverseSupportExtendsFurther()
    {
        var carrier = new KernelCarrier(10);
        var open = KernelRelation.Open();
        var expansiveUnit = new KernelRelation(new KernelMeasure(10, carrier), new KernelMeasure(14, carrier));
        var contractiveUnit = KernelRelation.Oppose(expansiveUnit);
        var forward = new KernelRelationFold(new KernelRelation(new KernelMeasure(8, carrier), open), expansiveUnit);
        var reverse = new KernelRelationFold(new KernelRelation(new KernelMeasure(12, carrier), open), contractiveUnit);
        var segment = new KernelRelation(forward, reverse);

        Assert.Equal(new[] { (0L, 12L) }, ReadSupport(KernelSupportProjection.SupportOn(segment, carrier)));
        Assert.Equal(new[] { (0L, 8L) }, ReadSupport(KernelSupportProjection.TensionOn(segment, carrier)));
        Assert.Equal(new[] { (8L, 12L) }, ReadSupport(KernelSupportProjection.ExclusiveSupportOn(segment, carrier)));
        Assert.Empty(ReadSupport(KernelSupportProjection.ForwardResidueOn(segment, carrier)));
        Assert.Equal(new[] { (12L, 8L) }, ReadSupport(KernelSupportProjection.ReverseResidueOn(segment, carrier)));
    }

    [Fact]
    public void SupportReadEquationsExposeNamedAndUncommittedNetSupportReads()
    {
        var overlap = new KernelRelation(new KernelMeasure(1), new KernelMeasure(1));
        var forwardResidue = new KernelRelation(new KernelMeasure(0), new KernelMeasure(1));

        Assert.Equal((0L, 0L), ReadFlowPair(KernelSupportReadEquations.Net(overlap)));
        Assert.Equal((1L, 1L), ReadFlowPair(KernelSupportReadEquations.Support(overlap)));
        Assert.Equal((0L, 1L), ReadFlowPair(KernelSupportReadEquations.ForwardOccupancy(overlap)));
        Assert.Equal((1L, 0L), ReadFlowPair(KernelSupportReadEquations.ReverseOccupancy(overlap)));
        Assert.Equal((1L, 1L), ReadFlowPair(KernelSupportReadEquations.Tension(overlap)));
        Assert.Equal((0L, 0L), ReadFlowPair(KernelSupportReadEquations.Exclusive(overlap)));
        Assert.Equal((0L, 1L), ReadFlowPair(KernelSupportReadEquations.LargerFlow(overlap)));
        Assert.Equal((1L, 1L), ReadFlowPair(KernelSupportReadEquations.CanceledSupport(overlap)));

        Assert.Equal((0L, 1L), ReadFlowPair(KernelSupportReadEquations.Net(forwardResidue)));
        Assert.Equal((0L, 1L), ReadFlowPair(KernelSupportReadEquations.Support(forwardResidue)));
        Assert.Equal((0L, 1L), ReadFlowPair(KernelSupportReadEquations.ForwardOccupancy(forwardResidue)));
        Assert.Equal((0L, 0L), ReadFlowPair(KernelSupportReadEquations.ReverseOccupancy(forwardResidue)));
        Assert.Equal((0L, 0L), ReadFlowPair(KernelSupportReadEquations.Tension(forwardResidue)));
        Assert.Equal((0L, 1L), ReadFlowPair(KernelSupportReadEquations.Exclusive(forwardResidue)));
        Assert.Equal((0L, 1L), ReadFlowPair(KernelSupportReadEquations.ForwardResidue(forwardResidue)));
        Assert.Equal((0L, 0L), ReadFlowPair(KernelSupportReadEquations.ReverseResidue(forwardResidue)));

        Assert.Equal(0, KernelSupportReadEquations.NetMagnitude(overlap));
        Assert.Equal(2, KernelSupportReadEquations.SupportMagnitude(overlap));
        Assert.Equal(1, KernelSupportReadEquations.NetMagnitude(forwardResidue));
        Assert.Equal(1, KernelSupportReadEquations.SupportMagnitude(forwardResidue));
    }

    [Fact]
    public void SupportReadEquationsExposeExplicitFourStateTruthTables()
    {
        var empty = new KernelRelation(new KernelMeasure(0), new KernelMeasure(0));
        var reverseOnly = new KernelRelation(new KernelMeasure(1), new KernelMeasure(0));
        var forwardOnly = new KernelRelation(new KernelMeasure(0), new KernelMeasure(1));
        var both = new KernelRelation(new KernelMeasure(1), new KernelMeasure(1));

        Assert.Equal((0L, 0L), ReadFlowPair(KernelSupportReadEquations.Support(empty)));
        Assert.Equal((1L, 0L), ReadFlowPair(KernelSupportReadEquations.Support(reverseOnly)));
        Assert.Equal((0L, 1L), ReadFlowPair(KernelSupportReadEquations.Support(forwardOnly)));
        Assert.Equal((1L, 1L), ReadFlowPair(KernelSupportReadEquations.Support(both)));

        Assert.Equal((0L, 0L), ReadFlowPair(KernelSupportReadEquations.ForwardOccupancy(empty)));
        Assert.Equal((0L, 0L), ReadFlowPair(KernelSupportReadEquations.ForwardOccupancy(reverseOnly)));
        Assert.Equal((0L, 1L), ReadFlowPair(KernelSupportReadEquations.ForwardOccupancy(forwardOnly)));
        Assert.Equal((0L, 1L), ReadFlowPair(KernelSupportReadEquations.ForwardOccupancy(both)));

        Assert.Equal((0L, 0L), ReadFlowPair(KernelSupportReadEquations.ReverseOccupancy(empty)));
        Assert.Equal((1L, 0L), ReadFlowPair(KernelSupportReadEquations.ReverseOccupancy(reverseOnly)));
        Assert.Equal((0L, 0L), ReadFlowPair(KernelSupportReadEquations.ReverseOccupancy(forwardOnly)));
        Assert.Equal((1L, 0L), ReadFlowPair(KernelSupportReadEquations.ReverseOccupancy(both)));

        Assert.Equal((0L, 0L), ReadFlowPair(KernelSupportReadEquations.Tension(empty)));
        Assert.Equal((0L, 0L), ReadFlowPair(KernelSupportReadEquations.Tension(reverseOnly)));
        Assert.Equal((0L, 0L), ReadFlowPair(KernelSupportReadEquations.Tension(forwardOnly)));
        Assert.Equal((1L, 1L), ReadFlowPair(KernelSupportReadEquations.Tension(both)));
    }

    [Fact]
    public void SupportProjectionCanUseUnnamedNetSupportEquationReads()
    {
        var carrier = new KernelCarrier(10);
        var open = KernelRelation.Open();
        var expansiveUnit = new KernelRelation(new KernelMeasure(10, carrier), new KernelMeasure(14, carrier));
        var contractiveUnit = KernelRelation.Oppose(expansiveUnit);
        var forward = new KernelRelationFold(new KernelRelation(new KernelMeasure(8, carrier), open), expansiveUnit);
        var reverse = new KernelRelationFold(new KernelRelation(new KernelMeasure(2, carrier), open), contractiveUnit);
        var segment = new KernelRelation(forward, reverse);

        Assert.Equal(new[] { (0L, 8L) }, ReadSupport(KernelSupportProjection.ProjectOn(segment, carrier, KernelSupportReadEquations.ForwardOccupancy)));
        Assert.Equal(new[] { (2L, 0L) }, ReadSupport(KernelSupportProjection.ProjectOn(segment, carrier, KernelSupportReadEquations.ReverseOccupancy)));
        Assert.Equal(new[] { (0L, 8L) }, ReadSupport(KernelSupportProjection.ProjectOn(segment, carrier, KernelSupportReadEquations.LargerFlow)));
    }

    [Fact]
    public void SupportSpanUtilitiesDiscoverDirectedCarrierSpansWithoutCollapsingOccurrences()
    {
        var carrier = new KernelCarrier(10);
        var open = KernelRelation.Open();
        var expansiveUnit = new KernelRelation(new KernelMeasure(10, carrier), new KernelMeasure(14, carrier));
        var contractiveUnit = KernelRelation.Oppose(expansiveUnit);
        var forward = new KernelRelationFold(new KernelRelation(new KernelMeasure(8, carrier), open), expansiveUnit);
        var reverse = new KernelRelationFold(new KernelRelation(new KernelMeasure(2, carrier), open), contractiveUnit);
        var segment = new KernelRelation(forward, reverse);

        Assert.Equal(
            new[]
            {
                new KernelDirectedSupportSpan(0, 8, 1),
                new KernelDirectedSupportSpan(0, 2, -1),
            },
            KernelSupportSpanUtilities.DiscoverDirectedSpans(segment, carrier));
    }

    [Fact]
    public void OpposedUnitCanBeDerivedFromBasisInsteadOfHandwrittenReverseLiteral()
    {
        var unit = new KernelRelation(new KernelMeasure(10), new KernelMeasure(14));
        var opposed = KernelRelation.Oppose(unit);

        Assert.Equal((4L, -1L), Fold(Assert.IsType<KernelRelation>(opposed)));
    }

    [Fact]
    public void SupportSpanUtilitiesBuildSupportStateSpansWithReverseAndForwardMagnitudes()
    {
        var states = KernelSupportSpanUtilities.BuildStateSpans(
            new[]
            {
                new KernelDirectedSupportSpan(0, 8, 1),
                new KernelDirectedSupportSpan(0, 2, -1),
            });

        Assert.Equal(
            new[]
            {
                new KernelSupportStateSpan(0, 2, 1, 1),
                new KernelSupportStateSpan(2, 8, 0, 1),
            },
            states);
    }

    private static (long First, long Second) Fold(KernelRelation relation) =>
        Read(new KernelRelationFold(relation.Left, relation.Right));

    private static (long First, long Second) Read(KernelRelationFold fold) =>
        (Assert.IsType<KernelMeasure>(fold.First).Raw, Assert.IsType<KernelMeasure>(fold.Second).Raw);

    private static (long First, long Second) ReadElement(Core4.Kernel.Core.IElement element) =>
        element switch
        {
            KernelRelationFold fold => Read(fold),
            KernelRelation relation => Fold(relation),
            _ => (Assert.IsType<KernelMeasure>(element).Raw, 0)
        };

    private static (long Reverse, long Forward) ReadFlowPair(Core4.Kernel.Core.IElement element)
    {
        var relation = Assert.IsType<KernelRelation>(element);
        return (Assert.IsType<KernelMeasure>(relation.Left).Raw, Assert.IsType<KernelMeasure>(relation.Right).Raw);
    }

    private static (long Start, long End)[] ReadSupport(Core4.Kernel.Core.IElement element) => element switch
    {
        KernelMeasure measure when measure.Raw == 0 => [],
        KernelRelation relation when relation.Left is KernelMeasure start && relation.Right is KernelMeasure end
            => [(start.Raw, end.Raw)],
        KernelRelation relation => ReadSupport(relation.Left).Concat(ReadSupport(relation.Right)).ToArray(),
        _ => throw new Xunit.Sdk.XunitException($"Unsupported support projection element: {element.GetType().Name}")
    };
}
