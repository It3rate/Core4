namespace Core4.Engine;

public interface ISupport
{
    Carrier PrimaryCarrier { get; }

    Measure SupportGrade { get; }

    Ratio Locate(Ratio position, Carrier carrier);

    Placement Place(Ratio ratio) => new(this, ratio.Normalize());
}
