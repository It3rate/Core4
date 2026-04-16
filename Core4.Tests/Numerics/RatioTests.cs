using Core4.Numerics;

namespace Core4.Tests.Numerics;

public sealed class RatioTests
{
    [Fact]
    public void CompatibilityConstructorDefaultsOriginToZero()
    {
        var ratio = new Ratio(5, 10);

        Assert.Equal(Measure.Zero, ratio.Origin);
        Assert.Equal(new Measure(5), ratio.Value);
        Assert.Equal(new Measure(10), ratio.UnitValue);
        Assert.Equal(new Measure(5), ratio.Recessive);
        Assert.Equal(new Measure(10), ratio.Dominant);
    }

    [Fact]
    public void ExplicitOriginRetainsPinnedLocationsAndDerivedOffsets()
    {
        var ratio = new Ratio(new Measure(100), new Measure(103), new Measure(104));

        Assert.Equal(new Measure(100), ratio.Origin);
        Assert.Equal(new Measure(103), ratio.Value);
        Assert.Equal(new Measure(104), ratio.UnitValue);
        Assert.Equal(new Measure(3), ratio.Recessive);
        Assert.Equal(new Measure(4), ratio.Dominant);
    }

    [Fact]
    public void ZeroAndTensionHelpersEncodeExpectedDominantState()
    {
        var resolvedZero = Ratio.Zero(new Measure(10));
        var tension = Ratio.Tension(new Measure(5));

        Assert.True(resolvedZero.IsResolved);
        Assert.False(resolvedZero.IsTension);
        Assert.Equal(new Ratio(0, 10), resolvedZero);

        Assert.False(tension.IsResolved);
        Assert.True(tension.IsTension);
        Assert.Equal(new Ratio(5, 0), tension);
    }

    [Fact]
    public void NormalizePreservesRawDenominatorOrientation()
    {
        Assert.Equal(new Ratio(5, 10), new Ratio(5, 10).Normalize());
        Assert.Equal(new Ratio(-5, -10), new Ratio(-5, -10).Normalize());
        Assert.Equal(new Ratio(5, -10), new Ratio(5, -10).Normalize());
    }

    [Fact]
    public void FoldDenominatorIsExplicitAndLossy()
    {
        Assert.Equal(new Ratio(-5, 10), new Ratio(5, -10).FoldDenominator());
        Assert.Equal(new Ratio(5, 10), new Ratio(-5, -10).FoldDenominator());
        Assert.Equal(new Ratio(5, 0), Ratio.Tension(new Measure(5)).FoldDenominator());
    }

    [Fact]
    public void FoldDenominatorPreservesPinnedOrigin()
    {
        var ratio = new Ratio(new Measure(100), new Measure(105), new Measure(90));
        var folded = ratio.FoldDenominator();

        Assert.Equal(new Measure(100), folded.Origin);
        Assert.Equal(new Measure(95), folded.Value);
        Assert.Equal(new Measure(110), folded.UnitValue);
        Assert.Equal(new Measure(-5), folded.Recessive);
        Assert.Equal(new Measure(10), folded.Dominant);
    }

    [Fact]
    public void CanonicalizeReducesResolvedRatiosWithoutRelocatingSign()
    {
        Assert.Equal(new Ratio(3, 4), new Ratio(6, 8).Canonicalize());
        Assert.Equal(new Ratio(3, -4), new Ratio(6, -8).Canonicalize());
        Assert.Equal(new Ratio(0, 1), new Ratio(0, 10).Canonicalize());
        Assert.Equal(new Ratio(0, -1), new Ratio(0, -10).Canonicalize());
        Assert.Equal(new Ratio(5, 0), Ratio.Tension(new Measure(5)).Canonicalize());
    }

    [Fact]
    public void CanonicalizePreservesPinnedOrigin()
    {
        var ratio = new Ratio(new Measure(100), new Measure(106), new Measure(108));
        var canonical = ratio.Canonicalize();

        Assert.Equal(new Measure(100), canonical.Origin);
        Assert.Equal(new Measure(103), canonical.Value);
        Assert.Equal(new Measure(104), canonical.UnitValue);
        Assert.Equal(new Measure(3), canonical.Recessive);
        Assert.Equal(new Measure(4), canonical.Dominant);
    }

    [Fact]
    public void AddWithSharedDenominatorKeepsTickResolution()
    {
        var result = new Ratio(2, 10).Add(new Ratio(3, 10));

        Assert.Equal(new Ratio(5, 10), result);
    }

    [Fact]
    public void AddWithDifferentDenominatorsComposesWithoutReducing()
    {
        var result = new Ratio(1, 2).Add(new Ratio(1, 4));

        Assert.Equal(new Ratio(6, 8), result);
    }

    [Fact]
    public void MultiplyAndDivideKeepStructuralResolution()
    {
        var product = new Ratio(2, 10).Multiply(new Ratio(3, 10));
        var quotient = new Ratio(2, 10).Divide(new Ratio(5, 10));

        Assert.Equal(new Ratio(6, 100), product);
        Assert.Equal(new Ratio(20, 50), quotient);
    }

