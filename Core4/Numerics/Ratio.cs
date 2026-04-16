using System.Globalization;
using System.Numerics;
using Core4.Structure;

namespace Core4.Aliases;

/// <summary>
/// An ordered support-local read stored as a pinned origin, a pinned unit location, and a current value location.
/// This remains the current machine-privileged read of a richer support-local structure. The older
/// recessive/dominant pair is preserved as a derived compatibility projection so the rest of the engine can keep
/// transitioning without leaving Core4-native structure. <c>m/0</c> carries unresolved tension, while <c>m/u</c> is a
/// resolved relation against a denominator. Denominator sign is preserved exactly; any fold into a positive
/// denominator is an explicit lossy operation.
/// Example: <c>var halfway = new Ratio(5, 10); var offsetHalfway = new Ratio(100, 105, 110);</c>
/// </summary>
public readonly record struct Ratio : ISupportProjection
{
    public Ratio(Measure recessive, Measure dominant)
        : this(Measure.Zero, recessive, dominant)
    {
    }

    public Ratio(Measure origin, Measure value, Measure unitValue)
    {
        Origin = origin;
        Value = value;
        UnitValue = unitValue;
    }

    public Measure Origin { get; init; }

    public Measure Value { get; init; }

    public Measure UnitValue { get; init; }

    public Measure Recessive => Difference(Value, Origin);

    public Measure Dominant => Difference(UnitValue, Origin);

    public Measure Numerator => Recessive;

    public Measure Denominator => Dominant;

    public Measure Potential => Recessive.Absolute;

    public static Ratio Zero(Measure dominant) => FromOffsets(Measure.Zero, Measure.Zero, dominant);

    public static Ratio Tension(Measure energy) => FromOffsets(Measure.Zero, energy, Measure.Zero);

    public bool IsResolved => Dominant.Raw != 0;

    public bool IsTension => !IsResolved;

    public Measure Sign
    {
        get
        {
            if (!IsResolved || Recessive.Raw == 0)
            {
                return Measure.Zero;
            }

            return Recessive.Sign == Dominant.Sign ? new Measure(1) : new Measure(-1);
        }
    }

    public Measure Whole => IsResolved ? new Measure(Recessive.Raw / Dominant.Raw) : Measure.Zero;

    public Measure Remainder => IsResolved ? new Measure(Recessive.Raw % Dominant.Raw) : Recessive;

    public double Approximate => IsResolved ? (double)Recessive.Raw / Dominant.Raw : 0d;

    public int CompareValue(Ratio other)
    {
        var left = FoldDenominator();
        var right = other.FoldDenominator();

        if (left.IsResolved && right.IsResolved)
        {
            var leftScaled = new BigInteger(left.Recessive.Raw) * right.Dominant.Raw;
            var rightScaled = new BigInteger(right.Recessive.Raw) * left.Dominant.Raw;
            return leftScaled.CompareTo(rightScaled);
        }

        return Recessive.Raw.CompareTo(other.Recessive.Raw);
    }

    public int CompareMagnitude(Ratio other)
    {
        if (IsResolved && other.IsResolved)
        {
            var leftScaled = BigInteger.Abs(new BigInteger(Recessive.Raw) * other.Dominant.Raw);
            var rightScaled = BigInteger.Abs(new BigInteger(other.Recessive.Raw) * Dominant.Raw);
            return leftScaled.CompareTo(rightScaled);
        }

        return Potential.Raw.CompareTo(other.Potential.Raw);
    }

    public Ratio Weaker(Ratio other) => CompareMagnitude(other) <= 0
        ? Normalize()
        : other.Normalize();

    public Ratio Stronger(Ratio other) => CompareMagnitude(other) >= 0
        ? Normalize()
        : other.Normalize();

    public Ratio ScaleBy(Ratio factor)
    {
        var normalizedFactor = factor.Normalize();

        if (normalizedFactor.IsTension)
        {
            return FromOffsets(Origin, Potential, Measure.Zero);
        }

        if (IsTension)
        {
            return FromOffsets(
                Origin,
                ToMeasure((new BigInteger(Recessive.Raw) * normalizedFactor.Recessive.Raw) / normalizedFactor.Dominant.Raw),
                Measure.Zero);
        }

        return FromBigInteger(
            new BigInteger(Recessive.Raw) * normalizedFactor.Recessive.Raw,
            new BigInteger(Dominant.Raw) * normalizedFactor.Dominant.Raw,
            Origin).Normalize();
    }

    public Ratio Add(Ratio other)
    {
        var origin = PreferredOriginOrZero(other);

        if (IsTension || other.IsTension)
        {
            return FromOffsets(origin, ToMeasure(new BigInteger(Recessive.Raw) + other.Recessive.Raw), Measure.Zero);
        }

        if (Dominant == other.Dominant)
        {
            return FromBigInteger(
                new BigInteger(Recessive.Raw) + other.Recessive.Raw,
                Dominant.Raw,
                origin);
        }

        return FromBigInteger(
            (new BigInteger(Recessive.Raw) * other.Dominant.Raw) + (new BigInteger(other.Recessive.Raw) * Dominant.Raw),
            new BigInteger(Dominant.Raw) * other.Dominant.Raw,
            origin);
    }

    public Ratio Subtract(Ratio other)
    {
        var origin = PreferredOriginOrZero(other);

        if (IsTension || other.IsTension)
        {
            return FromOffsets(origin, ToMeasure(new BigInteger(Recessive.Raw) - other.Recessive.Raw), Measure.Zero);
        }

        if (Dominant == other.Dominant)
        {
            return FromBigInteger(
                new BigInteger(Recessive.Raw) - other.Recessive.Raw,
                Dominant.Raw,
                origin);
        }

        return FromBigInteger(
            (new BigInteger(Recessive.Raw) * other.Dominant.Raw) - (new BigInteger(other.Recessive.Raw) * Dominant.Raw),
            new BigInteger(Dominant.Raw) * other.Dominant.Raw,
            origin);
    }

    public Ratio Multiply(Ratio other)
    {
        var origin = PreferredOriginOrZero(other);

        if (IsTension || other.IsTension)
        {
            return FromOffsets(origin, ToMeasure(new BigInteger(Recessive.Raw) * other.Recessive.Raw), Measure.Zero);
        }

        return FromBigInteger(
            new BigInteger(Recessive.Raw) * other.Recessive.Raw,
            new BigInteger(Dominant.Raw) * other.Dominant.Raw,
            origin);
    }

    public Ratio Divide(Ratio other)
    {
        var origin = PreferredOriginOrZero(other);

        if (IsTension || other.IsTension || other.Recessive == Measure.Zero)
        {
            return FromOffsets(origin, Potential, Measure.Zero);
        }

        return FromBigInteger(
            new BigInteger(Recessive.Raw) * other.Dominant.Raw,
            new BigInteger(Dominant.Raw) * other.Recessive.Raw,
            origin);
    }

    /// <summary>
    /// Reduces common magnitude without relocating sign between numerator and denominator.
    /// This keeps structural denominator orientation intact while still shrinking exact ratios.
    /// </summary>
    public Ratio Canonicalize()
    {
        if (IsTension)
        {
            return this;
        }

        if (Recessive.Raw == 0)
        {
            return FromOffsets(Origin, Measure.Zero, Dominant.Sign == Measure.Zero ? Measure.One : Dominant.Sign);
        }

        var numerator = Recessive.Absolute.Raw;
        var denominator = Dominant.Absolute.Raw;
        var gcd = GreatestCommonDivisor(numerator, denominator);

        if (gcd <= 1)
        {
            return this;
        }

        return FromOffsets(
            Origin,
            new Measure(Recessive.Raw / gcd),
            new Measure(Dominant.Raw / gcd));
    }

    /// <summary>
    /// Structural normalization is identity for ratios. Denominator sign is preserved exactly.
    /// Any positive-denominator conversion must be requested explicitly through <see cref="FoldDenominator"/>.
    /// </summary>
    public Ratio Normalize() => this;

    /// <summary>
    /// Lossy arithmetic fold into a positive denominator. Useful for projection or float-like comparison,
    /// but not for structural storage.
    /// </summary>
    public Ratio FoldDenominator() => IsTension || Dominant.Raw >= 0
        ? this
        : FromOffsets(Origin, -Recessive, -Dominant);

    public Ratio AsRatio() => Normalize();

    public override string ToString() => string.Create(
        CultureInfo.InvariantCulture,
        $"{Recessive.Raw}/{Dominant.Raw}");

    private Measure PreferredOriginOrZero(Ratio other)
    {
        if (Origin == other.Origin)
        {
            return Origin;
        }

        if (Origin != Measure.Zero && other.Origin == Measure.Zero)
        {
            return Origin;
        }

        if (Origin == Measure.Zero && other.Origin != Measure.Zero)
        {
            return other.Origin;
        }

        return Measure.Zero;
    }

    private static long GreatestCommonDivisor(long left, long right)
    {
        while (right != 0)
        {
            var next = left % right;
            left = right;
            right = next;
        }

        return left < 0 ? -left : left;
    }

    private static Ratio FromOffsets(Measure origin, Measure recessive, Measure dominant) => new(
        origin,
        AddOffset(origin, recessive),
        AddOffset(origin, dominant));

    private static Measure AddOffset(Measure origin, Measure offset) => ToMeasure(new BigInteger(origin.Raw) + offset.Raw);

    private static Measure Difference(Measure value, Measure origin) => ToMeasure(new BigInteger(value.Raw) - origin.Raw);

    private static Ratio FromBigInteger(BigInteger recessive, BigInteger dominant, Measure origin)
    {
        if (dominant.IsZero)
        {
            return FromOffsets(origin, ToMeasure(BigInteger.Abs(recessive)), Measure.Zero);
        }

        if (Fits(recessive) && Fits(dominant))
        {
            return FromOffsets(origin, new Measure((long)recessive), new Measure((long)dominant));
        }

        var gcd = BigInteger.GreatestCommonDivisor(BigInteger.Abs(recessive), BigInteger.Abs(dominant));
        if (gcd > BigInteger.One)
        {
            recessive /= gcd;
            dominant /= gcd;
        }

        if (Fits(recessive) && Fits(dominant))
        {
            return FromOffsets(origin, new Measure((long)recessive), new Measure((long)dominant));
        }

        throw new OverflowException("Ratio magnitude exceeded the current Measure range even after exact reduction.");
    }

    private static bool Fits(BigInteger value) => value >= long.MinValue && value <= long.MaxValue;

    private static Measure ToMeasure(BigInteger value)
    {
        if (!Fits(value))
        {
            throw new OverflowException("Ratio magnitude exceeded the current Measure range.");
        }

        return new Measure((long)value);
    }
}

