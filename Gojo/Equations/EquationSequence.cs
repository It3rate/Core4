using Gojo.Core;

namespace Gojo.Equations;

/// <summary>
/// Helpers for equation composition. A sequence is an ordered Gojo group of
/// equation surfaces; each step's output participation feeds the next step.
/// </summary>
public static class EquationSequence
{
    /// <summary>Creates an ordered sequence of equation surfaces.</summary>
    public static Relation Create(Carrier root, params Relation[] steps)
    {
        var holder = RootOrNew(root);
        var count = CountOf(steps);
        var elements = new IElement[count];

        for (var index = 0; index < count; index++)
        {
            elements[index] = steps[index] ?? Relation.Point(Carrier.Unknown, 0);
        }

        return Relation.OrderedGroup(holder, elements);
    }

    /// <summary>Reads the ordered equation surfaces in a sequence.</summary>
    public static bool TryReadSteps(Relation sequence, out Relation[] steps)
    {
        if (sequence?.Right is Carrier holder &&
            sequence.TryReadOrderedElementsOn(holder, out var elements))
        {
            steps = [.. elements.OfType<Relation>()];
            return true;
        }

        steps = [];
        return false;
    }

    /// <summary>Reads one scalar input sample through a sequence.</summary>
    public static Relation Predict(
        Relation sequence,
        CarrierLocation input,
        Carrier output,
        out Relation[] stepReads) =>
        Predict(sequence, (IElement)input, output, out stepReads);

    /// <summary>Reads one scalar or relation-shaped input sample through a sequence.</summary>
    public static Relation Predict(
        Relation sequence,
        IElement input,
        Carrier output,
        out Relation[] stepReads)
    {
        if (!TryReadSteps(sequence, out var steps) || steps.Length == 0)
        {
            stepReads = [];
            return new Relation(new CarrierLocation(RootOrUnknown(output), 0), input ?? Relation.Point(Carrier.Unknown, 0));
        }

        var current = input ?? Relation.Point(Carrier.Unknown, 0);
        stepReads = new Relation[steps.Length];

        for (var stepIndex = 0; stepIndex < steps.Length; stepIndex++)
        {
            var step = steps[stepIndex];
            var read = EquationFanout.TryReadBranches(step, out _)
                ? EquationFanout.Predict(step, current, out _)
                : Equation.Predict(
                    step,
                    current,
                    stepIndex == steps.Length - 1
                        ? RootOrUnknown(output)
                        : TryReadOutputCarrier(step, out var stepOutput)
                            ? stepOutput
                            : Carrier.Unknown);
            stepReads[stepIndex] = read;

            current = read.Left;
        }

        return Equation.TryReadPrediction(stepReads[^1], out var final, out _)
            ? new Relation(final, ReadResidueGroup(stepReads))
            : new Relation(new CarrierLocation(RootOrUnknown(output), 0), ReadResidueGroup(stepReads));
    }

    private static IElement ReadResidueGroup(IReadOnlyList<Relation> stepReads)
    {
        var holder = new Carrier();
        var residues = new IElement[stepReads.Count];

        for (var index = 0; index < stepReads.Count; index++)
        {
            residues[index] = Equation.TryReadPrediction(stepReads[index], out _, out var residue)
                ? residue
                : stepReads[index];
        }

        return Relation.OrderedGroup(holder, residues);
    }

    private static bool TryReadOutputCarrier(Relation equation, out Carrier carrier)
    {
        if (TryReadSteps(equation, out var steps) && steps.Length != 0)
        {
            return TryReadOutputCarrier(steps[^1], out carrier);
        }

        foreach (var candidate in CarriersOf(equation?.Right))
        {
            if (!candidate.IsUnknown)
            {
                carrier = candidate;
                return true;
            }
        }

        carrier = null!;
        return false;
    }

    private static IEnumerable<Carrier> CarriersOf(IElement? element) => element switch
    {
        Carrier carrier => [carrier],
        CarrierLocation location => [location.Carrier],
        Relation relation => relation.Carriers(),
        _ => []
    };

    private static int CountOf<T>(IReadOnlyList<T> values) =>
        values?.Count ?? 0;

    private static Carrier RootOrUnknown(Carrier carrier) =>
        carrier ?? Carrier.Unknown;

    private static Carrier RootOrNew(Carrier carrier) =>
        carrier ?? new Carrier();
}
