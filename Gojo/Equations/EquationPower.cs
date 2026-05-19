using Gojo.Core;
using Gojo.Structures;

namespace Gojo.Equations;

/// <summary>
/// Repeated-product equation foothold built as a repeat surface. Literal count
/// controllers can be read as powers today; non-literal controllers remain
/// inspectable repeat structure.
/// </summary>
public static class EquationPower
{
    /// <summary>Creates x^exponent as a repeat surface over a unit input port.</summary>
    public static Relation Create(
        Carrier root,
        Carrier input,
        Carrier output,
        int exponent) =>
        Create(root, input, output, RepeatSurface.Count(RepeatSurface.CreateAxis(new Carrier()), exponent));

    /// <summary>Creates a power-like repeat surface with an explicit controller.</summary>
    public static Relation Create(
        Carrier root,
        Carrier input,
        Carrier output,
        IElement controller)
    {
        var inputCarrier = RootOrUnknown(input);
        var outputCarrier = RootOrUnknown(output);
        var selected = Equation.UnitPort(inputCarrier);
        var transition = ProductTransition(inputCarrier, outputCarrier, ReadLiteralExponent(controller));
        return RepeatSurface.Create(
            RootOrNew(root),
            selected,
            controller ?? Relation.Point(Carrier.Unknown, 0),
            transition,
            Relation.Point(outputCarrier, 0),
            Relation.Point(Carrier.Unknown, 0));
    }

    /// <summary>Reads one input through a power repeat surface.</summary>
    public static Relation Predict(
        Relation power,
        CarrierLocation input,
        Carrier output,
        out Relation repeatRead)
    {
        if (!TryReadPowerSurface(
                power,
                out var selected,
                out var controller,
                out var transition,
                out _,
                out _,
                out _,
                out _,
                out var surfaceOutput))
        {
            repeatRead = power ?? Relation.Point(Carrier.Unknown, 0);
            return new Relation(new CarrierLocation(RootOrUnknown(output), 0), input);
        }

        var outputCarrier = surfaceOutput.IsUnknown ? RootOrUnknown(output) : surfaceOutput;
        if (!TryReadLiteralExponent(controller, out var exponent))
        {
            repeatRead = power;
            return new Relation(new CarrierLocation(outputCarrier, 0), input);
        }

        var inputCarrier = input.Carrier;
        var factorCarriers = CreateFactorCarriers(inputCarrier.UnitShape, exponent);
        var factorBranches = new Relation[exponent];
        var productPorts = new Relation[exponent];
        var stepReads = new Relation[exponent];
        var timelineSteps = new Relation[exponent];
        IElement previous = Relation.Point(Carrier.Unknown, 1);

        for (var index = 0; index < exponent; index++)
        {
            factorBranches[index] = Equation.Affine(inputCarrier, 0, 1, factorCarriers[index], 0, 1);
            productPorts[index] = Equation.UnitPort(factorCarriers[index]);
            var factorRead = Equation.Predict(factorBranches[index], input, factorCarriers[index]);
            stepReads[index] = factorRead;
            var partial = PartialProductLocation(outputCarrier, input.Ticks, index + 1);
            timelineSteps[index] = TimeStepSequence.Step(
                new Carrier(),
                inputCarrier,
                index,
                index + 1,
                previous,
                factorBranches[index],
                partial,
                factorRead.Right);
            previous = partial;
        }

        var prediction = exponent == 0
            ? new Relation(new CarrierLocation(outputCarrier, 1), input)
            : Equation.Predict(Equation.Product(outputCarrier, productPorts), ProductInput(stepReads), outputCarrier);
        var feedback = exponent == 0
            ? Relation.OrderedGroup(new Carrier(), Relation.Point(outputCarrier, 1))
            : Relation.OrderedGroup(new Carrier(), [.. stepReads.Select(read => read.Left)]);
        var timeline = TimeStepSequence.Create(
            new Carrier(),
            inputCarrier,
            Relation.Point(outputCarrier, 1),
            timelineSteps);

        repeatRead = RepeatSurface.Create(
            new Carrier(),
            selected,
            controller,
            transition,
            feedback,
            prediction.Right,
            timeline);
        return prediction;
    }

