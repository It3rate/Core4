using System.Collections.Immutable;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.Topology;

/// <summary>
/// The first topology layer for one site: which support legs meet here, what directions they carry, and which
/// carriers continue through versus terminate. Branch and join remain derived facts rather than symbolic labels.
/// </summary>
public readonly record struct SiteIncidence(Site Site, ImmutableArray<Relation> AlignedRelations)
{
    // TODO: Prefer querying this through ConnectedSiteTopology. This local form remains useful as a thin computed
    // view while stage-3 connected structures continue to grow.
    public ImmutableArray<SiteLeg> Legs
    {
        get
        {
            var legs = ImmutableArray.CreateBuilder<SiteLeg>(AlignedRelations.Length + Site.Orthogonal.Flows.Length);

            foreach (var relation in AlignedRelations)
            {
                if (SiteLeg.TryFromRelation(relation, Site, out var leg))
                {
                    legs.Add(leg);
                }
            }

            foreach (var flow in Site.Orthogonal.Flows)
            {
                legs.Add(SiteLeg.FromFlow(flow));
            }

            return legs.ToImmutable();
        }
    }

    public Measure LegCount => new(Legs.Length);

    public Measure CarrierCount => new(Carriers().Length);

    public Measure IncomingLegCount => Count(leg => leg.IsIncoming);

    public Measure OutgoingLegCount => Count(leg => leg.IsOutgoing);

    public Measure OscillatingLegCount => Count(leg => leg.IsOscillating);

    public Measure ContinuingCarrierCount => CountCarriers(Continues);

    public Measure TerminalCarrierCount => CountCarriers(Terminates);

    public bool SiteCarrierContinues => Continues(Site.Carrier);

    public static SiteIncidence Create(Site site, params Relation[] alignedRelations) => new(site, [.. alignedRelations]);

    public AlignedSiteSpan GeometryOn(Carrier carrier) => AlignedSiteSpan.Create(Site, carrier, [.. LegsOn(carrier)]);

    public ImmutableArray<SiteBasisEncounter> BasisEncounters()
    {
        var encounters = ImmutableArray.CreateBuilder<SiteBasisEncounter>();

        foreach (var carrier in Carriers())
        {
            foreach (var encounter in BasisEncountersOn(carrier))
            {
                encounters.Add(encounter);
            }
        }

        return encounters.ToImmutable();
    }

    public ImmutableArray<SiteBasisEncounter> BasisEncountersOn(Carrier carrier)
    {
        var incoming = ImmutableArray.CreateBuilder<Relation>();
        var outgoing = ImmutableArray.CreateBuilder<Relation>();
        var encounters = ImmutableArray.CreateBuilder<SiteBasisEncounter>();

        foreach (var leg in LegsOn(carrier))
        {
            if (leg.Support is Basis basis && basis.PrimaryCarrier == carrier)
            {
                encounters.Add(SiteBasisEncounter.FromPinnedBasis(Site, carrier, basis));
                continue;
            }

            if (leg.Support is not Relation relation)
            {
                continue;
            }

            if (leg.IsIncoming)
            {
                incoming.Add(relation);
                encounters.Add(SiteBasisEncounter.FromIncoming(Site, relation));
            }
            else if (leg.IsOutgoing)
            {
                outgoing.Add(relation);
                encounters.Add(SiteBasisEncounter.FromOutgoing(Site, relation));
            }
        }

        if (incoming.Count == 1 && outgoing.Count == 1)
        {
            encounters.Insert(0, SiteBasisEncounter.FromPassThrough(Site, incoming[0], outgoing[0]));
        }

        return encounters.ToImmutable();
    }

    public ImmutableArray<Carrier> Carriers()
    {
        var carriers = ImmutableArray.CreateBuilder<Carrier>();

        foreach (var leg in Legs)
        {
            if (!carriers.Contains(leg.Carrier))
            {
                carriers.Add(leg.Carrier);
            }
        }

        return carriers.ToImmutable();
    }

    public ImmutableArray<SiteLeg> LegsOn(Carrier carrier)
    {
        var legs = ImmutableArray.CreateBuilder<SiteLeg>();

        foreach (var leg in Legs)
        {
            if (leg.Carrier == carrier)
            {
                legs.Add(leg);
            }
        }

        return legs.ToImmutable();
    }

    public ImmutableArray<Relation> AlignedOn(Carrier carrier)
    {
        var relations = ImmutableArray.CreateBuilder<Relation>();

        foreach (var relation in AlignedRelations)
        {
            if (relation.Carrier == carrier && SiteLeg.TryFromRelation(relation, Site, out _))
            {
                relations.Add(relation);
            }
        }

        return relations.ToImmutable();
    }

    public bool HasIncoming(Carrier carrier)
    {
        foreach (var leg in Legs)
        {
            if (leg.Carrier == carrier && leg.IsIncoming)
            {
                return true;
            }
        }

        return false;
    }

    public bool HasOutgoing(Carrier carrier)
    {
        foreach (var leg in Legs)
        {
            if (leg.Carrier == carrier && leg.IsOutgoing)
            {
                return true;
            }
        }

        return false;
    }

    public bool HasOscillation(Carrier carrier)
    {
        foreach (var leg in Legs)
        {
            if (leg.Carrier == carrier && leg.IsOscillating)
            {
                return true;
            }
        }

        return false;
    }

    public bool Continues(Carrier carrier) => HasIncoming(carrier) && HasOutgoing(carrier);

    public bool Terminates(Carrier carrier)
    {
        foreach (var leg in Legs)
        {
            if (leg.Carrier != carrier)
            {
                continue;
            }

            return !Continues(carrier);
        }

        return false;
    }

    private Measure Count(Func<SiteLeg, bool> predicate)
    {
        long total = 0L;

        foreach (var leg in Legs)
        {
            if (predicate(leg))
            {
                total++;
            }
        }

        return new Measure(total);
    }

    private Measure CountCarriers(Func<Carrier, bool> predicate)
    {
        long total = 0L;

        foreach (var carrier in Carriers())
        {
            if (predicate(carrier))
            {
                total++;
            }
        }

        return new Measure(total);
    }
}
