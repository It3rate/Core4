using Gojo.Core;

namespace Gojo.Equations;

/// <summary>
/// Construction and single-sample read helpers for relation-shaped equations.
/// A relation-shaped equation is a Gojo surface: frames, operand ports, and
/// output participation remain visible as carrier/relation structure.
/// </summary>
public static class Equation
{
    /// <summary>Creates an affine frame surface from explicit input and output frames.</summary>
    public static Relation Affine(Relation inputFrame, Relation outputFrame) =>
        new(inputFrame ?? Relation.Point(Carrier.Unknown, 0), outputFrame ?? Relation.Point(Carrier.Unknown, 0));

    /// <summary>
    /// Creates an affine frame surface. The start/end values are basis endpoints,
    /// not consecutive samples.
    /// </summary>
    public static Relation Affine(
        Carrier input,
        long inputStart,
        long inputEnd,
        Carrier output,
        long outputStart,
        long outputEnd) =>
        Affine(
            Relation.Segment(RootOrUnknown(input), inputStart, inputEnd),
            Relation.Segment(RootOrUnknown(output), outputStart, outputEnd));

    /// <summary>Creates an operand port that projects one input frame into the equation reader basis.</summary>
    public static Relation WeightedPort(
        Carrier input,
        long inputStart,
        long inputEnd,
        long outputStart,
        long outputEnd) =>
        new(
            Relation.Segment(RootOrUnknown(input), inputStart, inputEnd),
            Relation.Segment(Carrier.Unknown, outputStart, outputEnd));

    /// <summary>Creates a unit operand port for additive or product operation surfaces.</summary>
    public static Relation UnitPort(Carrier input) =>
        WeightedPort(input, 0, 1, 0, 1);

    /// <summary>Creates an additive operation surface over relation-shaped operand input.</summary>
    public static Relation Sum(Carrier output, params Relation[] operandPorts) =>
        new(PortGroup(operandPorts), Relation.Point(RootOrUnknown(output), 1));

    /// <summary>Creates a product operation surface over relation-shaped operand input.</summary>
    public static Relation Product(Carrier output, params Relation[] operandPorts) =>
        new(PortGroup(operandPorts), Relation.Segment(RootOrUnknown(output), 0, 1));

    /// <summary>
    /// Creates an error surface that reads relation-shaped input as observed - predicted.
    /// </summary>
    public static Relation Difference(Carrier predicted, Carrier observed, Carrier error) =>
        Sum(
            error,
            WeightedPort(predicted, 0, 1, 0, -1),
            WeightedPort(observed, 0, 1, 0, 1));

    /// <summary>Projects one input sample through an equation into an output carrier.</summary>
    public static Relation Predict(Relation equation, CarrierLocation input, Carrier output) =>
        equation is null
            ? new Relation(new CarrierLocation(RootOrUnknown(output), 0), input)
            : equation.Project(input, RootOrUnknown(output));

    /// <summary>Reads one scalar or relation-shaped input sample through an equation.</summary>
    public static Relation Predict(Relation equation, IElement input, Carrier output)
    {
        if (equation is null)
        {
            return new Relation(new CarrierLocation(RootOrUnknown(output), 0), input ?? Relation.Point(Carrier.Unknown, 0));
        }

        if (equation.TryReadOperationFrom(input, RootOrUnknown(output), out var operationRead))
        {
            return operationRead;
        }

        return input is CarrierLocation location
            ? Predict(equation, location, output)
            : new Relation(new CarrierLocation(RootOrUnknown(output), 0), input);
    }

    /// <summary>Projects one input sample only when the equation surface accepts it.</summary>
    public static bool TryPredict(
        Relation equation,
        CarrierLocation input,
        Carrier output,
        out Relation prediction)
    {
        if (equation is not null && equation.TryProject(input, RootOrUnknown(output), out prediction))
        {
            return true;
        }

        prediction = new Relation(new CarrierLocation(RootOrUnknown(output), 0), input);
        return false;
    }

    /// <summary>Reads one scalar or relation-shaped input sample only when the equation accepts it.</summary>
    public static bool TryPredict(
        Relation equation,
        IElement input,
        Carrier output,
        out Relation prediction)
    {
        if (equation is not null &&
            (equation.TryReadOperationFrom(input, RootOrUnknown(output), out prediction) ||
                (input is CarrierLocation location && equation.TryProject(location, RootOrUnknown(output), out prediction))))
        {
            return true;
        }

        prediction = new Relation(new CarrierLocation(RootOrUnknown(output), 0), input ?? Relation.Point(Carrier.Unknown, 0));
        return false;
    }

    /// <summary>Reads a prediction as target output plus residue, preserving relation-shaped residue.</summary>
    public static bool TryReadPrediction(Relation prediction, out CarrierLocation predicted, out IElement residue)
    {
        if (prediction?.Left is CarrierLocation target)
        {
            predicted = target;
            residue = prediction.Right;
            return true;
        }

        predicted = default;
        residue = default!;
        return false;
    }

    private static IElement PortGroup(IReadOnlyList<Relation> ports)
    {
        if (CountOf(ports) == 0)
        {
            return new Relation(Relation.Point(Carrier.Unknown, 1), Relation.Point(Carrier.Unknown, 1));
        }

        IElement group = ports[0] ?? Relation.Point(Carrier.Unknown, 0);
        for (var index = 1; index < ports.Count; index++)
        {
            group = new Relation(group, ports[index] ?? Relation.Point(Carrier.Unknown, 0));
        }

        return group;
    }

    private static int CountOf<T>(IReadOnlyList<T> values) =>
        values?.Count ?? 0;

    private static Carrier RootOrUnknown(Carrier carrier) =>
        carrier ?? Carrier.Unknown;
}
