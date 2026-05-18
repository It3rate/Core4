using Gojo.Core;
using Gojo.Equations;
using Gojo.Patterns;

namespace Gojo.Tests.Equations;

public sealed class EquationPatternTests
{
    [Fact]
    public void CompleteEquationPatternPredictsThroughBoundSlots()
    {
        var xPort = new Placeholder();
        var yPort = UnitShape.DescriptorSlot(0);
        var outputSlot = new Placeholder();
        var xInput = new Placeholder();
        var yInput = UnitShape.DescriptorSlot(1);
        var x = new Carrier();
        var y = new Carrier();
        var result = new Carrier();
        var equationPattern = new Relation(new Relation(xPort, yPort), outputSlot);
        var inputPattern = new Relation(xInput, yInput);
        x.Set(3);
        y.Set(4);

        var complete = EquationPattern.TryPredict(
            equationPattern,
            inputPattern,
            result,
            out var prediction,
            out var unbound,
            StructuralPattern.Bind(xPort, Equation.UnitPort(x)),
            StructuralPattern.BindDescriptor(0, Equation.UnitPort(y)),
            StructuralPattern.Bind(outputSlot, Relation.Point(result, 1)),
            StructuralPattern.Bind(xInput, x.ValueSlot),
            StructuralPattern.BindDescriptor(1, y.ValueSlot));

        Assert.True(complete);
        Assert.Empty(unbound);
        Assert.True(Equation.TryReadPrediction(prediction, out var predicted, out _));
        Assert.Equal(new CarrierLocation(result, 7), predicted);
    }

    [Fact]
    public void PartialEquationPatternReportsUnboundInputSlotAsResidue()
    {
        var xPort = new Placeholder();
        var yPort = UnitShape.DescriptorSlot(0);
        var outputSlot = new Placeholder();
        var xInput = new Placeholder();
        var yInput = UnitShape.DescriptorSlot(1);
        var x = new Carrier();
        var y = new Carrier();
        var result = new Carrier();
        var equationPattern = new Relation(new Relation(xPort, yPort), outputSlot);
        var inputPattern = new Relation(xInput, yInput);
        x.Set(3);
        y.Set(4);

        var complete = EquationPattern.TryPredict(
            equationPattern,
            inputPattern,
            result,
            out var prediction,
            out var unbound,
            StructuralPattern.Bind(xPort, Equation.UnitPort(x)),
            StructuralPattern.BindDescriptor(0, Equation.UnitPort(y)),
            StructuralPattern.Bind(outputSlot, Relation.Point(result, 1)),
            StructuralPattern.Bind(xInput, x.ValueSlot));

        Assert.False(complete);
        Assert.Equal([yInput], unbound);
        Assert.True(Equation.TryReadPrediction(prediction, out var predicted, out var residue));
        Assert.Equal(new CarrierLocation(result, 0), predicted);
        Assert.Contains(yInput, StructuralPattern.ReadSlots(residue));
    }
}