    /// <summary>Attempts to read and predict a power surface while keeping the repeat read available.</summary>
    public static bool TryPredict(
        Relation power,
        IElement input,
        Carrier output,
        out Relation prediction,
        out Relation repeatRead)
    {
        if (!TryReadPowerSurface(
                power,
                out _,
                out _,
                out _,
                out _,
                out _,
                out _,
                out _,
                out var surfaceOutput))
        {
            prediction = null!;
            repeatRead = null!;
            return false;
        }

        if (input is CarrierLocation location)
        {
            prediction = Predict(power, location, output, out repeatRead);
            return true;
        }

        var target = surfaceOutput.IsUnknown ? RootOrUnknown(output) : surfaceOutput;
        repeatRead = power;
        prediction = new Relation(new CarrierLocation(target, 0), input ?? Relation.Point(Carrier.Unknown, 0));
        return true;
    }

    /// <summary>Attempts to read and predict a power surface.</summary>
    public static bool TryPredict(
        Relation power,
        IElement input,
        Carrier output,
        out Relation prediction) =>
        TryPredict(power, input, output, out prediction, out _);

    /// <summary>Reads the repeat surface fields for inspection.</summary>
    public static bool TryRead(
        Relation power,
        out IElement selected,
        out IElement controller,
        out IElement transition,
        out IElement feedback,
        out IElement residue,
        out IElement timeline) =>
        RepeatSurface.TryRead(power, out selected, out controller, out transition, out feedback, out residue, out timeline);

    /// <summary>Reads a repeat surface only when its structure is power-like.</summary>
    public static bool TryReadPowerSurface(
        Relation power,
        out IElement selected,
        out IElement controller,
        out IElement transition,
        out IElement feedback,
        out IElement residue,
        out IElement timeline,
        out Carrier input,
        out Carrier output)
    {
        if (TryRead(power, out selected, out controller, out transition, out feedback, out residue, out timeline) &&
            TryReadUnitPort(selected, out input) &&
            TryReadTransitionOutput(transition, controller, out output))
        {
            return true;
        }

        selected = default!;
        controller = default!;
        transition = default!;
        feedback = default!;
        residue = default!;
        timeline = default!;
        input = null!;
        output = null!;
        return false;
    }

    /// <summary>Reads a literal exponent from a controller span.</summary>
    public static bool TryReadLiteralExponent(IElement controller, out int exponent)
    {
        if (RepeatSurface.TryReadSpan(controller, out var start, out var end) &&
            start.Ticks == 0 &&
            end.Ticks >= 0 &&
            end.Ticks <= int.MaxValue)
        {
            exponent = checked((int)end.Ticks);
            return true;
        }

        exponent = 0;
        return false;
    }

    private static int ReadLiteralExponent(IElement controller) =>
        TryReadLiteralExponent(controller, out var exponent) ? exponent : 0;

    private static bool TryReadTransitionOutput(IElement transition, IElement controller, out Carrier output)
    {
        if (TryReadLiteralExponent(controller, out var exponent))
        {
            return exponent <= 0
                ? TryReadConstantOutput(transition, 1, out output)
                : TryReadProductTransition(transition, exponent, out output);
        }

        return TryReadConstantOutput(transition, 1, out output) ||
            TryReadProductTransition(transition, expectedCount: null, out output);
    }

    private static bool TryReadConstantOutput(IElement transition, long value, out Carrier output)
    {
        if (transition is Relation point &&
            point.Left is CarrierLocation location &&
            point.Right is Carrier carrier &&
            location.Carrier.SameIdentityAs(carrier) &&
            location.Ticks == value)
        {
            output = carrier;
            return true;
        }

        output = null!;
        return false;
    }

    private static bool TryReadProductTransition(IElement transition, int? expectedCount, out Carrier output)
    {
        if (transition is Relation product &&
            product.Right is Relation outputFrame &&
            TimeStepSequence.TryReadSpan(outputFrame, out var outputStart, out var outputEnd) &&
            outputStart.Ticks == 0 &&
            outputEnd.Ticks == 1 &&
            TryReadPortGroup(product.Left, expectedCount, out var ports) &&
            ports.Length > 0)
        {
            output = outputEnd.Carrier;
            return true;
        }

        output = null!;
        return false;
    }

