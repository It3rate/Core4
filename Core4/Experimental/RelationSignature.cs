using System.Collections.Immutable;

namespace Core4.Experimental;

/// <summary>
/// Per-element participation tuple calculated as the relation is constructed. The attached element contributes its own
/// polarity, requested merge scope, carrier/domain novelty relative to the elements already present, and multiplicity.
/// </summary>
public readonly record struct RelationSignatureEntry(
    Measure Polarity,
    Measure MergeScope,
    Measure CarrierNovelty,
    Measure DomainNovelty,
    Measure Multiplicity);

/// <summary>
/// Signature over the full ordered relation chain. One entry exists for each element added to the relation.
/// </summary>
public readonly record struct RelationSignature(ImmutableArray<RelationSignatureEntry> Entries);

public enum MergeScope : long
{
    Node = -1,
    Whole = 1
}

public enum Novelty : long
{
    Existing = -1,
    New = 1
}
