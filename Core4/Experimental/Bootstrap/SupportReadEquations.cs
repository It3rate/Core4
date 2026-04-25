namespace Core4.Experimental.Bootstrap;

public static class SupportReadEquations
{
    private static readonly IElement None = Coefficients(0, 0);
    private static readonly IElement ReverseSlot = Coefficients(1, 0);
    private static readonly IElement ForwardSlot = Coefficients(0, 1);
    private static readonly IElement BothSlots = Coefficients(1, 1);
    private static readonly IElement BalancedSlots = Coefficients(-1, 1);
    private static readonly IElement EmptyTable = TruthTable(None, None, None, None);
    private static readonly IElement SupportTable = TruthTable(None, ReverseSlot, ForwardSlot, BothSlots);
    private static readonly IElement ForwardOccupancyTable = TruthTable(None, None, ForwardSlot, ForwardSlot);
    private static readonly IElement ReverseOccupancyTable = TruthTable(None, ReverseSlot, None, ReverseSlot);
    private static readonly IElement TensionTable = TruthTable(None, None, None, BalancedSlots);
    private static readonly IElement NetTable = TruthTable(None, ReverseSlot, ForwardSlot, BothSlots);
    private static readonly IElement ForwardResidueTable = TruthTable(None, None, ForwardSlot, ForwardSlot);
    private static readonly IElement ReverseResidueTable = TruthTable(None, ReverseSlot, None, ReverseSlot);
    private static readonly IElement LargerForwardTable = TruthTable(None, ReverseSlot, ForwardSlot, ForwardSlot);
    private static readonly IElement LargerReverseTable = TruthTable(None, ReverseSlot, ForwardSlot, ReverseSlot);

    public static IElement Empty(IElement pair) => ApplyTable(pair, EmptyTable);

    public static IElement Support(IElement pair) => ApplyTable(pair, SupportTable);

    public static IElement Net(IElement pair) => ScaleByTable(pair, NetTable, NetMagnitude);

    public static IElement ForwardOccupancy(IElement pair) => ApplyTable(pair, ForwardOccupancyTable);

    public static IElement ReverseOccupancy(IElement pair) => ApplyTable(pair, ReverseOccupancyTable);

    public static IElement Tension(IElement pair) => ScaleByTable(pair, TensionTable, TensionMagnitude);

    public static IElement Exclusive(IElement pair) => ScaleByTable(pair, NetTable, NetMagnitude);

    public static IElement ForwardResidue(IElement pair) => ScaleByTable(pair, ForwardResidueTable, NetMagnitude);

    public static IElement ReverseResidue(IElement pair) => ScaleByTable(pair, ReverseResidueTable, NetMagnitude);

    public static IElement LargerFlow(IElement pair)
    {
        ReadPair(pair, out var reverse, out var forward);
        var table = forward >= -reverse ? LargerForwardTable : LargerReverseTable;
        return ApplyTable(pair, table);
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

    public static long TensionMagnitude(IElement pair)
    {
        ReadPair(pair, out var reverse, out var forward);
        return Math.Min(forward, -reverse);
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

    private static IElement ApplyTable(IElement pair, IElement table)
    {
        var coefficients = ReadTruthEntry(table, pair);
        return ApplyCoefficients(pair, coefficients);
    }

    private static IElement ScaleByTable(IElement pair, IElement table, Func<IElement, long> magnitude)
    {
        var coefficients = ReadTruthEntry(table, pair);
        return Scale(magnitude(pair), coefficients);
    }

    private static IElement ApplyCoefficients(IElement pair, IElement coefficients)
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

    private static IElement TruthTable(IElement empty, IElement reverseOnly, IElement forwardOnly, IElement both) =>
        new Relation(
            new Relation(empty, reverseOnly),
            new Relation(forwardOnly, both));

    private static IElement ReadTruthEntry(IElement table, IElement pair)
    {
        ReadPair(pair, out var reverse, out var forward);
        var reversePresent = reverse != 0;
        var forwardPresent = forward != 0;

        if (table is not Relation outer ||
            outer.Left is not Relation leftBranch ||
            outer.Right is not Relation rightBranch)
        {
            return None;
        }

        return reversePresent
            ? forwardPresent ? rightBranch.Right : leftBranch.Right
            : forwardPresent ? rightBranch.Left : leftBranch.Left;
    }

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
