using System.Collections.Immutable;

namespace Core4.Experimental;

/// <summary>
/// Minimal experimental relation chain. It starts from one attached element and grows by adding more attached
/// elements. Signature entries are calculated immediately so the raw participation pattern is available without a
/// separate inspection layer. Multiplicity is treated as construction-time expansion rather than stored final
/// structure, while optional transforms stay explicit on each attached participation.
/// </summary>
public sealed record Relation : IElement
{
    public static MergeScope DefaultMergeScope => MergeScope.Node;
    public static Measure DefaultCommitment => Measure.One;
    public static Measure DefaultMultiplicity => Measure.One;

    public ImmutableArray<RelationEntry> Entries { get; }
    public RelationSignature Signature { get; }
    internal ImmutableHashSet<Carrier> Carriers { get; }
    internal ImmutableHashSet<long> Domains { get; }

    public Measure Polarity =>
        TryGetUnitDelta(out var delta)
            ? delta.Sign
            : Entries.IsDefaultOrEmpty
                ? Measure.Zero
                : AppliedPolarityOf(Entries[^1].Attached, Entries[^1].Transform);

    public Relation(IElement attached, MergeScope mergeScope)
        : this(BuildState(attached, mergeScope, DefaultCommitment, DefaultMultiplicity))
    {
    }

    public Relation(IElement @base, params IElement[] attached)
        : this(BuildState(@base, attached))
    {
    }

    private Relation(RelationState state)
        : this(state.Entries, state.Signature, state.Carriers, state.Domains)
    {
    }

    private Relation(
        ImmutableArray<RelationEntry> entries,
        RelationSignature signature,
        ImmutableHashSet<Carrier> carriers,
        ImmutableHashSet<long> domains)
    {
        Entries = entries;
        Signature = signature;
        Carriers = carriers;
        Domains = domains;
    }

    public Relation Add(
        IElement attached,
        MergeScope mergeScope,
        IElement? transform = null,
        Measure? commitment = null,
        Measure? multiplicity = null)
    {
        var nextCommitment = commitment ?? DefaultCommitment;
        var nextMultiplicity = multiplicity ?? DefaultMultiplicity;
        if (nextMultiplicity.Raw <= 0)
        {
            return this;
        }

        var relation = this;
        for (long repeat = 0; repeat < nextMultiplicity.Raw; repeat++)
        {
            relation = relation.Append(attached, mergeScope, transform, nextCommitment);
        }

        return relation;
    }

    private static RelationState BuildState(
        IElement attached,
        MergeScope mergeScope,
        Measure commitment,
        Measure multiplicity)
    {
        var relation = Empty();
        var built = relation.Add(attached, mergeScope, commitment: commitment, multiplicity: multiplicity);
        return built.State();
    }

    private static RelationState BuildState(IElement @base, params IElement[] attached)
    {
        var relation = new Relation(BuildState(@base, DefaultMergeScope, DefaultCommitment, DefaultMultiplicity));

        foreach (var item in attached)
        {
            relation = relation.Add(item, DefaultMergeScope);
        }

        return relation.State();
    }

    private static Relation Empty() => new(
        ImmutableArray<RelationEntry>.Empty,
        new(ImmutableArray<RelationSignatureEntry>.Empty),
        ImmutableHashSet<Carrier>.Empty,
        ImmutableHashSet<long>.Empty);

    private RelationState State() => new(Entries, Signature, Carriers, Domains);

    private Relation Append(
        IElement attached,
        MergeScope mergeScope,
        IElement? transform,
        Measure commitment)
    {
        var nextEntry = new RelationEntry(attached, mergeScope, commitment, transform);
        var elementCarriers = CarrierContextOf(attached, transform);
        var carrierNovelty = NoveltyOfCarrierSet(elementCarriers, Carriers);
        var domainNovelty = NoveltyOfDomainSet(elementCarriers, Domains);
        var nextSignatureEntry = new RelationSignatureEntry(
            AppliedPolarityOf(attached, transform),
            TransformValueOf(transform),
            new((long)mergeScope),
            new((long)carrierNovelty),
            new((long)domainNovelty));

        return new(
            Entries.Add(nextEntry),
            new(Signature.Entries.Add(nextSignatureEntry)),
            Carriers.Union(elementCarriers),
            Domains.Union(elementCarriers.Select(carrier => carrier.Domain)));
    }

    private bool TryGetUnitDelta(out Measure delta)
    {
        if (Carriers.IsEmpty)
        {
            delta = Measure.Zero;
            return false;
        }

        Measure? previous = null;
        foreach (var entry in Entries)
        {
            if (entry.Attached is Measure measure)
            {
                if (previous is { } prior)
                {
                    delta = measure - prior;
                    return delta != Measure.Zero;
                }

                previous = measure;
            }
        }

        delta = Measure.Zero;
        return false;
    }

    private static ImmutableHashSet<Carrier> CarrierSetOf(IElement element) => element switch
    {
        Carrier carrier => ImmutableHashSet.Create(carrier),
        Relation relation => relation.Carriers,
        _ => ImmutableHashSet<Carrier>.Empty
    };

    private static ImmutableHashSet<Carrier> CarrierContextOf(IElement attached, IElement? transform)
    {
        var carriers = CarrierSetOf(attached);
        if (transform is null)
        {
            return carriers;
        }

        return carriers.Union(CarrierSetOf(transform));
    }

    private static Measure AppliedPolarityOf(IElement attached, IElement? transform)
    {
        var polarity = attached.Polarity;
        if (polarity == Measure.Zero || transform is null)
        {
            return polarity;
        }

        var transformPolarity = TransformValueOf(transform).Sign;
        return transformPolarity == Measure.Zero ? polarity : polarity * transformPolarity;
    }

    private static Measure TransformValueOf(IElement? transform) => transform switch
    {
        null => Measure.Zero,
        Measure measure => measure,
        _ => transform.Polarity
    };

    private static Novelty NoveltyOfCarrierSet(
        ImmutableHashSet<Carrier> elementCarriers,
        ImmutableHashSet<Carrier> seenCarriers)
    {
        if (seenCarriers.IsEmpty)
        {
            return Novelty.New;
        }

        foreach (var carrier in elementCarriers)
        {
            if (!seenCarriers.Contains(carrier))
            {
                return Novelty.New;
            }
        }

        return Novelty.Existing;
    }

    private static Novelty NoveltyOfDomainSet(
        ImmutableHashSet<Carrier> elementCarriers,
        ImmutableHashSet<long> seenDomains)
    {
        if (seenDomains.IsEmpty)
        {
            return Novelty.New;
        }

        foreach (var carrier in elementCarriers)
        {
            if (!seenDomains.Contains(carrier.Domain))
            {
                return Novelty.New;
            }
        }

        return Novelty.Existing;
    }
}

public readonly record struct RelationEntry(
    IElement Attached,
    MergeScope MergeScope,
    Measure Commitment,
    IElement? Transform);

internal readonly record struct RelationState(
    ImmutableArray<RelationEntry> Entries,
    RelationSignature Signature,
    ImmutableHashSet<Carrier> Carriers,
    ImmutableHashSet<long> Domains);
