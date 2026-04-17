using Core4.Encoding;

namespace Core4.Experimental;

/// <summary>
/// Temporary enum-backed numeric code for experimental signature axes. This keeps the signature physically numeric
/// while letting us adjust the small categorical ladders without hard-coding raw measures everywhere.
/// </summary>
public readonly record struct RelationCode(Measure Value)
{
    public static RelationCode Of(RelationPolarity value) => new(new((long)value));
    public static RelationCode Of(RelationMergeScope value) => new(new((long)value));
    public static RelationCode Of(RelationNovelty value) => new(new((long)value));

    public static implicit operator Measure(RelationCode code) => code.Value;
}

/// <summary>
/// Ordered participation pair for one experimental axis. Direction stays explicit instead of being packed into a
/// single derived code so later folds can choose their own traversal order.
/// </summary>
public readonly record struct OrderedRelationCode(RelationCode Base, RelationCode Attached);

/// <summary>
/// Minimal primitive signature for one experimental relation. Named readings like unit, segment, area, or complex
/// should be derived from these axes rather than stored here directly.
/// </summary>
public readonly record struct RelationSignature(
    OrderedRelationCode Polarity,
    OrderedRelationCode MergeScope,
    RelationCode CarrierRelation,
    RelationCode DomainRelation,
    Measure Multiplicity);

public enum RelationPolarity : long
{
    Recessive = -1,
    Unknown = 0,
    Dominant = 1,
}

public enum RelationMergeScope : long
{
    Node = -1,
    Unknown = 0,
    Whole = 1,
}

public enum RelationNovelty : long
{
    Existing = -1,
    Unknown = 0,
    New = 1
}
