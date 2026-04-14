using Core4.Numerics;

namespace Core4.Structure;

/// <summary>
/// A live ordered coupling between two sites on a chosen carrier. Freezing a relation produces the first reusable
/// basis, while leaving it live keeps it available for negotiation or future dynamic motion.
/// Example: <c>var relation = origin.Relate(anchor);</c>
/// </summary>
public readonly record struct Relation(Carrier Carrier, Site Origin, Site Anchor) : ISupport
{
    public Relation(Site origin, Site anchor) : this(origin.Carrier, origin, anchor)
    {
    }

    public Carrier PrimaryCarrier => Carrier;

    public Measure SupportGrade => Measure.Zero;

    public Ratio OriginPosition => Origin.On(Carrier);

    public Ratio AnchorPosition => Anchor.On(Carrier);

    public Ratio Span => AnchorPosition.Subtract(OriginPosition).Normalize();

    public Measure Delta => Span.Recessive;

    public Measure Orientation => Span.Recessive.Sign;

    public Measure Potential => Span.Potential;

    public Ratio Stiffness => Origin.Stiffness.Weaker(Anchor.Stiffness);

    public Relation Flip() => new(Carrier, Anchor, Origin);

    public Relation Harden(Ratio stiffness) => new(Carrier, Origin.Harden(stiffness), Anchor.Harden(stiffness));

    public Ratio Project(Site site)
    {
        var sitePosition = site.On(Carrier);
        var delta = sitePosition.Subtract(OriginPosition);
        return delta.Divide(Span);
    }

    public Ratio Project(Measure measure) => Project(Site.Open(Carrier, measure));

    public Placement Place(Ratio ratio) => new(this, ratio.Normalize());

    public Ratio Locate(Ratio position, Carrier carrier) => carrier == Carrier
        ? OriginPosition.Add(Span.Multiply(position)).Normalize()
        : Ratio.Tension(position.Potential);

    public Basis Freeze() => Basis.Freeze(this);
}
