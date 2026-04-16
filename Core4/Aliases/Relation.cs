using Core4.Numerics;
using Core4.Structure;

namespace Core4.Aliases;

/// <summary>
/// A live ordered coupling between two sites on a chosen carrier. In the support-first reading this is the lowest
/// ergonomic alias for a same-carrier support with two pinned landmarks. Freezing a relation produces the first
/// reusable basis, while leaving it live keeps it available for negotiation or future dynamic motion.
/// Example: <c>var relation = origin.Relate(anchor);</c>
/// </summary>
public readonly record struct Relation(Carrier Carrier, Site Origin, Site Anchor) : ISupport
{
    public Relation(Site origin, Site anchor) : this(origin.Carrier, origin, anchor)
    {
    }

    public Carrier PrimaryCarrier => Carrier;

    public Measure SupportGrade => Measure.Zero;

    internal SupportStructure Structure => SupportStructure.FromRelation(this);

    public Ratio OriginPosition => Structure.OriginPosition;

    public Ratio AnchorPosition => Structure.AnchorPosition;

    public Ratio Span => Structure.Span;

    public Measure Delta => Structure.Delta;

    public Measure Orientation => Structure.Orientation;

    public Measure Potential => Structure.Potential;

    public Ratio Stiffness => Structure.Stiffness;

    public Relation Flip() => new(Carrier, Anchor, Origin);

    public Relation Harden(Ratio stiffness) => new(Carrier, Origin.Harden(stiffness), Anchor.Harden(stiffness));

    public Ratio Project(Site site) => Structure.Project(site);

    public Ratio Project(Measure measure) => Structure.Project(measure);

    public Placement Place(Ratio ratio) => new(this, ratio.Normalize());

    public Placement Place(ISupportProjection projection) => new(this, projection.AsRatio());

    public Ratio Locate(Ratio position, Carrier carrier) => Structure.Locate(position, carrier);

    public Ratio Locate(ISupportProjection position, Carrier carrier) => Locate(position.AsRatio(), carrier);

    public Basis Freeze() => Basis.Freeze(this);
}
