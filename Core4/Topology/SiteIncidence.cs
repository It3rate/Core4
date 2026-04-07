using System.Collections.Immutable;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.Topology;

/// <summary>
/// The first topology layer for one site: which support legs meet here, what directions they carry, and which
/// carriers continue through versus terminate. Branch and join remain derived facts rather than symbolic labels.
/// </summary>
public readonly record struct SiteIncidence(Site Site, ImmutableArray<SiteLeg> Legs)
{
    public Measure LegCount => new(Legs.Length);

    public Measure CarrierCount => new(Carriers().Length);

    public Measure IncomingLegCount => Count(leg => leg.IsIncoming);

    public Measure OutgoingLegCount => Count(leg => leg.IsOutgoing);

    public Measure OscillatingLegCount => Count(leg => leg.IsOscillating);

    public Measure ContinuingCarrierCount => CountCarriers(Continues);

    public Measure TerminalCarrierCount => CountCarriers(Terminates);

    public bool SiteCarrierContinues => Continues(Site.Carrier);

    public static SiteIncidence Create(Site site, params Relation[] alignedRelations)
    {
        var legs = ImmutableArray.CreateBuilder<SiteLeg>(alignedRelations.Length + site.Orthogonal.Flows.Length);

        foreach (var relation in alignedRelations)
        {
            if (SiteLeg.TryFromRelation(relation, site, out var leg))
            {
                legs.Add(leg);
            }
        }

        foreach (var flow in site.Orthogonal.Flows)
        {
            legs.Add(SiteLeg.FromFlow(flow));
        }

        return new SiteIncidence(site, legs.ToImmutable());
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
