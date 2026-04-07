using System.Collections.Immutable;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.Topology;

/// <summary>
/// The stage-2 carrier geometry view for one site on one carrier. It reads where the site sits within the local
/// readable span on that carrier, whether the span continues through the site or terminates there, and how balanced
/// the span is around the site.
/// </summary>
public readonly record struct AlignedSiteSpan(Site Site, Carrier Carrier, ImmutableArray<SiteLeg> Legs)
{
    // TODO: Prefer querying this through ConnectedSiteTopology. This local helper remains the stage-2 geometry view
    // while the connected stage-3 context grows around it.
    // TODO: Generalize readable carrier geometry beyond relation-backed legs once inspection can interpolate through
    // broader support kinds directly.
    public ImmutableArray<Relation> IncomingRelations => SelectRelations(leg => leg.IsIncoming);

    public ImmutableArray<Relation> OutgoingRelations => SelectRelations(leg => leg.IsOutgoing);

    public Measure NonRelationLegCount => new(CountLegs(leg => leg.Support is not Relation));

    public Measure IncomingCount => new(IncomingRelations.Length);

    public Measure OutgoingCount => new(OutgoingRelations.Length);

    public Measure InboundSpan => SumPotential(IncomingRelations);

    public Measure OutboundSpan => SumPotential(OutgoingRelations);

    public Measure Span => InboundSpan + OutboundSpan;

    public bool HasGeometry => NonRelationLegCount == Measure.Zero
        && Span != Measure.Zero
        && IncomingCount.Raw <= 1L
        && OutgoingCount.Raw <= 1L;

    public bool IsInterior => IncomingCount != Measure.Zero && OutgoingCount != Measure.Zero;

    public bool IsStartEndpoint => IncomingCount == Measure.Zero && OutgoingCount != Measure.Zero;

    public bool IsEndEndpoint => IncomingCount != Measure.Zero && OutgoingCount == Measure.Zero;

    public bool IsCentered => HasGeometry && InboundSpan == OutboundSpan;

    public Ratio Pin => ResolvePin();

    public Relation Support => HasGeometry
        ? BuildSupport()
        : new Relation(Carrier, Site, Site);

    public Ratio Position => HasGeometry
        ? new Ratio(InboundSpan, Span).Normalize()
        : Ratio.Tension(Span);

    public Ratio Balance => HasGeometry
        ? new Ratio(OutboundSpan - InboundSpan, Span).Normalize()
        : Ratio.Tension(Span);

    public Ratio Asymmetry => HasGeometry
        ? new Ratio((OutboundSpan - InboundSpan).Absolute, Span).Normalize()
        : Ratio.Tension(Span);

    public static AlignedSiteSpan Create(Site site, Carrier carrier, params SiteLeg[] legs) => new(site, carrier, [.. legs]);

    private Ratio ResolvePin()
    {
        if (Site.Carrier == Carrier)
        {
            return Site.On(Carrier).Normalize();
        }

        if (IncomingCount == Measure.One && OutgoingCount == Measure.One)
        {
            var incoming = IncomingRelations[0].AnchorPosition.Normalize();
            var outgoing = OutgoingRelations[0].OriginPosition.Normalize();
            return incoming == outgoing ? incoming : Ratio.Tension(Span);
        }

        if (IncomingCount == Measure.One)
        {
            return IncomingRelations[0].AnchorPosition.Normalize();
        }

        if (OutgoingCount == Measure.One)
        {
            return OutgoingRelations[0].OriginPosition.Normalize();
        }

        return Ratio.Tension(Span);
    }

    private Relation BuildSupport()
    {
        if (IncomingCount == Measure.One && OutgoingCount == Measure.One)
        {
            return new Relation(Carrier, IncomingRelations[0].Origin, OutgoingRelations[0].Anchor);
        }

        if (IncomingCount == Measure.One)
        {
            return IncomingRelations[0];
        }

        if (OutgoingCount == Measure.One)
        {
            return OutgoingRelations[0];
        }

        return new Relation(Carrier, Site, Site);
    }

    private ImmutableArray<Relation> SelectRelations(Func<SiteLeg, bool> predicate)
    {
        var relations = ImmutableArray.CreateBuilder<Relation>();

        foreach (var leg in Legs)
        {
            if (leg.Carrier != Carrier || !predicate(leg) || leg.Support is not Relation relation)
            {
                continue;
            }

            relations.Add(relation);
        }

        return relations.ToImmutable();
    }

    private long CountLegs(Func<SiteLeg, bool> predicate)
    {
        long total = 0L;

        foreach (var leg in Legs)
        {
            if (leg.Carrier == Carrier && predicate(leg))
            {
                total++;
            }
        }

        return total;
    }

    private static Measure SumPotential(ImmutableArray<Relation> relations)
    {
        long total = 0L;

        foreach (var relation in relations)
        {
            total += relation.Potential.Raw;
        }

        return new Measure(total);
    }
}
