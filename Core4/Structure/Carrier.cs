using System.Globalization;
using Core4.Numerics;

namespace Core4.Structure;

/// <summary>
/// A primitive support: a directly traversable axis that can host exact placements and frozen relations.
/// Example: <c>var x = new Carrier(1); var point = x.At(10);</c>
/// </summary>
public readonly record struct Carrier(long Id) : ISupport
{
    public Carrier PrimaryCarrier => this;

    public Measure SupportGrade => Measure.Zero;

    public static implicit operator Carrier(long id) => new(id);

    public Placement At(Measure location) => Placement.Exact(this, location);

    public Placement At(Ratio ratio) => new(this, ratio.Normalize());

    public Ratio Locate(Ratio position, Carrier carrier) => carrier == this
        ? position.Normalize()
        : Ratio.Tension(position.Potential);

    public override string ToString() => Id.ToString(CultureInfo.InvariantCulture);
}