    private static bool TryReadPortGroup(IElement element, int? expectedCount, out Relation[] ports)
    {
        if (expectedCount is { } count)
        {
            var collected = new Relation[count];
            if (TryReadPortGroup(element, count, collected, 0))
            {
                ports = collected;
                return true;
            }

            ports = [];
            return false;
        }

        var dynamicPorts = new List<Relation>();
        if (TryCollectLeftAssociatedPorts(element, dynamicPorts) && dynamicPorts.Count > 0)
        {
            ports = [.. dynamicPorts];
            return true;
        }

        ports = [];
        return false;
    }

    private static bool TryReadPortGroup(IElement element, int count, Relation[] ports, int offset)
    {
        if (count == 1)
        {
            if (element is Relation port && TryReadUnitPort(port, out _))
            {
                ports[offset] = port;
                return true;
            }

            return false;
        }

        return element is Relation group &&
            TryReadPortGroup(group.Left, count - 1, ports, offset) &&
            TryReadPortGroup(group.Right, 1, ports, offset + count - 1);
    }

    private static bool TryCollectLeftAssociatedPorts(IElement element, List<Relation> ports)
    {
        if (element is not Relation relation)
        {
            return false;
        }

        if (TryReadUnitPort(relation, out _))
        {
            ports.Add(relation);
            return true;
        }

        return TryCollectLeftAssociatedPorts(relation.Left, ports) &&
            TryCollectLeftAssociatedPorts(relation.Right, ports);
    }

    private static bool TryReadUnitPort(IElement element, out Carrier input)
    {
        if (element is Relation port &&
            port.Left is Relation inputFrame &&
            port.Right is Relation outputFrame &&
            TimeStepSequence.TryReadSpan(inputFrame, out var inputStart, out var inputEnd) &&
            TimeStepSequence.TryReadSpan(outputFrame, out var outputStart, out var outputEnd) &&
            inputStart.Ticks == 0 &&
            inputEnd.Ticks == 1 &&
            outputStart.Ticks == 0 &&
            outputEnd.Ticks == 1)
        {
            input = inputEnd.Carrier;
            return true;
        }

        input = null!;
        return false;
    }

    private static Relation ProductTransition(Carrier input, Carrier output, int exponent)
    {
        var count = Math.Max(0, exponent);
        if (count == 0)
        {
            return Equation.Constant(output, 1);
        }

        var carriers = CreateFactorCarriers(input.UnitShape, count);
        var ports = new Relation[count];
        for (var index = 0; index < count; index++)
        {
            ports[index] = Equation.UnitPort(carriers[index]);
        }

        return Equation.Product(output, ports);
    }

    private static Carrier[] CreateFactorCarriers(UnitShape unitShape, int count)
    {
        var carriers = new Carrier[Math.Max(0, count)];
        for (var index = 0; index < carriers.Length; index++)
        {
            carriers[index] = new Carrier(unitShape);
        }

        return carriers;
    }

    private static IElement ProductInput(IReadOnlyList<Relation> factorReads)
    {
        if (factorReads.Count == 0)
        {
            return Relation.Point(Carrier.Unknown, 1);
        }

        IElement input = factorReads[0].Left;
        for (var index = 1; index < factorReads.Count; index++)
        {
            input = new Relation(input, factorReads[index].Left);
        }

        return input;
    }

    private static CarrierLocation PartialProductLocation(Carrier output, long inputTicks, int exponent)
    {
        var value = 1L;
        for (var index = 0; index < exponent; index++)
        {
            value = checked(value * inputTicks);
        }

        return new CarrierLocation(output, value);
    }

    private static Carrier RootOrUnknown(Carrier carrier) =>
        carrier ?? Carrier.Unknown;

    private static Carrier RootOrNew(Carrier carrier) =>
        carrier ?? new Carrier();
}
