using Gojo.Core;
using Gojo.Patterns;

namespace Gojo.ConceptShapes;

/// <summary>
/// Relation-shaped generators from the concept shapes codex. These build ordinary
/// Gojo structure only; they do not assign concept words.
/// </summary>
public static class ConceptPrimitives
{
    /// <summary>Grounded site <c>L(C,n) → C</c>.</summary>
    public static Relation Site(Carrier carrier, long ticks) =>
        Relation.Point(ConceptShapeRoot.RootOrNew(carrier), ticks);

    /// <summary>Shared tick coordinate across one or more carrier faces.</summary>
    public static (TickRef Coordinate, CarrierLocation Location) CoSite(Carrier carrier, long ticks)
    {
        var c = ConceptShapeRoot.RootOrNew(carrier);
        var coordinate = new TickRef(ticks);
        return (coordinate, new CarrierLocation(c, coordinate));
    }

    /// <summary>Oriented extent on one carrier.</summary>
    public static Relation DirectedExtent(Carrier carrier, long start, long end) =>
        Relation.Segment(ConceptShapeRoot.RootOrNew(carrier), start, end);

    /// <summary>Collapsed basis: orientation not yet chosen.</summary>
    public static Relation CollapsedExtent(Carrier carrier, long ticks) =>
        DirectedExtent(carrier, ticks, ticks);

    /// <summary>Symmetric extent without privileging traverse direction.</summary>
    public static Relation UnorientedSpan(Carrier carrier, long a, long b) =>
        new(DirectedExtent(carrier, a, b), DirectedExtent(carrier, b, a));

    /// <summary>Flow-oriented extent across opposed carrier faces.</summary>
    public static Relation DirectedFaceExtent(Carrier carrier, long inverseStart, long positiveEnd) =>
        Relation.DirectedSegment(ConceptShapeRoot.RootOrNew(carrier), inverseStart, positiveEnd);

    /// <summary>Uncalibrated orthogonal potential from a source element.</summary>
    public static Relation OrthogonalPotential(IElement source) =>
        Relation.OrthogonalOutlet(source);

    /// <summary>Radial / contour field seed from a point.</summary>
    public static Relation RadialPotential(Carrier center, long ticks = 0) =>
        OrthogonalPotential(Site(center, ticks));

    /// <summary>Ray / line potential from a collapsed segment.</summary>
    public static Relation RayPotential(Carrier carrier, long ticks = 0) =>
        OrthogonalPotential(CollapsedExtent(carrier, ticks));

    /// <summary>Sheet / side potential from a nonzero segment.</summary>
    public static Relation SheetPotential(Carrier carrier, long start, long end) =>
        OrthogonalPotential(DirectedExtent(carrier, start, end));

    /// <summary>Ordered coexistence at a holder carrier.</summary>
    public static Relation Gather(Carrier holder, params IElement[] participants) =>
        Relation.OrderedGroup(ConceptShapeRoot.RootOrNew(holder), participants);

    /// <summary>Discrete branches from one holder (group fan, not radial).</summary>
    public static Relation FanOut(Carrier holder, params IElement[] branches) =>
        Gather(holder, branches);

    /// <summary>Accounting boundary dividing one whole into <paramref name="partitions"/> parts.</summary>
    public static Relation WrapPartition(Carrier phase, long partitions) =>
        new(
            Relation.Point(ConceptShapeRoot.RootOrNew(phase), partitions),
            Relation.Point(phase, 0));

    /// <summary>Fixed ceiling/floor on a phase carrier.</summary>
    public static Relation ClampBoundary(Carrier phase, long ticks) =>
        new(Relation.Point(phase, ticks), Relation.Point(phase, ticks));

    /// <summary>Open placeholder slot binding.</summary>
    public static Relation OpenSlot(Placeholder? slot = null) =>
        StructuralPattern.Bind(slot ?? new Placeholder(), ConceptShapeRoot.UnknownPoint());

    /// <summary>Descriptor provenance slot binding.</summary>
    public static Relation DescriptorBinding(int index, IElement descriptor) =>
        StructuralPattern.BindDescriptor(index, descriptor);

    /// <summary>Continuous read from a selected element into a concrete port.</summary>
    public static Relation ExposedRead(IElement selected, Relation outputPort) =>
        Relation.LiveOutput(selected, outputPort);

    /// <summary>Aligned frame projection between two carrier spans.</summary>
    public static Relation ProjectionFrame(
        Carrier source,
        long sourceStart,
        long sourceEnd,
        Carrier target,
        long targetStart,
        long targetEnd) =>
        new(
            DirectedExtent(source, sourceStart, sourceEnd),
            DirectedExtent(target, targetStart, targetEnd));

    /// <summary>Reads an unoriented span pair when present.</summary>
    public static bool TryReadUnorientedSpan(
        Relation relation,
        out Carrier carrier,
        out long a,
        out long b)
    {
        carrier = default!;
        a = 0;
        b = 0;

        if (relation.Left is not Relation forward ||
            relation.Right is not Relation reverse ||
            !TryReadDirectedExtent(forward, out carrier, out a, out var forwardEnd) ||
            !TryReadDirectedExtent(reverse, out var carrierB, out var reverseStart, out b) ||
            !carrier.SameIdentityAs(carrierB) ||
            forwardEnd != reverseStart ||
            a != b)
        {
            return false;
        }

        return true;
    }

    /// <summary>Reads a directed segment extent.</summary>
    public static bool TryReadDirectedExtent(
        Relation relation,
        out Carrier carrier,
        out long start,
        out long end)
    {
        carrier = default!;
        start = 0;
        end = 0;

        if (relation.Left is Relation startPoint &&
            relation.Right is Relation endPoint &&
            startPoint.Left is CarrierLocation startLocation &&
            endPoint.Left is CarrierLocation endLocation &&
            startLocation.Carrier.SameIdentityAs(endLocation.Carrier))
        {
            carrier = startLocation.Carrier;
            start = startLocation.Ticks;
            end = endLocation.Ticks;
            return true;
        }

        return false;
    }
}
