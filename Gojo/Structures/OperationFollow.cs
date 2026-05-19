using Gojo.Core;

namespace Gojo.Structures;

/// <summary>
/// Builds a follow surface from an equality claim and a response route. The
/// helper reads how much residual would route back into the follower without
/// mutating the source carrier.
/// </summary>
public static class OperationFollow
{
    /// <summary>
    /// Claims current equals target, then routes numerator/denominator of the
    /// readable residual back into current's carrier face.
    /// </summary>
    public static Relation Step(
        Carrier root,
        IElement current,
        IElement target,
        Carrier residual,
        long responseNumerator,
        long responseDenominator)
    {
        var holder = root ?? new Carrier();
        var residualCarrier = residual ?? Carrier.Unknown;
        var currentElement = current ?? Relation.Point(Carrier.Unknown, 0);
        var targetElement = target ?? Relation.Point(Carrier.Unknown, 0);
        var follower = TryReadFaceLocation(currentElement, out var currentLocation)
            ? currentLocation.Carrier
            : Carrier.Unknown;
        var claim = OperationClaim.Equality(new Carrier(), currentElement, targetElement, residualCarrier);
        var responseRoute = new Relation(
            Relation.Point(residualCarrier, responseDenominator),
            Relation.Point(follower, responseNumerator));

        var read = ReadFollow(
            currentLocation,
            currentElement,
            follower,
            claim,
            responseRoute,
            residualCarrier,
            responseNumerator,
            responseDenominator);

        return Relation.OrderedGroup(
            holder,
            claim,
            responseRoute,
            read.Applied,
            read.Next,
            read.Remaining,
            read.RouteResidue);
    }

    /// <summary>Reads a follow step and its routed response structure.</summary>
    public static bool TryRead(
        Relation follow,
        out Relation claim,
        out Relation responseRoute,
        out IElement applied,
        out IElement next,
        out IElement remaining,
        out IElement routeResidue)
    {
        if (TryReadRootElements(follow, out var elements) &&
            elements.Length >= 6 &&
            elements[0] is Relation equalityClaim &&
            elements[1] is Relation route &&
            OperationClaim.TryRead(equalityClaim, out _, out _, out _, out _, out _, out _))
        {
            claim = equalityClaim;
            responseRoute = route;
            applied = elements[2];
            next = elements[3];
            remaining = elements[4];
            routeResidue = elements[5];
            return true;
        }

        claim = null!;
        responseRoute = null!;
        applied = default!;
        next = default!;
        remaining = default!;
        routeResidue = default!;
        return false;
    }

    private static FollowRead ReadFollow(
        CarrierLocation current,
        IElement currentElement,
        Carrier follower,
        Relation claim,
        Relation responseRoute,
        Carrier residualCarrier,
        long responseNumerator,
        long responseDenominator)
    {
        if (!OperationClaim.TryRead(
                claim,
                out _,
                out var residual,
                out _,
                out _,
                out _,
                out _) ||
            residual is not CarrierLocation residualLocation ||
            responseNumerator == 0 ||
            responseDenominator == 0 ||
            !responseRoute.TryProject(residualLocation, follower, out var projected) ||
            !projected.TryGetCarrierLocations(out var applied, out var routeResidue) ||
            !applied.Carrier.SameIdentityAs(follower))
        {
            return new FollowRead(
                new CarrierLocation(follower, 0),
                currentElement,
                residual,
                residual);
        }

        var next = new CarrierLocation(follower, SaturatingAdd(current.Ticks, applied.Ticks));
        var remaining = new CarrierLocation(residualCarrier, SaturatingSubtract(residualLocation.Ticks, applied.Ticks));
        return new FollowRead(applied, next, remaining, routeResidue);
    }

    private static bool TryReadFaceLocation(IElement element, out CarrierLocation location)
    {
        if (element is CarrierLocation direct)
        {
            location = direct;
            return true;
        }

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

        return value < long.MinValue ? long.MinValue : (long)value;
    }

    private readonly record struct FollowRead(
        IElement Applied,
        IElement Next,
        IElement Remaining,
        IElement RouteResidue);
}
