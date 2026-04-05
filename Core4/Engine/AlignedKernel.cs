namespace Core4.Engine;

/// <summary>
/// The fixed aligned pair for one carrier-local reading. Recessive retains anchor or memory, and dominant carries
/// the current asserted state for that carrier.
/// Example: <c>var kernel = AlignedKernel.Open.Anchor(new Ratio(0, 1)).Assert(new Ratio(5, 10));</c>
/// </summary>
public readonly record struct AlignedKernel(Ratio Recessive, Ratio Dominant)
{
    public static AlignedKernel Open => new(Ratio.Zero(Measure.Zero), Ratio.Zero(Measure.Zero));

    public Measure Potential => Recessive.Potential + Dominant.Potential;

    public Measure OccupiedCount => Count(Recessive) + Count(Dominant);

    public Measure BoundCount => Bound(Recessive) + Bound(Dominant);

    public bool IsOpen => OccupiedCount == Measure.Zero;

    public AlignedKernel Anchor(Ratio state) => new(state.Normalize(), Dominant);

    public AlignedKernel Assert(Ratio state) => new(Recessive, state.Normalize());

    public AlignedKernel Focus(Measure energy) => Assert(Ratio.Tension(energy));

    private static Measure Count(Ratio state) => state.Potential != Measure.Zero || state.Dominant != Measure.Zero
        ? Measure.One
        : Measure.Zero;

    private static Measure Bound(Ratio state) => state.Dominant != Measure.Zero
        ? Measure.One
        : Measure.Zero;
}
