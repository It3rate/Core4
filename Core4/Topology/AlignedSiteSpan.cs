using System.Collections.Immutable;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.Topology;

/// <summary>
/// The stage-2 aligned geometry view for one site on one carrier. It reads where the site is pinned along the local
/// support span, whether that pin is interior or terminal, and how balanced the span is around the site.
/// </summary>
public readonly record struct AlignedSiteSpan(Site Site, Carrier Carrier, ImmutableArray<Relation> Relations)
{
    // TODO: Stage 3 should derive these aligned relations from a broader connected topology context instead of
    // receiving them directly as local inputs.
    public ImmutableArray<Relation> IncomingRelations => SelectRelations(-Measure.One);

    public ImmutableArray<Relation> OutgoingRelations => SelectRelations(Measure.One);

    public Measure IncomingCount => new(IncomingRelations.Length);

    public Measure OutgoingCount => new(OutgoingRelations.Length);

    public Measure InboundSpan => SumPotential(IncomingRelations);

    public Measure OutboundSpan => SumPotential(OutgoingRelations);

    public Measure Span => InboundSpan + OutboundSpan;

    public bool HasGeometry => Span != Measure.Zero && IncomingCount.Raw <= 1L && OutgoingCount.Raw <= 1L;

    public bool IsInterior => IncomingCount != Measure.Zero && OutgoingCount != Measure.Zero;

    public bool IsStartEndpoint => IncomingCount == Measure.Zero && OutgoingCount != Measure.Zero;

    public bool IsEndEndpoint => IncomingCount != Measure.Zero && OutgoingCount == Measure.Zero;

    public bool IsCentered => HasGeometry && InboundSpan == OutboundSpan;

    public Relation Support => HasGeometry
        ? new Relation(Carrier, StartSite(), EndSite())
        : new Relation(Carrier, Site, Site);

    public Ratio Position => HasGeometry
        ? Support.Project(Site).Normalize()
        : Ratio.Tension(Span);

    public Ratio Balance => HasGeometry
        ? new Ratio(OutboundSpan - InboundSpan, Span).Normalize()
        : Ratio.Tension(Span);

    public Ratio Asymmetry => HasGeometry
        ? new Ratio((OutboundSpan - InboundSpan).Absolute, Span).Normalize()
        : Ratio.Tension(Span);

    public static AlignedSiteSpan Create(Site site, Carrier carrier, params Relation[] relations) => new(site, carrier, [.. relations]);

    private Site StartSite() => IncomingCount == Measure.One ? IncomingRelations[0].Origin : Site;

    private Site EndSite() => OutgoingCount == Measure.One ? OutgoingRelations[0].Anchor : Site;

    private ImmutableArray<Relation> SelectRelations(Measure direction)
    {
        var relations = ImmutableArray.CreateBuilder<Relation>();

        foreach (var relation in Relations)
        {
            if (!SiteLeg.TryFromRelation(relation, Site, out var leg))
            {
                continue;
            }

            if (leg.Carrier == Carrier && leg.Direction == direction)
            {
                relations.Add(relation);
            }
        }

        return relations.ToImmutable();
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
