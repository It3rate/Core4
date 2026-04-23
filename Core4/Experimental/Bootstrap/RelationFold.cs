namespace Core4.Experimental.Bootstrap;

public sealed class RelationFold : IElement
{
    public RelationFold(IElement left, IElement right)
    {
        Left = left;
        Right = right;
        var folded = Fold(left, right);
        Numerator = Measure.Fixed(folded.Numerator);
        Denominator = Measure.Fixed(folded.ResolvedDenominator);
        Reduced = Reduce(left, right, Numerator, Denominator);
    }

    public IElement Left { get; }
    public IElement Right { get; }
    public IElement Numerator { get; }
    public IElement Denominator { get; }

    public IElement Reduced { get; }

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
        var flowDirection = denominator == 0 ? Math.Sign(numerator) : Math.Sign(denominator);
        var totalWidth = Math.Abs(numerator);
        var leadWidth = totalWidth % unitSize;
        var firstWidth = leadWidth == 0 ? unitSize : leadWidth;
        var runStart = flowDirection >= 0 ? Math.Min(0, numerator) : Math.Max(0, numerator);
        var emitted = 0L;
        var currentStart = runStart;

        while (emitted < totalWidth)
        {
            var widthMagnitude = emitted == 0 ? firstWidth : Math.Min(unitSize, totalWidth - emitted);
            if (widthMagnitude <= 0)
            {
                yield break;
            }

            var width = flowDirection * widthMagnitude;
            yield return new(currentStart, width);
            currentStart += width;
            emitted += widthMagnitude;
        }
    }

    private static FoldValue Fold(IElement op0, IElement op1)
    {
        var left = Read(op0);
        var right = Read(op1);

        if (left.Denominator is null && right.Denominator is null)
        {
            var extent = right.Numerator - left.Numerator;
            return new(extent, extent);
        }

        if (right.Denominator == 0)
        {
            return new(left.Numerator + right.Numerator, left.ResolvedDenominator);
        }

        var result = 0d;
        var ratio1 = right.Proportion;
        foreach (var partition in op0.Partitions())
        {
            result += partition.Width * ratio1;
        }

        return new(ToLong(result), MergeDenominator(left, right));
    }

    private static IElement Reduce(IElement left, IElement right, IElement numerator, IElement denominator) =>
        Relation.OpenSiteCount(left) + Relation.OpenSiteCount(right) > 0
            ? new Relation(left, right)
            : new Relation(numerator, denominator);

    private static long RawOf(IElement element) => element switch
    {
        Measure measure => measure.Raw,
        RelationFold fold => RawOf(fold.Numerator),
        Relation relation => RawOf(new RelationFold(relation.Left, relation.Right).Numerator),
        _ => 0
    };

    private static FoldValue Read(IElement element) => element switch
    {
        Measure measure => new(measure.Raw, null),
        RelationFold fold => new(RawOf(fold.Numerator), RawOf(fold.Denominator)),
        Relation relation => Fold(relation.Left, relation.Right),
        _ => new(0, null)
    };

    private static long MergeDenominator(FoldValue left, FoldValue right)
    {
        if (left.Denominator is long leftDenominator && leftDenominator != 0)
        {
            return leftDenominator;
        }

        if (right.Denominator is long rightDenominator && rightDenominator != 0)
        {
            return rightDenominator;
        }

        return left.ResolvedDenominator;
    }

    private static long ToLong(double value) => (long)Math.Round(value, MidpointRounding.AwayFromZero);

    private readonly record struct FoldValue(long Numerator, long? Denominator)
    {
        public long ResolvedDenominator => Denominator ?? 0;

        public double Proportion => Denominator switch
        {
            null => Numerator,
            0 => Numerator,
            var value => (double)Numerator / value.Value
        };
    }
}
