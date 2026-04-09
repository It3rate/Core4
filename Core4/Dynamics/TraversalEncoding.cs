using System.Collections.Immutable;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.Dynamics;

/// <summary>
/// A small Core4-native read of a carried traversal state. It keeps placement, perspective, carried value, and
/// provenance together so later transport, re-entry, and path choice can compare structural state rather than only
/// raw site identity.
/// </summary>
public readonly record struct TraversalSignature(
    Placement Placement,
    Measure Perspective,
    Ratio Carried,
    Provenance Provenance)
{
    public Carrier Carrier => Placement.PrimaryCarrier;

    public Measure Grade => Placement.SupportGrade;

    public Ratio Position => Placement.Ratio.Normalize();

    public Measure Potential => Placement.Potential + Carried.Potential + Provenance.Potential;

    public bool SameProjection(TraversalSignature other) =>
        Carrier == other.Carrier && Position == other.Position;

    public bool Equivalent(TraversalSignature other) =>
        SameProjection(other)
        && Grade == other.Grade
        && Perspective.Sign == other.Perspective.Sign
        && Provenance.Direction == other.Provenance.Direction
        && Carried.Sign == other.Carried.Sign;

    public Ratio ProjectionAgreement(TraversalSignature other) => SameProjection(other)
        ? new Ratio(Measure.One, Measure.One)
        : Ratio.Zero(Measure.One);

    public Ratio PerspectiveAgreement(TraversalSignature other) => new Ratio(
        new Measure(Perspective.Sign.Raw * other.Perspective.Sign.Raw),
        Measure.One).Normalize();

    public Ratio ReentryAgreement(TraversalSignature other) => Equivalent(other)
        ? new Ratio(Measure.One, Measure.One)
        : SameProjection(other)
            ? Ratio.Zero(Measure.One)
            : new Ratio(-Measure.One, Measure.One).Normalize();

    public static TraversalSignature FromPlacement(
        Placement placement,
        Measure perspective,
        Ratio carried,
        Provenance provenance) =>
        new(placement, perspective, carried.Normalize(), provenance);

    public static TraversalSignature FromSite(
        Site site,
        Measure perspective,
        Ratio carried,
        Provenance provenance) =>
        FromPlacement(site.Placement, perspective, carried, provenance);

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

        if (observerPerspective.Raw < 0L && ReturnsAlongIngress)
        {
            return true;
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
    {
        var leftScaled = checked(left.Recessive.Raw * right.Dominant.Raw);
        var rightScaled = checked(right.Recessive.Raw * left.Dominant.Raw);
        return leftScaled.CompareTo(rightScaled);
    }
}
