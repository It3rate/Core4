namespace Core4.Engine;

public readonly record struct Relation(Site Origin, Site Anchor)
{
    public Carrier Carrier => Origin.Carrier;

    public Measure Delta => Anchor.Location - Origin.Location;

    public Measure Orientation => Delta.Sign;

    public Measure Potential => Absolute(Delta);

    public Relation Flip() => new(Anchor, Origin);

    public Ratio Project(Site site) => site.Carrier == Carrier
        ? new Ratio(site.Location - Origin.Location, Delta).Normalize()
        : Ratio.Tension(site.Potential);

    public Ratio Project(Measure measure) => Project(Site.Open(Carrier, measure));

    public Basis Freeze() => Basis.Freeze(this);

    private static Measure Absolute(Measure value) => value.Raw < 0 ? -value : value;
}
