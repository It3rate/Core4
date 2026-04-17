using System.Globalization;

namespace Core4.Encoding;

/// <summary>
/// The primitive Core4 reading. All canonical numeric storage stays wrapped as a measure so the backing type
/// can change later without changing the engine vocabulary.
/// Example: <c>var heat = new Measure(10);</c>
/// </summary>
public readonly record struct Measure(long Raw) : IElement
{
    public static Measure Zero => new(0);

    public static Measure One => new(1);

    public Measure Sign => new(Raw < 0 ? -1L : Raw > 0 ? 1L : 0L);

    public Measure Absolute => Raw < 0 ? -this : this;

    public static Measure operator +(Measure left, Measure right) => new(left.Raw + right.Raw);

    public static Measure operator -(Measure left, Measure right) => new(left.Raw - right.Raw);

    public static Measure operator -(Measure value) => new(-value.Raw);

    public static implicit operator Measure(long raw) => new(raw);

    public static implicit operator long(Measure value) => value.Raw;

    public override string ToString() => Raw.ToString(CultureInfo.InvariantCulture);
}

