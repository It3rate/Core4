using Core4.Numerics;

namespace Core4.Structure;

/// <summary>
/// The fixed aligned pair for one carrier-local reading. Recessive retains anchor or memory, and dominant carries
/// the current asserted state for that carrier.
/// Stiffness carries how hard the aligned claim currently holds, with <c>0/1</c> fully open and <c>1/1</c> fully
/// pinned for the current engine pass. Routing gain carries how much of the softened share is currently routed back
/// into the aligned claim, with <c>1/1</c> meaning fully routeable.
/// Example: <c>var kernel = AlignedKernel.Open.Anchor(new Ratio(0, 1)).Assert(new Ratio(5, 10)).Harden(new Ratio(1, 1)).Route(new Ratio(1, 2));</c>
/// </summary>
public readonly record struct AlignedKernel(Ratio Recessive, Ratio Dominant, Ratio Stiffness, Ratio RoutingGain)
{
    public static AlignedKernel Open => new(
        Ratio.Zero(Measure.Zero),
        Ratio.Zero(Measure.Zero),
        Ratio.Zero(Measure.One),
        new Ratio(Measure.One, Measure.One));

    public Measure Potential => Recessive.Potential + Dominant.Potential;

    public Measure OccupiedCount => Count(Recessive) + Count(Dominant);

    public Measure BoundCount => Bound(Recessive) + Bound(Dominant);

    public bool IsOpen => OccupiedCount == Measure.Zero;

    public AlignedKernel Anchor(Ratio state) => new(state.Normalize(), Dominant, Stiffness, RoutingGain);

    public AlignedKernel Assert(Ratio state) => new(Recessive, state.Normalize(), Stiffness, RoutingGain);

    public AlignedKernel Focus(Measure energy) => Assert(Ratio.Tension(energy));

    public AlignedKernel Harden(Ratio stiffness) => new(Recessive, Dominant, stiffness.Normalize(), RoutingGain);

    public AlignedKernel Route(Ratio gain) => new(Recessive, Dominant, Stiffness, gain.Normalize());

    private static Measure Count(Ratio state) => state.Potential != Measure.Zero || state.Dominant != Measure.Zero
        ? Measure.One
        : Measure.Zero;

    private static Measure Bound(Ratio state) => state.Dominant != Measure.Zero
        ? Measure.One
        : Measure.Zero;
}
