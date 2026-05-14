using Gojo.Core;
using Gojo.Equations;

namespace Gojo.Tests.Equations;

public sealed class EquationCatalogTests
{
    [Fact]
    public void AffineEquationNormalizesSensorRange()
    {
        var sensor = new Carrier();
        var percent = new Carrier();
        var normalization = Equation.Affine(sensor, 0, 1024, percent, 0, 100);
        sensor.Set(512);

        var prediction = Equation.Predict(normalization, sensor.ValueSlot, percent);

        Assert.True(prediction.TryGetCarrierLocations(out var normalized, out var residue));
        Assert.Equal(new CarrierLocation(percent, 50), normalized);
        Assert.Equal(new CarrierLocation(sensor, 0), residue);
    }

    [Fact]
    public void AffineEquationModelsOpposedGearRatio()
    {
        var driver = new Carrier();
        var driven = new Carrier();
        var ratio = Equation.Affine(driver, 0, 12, driven, 0, -24);
        driver.Set(6);

        var prediction = Equation.Predict(ratio, driver.ValueSlot, driven);

        Assert.True(prediction.TryGetCarrierLocations(out var rotation, out var residue));
        Assert.Equal(new CarrierLocation(driven, -12), rotation);
        Assert.Equal(new CarrierLocation(driver, 0), residue);
    }

    [Fact]
    public void AffineEquationReadsOffsetRangeAsSignedDeviation()
    {
        var heat = new Carrier();
        var deviation = new Carrier();
        var centered = Equation.Affine(heat, 40, 80, deviation, -100, 100);
        heat.Set(60);

        var prediction = Equation.Predict(centered, heat.ValueSlot, deviation);

        Assert.True(prediction.TryGetCarrierLocations(out var centeredValue, out var residue));
        Assert.Equal(new CarrierLocation(deviation, 0), centeredValue);
        Assert.Equal(new CarrierLocation(heat, 0), residue);
    }

    [Fact]
    public void AffineStreamMapAndComparisonExposeFitErrors()
    {
        var input = new Carrier();
        var output = new Carrier();
        var error = new Carrier();
        var equation = Equation.Affine(input, 0, 10, output, 0, 100);
        var inputs = new[]
        {
            new CarrierLocation(input, 0),
            new CarrierLocation(input, 5),
            new CarrierLocation(input, 10)
        };
        var observed = new[]
        {
            new CarrierLocation(output, 0),
            new CarrierLocation(output, 48),
            new CarrierLocation(output, 105)
        };

        Equation.Map(new Carrier(), equation, inputs, output, out var predictions);
        var predictedLocations = predictions
            .Select(sample =>
            {
                Assert.True(Equation.TryReadPredictionSample(sample, out _, out _, out var predicted, out _));
                return predicted;
            })
            .ToArray();
        Equation.Compare(new Carrier(), predictedLocations, observed, error, out var comparisons);

        Assert.True(Equation.TryReadComparisonSample(comparisons[0], out _, out _, out _, out var firstError));
        Assert.True(Equation.TryReadComparisonSample(comparisons[1], out _, out _, out _, out var secondError));
        Assert.True(Equation.TryReadComparisonSample(comparisons[2], out _, out _, out _, out var thirdError));
        Assert.Equal(new CarrierLocation(error, 0), firstError);
        Assert.Equal(new CarrierLocation(error, -2), secondError);
        Assert.Equal(new CarrierLocation(error, 5), thirdError);
    }
}
