namespace Core4.Experimental;

/// <summary>
/// Minimal experimental measure. A bare measure is polarity-neutral until a relation gives it calibration.
/// </summary>
public readonly record struct Measure(long Raw) : IElement
{
    public static Measure Zero => new(0);
    public static Measure One => new(1);

    public Measure Polarity => Zero;

    public Measure Sign => Raw switch
    {
        > 0 => One,
        < 0 => -One,
        _ => Zero
    };

    public static Measure operator +(Measure left, Measure right) => new(left.Raw + right.Raw);
    public static Measure operator -(Measure left, Measure right) => new(left.Raw - right.Raw);
    public static Measure operator *(Measure left, Measure right) => new(left.Raw * right.Raw);
    public static Measure operator -(Measure value) => new(-value.Raw);

    public static implicit operator Measure(long raw) => new(raw);
    public static implicit operator long(Measure measure) => measure.Raw;

    public override string ToString() => Raw.ToString();
}
