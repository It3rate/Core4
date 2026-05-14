using Gojo.Core;
using Gojo.Equations;

namespace Gojo.Tests.Equations;

public sealed class EquationHelperTests
{
    [Fact]
    public void AffinePredictionProjectsInputThroughOutputBasis()
    {
        var input = new Carrier();
        var output = new Carrier();
        var equation = Equation.Affine(input, 0, 10, output, 0, 100);
        input.Set(3);

        var prediction = Equation.Predict(equation, input.ValueSlot, output);

        Assert.True(prediction.TryGetCarrierLocations(out var predicted, out var residue));
        Assert.Equal(new CarrierLocation(output, 30), predicted);
        Assert.Equal(new CarrierLocation(input, 0), residue);
    }

    [Fact]
    public void AffinePredictionPreservesUnrepresentedSourceResidue()
    {
        var input = new Carrier();
        var output = new Carrier();
        var equation = Equation.Affine(input, 0, 10, output, 0, 3);
        input.Set(7);

        var prediction = Equation.Predict(equation, input.ValueSlot, output);

        Assert.True(prediction.TryGetCarrierLocations(out var predicted, out var residue));
        Assert.Equal(new CarrierLocation(output, 2), predicted);
        Assert.Equal(new CarrierLocation(input, 1), residue);
    }

    [Fact]
    public void MapCreatesIndexedPredictionSamples()
    {
        var root = new Carrier();
        var input = new Carrier();
        var output = new Carrier();
        var equation = Equation.Affine(input, 0, 10, output, 0, 100);
        var inputs = new[]
        {
            new CarrierLocation(input, 0),
            new CarrierLocation(input, 5),
            new CarrierLocation(input, 10)
        };

        var map = Equation.Map(root, equation, inputs, output, out var samples);

        Assert.True(map.TryReadOrderedElementsOn(root, out var elements));
        Assert.Equal(5, elements.Length);
        Assert.True(Equation.TryReadMapSamples(map, out var readSamples));
        Assert.Equal(samples, readSamples);
        Assert.True(Equation.TryReadPredictionSample(samples[1], out var index, out var sampleInput, out var predicted, out var residue));
        Assert.Equal(1, index.Ticks);
        Assert.Equal(inputs[1], sampleInput);
        Assert.Equal(new CarrierLocation(output, 50), predicted);
        Assert.Equal(new CarrierLocation(input, 0), residue);
    }

    [Fact]
    public void CompareCreatesIndexedErrorSamples()
    {
        var root = new Carrier();
        var output = new Carrier();
        var error = new Carrier();
        var predictions = new[]
        {
            new CarrierLocation(output, 10),
            new CarrierLocation(output, 25)
        };
        var observations = new[]
        {
            new CarrierLocation(output, 12),
            new CarrierLocation(output, 20)
        };

        var comparison = Equation.Compare(root, predictions, observations, error, out var samples);

        Assert.True(Equation.TryReadComparisonSamples(comparison, out var readSamples));
        Assert.Equal(samples, readSamples);
        Assert.True(Equation.TryReadComparisonSample(samples[0], out var firstIndex, out _, out _, out var firstError));
        Assert.True(Equation.TryReadComparisonSample(samples[1], out var secondIndex, out _, out _, out var secondError));
        Assert.Equal(0, firstIndex.Ticks);
        Assert.Equal(new CarrierLocation(error, 2), firstError);
        Assert.Equal(1, secondIndex.Ticks);
        Assert.Equal(new CarrierLocation(error, -5), secondError);
    }
}
