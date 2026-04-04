namespace Core4.Engine;

public readonly record struct Site(Placement Placement, SiteField Field)
{
    public static Site Open(Carrier carrier, Measure location) => new(carrier.At(location), SiteField.Open);

    public static Site Open(Placement placement) => new(placement, SiteField.Open);

    public static Site Tension(Carrier carrier, Measure location, Measure energy) => new(
        carrier.At(location),
        SiteField.Open.FocusAlignedDominant(energy));

    public Carrier Carrier => Placement.PrimaryCarrier;

    public Ratio Position => Placement.Ratio;

    public Ratio State => Field.AlignedDominant;

    public Measure Potential => Field.Potential;

    public bool IsOpen => Field.OccupiedCount == Measure.Zero;

    public bool IsTension => Field.BoundCount == Measure.Zero && Potential != Measure.Zero;

    public Ratio On(Carrier carrier) => Placement.On(carrier);

    public Measure Location => On(Carrier).Whole;

    public Site Place(Placement placement) => new(placement, Field);

    public Site Focus(Measure energy) => new(Placement, Field.FocusAlignedDominant(energy));

    public Site Resolve(Ratio state) => new(Placement, Field.Replace(1, state));

    public Site Resolve(Measure slot, Ratio state) => new(Placement, Field.Replace(slot, state));

    public Site Load(Measure slot, Ratio state) => new(Placement, Field.Add(slot, state));

    public Relation Relate(Site anchor) => new(Carrier, this, anchor);

    public Relation Relate(Carrier carrier, Site anchor) => new(carrier, this, anchor);
}
