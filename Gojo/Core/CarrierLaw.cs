namespace Gojo.Core;

/// <summary>
/// Sign/boundary and flow law as paired pole markers on a holder. Each axis uses
/// a positive attachment (0 or 1) and a negative attachment (0 or -1); the four
/// combinations are P, N, PN, and S (or F, I, FI, Ø for flow). Missing poles
/// read as compatibility defaults: sign PN (1, -1), flow F (1, 0).
/// </summary>
public static class CarrierLaw
{
    /// <summary>
    /// Labels the unknown-carrier surface that attaches the sign-law positive pole
    /// to a holder as <c>Point(holder, 0)</c> or <c>Point(holder, 1)</c>.
    /// </summary>
    public const long SignPositiveSurfaceTicks = -3;

    /// <summary>
    /// Labels the unknown-carrier surface that attaches the sign-law negative pole
    /// to a holder as <c>Point(holder, 0)</c> or <c>Point(holder, -1)</c>.
    /// </summary>
    public const long SignNegativeSurfaceTicks = -4;

    /// <summary>
    /// Labels the unknown-carrier surface for the flow-law positive pole (0 or 1).
    /// </summary>
    public const long FlowPositiveSurfaceTicks = -5;

    /// <summary>
    /// Labels the unknown-carrier surface for the flow-law negative pole (0 or -1).
    /// </summary>
    public const long FlowNegativeSurfaceTicks = -6;

    public const long DefaultSignPositivePole = 1;
    public const long DefaultSignNegativePole = -1;
    public const long DefaultFlowPositivePole = 1;
    public const long DefaultFlowNegativePole = 0;

    public static Relation SignPositivePole(Carrier holder, long value) =>
        PoleSurface(holder ?? Carrier.Unknown, SignPositiveSurfaceTicks, value);

    public static Relation SignNegativePole(Carrier holder, long value) =>
        PoleSurface(holder ?? Carrier.Unknown, SignNegativeSurfaceTicks, value);

    public static Relation FlowPositivePole(Carrier holder, long value) =>
        PoleSurface(holder ?? Carrier.Unknown, FlowPositiveSurfaceTicks, value);

    public static Relation FlowNegativePole(Carrier holder, long value) =>
        PoleSurface(holder ?? Carrier.Unknown, FlowNegativeSurfaceTicks, value);

    public static Relation SegmentBounds(Carrier holder, long anchorA, long anchorB) =>
        Relation.Segment(holder ?? Carrier.Unknown, anchorA, anchorB);

    public static Relation AttachSignPositivePole(Carrier holder, long value) =>
        AttachPole(holder, SignPositivePole(holder, value));

    public static Relation AttachSignNegativePole(Carrier holder, long value) =>
        AttachPole(holder, SignNegativePole(holder, value));

    public static Relation AttachFlowPositivePole(Carrier holder, long value) =>
        AttachPole(holder, FlowPositivePole(holder, value));

    public static Relation AttachFlowNegativePole(Carrier holder, long value) =>
        AttachPole(holder, FlowNegativePole(holder, value));

    public static Relation AttachSignPoles(Carrier holder, long positive, long negative)
    {
        AttachSignPositivePole(holder, positive);
        return AttachSignNegativePole(holder, negative);
    }

    public static Relation AttachFlowPoles(Carrier holder, long positive, long negative)
    {
        AttachFlowPositivePole(holder, positive);
        return AttachFlowNegativePole(holder, negative);
    }

    public static Relation AttachSegmentBounds(Carrier holder, long anchorA, long anchorB) =>
        AttachPole(holder, SegmentBounds(holder, anchorA, anchorB));

    public static bool TryReadSignPositivePole(Carrier holder, out long value) =>
        TryReadPole(holder, SignPositiveSurfaceTicks, allowed: [0, 1], out value);

    public static bool TryReadSignNegativePole(Carrier holder, out long value) =>
        TryReadPole(holder, SignNegativeSurfaceTicks, allowed: [0, -1], out value);

    public static bool TryReadFlowPositivePole(Carrier holder, out long value) =>
        TryReadPole(holder, FlowPositiveSurfaceTicks, allowed: [0, 1], out value);

    public static bool TryReadFlowNegativePole(Carrier holder, out long value) =>
        TryReadPole(holder, FlowNegativeSurfaceTicks, allowed: [0, -1], out value);

    /// <summary>Both sign poles explicitly attached with allowed values.</summary>
    public static bool TryReadExplicitSignPoles(Carrier holder, out long positive, out long negative) =>
        TryReadExplicitPoles(holder, SignPositiveSurfaceTicks, [0, 1], SignNegativeSurfaceTicks, [0, -1], out positive, out negative);

