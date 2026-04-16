using System.Collections.Immutable;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.Topology;

/// <summary>
/// A trolley-local modifier applied through the carried calibration law. This is the mutable traversal state:
/// intent, handedness, and later step shaping should live here rather than by mutating the basis being carried.
/// </summary>
public readonly record struct TraversalModifier(Ratio State)
{
    public static TraversalModifier Identity => new(new Ratio(Measure.One, Measure.One));

    public Ratio Ratio => State.Normalize();

    public Measure Direction => Ratio.Sign;

    public Measure Potential => Ratio.Potential;

    public Ratio Agreement(TraversalModifier other) => new Ratio(
        new Measure(Direction.Raw * other.Direction.Raw),
        Measure.One).Normalize();

    public static TraversalModifier FromDirection(Measure direction) => direction.Raw == 0L
        ? new TraversalModifier(Ratio.Zero(Measure.One))
        : new TraversalModifier(new Ratio(direction.Sign, Measure.One).Normalize());
}

/// <summary>
/// A small Core4-native read of a carried traversal state. It keeps placement, carried calibration, mutable
/// traversal modifier, and provenance together so later transport, re-entry, and path choice can compare structural
/// state rather than only raw site identity.
/// </summary>
public readonly record struct TraversalSignature(
    Placement Placement,
    ISupportProjection CalibrationRead,
    TraversalModifier Modifier,
    Provenance Provenance)
{
    public Carrier Carrier => Placement.PrimaryCarrier;

    public Measure Grade => Placement.SupportGrade;

    public ISupportProjection ProjectionRead => Placement.ProjectionRead;

    // TODO(core): Delete this compatibility alias once callers consume ProjectionRead directly.
    public Ratio Projection => Placement.Projection.Normalize();

    // TODO(core): Delete this compatibility alias once callers stop expecting position as raw Ratio.
    public Ratio Position => Projection;

    // TODO(core): Delete this compatibility alias once callers consume CalibrationRead directly.
    public Ratio Calibration => CalibrationRead.AsRatio().Normalize();

    // TODO(core): Delete this compatibility alias once callers stop expecting carried calibration as raw Ratio.
    public Ratio Basis => Calibration;

    public Measure CalibrationDirection => Calibration.Sign;

    public Measure Perspective => Modifier.Direction;

    public Measure EffectiveDirection => new Measure(CalibrationDirection.Raw * Modifier.Direction.Raw);

    public Measure Potential => Placement.Potential + CalibrationRead.Potential + Provenance.Potential;

    public bool SameProjection(TraversalSignature other) =>
        Carrier == other.Carrier && Projection == other.Projection;

    public bool Equivalent(TraversalSignature other) =>
        SameProjection(other)
        && Grade == other.Grade
        && Calibration == other.Calibration
        && Modifier == other.Modifier
        && Provenance.Direction == other.Provenance.Direction
        && Perspective.Sign == other.Perspective.Sign;

    public Ratio ProjectionAgreement(TraversalSignature other) => SameProjection(other)
        ? new Ratio(Measure.One, Measure.One)
        : Ratio.Zero(Measure.One);

    public Ratio CalibrationAgreement(TraversalSignature other) => Calibration == other.Calibration
        ? new Ratio(Measure.One, Measure.One)
        : new Ratio(new Measure(Calibration.Sign.Raw * other.Calibration.Sign.Raw), Measure.One).Normalize();

    public Ratio ModifierAgreement(TraversalSignature other) => Modifier.Agreement(other.Modifier);

    public Ratio PerspectiveAgreement(TraversalSignature other) => new Ratio(
        new Measure(Perspective.Sign.Raw * other.Perspective.Sign.Raw),
        Measure.One).Normalize();

    public Ratio EffectiveAgreement(TraversalSignature other) => new Ratio(
        new Measure(EffectiveDirection.Sign.Raw * other.EffectiveDirection.Sign.Raw),
        Measure.One).Normalize();

    public Ratio ReentryAgreement(TraversalSignature other) => Equivalent(other)
        ? new Ratio(Measure.One, Measure.One)
        : SameProjection(other)
            ? Ratio.Zero(Measure.One)
            : new Ratio(-Measure.One, Measure.One).Normalize();

    public static TraversalSignature FromPlacement(
        Placement placement,
        ISupportProjection calibration,
        TraversalModifier modifier,
        Provenance provenance) =>
        new(placement, calibration, modifier, provenance);

    // TODO(core): Delete this compatibility overload once callers pass support-shaped calibration reads directly.
    public static TraversalSignature FromPlacement(
        Placement placement,
        Ratio basis,
        TraversalModifier modifier,
        Provenance provenance) =>
        FromPlacement(placement, (ISupportProjection)basis.Normalize(), modifier, provenance);

    // TODO(core): Delete this compatibility overload once callers pass support-shaped calibration reads directly.
    public static TraversalSignature FromPlacement(
        Placement placement,
        Measure perspective,
        Ratio basis,
        Provenance provenance) =>
        FromPlacement(placement, basis, TraversalModifier.FromDirection(perspective), provenance);

    public static TraversalSignature FromSite(
        Site site,
        ISupportProjection calibration,
        TraversalModifier modifier,
        Provenance provenance) =>
        FromPlacement(site.Placement, calibration, modifier, provenance);

    // TODO(core): Delete this compatibility overload once callers pass support-shaped calibration reads directly.
    public static TraversalSignature FromSite(
        Site site,
        Ratio basis,
        TraversalModifier modifier,
        Provenance provenance) =>
        FromPlacement(site.Placement, basis, modifier, provenance);

    // TODO(core): Delete this compatibility overload once callers pass support-shaped calibration reads directly.
    public static TraversalSignature FromSite(
        Site site,
        Measure perspective,
        Ratio basis,
        Provenance provenance) =>
        FromPlacement(site.Placement, perspective, basis, provenance);

    public static Provenance ProvenanceFor(Measure perspective, Measure potential)
    {
        if (perspective.Raw > 0L)
        {
            return Provenance.SelfDriven(potential);
        }

        if (perspective.Raw < 0L)
        {
            return Provenance.OtherDriven(potential);
        }

        return Provenance.Shared(potential == Measure.Zero ? Measure.One : potential);
    }
}

