namespace Core4.Experimental;

/// <summary>
/// Minimal experimental measure. It only wraps a raw long plus an optional explicit polarity.
/// </summary>
public readonly record struct Measure(long Raw, long PolarityRaw = 0) : IElement
{
    public static Measure Zero => new(0);
    public static Measure One => new(1);

    public Measure Polarity => new(PolarityRaw);


    public static implicit operator Measure(long raw) => new(raw);
    public static implicit operator long(Measure measure) => measure.Raw;

    public override string ToString() => Raw.ToString();
}
