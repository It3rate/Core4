using System.Collections.Immutable;

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

    public ImmutableArray<SiteBasisEncounter> BasisEncountersAt(Site site) => IncidenceAt(site).BasisEncounters();

    public ImmutableArray<SiteBasisEncounter> BasisEncountersAt(Site site, Carrier carrier) =>
        IncidenceAt(site).BasisEncountersOn(carrier);

    public ImmutableArray<SiteConnector> Connectors()
    {
        var connectors = ImmutableArray.CreateBuilder<SiteConnector>(Sites.Length);

        foreach (var site in Sites)
        {
            connectors.Add(new SiteConnector(this, site));
        }

        return connectors.ToImmutable();
    }

    public ImmutableArray<CarrierChain> Chains()
    {
        var chains = ImmutableArray.CreateBuilder<CarrierChain>();

        foreach (var carrier in Carriers())
        {
            foreach (var chain in ChainsOn(carrier))
            {
                chains.Add(chain);
            }
        }

        return chains.ToImmutable();
    }

    public ImmutableArray<CarrierChain> ChainsOn(Carrier carrier)
    {
        var relations = SharedRelationsOn(carrier);
        var nodes = CarrierNodesOn(carrier, relations);
        var chains = ImmutableArray.CreateBuilder<CarrierChain>();
        var remaining = ImmutableArray.CreateBuilder<CarrierChainNode>(nodes.Length);

        foreach (var node in nodes)
        {
            remaining.Add(node);
        }

        while (remaining.Count != 0)
        {
            // Split one carrier into connected runs. Each run owns only same-carrier relations; wider topology stays
            // discoverable through the attached sites pinned onto the run's local nodes.
            var componentNodes = ImmutableArray.CreateBuilder<CarrierChainNode>();
            var queue = ImmutableArray.CreateBuilder<CarrierChainNode>();
            queue.Add(remaining[0]);
            RemoveNode(remaining, remaining[0].Site);

            for (var index = 0; index < queue.Count; index++)
            {
                var node = queue[index];
                componentNodes.Add(node);

                foreach (var neighbor in AdjacentNodes(node.Site, nodes, relations))
                {
                    if (!Contains(componentNodes, neighbor.Site) && RemoveNode(remaining, neighbor.Site))
                    {
                        queue.Add(neighbor);
                    }
                }
            }

            chains.Add(new CarrierChain(
                carrier,
                OrderNodes(componentNodes.ToImmutable()),
                OrderRelations(carrier, componentNodes.ToImmutable(), relations)));
        }

        return chains.ToImmutable();
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

    private ImmutableArray<Relation> SharedRelationsOn(Carrier carrier)
    {
        var relations = ImmutableArray.CreateBuilder<Relation>();

        foreach (var relation in SharedRelations())
        {
            if (relation.Carrier == carrier)
            {
                relations.Add(relation);
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

    private ImmutableArray<CarrierChainNode> CarrierNodesOn(Carrier carrier, ImmutableArray<Relation> relations)
    {
        var nodes = ImmutableArray.CreateBuilder<CarrierChainNode>();

        foreach (var relation in relations)
        {
            AddChainNode(nodes, relation.Origin);
            AddChainNode(nodes, relation.Anchor);
        }

        foreach (var site in SitesOn(carrier))
        {
            AddAttachment(nodes, carrier, site);
        }

        return nodes.ToImmutable();
    }

    private ImmutableArray<CarrierChainNode> AdjacentNodes(
        Site site,
        ImmutableArray<CarrierChainNode> nodes,
        ImmutableArray<Relation> relations)
    {
        var adjacent = ImmutableArray.CreateBuilder<CarrierChainNode>();

        foreach (var relation in relations)
        {
            if (SameNode(relation.Origin, site))
            {
                AddNode(adjacent, FindNode(nodes, relation.Anchor));
            }
            else if (SameNode(relation.Anchor, site))
            {
                AddNode(adjacent, FindNode(nodes, relation.Origin));
            }
        }

        return adjacent.ToImmutable();
    }

    private void AddAttachment(ImmutableArray<CarrierChainNode>.Builder nodes, Carrier carrier, Site attachedSite)
    {
        var geometry = GeometryAt(attachedSite, carrier);
        var pin = geometry.Pin.Normalize();

        if (!pin.IsResolved)
        {
            return;
        }

        var pinSite = attachedSite.Carrier == carrier
            ? Resolve(attachedSite)
            : Site.Open(carrier.At(pin));

        for (var index = 0; index < nodes.Count; index++)
        {
            if (!SameNode(nodes[index].Site, pinSite))
            {
                continue;
            }

            if (!Contains(nodes[index].AttachedSites, attachedSite))
            {
                nodes[index] = nodes[index] with { AttachedSites = nodes[index].AttachedSites.Add(attachedSite) };
            }

            return;
        }

        nodes.Add(new CarrierChainNode(pinSite, [attachedSite]));
    }

    private static void AddCarrier(ImmutableArray<Carrier>.Builder carriers, Carrier carrier)
    {
        if (!carriers.Contains(carrier))
        {
            carriers.Add(carrier);
        }
    }

    private static void AddChainNode(ImmutableArray<CarrierChainNode>.Builder nodes, Site site)
    {
        for (var index = 0; index < nodes.Count; index++)
        {
            if (SameNode(nodes[index].Site, site))
            {
                return;
            }
        }

        nodes.Add(new CarrierChainNode(site, ImmutableArray<Site>.Empty));
    }

    private static void AddNode(ImmutableArray<CarrierChainNode>.Builder nodes, CarrierChainNode node)
    {
        if (!Contains(nodes, node.Site))
        {
            nodes.Add(node);
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

    private static bool Contains(ImmutableArray<Site> sites, Site site)
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

    private static bool Contains(ImmutableArray<CarrierChainNode>.Builder nodes, Site site)
    {
        foreach (var node in nodes)
        {
            if (SameNode(node.Site, site))
            {
                return true;
            }
        }

        return false;
    }

    private static bool Contains(ImmutableArray<CarrierChainNode> nodes, Site site)
    {
        foreach (var node in nodes)
        {
            if (SameNode(node.Site, site))
            {
                return true;
            }
        }

        return false;
    }

    private static bool RemoveNode(ImmutableArray<CarrierChainNode>.Builder nodes, Site site)
    {
        for (var index = 0; index < nodes.Count; index++)
        {
            if (!SameNode(nodes[index].Site, site))
            {
                continue;
            }

            nodes.RemoveAt(index);
            return true;
        }

        return false;
    }

    private static CarrierChainNode FindNode(ImmutableArray<CarrierChainNode> nodes, Site site)
    {
        foreach (var node in nodes)
        {
            if (SameNode(node.Site, site))
            {
                return node;
            }
        }

        return default;
    }

    private static ImmutableArray<CarrierChainNode> OrderNodes(ImmutableArray<CarrierChainNode> nodes)
    {
        var ordered = ImmutableArray.CreateBuilder<CarrierChainNode>(nodes.Length);

        foreach (var node in nodes)
        {
            InsertNode(ordered, node);
        }

        return ordered.ToImmutable();
    }

    private static ImmutableArray<Relation> OrderRelations(
        Carrier carrier,
        ImmutableArray<CarrierChainNode> nodes,
        ImmutableArray<Relation> relations)
    {
        var ordered = ImmutableArray.CreateBuilder<Relation>(relations.Length);

        foreach (var relation in relations)
        {
            if (Contains(nodes, relation.Origin) && Contains(nodes, relation.Anchor))
            {
                InsertRelation(ordered, carrier, relation);
            }
        }

        return ordered.ToImmutable();
    }

    private static void InsertNode(ImmutableArray<CarrierChainNode>.Builder nodes, CarrierChainNode node)
    {
        var location = node.Position.Normalize();

        for (var index = 0; index < nodes.Count; index++)
        {
            if (Compare(location, nodes[index].Position.Normalize()) < 0)
            {
                nodes.Insert(index, node);
                return;
            }
        }

        nodes.Add(node);
    }

    private static void InsertRelation(ImmutableArray<Relation>.Builder relations, Carrier carrier, Relation relation)
    {
        var start = LowerPosition(carrier, relation).Normalize();

        for (var index = 0; index < relations.Count; index++)
        {
            if (Compare(start, LowerPosition(carrier, relations[index]).Normalize()) < 0)
            {
                relations.Insert(index, relation);
                return;
            }
        }

        relations.Add(relation);
    }

    private static Ratio LowerPosition(Carrier carrier, Relation relation)
    {
        var origin = relation.Origin.On(carrier).Normalize();
        var anchor = relation.Anchor.On(carrier).Normalize();
        return Compare(origin, anchor) <= 0 ? origin : anchor;
    }

    private static int Compare(Ratio left, Ratio right)
        => left.CompareValue(right);

    private static bool SameNode(Site left, Site right) => left.Carrier == right.Carrier
        && left.On(left.Carrier).Normalize() == right.On(right.Carrier).Normalize();
}