/// <summary>
/// A numeric encoding of how one continuation relates to the current traversal state. It avoids symbolic branch tags:
/// continuity, perspective, and re-entry bias remain ordinary signed Core4 values.
/// </summary>
public readonly record struct TraversalDisposition(
    Ratio Continuity,
    Ratio Perspective,
    Ratio Reentry)
{
    public Measure Direction => Perspective.Sign;

    public bool ContinuesCarrier => Continuity.Sign.Raw > 0;

    public bool ReturnsAlongIngress => Reentry.Sign.Raw < 0;

    public bool IsIncoming => Direction.Raw < 0;

    public bool IsOutgoing => Direction.Raw > 0;

    public bool IsOscillating => Direction.Raw == 0;

    public Ratio Rank => Continuity.Add(Perspective).Add(Reentry).Normalize();

    public Measure DirectionIn(Measure observerPerspective)
    {
        var sign = observerPerspective.Raw < 0L ? -1L : 1L;
        return new Measure(Direction.Raw * sign);
    }

    public bool ReturnsAlongIngressIn(Measure observerPerspective) =>
        observerPerspective.Raw >= 0L && ReturnsAlongIngress;

    public bool IsForward(Measure observerPerspective, bool carrierContinues)
    {
        if (ReturnsAlongIngressIn(observerPerspective))
        {
            return false;
        }

        var direction = DirectionIn(observerPerspective);
        return direction.Raw > 0L;
    }

    public Ratio ForwardPreference(Measure observerPerspective, bool carrierContinues)
    {
        var direction = DirectionIn(observerPerspective);
        var directionBias = direction.Raw > 0L
            ? new Ratio(Measure.One, Measure.One)
            : carrierContinues
                ? Ratio.Zero(Measure.One)
                : new Ratio(-Measure.One, Measure.One);
        var ingressBias = ReturnsAlongIngressIn(observerPerspective)
            ? new Ratio(-Measure.One, Measure.One)
            : Ratio.Zero(Measure.One);

        return Continuity.Add(directionBias).Add(ingressBias).Normalize();
    }

    public static TraversalDisposition Create(bool continuesCarrier, Measure direction, bool returnsAlongIngress) => new(
        continuesCarrier ? new Ratio(Measure.One, Measure.One) : Ratio.Zero(Measure.One),
        new Ratio(direction, Measure.One).Normalize(),
        new Ratio(returnsAlongIngress ? -Measure.One : Measure.One, Measure.One).Normalize());
}

