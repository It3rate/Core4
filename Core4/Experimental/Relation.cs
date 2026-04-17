using Core4.Aliases;
using Core4.Encoding;

namespace Core4.Experimental;

/// <summary>
/// The current experimental primitive: one ordered relation from one element to another. The same structure can
/// express carrier-to-measure pinning, carrier-to-carrier alignment, sample attachment, or relation-to-relation
/// fusion because the endpoints are just <see cref="IElement"/> and this relation is itself an <see cref="IElement"/>.
/// </summary>
public readonly record struct Relation(
    IElement Base,
    IElement Attached,
    Ratio Commitment,
    Measure Multiplicity) : IElement
{
    public Relation(IElement @base, IElement attached)
        : this(@base, attached, Ratio.Zero(Measure.One), Measure.One)
    {
    }

    public Relation(IElement @base, IElement attached, Ratio commitment)
        : this(@base, attached, commitment, Measure.One)
    {
    }
}
