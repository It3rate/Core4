using System.Collections.Immutable;
using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;

namespace Core4.Dynamics;

/// <summary>
/// A locality-preserving carrier field. Sites remain purely local claim machines; this layer transports resolved
/// carrier-backed publication between listening sites while leaving unresolved orthogonal residual local unless some
/// later law explicitly republishes it. Read-only orthogonal site publications ride the same listener ports.
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
/// One repeated carrier communication field over locally settling sites. Carrier-backed flows act as ports here:
/// they do not feed the site's same-pass local settlement, but resolved outgoing values can publish across them
/// between rounds. Carrier-backed publication payloads are also delivered here as read-only site references.
/// </summary>
public readonly record struct CarrierCommunicationState(
    CarrierCommunication Communication,
    ImmutableArray<Site> Sites,
    ImmutableArray<SiteSettlement> Settlements,
    ImmutableArray<CarrierFieldState> Fields,
    ImmutableArray<CarrierTransfer> Transfers,
    ImmutableArray<CarrierPublicationFieldState> PublicationFields,
    ImmutableArray<CarrierPublicationTransfer> PublicationTransfers,
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
        var listenersByCarrier = new Dictionary<Carrier, List<CarrierListener>>();
        var fieldAccumulators = new Dictionary<Carrier, FieldAccumulator>();
        var publicationFieldAccumulators = new Dictionary<Carrier, PublicationFieldAccumulator>();
        var publicationReceipts = new ImmutableArray<OrthogonalPublication>.Builder[Sites.Length];
        var transfers = ImmutableArray.CreateBuilder<CarrierTransfer>();
        var publicationTransfers = ImmutableArray.CreateBuilder<CarrierPublicationTransfer>();

        for (var index = 0; index < Sites.Length; index++)
        {
            var site = Sites[index];
            publicationReceipts[index] = ImmutableArray.CreateBuilder<OrthogonalPublication>();
            var ports = CarrierPorts(site.Orthogonal.Flows, site.Orthogonal.Publications);
            var localSite = new Site(
                site.Placement,
                site.Kernel,
                new OrthogonalReservoir(site.Orthogonal.Residual, ports.LocalFlows, site.Orthogonal.OwnedPublications()));
            var localSettlement = SiteDynamics.Settle(localSite);
            var nextSite = new Site(
                localSettlement.Next.Placement,
                localSettlement.Next.Kernel,
                new OrthogonalReservoir(localSettlement.Next.Orthogonal.Residual, site.Orthogonal.Flows, site.Orthogonal.OwnedPublications()));
            var settlement = localSettlement with { Source = site, Next = nextSite };

            settlements.Add(settlement);
            nextSites.Add(nextSite);

            foreach (var listener in ports.Listeners)
            {
                AccumulateField(fieldAccumulators, listener.Carrier).ListenerWeight += listener.Weight;
                AccumulatePublicationField(publicationFieldAccumulators, listener.Carrier).ListenerCount += Measure.One;

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

                var publicationField = AccumulatePublicationField(publicationFieldAccumulators, publication.Carrier);
                publicationField.PublishedCount += Measure.One;

                if (!publicationByCarrier.TryGetValue(publication.Carrier, out var publications))
                {
                    publications = [];
                    publicationByCarrier[publication.Carrier] = publications;
                }

                publications.Add(new CarrierPublicationEmission(index, publication));
            }
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

        var finalized = ImmutableArray.CreateBuilder<Site>(Sites.Length);
        var fields = ImmutableArray.CreateBuilder<CarrierFieldState>(fieldAccumulators.Count);
        var publicationFields = ImmutableArray.CreateBuilder<CarrierPublicationFieldState>(publicationFieldAccumulators.Count);
        var maxTransfer = Ratio.Zero(Measure.One);

        foreach (var transfer in transfers)
        {
            maxTransfer = CompareMagnitude(transfer.Value, maxTransfer) > 0
                ? transfer.Value.Normalize()
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

        for (var index = 0; index < nextSites.Count; index++)
        {
            var nextSite = nextSites[index];
            finalized.Add(new Site(
                nextSite.Placement,
                nextSite.Kernel,
                nextSite.Orthogonal
                    .Resolve(nextSite.Orthogonal.Residual)
                    .WithPublications(
                        nextSite.Orthogonal.OwnedPublications()
                            .AddRange(publicationReceipts[index].ToImmutable()))));
        }

        return new CarrierCommunicationState(
            Communication,
            finalized.ToImmutable(),
            settlements.ToImmutable(),
            fields.ToImmutable(),
            transfers.ToImmutable(),
            publicationFields.ToImmutable(),
            publicationTransfers.ToImmutable(),
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
            publicationEmitters.ToImmutable());
    }

    private static bool SameNode(Site left, Site right) => left.Carrier == right.Carrier && left.Location == right.Location;

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

    private readonly record struct CarrierPortSet(
        ImmutableArray<Flow> LocalFlows,
        ImmutableArray<CarrierValueEmitter> ValueEmitters,
        ImmutableArray<CarrierWeight> Listeners,
        ImmutableArray<OrthogonalPublication> PublicationEmitters);

    private readonly record struct CarrierWeight(Carrier Carrier, Measure Weight);

    private readonly record struct CarrierValueEmitter(Carrier Carrier, Ratio Value, Measure Weight);

    private readonly record struct CarrierPublicationEmission(int SiteIndex, OrthogonalPublication Publication);

    private readonly record struct CarrierValueEmission(int SourceIndex, Carrier Carrier, Ratio Value);

    private readonly record struct CarrierListener(int SiteIndex, Measure Weight);

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
}
