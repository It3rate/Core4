namespace Core4.Engine;

public readonly record struct Placement(ISupport Support, Ratio Ratio)
{
    public static Placement Exact(Carrier carrier, Measure location) => new(
        carrier,
        new Ratio(location, Measure.One));

    public Carrier PrimaryCarrier => Support.PrimaryCarrier;

    public Measure SupportGrade => Support.SupportGrade;

    public Measure Potential => Ratio.Potential;

    public Ratio On(Carrier carrier) => Support.Locate(Ratio.Normalize(), carrier).Normalize();

    public Placement Resolve(Ratio ratio) => new(Support, ratio.Normalize());
}
