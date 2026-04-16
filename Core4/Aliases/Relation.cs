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

    public ISupportProjection ProjectRead(Site site) => Structure.ProjectRead(site);

    // TODO(core): Delete this compatibility alias once callers consume ProjectRead directly.
    public Ratio Project(Site site) => ProjectRead(site).AsRatio();

    public ISupportProjection ProjectRead(Measure measure) => Structure.ProjectRead(measure);

    // TODO(core): Delete this compatibility alias once callers consume ProjectRead directly.
    public Ratio Project(Measure measure) => ProjectRead(measure).AsRatio();

    public Placement Place(ISupportProjection projection) => new(this, projection);

    // TODO(core): Delete this compatibility overload once callers pass support projections directly.
    public Placement Place(Ratio ratio) => Place((ISupportProjection)ratio);

    public ISupportProjection LocateRead(ISupportProjection position, Carrier carrier) => Structure.LocateRead(position, carrier);

    // TODO(core): Delete this compatibility alias once callers consume LocateRead directly.
    public Ratio Locate(ISupportProjection position, Carrier carrier) => LocateRead(position, carrier).AsRatio();

    // TODO(core): Delete this compatibility overload once callers pass support projections directly.
    public Ratio Locate(Ratio position, Carrier carrier) => Locate((ISupportProjection)position, carrier);

    public Basis Freeze() => Basis.Freeze(this);
}
