using System.Collections.Immutable;

namespace Core4.Experimental;

/// <summary>
/// Minimal experimental relation chain. It starts from one attached element and grows by adding more attached
/// elements. Signature entries are calculated immediately so the raw participation pattern is available without a
/// separate inspection layer.
/// </summary>
public sealed record Relation : IElement
{
    public ImmutableArray<RelationEntry> Entries { get; }
    public RelationSignature Signature { get; }
    internal ImmutableHashSet<Carrier> Carriers { get; }
    internal ImmutableHashSet<long> Domains { get; }

    public Measure Polarity =>
        Entries.IsDefaultOrEmpty
            ? Measure.Zero
            : Entries[^1].Attached.Polarity;

    public Relation(IElement attached, MergeScope mergeScope)
        : this(BuildState(attached, mergeScope, Measure.Zero, Measure.One))
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
        Measure? commitment = null,
        Measure? multiplicity = null)
    {
        var nextCommitment = commitment ?? Measure.Zero;
        var nextMultiplicity = multiplicity ?? Measure.One;
        var nextEntry = new RelationEntry(attached, mergeScope, nextCommitment, nextMultiplicity);
        var elementCarriers = CarrierSetOf(attached);
        var carrierNovelty = NoveltyOfCarrierSet(elementCarriers, Carriers);
        var domainNovelty = NoveltyOfDomainSet(elementCarriers, Domains);
        var nextSignatureEntry = new RelationSignatureEntry(
            attached.Polarity,
            new((long)mergeScope),
            new((long)carrierNovelty),
            new((long)domainNovelty),
            nextMultiplicity);

        return new(
            Entries.Add(nextEntry),
            new(Signature.Entries.Add(nextSignatureEntry)),
            Carriers.Union(elementCarriers),
            Domains.Union(elementCarriers.Select(carrier => carrier.Domain)));
    }

    private static RelationState BuildState(
        IElement attached,
        MergeScope mergeScope,
        Measure commitment,
        Measure multiplicity)
    {
        var relation = Empty();
        var built = relation.Add(attached, mergeScope, commitment, multiplicity);
        return built.State();
    }

    private static RelationState BuildState(IElement @base, params IElement[] attached)
    {
        var relation = new Relation(BuildState(@base, MergeScope.Whole, Measure.Zero, Measure.One));

        foreach (var item in attached)
        {
            relation = relation.Add(item, MergeScope.Whole);
        }

        return relation.State();
    }

    private static Relation Empty() => new(
        ImmutableArray<RelationEntry>.Empty,
        new(ImmutableArray<RelationSignatureEntry>.Empty),
        ImmutableHashSet<Carrier>.Empty,
        ImmutableHashSet<long>.Empty);

    private RelationState State() => new(Entries, Signature, Carriers, Domains);

    private static ImmutableHashSet<Carrier> CarrierSetOf(IElement element) => element switch
    {
        Carrier carrier => ImmutableHashSet.Create(carrier),
        Relation relation => relation.Carriers,
        _ => ImmutableHashSet<Carrier>.Empty
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
    Measure Multiplicity);

internal readonly record struct RelationState(
    ImmutableArray<RelationEntry> Entries,
    RelationSignature Signature,
    ImmutableHashSet<Carrier> Carriers,
    ImmutableHashSet<long> Domains);
