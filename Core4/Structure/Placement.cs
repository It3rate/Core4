using Core4.Numerics;

namespace Core4.Structure;

/// <summary>
/// A location expressed as support plus a support-derived projection. The current compatibility projection is still
/// <see cref="Ratio"/>, but the support-first direction is that placements store "how this support is being read"
/// rather than assuming ratio is the only possible projection forever.
/// Example: <c>var p = basis.Place(new Ratio(3, 10));</c>
/// </summary>
public readonly record struct Placement(ISupport Support, Ratio Projection)
{
    public static Placement Exact(Carrier carrier, Measure location) => new(
        carrier,
        new Ratio(location, Measure.One));

    public Carrier PrimaryCarrier => Support.PrimaryCarrier;

    public Measure SupportGrade => Support.SupportGrade;

    // Compatibility alias while callers are moved toward projection-first naming.
    public Ratio Ratio => Projection;

    public Measure Potential => Projection.Potential;

    public Ratio On(Carrier carrier) => Support.Locate((ISupportProjection)Projection.Normalize(), carrier).Normalize();

    public Placement Resolve(Ratio projection) => Resolve((ISupportProjection)projection);

    public Placement Resolve(ISupportProjection projection) => new(Support, projection.AsRatio());
}