    [Fact]
    public void ArithmeticPreservesDenominatorOrientation()
    {
        var sum = new Ratio(1, -2).Add(new Ratio(1, 4));
        var product = new Ratio(2, -10).Multiply(new Ratio(3, 10));
        var quotient = new Ratio(2, -10).Divide(new Ratio(5, 10));

        Assert.Equal(new Ratio(2, -8), sum);
        Assert.Equal(new Ratio(6, -100), product);
        Assert.Equal(new Ratio(20, -50), quotient);
    }

    [Fact]
    public void StructuralEqualityDoesNotCollapseDenominatorSign()
    {
        Assert.NotEqual(new Ratio(1, -2), new Ratio(-1, 2));
        Assert.Equal(new Ratio(-1, 2), new Ratio(1, -2).FoldDenominator());
    }

    [Fact]
    public void TensionArithmeticStaysUnresolvedAndAccumulatesNumerator()
    {
        var sum = Ratio.Tension(new Measure(3)).Add(Ratio.Tension(new Measure(4)));
        var difference = Ratio.Tension(new Measure(10)).Subtract(Ratio.Tension(new Measure(4)));

        Assert.Equal(new Ratio(7, 0), sum);
        Assert.Equal(new Ratio(6, 0), difference);
        Assert.True(sum.IsTension);
        Assert.True(difference.IsTension);
    }

    [Fact]
    public void CompareMagnitudeCanSelectWeakerAndStrongerResolvedRatios()
    {
        var weaker = new Ratio(1, 2);
        var stronger = new Ratio(3, 4);

        Assert.True(weaker.CompareMagnitude(stronger) < 0);
        Assert.True(stronger.CompareMagnitude(weaker) > 0);
        Assert.Equal(weaker, weaker.Weaker(stronger));
        Assert.Equal(stronger, weaker.Stronger(stronger));
    }

    [Fact]
    public void CompareValueOrdersResolvedRatiosExactlyWithoutAssumingPositiveDenominator()
    {
        Assert.True(new Ratio(3, 4).CompareValue(new Ratio(2, 3)) > 0);
        Assert.True(new Ratio(1, -2).CompareValue(new Ratio(1, 4)) < 0);
        Assert.Equal(0, new Ratio(2, 4).CompareValue(new Ratio(1, 2)));
    }

    [Fact]
    public void CompareValueFallsBackToRawRecessiveForUnresolvedReads()
    {
        Assert.True(Ratio.Tension(new Measure(7)).CompareValue(Ratio.Tension(new Measure(5))) > 0);
        Assert.True(Ratio.Tension(new Measure(-3)).CompareValue(Ratio.Tension(new Measure(2))) < 0);
    }

    [Fact]
    public void CompareValueUsesBigIntegerForLargeResolvedCrossProducts()
    {
        var left = new Ratio(new Measure(long.MaxValue), new Measure(long.MaxValue - 1));
        var right = new Ratio(new Measure(long.MaxValue - 1), new Measure(long.MaxValue - 2));

        Assert.True(left.CompareValue(right) < 0);
    }

    [Fact]
    public void CompareValueIgnoresPinnedOriginWhenRelativeReadMatches()
    {
        var left = new Ratio(new Measure(100), new Measure(103), new Measure(104));
        var right = new Ratio(new Measure(-50), new Measure(-47), new Measure(-46));

        Assert.Equal(0, left.CompareValue(right));
    }

    [Fact]
    public void SharedOriginArithmeticKeepsPinnedOrigin()
    {
        var left = new Ratio(new Measure(100), new Measure(102), new Measure(110));
        var right = new Ratio(new Measure(100), new Measure(103), new Measure(110));

        Assert.Equal(new Ratio(new Measure(100), new Measure(105), new Measure(110)), left.Add(right));
    }

    [Fact]
    public void AddCanKeepExplicitOriginWhenOtherSideIsZeroOriginOffset()
    {
        var anchored = new Ratio(new Measure(100), new Measure(105), new Measure(110));
        var offset = new Ratio(2, 10);

        Assert.Equal(new Ratio(new Measure(100), new Measure(107), new Measure(110)), anchored.Add(offset));
        Assert.Equal(new Ratio(new Measure(100), new Measure(107), new Measure(110)), offset.Add(anchored));
    }

    [Fact]
    public void SubtractCanKeepExplicitOriginWhenOtherSideIsZeroOriginOffset()
    {
        var anchored = new Ratio(new Measure(100), new Measure(105), new Measure(110));
        var offset = new Ratio(2, 10);

        Assert.Equal(new Ratio(new Measure(100), new Measure(103), new Measure(110)), anchored.Subtract(offset));
    }

    [Fact]
    public void MultiplyAndDivideCanKeepExplicitOriginWhenScalingByZeroOriginFactor()
    {
        var anchored = new Ratio(new Measure(100), new Measure(105), new Measure(110));
        var factor = new Ratio(2, 1);
        var divisor = new Ratio(1, 2);

        Assert.Equal(new Ratio(new Measure(100), new Measure(110), new Measure(110)), anchored.Multiply(factor));
        Assert.Equal(new Ratio(new Measure(100), new Measure(110), new Measure(110)), anchored.Divide(divisor));
    }

    [Fact]
    public void ScaleByCanPartitionUnresolvedTensionWithResolvedShare()
    {
        var tension = Ratio.Tension(new Measure(11));
        var uptake = new Ratio(1, 4);

        Assert.Equal(new Ratio(2, 0), tension.ScaleBy(uptake));
    }
}
