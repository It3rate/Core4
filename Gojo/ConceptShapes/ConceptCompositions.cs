using Gojo.Core;
using Gojo.Patterns;
using Gojo.Structures;

namespace Gojo.ConceptShapes;

/// <summary>
/// First-pass composed concept topologies built only from <see cref="ConceptPrimitives"/>.
/// </summary>
public static class ConceptCompositions
{
    /// <summary>
    /// Two extents meeting at one co-site. Optional transition radius slot stays open.
    /// </summary>
    public static Relation Corner(
        Carrier carrier,
        long jointTicks,
        long armATicks,
        long armBTicks,
        Placeholder? transitionRadiusSlot = null)
    {
        var holder = new Carrier();
        var topology = ConceptPrimitives.Gather(
            holder,
            ConceptPrimitives.UnorientedSpan(carrier, jointTicks, armATicks),
            ConceptPrimitives.UnorientedSpan(carrier, jointTicks, armBTicks),
            Relation.Point(carrier, jointTicks));

        if (transitionRadiusSlot is null)
        {
            return topology;
        }

        return ConceptPrimitives.Gather(
            holder,
            topology,
            ConceptPrimitives.RadialPotential(carrier, jointTicks),
            ConceptPrimitives.OpenSlot(transitionRadiusSlot));
    }

    /// <summary>Neck between two region carriers; agnostic traversal until flow is attached.</summary>
    public static Relation Passage(
        Carrier regionA,
        Carrier regionB,
        Carrier neck,
        long neckEndTicks)
    {
        var holder = new Carrier();
        return ConceptPrimitives.Gather(
            holder,
            ConceptPrimitives.Site(regionA, 0),
            ConceptPrimitives.DirectedExtent(neck, 0, neckEndTicks),
            ConceptPrimitives.Site(regionB, 0));
    }

    /// <summary>Passage with a live read on the neck span (choke-capable wiring).</summary>
    public static Relation PassageWithNeckRead(
        Carrier regionA,
        Carrier regionB,
        Carrier neck,
        long neckEndTicks,
        Carrier readTarget)
    {
        var neckSpan = ConceptPrimitives.DirectedExtent(neck, 0, neckEndTicks);
        var port = new Relation(neckSpan, ConceptPrimitives.Site(readTarget, 0));
        var topology = Passage(regionA, regionB, neck, neckEndTicks);
        return ConceptPrimitives.Gather(
            new Carrier(),
            topology,
            ConceptPrimitives.ExposedRead(neckSpan, port));
    }

    /// <summary>Collector-side radial potential narrowing to a neck; outlet omitted.</summary>
    public static Relation PeninsulaCollector(Carrier collector, Carrier neck, long neckEndTicks)
    {
        var holder = new Carrier();
        return ConceptPrimitives.Gather(
            holder,
            ConceptPrimitives.RadialPotential(collector),
            ConceptPrimitives.DirectedExtent(neck, 0, neckEndTicks));
    }

    /// <summary>Radial collector, neck, and forward outlet extent.</summary>
    public static Relation Funnel(
        Carrier collector,
        Carrier neck,
        long neckEndTicks,
        Carrier outlet,
        long outletEndTicks)
    {
        var holder = new Carrier();
        return ConceptPrimitives.Gather(
            holder,
            ConceptPrimitives.RadialPotential(collector),
            ConceptPrimitives.DirectedExtent(neck, 0, neckEndTicks),
            ConceptPrimitives.DirectedExtent(outlet, 0, outletEndTicks));
    }

    /// <summary>Peninsula with an open attach at the neck end instead of an outlet.</summary>
    public static Relation PeninsulaWithOpenOutlet(
        Carrier collector,
        Carrier neck,
        long neckEndTicks,
        Placeholder? outletSlot = null)
    {
        var neckEnd = ConceptPrimitives.Site(neck, neckEndTicks);
        return ConceptPrimitives.Gather(
            new Carrier(),
            PeninsulaCollector(collector, neck, neckEndTicks),
            OpenAttach(neckEnd, outletSlot));
    }

    /// <summary>Orthogonal outlet plus unbound slot — flow may leave without a receiver.</summary>
    public static Relation OpenAttach(IElement source, Placeholder? slot = null)
    {
        var holder = new Carrier();
        return ConceptPrimitives.Gather(
            holder,
            ConceptPrimitives.OrthogonalPotential(source),
            ConceptPrimitives.OpenSlot(slot));
    }

    /// <summary>Routes participation into a residue site with no consumer binding.</summary>
    public static Relation VoidSink(IElement source, Carrier residue)
    {
        var holder = new Carrier();
        return ConceptPrimitives.Gather(
            holder,
            new Relation(source, ConceptPrimitives.Site(residue, 0)));
    }

    /// <summary>Three-leg junction sharing one site tick.</summary>
    public static Relation TJunction(
        Carrier carrier,
        long jointTicks,
        long stemEndTicks,
        long branchAEndTicks,
        long branchBEndTicks)
    {
        var holder = new Carrier();
        return ConceptPrimitives.Gather(
            holder,
            ConceptPrimitives.DirectedExtent(carrier, jointTicks, stemEndTicks),
            ConceptPrimitives.DirectedExtent(carrier, jointTicks, branchAEndTicks),
            ConceptPrimitives.DirectedExtent(carrier, jointTicks, branchBEndTicks),
            Relation.Point(carrier, jointTicks));
    }

    /// <summary>Coarse valve-style partition count without numeric ranges.</summary>
    public static Relation CoarseValvePhase(Carrier phase, int partitionCount = 3)
    {
        var count = Math.Max(2, partitionCount);
        return ConceptPrimitives.WrapPartition(phase, count);
    }

    /// <summary>One refinement step binding a placeholder slot.</summary>
    public static Relation RefinementBindStep(
        Carrier time,
        long stepIndex,
        IElement beforeTopology,
        Placeholder slot,
        IElement bound,
        IElement afterTopology,
        IElement residue)
    {
        var start = stepIndex * 10;
        var end = start + 10;
        var transition = StructuralPattern.Bind(slot, bound);
        return TimeStepSequence.Step(
            new Carrier(),
            time,
            start,
            end,
            beforeTopology,
            transition,
            afterTopology,
            residue);
    }
}
