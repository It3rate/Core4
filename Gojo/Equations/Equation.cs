using Gojo.Core;

namespace Gojo.Equations;

/// <summary>
/// Construction and inspection helpers for relation-shaped equations. These
/// helpers execute simple reads today, but they leave a visible Gojo structure:
/// frames, ordered samples, index sites, predictions, residues, and errors.
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

    /// <summary>
    /// Applies one equation to an ordered group of input samples. The returned
    /// structure records the map span, equation, and one sample group per input.
    /// </summary>
    public static Relation Map(
        Carrier root,
        Relation equation,
        IReadOnlyList<CarrierLocation> inputs,
        Carrier output,
        out Relation[] samples)
    {
        var holder = RootOrNew(root);
        var index = new Carrier();
        var count = CountOf(inputs);
        samples = new Relation[count];

        for (var sampleIndex = 0; sampleIndex < count; sampleIndex++)
        {
            var prediction = Predict(equation, inputs[sampleIndex], output);
            samples[sampleIndex] = PredictionSample(index, sampleIndex, inputs[sampleIndex], prediction);
        }

        return Relation.OrderedGroup(
            holder,
            [Relation.Segment(index, 0, count), equation ?? Relation.Point(Carrier.Unknown, 0), .. samples]);
    }

    /// <summary>
    /// Applies one equation to an ordered group of scalar or relation-shaped
    /// input samples.
    /// </summary>
    public static Relation Map(
        Carrier root,
        Relation equation,
        IReadOnlyList<IElement> inputs,
        Carrier output,
        out Relation[] samples)
    {
        var holder = RootOrNew(root);
        var index = new Carrier();
        var count = CountOf(inputs);
        samples = new Relation[count];

        for (var sampleIndex = 0; sampleIndex < count; sampleIndex++)
        {
            var input = inputs[sampleIndex] ?? Relation.Point(Carrier.Unknown, 0);
            var prediction = Predict(equation, input, output);
            samples[sampleIndex] = PredictionSample(index, sampleIndex, input, prediction);
        }

        return Relation.OrderedGroup(
            holder,
            [Relation.Segment(index, 0, count), equation ?? Relation.Point(Carrier.Unknown, 0), .. samples]);
    }

    /// <summary>
    /// Compares predicted and observed samples by shared index and creates
    /// per-sample error locations. Extra samples on either side are left out.
    /// </summary>
    public static Relation Compare(
        Carrier root,
        IReadOnlyList<CarrierLocation> predictions,
        IReadOnlyList<CarrierLocation> observations,
        Carrier error,
        out Relation[] samples)
    {
        var holder = RootOrNew(root);
        var errorCarrier = RootOrUnknown(error);
        var index = new Carrier();
        var count = Math.Min(CountOf(predictions), CountOf(observations));
        samples = new Relation[count];

        for (var sampleIndex = 0; sampleIndex < count; sampleIndex++)
        {
            samples[sampleIndex] = ComparisonSample(
                index,
                sampleIndex,
                predictions[sampleIndex],
                observations[sampleIndex],
                new CarrierLocation(errorCarrier, SaturatingSubtract(observations[sampleIndex].Ticks, predictions[sampleIndex].Ticks)));
        }

        return Relation.OrderedGroup(holder, [Relation.Segment(index, 0, count), .. samples]);
    }

    /// <summary>Reads mapped sample groups from a structure returned by Map.</summary>
    public static bool TryReadMapSamples(Relation map, out Relation[] samples)
    {
        if (!TryReadRootElements(map, out var elements) || elements.Length < 2)
        {
            samples = [];
            return false;
        }

        samples = [.. elements.Skip(2).OfType<Relation>()];
        return true;
    }

    /// <summary>Reads comparison sample groups from a structure returned by Compare.</summary>
    public static bool TryReadComparisonSamples(Relation comparison, out Relation[] samples)
    {
        if (!TryReadRootElements(comparison, out var elements) || elements.Length < 1)
        {
            samples = [];
            return false;
        }

        samples = [.. elements.Skip(1).OfType<Relation>()];
        return true;
    }

    /// <summary>Reads one prediction sample group.</summary>
    public static bool TryReadPredictionSample(
        Relation sample,
        out CarrierLocation index,
        out CarrierLocation input,
        out CarrierLocation predicted,
        out CarrierLocation residue)
    {
        if (TryReadSampleLocations(sample, expectedLocations: 4, out var locations))
        {
            index = locations[0];
            input = locations[1];
            predicted = locations[2];
            residue = locations[3];
            return true;
        }

        index = default;
        input = default;
        predicted = default;
        residue = default;
        return false;
    }

    /// <summary>Reads one prediction sample while preserving relation-shaped input and residue.</summary>
    public static bool TryReadPredictionSampleElement(
        Relation sample,
        out CarrierLocation index,
        out IElement input,
        out CarrierLocation predicted,
        out IElement residue)
    {
        if (TryReadRootElements(sample, out var elements) &&
            elements.Length == 4 &&
            elements[0] is Relation indexPoint &&
            indexPoint.Left is CarrierLocation indexLocation &&
            elements[2] is CarrierLocation predictedLocation)
        {
            index = indexLocation;
            input = elements[1];
            predicted = predictedLocation;
            residue = elements[3];
            return true;
        }

        index = default;
        input = default!;
        predicted = default;
        residue = default!;
        return false;
    }

    /// <summary>Reads one comparison sample group.</summary>
    public static bool TryReadComparisonSample(
        Relation sample,
        out CarrierLocation index,
        out CarrierLocation predicted,
        out CarrierLocation observed,
        out CarrierLocation error)
    {
        if (TryReadSampleLocations(sample, expectedLocations: 4, out var locations))
        {
            index = locations[0];
            predicted = locations[1];
            observed = locations[2];
            error = locations[3];
            return true;
        }

        index = default;
        predicted = default;
        observed = default;
        error = default;
        return false;
    }

    private static Relation PredictionSample(
        Carrier index,
        int sampleIndex,
        CarrierLocation input,
        Relation prediction) =>
        PredictionSample(index, sampleIndex, (IElement)input, prediction);

    private static Relation PredictionSample(
        Carrier index,
        int sampleIndex,
        IElement input,
        Relation prediction)
    {
        if (prediction.Left is CarrierLocation predicted)
        {
            return Relation.OrderedGroup(
                new Carrier(),
                Relation.Point(index, sampleIndex),
                input,
                predicted,
                prediction.Right);
        }

        return Relation.OrderedGroup(new Carrier(), Relation.Point(index, sampleIndex), input, prediction);
    }

    private static Relation ComparisonSample(
        Carrier index,
        int sampleIndex,
        CarrierLocation predicted,
        CarrierLocation observed,
        CarrierLocation error) =>
        Relation.OrderedGroup(
            new Carrier(),
            Relation.Point(index, sampleIndex),
            predicted,
            observed,
            error);

    private static bool TryReadRootElements(Relation structure, out IElement[] elements)
    {
        if (structure?.Right is Carrier holder &&
            structure.TryReadOrderedElementsOn(holder, out elements))
        {
            return true;
        }

        elements = [];
        return false;
    }

    private static bool TryReadSampleLocations(
        Relation sample,
        int expectedLocations,
        out CarrierLocation[] locations)
    {
        if (TryReadRootElements(sample, out var elements) &&
            elements.Length == expectedLocations &&
            elements[0] is Relation indexPoint &&
            indexPoint.Left is CarrierLocation index &&
            elements.Skip(1).All(element => element is CarrierLocation))
        {
            locations = [index, .. elements.Skip(1).Cast<CarrierLocation>()];
            return true;
        }

        locations = [];
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

    private static Carrier RootOrNew(Carrier carrier) =>
        carrier ?? new Carrier();

    private static long SaturatingSubtract(long left, long right)
    {
        var difference = (Int128)left - right;
        if (difference > long.MaxValue)
        {
            return long.MaxValue;
        }

        return difference < long.MinValue ? long.MinValue : (long)difference;
    }
}
