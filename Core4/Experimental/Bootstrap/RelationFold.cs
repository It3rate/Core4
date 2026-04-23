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
        foreach (var partition in FoldPartitionsFor(op0))
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
        var reducedLeft = ReduceElement(left);
        var reducedRight = ReduceElement(right);
        var leftOpenSites = Relation.OpenSiteCount(reducedLeft);
        var rightOpenSites = Relation.OpenSiteCount(reducedRight);

        if (leftOpenSites == 0 && rightOpenSites == 0)
        {
            return false;
        }

        if (leftOpenSites > 0 && rightOpenSites == 0 && !Relation.IsOpenSite(reducedLeft))
        {
            return false;
        }

        if (rightOpenSites > 0 && leftOpenSites == 0 && !Relation.IsOpenSite(reducedRight))
        {
            return false;
        }

        return true;
    }

    private static IElement ReduceClosedStructure(IElement left, IElement right, IElement numerator, IElement denominator)
    {
        var reducedLeft = ReduceElement(left);
        var reducedRight = ReduceElement(right);
        var leftOpenSites = Relation.OpenSiteCount(reducedLeft);
        var rightOpenSites = Relation.OpenSiteCount(reducedRight);

        if (leftOpenSites > 0 || rightOpenSites > 0)
        {
            return Relation.CombineStructural(reducedLeft, reducedRight);
        }

        if (TryBuildContributionMatrix(left, right, out var contributionMatrix))
        {
            return contributionMatrix;
        }

        if (ShouldPreserveClosedComposite(reducedLeft, reducedRight))
        {
            return new Relation(reducedLeft, reducedRight);
        }

        return new Relation(numerator, denominator);
    }

    private static bool IsComposite(IElement element) => element is Relation or RelationFold;

    private static IElement ReduceElement(IElement element) => element switch
    {
        RelationFold fold => fold,
        Relation relation => new RelationFold(relation.Left, relation.Right),
        _ => element
    };

    private static IEnumerable<PartitionStep> PartitionsForReduced(
        IElement reduced,
        IElement numerator,
        IElement denominator)
    {
        if (!ShouldTraverseReducedStructure(reduced))
        {
            return PartitionsOf(RawOf(numerator), RawOf(denominator));
        }

        return PartitionsForPreservedStructure(reduced);
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

    private static IEnumerable<PartitionStep> FoldPartitionsFor(IElement element) => element switch
    {
        Measure measure => PartitionsOf(measure.Raw, 0),
        RelationFold fold => PartitionsOf(RawOf(fold.Numerator), RawOf(fold.Denominator)),
        Relation relation => FoldPartitionsForRelation(relation),
        _ => Array.Empty<PartitionStep>()
    };

    private static IEnumerable<PartitionStep> PartitionsForPreservedStructure(IElement element) => element switch
    {
        Relation relation => PartitionsForPreservedStructure(relation.Left).Concat(PartitionsForPreservedStructure(relation.Right)),
        RelationFold fold when ShouldTraverseReducedStructure(fold.Reduced) => PartitionsForPreservedStructure(fold.Reduced),
        _ => PartitionsFor(element)
    };

    private static IEnumerable<PartitionStep> FoldPartitionsForRelation(Relation relation)
    {
        var folded = Fold(relation.Left, relation.Right);
        return PartitionsOf(folded.Numerator, folded.ResolvedDenominator);
    }

    private static bool TryBuildContributionMatrix(IElement left, IElement right, out IElement contributionMatrix)
    {
        if (TryGetResolvedBranchPair(left, out var leftA, out var leftB) &&
            TryGetResolvedBranchPair(right, out var rightA, out var rightB))
        {
            var aligned = new Relation(new RelationFold(leftA, rightA), new RelationFold(leftB, rightB));
            var orthogonal = new Relation(new RelationFold(leftA, rightB), new RelationFold(leftB, rightA));
            contributionMatrix = new Relation(aligned, orthogonal);
            return true;
        }

        contributionMatrix = Measure.Fixed(0);
        return false;
    }

    private static bool TryGetResolvedBranchPair(IElement element, out IElement branchA, out IElement branchB)
    {
        switch (element)
        {
            case Relation relation:
                return TryGetResolvedBranchPair(relation.Left, relation.Right, out branchA, out branchB);

            case RelationFold { Reduced: Relation relation }:
                return TryGetResolvedBranchPair(relation.Left, relation.Right, out branchA, out branchB);

            default:
                branchA = Measure.Fixed(0);
                branchB = Measure.Fixed(0);
                return false;
        }
    }

    private static bool TryGetResolvedBranchPair(
        IElement left,
        IElement right,
        out IElement branchA,
        out IElement branchB)
    {
        var normalizedLeft = ReduceElement(left);
        var normalizedRight = ReduceElement(right);

        if (IsResolvedContribution(normalizedLeft) && IsResolvedContribution(normalizedRight))
        {
            branchA = normalizedLeft;
            branchB = normalizedRight;
            return true;
        }

        branchA = Measure.Fixed(0);
        branchB = Measure.Fixed(0);
        return false;
    }

    private static bool ShouldTraverseReducedStructure(IElement reduced) =>
        Relation.OpenSiteCount(reduced) > 0 || HasCompositeChildren(reduced);

    private static bool ShouldPreserveClosedComposite(IElement reducedLeft, IElement reducedRight) =>
        IsResolvedComposite(reducedLeft) && IsResolvedComposite(reducedRight);

    private static bool HasCompositeChildren(IElement reduced) =>
        reduced is Relation relation && (IsComposite(relation.Left) || IsComposite(relation.Right));

    private static bool IsResolvedComposite(IElement element) =>
        element is RelationFold fold && RawOf(fold.Denominator) != 0;

    private static bool IsResolvedContribution(IElement element) => Read(element).ResolvedDenominator != 0;

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
