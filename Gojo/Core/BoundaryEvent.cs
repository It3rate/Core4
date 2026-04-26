namespace Gojo.Core;

public readonly record struct BoundaryEvent(long Side, long Travel)
{
    public static BoundaryEvent PositiveOverflow => new(1, 1);

    public static BoundaryEvent PositiveRefill => new(1, -1);

    public static BoundaryEvent NegativeRefill => new(-1, 1);

    public static BoundaryEvent NegativeOverflow => new(-1, -1);

    public long Alignment => Math.Sign(Side * Travel);
}
