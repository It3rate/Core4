using Gojo.Core;

namespace Gojo.Reads;

/// <summary>
/// Observational reads for directed endpoint segments. A directed segment has
/// an opposed-flow start point and a positive-flow endpoint.
/// </summary>
public static class DirectedSegmentReads
{
    /// <summary>Reads the opposed start and positive endpoint of a directed segment.</summary>
    public static bool TryReadDirectedSegment(
        this Relation segment,
        out CarrierLocation inverseStart,
        out CarrierLocation positiveEnd)
    {
        if (segment?.Left is Relation startPoint &&
            segment.Right is Relation endPoint &&
            TryReadPoint(startPoint, out inverseStart, out var inverseCarrier) &&
            TryReadPoint(endPoint, out positiveEnd, out var positiveCarrier) &&
            inverseCarrier.SameIdentityAs(inverseStart.Carrier) &&
            positiveCarrier.SameIdentityAs(positiveEnd.Carrier) &&
            inverseCarrier.IsInverse &&
            !positiveCarrier.IsInverse &&
            positiveCarrier.ExistingInverse is { } existingInverse &&
            existingInverse.SameIdentityAs(inverseCarrier))
        {
            return true;
        }

        inverseStart = default;
        positiveEnd = default;
        return false;
    }

    /// <summary>
    /// Multiplies two directed endpoint segments by distributing their endpoint
    /// participation into four visible product terms.
    /// </summary>
    public static Relation DirectedSegmentProduct(
        Relation left,
        Relation right,
        Carrier productCarrier,
        out Relation[] terms)
    {
        var carrier = productCarrier ?? Carrier.Unknown;
        if (!left.TryReadDirectedSegment(out var leftStart, out var leftEnd) ||
            !right.TryReadDirectedSegment(out var rightStart, out var rightEnd))
        {
            terms = [];
            return Relation.OrderedGroup(new Carrier());
        }

        terms =
        [
            ProductTerm(carrier, leftStart, rightStart),
            ProductTerm(carrier, leftStart, rightEnd),
            ProductTerm(carrier, leftEnd, rightStart),
            ProductTerm(carrier, leftEnd, rightEnd)
        ];
        return Relation.OrderedGroup(new Carrier(), terms);
    }

    private static Relation ProductTerm(
        Carrier productCarrier,
        CarrierLocation left,
        CarrierLocation right)
    {
        var targetCarrier = left.Carrier.IsInverse || right.Carrier.IsInverse
            ? productCarrier.Inverse()
            : productCarrier;
        return new Relation(
            new CarrierLocation(targetCarrier, checked(left.Ticks * right.Ticks)),
            new Relation(left, right));
    }

    private static bool TryReadPoint(
        Relation point,
        out CarrierLocation location,
        out Carrier carrier)
    {
        if (point.Left is CarrierLocation pointLocation &&
            point.Right is Carrier pointCarrier &&
            pointLocation.Carrier.SameIdentityAs(pointCarrier))
        {
            location = pointLocation;
            carrier = pointCarrier;
            return true;
        }

        location = default;
        carrier = null!;
        return false;
    }
}
