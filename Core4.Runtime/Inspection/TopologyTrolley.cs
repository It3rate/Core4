using System.Collections.Immutable;
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
    ISupportProjection CarriedCalibrationRead,
    ImmutableArray<TraversalSignature> TrailSignatures,
    ImmutableArray<Site> TrailSites,
    ImmutableArray<Relation> TrailRelations)
{
    public static TopologyTrolley Start(ConnectedSiteTopology topology, Site site)
    {
        var resolved = Canonicalize(topology, site);
        var calibration = InitialCalibration(resolved);
        var signature = SignatureOf(resolved, Measure.Zero, calibration, null);
        return new TopologyTrolley(
            topology,
            resolved,
            null,
            Measure.Zero,
            calibration,
            [signature],
            [resolved],
            ImmutableArray<Relation>.Empty);
    }

    public SiteConnector Connector => Topology.ConnectorAt(Site);

    public ImmutableArray<SiteBasisEncounter> BasisEncounters => Topology.BasisEncountersAt(Site);

    // TODO(core): Delete this compatibility alias once callers consume CarriedCalibrationRead directly.
    public Ratio CarriedCalibration => CarriedCalibrationRead.CompatibilityRatio();

    public TraversalSignature Signature => SignatureOf(Site, Direction, CarriedCalibrationRead, Ingress);

    public Measure ExactReentryPeriod => ReentryPeriod((prior, current) => prior.Equivalent(current));

    public Measure ProjectedReentryPeriod => ReentryPeriod((prior, current) => prior.SameProjection(current));

    public bool HasExactReentry => ExactReentryPeriod != Measure.Zero;

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
                relation.SpanRead,
                TraversalModifier.FromDirection(disposition.Direction),
                TraversalSignature.ProvenanceFor(disposition.Direction, relation.Potential));
            var basisAgreements = ImmutableArray.CreateBuilder<BasisEncounterAgreement>();

            foreach (var encounter in Topology.BasisEncountersAt(Site, relation.Carrier))
            {
                basisAgreements.Add(encounter.Agree(sourceSignature, disposition.Direction));
            }

            continuations.Add(new TopologyTrolleyContinuation(
                relation,
                Site,
                resolvedTarget,
                disposition,
                sourceSignature,
                targetSignature,
                basisAgreements.ToImmutable(),
                new SupportProjectionRead(new Ratio(Measure.One, Measure.One)),
                new SupportProjectionRead(Ratio.Zero(Measure.One))));
        }

        return new TopologyTrolleyObservation(this, continuations.ToImmutable());
    }

    public TopologyTrolleyObservation Observe(Measure observerPerspective) => Observe();

    public TopologyTrolley Move(TopologyTrolleyContinuation continuation)
    {
        var next = Canonicalize(Topology, continuation.To);
        var calibration = continuation.NextCalibrationRead;
        var nextSignature = SignatureOf(next, continuation.Direction, calibration, continuation.Relation);
        return new TopologyTrolley(
            Topology,
            next,
            continuation.Relation,
            continuation.Direction,
            calibration,
            TrailSignatures.Add(nextSignature),
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

    private static bool SameNode(Site left, Site right) => left.Carrier == right.Carrier
        && left.On(left.Carrier).Normalize() == right.On(right.Carrier).Normalize();

    private static TraversalSignature SignatureOf(
        Site site,
        Measure direction,
        ISupportProjection calibration,
        Relation? ingress) =>
        TraversalSignature.FromSite(
            site,
            calibration,
            TraversalModifier.FromDirection(direction),
            TraversalSignature.ProvenanceFor(direction, ingress?.Potential ?? site.Potential));

    private static ISupportProjection InitialCalibration(Site site)
    {
        if (site.State.Potential != Measure.Zero || site.State.Dominant != Measure.Zero)
        {
            return new SupportProjectionRead(site.State.Normalize().Canonicalize());
        }

        return new SupportProjectionRead(Ratio.Tension(site.Potential).Canonicalize());
    }

    private Measure ReentryPeriod(Func<TraversalSignature, TraversalSignature, bool> predicate)
    {
        if (TrailSignatures.Length < 2)
        {
            return Measure.Zero;
        }

        var current = TrailSignatures[^1];

        for (var index = TrailSignatures.Length - 2; index >= 0; index--)
        {
            if (predicate(TrailSignatures[index], current))
            {
                return new Measure(TrailSignatures.Length - 1 - index);
            }
        }

        return Measure.Zero;
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
    ISupportProjection TransmissionRead,
    ISupportProjection RetentionRead)
{
    public Carrier Carrier => Relation.Carrier;

    public TraversalContinuation Traversal => new(Carrier, SourceSignature, TargetSignature, Disposition);

    public Measure Potential => Relation.Potential;

    public Measure Direction => Disposition.Direction;

    public bool ContinuesCarrier => Disposition.ContinuesCarrier;

    public bool ReturnsAlongIngress => Disposition.ReturnsAlongIngress;

    public bool IsIncoming => Disposition.IsIncoming;

    public bool IsOutgoing => Disposition.IsOutgoing;

    // TODO(core): Delete this compatibility alias once callers consume TransmissionRead directly.
    public Ratio Transmission => TransmissionRead.CompatibilityRatio();

    public ISupportProjection TransmittedCalibrationRead =>
        SourceSignature.CalibrationRead.MultiplyRead(TransmissionRead);

    // TODO(core): Delete this compatibility alias once callers consume TransmittedCalibrationRead directly.
    public Ratio TransmittedCalibration => TransmittedCalibrationRead.CompatibilityRatio();

    public ISupportProjection RetainedCalibrationRead =>
        SourceSignature.CalibrationRead.MultiplyRead(RetentionRead);

    // TODO(core): Delete this compatibility alias once callers consume RetentionRead directly.
    public Ratio Retention => RetentionRead.CompatibilityRatio();

    // TODO(core): Delete this compatibility alias once callers consume RetainedCalibrationRead directly.
    public Ratio RetainedCalibration => RetainedCalibrationRead.CompatibilityRatio();

    public ISupportProjection NextCalibrationRead =>
        TargetSignature.CalibrationRead.MultiplyRead(TransmissionRead);

    public bool HasRetention => RetentionRead.Potential != Measure.Zero;

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
                if (!hasBest || agreement.ComparePreference(best) > 0)
                {
                    best = agreement;
                    hasBest = true;
                }
            }

            return best;
        }
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
        var bestAgreement = default(BasisEncounterAgreement);
        var hasBest = false;
        var ambiguous = false;

        foreach (var candidate in forward)
        {
            var candidateAgreement = candidate.HasBasisAgreement
                ? candidate.BestBasisAgreement
                : default;
            var comparison = hasBest
                ? candidateAgreement.CompareTraversalPreference(bestAgreement)
                : 1;

            if (!hasBest || comparison > 0)
            {
                best = candidate;
                bestAgreement = candidateAgreement;
                hasBest = true;
                ambiguous = false;
                continue;
            }

            if (comparison == 0)
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

    private ImmutableArray<TopologyTrolleyContinuation> ReflectiveReturns()
    {
        var continuations = ImmutableArray.CreateBuilder<TopologyTrolleyContinuation>();

        foreach (var continuation in Continuations)
        {
            if (!continuation.ReturnsAlongIngress || !TryGetExplicitReflectiveAgreement(continuation, out var agreement))
            {
                continue;
            }

            if (!agreement.HasCommittedTransfer)
            {
                continue;
            }

            continuations.Add(continuation with
            {
                TransmissionRead = agreement.TransmissionRead,
                RetentionRead = agreement.RetentionRead
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

            var agreement = encounter.Agree(Trolley.Signature, preservedDirection);

            if (!agreement.HasCommittedTransfer)
            {
                continue;
            }

            var continuationDirection = agreement.InvertsDirection
                ? preservedDirection
                : agreement.PreservesDirection
                    ? -preservedDirection
                    : Measure.Zero;

            if (continuationDirection == Measure.Zero)
            {
                continue;
            }

            var relation = CanonicalRelation(Trolley.Site, target, encounter.Carrier);
            var disposition = TraversalDisposition.Create(false, continuationDirection, false);
            var targetSignature = TraversalSignature.FromSite(
                target,
                relation.SpanRead,
                TraversalModifier.FromDirection(continuationDirection),
                TraversalSignature.ProvenanceFor(continuationDirection, relation.Potential));

            continuations.Add(new TopologyTrolleyContinuation(
                relation,
                Trolley.Site,
                target,
                disposition,
                Trolley.Signature,
                targetSignature,
                [agreement],
                agreement.TransmissionRead,
                agreement.RetentionRead));
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
            var hasClosureTarget = TryResolveClosureTarget(agreement.Encounter, out _);

            if (agreement.CanReflectAtLocalPinnedBoundary(hasClosureTarget))
            {
                if (!hasSelected || agreement.CompareTransferPreference(selected) > 0)
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
        return leftPosition.CompareValue(rightPosition) <= 0
            ? new Relation(carrier, left, right)
            : new Relation(carrier, right, left);
    }

    private static bool SameNode(Site left, Site right) => left.Carrier == right.Carrier
        && left.On(left.Carrier).Normalize() == right.On(right.Carrier).Normalize();
}
