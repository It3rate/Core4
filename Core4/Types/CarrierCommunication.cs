using System.Collections.Immutable;
using Core4.Dynamics;
using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;

namespace Core4.Types;

/// <summary>
/// A locality-preserving carrier field. Sites remain purely local claim machines; this layer only lets carrier-backed
/// orthogonal residual leave one site and arrive as next-round orthogonal input at other listening sites.
/// </summary>
public readonly record struct CarrierCommunication(ImmutableArray<Site> Sites, ImmutableArray<Relation> Relations)
{
    public static CarrierCommunication Create(ConnectedSiteTopology topology) => new(topology.Sites, topology.Relations);

    public CarrierCommunicationState Seed() => new(
        this,
        Sites,
        ImmutableArray<SiteSettlement>.Empty,
        ImmutableArray<CarrierTransfer>.Empty,
        Measure.Zero,
        Ratio.Zero(Measure.One));
}

/// <summary>
/// One carrier-level transfer produced by a single relaxation step.
/// </summary>
public readonly record struct CarrierTransfer(Carrier Carrier, Placement Source, Placement Target, Ratio Value);

/// <summary>
/// One repeated carrier communication field over locally settling sites. Carrier-backed flows act as ports here:
/// they do not feed the site's same-pass local settlement, but they do publish and receive orthogonal residual
/// between rounds.
/// </summary>
public readonly record struct CarrierCommunicationState(
    CarrierCommunication Communication,
    ImmutableArray<Site> Sites,
    ImmutableArray<SiteSettlement> Settlements,
    ImmutableArray<CarrierTransfer> Transfers,
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
        var emissionsByCarrier = new Dictionary<Carrier, List<CarrierEmission>>();
        var listenersByCarrier = new Dictionary<Carrier, List<CarrierListener>>();
        var hasEmitters = new bool[Sites.Length];
        var receipts = new RatioAccumulator[Sites.Length];
        var retained = new RatioAccumulator[Sites.Length];
        var transfers = ImmutableArray.CreateBuilder<CarrierTransfer>();

        for (var index = 0; index < Sites.Length; index++)
        {
            var site = Sites[index];
            var ports = CarrierPorts(site.Orthogonal.Flows);
            var localSite = new Site(
                site.Placement,
                site.Kernel,
                new OrthogonalReservoir(site.Orthogonal.Residual, ports.LocalFlows));
            var localSettlement = localSite.Settle();
            var nextSite = new Site(
                localSettlement.Next.Placement,
                localSettlement.Next.Kernel,
                new OrthogonalReservoir(localSettlement.Next.Orthogonal.Residual, site.Orthogonal.Flows));
            var settlement = localSettlement with { Source = site, Next = nextSite };

            settlements.Add(settlement);
            nextSites.Add(nextSite);
            hasEmitters[index] = ports.Emitters.Length != 0;

            foreach (var listener in ports.Listeners)
            {
                if (!listenersByCarrier.TryGetValue(listener.Carrier, out var listeners))
                {
                    listeners = [];
                    listenersByCarrier[listener.Carrier] = listeners;
                }

                listeners.Add(new CarrierListener(index, listener.Weight));
            }

            if (settlement.Remainder.Potential == Measure.Zero || ports.Emitters.Length == 0)
            {
                retained[index].Add(settlement.Remainder);
                continue;
            }

            var totalWeight = Measure.Zero;

            foreach (var emitter in ports.Emitters)
            {
                totalWeight += emitter.Weight;
            }

            if (totalWeight == Measure.Zero)
            {
                retained[index].Add(settlement.Remainder);
                continue;
            }

            foreach (var emitter in ports.Emitters)
            {
                var share = settlement.Remainder.ScaleBy(new Ratio(emitter.Weight, totalWeight)).Normalize();

                if (!emissionsByCarrier.TryGetValue(emitter.Carrier, out var emissions))
                {
                    emissions = [];
                    emissionsByCarrier[emitter.Carrier] = emissions;
                }

                emissions.Add(new CarrierEmission(index, share));
            }
        }

        foreach (var (carrier, emissions) in emissionsByCarrier)
        {
            if (!listenersByCarrier.TryGetValue(carrier, out var listeners))
            {
                foreach (var emission in emissions)
                {
                    retained[emission.SiteIndex].Add(emission.Value);
                }

                continue;
            }

            foreach (var emission in emissions)
            {
                var totalListenerWeight = Measure.Zero;

                foreach (var listener in listeners)
                {
                    if (listener.SiteIndex != emission.SiteIndex)
                    {
                        totalListenerWeight += listener.Weight;
                    }
                }

                if (totalListenerWeight == Measure.Zero)
                {
                    retained[emission.SiteIndex].Add(emission.Value);
                    continue;
                }

                var delivered = new RatioAccumulator();

                foreach (var listener in listeners)
                {
                    if (listener.SiteIndex == emission.SiteIndex)
                    {
                        continue;
                    }

                    var share = emission.Value.ScaleBy(new Ratio(listener.Weight, totalListenerWeight)).Normalize();

                    if (share.Potential == Measure.Zero)
                    {
                        continue;
                    }

                    receipts[listener.SiteIndex].Add(share);
                    delivered.Add(share);
                    transfers.Add(new CarrierTransfer(
                        carrier,
                        Sites[emission.SiteIndex].Placement,
                        Sites[listener.SiteIndex].Placement,
                        share));
                }

                retained[emission.SiteIndex].Add(emission.Value.Subtract(delivered.ValueOrZero()).Normalize());
            }
        }

        var finalized = ImmutableArray.CreateBuilder<Site>(Sites.Length);
        var maxTransfer = Ratio.Zero(Measure.One);

        foreach (var transfer in transfers)
        {
            maxTransfer = CompareMagnitude(transfer.Value, maxTransfer) > 0
                ? transfer.Value.Normalize()
                : maxTransfer;
        }

        for (var index = 0; index < nextSites.Count; index++)
        {
            var nextSite = nextSites[index];
            var residual = new RatioAccumulator();

            if (!hasEmitters[index])
            {
                residual.Add(nextSite.Orthogonal.Residual);
            }
            else
            {
                residual.Add(retained[index].ValueOrZero());
            }

            residual.Add(receipts[index].ValueOrZero());
            finalized.Add(nextSite.WithResidual(residual.ValueOrZero()));
        }

        return new CarrierCommunicationState(
            Communication,
            finalized.ToImmutable(),
            settlements.ToImmutable(),
            transfers.ToImmutable(),
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

    private static CarrierPortSet CarrierPorts(ImmutableArray<Flow> flows)
    {
        var localFlows = ImmutableArray.CreateBuilder<Flow>();
        var emitters = new Dictionary<Carrier, Measure>();
        var listeners = new Dictionary<Carrier, Measure>();

        foreach (var flow in flows)
        {
            if (flow.Support is not Carrier carrier)
            {
                localFlows.Add(flow);
                continue;
            }

            if ((flow.IsOutgoing || flow.IsOscillating) && flow.Potential != Measure.Zero)
            {
                emitters[carrier] = emitters.TryGetValue(carrier, out var current)
                    ? current + flow.Potential
                    : flow.Potential;
            }

            if ((flow.IsIncoming || flow.IsOscillating) && flow.Potential != Measure.Zero)
            {
                listeners[carrier] = listeners.TryGetValue(carrier, out var current)
                    ? current + flow.Potential
                    : flow.Potential;
            }
        }

        return new CarrierPortSet(
            localFlows.ToImmutable(),
            [.. emitters.Select(pair => new CarrierWeight(pair.Key, pair.Value))],
            [.. listeners.Select(pair => new CarrierWeight(pair.Key, pair.Value))]);
    }

    private static bool SameNode(Site left, Site right) => left.Carrier == right.Carrier && left.Location == right.Location;

    private static int CompareMagnitude(Ratio left, Ratio right) => left.CompareMagnitude(right);

    private readonly record struct CarrierPortSet(
        ImmutableArray<Flow> LocalFlows,
        ImmutableArray<CarrierWeight> Emitters,
        ImmutableArray<CarrierWeight> Listeners);

    private readonly record struct CarrierWeight(Carrier Carrier, Measure Weight);

    private readonly record struct CarrierEmission(int SiteIndex, Ratio Value);

    private readonly record struct CarrierListener(int SiteIndex, Measure Weight);

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
}
