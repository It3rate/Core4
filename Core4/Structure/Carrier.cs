using System.Globalization;
using Core4.Numerics;

namespace Core4.Structure;

/// <summary>
/// A primitive support: one concrete ordered carrier instance inside some semantic domain. Different carrier instances
/// may belong to the same domain without already agreeing on origin, scale, or calibration.
/// Example: <c>var x = new Carrier(1); var point = x.At(10);</c>
/// </summary>
public readonly record struct Carrier(long Id, CarrierDomain Domain) : ISupport
{
    public Carrier(long id) : this(id, new CarrierDomain(id))
    {
    }

    public Carrier PrimaryCarrier => this;

    public Measure SupportGrade => Measure.Zero;

    public static implicit operator Carrier(long id) => new(id);

    internal SupportStructure Structure => SupportStructure.ForCarrier(this);

    public Placement At(Measure location) => Placement.Exact(this, location);

    public Placement At(Ratio ratio) => new(this, ratio.Normalize());

    public Placement Place(Ratio ratio) => At(ratio);

    public Placement Place(ISupportProjection projection) => At(projection.AsRatio());

    public Ratio Locate(Ratio position, Carrier carrier) => Structure.Locate(position, carrier);

    public Ratio Locate(ISupportProjection position, Carrier carrier) => Locate(position.AsRatio(), carrier);

    public Carrier InDomain(CarrierDomain domain) => new(Id, domain);

    public override string ToString() => Id.ToString(CultureInfo.InvariantCulture);
}
