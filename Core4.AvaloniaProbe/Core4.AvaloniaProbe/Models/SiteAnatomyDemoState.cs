using System.Collections.Immutable;
using System.Linq;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Models;

/// <summary>
/// One representative site showing the current engine mechanics: local aligned claim/support, orthogonal residual,
/// carrier-backed publish/request ports, and local self-return controls.
/// </summary>
public readonly record struct SiteAnatomyDemoState(Site Site)
{
    private static readonly Carrier SiteCarrier = new(11);
    private static readonly Carrier PublishCarrierP = new(21);
    private static readonly Carrier PublishCarrierQ = new(22);
    private static readonly Carrier ListenCarrierP = new(21);
    private static readonly Carrier ListenCarrierQ = new(22);

    public static SiteAnatomyDemoState Default => new(
        Site.Open(SiteCarrier, Measure.Zero)
            .Anchor(new Ratio(Measure.One, Measure.One))
            .Resolve(new Ratio(new Measure(2), Measure.One))
            .Harden(new Ratio(new Measure(3), new Measure(4)))
            .Route(new Ratio(new Measure(2), new Measure(3)))
            .BiasToDominant(new Ratio(new Measure(3), new Measure(4)))
            .WithResidual(Ratio.Tension(new Measure(6)))
            .Publish(PublishCarrierP, new Ratio(new Measure(3), Measure.One))
            .Publish(PublishCarrierQ, new Ratio(Measure.One, Measure.One))
            .Request(ListenCarrierP, new Ratio(new Measure(2), Measure.One))
            .Request(ListenCarrierQ, new Ratio(Measure.One, Measure.One)));

    public Ratio Recessive => Site.Kernel.Recessive.Normalize();

    public Ratio Dominant => Site.Kernel.Dominant.Normalize();

    public Ratio Residual => Site.Orthogonal.Residual.Normalize();

    public Ratio Stiffness => Site.Kernel.Stiffness.Normalize();

    public Ratio Route => Site.Kernel.RoutingGain.Normalize();

    public Ratio Bias => Site.Kernel.DominantBias.Normalize();

    public ImmutableArray<Flow> PublishPorts => [.. Site.Orthogonal.Flows.Where(flow => flow.Support is Carrier && flow.IsOutgoing)];

    public ImmutableArray<Flow> ListenPorts => [.. Site.Orthogonal.Flows.Where(flow => flow.Support is Carrier && flow.IsIncoming)];
}