/// <summary>
/// A support-local candidate continuation expressed only in Core4 traversal terms: source/target signatures plus the
/// numeric disposition relating them.
/// </summary>
public readonly record struct TraversalContinuation(
    Carrier Carrier,
    TraversalSignature Source,
    TraversalSignature Target,
    TraversalDisposition Disposition)
{
    public Measure Direction => Disposition.Direction;

    public bool ContinuesCarrier => Disposition.ContinuesCarrier;

    public bool ReturnsAlongIngress => Disposition.ReturnsAlongIngress;

    public bool IsIncoming => Disposition.IsIncoming;

    public bool IsOutgoing => Disposition.IsOutgoing;
}

/// <summary>
/// A generic traversal read over any ordered Core4 support. It determines admissible forward continuations and
/// preferred next motion numerically from the continuation dispositions rather than from topology-specific rules.
/// </summary>
public readonly record struct TraversalObservation(
    TraversalSignature Signature,
    ImmutableArray<TraversalContinuation> Continuations)
{
    public ImmutableArray<TraversalContinuation> ForwardContinuations => ForwardContinuationsIn(Measure.One);

    public ImmutableArray<TraversalContinuation> ForwardContinuationsIn(Measure observerPerspective)
    {
        var indexes = ForwardIndexes(observerPerspective);
        var forward = ImmutableArray.CreateBuilder<TraversalContinuation>(indexes.Length);

        foreach (var index in indexes)
        {
            forward.Add(Continuations[index]);
        }

        return forward.ToImmutable();
    }

    public bool IsDecisionPoint => ForwardIndexes().Length > 1;

    public ImmutableArray<int> ForwardIndexes() => ForwardIndexes(Measure.One);

    public ImmutableArray<int> ForwardIndexes(Measure observerPerspective)
    {
        var indexes = ImmutableArray.CreateBuilder<int>();

        for (var index = 0; index < Continuations.Length; index++)
        {
            var continuation = Continuations[index];
            var carrierContinues = HasOutgoingOnCarrier(continuation.Carrier);

            if (continuation.Disposition.IsForward(observerPerspective, carrierContinues))
            {
                indexes.Add(index);
            }
        }

        if (indexes.Count == 0 && observerPerspective.Raw < 0L)
        {
            for (var index = 0; index < Continuations.Length; index++)
            {
                if (Continuations[index].ReturnsAlongIngress)
                {
                    indexes.Add(index);
                }
            }
        }

        return indexes.ToImmutable();
    }

    public bool TryPreferredIndex(out int index) => TryPreferredIndex(Measure.One, out index);

    public bool TryPreferredIndex(Measure observerPerspective, out int index)
    {
        var forward = ForwardIndexes(observerPerspective);

        if (forward.Length == 0)
        {
            index = -1;
            return false;
        }

        if (forward.Length == 1)
        {
            index = forward[0];
            return true;
        }

        var bestIndex = -1;
        var bestPreference = Ratio.Zero(Measure.One);
        var ambiguous = false;

        foreach (var candidateIndex in forward)
        {
            var continuation = Continuations[candidateIndex];
            var preference = continuation.Disposition.ForwardPreference(
                observerPerspective,
                HasOutgoingOnCarrier(continuation.Carrier));
            var comparison = Compare(preference, bestPreference);

            if (bestIndex < 0 || comparison > 0)
            {
                bestIndex = candidateIndex;
                bestPreference = preference;
                ambiguous = false;
                continue;
            }

            if (comparison == 0)
            {
                ambiguous = true;
            }
        }

        index = ambiguous ? -1 : bestIndex;
        return !ambiguous && bestIndex >= 0;
    }

    public bool TryPreferred(out TraversalContinuation continuation) => TryPreferred(Measure.One, out continuation);

    public bool TryPreferred(Measure observerPerspective, out TraversalContinuation continuation)
    {
        if (TryPreferredIndex(observerPerspective, out var index))
        {
            continuation = Continuations[index];
            return true;
        }

        continuation = default;
        return false;
    }

    private bool HasOutgoingOnCarrier(Carrier carrier)
    {
        foreach (var continuation in Continuations)
        {
            if (!continuation.ReturnsAlongIngress
                && continuation.Carrier == carrier
                && continuation.IsOutgoing)
            {
                return true;
            }
        }

        return false;
    }

    private static int Compare(Ratio left, Ratio right)
        => left.CompareValue(right);
}
