using Core4.Numerics;

namespace Core4.Tests.Numerics;

public sealed class RatioTests
{
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
    public void NormalizePreservesResolutionAndOnlyFlipsNegativeDenominator()
    {
        Assert.Equal(new Ratio(5, 10), new Ratio(5, 10).Normalize());
        Assert.Equal(new Ratio(5, 10), new Ratio(-5, -10).Normalize());
        Assert.Equal(new Ratio(-5, 10), new Ratio(5, -10).Normalize());
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
    public void TensionArithmeticStaysUnresolvedAndAccumulatesNumerator()
    {
        var sum = Ratio.Tension(new Measure(3)).Add(Ratio.Tension(new Measure(4)));
        var difference = Ratio.Tension(new Measure(10)).Subtract(Ratio.Tension(new Measure(4)));

        Assert.Equal(new Ratio(7, 0), sum);
        Assert.Equal(new Ratio(6, 0), difference);
        Assert.True(sum.IsTension);
        Assert.True(difference.IsTension);
    }
}
