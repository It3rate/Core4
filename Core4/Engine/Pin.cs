namespace Core4.Engine;

public readonly record struct Pin(Carrier Carrier, Measure Origin, Measure Anchor)
{
    public Measure Delta => Anchor - Origin;

    public Measure Orientation => Delta.Sign;

    public Pin Flip() => new(Carrier, Anchor, Origin);

    public Ratio Project(Measure measure) => new(measure - Origin, Delta);

    public Basis Freeze() => Basis.Freeze(this);
}
