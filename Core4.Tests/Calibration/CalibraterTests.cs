using Core4.Calibration;
using Core4.Numerics;

namespace Core4.Tests.Calibration;

public sealed class CalibraterTests
{
    [Fact]
    public void CalibraterUsesFirstThreeSamplesAsOriginUnitAndValue()
    {
        var calibrater = Calibrater.Start(71)
            .Input(10)
            .Input(14)
            .Input(17);

        Assert.Equal(new Measure(10), calibrater.OriginMeasure);
        Assert.Equal(new Measure(14), calibrater.UnitMeasure);
        Assert.Equal(new Measure(17), calibrater.ValueMeasure);
        Assert.Equal(new Measure(4), calibrater.TickSize);
        Assert.Equal(new Ratio(new Measure(7), new Measure(4)), calibrater.ValueProjection);
    }

    [Fact]
    public void CalibraterShiftsWindowLeftWhenNewSamplesArrive()
    {
        var calibrater = Calibrater.Start(72)
            .Input(10)
            .Input(14)
            .Input(17)
            .Input(20);

        Assert.Equal(new Measure(14), calibrater.OriginMeasure);
        Assert.Equal(new Measure(17), calibrater.UnitMeasure);
        Assert.Equal(new Measure(20), calibrater.ValueMeasure);
        Assert.Equal(new Measure(3), calibrater.TickSize);
        Assert.Equal(new Ratio(new Measure(6), new Measure(3)), calibrater.ValueProjection);
    }

    [Fact]
    public void CalibraterProducesTensionWhenUnitSpanCollapsesToZero()
    {
        var calibrater = Calibrater.Start(73)
            .Input(12)
            .Input(12)
            .Input(17);

        Assert.True(calibrater.ValueProjection.IsTension);
        Assert.Equal(new Measure(5), calibrater.ValueProjection.Potential);
    }

    [Fact]
    public void CalibraterPressureRefinesUnitWhenSuccessiveValuesCollapseToZeroTicks()
    {
        var calibrater = Calibrater.Start(74)
            .Input(100)
            .Input(120)
            .Input(140)
            .InputShifted(
                145,
                new Ratio(new Measure(0), new Measure(100)),
                new Ratio(new Measure(10), new Measure(100)));

        Assert.Equal(new Measure(100), calibrater.OriginMeasure);
        Assert.Equal(new Measure(118), calibrater.UnitMeasure);
        Assert.Equal(new Measure(145), calibrater.ValueMeasure);
    }

    [Fact]
    public void CalibraterPressureCoarsensUnitWhenSuccessiveValuesExceedEightTicks()
    {
        var calibrater = Calibrater.Start(75)
            .Input(100)
            .Input(110)
            .Input(120)
            .InputShifted(
                1000,
                new Ratio(new Measure(0), new Measure(100)),
                new Ratio(new Measure(20), new Measure(100)));

        Assert.Equal(new Measure(100), calibrater.OriginMeasure);
        Assert.Equal(new Measure(112), calibrater.UnitMeasure);
        Assert.Equal(new Measure(1000), calibrater.ValueMeasure);
    }
}
