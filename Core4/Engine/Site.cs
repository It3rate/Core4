namespace Core4.Engine;

public readonly record struct Site(Carrier Carrier, Measure Location, Ratio State)
{
    public static Site Open(Carrier carrier, Measure location) => new(carrier, location, Ratio.Zero(Measure.Zero));

    public static Site Tension(Carrier carrier, Measure location, Measure energy) => new(carrier, location, Ratio.Tension(energy));

    public Measure Potential => State.Recessive;

    public bool IsOpen => State.Recessive == Measure.Zero && State.Dominant == Measure.Zero;

    public bool IsTension => State.IsTension && State.Recessive.Raw != 0;

    public Site Focus(Measure energy) => new(Carrier, Location, Ratio.Tension(energy));

    public Site Resolve(Ratio state) => new(Carrier, Location, state.Normalize());

    public Relation Relate(Site anchor) => new(this, anchor);
}
