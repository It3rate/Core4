using Core4.Numerics;

namespace Core4.Structure;

/// <summary>
/// A location expressed as support plus a support-derived projection. The current compatibility projection is still
/// <see cref="Ratio"/>, but the support-first direction is that placements store "how this support is being read"
/// rather than assuming ratio is the only possible projection forever.
/// Example: <c>var p = basis.Place(new Ratio(3, 10));</c>
/// </summary>
public readonly record struct Placement(ISupport Support, ISupportProjection ProjectionRead)
{
    public static Placement Exact(Carrier carrier, Measure location) => new(
        carrier,
        new Ratio(location, Measure.One));

    public Carrier PrimaryCarrier => Support.PrimaryCarrier;

    public Measure SupportGrade => Support.SupportGrade;

    // TODO(core): Delete this compatibility alias once callers read ProjectionRead directly.
    public Ratio Projection => ProjectionRead.AsRatio();

    // TODO(core): Delete this compatibility alias once callers read Projection directly.
    public Ratio Ratio => Projection;

    public Measure Potential => ProjectionRead.Potential;

    public Ratio On(Carrier carrier) => Support.Locate(ProjectionRead, carrier).Normalize();

    // TODO(core): Delete this compatibility overload once callers resolve through support projections directly.
    public Placement Resolve(Ratio projection) => Resolve((ISupportProjection)projection);

    public Placement Resolve(ISupportProjection projection) => new(Support, projection);
}
