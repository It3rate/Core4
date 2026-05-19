using Gojo.Core;
using Gojo.Structures;

namespace Gojo.Equations;

/// <summary>
/// Coarse-to-fine parameter search over ordinary equation surfaces. The helper
/// records candidate inputs, predictions, claim residuals, residual magnitudes,
/// and read residue as Gojo relation structure.
/// </summary>
public static class EquationResolutionSearch
{
    /// <summary>
    /// Samples an equation over a span, then repeatedly narrows around the
    /// lowest-magnitude residual candidate.
    /// </summary>
    public static Relation Search(
        Carrier root,
        Relation equation,
        Carrier input,
        Carrier output,
        long targetTicks,
        long startTicks,
        long endTicks,
        int candidateCount,
        int roundCount,
        out Relation[] rounds)
    {
        var holder = RootOrNew(root);
        var inputCarrier = RootOrUnknown(input);
        var outputCarrier = RootOrUnknown(output);
        var target = new CarrierLocation(outputCarrier, targetTicks);
        var error = new Carrier(outputCarrier.UnitShape);
        var magnitude = new Carrier(outputCarrier.UnitShape);
        var roundIndex = new Carrier();
        var stepCarrier = new Carrier(inputCarrier.UnitShape);
        var selectedIndex = new Carrier();
        var count = Math.Max(0, roundCount);
        rounds = new Relation[count];

        var spanStart = startTicks;
        var spanEnd = endTicks;
        for (var round = 0; round < count; round++)
        {
            rounds[round] = SearchRound(
                roundIndex,
                stepCarrier,
                selectedIndex,
                round,
                equation,
                inputCarrier,
                outputCarrier,
                target,
                error,
                magnitude,
                spanStart,
                spanEnd,
                candidateCount,
                out var bestInput,
                out var step);

            var halfWidth = Math.Max(1, MagnitudeOf(step));
            spanStart = SaturatingSubtract(bestInput.Ticks, halfWidth);
            spanEnd = SaturatingAdd(bestInput.Ticks, halfWidth);
        }

        return Relation.OrderedGroup(
            holder,
            [
                Relation.Segment(inputCarrier, startTicks, endTicks),
                equation ?? Relation.Point(Carrier.Unknown, 0),
                target,
                error,
                magnitude,
                .. rounds
            ]);
    }

    /// <summary>Reads search rounds from a structure returned by <see cref="Search"/>.</summary>
    public static bool TryReadRounds(Relation search, out Relation[] rounds)
    {
        if (TryReadRootElements(search, out var elements) && elements.Length >= 5)
        {
            rounds = [.. elements.Skip(5).OfType<Relation>()];
            return true;
        }

        rounds = [];
        return false;
    }

    /// <summary>Reads one refinement round and its candidate samples.</summary>
    public static bool TryReadRound(
        Relation round,
        out CarrierLocation roundIndex,
        out Relation span,
        out CarrierLocation step,
        out CarrierLocation selectedIndex,
        out Relation[] samples)
    {
        if (TryReadRootElements(round, out var elements) &&
            elements.Length >= 5 &&
            TryReadPoint(elements[0], out roundIndex) &&
            elements[1] is Relation spanRelation &&
            TryReadPoint(elements[2], out step) &&
            TryReadPoint(elements[3], out selectedIndex))
        {
            span = spanRelation;
            samples = [.. elements.Skip(5).OfType<Relation>()];
            return true;
        }

        roundIndex = default;
        span = null!;
        step = default;
        selectedIndex = default;
        samples = [];
        return false;
    }

    /// <summary>Reads the min-magnitude tournament recorded by one refinement round.</summary>
    public static bool TryReadRoundTournament(Relation round, out Relation tournament)
    {
        if (TryReadRootElements(round, out var elements) &&
            elements.Length >= 5 &&
            elements[4] is Relation candidateTournament &&
            ExtremaTournament.TryReadWinner(candidateTournament, out _))
        {
            tournament = candidateTournament;
            return true;
        }

        tournament = null!;
        return false;
    }

    /// <summary>Reads the selected sample from one refinement round.</summary>
    public static bool TryReadSelectedSample(Relation round, out Relation sample)
    {
        if (TryReadRound(round, out _, out _, out _, out var selectedIndex, out var samples))
        {
            foreach (var candidate in samples)
            {
                if (TryReadCandidateSample(candidate, out var index, out _, out _, out _, out _, out _, out _) &&
                    index.Ticks == selectedIndex.Ticks)
                {
                    sample = candidate;
                    return true;
                }
            }
        }

        sample = null!;
        return false;
    }

    /// <summary>Reads one candidate sample from a refinement round.</summary>
    public static bool TryReadCandidateSample(
        Relation sample,
        out CarrierLocation index,
        out CarrierLocation input,
        out CarrierLocation predicted,
        out CarrierLocation signedResidual,
        out CarrierLocation readableResidual,
        out CarrierLocation magnitude,
        out IElement residue)
    {
        if (TryReadRootElements(sample, out var elements) &&
            elements.Length >= 7 &&
            TryReadPoint(elements[0], out index) &&
            elements[1] is CarrierLocation inputLocation &&
            elements[2] is CarrierLocation predictedLocation &&
            elements[3] is CarrierLocation signedResidualLocation &&
            elements[4] is CarrierLocation readableResidualLocation &&
            elements[5] is CarrierLocation magnitudeLocation)
        {
            input = inputLocation;
            predicted = predictedLocation;
            signedResidual = signedResidualLocation;
            readableResidual = readableResidualLocation;
            magnitude = magnitudeLocation;
            residue = elements[6];
            return true;
        }

        index = default;
        input = default;
        predicted = default;
        signedResidual = default;
        readableResidual = default;
        magnitude = default;
        residue = default!;
        return false;
    }

