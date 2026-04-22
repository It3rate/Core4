namespace Core4.Experimental;

/// <summary>
/// Experimental structure-to-structure transform. The substrate stays one ordered relation, but each participation is
/// itself a full relation stage so fold and normalization can remain structural instead of collapsing into one
/// signature.
/// </summary>
public sealed record RelationFold : IElement
{
    public static MergeScope DefaultMergeScope => MergeScope.Whole;
    public static Measure DefaultCommitment => Relation.DefaultCommitment;

    public Relation Structure { get; }

    public Relation Input => StageAt(0);
    public Relation Output => StageAt(Structure.Entries.Length - 1);

    public RelationSignature InputSignature => Input.Signature;
    public RelationSignature OutputSignature => Output.Signature;

    public Measure StageCount => new(Structure.Entries.Length);
    public Measure Polarity => Output.Polarity;

    public RelationFold(
        Relation input,
        Relation output,
        MergeScope mergeScope = MergeScope.Whole,
        IElement? transform = null,
        IElement? aspect = null,
        Measure? commitment = null)
        : this(new Relation(input, mergeScope, aspect: aspect)
            .Add(output, mergeScope, transform: transform, commitment: commitment ?? DefaultCommitment))
    {
    }

    private RelationFold(Relation structure)
    {
        Validate(structure);
        Structure = structure;
    }

    public static RelationFold Identity(
        Relation structure,
        MergeScope mergeScope = MergeScope.Whole,
        IElement? aspect = null,
        Measure? commitment = null) =>
        new(structure, structure, mergeScope, aspect: aspect, commitment: commitment);

    public RelationFold Then(
        Relation output,
        MergeScope mergeScope = MergeScope.Whole,
        IElement? transform = null,
        IElement? aspect = null,
        Measure? commitment = null) =>
        new(Structure.Add(output, mergeScope, transform, aspect, commitment ?? DefaultCommitment));

    private Relation StageAt(int index)
    {
        var attached = Structure.Entries[index].Attached;
        return attached as Relation
            ?? throw new InvalidOperationException("Experimental fold stages must remain relation structures.");
    }

    private static void Validate(Relation structure)
    {
        if (structure.Entries.Length < 2)
        {
            throw new ArgumentException(
                "Experimental fold requires at least an input relation and an output relation.",
                nameof(structure));
        }

        foreach (var entry in structure.Entries)
        {
            if (entry.Attached is not Relation)
            {
                throw new ArgumentException(
                    "Experimental fold stages must attach full relation structures.",
                    nameof(structure));
            }
        }
    }
}
