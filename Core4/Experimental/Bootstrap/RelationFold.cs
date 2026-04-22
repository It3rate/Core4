namespace Core4.Experimental.Bootstrap;

public sealed class RelationFold : IElement
{
    public RelationFold(IElement left, IElement right)
    {
        Left = left;
        Right = right;

        var reduced = Reduce(left, right);
        Numerator = Measure.Fixed(ToLong(reduced.Numerator));
        Denominator = Measure.Fixed(ToLong(reduced.Denominator));
    }

    public IElement Left { get; }
    public IElement Right { get; }
    public IElement Numerator { get; }
    public IElement Denominator { get; }

    public Relation Reduced => new(Numerator, Denominator);

    public long PartitionCount => ElementProjection.Of(this).PartitionCount;

    public IEnumerable<PartitionStep> Partitions() => ElementProjection.Of(this).Partitions();

    public override string ToString() => $"({Numerator}/{Denominator})";

    private static ElementProjection Reduce(IElement left, IElement right)
    {
        if (left is Measure leftMeasure && right is Measure rightMeasure)
        {
            return ReduceMeasures(leftMeasure, rightMeasure);
        }

        var leftProjection = ElementProjection.Of(left);
        var rightProjection = ElementProjection.Of(right);

        if (rightProjection.Denominator == 0)
        {
            var denominator = leftProjection.Denominator == 0 ? 0 : leftProjection.Denominator;
            return new(leftProjection.Numerator + rightProjection.Numerator, denominator);
        }

        var accumulated = 0d;
        var partnerValue = (double)rightProjection.Numerator / rightProjection.Denominator;
        foreach (var partition in left.Partitions())
        {
            accumulated += partition.Width * partnerValue;
        }

        var resultDenominator = leftProjection.Denominator == 0 ? rightProjection.Denominator : leftProjection.Denominator;
        return new(ToLong(accumulated), resultDenominator);
    }

    private static ElementProjection ReduceMeasures(Measure left, Measure right)
    {
        var extent = right.Raw - left.Raw;
        var direction = Math.Sign(extent);
        var partitions = Math.Abs(extent);
        long filled = 0;

        for (long tick = 0; tick < partitions; tick++)
        {
            filled += direction;
        }

        return new(filled, extent);
    }

    private static long ToLong(double value) => (long)Math.Round(value, MidpointRounding.AwayFromZero);
}

internal readonly record struct ElementProjection(long Numerator, long Denominator)
{
    public long UnitSize => Denominator == 0 ? 1 : Math.Abs(Denominator);

    public long PartitionCount =>
        Numerator == 0
            ? 0
            : (Math.Abs(Numerator) + UnitSize - 1) / UnitSize;

    public IEnumerable<PartitionStep> Partitions()
    {
        if (Numerator == 0)
        {
            yield break;
        }

        var magnitude = Math.Abs(Numerator);
        var unitSize = UnitSize;
        var direction = Math.Sign(Numerator);

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

    public static ElementProjection Of(IElement element) => element switch
    {
        Measure measure => new(measure.Raw, 0),
        Relation relation => Of(new RelationFold(relation.Left, relation.Right)),
        RelationFold fold => new(ReadScalar(fold.Numerator), ReadScalar(fold.Denominator)),
        _ => new(0, 0)
    };

    private static long ReadScalar(IElement element) => element switch
    {
        Measure measure => measure.Raw,
        RelationFold fold => Of(fold).Numerator,
        Relation relation => Of(new RelationFold(relation.Left, relation.Right)).Numerator,
        _ => 0
    };
}