    /// <summary>Both flow poles explicitly attached with allowed values.</summary>
    public static bool TryReadExplicitFlowPoles(Carrier holder, out long positive, out long negative) =>
        TryReadExplicitPoles(holder, FlowPositiveSurfaceTicks, [0, 1], FlowNegativeSurfaceTicks, [0, -1], out positive, out negative);

    public static long ReadEffectiveSignPositive(Carrier holder) =>
        TryReadSignPositivePole(holder, out var value) ? value : DefaultSignPositivePole;

    public static long ReadEffectiveSignNegative(Carrier holder) =>
        TryReadSignNegativePole(holder, out var value) ? value : DefaultSignNegativePole;

    public static long ReadEffectiveFlowPositive(Carrier holder) =>
        TryReadFlowPositivePole(holder, out var value) ? value : DefaultFlowPositivePole;

    public static long ReadEffectiveFlowNegative(Carrier holder) =>
        TryReadFlowNegativePole(holder, out var value) ? value : DefaultFlowNegativePole;

    public static bool IsEffectiveSegmentSign(Carrier holder) =>
        ReadEffectiveSignPositive(holder) == 0 && ReadEffectiveSignNegative(holder) == 0;

    public static bool TryReadSegmentBounds(Carrier holder, out long anchorA, out long anchorB)
    {
        anchorA = 0;
        anchorB = 0;
        if (holder is null || holder.IsUnknown || !IsEffectiveSegmentSign(holder))
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

    private static Relation AttachPole(Carrier? holder, Relation pole)
    {
        if (holder is null || holder.IsUnknown)
        {
            return pole;
        }

        return holder.Attach(pole);
    }

    private static Relation PoleSurface(Carrier holder, long surfaceTicks, long poleValue) =>
        new(
            Relation.Point(Carrier.Unknown, surfaceTicks),
            Relation.Point(holder, poleValue));

    private static bool TryReadExplicitPoles(
        Carrier? holder,
        long positiveSurface,
        ReadOnlySpan<long> positiveAllowed,
        long negativeSurface,
        ReadOnlySpan<long> negativeAllowed,
        out long positive,
        out long negative)
    {
        positive = 0;
        negative = 0;
        if (holder is null || holder.IsUnknown)
        {
            return false;
        }

        var hasPositive = TryReadPole(holder, positiveSurface, positiveAllowed, out positive);
        var hasNegative = TryReadPole(holder, negativeSurface, negativeAllowed, out negative);
        return hasPositive && hasNegative;
    }

    private static bool TryReadPole(
        Carrier? holder,
        long surfaceTicks,
        ReadOnlySpan<long> allowed,
        out long value)
    {
        value = 0;
        if (holder is null || holder.IsUnknown)
        {
            return false;
        }

        if (!TryReadPoleSurface(holder, surfaceTicks, out value) || !IsAllowed(value, allowed))
        {
            value = 0;
            return false;
        }

        return true;
    }

    private static bool TryReadPoleSurface(Carrier holder, long surfaceTicks, out long poleValue)
    {
        poleValue = 0;
        foreach (var relation in holder.AttachedRelations)
        {
            if (TryReadPoleAttachment(relation, holder, surfaceTicks, out poleValue))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryReadPoleAttachment(
        Relation relation,
        Carrier holder,
        long surfaceTicks,
        out long poleValue)
    {
        poleValue = 0;
        if (relation.Left is not Relation surfacePoint ||
            relation.Right is not Relation polePoint)
        {
            return false;
        }

        if (!IsPoint(surfacePoint, Carrier.Unknown, surfaceTicks) ||
            !IsPoint(polePoint, out var poleLocation) ||
            !poleLocation.Carrier.SameIdentityAs(holder))
        {
            return false;
        }

        poleValue = poleLocation.Ticks;
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

    private static bool IsAllowed(long value, ReadOnlySpan<long> allowed)
    {
        foreach (var candidate in allowed)
        {
            if (value == candidate)
            {
                return true;
            }
        }

        return false;
    }
}

/// <summary>Named sign-law pole pairs (positive, negative); not stored enums.</summary>
public static class SignLawPoles
{
    public const long Positive = 1;
    public const long PositiveOff = 0;
    public const long NegativeOff = 0;
    public const long Negative = -1;

    public static (long Positive, long Negative) P => (1, 0);
    public static (long Positive, long Negative) N => (0, -1);
    public static (long Positive, long Negative) BothSigns => (1, -1);
    public static (long Positive, long Negative) Segment => (0, 0);
}

/// <summary>Named flow-law pole pairs (positive, negative); not stored enums.</summary>
public static class FlowLawPoles
{
    public static (long Positive, long Negative) Forward => (1, 0);
    public static (long Positive, long Negative) Inverse => (0, -1);
    public static (long Positive, long Negative) BothFaces => (1, -1);
    public static (long Positive, long Negative) Neither => (0, 0);
}
