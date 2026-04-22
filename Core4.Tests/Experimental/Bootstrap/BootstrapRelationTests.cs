using BootstrapMeasure = Core4.Experimental.Bootstrap.Measure;
using BootstrapRelationFold = Core4.Experimental.Bootstrap.RelationFold;
using BootstrapRelation = Core4.Experimental.Bootstrap.Relation;

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

            Assert.Equal((4L, 4L), BootstrapRelationFold.Fold(start, end));

            BootstrapMeasure.Offset = 100;

            Assert.Equal((4L, 4L), BootstrapRelationFold.Fold(start, end));
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

        Assert.Equal((0L, 0L), BootstrapRelationFold.Fold(point, point));
    }

    [Fact]
    public void RawMeasureLoopPreservesDirectionAcrossAllTicks()
    {
        var dominantStart = new BootstrapMeasure(10);
        var dominantEnd = new BootstrapMeasure(20);
        var recessiveStart = new BootstrapMeasure(20);
        var recessiveEnd = new BootstrapMeasure(10);

        Assert.Equal((10L, 10L), BootstrapRelationFold.Fold(dominantStart, dominantEnd));
        Assert.Equal((-10L, -10L), BootstrapRelationFold.Fold(recessiveStart, recessiveEnd));
    }

    [Fact]
    public void ZeroPartitionPointClaimsAppendWithoutMixing()
    {
        var open = new BootstrapRelation(new BootstrapMeasure(0), new BootstrapMeasure(0));
        var leftPoint = new BootstrapRelation(new BootstrapMeasure(8), open);
        var rightPoint = new BootstrapRelation(new BootstrapMeasure(12), open);

        Assert.Equal((8L, 0L), Fold(leftPoint));
        Assert.Equal((12L, 0L), Fold(rightPoint));
        Assert.Equal((20L, 0L), BootstrapRelationFold.Fold(leftPoint, rightPoint));
    }

    [Fact]
    public void PointClaimCanBeResolvedThroughSharedUnit()
    {
        var open = new BootstrapRelation(new BootstrapMeasure(0), new BootstrapMeasure(0));
        var point = new BootstrapRelation(new BootstrapMeasure(8), open);
        var unit = new BootstrapRelation(new BootstrapMeasure(10), new BootstrapMeasure(14));

        Assert.Equal((8L, 0L), Fold(point));
        Assert.Equal((4L, 4L), Fold(unit));
        Assert.Equal((8L, 4L), BootstrapRelationFold.Fold(point, unit));
    }

    [Fact]
    public void SharedBasisSmearingProducesMultiplicationLikeResult()
    {
        var open = new BootstrapRelation(new BootstrapMeasure(0), new BootstrapMeasure(0));
        var unit = new BootstrapRelation(new BootstrapMeasure(10), new BootstrapMeasure(14));
        var two = new BootstrapRelation(new BootstrapRelation(new BootstrapMeasure(8), open), unit);
        var three = new BootstrapRelation(new BootstrapRelation(new BootstrapMeasure(12), open), unit);

        Assert.Equal((8L, 4L), Fold(two));
        Assert.Equal((12L, 4L), Fold(three));
        Assert.Equal((24L, 4L), BootstrapRelationFold.Fold(two, three));
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
            var reused = new BootstrapRelation(unit, unit);

            Assert.Equal((4L, 4L), Fold(reused));

            BootstrapMeasure.Offset = 50;

            var reusedLeft = Assert.IsType<BootstrapRelation>(reused.Left);
            var reusedRight = Assert.IsType<BootstrapRelation>(reused.Right);

            Assert.Same(reusedLeft, reusedRight);
            Assert.Equal((4L, 4L), Fold(reusedLeft));
            Assert.Equal((4L, 4L), Fold(reused));
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

    private static (long Numerator, long Denominator) Fold(BootstrapRelation relation) =>
        BootstrapRelationFold.Fold(relation.Left, relation.Right);
}
