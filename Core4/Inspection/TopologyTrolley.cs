using System.Collections.Immutable;
using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;

namespace Core4.Inspection;

/// <summary>
/// A small external inspector that rides a connected topology one relation at a time. It never mutates the topology:
/// it only tracks a current site, the relation used to arrive there, and the trail it has taken so far.
/// </summary>
public readonly record struct TopologyTrolley(
    ConnectedSiteTopology Topology,
    Site Site,
    Relation? Ingress,
    Measure Direction,
    ImmutableArray<Site> TrailSites,
    ImmutableArray<Relation> TrailRelations)
{
    public static TopologyTrolley Start(ConnectedSiteTopology topology, Site site)
    {
        var resolved = Canonicalize(topology, site);
        return new TopologyTrolley(
            topology,
            resolved,
            null,
            Measure.Zero,
            [resolved],
            ImmutableArray<Relation>.Empty);
    }

    public SiteConnector Connector => Topology.ConnectorAt(Site);

    public TopologyTrolleyObservation Observe()
    {
        var continuations = ImmutableArray.CreateBuilder<TopologyTrolleyContinuation>();
        var incidence = Topology.IncidenceAt(Site);

        foreach (var leg in incidence.Legs)
        {
            if (leg.Support is not Relation relation || leg.IsOscillating)
            {
                continue;
            }

            var target = leg.IsOutgoing
                ? relation.Anchor
                : relation.Origin;

            if (SameNode(target, Site))
            {
                continue;
            }

            var resolvedTarget = Topology.IncidenceAt(target).Site;
            var returnsAlongIngress = Ingress.HasValue && relation == Ingress.Value;
            var continuesCarrier = Ingress.HasValue
                ? relation != Ingress.Value && relation.Carrier == Ingress.Value.Carrier
                : relation.Carrier == Site.Carrier;

            continuations.Add(new TopologyTrolleyContinuation(
                relation,
                Site,
                resolvedTarget,
                leg.Direction,
                continuesCarrier,
                returnsAlongIngress));
        }

        return new TopologyTrolleyObservation(this, continuations.ToImmutable());
    }

    public TopologyTrolley Move(TopologyTrolleyContinuation continuation)
    {
        var next = Canonicalize(Topology, continuation.To);
        return new TopologyTrolley(
            Topology,
            next,
            continuation.Relation,
            continuation.Direction,
            TrailSites.Add(next),
            TrailRelations.Add(continuation.Relation));
    }

    public bool TryContinuePreferred(out TopologyTrolley next)
    {
        var observation = Observe();

        if (observation.TryPreferred(out var continuation))
        {
            next = Move(continuation);
            return true;
        }

        next = default;
        return false;
    }

    public ImmutableArray<TopologyTrolley> ForkAll()
    {
        var observation = Observe();
        var forks = ImmutableArray.CreateBuilder<TopologyTrolley>(observation.ForwardContinuations.Length);

        foreach (var continuation in observation.ForwardContinuations)
        {
            forks.Add(Move(continuation));
        }

        return forks.ToImmutable();
    }

    public bool TryReverse(out TopologyTrolley next)
    {
        if (TrailSites.Length >= 2 && TrailRelations.Length >= 1)
        {
            var previousSite = TrailSites[^2];
            var previousIngress = TrailRelations.Length >= 2
                ? TrailRelations[^2]
                : (Relation?)null;
            var previousDirection = previousIngress.HasValue
                ? TrailRelations[^1] == previousIngress.Value
                    ? Direction
                    : -Direction
                : Measure.Zero;

            next = new TopologyTrolley(
                Topology,
                previousSite,
                previousIngress,
                previousDirection,
                TrailSites.RemoveAt(TrailSites.Length - 1),
                TrailRelations.RemoveAt(TrailRelations.Length - 1));
            return true;
        }

        next = default;
        return false;
    }

    private static bool SameNode(Site left, Site right) => left.Carrier == right.Carrier && left.Location == right.Location;

    private static Site Canonicalize(ConnectedSiteTopology topology, Site site)
    {
        var resolved = topology.IncidenceAt(site).Site;
        var pin = resolved.On(resolved.Carrier).Normalize();
        var best = resolved;
        var bestCarrierCount = topology.ConnectorAt(best).Carriers.Length;
        var bestLegCount = topology.IncidenceAt(best).LegCount.Raw;

        foreach (var candidate in topology.Sites)
        {
            var geometry = topology.GeometryAt(candidate, resolved.Carrier);
            var candidatePin = geometry.Pin.Normalize();

            if (!candidatePin.IsResolved || candidatePin != pin)
            {
                continue;
            }

            var connector = topology.ConnectorAt(candidate);
            var legCount = topology.IncidenceAt(candidate).LegCount.Raw;

            if (connector.Carriers.Length > bestCarrierCount
                || (connector.Carriers.Length == bestCarrierCount && legCount > bestLegCount))
            {
                best = candidate;
                bestCarrierCount = connector.Carriers.Length;
                bestLegCount = legCount;
            }
        }

        return best;
    }
}

/// <summary>
/// One admissible next relation from the trolley's current site. It can either continue forward, branch onto a new
/// carrier, or reverse back along the ingress relation.
/// </summary>
public readonly record struct TopologyTrolleyContinuation(
    Relation Relation,
    Site From,
    Site To,
    Measure Direction,
    bool ContinuesCarrier,
    bool ReturnsAlongIngress)
{
    public Carrier Carrier => Relation.Carrier;

    public Measure Potential => Relation.Potential;

    public bool IsIncoming => Direction.Raw < 0;

    public bool IsOutgoing => Direction.Raw > 0;
}

/// <summary>
/// A site-local trolley reading: where the trolley is, what connector it is sitting on, and which continuations are
/// available before a caller chooses to continue, fork, or reverse.
/// </summary>
public readonly record struct TopologyTrolleyObservation(
    TopologyTrolley Trolley,
    ImmutableArray<TopologyTrolleyContinuation> Continuations)
{
    public Site Site => Trolley.Site;

    public SiteConnector Connector => Trolley.Connector;

    public ImmutableArray<TopologyTrolleyContinuation> ForwardContinuations
    {
        get
        {
            var continuations = ImmutableArray.CreateBuilder<TopologyTrolleyContinuation>();

            foreach (var continuation in Continuations)
            {
                if (!continuation.ReturnsAlongIngress && !continuation.IsIncoming)
                {
                    continuations.Add(continuation);
                }
            }

            return continuations.ToImmutable();
        }
    }

    public bool IsDecisionPoint => ForwardContinuations.Length > 1;

    public bool TryPreferred(out TopologyTrolleyContinuation continuation)
    {
        var forward = ForwardContinuations;
        var continued = ImmutableArray.CreateBuilder<TopologyTrolleyContinuation>();

        foreach (var candidate in forward)
        {
            if (candidate.ContinuesCarrier)
            {
                continued.Add(candidate);
            }
        }

        if (continued.Count == 1)
        {
            continuation = continued[0];
            return true;
        }

        if (forward.Length == 1)
        {
            continuation = forward[0];
            return true;
        }

        continuation = default;
        return false;
    }
}
