using System.Collections.Immutable;
using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;

namespace Core4.Dynamics;

/// <summary>
/// A locality-preserving carrier field. Sites remain purely local claim machines; this layer transports resolved
/// carrier-backed publication between listening sites while leaving unresolved orthogonal residual local unless some
/// later law explicitly republishes it. Read-only orthogonal site publications ride the same listener ports, resolved
/// carrier requests return read-only feedback back to the original publisher on the same carrier, and sender-side
/// feedback summaries can now retune next-round carrier publication numerically instead of remaining note-only.
/// </summary>
public readonly record struct CarrierCommunication(ImmutableArray<Site> Sites, ImmutableArray<Relation> Relations)
{
    public static CarrierCommunication Create(ConnectedSiteTopology topology) => new(topology.Sites, topology.Relations);

    public CarrierCommunicationState Seed() => new(
        this,
        Sites,
        ImmutableArray<SiteSettlement>.Empty,
        ImmutableArray<CarrierFieldState>.Empty,
        ImmutableArray<CarrierTransfer>.Empty,
        ImmutableArray<CarrierPublicationFieldState>.Empty,
        ImmutableArray<CarrierPublicationTransfer>.Empty,
        ImmutableArray<CarrierFeedbackFieldState>.Empty,
        ImmutableArray<CarrierFeedbackTransfer>.Empty,
        ImmutableArray<CarrierFeedbackSummary>.Empty,
        Measure.Zero,
        Ratio.Zero(Measure.One));
}

/// <summary>
/// One carrier-level resolved publication transfer produced by a single communication step.
/// </summary>
public readonly record struct CarrierTransfer(Carrier Carrier, Placement Source, Placement Target, Ratio Value);

/// <summary>
/// One carrier-level read-only publication transfer produced by a single communication step.
/// </summary>
public readonly record struct CarrierPublicationTransfer(
    Carrier Carrier,
    Placement Source,
    Placement Target,
    OrthogonalPublication Publication);

/// <summary>
/// One carrier-level read-only feedback transfer produced by a single communication step.
/// </summary>
public readonly record struct CarrierFeedbackTransfer(
    Carrier Carrier,
    Placement Source,
    Placement Target,
    OrthogonalFeedback Feedback);

/// <summary>
/// One aggregate resolved publication field state for a single communication round.
/// </summary>
public readonly record struct CarrierFieldState(
    Carrier Carrier,
    Ratio Published,
    Ratio Delivered,
    Ratio Retained,
    Measure EmitterWeight,
    Measure ListenerWeight,
    Measure TransferCount)
{
    public Ratio DeliveryFraction => Published.Potential == Measure.Zero
        ? Ratio.Zero(Measure.One)
        : new Ratio(Delivered.Potential, Published.Potential).Normalize();
}

/// <summary>
/// One aggregate publication field state for a single communication round.
/// </summary>
public readonly record struct CarrierPublicationFieldState(
    Carrier Carrier,
    Measure PublishedCount,
    Measure DeliveredCount,
    Measure ListenerCount,
    Measure TransferCount);

/// <summary>
/// One aggregate feedback field state for a single communication round.
/// </summary>
public readonly record struct CarrierFeedbackFieldState(
    Carrier Carrier,
    Ratio Requested,
    Ratio Delivered,
    Ratio Retained,
    Measure PublisherCount,
    Measure RequesterCount,
    Measure TransferCount);

/// <summary>
/// One sender-side aggregate of feedback delivered back to a published carrier output.
/// </summary>
public readonly record struct CarrierFeedbackSummary(
    Carrier Carrier,
    Placement Target,
    Ratio Published,
    Ratio Preferred,
    Ratio DesiredResolution,
    Ratio ValueDelta,
    Ratio ResolutionDelta,
    Ratio ValueSpread,
    Ratio ResolutionSpread,
    Measure FeedbackCount);

