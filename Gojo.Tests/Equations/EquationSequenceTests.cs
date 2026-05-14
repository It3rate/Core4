using Gojo.Core;
using Gojo.Equations;

namespace Gojo.Tests.Equations;

public sealed class EquationSequenceTests
{
    [Fact]
    public void SequenceFeedsOneAffineReadIntoTheNext()
    {
        var input = new Carrier();
        var normalized = new Carrier();
        var output = new Carrier();
        var first = Equation.Affine(input, 0, 10, normalized, 0, 100);
        var second = Equation.Affine(normalized, 0, 100, output, 0, 5);
        var sequenceRoot = new Carrier();
        var sequence = EquationSequence.Create(sequenceRoot, first, second);

        var prediction = EquationSequence.Predict(sequence, new CarrierLocation(input, 6), output, out var stepReads);

        Assert.True(EquationSequence.TryReadSteps(sequence, out var steps));
        Assert.Equal(new[] { first, second }, steps);
        Assert.Equal(2, stepReads.Length);
        Assert.True(Equation.TryReadPrediction(stepReads[0], out var middle, out _));
        Assert.Equal(new CarrierLocation(normalized, 60), middle);
        Assert.True(Equation.TryReadPrediction(prediction, out var final, out var residue));
        Assert.Equal(new CarrierLocation(output, 3), final);
        Assert.IsType<Relation>(residue);
    }

    [Fact]
    public void SequenceCanFeedProductOutputIntoAffineRead()
    {
        var width = new Carrier();
        var height = new Carrier();
        var area = new Carrier();
        var score = new Carrier();
        var product = Equation.Product(area, Equation.UnitPort(width), Equation.UnitPort(height));
        var areaToScore = Equation.Affine(area, 0, 100, score, 0, 10);
        var sequence = EquationSequence.Create(new Carrier(), product, areaToScore);

        var prediction = EquationSequence.Predict(
            sequence,
            new Relation(new CarrierLocation(width, 8), new CarrierLocation(height, 5)),
            score,
            out var stepReads);

        Assert.Equal(2, stepReads.Length);
        Assert.True(Equation.TryReadPrediction(stepReads[0], out var productRead, out _));
        Assert.Equal(new CarrierLocation(area, 40), productRead);
        Assert.True(Equation.TryReadPrediction(prediction, out var final, out _));
        Assert.Equal(new CarrierLocation(score, 4), final);
    }

    [Fact]
    public void EmptySequencePreservesInputAsUnresolvedResidue()
    {
        var input = new Carrier();
        var output = new Carrier();
        var sequence = EquationSequence.Create(new Carrier());
        var supplied = new CarrierLocation(input, 7);

        var prediction = EquationSequence.Predict(sequence, supplied, output, out var stepReads);

        Assert.Empty(stepReads);
        Assert.True(Equation.TryReadPrediction(prediction, out var final, out var residue));
        Assert.Equal(new CarrierLocation(output, 0), final);
        Assert.Equal(supplied, residue);
    }
}
