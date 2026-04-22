namespace Core4.Experimental.Bootstrap;

public sealed class RelationFold : IElement
{
    public RelationFold(IElement left, IElement right)
    {
        Left = left;
        Right = right;
        Reduced = Fold(left, right);
    }

    public IElement Left { get; }
    public IElement Right { get; }
    public IElement Numerator => Reduced.Left;
    public IElement Denominator => Reduced.Right;

    public Relation Reduced { get; }

    public long PartitionCount => PartitionCountOf(RawOf(Numerator), RawOf(Denominator));

    public IEnumerable<PartitionStep> Partitions() => PartitionsOf(RawOf(Numerator), RawOf(Denominator));

    public override string ToString() => $"({Numerator}/{Denominator})";

    internal static long PartitionCountOf(long numerator, long denominator)
    {
        var unitSize = denominator == 0 ? 1 : Math.Abs(denominator);
        return numerator == 0 ? 0 : (Math.Abs(numerator) + unitSize - 1) / unitSize;
    }

    internal static IEnumerable<PartitionStep> PartitionsOf(long numerator, long denominator)
    {
        if (numerator == 0)
        {
            yield break;
        }

        var unitSize = denominator == 0 ? 1 : Math.Abs(denominator);
        var magnitude = Math.Abs(numerator);
        var direction = Math.Sign(numerator);

        for (long start = 0; start < magnitude; start += unitSize)
        {
            var width = Math.Min(unitSize, magnitude - start);
            if (width <= 0)
            {
                yield break;
            }

            yield return new(direction * start, direction * width);
        }
    }

    private static Relation Fold(IElement op0, IElement op1)
    {
        if (op0 is Measure leftMeasure && op1 is Measure rightMeasure)
        {
            return FoldMeasures(leftMeasure, rightMeasure);
        }

        var num0 = RawOf(op0.Numerator);
        var den0 = RawOf(op0.Denominator);
        var num1 = RawOf(op1.Numerator);
        var den1 = RawOf(op1.Denominator);

        if (den1 == 0)
        {
            return ReducedValue(num0 + num1, den0 == 0 ? 0 : den0);
        }

        var result = 0d;
        var ratio1 = (double)num1 / den1;
        foreach (var partition in op0.Partitions())
        {
            result += partition.Width * ratio1;
        }

        return ReducedValue(ToLong(result), den0 == 0 ? den1 : den0);
    }

    private static Relation FoldMeasures(Measure left, Measure right)
    {
        var extent = right.Raw - left.Raw;
        var direction = Math.Sign(extent);
        var partitions = Math.Abs(extent);
        long filled = 0;

        for (long tick = 0; tick < partitions; tick++)
        {
            filled += direction;
        }

        return ReducedValue(filled, extent);
    }

    private static Relation ReducedValue(long numerator, long denominator) =>
        new(Measure.Fixed(numerator), Measure.Fixed(denominator));

    private static long RawOf(IElement element) => element switch
    {
        Measure measure => measure.Raw,
        RelationFold fold => RawOf(fold.Numerator),
        Relation relation => RawOf(relation.Numerator),
        _ => 0
    };

    private static long ToLong(double value) => (long)Math.Round(value, MidpointRounding.AwayFromZero);
}
