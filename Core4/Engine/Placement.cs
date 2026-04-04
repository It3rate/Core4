namespace Core4.Engine;

/// <summary>
/// A location expressed as support plus ratio. Exact positions are just ratios whose denominator is the support's
/// own unit, while more general positions can stay relative and dynamic.
/// Example: <c>var p = basis.Place(new Ratio(3, 10));</c>
/// </summary>
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
