using Core4.Numerics;

namespace Core4.Tests.Numerics;

public sealed class MeasureTests
{
    [Fact]
    public void SignAndAbsoluteFollowRawDirection()
    {
        Assert.Equal(new Measure(-1), new Measure(-7).Sign);
        Assert.Equal(Measure.Zero, Measure.Zero.Sign);
        Assert.Equal(Measure.One, new Measure(9).Sign);

        Assert.Equal(new Measure(7), new Measure(-7).Absolute);
        Assert.Equal(new Measure(9), new Measure(9).Absolute);
    }

    [Fact]
    public void ArithmeticOperatorsPreserveOrderableMagnitude()
    {
        var left = new Measure(7);
        var right = new Measure(3);

        Assert.Equal(new Measure(10), left + right);
        Assert.Equal(new Measure(4), left - right);
        Assert.Equal(new Measure(-7), -left);
    }

    [Fact]
    public void ImplicitConversionsRoundTripRawValues()
    {
        Measure measure = 42L;
        long raw = measure;

        Assert.Equal(42L, raw);
        Assert.Equal(new Measure(42), measure);
    }
}
