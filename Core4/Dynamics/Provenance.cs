using Core4.Numerics;

namespace Core4.Dynamics;

/// <summary>
/// Self/other weighting encoded on a provenance axis. Positive settled values are self-driven, negative settled
/// values are other-driven, zero settled values are shared, and <c>m/0</c> carries unresolved provenance.
/// Example: <c>var incoming = Provenance.OtherDriven(5);</c>
/// </summary>
public readonly record struct Provenance(Ratio Axis)
{
    public static Provenance SelfDriven(Measure strength) => new(new Ratio(strength, Measure.One));

    public static Provenance OtherDriven(Measure strength) => new(new Ratio(-strength, Measure.One));

    public static Provenance Shared(Measure strength) => new(new Ratio(Measure.Zero, strength));

    public static Provenance Unknown(Measure strength) => new(Ratio.Tension(strength));

    public Measure Direction => Axis.IsTension ? Measure.Zero : Axis.Recessive.Sign;

    public Measure Potential => Axis.IsTension
        ? Axis.Potential
        : Axis.Recessive.Absolute != Measure.Zero
            ? Axis.Recessive.Absolute
            : Axis.Dominant.Absolute;

    public bool IsUnknown => Axis.IsTension;

    public bool IsSelf => Direction.Raw > 0;

    public bool IsOther => Direction.Raw < 0;

    public bool IsShared => !IsUnknown && Direction == Measure.Zero;

    public Measure Self => IsSelf || IsShared ? Potential : Measure.Zero;

    public Measure Other => IsOther || IsShared ? Potential : Measure.Zero;
}
