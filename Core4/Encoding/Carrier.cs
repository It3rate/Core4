using System.Globalization;

namespace Core4.Encoding;

/// <summary>
/// One concrete ordered carrier instance inside some semantic domain. A carrier is identity/order substrate, not the
/// owner of all local structure on that line: many local supports and many sample observations may coexist on the same
/// carrier without one universal ruler already being chosen.
/// Example: <c>var x = new Carrier(1); var point = x.At(10);</c>
/// </summary>
public readonly record struct Carrier(long Id, CarrierDomain Domain) : ISupport, IElement
{
    public Carrier(long id) : this(id, new CarrierDomain(id))
    {
    }

    public Carrier DefaultCarrier => this;

    // TODO(core): Delete this compatibility alias once callers stop expecting supports to expose a single primary
    // carrier. The real carrier support surface is DefaultCarrier.
    public Carrier PrimaryCarrier => DefaultCarrier;

    public Measure SupportGrade => Measure.Zero;

    public static implicit operator Carrier(long id) => new(id);

    public Placement At(Measure location) => Placement.Exact(this, location);

    public Placement At(Ratio ratio) => new(this, ratio.Normalize());

    public Placement Place(ISupportProjection projection) => new(this, projection);

    // TODO(core): Delete this compatibility overload once callers pass support projections directly.
    public Placement Place(Ratio ratio) => Place((ISupportProjection)ratio);

    public ISupportProjection LocateRead(ISupportProjection position, Carrier carrier) => new SupportProjectionRead(
        carrier == this
            ? position.AsRatio().Normalize()
            : Ratio.Tension(position.Potential));

    // TODO(core): Delete this compatibility alias once callers consume LocateRead directly.
    public Ratio Locate(ISupportProjection position, Carrier carrier) => LocateRead(position, carrier).AsRatio();

    // TODO(core): Delete this compatibility overload once callers pass support projections directly.
    public Ratio Locate(Ratio position, Carrier carrier) => Locate((ISupportProjection)position, carrier);

    public Carrier InDomain(CarrierDomain domain) => new(Id, domain);

    public override string ToString() => Id.ToString(CultureInfo.InvariantCulture);
}