    /// <summary>Reads the equality claim tested by one candidate sample.</summary>
    public static bool TryReadCandidateClaim(Relation sample, out Relation claim)
    {
        if (TryReadRootElements(sample, out var elements) &&
            elements.Length >= 8 &&
            elements[7] is Relation sampleClaim &&
            OperationClaim.TryRead(sampleClaim, out _, out _, out _, out _, out _, out _))
        {
            claim = sampleClaim;
            return true;
        }

        claim = null!;
        return false;
    }

    private static Relation SearchRound(
        Carrier roundIndex,
        Carrier stepCarrier,
        Carrier selectedIndexCarrier,
        int round,
        Relation equation,
        Carrier input,
        Carrier output,
        CarrierLocation target,
        Carrier error,
        Carrier magnitude,
        long startTicks,
        long endTicks,
        int candidateCount,
        out CarrierLocation bestInput,
        out long step)
    {
        var sampleCount = Math.Max(1, candidateCount);
        var sampleIndex = new Carrier();
        var samples = new Relation[sampleCount];
        var entries = new Relation[sampleCount];
        var span = checked(endTicks - startTicks);
        step = sampleCount <= 1 ? 0 : span / checked(sampleCount - 1);
        bestInput = new CarrierLocation(input, startTicks);

        for (var index = 0; index < sampleCount; index++)
        {
            var candidateTicks = CandidateTicks(startTicks, span, sampleCount, index);
            var candidate = new CarrierLocation(input, candidateTicks);
            var prediction = Equation.Predict(equation, candidate, output);
            var predicted = Equation.TryReadPrediction(prediction, out var predictedLocation, out var residue)
                ? predictedLocation
                : new CarrierLocation(output, 0);
            var claim = OperationClaim.Equality(new Carrier(), predicted, target, error);
            OperationClaim.TryRead(
                claim,
                out _,
                out var residual,
                out _,
                out var residualMagnitude,
                out _,
                out _);
            var signedResidual = residual is CarrierLocation residualLocation
                ? residualLocation
                : new CarrierLocation(error, 0);
            var magnitudeLocation = new CarrierLocation(magnitude, residualMagnitude.Ticks);
            samples[index] = CandidateSample(
                sampleIndex,
                index,
                candidate,
                predicted,
                signedResidual,
                signedResidual,
                magnitudeLocation,
                residue ?? prediction,
                claim);
            entries[index] = ExtremaTournament.Entry(samples[index], magnitudeLocation);
        }

        var tournament = ExtremaTournament.Minimum(new Carrier(), entries, out _);
        var bestIndex = ReadWinningCandidate(tournament, out bestInput);

        return Relation.OrderedGroup(
            new Carrier(),
            [
                Relation.Point(roundIndex, round),
                Relation.Segment(input, startTicks, endTicks),
                Relation.Point(stepCarrier, step),
                Relation.Point(selectedIndexCarrier, bestIndex),
                tournament,
                .. samples
            ]);
    }

    private static int ReadWinningCandidate(Relation tournament, out CarrierLocation input)
    {
        if (ExtremaTournament.TryReadWinner(tournament, out var winner) &&
            ExtremaTournament.TryReadEntry(winner, out var value, out _) &&
            value is Relation sample &&
            TryReadCandidateSample(
                sample,
                out var index,
                out input,
                out _,
                out _,
                out _,
                out _,
                out _))
        {
            return checked((int)index.Ticks);
        }

        input = default;
        return 0;
    }

    private static Relation CandidateSample(
        Carrier sampleIndex,
        int index,
        CarrierLocation input,
        CarrierLocation predicted,
        CarrierLocation signedResidual,
        CarrierLocation readableResidual,
        CarrierLocation magnitude,
        IElement residue,
        Relation claim) =>
        Relation.OrderedGroup(
            new Carrier(),
            Relation.Point(sampleIndex, index),
            input,
            predicted,
            signedResidual,
            readableResidual,
            magnitude,
            residue,
            claim);

    private static long CandidateTicks(long startTicks, long span, int sampleCount, int index) =>
        sampleCount <= 1
            ? startTicks
            : SaturatingAdd(startTicks, checked(span * index / checked(sampleCount - 1)));

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

    private static bool TryReadPoint(IElement element, out CarrierLocation location)
    {
        if (element is Relation point &&
            point.Left is CarrierLocation pointLocation &&
            point.Right is Carrier pointCarrier &&
            pointLocation.Carrier.SameIdentityAs(pointCarrier))
        {
            location = pointLocation;
            return true;
        }

        location = default;
        return false;
    }

    private static long MagnitudeOf(long ticks)
    {
        var magnitude = ticks < 0 ? -(Int128)ticks : (Int128)ticks;
        return magnitude > long.MaxValue ? long.MaxValue : (long)magnitude;
    }

    private static long SaturatingAdd(long left, long right)
    {
        var sum = (Int128)left + right;
        return ClampToLong(sum);
    }

    private static long SaturatingSubtract(long left, long right)
    {
        var difference = (Int128)left - right;
        return ClampToLong(difference);
    }

    private static long ClampToLong(Int128 value)
    {
        if (value > long.MaxValue)
        {
            return long.MaxValue;
        }

        if (value < long.MinValue)
        {
            return long.MinValue;
        }

        return (long)value;
    }

    private static Carrier RootOrUnknown(Carrier carrier) =>
        carrier ?? Carrier.Unknown;

    private static Carrier RootOrNew(Carrier carrier) =>
        carrier ?? new Carrier();
}
