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

    public long PartitionCount => PartitionCountFor(this);

    public IEnumerable<PartitionStep> Partitions() => PartitionsFor(this);

    public override string ToString() => $"({Numerator}/{Denominator})";

    internal static long PartitionCountOf(long numerator, long denominator)
    {
        var unitSize = denominator == 0 ? 1 : Math.Abs(denominator);
        return numerator == 0 ? 0 : (Math.Abs(numerator) + unitSize - 1) / unitSize;
    }

    internal static long PartitionCountFor(IElement element) => PartitionsFor(element).LongCount();

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

    internal static IEnumerable<PartitionStep> PartitionsFor(IElement element) => element switch
    {
        Measure measure => PartitionsOf(measure.Raw, 0),
        RelationFold fold => PartitionsForReduced(fold.Reduced, fold.Numerator, fold.Denominator),
        Relation relation => PartitionsForRelation(relation),
        _ => Array.Empty<PartitionStep>()
    };

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
        ShouldPreserveOpenStructure(left, right)
            ? new Relation(left, right)
            : ReduceClosedStructure(left, right, numerator, denominator);

    private static bool ShouldPreserveOpenStructure(IElement left, IElement right)
    {
        var leftOpenSites = Relation.OpenSiteCount(left);
        var rightOpenSites = Relation.OpenSiteCount(right);

        if (leftOpenSites == 0 && rightOpenSites == 0)
        {
            return false;
        }

        if (leftOpenSites > 0 && rightOpenSites == 0 && !Relation.IsOpenSite(left))
        {
            return false;
        }

        if (rightOpenSites > 0 && leftOpenSites == 0 && !Relation.IsOpenSite(right))
        {
            return false;
        }

        return true;
    }

    private static IElement ReduceClosedStructure(IElement left, IElement right, IElement numerator, IElement denominator)
    {
        var leftOpenSites = Relation.OpenSiteCount(left);
        var rightOpenSites = Relation.OpenSiteCount(right);

        if (leftOpenSites > 0 || rightOpenSites > 0)
        {
            return Relation.CombineStructural(left, right);
        }

        return new Relation(numerator, denominator);
    }

    private static IEnumerable<PartitionStep> PartitionsForReduced(IElement reduced, IElement numerator, IElement denominator)
    {
        if (Relation.OpenSiteCount(reduced) == 0)
        {
            return PartitionsOf(RawOf(numerator), RawOf(denominator));
        }

        return PartitionsFor(reduced);
    }

    private static IEnumerable<PartitionStep> PartitionsForRelation(Relation relation)
    {
        var leftOpenSites = Relation.OpenSiteCount(relation.Left);
        var rightOpenSites = Relation.OpenSiteCount(relation.Right);

        if (leftOpenSites == 0 && rightOpenSites == 0)
        {
            var folded = Fold(relation.Left, relation.Right);
            return PartitionsOf(folded.Numerator, folded.ResolvedDenominator);
        }

        if (leftOpenSites > 0 && rightOpenSites == 0)
        {
            return PartitionsFor(relation.Right);
        }

        if (rightOpenSites > 0 && leftOpenSites == 0)
        {
            return PartitionsFor(relation.Left);
        }

        return PartitionsFor(relation.Left).Concat(PartitionsFor(relation.Right));
    }

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
