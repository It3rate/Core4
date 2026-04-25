namespace Core4.Kernel.Core;

public sealed class Relation : IElement
{
    public Relation(IElement left, IElement right)
    {
        Left = left;
        Right = right;
    }

    public static Relation Open() => new(Measure.Fixed(0), Measure.Fixed(0));

    public static IElement Claim(IElement value) => new Relation(value, Open());

    public static IElement ClaimOpposed(IElement value) => new Relation(Open(), value);

    public static IElement Compose(IElement left, IElement right) => CombineStructural(left, right);

    public static IElement Apply(IElement transform, IElement input) => CombineStructural(transform, input);

    public static IElement Oppose(IElement element) => element switch
    {
        Relation relation => new Relation(relation.Right, relation.Left),
        RelationFold fold => new RelationFold(fold.Right, fold.Left),
        _ => element
    };

    public static IElement Invert(IElement element) => element switch
    {
        RelationFold fold => new RelationFold(fold.Left, Oppose(fold.Right)),
        Relation relation when IsOpenSite(relation.Right) && relation.Left is Measure => ClaimOpposed(relation.Left),
        Relation relation when IsOpenSite(relation.Left) && relation.Right is Measure => Claim(relation.Right),
        Relation relation when relation.Left is Measure && relation.Right is Measure => Oppose(relation),
        Relation relation => new Relation(relation.Left, Oppose(relation.Right)),
        _ => element
    };

    public static int OpenSiteCount(IElement element) => CountOpenSites(element);

    public IElement Left { get; }
    public IElement Right { get; }

    public long PartitionCount => new RelationFold(Left, Right).PartitionCount;

    public IEnumerable<PartitionStep> Partitions() => new RelationFold(Left, Right).Partitions();

    public override string ToString() => $"({Left} -> {Right})";

    internal static IElement CombineStructural(IElement left, IElement right)
    {
        var leftOpenSites = CountOpenSites(left);
        var rightOpenSites = CountOpenSites(right);

        if (leftOpenSites > 0 && rightOpenSites == 0 && TryFillFirstOpenSite(left, right, out var leftApplied))
        {
            return leftApplied;
        }

        if (rightOpenSites > 0 && leftOpenSites == 0 && TryFillFirstOpenSite(right, left, out var rightApplied))
        {
            return rightApplied;
        }

        return new Relation(left, right);
    }

    private static int CountOpenSites(IElement element) => element switch
    {
        Relation relation when IsOpenSite(relation) => 1,
        Relation relation => CountOpenSites(relation.Left) + CountOpenSites(relation.Right),
        RelationFold fold => CountOpenSites(fold.Reduced),
        _ => 0
    };

    private static bool TryFillFirstOpenSite(IElement structure, IElement input, out IElement applied)
    {
        switch (structure)
        {
            case Relation relation when IsOpenSite(relation):
                applied = input;
                return true;

            case Relation relation when TryFillFirstOpenSite(relation.Left, input, out var newLeft):
                applied = new Relation(newLeft, relation.Right);
                return true;

            case Relation relation when TryFillFirstOpenSite(relation.Right, input, out var newRight):
                applied = new Relation(relation.Left, newRight);
                return true;

            case RelationFold fold when TryFillFirstOpenSite(fold.Left, input, out var newFoldLeft):
                applied = new RelationFold(newFoldLeft, fold.Right);
                return true;

            case RelationFold fold when TryFillFirstOpenSite(fold.Right, input, out var newFoldRight):
                applied = new RelationFold(fold.Left, newFoldRight);
                return true;

            default:
                applied = structure;
                return false;
        }
    }

    internal static bool IsOpenSite(IElement element) =>
        element is Relation relation && IsOpenSite(relation);

    private static bool IsOpenSite(Relation relation) =>
        IsZeroMeasure(relation.Left) && IsZeroMeasure(relation.Right);

    private static bool IsZeroMeasure(IElement element) =>
        element is Measure measure && measure.Raw == 0;
}
