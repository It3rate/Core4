using System;
using System.Collections.Immutable;
using Core4.Dynamics;
using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;

namespace Core4.AvaloniaProbe.Models;

/// <summary>
/// A tiny two-site transport scene: one source self-returns part of its orthogonal residual locally, then publishes
/// the remainder onto one shared carrier that a listening site can absorb on the next round.
/// </summary>
public readonly record struct RoutingStiffnessDemoState(
    Measure SourceInput,
    Measure SourceStiffnessPercent,
    Measure SourceRoutingPercent,
    Measure SourceDominantPercent,
    Measure SinkStiffnessPercent,
    Measure SinkRoutingPercent,
    Measure SinkDominantPercent,
    Measure Round)
{
    public const long MaxPercent = 100L;
    public const long MaxRound = 4L;
    public const long MaxInput = 24L;

    private static readonly Carrier SourceCarrier = new(1);
    private static readonly Carrier SharedCarrier = new(2);
    private static readonly Carrier SinkCarrier = new(3);
    private static readonly Measure PortWeight = Measure.One;

    public static RoutingStiffnessDemoState Default => new(
        new Measure(18),
        new Measure(25),
        new Measure(75),
        new Measure(85),
        new Measure(45),
        new Measure(70),
        new Measure(100),
        new Measure(2));

    public Ratio SourceStiffness => Percent(SourceStiffnessPercent);

    public Ratio SourceRoutingGain => Percent(SourceRoutingPercent);

    public Ratio SourceDominantBias => Percent(SourceDominantPercent);

    public Ratio SinkStiffness => Percent(SinkStiffnessPercent);

    public Ratio SinkRoutingGain => Percent(SinkRoutingPercent);

    public Ratio SinkDominantBias => Percent(SinkDominantPercent);

    public Measure CurrentRound => ClampRound(Round);

    public Site SeedSourceSite => Site.Open(SourceCarrier, Measure.Zero)
        .Anchor(new Ratio(Measure.Zero, Measure.One))
        .Resolve(new Ratio(Measure.Zero, Measure.One))
        .WithResidual(Ratio.Tension(SourceInput.Absolute))
        .Harden(SourceStiffness)
        .Route(SourceRoutingGain)
        .BiasToDominant(SourceDominantBias)
        .Connect(new Flow(SharedCarrier, Ratio.Tension(PortWeight), Provenance.SelfDriven(PortWeight)));

    public Site SeedSinkSite => Site.Open(SinkCarrier, Measure.Zero)
        .Anchor(new Ratio(Measure.Zero, Measure.One))
        .Resolve(new Ratio(Measure.Zero, Measure.One))
        .Harden(SinkStiffness)
        .Route(SinkRoutingGain)
        .BiasToDominant(SinkDominantBias)
        .Connect(new Flow(SharedCarrier, Ratio.Tension(PortWeight), Provenance.OtherDriven(PortWeight)));

    public ConnectedSiteTopology Topology => ConnectedSiteTopology.Create([SeedSourceSite, SeedSinkSite]);

    public CarrierCommunication Communication => CarrierCommunication.Create(Topology);

    public ImmutableArray<CarrierCommunicationState> Timeline
    {
        get
        {
            var states = ImmutableArray.CreateBuilder<CarrierCommunicationState>((int)MaxRound + 1);
            var current = Communication.Seed();
            states.Add(current);

            for (var round = 0; round < MaxRound; round++)
            {
                current = current.Relax();
                states.Add(current);
            }

            return states.ToImmutable();
        }
    }

    public CarrierCommunicationState CurrentState => Timeline[(int)CurrentRound.Raw];

    public Site CurrentSourceSite => SiteOn(CurrentState, SourceCarrier, SeedSourceSite);

    public Site CurrentSinkSite => SiteOn(CurrentState, SinkCarrier, SeedSinkSite);

    public SiteSettlement? CurrentSourceSettlement => SettlementOn(CurrentState, SourceCarrier);

    public SiteSettlement? CurrentSinkSettlement => SettlementOn(CurrentState, SinkCarrier);

    public CarrierFieldState? CurrentField => FieldOn(CurrentState, SharedCarrier);

    public Ratio SourceAbsorbed => CurrentSourceSettlement is { } settlement
        ? LocalAbsorbed(settlement)
        : Ratio.Zero(Measure.Zero);

    public Ratio SinkAbsorbed => CurrentSinkSettlement is { } settlement
        ? LocalAbsorbed(settlement)
        : Ratio.Zero(Measure.Zero);

    public string RoundDescription => CurrentRound.Raw switch
    {
        0L => "Seed only. The source holds raw local residual and the listener only exposes a shared-carrier port.",
        1L => "The source self-returns part of its residual locally, then publishes the remainder onto the shared carrier. The listener receives pending residual but has not yet absorbed it.",
        2L => "The listener begins absorbing what the previous round delivered, while the source can continue settling any retained remainder and republishing its new excess.",
        _ => "Repeated rounds continue the same law: local self-return first, carrier publication second, listener absorption on the following round."
    };

    public RoutingStiffnessDemoState WithSourceInput(Measure sourceInput) =>
        this with { SourceInput = Clamp(sourceInput.Absolute, 0L, MaxInput) };

    public RoutingStiffnessDemoState WithSourceStiffnessPercent(Measure percent) =>
        this with { SourceStiffnessPercent = Clamp(percent.Absolute, 0L, MaxPercent) };

    public RoutingStiffnessDemoState WithSourceRoutingPercent(Measure percent) =>
        this with { SourceRoutingPercent = Clamp(percent.Absolute, 0L, MaxPercent) };

    public RoutingStiffnessDemoState WithSourceDominantPercent(Measure percent) =>
        this with { SourceDominantPercent = Clamp(percent.Absolute, 0L, MaxPercent) };

    public RoutingStiffnessDemoState WithSinkStiffnessPercent(Measure percent) =>
        this with { SinkStiffnessPercent = Clamp(percent.Absolute, 0L, MaxPercent) };

    public RoutingStiffnessDemoState WithSinkRoutingPercent(Measure percent) =>
        this with { SinkRoutingPercent = Clamp(percent.Absolute, 0L, MaxPercent) };

    public RoutingStiffnessDemoState WithSinkDominantPercent(Measure percent) =>
        this with { SinkDominantPercent = Clamp(percent.Absolute, 0L, MaxPercent) };

    public RoutingStiffnessDemoState WithRound(Measure round) =>
        this with { Round = ClampRound(round.Absolute) };

    private static Ratio Percent(Measure percent) => new Ratio(
        Clamp(percent.Absolute, 0L, MaxPercent),
        new Measure(MaxPercent));

    private static Measure ClampRound(Measure round) => Clamp(round, 0L, MaxRound);

    private static Measure Clamp(Measure measure, long minimum, long maximum) =>
        new(Math.Clamp(measure.Raw, minimum, maximum));

    private static Site SiteOn(CarrierCommunicationState state, Carrier carrier, Site fallback)
    {
        foreach (var site in state.Sites)
        {
            if (site.Carrier == carrier)
            {
                return site;
            }
        }

        return fallback;
    }

    private static SiteSettlement? SettlementOn(CarrierCommunicationState state, Carrier carrier)
    {
        foreach (var settlement in state.Settlements)
        {
            if (settlement.Source.Carrier == carrier)
            {
                return settlement;
            }
        }

        return null;
    }

    private static CarrierFieldState? FieldOn(CarrierCommunicationState state, Carrier carrier)
    {
        foreach (var field in state.Fields)
        {
            if (field.Carrier == carrier)
            {
                return field;
            }
        }

        return null;
    }

    private static Ratio LocalAbsorbed(SiteSettlement settlement)
    {
        var sourceTotal = settlement.Source.Kernel.Recessive
            .Add(settlement.Source.Kernel.Dominant)
            .Normalize();
        var nextTotal = settlement.Next.Kernel.Recessive
            .Add(settlement.Next.Kernel.Dominant)
            .Normalize();
        return nextTotal.Subtract(sourceTotal).Normalize();
    }
}
