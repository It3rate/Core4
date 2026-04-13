using Core4.Calibration;
using Core4.Numerics;

namespace Core4.Tests.Calibration;

public sealed class PatternStateTests
{
    [Fact]
    public void PatternStateBuildsRollingThreePointWindow()
    {
        var state = PatternState.Start(81)
            .Input(10)
            .Input(14)
            .Input(19);

        Assert.Equal(new Measure(10), state.OriginMeasure);
        Assert.Equal(new Measure(14), state.UnitMeasure);
        Assert.Equal(new Measure(19), state.ValueMeasure);
        Assert.Equal(new Measure(4), state.FirstDelta);
        Assert.Equal(new Measure(5), state.SecondDelta);
        Assert.Equal(new Measure(1), state.Momentum);
        Assert.Equal(new Measure(9), state.WindowRange);
        Assert.Equal(new Measure(13), state.RunningAverage);
        Assert.Equal(Measure.Zero, state.AdditiveFitPressure);
        Assert.Equal(Measure.Zero, state.MultiplicativeFitPressure);
    }

    [Fact]
    public void PatternStateSlidesWindowLeftOnLaterSamples()
    {
        var state = PatternState.Start(82)
            .Input(10)
            .Input(14)
            .Input(19)
            .Input(21);

        Assert.Equal(new Measure(14), state.OriginMeasure);
        Assert.Equal(new Measure(19), state.UnitMeasure);
        Assert.Equal(new Measure(21), state.ValueMeasure);
        Assert.Equal(new Measure(5), state.FirstDelta);
        Assert.Equal(new Measure(2), state.SecondDelta);
    }

    [Fact]
    public void PatternStateTracksRunningAverageWithCore4RatioWeight()
    {
        var state = PatternState.Start(83)
            .Input(100, new Ratio(1, 10))
            .Input(110, new Ratio(1, 10))
            .Input(130, new Ratio(1, 10));

        Assert.Equal(new Measure(103), state.RunningAverage);
    }

    [Fact]
    public void PatternStateDetectsReversalThroughSignedStreakAndStrength()
    {
        var state = PatternState.Start(84)
            .Input(10)
            .Input(15)
            .Input(12);

        Assert.Equal(new Measure(-1), state.DirectionStreak);
        Assert.Equal(Measure.Zero, state.StepsSinceLastReversal);
        Assert.Equal(new Measure(3), state.ReversalStrength);
        Assert.Equal(new Measure(2), state.OscillationResidual);
    }

    [Fact]
    public void PatternStateExposesLowResidualForLocallyLinearTrend()
    {
        var state = PatternState.Start(85)
            .Input(3)
            .Input(7)
            .Input(11);

        Assert.Equal(Measure.Zero, state.LinearResidual);
        Assert.Equal(Measure.Zero, state.ShockResidual);
        Assert.Equal(new Ratio(7, 3), state.FirstValueRatio);
        Assert.Equal(new Ratio(11, 7), state.SecondValueRatio);
        Assert.True(state.AdditiveFitPressure.Raw <= state.MultiplicativeFitPressure.Raw);
    }

    [Fact]
    public void PatternStateCarriesGeometricResidualAsRatioDifference()
    {
        var state = PatternState.Start(86)
            .Input(10)
            .Input(20)
            .Input(40);

        Assert.Equal(new Ratio(2, 1), state.FirstValueRatio.Canonicalize());
        Assert.Equal(new Ratio(2, 1), state.SecondValueRatio.Canonicalize());
        Assert.Equal(new Ratio(0, 1), state.GeometricResidual.Canonicalize());
        Assert.True(state.MultiplicativeFitPressure.Raw <= state.AdditiveFitPressure.Raw);
    }
}
