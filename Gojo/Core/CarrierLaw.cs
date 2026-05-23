namespace Gojo.Core;

/// <summary>
/// Sign/boundary and flow law markers attached to carrier holders as relation
/// structure. Missing attachments read as <see cref="SignLawKind.BothSigns"/> and
/// <see cref="FlowLawKind.Forward"/> for compatibility with bare carriers.
/// </summary>
public static class CarrierLaw
{
    /// <summary>Unknown-carrier tick marking a sign-law surface attachment.</summary>
    public const long SignLawMarkerTicks = -3;

    /// <summary>Unknown-carrier tick marking a flow-law surface attachment.</summary>
    public const long FlowLawMarkerTicks = -4;

    /// <summary>Creates the sign-law marker relation for a holder.</summary>
    public static Relation SignLaw(Carrier holder, SignLawKind kind)
    {
        ArgumentNullException.ThrowIfNull(holder);
        return LawSurface(holder, SignLawMarkerTicks, (long)kind);
    }

    /// <summary>Creates the flow-law marker relation for a holder.</summary>
    public static Relation FlowLaw(Carrier holder, FlowLawKind kind)
    {
        ArgumentNullException.ThrowIfNull(holder);
        return LawSurface(holder, FlowLawMarkerTicks, (long)kind);
    }

    /// <summary>Creates bounded-segment anchors for sign law <see cref="SignLawKind.Segment"/>.</summary>
    public static Relation SegmentBounds(Carrier holder, long anchorA, long anchorB) =>
        Relation.Segment(holder ?? throw new ArgumentNullException(nameof(holder)), anchorA, anchorB);

    /// <summary>Attaches a sign-law marker and settles the holder world.</summary>
    public static Relation AttachSignLaw(Carrier holder, SignLawKind kind) =>
        holder.Attach(SignLaw(holder, kind));

    /// <summary>Attaches a flow-law marker and settles the holder world.</summary>
    public static Relation AttachFlowLaw(Carrier holder, FlowLawKind kind) =>
        holder.Attach(FlowLaw(holder, kind));

    /// <summary>Attaches segment bounds on the holder for segment sign law.</summary>
    public static Relation AttachSegmentBounds(Carrier holder, long anchorA, long anchorB) =>
        holder.Attach(SegmentBounds(holder, anchorA, anchorB));

    /// <summary>Reads an explicitly attached sign law when present and valid.</summary>
    public static bool TryReadSignLaw(Carrier holder, out SignLawKind kind)
    {
        ArgumentNullException.ThrowIfNull(holder);
        if (!TryReadLawKind(holder, SignLawMarkerTicks, out var raw))
        {
            kind = default;
            return false;
        }

        if (!TryMapSignLaw(raw, out kind))
        {
            kind = default;
            return false;
        }

        return true;
    }

    /// <summary>Reads an explicitly attached flow law when present and valid.</summary>
    public static bool TryReadFlowLaw(Carrier holder, out FlowLawKind kind)
    {
        ArgumentNullException.ThrowIfNull(holder);
        if (!TryReadLawKind(holder, FlowLawMarkerTicks, out var raw))
        {
            kind = default;
            return false;
        }

        if (!TryMapFlowLaw(raw, out kind))
        {
            kind = default;
            return false;
        }

        return true;
    }

    /// <summary>Sign law used for reads: explicit attachment or <see cref="SignLawKind.BothSigns"/>.</summary>
    public static SignLawKind ReadEffectiveSignLaw(Carrier holder) =>
        TryReadSignLaw(holder, out var kind) ? kind : SignLawKind.BothSigns;

    /// <summary>Flow law used for reads: explicit attachment or <see cref="FlowLawKind.Forward"/>.</summary>
    public static FlowLawKind ReadEffectiveFlowLaw(Carrier holder) =>
        TryReadFlowLaw(holder, out var kind) ? kind : FlowLawKind.Forward;

