using Core4.Aliases;
using Core4.Numerics;

namespace Core4.Structure;

/// <summary>
/// A local place where connection can happen. A site has a placement, a fixed aligned kernel, and an orthogonal
/// reservoir that can hold unresolved remainder plus articulated child flows. Aligned state is same-support local
/// meaning; orthogonal state is foreign or not-yet-translated structure at this site, even if that structure is
/// already well-formed elsewhere.
/// Example: <c>var site = Site.Open(x, 0).WithResidual(Ratio.Tension(10));</c>
/// </summary>
public readonly record struct Site(Placement Placement, AlignedKernel Kernel, OrthogonalReservoir Orthogonal)
{
    public static Site Open(Carrier carrier, Measure location) => new(carrier.At(location), AlignedKernel.Open, OrthogonalReservoir.Open);

    public static Site Open(Placement placement) => new(placement, AlignedKernel.Open, OrthogonalReservoir.Open);

    public static Site Tension(Carrier carrier, Measure location, Measure energy) => Open(carrier, location).Focus(energy);

    public Carrier Carrier => Placement.PrimaryCarrier;

    public ISupportProjection PositionRead => Placement.ProjectionRead;

    // TODO(core): Delete this compatibility alias once callers consume PositionRead directly.
    public Ratio Position => Placement.Projection;

    public ISupportProjection StateRead => Kernel.DominantRead;

    // TODO(core): Delete this compatibility alias once callers consume StateRead directly.
    public Ratio State => StateRead.CompatibilityRatio();

    public ISupportProjection StiffnessRead => Kernel.StiffnessRead;

    // TODO(core): Delete this compatibility alias once callers consume StiffnessRead directly.
    public Ratio Stiffness => StiffnessRead.CompatibilityRatio();

    public ISupportProjection RoutingGainRead => Kernel.RoutingGainRead;

    // TODO(core): Delete this compatibility alias once callers consume RoutingGainRead directly.
    public Ratio RoutingGain => RoutingGainRead.CompatibilityRatio();

    public ISupportProjection DominantBiasRead => Kernel.DominantBiasRead;

    // TODO(core): Delete this compatibility alias once callers consume DominantBiasRead directly.
    public Ratio DominantBias => DominantBiasRead.CompatibilityRatio();

    public ISupportProjection ResidualRead => Orthogonal.ResidualRead;

    // TODO(core): Delete this compatibility alias once callers consume ResidualRead directly.
    public Ratio Residual => ResidualRead.CompatibilityRatio();

    public Measure Potential => Kernel.Potential + Orthogonal.Potential;

    public bool IsOpen => Kernel.IsOpen && Orthogonal.OccupiedCount == Measure.Zero;

    public bool IsTension => Kernel.BoundCount + Orthogonal.BoundCount == Measure.Zero && Potential != Measure.Zero;

    public ISupportProjection OnRead(Carrier carrier) => Placement.OnRead(carrier);

    // TODO(core): Delete this compatibility alias once callers consume OnRead directly.
    public Ratio On(Carrier carrier) => OnRead(carrier).CompatibilityRatio();

    public Measure Location => OnRead(Carrier).CompatibilityRatio().Whole;

    public Site Place(Placement placement) => new(placement, Kernel, Orthogonal);

    public Site AnchorRead(ISupportProjection state) => new(Placement, Kernel.AnchorRead(state), Orthogonal);

    // TODO(core): Delete this compatibility overload once callers pass support-shaped reads directly.
    public Site Anchor(Ratio state) => AnchorRead(state);

    public Site Focus(Measure energy) => new(Placement, Kernel.Focus(energy), Orthogonal);

    public Site ResolveRead(ISupportProjection state) => new(Placement, Kernel.AssertRead(state), Orthogonal);

    // TODO(core): Delete this compatibility overload once callers pass support-shaped reads directly.
    public Site Resolve(Ratio state) => ResolveRead(state);

    public Site HardenRead(ISupportProjection stiffness) => new(Placement, Kernel.HardenRead(stiffness), Orthogonal);

    // TODO(core): Delete this compatibility overload once callers pass support-shaped reads directly.
    public Site Harden(Ratio stiffness) => HardenRead(stiffness);

    public Site RouteRead(ISupportProjection gain) => new(Placement, Kernel.RouteRead(gain), Orthogonal);

    // TODO(core): Delete this compatibility overload once callers pass support-shaped reads directly.
    public Site Route(Ratio gain) => RouteRead(gain);

    public Site BiasToDominantRead(ISupportProjection bias) => new(Placement, Kernel.BiasToDominantRead(bias), Orthogonal);

    // TODO(core): Delete this compatibility overload once callers pass support-shaped reads directly.
    public Site BiasToDominant(Ratio bias) => BiasToDominantRead(bias);

    public Site Pin() => Harden(new Ratio(Measure.One, Measure.One));

    public Site Pin(ISupportProjection stiffness) => HardenRead(stiffness);

    // TODO(core): Delete this compatibility overload once callers pass support-shaped reads directly.
    public Site Pin(Ratio stiffness) => HardenRead(stiffness);

    public Site WithResidualRead(ISupportProjection residual) => new(Placement, Kernel, Orthogonal.ResolveRead(residual));

    // TODO(core): Delete this compatibility overload once callers pass support-shaped reads directly.
    public Site WithResidual(Ratio residual) => WithResidualRead(residual);

    public Site Connect(Flow flow) => new(Placement, Kernel, Orthogonal.Add(flow));

    public Site Connect(OrthogonalPublication publication) => new(Placement, Kernel, Orthogonal.Add(publication));

    public Site PublishRead(ISupport support, ISupportProjection value) =>
        Connect(new Flow(support, value, Provenance.SelfDriven(value.Potential)));

    // TODO(core): Delete this compatibility overload once callers pass support-shaped reads directly.
    public Site Publish(ISupport support, Ratio value) => PublishRead(support, value);

    public Site RequestRead(ISupport support, ISupportProjection value) =>
        Connect(new Flow(support, value, Provenance.OtherDriven(value.Potential)));

    // TODO(core): Delete this compatibility overload once callers pass support-shaped reads directly.
    public Site Request(ISupport support, Ratio value) => RequestRead(support, value);

    public Site FeedbackRead(ISupport support, ISupportProjection desired) => RequestRead(support, desired);

    // TODO(core): Delete this compatibility overload once callers pass support-shaped reads directly.
    public Site Feedback(ISupport support, Ratio desired) => FeedbackRead(support, desired);

    public Site ExchangeRead(ISupport support, ISupportProjection value, Provenance provenance) =>
        Connect(new Flow(support, value, provenance));

    // TODO(core): Delete this compatibility overload once callers pass support-shaped reads directly.
    public Site Exchange(ISupport support, Ratio value, Provenance provenance) => ExchangeRead(support, value, provenance);

    public Site PublishReference(ISupport support) => PublishReference(support, this, StateRead);

    public Site PublishReference(ISupport support, Site site) => PublishReference(support, site, site.StateRead);

    public Site PublishReference(ISupport support, Site site, ISupportProjection value) =>
        Connect(OrthogonalPublication.Publish(support, site, value));

    // TODO(core): Delete this compatibility overload once callers pass support-shaped reads directly.
    public Site PublishReference(ISupport support, Site site, Ratio value) => PublishReference(support, site, (ISupportProjection)value);

    public Relation Relate(Site anchor) => new(Carrier, this, anchor);

    public Relation Relate(Carrier carrier, Site anchor) => new(carrier, this, anchor);
}
