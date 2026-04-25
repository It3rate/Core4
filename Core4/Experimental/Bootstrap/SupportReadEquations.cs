namespace Core4.Experimental.Bootstrap;

public static class SupportReadEquations
{
    private static readonly IElement None = Coefficients(0, 0);
    private static readonly IElement ReverseSlot = Coefficients(1, 0);
    private static readonly IElement ForwardSlot = Coefficients(0, 1);
    private static readonly IElement BothSlots = Coefficients(1, 1);
    private static readonly IElement BalancedSlots = Coefficients(-1, 1);

    public static IElement Empty(IElement pair) => Select(pair, None);

    public static IElement Support(IElement pair) => Select(pair, BothSlots);

    public static IElement Net(IElement pair)
    {
        return Scale(NetMagnitude(pair), BothSlots);
    }

    public static IElement ForwardOccupancy(IElement pair) => Select(pair, ForwardSlot);

    public static IElement ReverseOccupancy(IElement pair) => Select(pair, ReverseSlot);

    public static IElement Tension(IElement pair)
    {
        ReadPair(pair, out var reverse, out var forward);
        return Scale(Math.Min(forward, -reverse), BalancedSlots);
    }

    public static IElement Exclusive(IElement pair) => Scale(NetMagnitude(pair), BothSlots);

    public static IElement ForwardResidue(IElement pair) => Scale(NetMagnitude(pair), ForwardSlot);

    public static IElement ReverseResidue(IElement pair) => Scale(NetMagnitude(pair), ReverseSlot);

    public static IElement LargerFlow(IElement pair)
    {
        ReadPair(pair, out var reverse, out var forward);
        return forward >= -reverse ? Select(pair, ForwardSlot) : Select(pair, ReverseSlot);
    }

    public static IElement CanceledSupport(IElement pair) => Tension(pair);

    public static long NetMagnitude(IElement pair)
    {
        ReadPair(pair, out var reverse, out var forward);
        return forward + reverse;
    }

    public static long SupportMagnitude(IElement pair)
    {
        ReadPair(pair, out var reverse, out var forward);
        return forward - reverse;
    }

    private static void ReadPair(IElement pair, out long reverse, out long forward)
    {
        if (pair is Relation relation &&
            relation.Left is Measure reverseMeasure &&
            relation.Right is Measure forwardMeasure)
        {
            reverse = reverseMeasure.Raw;
            forward = forwardMeasure.Raw;
            return;
        }

        reverse = 0;
        forward = 0;
    }

    private static IElement Select(IElement pair, IElement coefficients)
    {
        ReadPair(pair, out var reverse, out var forward);
        ReadPair(coefficients, out var reverseFactor, out var forwardFactor);
        return Emit(reverse * reverseFactor, forward * forwardFactor);
    }

    private static IElement Scale(long magnitude, IElement coefficients)
    {
        ReadPair(coefficients, out var reverseFactor, out var forwardFactor);
        return Emit(magnitude * reverseFactor, magnitude * forwardFactor);
    }

    private static IElement Coefficients(long reverse, long forward) =>
        new Relation(Measure.Fixed(reverse), Measure.Fixed(forward));

    private static IElement Emit(long reverse, long forward)
    {
        // TODO: These slot clamps are standing in for a future structural law.
        // The reverse slot should eventually be a container that rejects direct
        // flow, and the forward slot should reject inverse flow, likely through
        // the same boundary machinery used elsewhere. For now we truncate
        // incompatible emissions numerically so pair-valued operators remain
        // stable while that container/boundary layer is still missing.
        var clampedReverse = Math.Min(reverse, 0);
        var clampedForward = Math.Max(forward, 0);
        return new Relation(Measure.Fixed(clampedReverse), Measure.Fixed(clampedForward));
    }
}