    /// <summary>
    /// Reads segment anchors on the holder when sign law is segment and a
    /// same-carrier segment relation is attached.
    /// </summary>
    public static bool TryReadSegmentBounds(Carrier holder, out long anchorA, out long anchorB)
    {
        ArgumentNullException.ThrowIfNull(holder);
        anchorA = 0;
        anchorB = 0;

        if (ReadEffectiveSignLaw(holder) != SignLawKind.Segment)
        {
            return false;
        }

        foreach (var relation in holder.AttachedRelations)
        {
            if (!TryReadSegmentOn(holder, relation, out var left, out var right))
            {
                continue;
            }

            anchorA = left.Ticks;
            anchorB = right.Ticks;
            return true;
        }

        return false;
    }

    private static Relation LawSurface(Carrier holder, long markerTicks, long kindTicks) =>
        new(
            Relation.Point(Carrier.Unknown, markerTicks),
            Relation.Point(holder, kindTicks));

    private static bool TryReadLawKind(Carrier holder, long markerTicks, out long kindTicks)
    {
        kindTicks = 0;
        foreach (var relation in holder.AttachedRelations)
        {
            if (!TryReadLawSurface(relation, holder, markerTicks, out kindTicks))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private static bool TryReadLawSurface(
        Relation relation,
        Carrier holder,
        long markerTicks,
        out long kindTicks)
    {
        kindTicks = 0;
        if (relation.Left is not Relation markerPoint ||
            relation.Right is not Relation kindPoint)
        {
            return false;
        }

        if (!IsPoint(markerPoint, Carrier.Unknown, markerTicks) ||
            !IsPoint(kindPoint, out var kindLocation) ||
            !kindLocation.Carrier.SameIdentityAs(holder))
        {
            return false;
        }

        kindTicks = kindLocation.Ticks;
        return true;
    }

    private static bool TryReadSegmentOn(
        Carrier holder,
        Relation relation,
        out CarrierLocation left,
        out CarrierLocation right)
    {
        if (relation.Left is not Relation leftPoint ||
            relation.Right is not Relation rightPoint ||
            !IsPoint(leftPoint, out left) ||
            !IsPoint(rightPoint, out right) ||
            !left.Carrier.SameIdentityAs(holder) ||
            !right.Carrier.SameIdentityAs(holder))
        {
            left = default;
            right = default;
            return false;
        }

        return true;
    }

    private static bool IsPoint(Relation relation, out CarrierLocation location)
    {
        if (relation.Left is CarrierLocation pointLocation &&
            relation.Right is Carrier carrier &&
            pointLocation.Carrier.SameIdentityAs(carrier))
        {
            location = pointLocation;
            return true;
        }

        location = default;
        return false;
    }

    private static bool IsPoint(Relation relation, Carrier carrier, long ticks) =>
        IsPoint(relation, out var location) &&
        location.Carrier.SameIdentityAs(carrier) &&
        location.Ticks == ticks;

    private static bool TryMapSignLaw(long raw, out SignLawKind kind)
    {
        if (raw is < 0 or > 3)
        {
            kind = default;
            return false;
        }

        kind = (SignLawKind)raw;
        return true;
    }

    private static bool TryMapFlowLaw(long raw, out FlowLawKind kind)
    {
        if (raw is < 0 or > 3)
        {
            kind = default;
            return false;
        }

        kind = (FlowLawKind)raw;
        return true;
    }
}

/// <summary>Sign and boundary grammar for coordinates on a carrier holder.</summary>
public enum SignLawKind
{
    /// <summary>Positive / uint ray toward one goal (P).</summary>
    Positive = 0,

    /// <summary>Negative / nint ray with against-goal ticks first-class (N).</summary>
    Negative = 1,

    /// <summary>Both signs / int line on the face (PN).</summary>
    BothSigns = 2,

    /// <summary>Bounded segment whole between two anchors (S).</summary>
    Segment = 3,
}

/// <summary>Flow calibration grammar for settlement on carrier faces.</summary>
public enum FlowLawKind
{
    /// <summary>Forward face calibration (F).</summary>
    Forward = 0,

    /// <summary>Inverse face calibration (I).</summary>
    Inverse = 1,

    /// <summary>Both faces may calibrate when wired (FI).</summary>
    BothFaces = 2,

    /// <summary>No flowing calibration; layout and partitions only (Ø).</summary>
    Neither = 3,
}
