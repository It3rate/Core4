using System.Globalization;

namespace Core4.Engine;

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
