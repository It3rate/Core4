using Core4.Numerics;
using Core4.Structure;

namespace Core4.Tests.Structure;

public sealed class RelationAndBasisTests
{
    [Fact]
    public void RelationProjectsAndLocatesWithoutReducingTickResolution()
    {
        Carrier x = 1;
        var relation = new Relation(
            Site.Open(x, new Measure(100)),
            Site.Open(x, new Measure(110)));

        var projection = relation.Project(new Measure(107));
        var located = relation.Locate(projection, x);

        Assert.Equal(new Ratio(7, 10), projection);
        Assert.Equal(new Ratio(1070, 10), located);
        Assert.Equal(new Measure(107), located.Whole);
    }

    [Fact]
    public void AtomicBasisFreezeCreatesGradeZeroReusableUnit()
    {
        Carrier x = 1;
        var basis = Basis.Freeze(x, new Measure(0), new Measure(10));

        Assert.True(basis.IsAtomic);
        Assert.False(basis.IsComposite);
        Assert.Equal(Measure.Zero, basis.Grade);
        Assert.Equal(new Measure(1), basis.RelationCount);
        Assert.Equal(new Measure(1), basis.CarrierCount);
        Assert.Equal(new Measure(10), basis.Potential);
        Assert.Equal(new Ratio(10, 0), basis.Ratio);
        Assert.Equal(Measure.One, basis.Orientation);
    }

    [Fact]
    public void FrozenUnitCanIterateNormalizedTicks()
    {
        Carrier x = 1;
        var basis = Basis.Freeze(x, new Measure(0), new Measure(10));

        for (long tick = 0; tick <= 10; tick++)
        {
            Assert.Equal(new Ratio(new Measure(tick), new Measure(10)), basis.Project(new Measure(tick)));
        }
    }

    [Fact]
    public void FusingAtomicBasesIncreasesGradeWithCompositionDepth()
    {
        Carrier x = 1;
        Carrier y = 2;

        var recessive = Basis.Freeze(x, new Measure(0), new Measure(-10));
        var dominant = Basis.Freeze(x, new Measure(0), new Measure(10));
        var orthogonal = Basis.Freeze(y, new Measure(0), new Measure(10));

        var pair = Basis.Fuse(recessive, dominant);
        var triple = Basis.Fuse(recessive, dominant, orthogonal);

        Assert.Equal(new Measure(1), pair.Grade);
        Assert.Equal(new Measure(2), pair.RelationCount);

        Assert.Equal(new Measure(2), triple.Grade);
        Assert.Equal(new Measure(3), triple.RelationCount);
    }

    [Fact]
    public void OpposedSameCarrierPairFormsDirectedSegmentWithAdditivePotential()
    {
        Carrier x = 1;

        var recessive = Basis.Freeze(x, new Measure(0), new Measure(-10));
        var dominant = Basis.Freeze(x, new Measure(0), new Measure(10));
        var segment = Basis.Fuse(recessive, dominant);
        var midpoint = segment.Locate(new Ratio(1, 2), x);

        Assert.True(segment.IsAtomicPair);
        Assert.True(segment.IsSameCarrierPair());
        Assert.True(segment.IsOpposedPair());
        Assert.False(segment.IsAlignedPair());
        Assert.Equal(new Measure(20), segment.Potential);
        Assert.Equal(new Ratio(10, 10), segment.Ratio);
        Assert.Equal(new Ratio(0, 2), midpoint);
        Assert.Equal(Measure.Zero, midpoint.Whole);
    }

    [Fact]
    public void AlignedSameCarrierPairKeepsSelfInteractionMultiplicative()
    {
        Carrier x = 1;

        var dominant = Basis.Freeze(x, new Measure(0), new Measure(10));
        var dominantWide = Basis.Freeze(x, new Measure(0), new Measure(20));
        var self = Basis.Fuse(dominant, dominantWide);

        Assert.True(self.IsSameCarrierPair());
        Assert.True(self.IsAlignedPair());
        Assert.False(self.IsOpposedPair());
        Assert.Equal(new Measure(200), self.Potential);
        Assert.Equal(new Ratio(10, 20), self.Ratio);
    }

    [Theory]
    [InlineData(-10, -10, 0)]
    [InlineData(-10, 10, 1)]
    [InlineData(10, -10, 2)]
    [InlineData(10, 10, 3)]
    public void KernelCodeTracksRecessiveAndDominantOrientation(long leftAnchor, long rightAnchor, long expectedCode)
    {
        Carrier x = 1;
        Carrier y = 2;

        var left = Basis.Freeze(x, new Measure(0), new Measure(leftAnchor));
        var right = Basis.Freeze(y, new Measure(0), new Measure(rightAnchor));
        var fused = Basis.Fuse(left, right);

        Assert.Equal(new Measure(expectedCode), fused.KernelCode());
    }
}
