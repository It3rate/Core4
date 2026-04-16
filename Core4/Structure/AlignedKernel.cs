using Core4.Numerics;

namespace Core4.Structure;

/// <summary>
/// The fixed aligned pair for one carrier-local reading. Recessive retains anchor or memory, and dominant carries
/// the current asserted state for that carrier.
/// Stiffness carries how hard the aligned claim currently holds, with <c>0/1</c> fully open and <c>1/1</c> fully
/// pinned for the current engine pass. Routing gain carries how much of the softened share is currently routed back
/// into the aligned structure, with <c>1/1</c> meaning fully routeable. Dominant bias partitions that routed share
/// between aligned recessive support and aligned dominant claim.
/// Example: <c>var kernel = AlignedKernel.Open.Anchor(new Ratio(0, 1)).Assert(new Ratio(5, 10)).Harden(new Ratio(1, 1)).Route(new Ratio(1, 2)).BiasToDominant(new Ratio(3, 4));</c>
/// </summary>
public readonly record struct AlignedKernel(
    SupportProjectionRead RecessiveRead,
    SupportProjectionRead DominantRead,
    SupportProjectionRead StiffnessRead,
    SupportProjectionRead RoutingGainRead,
    SupportProjectionRead DominantBiasRead)
{
    public AlignedKernel(
        ISupportProjection recessive,
        ISupportProjection dominant,
        ISupportProjection stiffness,
        ISupportProjection routingGain,
        ISupportProjection dominantBias)
        : this(
            recessive.ToSupportRead(),
            dominant.ToSupportRead(),
            stiffness.ToSupportRead(),
            routingGain.ToSupportRead(),
            dominantBias.ToSupportRead())
    {
    }

    public static AlignedKernel Open => new(
        Ratio.Zero(Measure.Zero),
        Ratio.Zero(Measure.Zero),
        Ratio.Zero(Measure.One),
        new Ratio(Measure.One, Measure.One),
        new Ratio(Measure.One, Measure.One));

    // TODO(core): Delete this compatibility alias once callers consume RecessiveRead directly.
    public Ratio Recessive => RecessiveRead.CompatibilityRatio();

    // TODO(core): Delete this compatibility alias once callers consume DominantRead directly.
    public Ratio Dominant => DominantRead.CompatibilityRatio();

    // TODO(core): Delete this compatibility alias once callers consume StiffnessRead directly.
    public Ratio Stiffness => StiffnessRead.CompatibilityRatio();

    // TODO(core): Delete this compatibility alias once callers consume RoutingGainRead directly.
    public Ratio RoutingGain => RoutingGainRead.CompatibilityRatio();

    // TODO(core): Delete this compatibility alias once callers consume DominantBiasRead directly.
    public Ratio DominantBias => DominantBiasRead.CompatibilityRatio();

    public Measure Potential => RecessiveRead.Potential + DominantRead.Potential;

    public Measure OccupiedCount => Count(RecessiveRead) + Count(DominantRead);

    public Measure BoundCount => Bound(RecessiveRead) + Bound(DominantRead);

    public bool IsOpen => OccupiedCount == Measure.Zero;

    public AlignedKernel AnchorRead(ISupportProjection state) => new(
        state.ToSupportRead(),
        DominantRead,
        StiffnessRead,
        RoutingGainRead,
        DominantBiasRead);

    // TODO(core): Delete this compatibility overload once callers pass support-shaped reads directly.
    public AlignedKernel Anchor(Ratio state) => AnchorRead(state);

    public AlignedKernel AssertRead(ISupportProjection state) => new(
        RecessiveRead,
        state.ToSupportRead(),
        StiffnessRead,
        RoutingGainRead,
        DominantBiasRead);

    // TODO(core): Delete this compatibility overload once callers pass support-shaped reads directly.
    public AlignedKernel Assert(Ratio state) => AssertRead(state);

    public AlignedKernel Focus(Measure energy) => AssertRead(Ratio.Tension(energy));

    public AlignedKernel HardenRead(ISupportProjection stiffness) => new(
        RecessiveRead,
        DominantRead,
        stiffness.ToSupportRead(),
        RoutingGainRead,
        DominantBiasRead);

    // TODO(core): Delete this compatibility overload once callers pass support-shaped reads directly.
    public AlignedKernel Harden(Ratio stiffness) => HardenRead(stiffness);

    public AlignedKernel RouteRead(ISupportProjection gain) => new(
        RecessiveRead,
        DominantRead,
        StiffnessRead,
        gain.ToSupportRead(),
        DominantBiasRead);

    // TODO(core): Delete this compatibility overload once callers pass support-shaped reads directly.
    public AlignedKernel Route(Ratio gain) => RouteRead(gain);

    public AlignedKernel BiasToDominantRead(ISupportProjection bias) => new(
        RecessiveRead,
        DominantRead,
        StiffnessRead,
        RoutingGainRead,
        bias.ToSupportRead());

    // TODO(core): Delete this compatibility overload once callers pass support-shaped reads directly.
    public AlignedKernel BiasToDominant(Ratio bias) => BiasToDominantRead(bias);

    private static Measure Count(ISupportProjection state) => state.Potential != Measure.Zero || state.CompatibilityRatio().Dominant != Measure.Zero
        ? Measure.One
        : Measure.Zero;

    private static Measure Bound(ISupportProjection state) => state.CompatibilityRatio().Dominant != Measure.Zero
        ? Measure.One
        : Measure.Zero;
}
