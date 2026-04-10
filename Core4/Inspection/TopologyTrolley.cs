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

    public ImmutableArray<SiteBasisEncounter> BasisEncounters => Topology.BasisEncountersAt(Site);

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
            var basisAgreements = ImmutableArray.CreateBuilder<BasisEncounterAgreement>();

            foreach (var encounter in Topology.BasisEncountersAt(Site, relation.Carrier))
            {
                basisAgreements.Add(BasisEncounterAgreement.Create(sourceSignature, encounter, disposition.Direction));
            }

            continuations.Add(new TopologyTrolleyContinuation(
                relation,
                Site,
                resolvedTarget,
                disposition,
                sourceSignature,
                targetSignature,
                basisAgreements.ToImmutable(),
                new Ratio(Measure.One, Measure.One),
                Ratio.Zero(Measure.One)));
        }

        return new TopologyTrolleyObservation(this, continuations.ToImmutable());
    }

    public TopologyTrolleyObservation Observe(Measure observerPerspective) => Observe();

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
        var observation = Observe(Measure.One);

        if (observation.TryPreferred(Measure.One, out var continuation))
        {
            next = Move(continuation);
            return true;
        }

        next = default;
        return false;
    }

    public bool TryContinuePreferred(Measure observerPerspective, out TopologyTrolley next)
    {
        var observation = Observe(observerPerspective);

        if (observation.TryPreferred(observerPerspective, out var continuation))
        {
            next = Move(continuation);
            return true;
        }

        next = default;
        return false;
    }

    public ImmutableArray<TopologyTrolley> ForkAll()
    {
        var observation = Observe(Measure.One);
        var forks = ImmutableArray.CreateBuilder<TopologyTrolley>(observation.ForwardContinuations.Length);

        foreach (var continuation in observation.ForwardContinuations)
        {
            forks.Add(Move(continuation));
        }

        return forks.ToImmutable();
    }

    public ImmutableArray<TopologyTrolley> ForkAll(Measure observerPerspective)
    {
        var observation = Observe(observerPerspective);
        var forks = ImmutableArray.CreateBuilder<TopologyTrolley>(observation.ForwardContinuationsIn(observerPerspective).Length);

        foreach (var continuation in observation.ForwardContinuationsIn(observerPerspective))
        {
            forks.Add(Move(continuation));
        }

        return forks.ToImmutable();
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
    ImmutableArray<BasisEncounterAgreement> BasisAgreements,
    Ratio Transmission,
    Ratio Retention)
{
    public Carrier Carrier => Relation.Carrier;

    public TraversalContinuation Traversal => new(Carrier, SourceSignature, TargetSignature, Disposition);

    public Measure Potential => Relation.Potential;

    public Measure Direction => Disposition.Direction;

    public bool ContinuesCarrier => Disposition.ContinuesCarrier;

    public bool ReturnsAlongIngress => Disposition.ReturnsAlongIngress;

    public bool IsIncoming => Disposition.IsIncoming;

    public bool IsOutgoing => Disposition.IsOutgoing;

    public bool IsPartialTransfer => Transmission != new Ratio(Measure.One, Measure.One)
        || Retention != Ratio.Zero(Measure.One);

    public bool HasBasisAgreement => BasisAgreements.Length != 0;

    public BasisEncounterAgreement BestBasisAgreement
    {
        get
        {
            var best = default(BasisEncounterAgreement);
            var hasBest = false;

            foreach (var agreement in BasisAgreements)
            {
                if (!hasBest || Compare(agreement, best) > 0)
                {
                    best = agreement;
                    hasBest = true;
                }
            }

            return best;
        }
    }

    private static int Compare(Ratio left, Ratio right)
    {
        var leftScaled = checked(left.Recessive.Raw * right.Dominant.Raw);
        var rightScaled = checked(right.Recessive.Raw * left.Dominant.Raw);
        return leftScaled.CompareTo(rightScaled);
    }

    private static int Compare(BasisEncounterAgreement left, BasisEncounterAgreement right)
    {
        var continuation = Compare(left.Continuation, right.Continuation);

        if (continuation != 0)
        {
            return continuation;
        }

        var capability = Compare(left.Capability, right.Capability);

        if (capability != 0)
        {
            return capability;
        }

        var commitment = Compare(left.Commitment, right.Commitment);

        if (commitment != 0)
        {
            return commitment;
        }

        return Compare(left.Resolution, right.Resolution);
    }
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

    public ImmutableArray<SiteBasisEncounter> BasisEncounters => Trolley.BasisEncounters;

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

    public ImmutableArray<TopologyTrolleyContinuation> ForwardContinuations => ForwardContinuationsIn(Measure.One);

    public ImmutableArray<TopologyTrolleyContinuation> ForwardContinuationsIn(Measure observerPerspective)
    {
        var forward = Traversal.ForwardIndexes(observerPerspective);
        var continuations = ImmutableArray.CreateBuilder<TopologyTrolleyContinuation>(forward.Length);

        foreach (var index in forward)
        {
            continuations.Add(Continuations[index]);
        }

        if (observerPerspective.Raw >= 0L && continuations.Count == 0)
        {
            foreach (var continuation in ClosureContinuations(observerPerspective))
            {
                continuations.Add(continuation);
            }
        }

        if (observerPerspective.Raw >= 0L && continuations.Count == 0)
        {
            foreach (var continuation in ReflectiveReturns())
            {
                continuations.Add(continuation);
            }
        }

        return continuations.ToImmutable();
    }

    public bool IsDecisionPoint => ForwardContinuations.Length > 1;

    public bool TryPreferred(out TopologyTrolleyContinuation continuation) =>
        TryPreferred(Measure.One, out continuation);

    public bool TryPreferred(Measure observerPerspective, out TopologyTrolleyContinuation continuation)
    {
        if (Traversal.TryPreferredIndex(observerPerspective, out var index))
        {
            continuation = Continuations[index];
            return true;
        }

        var forward = ForwardContinuationsIn(observerPerspective);

        if (forward.Length == 0)
        {
            continuation = default;
            return false;
        }

        if (forward.Length == 1)
        {
            continuation = forward[0];
            return true;
        }

        var best = default(TopologyTrolleyContinuation);
        var bestContinuation = new Ratio(-Measure.One, Measure.One);
        var bestCapability = Ratio.Zero(Measure.One);
        var bestCommitment = Ratio.Zero(Measure.One);
        var hasBest = false;
        var ambiguous = false;

        foreach (var candidate in forward)
        {
            var continuationRank = candidate.HasBasisAgreement
                ? candidate.BestBasisAgreement.Continuation
                : Ratio.Zero(Measure.One);
            var capabilityRank = candidate.HasBasisAgreement
                ? candidate.BestBasisAgreement.Capability
                : Ratio.Zero(Measure.One);
            var commitmentRank = candidate.HasBasisAgreement
                ? candidate.BestBasisAgreement.Commitment
                : Ratio.Zero(Measure.One);
            var continuationComparison = Compare(continuationRank, bestContinuation);
            var capabilityComparison = continuationComparison == 0
                ? Compare(capabilityRank, bestCapability)
                : 0;
            var commitmentComparison = continuationComparison == 0 && capabilityComparison == 0
                ? Compare(commitmentRank, bestCommitment)
                : 0;

            if (!hasBest
                || continuationComparison > 0
                || (continuationComparison == 0 && capabilityComparison > 0)
                || (continuationComparison == 0 && capabilityComparison == 0 && commitmentComparison > 0))
            {
                best = candidate;
                bestContinuation = continuationRank;
                bestCapability = capabilityRank;
                bestCommitment = commitmentRank;
                hasBest = true;
                ambiguous = false;
                continue;
            }

            if (continuationComparison == 0 && capabilityComparison == 0 && commitmentComparison == 0)
            {
                ambiguous = true;
            }
        }

        if (ambiguous || !hasBest)
        {
            continuation = default;
            return false;
        }

        continuation = best;
        return true;
    }

    private static int Compare(Ratio left, Ratio right)
    {
        var leftScaled = checked(left.Recessive.Raw * right.Dominant.Raw);
        var rightScaled = checked(right.Recessive.Raw * left.Dominant.Raw);
        return leftScaled.CompareTo(rightScaled);
    }

    private ImmutableArray<TopologyTrolleyContinuation> ReflectiveReturns()
    {
        var continuations = ImmutableArray.CreateBuilder<TopologyTrolleyContinuation>();

        foreach (var continuation in Continuations)
        {
            if (!continuation.ReturnsAlongIngress || !TryGetExplicitReflectiveAgreement(continuation, out var agreement))
            {
                continue;
            }

            continuations.Add(continuation with
            {
                Transmission = agreement.Commitment.Canonicalize(),
                Retention = new Ratio(Measure.One, Measure.One).Subtract(agreement.Commitment).Canonicalize()
            });
        }

        return continuations.ToImmutable();
    }

    private ImmutableArray<TopologyTrolleyContinuation> ClosureContinuations(Measure observerPerspective)
    {
        var continuations = ImmutableArray.CreateBuilder<TopologyTrolleyContinuation>();
        var preservedDirection = Trolley.Signature.EffectiveDirection.Raw != 0L
            ? Trolley.Signature.EffectiveDirection.Sign
            : observerPerspective.Sign;

        foreach (var encounter in BasisEncounters)
        {
            if (!TryResolveClosureTarget(encounter, out var target))
            {
                continue;
            }

            var agreement = BasisEncounterAgreement.Create(Trolley.Signature, encounter, preservedDirection);

            if (!agreement.InvertsDirection)
            {
                continue;
            }

            var relation = CanonicalRelation(Trolley.Site, target, encounter.Carrier);
            var disposition = TraversalDisposition.Create(false, preservedDirection, false);
            var targetSignature = TraversalSignature.FromSite(
                target,
                relation.Span.Normalize(),
                TraversalModifier.FromDirection(preservedDirection),
                TraversalSignature.ProvenanceFor(preservedDirection, relation.Potential));

            continuations.Add(new TopologyTrolleyContinuation(
                relation,
                Trolley.Site,
                target,
                disposition,
                Trolley.Signature,
                targetSignature,
                [agreement],
                agreement.Commitment.Canonicalize(),
                new Ratio(Measure.One, Measure.One).Subtract(agreement.Commitment).Canonicalize()));
        }

        return continuations.ToImmutable();
    }

    private bool TryGetExplicitReflectiveAgreement(
        TopologyTrolleyContinuation continuation,
        out BasisEncounterAgreement selected)
    {
        var hasSelected = false;
        selected = default;

        foreach (var agreement in continuation.BasisAgreements)
        {
            if (agreement.Encounter.IsExplicitPinnedBasis
                && agreement.Encounter.IsLocalToSite
                && !TryResolveClosureTarget(agreement.Encounter, out _)
                && agreement.PreservesDirection)
            {
                if (!hasSelected || Compare(agreement.Commitment, selected.Commitment) > 0)
                {
                    selected = agreement;
                    hasSelected = true;
                }
            }
        }

        return hasSelected;
    }

    private bool TryResolveClosureTarget(SiteBasisEncounter encounter, out Site target)
    {
        if (!encounter.TryGetExplicitRemap(out var remapTarget, out _))
        {
            target = default;
            return false;
        }

        foreach (var site in Trolley.Topology.Sites)
        {
            if (SameNode(site, remapTarget))
            {
                target = Trolley.Topology.IncidenceAt(site).Site;
                return true;
            }
        }

        target = default;
        return false;
    }

    private static Relation CanonicalRelation(Site left, Site right, Carrier carrier)
    {
        var leftPosition = left.On(carrier).Normalize();
        var rightPosition = right.On(carrier).Normalize();
        return Compare(leftPosition, rightPosition) <= 0
            ? new Relation(carrier, left, right)
            : new Relation(carrier, right, left);
    }

    private static bool SameNode(Site left, Site right) => left.Carrier == right.Carrier && left.Location == right.Location;
}
