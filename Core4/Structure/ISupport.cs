using Core4.Numerics;

namespace Core4.Structure;

/// <summary>
/// Anything traversable by committed structure. A carrier is the simplest support, a basis is a reusable frozen
/// support, and larger linked structures can implement the same contract later without changing placement logic.
/// Example: <c>Placement p = support.Place(new Ratio(3, 10));</c>
/// </summary>
public interface ISupport
{
    Carrier PrimaryCarrier { get; }

    Measure SupportGrade { get; }

    Ratio Locate(Ratio position, Carrier carrier);

    Ratio Locate(ISupportProjection position, Carrier carrier) => Locate(position.AsRatio(), carrier);

    Placement Place(Ratio ratio) => new(this, ratio.Normalize());

    Placement Place(ISupportProjection projection) => new(this, projection.AsRatio());
}
