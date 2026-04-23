using BootstrapMeasure = Core4.Experimental.Bootstrap.Measure;
using BootstrapRelationFold = Core4.Experimental.Bootstrap.RelationFold;
using BootstrapRelation = Core4.Experimental.Bootstrap.Relation;
using BootstrapPartitionStep = Core4.Experimental.Bootstrap.PartitionStep;

namespace Core4.Tests.Experimental.Bootstrap;

public sealed class BootstrapRelationTests
{
    [Fact]
    public void RawMeasuresFoldToUnitLikeFraction()
    {
        BootstrapMeasure.Offset = 0;
        try
        {
            var start = new BootstrapMeasure(10);
            var end = new BootstrapMeasure(14);
            var fold = new BootstrapRelationFold(start, end);

            Assert.Equal((4L, 4L), Read(fold));

            BootstrapMeasure.Offset = 100;

            Assert.Equal((4L, 4L), Read(fold));
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

        Assert.Equal((10L, 10L), Read(new BootstrapRelationFold(dominantStart, dominantEnd)));
        Assert.Equal((-10L, -10L), Read(new BootstrapRelationFold(recessiveStart, recessiveEnd)));
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
        Assert.Equal((4L, 4L), Fold(unit));
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
        Assert.Equal((4L, 4L), Fold(relation));
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
        Assert.Equal((8L, 4L), Fold(outer));
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

            Assert.Equal((4L, 4L), Read(reused));

            BootstrapMeasure.Offset = 50;

            var reusedLeft = Assert.IsType<BootstrapRelation>(reused.Left);
            var reusedRight = Assert.IsType<BootstrapRelation>(reused.Right);

            Assert.Same(reusedLeft, reusedRight);
            Assert.Equal((4L, 4L), Fold(reusedLeft));
            Assert.Equal((4L, 4L), Read(reused));
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
    public void FoldResultIsItselfAnElementWithNumeratorAndDenominator()
    {
        var fold = new BootstrapRelationFold(new BootstrapMeasure(10), new BootstrapMeasure(14));

        Assert.IsAssignableFrom<Core4.Experimental.Bootstrap.IElement>(fold);
        Assert.Equal((4L, 4L), Read(fold));
        Assert.Equal((4L, 0L), ReadElement(fold.Numerator));
        Assert.Equal((4L, 0L), ReadElement(fold.Denominator));
        var reduced = Assert.IsType<BootstrapRelation>(fold.Reduced);
        Assert.Equal(4, Assert.IsType<BootstrapMeasure>(reduced.Left).Raw);
        Assert.Equal(4, Assert.IsType<BootstrapMeasure>(reduced.Right).Raw);
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
        Assert.Equal((4L, 4L), Read(reducedRight));
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
                new BootstrapPartitionStep(0, 4),
            },
            transform.Partitions().ToArray());

        Assert.Equal(
            new[]
            {
                new BootstrapPartitionStep(0, 4),
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

    private static (long Numerator, long Denominator) Fold(BootstrapRelation relation) =>
        Read(new BootstrapRelationFold(relation.Left, relation.Right));

    private static (long Numerator, long Denominator) Read(BootstrapRelationFold fold) =>
        (Assert.IsType<BootstrapMeasure>(fold.Numerator).Raw, Assert.IsType<BootstrapMeasure>(fold.Denominator).Raw);

    private static (long Numerator, long Denominator) ReadElement(Core4.Experimental.Bootstrap.IElement element) =>
        element is BootstrapRelationFold fold
            ? Read(fold)
            : (Assert.IsType<BootstrapMeasure>(element).Raw, 0);
}
