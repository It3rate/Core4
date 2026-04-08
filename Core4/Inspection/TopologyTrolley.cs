using System.Collections.Immutable;
using Core4.Dynamics;
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
    ImmutableArray<Relation> TrailRelations,
    TraversalHistory History)
{
    public static TopologyTrolley Start(ConnectedSiteTopology topology, Site site)
    {
        var resolved = Canonicalize(topology, site);
        var signature = SignatureOf(resolved, Measure.Zero, null);
        return new TopologyTrolley(
            topology,
            resolved,
            null,
            Measure.Zero,
            [resolved],
            ImmutableArray<Relation>.Empty,
            TraversalHistory.Start(signature));
    }

    public SiteConnector Connector => Topology.ConnectorAt(Site);

    public TraversalSignature Signature => SignatureOf(Site, Direction, Ingress);

    public TopologyTrolleyObservation Observe()
    {
        var continuations = ImmutableArray.CreateBuilder<TopologyTrolleyContinuation>();
        var incidence = Topology.IncidenceAt(Site);
        var sourceSignature = Signature;

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
            var disposition = TraversalDisposition.Create(continuesCarrier, leg.Direction, returnsAlongIngress);
            var targetSignature = TraversalSignature.FromSite(
                resolvedTarget,
                disposition.Direction,
                relation.Span.Normalize(),
                TraversalSignature.ProvenanceFor(disposition.Direction, relation.Potential));
            var reentry = History.Observe(targetSignature);

            continuations.Add(new TopologyTrolleyContinuation(
                relation,
                Site,
                resolvedTarget,
                disposition,
                sourceSignature,
                targetSignature,
                reentry));
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
            TrailRelations.Add(continuation.Relation),
            History.Append(continuation.TargetSignature));
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
                TrailRelations.RemoveAt(TrailRelations.Length - 1),
                History.TrimLast());
            return true;
        }

        next = default;
        return false;
    }

    private static bool SameNode(Site left, Site right) => left.Carrier == right.Carrier && left.Location == right.Location;

    private static TraversalSignature SignatureOf(Site site, Measure direction, Relation? ingress) =>
        TraversalSignature.FromSite(
            site,
            direction,
            Carry(site, ingress),
            TraversalSignature.ProvenanceFor(direction, ingress?.Potential ?? site.Potential));

    private static Ratio Carry(Site site, Relation? ingress)
    {
        if (ingress.HasValue)
        {
            return ingress.Value.Span.Normalize();
        }

        if (site.State.Potential != Measure.Zero || site.State.Dominant != Measure.Zero)
        {
            return site.State.Normalize();
        }

        return Ratio.Tension(site.Potential);
    }

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
    TraversalDisposition Disposition,
    TraversalSignature SourceSignature,
    TraversalSignature TargetSignature,
    TraversalReentry Reentry)
{
    public Carrier Carrier => Relation.Carrier;

    public TraversalContinuation Traversal => new(Carrier, SourceSignature, TargetSignature, Disposition, Reentry);

    public Measure Potential => Relation.Potential;

    public Measure Direction => Disposition.Direction;

    public bool ContinuesCarrier => Disposition.ContinuesCarrier;

    public bool ReturnsAlongIngress => Disposition.ReturnsAlongIngress;

    public bool IsIncoming => Disposition.IsIncoming;

    public bool IsOutgoing => Disposition.IsOutgoing;
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

    public TraversalObservation Traversal
    {
        get
        {
            var continuations = ImmutableArray.CreateBuilder<TraversalContinuation>(Continuations.Length);

            foreach (var continuation in Continuations)
            {
                continuations.Add(continuation.Traversal);
            }

            return new TraversalObservation(Trolley.Signature, continuations.ToImmutable());
        }
    }

    public ImmutableArray<TopologyTrolleyContinuation> ForwardContinuations
    {
        get
        {
            var forward = Traversal.ForwardIndexes();
            var continuations = ImmutableArray.CreateBuilder<TopologyTrolleyContinuation>(forward.Length);

            foreach (var index in forward)
            {
                continuations.Add(Continuations[index]);
            }

            return continuations.ToImmutable();
        }
    }

    public bool IsDecisionPoint => ForwardContinuations.Length > 1;

    public bool TryPreferred(out TopologyTrolleyContinuation continuation)
    {
        if (Traversal.TryPreferredIndex(out var index))
        {
            continuation = Continuations[index];
            return true;
        }

        continuation = default;
        return false;
    }
}
