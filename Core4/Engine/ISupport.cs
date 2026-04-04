namespace Core4.Engine;

/// <summary>
/// Anything traversable by ratio. A carrier is the simplest support, a basis is a frozen support, and larger linked
/// structures can implement the same contract later without changing placement logic.
/// Example: <c>Placement p = support.Place(new Ratio(3, 10));</c>
/// </summary>
public interface ISupport
{
    Carrier PrimaryCarrier { get; }

    Measure SupportGrade { get; }

    Ratio Locate(Ratio position, Carrier carrier);

    Placement Place(Ratio ratio) => new(this, ratio.Normalize());
}
