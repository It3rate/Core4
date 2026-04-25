namespace Core4.Experimental.Bootstrap;

public static class SupportReadEquations
{
    public static IElement Empty(IElement pair) => Pair(0, 0);

    public static IElement Support(IElement pair)
    {
        ReadPair(pair, out var reverse, out var forward);
        return Pair(reverse, forward);
    }

    public static IElement Net(IElement pair)
    {
        ReadPair(pair, out var reverse, out var forward);
        var net = forward + reverse;
        return Pair(net, net);
    }

    public static IElement ForwardOccupancy(IElement pair)
    {
        ReadPair(pair, out _, out var forward);
        return Pair(0, forward);
    }

    public static IElement ReverseOccupancy(IElement pair)
    {
        ReadPair(pair, out var reverse, out _);
        return Pair(reverse, 0);
    }

    public static IElement Tension(IElement pair)
    {
        ReadPair(pair, out var reverse, out var forward);
        var overlap = Math.Min(forward, -reverse);
        return Pair(-overlap, overlap);
    }

    public static IElement Exclusive(IElement pair)
    {
        ReadPair(pair, out var reverse, out var forward);
        var net = forward + reverse;
        return Pair(net, net);
    }

    public static IElement ForwardResidue(IElement pair)
    {
        ReadPair(pair, out var reverse, out var forward);
        var net = forward + reverse;
        return Pair(0, net);
    }

    public static IElement ReverseResidue(IElement pair)
    {
        ReadPair(pair, out var reverse, out var forward);
        var net = forward + reverse;
        return Pair(net, 0);
    }

    public static IElement LargerFlow(IElement pair)
    {
        ReadPair(pair, out var reverse, out var forward);
        return forward >= -reverse ? Pair(0, forward) : Pair(reverse, 0);
    }

    public static IElement CanceledSupport(IElement pair)
    {
        ReadPair(pair, out var reverse, out var forward);
        var overlap = Math.Min(forward, -reverse);
        return Pair(-overlap, overlap);
    }

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

    private static IElement Pair(long reverse, long forward)
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
