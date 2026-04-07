using System.Collections.Immutable;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.Topology;

/// <summary>
/// A shared connected topology context for real Core4 sites and live relations. It owns the aligned relation field
/// for a connected patch so local incidence and geometry can be queried without hand-passing relation arrays.
/// </summary>
public readonly record struct ConnectedSiteTopology(ImmutableArray<Site> Sites, ImmutableArray<Relation> Relations)
{
    public Measure SiteCount => new(Sites.Length);

    public Measure RelationCount => new(SharedRelations().Length);

    public Measure CarrierCount => new(Carriers().Length);

    public static ConnectedSiteTopology FromRelations(params Relation[] relations) => new(
        MergeSites(ImmutableArray<Site>.Empty, [.. relations]),
        [.. relations]);

    public static ConnectedSiteTopology Create(IEnumerable<Site> sites, params Relation[] relations) => new(
        MergeSites([.. sites], [.. relations]),
        [.. relations]);

    public SiteIncidence IncidenceAt(Site site)
    {
        var resolved = Resolve(site);
        return new SiteIncidence(resolved, AlignedOn(resolved));
    }

    public SiteConnector ConnectorAt(Site site) => new(this, Resolve(site));

    public ImmutableArray<SiteConnector> Connectors()
    {
        var connectors = ImmutableArray.CreateBuilder<SiteConnector>(Sites.Length);

        foreach (var site in Sites)
        {
            connectors.Add(new SiteConnector(this, site));
        }

        return connectors.ToImmutable();
    }

    public AlignedSiteSpan GeometryAt(Site site, Carrier carrier) => IncidenceAt(site).GeometryOn(carrier);

    public ImmutableArray<Relation> AlignedOn(Site site)
    {
        var resolved = Resolve(site);
        var relations = ImmutableArray.CreateBuilder<Relation>();

        foreach (var relation in SharedRelations())
        {
            if (SiteLeg.TryFromRelation(relation, resolved, out _))
            {
                relations.Add(relation);
            }
        }

        return relations.ToImmutable();
    }

    public ImmutableArray<Site> NeighborsOf(Site site)
    {
        var resolved = Resolve(site);
        var neighbors = ImmutableArray.CreateBuilder<Site>();

        foreach (var leg in RelationLegsAt(resolved))
        {
            var relation = (Relation)leg.Support;
            var neighbor = leg.IsOutgoing ? relation.Anchor : leg.IsIncoming ? relation.Origin : resolved;

            if (!SameNode(neighbor, resolved) && !Contains(neighbors, neighbor))
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors.ToImmutable();
    }

    public ImmutableArray<Site> SitesOn(Carrier carrier)
    {
        var sites = ImmutableArray.CreateBuilder<Site>();

        foreach (var site in Sites)
        {
            if (Touches(site, carrier))
            {
                sites.Add(site);
            }
        }

        return sites.ToImmutable();
    }

    public ImmutableArray<Carrier> Carriers()
    {
        var carriers = ImmutableArray.CreateBuilder<Carrier>();

        foreach (var relation in SharedRelations())
        {
            AddCarrier(carriers, relation.Carrier);
        }

        foreach (var site in Sites)
        {
            AddCarrier(carriers, site.Carrier);

            foreach (var flow in site.Orthogonal.Flows)
            {
                AddCarrier(carriers, flow.Carrier);
            }
        }

        return carriers.ToImmutable();
    }

    private bool Touches(Site site, Carrier carrier)
    {
        if (site.Carrier == carrier)
        {
            return true;
        }

        foreach (var flow in site.Orthogonal.Flows)
        {
            if (flow.Carrier == carrier)
            {
                return true;
            }
        }

        foreach (var relation in Relations)
        {
            if (relation.Carrier == carrier && SiteLeg.TryFromRelation(relation, site, out _))
            {
                return true;
            }
        }

        return false;
    }

    private Site Resolve(Site site)
    {
        foreach (var known in Sites)
        {
            if (SameNode(known, site))
            {
                return known;
            }
        }

        return site;
    }

    private static ImmutableArray<Site> MergeSites(ImmutableArray<Site> sites, ImmutableArray<Relation> relations)
    {
        var merged = ImmutableArray.CreateBuilder<Site>();

        foreach (var site in sites)
        {
            AddSite(merged, site);
        }

        foreach (var relation in relations)
        {
            AddSite(merged, relation.Origin);
            AddSite(merged, relation.Anchor);
        }

        return merged.ToImmutable();
    }

    private ImmutableArray<Relation> SharedRelations()
    {
        var relations = ImmutableArray.CreateBuilder<Relation>();

        foreach (var relation in Relations)
        {
            AddRelation(relations, relation);
        }

        foreach (var site in Sites)
        {
            foreach (var flow in site.Orthogonal.Flows)
            {
                if (flow.Support is Relation relation)
                {
                    AddRelation(relations, relation);
                }
            }
        }

        return relations.ToImmutable();
    }

    private ImmutableArray<SiteLeg> RelationLegsAt(Site site)
    {
        var legs = ImmutableArray.CreateBuilder<SiteLeg>();

        foreach (var leg in IncidenceAt(site).Legs)
        {
            if (leg.Support is Relation)
            {
                legs.Add(leg);
            }
        }

        return legs.ToImmutable();
    }

    private static void AddCarrier(ImmutableArray<Carrier>.Builder carriers, Carrier carrier)
    {
        if (!carriers.Contains(carrier))
        {
            carriers.Add(carrier);
        }
    }

    private static void AddRelation(ImmutableArray<Relation>.Builder relations, Relation relation)
    {
        if (!relations.Contains(relation))
        {
            relations.Add(relation);
        }
    }

    private static void AddSite(ImmutableArray<Site>.Builder sites, Site site)
    {
        if (!Contains(sites, site))
        {
            sites.Add(site);
        }
    }

    private static bool Contains(ImmutableArray<Site>.Builder sites, Site site)
    {
        foreach (var known in sites)
        {
            if (SameNode(known, site))
            {
                return true;
            }
        }

        return false;
    }

    private static bool SameNode(Site left, Site right) => left.Carrier == right.Carrier && left.Location == right.Location;
}
