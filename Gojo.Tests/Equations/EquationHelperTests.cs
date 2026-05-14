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

    [Fact]
    public void SumEquationPredictsWeightedRelationShapedInput()
    {
        var x = new Carrier();
        var y = new Carrier();
        var bias = new Carrier();
        var output = new Carrier();
        var sum = Equation.Sum(
            output,
            Equation.WeightedPort(x, 0, 1, 0, 2),
            Equation.WeightedPort(y, 0, 1, 0, 3),
            Equation.WeightedPort(bias, 0, 1, 0, 5));
        x.Set(3);
        y.Set(4);
        bias.Set(1);

        var prediction = Equation.Predict(sum, new Relation(new Relation(x.ValueSlot, y.ValueSlot), bias.ValueSlot), output);

        Assert.True(Equation.TryReadPrediction(prediction, out var predicted, out _));
        Assert.Equal(new CarrierLocation(output, 23), predicted);
    }

    [Fact]
    public void ProductEquationPredictsBilinearInput()
    {
        var width = new Carrier();
        var height = new Carrier();
        var area = new Carrier();
        var product = Equation.Product(area, Equation.UnitPort(width), Equation.UnitPort(height));
        width.Set(3);
        height.Set(4);

        var prediction = Equation.Predict(product, new Relation(width.ValueSlot, height.ValueSlot), area);

        Assert.True(Equation.TryReadPrediction(prediction, out var predicted, out _));
        Assert.Equal(new CarrierLocation(area, 12), predicted);
    }

    [Fact]
    public void MapPreservesRelationShapedInputsAndResidue()
    {
        var root = new Carrier();
        var x = new Carrier();
        var y = new Carrier();
        var output = new Carrier();
        var sum = Equation.Sum(output, Equation.UnitPort(x), Equation.UnitPort(y));
        IElement[] inputs =
        [
            new Relation(new CarrierLocation(x, 2), new CarrierLocation(y, 3)),
            new Relation(new CarrierLocation(x, 5), new CarrierLocation(y, 7))
        ];

        var map = Equation.Map(root, sum, inputs, output, out var samples);

        Assert.True(Equation.TryReadMapSamples(map, out var readSamples));
        Assert.Equal(samples, readSamples);
        Assert.True(Equation.TryReadPredictionSampleElement(samples[1], out var index, out var sampleInput, out var predicted, out var residue));
        Assert.Equal(1, index.Ticks);
        Assert.Equal(inputs[1], sampleInput);
        Assert.Equal(new CarrierLocation(output, 12), predicted);
        Assert.IsType<Relation>(residue);
    }
}
