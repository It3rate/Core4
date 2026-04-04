namespace Core4.Engine;

public readonly record struct Relation(Carrier Carrier, Site Origin, Site Anchor)
{
    public Relation(Site origin, Site anchor) : this(origin.Carrier, origin, anchor)
    {
    }

    public Ratio OriginPosition => Origin.On(Carrier);

    public Ratio AnchorPosition => Anchor.On(Carrier);

    public Ratio Span => AnchorPosition.Subtract(OriginPosition).Normalize();

    public Measure Delta => Span.Recessive;

    public Measure Orientation => Span.Recessive.Sign;

    public Measure Potential => Span.Potential;

    public Relation Flip() => new(Carrier, Anchor, Origin);

    public Ratio Project(Site site)
    {
        var sitePosition = site.On(Carrier);
        var delta = sitePosition.Subtract(OriginPosition);
        return delta.Divide(Span);
    }

    public Ratio Project(Measure measure) => Project(Site.Open(Carrier, measure));

    public Ratio Locate(Ratio position, Carrier carrier) => carrier == Carrier
        ? OriginPosition.Add(Span.Multiply(position)).Normalize()
        : Ratio.Tension(position.Potential);

    public Basis Freeze() => Basis.Freeze(this);
}