/// <summary>
/// One repeated carrier communication field over locally settling sites. Carrier-backed flows act as ports here:
/// they do not feed the site's same-pass local settlement, but resolved outgoing values can publish across them
/// between rounds. Carrier-backed publication payloads are also delivered here as read-only site references, while
/// resolved incoming carrier requests are interpreted as read-only feedback about those publications and can retune
/// the sender's next-round carrier publication surface through a numeric participation law.
/// </summary>
public readonly record struct CarrierCommunicationState(
    CarrierCommunication Communication,
    ImmutableArray<Site> Sites,
    ImmutableArray<SiteSettlement> Settlements,
    ImmutableArray<CarrierFieldState> Fields,
    ImmutableArray<CarrierTransfer> Transfers,
    ImmutableArray<CarrierPublicationFieldState> PublicationFields,
    ImmutableArray<CarrierPublicationTransfer> PublicationTransfers,
    ImmutableArray<CarrierFeedbackFieldState> FeedbackFields,
    ImmutableArray<CarrierFeedbackTransfer> FeedbackTransfers,
    ImmutableArray<CarrierFeedbackSummary> FeedbackSummaries,
    Measure Iteration,
    Ratio MaxTransfer)
{
    public ConnectedSiteTopology Topology => ConnectedSiteTopology.Create(Sites, [.. Communication.Relations]);

    public Site At(Site site)
    {
        foreach (var candidate in Sites)
        {
            if (SameNode(candidate, site))
            {
                return candidate;
            }
        }

        return site;
    }

    public CarrierCommunicationState Relax()
    {
        var settlements = ImmutableArray.CreateBuilder<SiteSettlement>(Sites.Length);
        var nextSites = ImmutableArray.CreateBuilder<Site>(Sites.Length);
        var emissionsByCarrier = new Dictionary<Carrier, List<CarrierValueEmission>>();
        var publicationByCarrier = new Dictionary<Carrier, List<CarrierPublicationEmission>>();
        var feedbackByCarrier = new Dictionary<Carrier, List<CarrierFeedbackEmission>>();
        var publishersByCarrier = new Dictionary<Carrier, Dictionary<int, PublisherAggregate>>();
        var listenersByCarrier = new Dictionary<Carrier, List<CarrierListener>>();
        var fieldAccumulators = new Dictionary<Carrier, FieldAccumulator>();
        var publicationFieldAccumulators = new Dictionary<Carrier, PublicationFieldAccumulator>();
        var feedbackFieldAccumulators = new Dictionary<Carrier, FeedbackFieldAccumulator>();
        var publicationReceipts = new ImmutableArray<OrthogonalPublication>.Builder[Sites.Length];
        var feedbackReceipts = new ImmutableArray<OrthogonalFeedback>.Builder[Sites.Length];
        var transfers = ImmutableArray.CreateBuilder<CarrierTransfer>();
        var publicationTransfers = ImmutableArray.CreateBuilder<CarrierPublicationTransfer>();
        var feedbackTransfers = ImmutableArray.CreateBuilder<CarrierFeedbackTransfer>();

        for (var index = 0; index < Sites.Length; index++)
        {
            var site = Sites[index];
            publicationReceipts[index] = ImmutableArray.CreateBuilder<OrthogonalPublication>();
            feedbackReceipts[index] = ImmutableArray.CreateBuilder<OrthogonalFeedback>();
            var ports = CarrierPorts(site.Orthogonal.Flows, site.Orthogonal.Publications);
            var localSite = new Site(
                site.Placement,
                site.Kernel,
                new OrthogonalReservoir(
                    site.Orthogonal.Residual,
                    ports.LocalFlows,
                    site.Orthogonal.OwnedPublications(),
                    ImmutableArray<OrthogonalFeedback>.Empty));
            var localSettlement = SiteDynamics.Settle(localSite);
            var nextSite = new Site(
                localSettlement.Next.Placement,
                localSettlement.Next.Kernel,
                new OrthogonalReservoir(
                    localSettlement.Next.Orthogonal.Residual,
                    site.Orthogonal.Flows,
                    site.Orthogonal.OwnedPublications(),
                    ImmutableArray<OrthogonalFeedback>.Empty));
            var settlement = localSettlement with { Source = site, Next = nextSite };

            settlements.Add(settlement);
            nextSites.Add(nextSite);

            foreach (var listener in ports.Listeners)
            {
                AccumulateField(fieldAccumulators, listener.Carrier).ListenerWeight += listener.Weight;
                AccumulatePublicationField(publicationFieldAccumulators, listener.Carrier).ListenerCount += Measure.One;
                AccumulateFeedbackField(feedbackFieldAccumulators, listener.Carrier);

                if (!listenersByCarrier.TryGetValue(listener.Carrier, out var listeners))
                {
                    listeners = [];
                    listenersByCarrier[listener.Carrier] = listeners;
                }

                listeners.Add(new CarrierListener(index, listener.Weight));
            }

            foreach (var emitter in ports.ValueEmitters)
            {
                var field = AccumulateField(fieldAccumulators, emitter.Carrier);
                field.EmitterWeight += emitter.Weight;
                field.Published.Add(emitter.Value);
                AccumulateFeedbackField(feedbackFieldAccumulators, emitter.Carrier);
                AccumulatePublisher(publishersByCarrier, emitter.Carrier, index, site).Add(emitter.Value);

                if (!emissionsByCarrier.TryGetValue(emitter.Carrier, out var emissions))
                {
                    emissions = [];
                    emissionsByCarrier[emitter.Carrier] = emissions;
                }

                emissions.Add(new CarrierValueEmission(index, emitter.Carrier, emitter.Value.Normalize()));

                var publicationField = AccumulatePublicationField(publicationFieldAccumulators, emitter.Carrier);
                publicationField.PublishedCount += Measure.One;

                if (!publicationByCarrier.TryGetValue(emitter.Carrier, out var publications))
                {
                    publications = [];
                    publicationByCarrier[emitter.Carrier] = publications;
                }

                publications.Add(new CarrierPublicationEmission(
                    index,
                    OrthogonalPublication.Publish(emitter.Carrier, site, emitter.Value)));
            }

            foreach (var publication in ports.PublicationEmitters)
            {
                var field = AccumulateField(fieldAccumulators, publication.Carrier);
                field.EmitterWeight += publication.Potential;
                field.Published.Add(publication.Value);
                AccumulateFeedbackField(feedbackFieldAccumulators, publication.Carrier);
                AccumulatePublisher(publishersByCarrier, publication.Carrier, index, site).Add(publication.Value);

                var publicationField = AccumulatePublicationField(publicationFieldAccumulators, publication.Carrier);
                publicationField.PublishedCount += Measure.One;

                if (!publicationByCarrier.TryGetValue(publication.Carrier, out var publications))
                {
                    publications = [];
                    publicationByCarrier[publication.Carrier] = publications;
                }

                publications.Add(new CarrierPublicationEmission(index, publication));
            }

            foreach (var feedback in ports.FeedbackEmitters)
            {
                var field = AccumulateFeedbackField(feedbackFieldAccumulators, feedback.Carrier);
                field.RequesterCount += Measure.One;
                field.Requested.Add(feedback.Desired);

                if (!feedbackByCarrier.TryGetValue(feedback.Carrier, out var feedbacks))
                {
                    feedbacks = [];
                    feedbackByCarrier[feedback.Carrier] = feedbacks;
                }

                feedbacks.Add(new CarrierFeedbackEmission(index, feedback.Carrier, feedback.Desired.Normalize()));
            }
        }

        foreach (var (carrier, publishers) in publishersByCarrier)
        {
            AccumulateFeedbackField(feedbackFieldAccumulators, carrier).PublisherCount = new Measure(publishers.Count);
        }

        foreach (var (carrier, emissions) in emissionsByCarrier)
        {
            if (!listenersByCarrier.TryGetValue(carrier, out var listeners))
            {
                var field = AccumulateField(fieldAccumulators, carrier);

                foreach (var emission in emissions)
                {
                    field.Retained.Add(emission.Value);
                }

                continue;
            }

            foreach (var emission in emissions)
            {
                var delivered = false;

                foreach (var listener in listeners)
                {
                    if (listener.SiteIndex == emission.SourceIndex)
                    {
                        continue;
                    }

                    delivered = true;
                    var field = AccumulateField(fieldAccumulators, carrier);
                    field.TransferCount += Measure.One;
                    transfers.Add(new CarrierTransfer(
                        carrier,
                        Sites[emission.SourceIndex].Placement,
                        Sites[listener.SiteIndex].Placement,
                        emission.Value));
                }

                var aggregate = AccumulateField(fieldAccumulators, carrier);

                if (delivered)
                {
                    aggregate.Delivered.Add(emission.Value);
                }
                else
                {
                    aggregate.Retained.Add(emission.Value);
                }
            }
        }

        foreach (var (carrier, publications) in publicationByCarrier)
        {
            if (!listenersByCarrier.TryGetValue(carrier, out var listeners))
            {
                var field = AccumulatePublicationField(publicationFieldAccumulators, carrier);
                field.TransferCount += Measure.Zero;
                continue;
            }

            foreach (var emission in publications)
            {
                var delivered = false;

                foreach (var listener in listeners)
                {
                    if (listener.SiteIndex == emission.SiteIndex)
                    {
                        continue;
                    }

                    delivered = true;
                    var deliveredPublication = OrthogonalPublication.Receive(
                        carrier,
                        emission.Publication.Site,
                        emission.Publication.Value);
                    publicationReceipts[listener.SiteIndex].Add(deliveredPublication);
                    var field = AccumulatePublicationField(publicationFieldAccumulators, carrier);
                    field.DeliveredCount += Measure.One;
                    field.TransferCount += Measure.One;
                    publicationTransfers.Add(new CarrierPublicationTransfer(
                        carrier,
                        Sites[emission.SiteIndex].Placement,
                        Sites[listener.SiteIndex].Placement,
                        deliveredPublication));
                }

                if (!delivered)
                {
                    AccumulatePublicationField(publicationFieldAccumulators, carrier).TransferCount += Measure.Zero;
                }
            }
        }

        foreach (var (carrier, feedbacks) in feedbackByCarrier)
        {
            if (!publishersByCarrier.TryGetValue(carrier, out var publishers))
            {
                var field = AccumulateFeedbackField(feedbackFieldAccumulators, carrier);

                foreach (var feedback in feedbacks)
                {
                    field.Retained.Add(feedback.Desired);
                }

                continue;
            }

            foreach (var feedback in feedbacks)
            {
                var delivered = false;

                foreach (var publisher in publishers)
                {
                    if (publisher.Key == feedback.SourceIndex)
                    {
                        continue;
                    }

                    delivered = true;
                    var deliveredFeedback = OrthogonalFeedback.Request(
                        carrier,
                        Sites[feedback.SourceIndex],
                        publisher.Value.Site,
                        publisher.Value.Published.ValueOrZero(),
                        feedback.Desired);
                    feedbackReceipts[publisher.Key].Add(deliveredFeedback);
                    var field = AccumulateFeedbackField(feedbackFieldAccumulators, carrier);
                    field.TransferCount += Measure.One;
                    feedbackTransfers.Add(new CarrierFeedbackTransfer(
                        carrier,
                        Sites[feedback.SourceIndex].Placement,
                        Sites[publisher.Key].Placement,
                        deliveredFeedback));
                }

                var aggregate = AccumulateFeedbackField(feedbackFieldAccumulators, carrier);

                if (delivered)
                {
                    aggregate.Delivered.Add(feedback.Desired);
                }
                else
                {
                    aggregate.Retained.Add(feedback.Desired);
                }
            }
        }

        var finalized = ImmutableArray.CreateBuilder<Site>(Sites.Length);
        var fields = ImmutableArray.CreateBuilder<CarrierFieldState>(fieldAccumulators.Count);
        var publicationFields = ImmutableArray.CreateBuilder<CarrierPublicationFieldState>(publicationFieldAccumulators.Count);
        var feedbackFields = ImmutableArray.CreateBuilder<CarrierFeedbackFieldState>(feedbackFieldAccumulators.Count);
        var feedbackSummaries = ImmutableArray.CreateBuilder<CarrierFeedbackSummary>();
        var maxTransfer = Ratio.Zero(Measure.One);

        foreach (var transfer in transfers)
        {
            maxTransfer = CompareMagnitude(transfer.Value, maxTransfer) > 0
                ? transfer.Value.Normalize()
                : maxTransfer;
        }

        foreach (var transfer in feedbackTransfers)
        {
            maxTransfer = CompareMagnitude(transfer.Feedback.Desired, maxTransfer) > 0
                ? transfer.Feedback.Desired.Normalize()
                : maxTransfer;
        }

        foreach (var field in fieldAccumulators.OrderBy(pair => pair.Key.Id))
        {
            fields.Add(new CarrierFieldState(
                field.Key,
                field.Value.Published.ValueOrZero(),
                field.Value.Delivered.ValueOrZero(),
                field.Value.Retained.ValueOrZero(),
                field.Value.EmitterWeight,
                field.Value.ListenerWeight,
                field.Value.TransferCount));
        }

        foreach (var field in publicationFieldAccumulators.OrderBy(pair => pair.Key.Id))
        {
            publicationFields.Add(new CarrierPublicationFieldState(
                field.Key,
                field.Value.PublishedCount,
                field.Value.DeliveredCount,
                field.Value.ListenerCount,
                field.Value.TransferCount));
        }

        foreach (var field in feedbackFieldAccumulators.OrderBy(pair => pair.Key.Id))
        {
            feedbackFields.Add(new CarrierFeedbackFieldState(
                field.Key,
                field.Value.Requested.ValueOrZero(),
                field.Value.Delivered.ValueOrZero(),
                field.Value.Retained.ValueOrZero(),
                field.Value.PublisherCount,
                field.Value.RequesterCount,
                field.Value.TransferCount));
        }

        for (var index = 0; index < nextSites.Count; index++)
        {
            var nextSite = nextSites[index];
            var receivedFeedback = feedbackReceipts[index].ToImmutable();
            var adaptiveSummaries = ImmutableArray.CreateBuilder<CarrierFeedbackSummary>();

            foreach (var byCarrier in receivedFeedback
                         .GroupBy(feedback => feedback.Carrier)
                         .OrderBy(group => group.Key.Id))
            {
                var feedbacks = byCarrier.ToImmutableArray();
                var published = publishersByCarrier.TryGetValue(byCarrier.Key, out var publishers)
                                && publishers.TryGetValue(index, out var aggregate)
                    ? aggregate.Published.ValueOrZero()
                    : Ratio.Zero(Measure.Zero);
                var preferred = ComposeDesired(feedbacks);
                var desiredResolution = AverageResolution(feedbacks);
                var summary = new CarrierFeedbackSummary(
                    byCarrier.Key,
                    nextSite.Placement,
                    published,
                    preferred,
                    desiredResolution,
                    preferred.IsResolved && published.IsResolved
                        ? preferred.Subtract(published).Normalize()
                        : Ratio.Tension(preferred.Potential + published.Potential),
                    desiredResolution.Subtract(new Ratio(published.Dominant, Measure.One)).Normalize(),
                    ValueSpread(feedbacks),
                    ResolutionSpread(feedbacks),
                    new Measure(feedbacks.Length));
                feedbackSummaries.Add(summary);
                adaptiveSummaries.Add(summary);
            }

            nextSite = ApplyAdaptivePublicationUpdates(nextSite, adaptiveSummaries.ToImmutable());
            finalized.Add(new Site(
                nextSite.Placement,
                nextSite.Kernel,
                nextSite.Orthogonal
                    .Resolve(nextSite.Orthogonal.Residual)
                    .WithPublications(
                        nextSite.Orthogonal.OwnedPublications()
                            .AddRange(publicationReceipts[index].ToImmutable()))
                    .WithFeedback(receivedFeedback)));
        }

        return new CarrierCommunicationState(
            Communication,
            finalized.ToImmutable(),
            settlements.ToImmutable(),
            fields.ToImmutable(),
            transfers.ToImmutable(),
            publicationFields.ToImmutable(),
            publicationTransfers.ToImmutable(),
            feedbackFields.ToImmutable(),
            feedbackTransfers.ToImmutable(),
            feedbackSummaries.ToImmutable(),
            Iteration + Measure.One,
            maxTransfer);
    }

    public CarrierCommunicationState Relax(Measure steps)
    {
        var state = this;

        for (var index = 0L; index < steps.Raw; index++)
        {
            state = state.Relax();
        }

        return state;
    }

    public CarrierCommunicationState Settle(Measure maxSteps)
    {
        var state = this;

        for (var index = 0L; index < maxSteps.Raw; index++)
        {
            state = state.Relax();

            if (state.MaxTransfer.Potential == Measure.Zero)
            {
                break;
            }
        }

        return state;
    }

    private static CarrierPortSet CarrierPorts(
        ImmutableArray<Flow> flows,
        ImmutableArray<OrthogonalPublication> publications)
    {
        var localFlows = ImmutableArray.CreateBuilder<Flow>();
        var valueEmitters = ImmutableArray.CreateBuilder<CarrierValueEmitter>();
        var listeners = new Dictionary<Carrier, Measure>();
        var publicationEmitters = ImmutableArray.CreateBuilder<OrthogonalPublication>();
        var feedbackEmitters = ImmutableArray.CreateBuilder<CarrierFeedbackEmitter>();

        foreach (var flow in flows)
        {
            if (flow.Support is not Carrier carrier)
            {
                localFlows.Add(flow);
                continue;
            }

            if ((flow.IsOutgoing || flow.IsOscillating) && flow.Potential != Measure.Zero && flow.Value.IsResolved)
            {
                valueEmitters.Add(new CarrierValueEmitter(carrier, flow.Value.Normalize(), flow.Potential));
            }

            if ((flow.IsIncoming || flow.IsOscillating) && flow.Potential != Measure.Zero)
            {
                listeners[carrier] = listeners.TryGetValue(carrier, out var current)
                    ? current + flow.Potential
                    : flow.Potential;
            }

            if ((flow.IsIncoming || flow.IsOscillating) && flow.Potential != Measure.Zero && flow.Value.IsResolved)
            {
                feedbackEmitters.Add(new CarrierFeedbackEmitter(carrier, flow.Value.Normalize(), flow.Potential));
            }
        }

        foreach (var publication in publications)
        {
            if (publication.Support is not Carrier)
            {
                continue;
            }

            if ((publication.IsOutgoing || publication.IsOscillating) && publication.Value.IsResolved)
            {
                publicationEmitters.Add(publication);
            }
        }

        return new CarrierPortSet(
            localFlows.ToImmutable(),
            valueEmitters.ToImmutable(),
            [.. listeners.Select(pair => new CarrierWeight(pair.Key, pair.Value))],
            publicationEmitters.ToImmutable(),
            feedbackEmitters.ToImmutable());
    }

    private static bool SameNode(Site left, Site right) => left.Carrier == right.Carrier
        && left.On(left.Carrier).Normalize() == right.On(right.Carrier).Normalize();

    private static int CompareMagnitude(Ratio left, Ratio right) => left.CompareMagnitude(right);

    private static FieldAccumulator AccumulateField(Dictionary<Carrier, FieldAccumulator> fields, Carrier carrier)
    {
        if (!fields.TryGetValue(carrier, out var field))
        {
            field = new FieldAccumulator();
            fields[carrier] = field;
        }

        return field;
    }

    private static FeedbackFieldAccumulator AccumulateFeedbackField(
        Dictionary<Carrier, FeedbackFieldAccumulator> fields,
        Carrier carrier)
    {
        if (!fields.TryGetValue(carrier, out var field))
        {
            field = new FeedbackFieldAccumulator();
            fields[carrier] = field;
        }

        return field;
    }

    private static PublicationFieldAccumulator AccumulatePublicationField(
        Dictionary<Carrier, PublicationFieldAccumulator> fields,
        Carrier carrier)
    {
        if (!fields.TryGetValue(carrier, out var field))
        {
            field = new PublicationFieldAccumulator();
            fields[carrier] = field;
        }

        return field;
    }

    private static PublisherAggregate AccumulatePublisher(
        Dictionary<Carrier, Dictionary<int, PublisherAggregate>> publishers,
        Carrier carrier,
        int siteIndex,
        Site site)
    {
        if (!publishers.TryGetValue(carrier, out var bySite))
        {
            bySite = [];
            publishers[carrier] = bySite;
        }

        if (!bySite.TryGetValue(siteIndex, out var aggregate))
        {
            aggregate = new PublisherAggregate(site);
            bySite[siteIndex] = aggregate;
        }

        return aggregate;
    }

    private readonly record struct CarrierPortSet(
        ImmutableArray<Flow> LocalFlows,
        ImmutableArray<CarrierValueEmitter> ValueEmitters,
        ImmutableArray<CarrierWeight> Listeners,
        ImmutableArray<OrthogonalPublication> PublicationEmitters,
        ImmutableArray<CarrierFeedbackEmitter> FeedbackEmitters);

    private readonly record struct CarrierWeight(Carrier Carrier, Measure Weight);

    private readonly record struct CarrierValueEmitter(Carrier Carrier, Ratio Value, Measure Weight);

    private readonly record struct CarrierFeedbackEmitter(Carrier Carrier, Ratio Desired, Measure Weight);

    private readonly record struct CarrierPublicationEmission(int SiteIndex, OrthogonalPublication Publication);

    private readonly record struct CarrierValueEmission(int SourceIndex, Carrier Carrier, Ratio Value);

    private readonly record struct CarrierFeedbackEmission(int SourceIndex, Carrier Carrier, Ratio Desired);

    private readonly record struct CarrierListener(int SiteIndex, Measure Weight);

    private sealed class PublisherAggregate(Site site)
    {
        public Site Site { get; } = site;

        public RatioAccumulator Published;

        public void Add(Ratio value) => Published.Add(value);
    }

    private sealed class FieldAccumulator
    {
        public RatioAccumulator Published;

        public RatioAccumulator Delivered;

        public RatioAccumulator Retained;

        public Measure EmitterWeight;

        public Measure ListenerWeight;

        public Measure TransferCount;
    }

    private sealed class PublicationFieldAccumulator
    {
        public Measure PublishedCount;

        public Measure DeliveredCount;

        public Measure ListenerCount;

        public Measure TransferCount;
    }

    private sealed class FeedbackFieldAccumulator
    {
        public RatioAccumulator Requested;

        public RatioAccumulator Delivered;

        public RatioAccumulator Retained;

        public Measure PublisherCount;

        public Measure RequesterCount;

        public Measure TransferCount;
    }

    private struct RatioAccumulator
    {
        private bool _hasValue;
        private Ratio _value;

        public void Add(Ratio value)
        {
            if (value.Potential == Measure.Zero)
            {
                return;
            }

            _value = _hasValue ? _value.Add(value).Normalize() : value.Normalize();
            _hasValue = true;
        }

        public Ratio ValueOrZero() => _hasValue ? _value.Normalize() : Ratio.Zero(Measure.Zero);
    }

    private static Ratio ComposeDesired(ImmutableArray<OrthogonalFeedback> feedbacks)
    {
        if (feedbacks.IsDefaultOrEmpty)
        {
            return Ratio.Zero(Measure.Zero);
        }

        long numerator = 0L;
        long denominator = 0L;

        foreach (var feedback in feedbacks)
        {
            numerator += feedback.Desired.Recessive.Raw;
            denominator += feedback.Desired.Dominant.Raw;
        }

        return denominator == 0L
            ? Ratio.Tension(new Measure(Math.Abs(numerator)))
            : new Ratio(new Measure(numerator), new Measure(denominator)).Normalize();
    }

    private static Ratio AverageResolution(ImmutableArray<OrthogonalFeedback> feedbacks)
    {
        if (feedbacks.IsDefaultOrEmpty)
        {
            return Ratio.Zero(Measure.One);
        }

        long sum = 0L;

        foreach (var feedback in feedbacks)
        {
            sum += feedback.Desired.Dominant.Raw;
        }

        return new Ratio(new Measure(sum), new Measure(feedbacks.Length)).Normalize();
    }

    private static Ratio ValueSpread(ImmutableArray<OrthogonalFeedback> feedbacks)
    {
        if (feedbacks.Length <= 1)
        {
            return Ratio.Zero(Measure.One);
        }

        var low = feedbacks[0].Desired;
        var high = feedbacks[0].Desired;

        foreach (var feedback in feedbacks)
        {
            if (feedback.Desired.Approximate < low.Approximate)
            {
                low = feedback.Desired;
            }

            if (feedback.Desired.Approximate > high.Approximate)
            {
                high = feedback.Desired;
            }
        }

        return high.Subtract(low).Normalize();
    }

    private static Ratio ResolutionSpread(ImmutableArray<OrthogonalFeedback> feedbacks)
    {
        if (feedbacks.Length <= 1)
        {
            return Ratio.Zero(Measure.One);
        }

        long low = feedbacks[0].Desired.Dominant.Raw;
        long high = feedbacks[0].Desired.Dominant.Raw;

        foreach (var feedback in feedbacks)
        {
            low = Math.Min(low, feedback.Desired.Dominant.Raw);
            high = Math.Max(high, feedback.Desired.Dominant.Raw);
        }

        return new Ratio(new Measure(high - low), Measure.One);
    }

    private static Site ApplyAdaptivePublicationUpdates(Site site, ImmutableArray<CarrierFeedbackSummary> summaries)
    {
        if (summaries.IsDefaultOrEmpty)
        {
            return site;
        }

        var flows = site.Orthogonal.Flows.ToBuilder();
        var publications = site.Orthogonal.OwnedPublications().ToBuilder();
        var changed = false;

        foreach (var summary in summaries)
        {
            if (!summary.Published.IsResolved || !summary.Preferred.IsResolved)
            {
                continue;
            }

            var response = AdaptationParticipation(summary);

            if (response.Potential == Measure.Zero)
            {
                continue;
            }

            var appliedDelta = summary.ValueDelta.ScaleBy(response).Normalize();
            var flowIndexes = FlowEmittersOn(flows, summary.Carrier);
            var publicationIndexes = PublicationEmittersOn(publications, summary.Carrier);
            var emitterCount = new Measure(flowIndexes.Length + publicationIndexes.Length);

            if (emitterCount == Measure.Zero
                || (appliedDelta.Potential == Measure.Zero && appliedDelta.Dominant == Measure.Zero))
            {
                continue;
            }

            foreach (var flowIndex in flowIndexes)
            {
                var flow = flows[flowIndex];
                var participation = EmitterParticipation(flow.Value, summary.Published, emitterCount);
                flows[flowIndex] = flow.Retune(flow.Value.Add(appliedDelta.Multiply(participation)).Normalize());
            }

            foreach (var publicationIndex in publicationIndexes)
            {
                var publication = publications[publicationIndex];
                var participation = EmitterParticipation(publication.Value, summary.Published, emitterCount);
                publications[publicationIndex] = publication.Revalue(
                    publication.Value.Add(appliedDelta.Multiply(participation)).Normalize());
            }

            changed = true;
        }

        if (!changed)
        {
            return site;
        }

        return new Site(
            site.Placement,
            site.Kernel,
            site.Orthogonal
                .WithFlows(flows.ToImmutable())
                .WithPublications(publications.ToImmutable()));
    }

    private static Ratio AdaptationParticipation(CarrierFeedbackSummary summary)
    {
        if (summary.FeedbackCount == Measure.Zero)
        {
            return Ratio.Zero(Measure.One);
        }

        // Agreement hardens participation. More listeners strengthen the update, while wider disagreement keeps the
        // publication closer to its current value until the returning field converges.
        var disagreement = summary.ValueSpread.Potential + summary.ResolutionSpread.Potential;
        return new Ratio(summary.FeedbackCount, summary.FeedbackCount + disagreement + Measure.One).Normalize();
    }

    private static ImmutableArray<int> FlowEmittersOn(ImmutableArray<Flow>.Builder flows, Carrier carrier)
    {
        var indexes = ImmutableArray.CreateBuilder<int>();

        for (var index = 0; index < flows.Count; index++)
        {
            var flow = flows[index];

            if (flow.Support is Carrier flowCarrier
                && flowCarrier == carrier
                && flow.Value.IsResolved
                && (flow.IsOutgoing || flow.IsOscillating))
            {
                indexes.Add(index);
            }
        }

        return indexes.ToImmutable();
    }

    private static ImmutableArray<int> PublicationEmittersOn(
        ImmutableArray<OrthogonalPublication>.Builder publications,
        Carrier carrier)
    {
        var indexes = ImmutableArray.CreateBuilder<int>();

        for (var index = 0; index < publications.Count; index++)
        {
            var publication = publications[index];

            if (publication.Support is Carrier publicationCarrier
                && publicationCarrier == carrier
                && publication.Value.IsResolved
                && (publication.IsOutgoing || publication.IsOscillating))
            {
                indexes.Add(index);
            }
        }

        return indexes.ToImmutable();
    }

    private static Ratio EmitterParticipation(Ratio emitterValue, Ratio aggregatePublished, Measure emitterCount) =>
        aggregatePublished.IsResolved && aggregatePublished.Potential != Measure.Zero
            ? emitterValue.Divide(aggregatePublished).Normalize()
            : new Ratio(Measure.One, emitterCount).Normalize();
}
