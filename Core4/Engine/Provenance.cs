namespace Core4.Engine;

/// <summary>
/// Self/other weighting for a live flow. Provenance does not replace polarity; it overlays where the current
/// pressure comes from.
/// Example: <c>var incoming = Provenance.OtherDriven(5);</c>
/// </summary>
public readonly record struct Provenance(Measure Self, Measure Other)
{
    public static Provenance SelfDriven(Measure strength) => new(strength, Measure.Zero);

    public static Provenance OtherDriven(Measure strength) => new(Measure.Zero, strength);

    public static Provenance Shared(Measure strength) => new(strength, strength);

    public Measure Balance => Self - Other;

    public Measure Direction => Balance.Sign;

    public Measure Potential => Self.Absolute + Other.Absolute;
}
