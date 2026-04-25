namespace Core4.Experimental.Bootstrap;

public static class SupportReadEquations
{
    public static long Net(long reverse, long forward) => forward + reverse;

    public static long TotalSupport(long reverse, long forward) => forward - reverse;

    public static long ForwardOccupancy(long reverse, long forward) => forward;

    public static long ReverseOccupancy(long reverse, long forward) => reverse;

    public static long Tension(long reverse, long forward) => Math.Min(forward, -reverse);

    public static long Exclusive(long reverse, long forward) => Math.Abs(Net(reverse, forward));

    public static long ForwardResidue(long reverse, long forward) => Math.Max(Net(reverse, forward), 0);

    public static long ReverseResidue(long reverse, long forward) => Math.Min(Net(reverse, forward), 0);

    public static long LargerFlow(long reverse, long forward) => forward >= -reverse ? forward : reverse;

    public static long CanceledSupport(long reverse, long forward) => TotalSupport(reverse, forward) - Exclusive(reverse, forward);
}
