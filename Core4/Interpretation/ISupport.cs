
namespace Core4.Interpretation;

/// <summary>
/// Anything traversable by committed structure. A carrier is the simplest support, a basis is a reusable frozen
/// support, and larger linked structures can implement the same contract later without changing placement logic.
/// Example: <c>Placement p = support.Place(new Ratio(3, 10));</c>
/// </summary>
public interface ISupport
{
    Carrier PrimaryCarrier { get; }

    Measure SupportGrade { get; }

    ISupportProjection LocateRead(ISupportProjection position, Carrier carrier);

    // TODO(core): Delete this compatibility alias once callers consume LocateRead directly.
    Ratio Locate(ISupportProjection position, Carrier carrier) => LocateRead(position, carrier).AsRatio();

    // TODO(core): Delete this compatibility overload once callers pass support projections directly.
    Ratio Locate(Ratio position, Carrier carrier) => LocateRead(position, carrier).AsRatio();

    Placement Place(ISupportProjection projection) => new(this, projection);

    // TODO(core): Delete this compatibility overload once callers pass support projections directly.
    Placement Place(Ratio ratio) => Place((ISupportProjection)ratio);
}

