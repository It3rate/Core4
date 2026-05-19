using Gojo.Core;
using Gojo.Equations;
using Gojo.Structures;

namespace Gojo.Tests.Equations;

public sealed class EquationPowerTests
{
    [Fact]
    public void LiteralPowerPredictsRepeatedProductAndRecordsRepeatRead()
    {
        var x = new Carrier();
        var output = new Carrier();
        var power = EquationPower.Create(new Carrier(), x, output, exponent: 3);

        var prediction = EquationPower.Predict(
            power,
            new CarrierLocation(x, 2),
            output,
            out var repeatRead);

        Assert.True(Equation.TryReadPrediction(prediction, out var predicted, out _));
        Assert.Equal(new CarrierLocation(output, 8), predicted);
        Assert.True(EquationPower.TryRead(
            repeatRead,
            out var selected,
            out var controller,
            out var transition,
            out var feedback,
            out var residue,
            out var timeline));
        Assert.Equal(Equation.UnitPort(x), selected);
        Assert.True(EquationPower.TryReadLiteralExponent(controller, out var exponent));
        Assert.Equal(3, exponent);
        Assert.True(RepeatSurface.TryReadSpan(controller, out var start, out var end));
        Assert.Equal(new CarrierLocation(start.Carrier, 0), start);
        Assert.Equal(new CarrierLocation(start.Carrier, 3), end);
        Assert.IsType<Relation>(transition);
        Assert.IsType<Relation>(residue);

        var factorOutputs = OrderedElements(Assert.IsType<Relation>(feedback));
        Assert.Equal(new[] { 2L, 2L, 2L }, factorOutputs.Cast<CarrierLocation>().Select(location => location.Ticks).ToArray());

        var timelineRelation = Assert.IsType<Relation>(timeline);
        Assert.True(TimeStepSequence.TryRead(timelineRelation, out var origin, out var initial, out var steps));
        Assert.Equal(new CarrierLocation(x, 0), origin);
        Assert.Equal(Relation.Point(output, 1), initial);
        Assert.Equal(3, steps.Length);
        Assert.True(TimeStepSequence.TryReadStep(
            steps[2],
            out _,
            out var lastStart,
            out var lastEnd,
            out var before,
            out _,
            out var after,
            out _));
        Assert.Equal(new CarrierLocation(x, 2), lastStart);
        Assert.Equal(new CarrierLocation(x, 3), lastEnd);
        Assert.Equal(new CarrierLocation(output, 4), before);
        Assert.Equal(new CarrierLocation(output, 8), after);
    }

    [Fact]
    public void GenericEquationPredictionReadsLiteralPowerBeforeSequenceShape()
    {
        var x = new Carrier();
        var output = new Carrier();
        var power = EquationPower.Create(new Carrier(), x, output, exponent: 3);

        var prediction = Equation.Predict(power, new CarrierLocation(x, 4), Carrier.Unknown);

        Assert.True(Equation.TryReadPrediction(prediction, out var predicted, out _));
        Assert.Equal(new CarrierLocation(output, 64), predicted);
    }

    [Fact]
    public void ZeroExponentPredictsUnitAndEmptyTimeline()
    {
        var x = new Carrier();
        var output = new Carrier();
        var power = EquationPower.Create(new Carrier(), x, output, exponent: 0);

        var prediction = EquationPower.Predict(
            power,
            new CarrierLocation(x, 9),
            output,
            out var repeatRead);

        Assert.True(Equation.TryReadPrediction(prediction, out var predicted, out _));
        Assert.Equal(new CarrierLocation(output, 1), predicted);
        Assert.True(EquationPower.TryRead(repeatRead, out _, out var controller, out _, out _, out _, out var timeline));
        Assert.True(EquationPower.TryReadLiteralExponent(controller, out var exponent));
        Assert.Equal(0, exponent);
        Assert.True(TimeStepSequence.TryRead(Assert.IsType<Relation>(timeline), out _, out _, out var steps));
        Assert.Empty(steps);
    }

