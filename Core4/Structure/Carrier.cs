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

    public Placement Place(ISupportProjection projection) => new(this, projection);

    // TODO(core): Delete this compatibility overload once callers pass support projections directly.
    public Placement Place(Ratio ratio) => Place((ISupportProjection)ratio);

    public ISupportProjection LocateRead(ISupportProjection position, Carrier carrier) => Structure.LocateRead(position, carrier);

    // TODO(core): Delete this compatibility alias once callers consume LocateRead directly.
    public Ratio Locate(ISupportProjection position, Carrier carrier) => LocateRead(position, carrier).AsRatio();

    // TODO(core): Delete this compatibility overload once callers pass support projections directly.
    public Ratio Locate(Ratio position, Carrier carrier) => Locate((ISupportProjection)position, carrier);

    public Carrier InDomain(CarrierDomain domain) => new(Id, domain);

    public override string ToString() => Id.ToString(CultureInfo.InvariantCulture);
}