    [Fact]
    public void EquationControllerDerivesExponentFromPrediction()
    {
        var x = new Carrier();
        var output = new Carrier();
        var countInput = new Carrier();
        var countOutput = new Carrier();
        var countEquation = Equation.Affine(countInput, 0, 1, countOutput, 0, 1);
        var controller = EquationPower.CountFromEquation(
            new Carrier(),
            new Carrier(),
            countEquation,
            new CarrierLocation(countInput, 3),
            countOutput);
        var power = EquationPower.Create(new Carrier(), x, output, controller);

        Assert.True(EquationPower.TryReadResolvedExponent(controller, out var exponent));
        Assert.Equal(3, exponent);

        var prediction = EquationPower.Predict(
            power,
            new CarrierLocation(x, 2),
            output,
            out var repeatRead);

        Assert.True(Equation.TryReadPrediction(prediction, out var predicted, out _));
        Assert.Equal(new CarrierLocation(output, 8), predicted);
        Assert.True(EquationPower.TryRead(repeatRead, out _, out var readController, out _, out _, out _, out _));
        Assert.Equal(controller, readController);
    }

    [Fact]
    public void ResolutionSearchControllerUsesSelectedInputAsExponent()
    {
        var countInput = new Carrier();
        var countOutput = new Carrier();
        var identity = Equation.Affine(countInput, 0, 1, countOutput, 0, 1);
        var search = EquationResolutionSearch.Search(
            new Carrier(),
            identity,
            countInput,
            countOutput,
            targetTicks: 3,
            startTicks: 0,
            endTicks: 5,
            candidateCount: 6,
            roundCount: 1,
            out _);
        var controller = EquationPower.CountFromResolutionSearch(new Carrier(), new Carrier(), search);
        var x = new Carrier();
        var output = new Carrier();
        var power = EquationPower.Create(new Carrier(), x, output, controller);

        Assert.True(EquationPower.TryReadResolvedExponent(controller, out var exponent));
        Assert.Equal(3, exponent);

        var prediction = Equation.Predict(power, new CarrierLocation(x, 2), output);

        Assert.True(Equation.TryReadPrediction(prediction, out var predicted, out _));
        Assert.Equal(new CarrierLocation(output, 8), predicted);
    }

    [Fact]
    public void ClaimControllerUsesReadableViolationMagnitudeAsExponent()
    {
        var residual = new Carrier();
        var measured = new Carrier();
        var claim = OperationClaim.AtMost(
            new Carrier(),
            new CarrierLocation(measured, 5),
            new CarrierLocation(measured, 2),
            residual);
        var x = new Carrier();
        var output = new Carrier();
        var power = EquationPower.Create(new Carrier(), x, output, claim);

        Assert.True(EquationPower.TryReadResolvedExponent(claim, out var exponent));
        Assert.Equal(3, exponent);

        var prediction = Equation.Predict(power, new CarrierLocation(x, 2), output);

        Assert.True(Equation.TryReadPrediction(prediction, out var predicted, out _));
        Assert.Equal(new CarrierLocation(output, 8), predicted);
    }

    [Fact]
    public void NonLiteralControllerStaysInspectableWithoutPretendingToPredict()
    {
        var x = new Carrier();
        var output = new Carrier();
        var count = new Carrier();
        var controller = new Relation(Relation.Point(x, 0), Relation.Point(count, 0));
        var power = EquationPower.Create(new Carrier(), x, output, controller);

        Assert.True(EquationPower.TryRead(
            power,
            out var selected,
            out var readController,
            out _,
            out _,
            out _,
            out _));
        Assert.Equal(Equation.UnitPort(x), selected);
        Assert.Equal(controller, readController);
        Assert.False(EquationPower.TryReadLiteralExponent(readController, out _));

        var prediction = EquationPower.Predict(
            power,
            new CarrierLocation(x, 3),
            output,
            out var repeatRead);

        Assert.True(Equation.TryReadPrediction(prediction, out var predicted, out var residue));
        Assert.Equal(new CarrierLocation(output, 0), predicted);
        Assert.Equal(new CarrierLocation(x, 3), residue);
        Assert.Equal(power, repeatRead);
    }

    private static IElement[] OrderedElements(Relation structure)
    {
        var holder = Assert.IsType<Carrier>(structure.Right);
        Assert.True(structure.TryReadOrderedElementsOn(holder, out var elements));
        return elements;
    }
}
