using System.Globalization;
using System.Numerics;

namespace Core4.Numerics;

/// <summary>
/// An ordered recessive/dominant relation. <c>m/0</c> carries unresolved tension, while <c>m/u</c> is a resolved
/// relation against a denominator. This is the shared numeric form behind placement, projection, and slot state.
/// Example: <c>var halfway = new Ratio(5, 10);</c>
/// </summary>
public readonly record struct Ratio(Measure Recessive, Measure Dominant)
{
    public Measure Numerator => Recessive;

    public Measure Denominator => Dominant;

    public Measure Potential => Recessive.Absolute;

    public static Ratio Zero(Measure dominant) => new(Measure.Zero, dominant);

    public static Ratio Tension(Measure energy) => new(energy, Measure.Zero);

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
            return Tension(Potential);
        }

        if (IsTension)
        {
            return Tension(ToMeasure((new BigInteger(Recessive.Raw) * normalizedFactor.Recessive.Raw) / normalizedFactor.Dominant.Raw));
        }

        return Multiply(normalizedFactor).Normalize();
    }

    public Ratio Add(Ratio other)
    {
        if (IsTension || other.IsTension)
        {
            return Tension(ToMeasure(new BigInteger(Recessive.Raw) + other.Recessive.Raw));
        }

        if (Dominant == other.Dominant)
        {
            return FromBigInteger(
                new BigInteger(Recessive.Raw) + other.Recessive.Raw,
                Dominant.Raw);
        }

        return FromBigInteger(
            (new BigInteger(Recessive.Raw) * other.Dominant.Raw) + (new BigInteger(other.Recessive.Raw) * Dominant.Raw),
            new BigInteger(Dominant.Raw) * other.Dominant.Raw);
    }

    public Ratio Subtract(Ratio other)
    {
        if (IsTension || other.IsTension)
        {
            return Tension(ToMeasure(new BigInteger(Recessive.Raw) - other.Recessive.Raw));
        }

        if (Dominant == other.Dominant)
        {
            return FromBigInteger(
                new BigInteger(Recessive.Raw) - other.Recessive.Raw,
                Dominant.Raw);
        }

        return FromBigInteger(
            (new BigInteger(Recessive.Raw) * other.Dominant.Raw) - (new BigInteger(other.Recessive.Raw) * Dominant.Raw),
            new BigInteger(Dominant.Raw) * other.Dominant.Raw);
    }

    public Ratio Multiply(Ratio other)
    {
        if (IsTension || other.IsTension)
        {
            return Tension(ToMeasure(new BigInteger(Recessive.Raw) * other.Recessive.Raw));
        }

        return FromBigInteger(
            new BigInteger(Recessive.Raw) * other.Recessive.Raw,
            new BigInteger(Dominant.Raw) * other.Dominant.Raw);
    }

    public Ratio Divide(Ratio other)
    {
        if (IsTension || other.IsTension || other.Recessive == Measure.Zero)
        {
            return Tension(Potential);
        }

        return FromBigInteger(
            new BigInteger(Recessive.Raw) * other.Dominant.Raw,
            new BigInteger(Dominant.Raw) * other.Recessive.Raw);
    }

    public Ratio Canonicalize()
    {
        if (IsTension)
        {
            return this;
        }

        var normalized = Normalize();

        if (normalized.Recessive.Raw == 0)
        {
            return new Ratio(Measure.Zero, Measure.One);
        }

        var numerator = normalized.Recessive.Absolute.Raw;
        var denominator = normalized.Dominant.Absolute.Raw;
        var gcd = GreatestCommonDivisor(numerator, denominator);

        if (gcd <= 1)
        {
            return normalized;
        }

        return new Ratio(
            new Measure(normalized.Recessive.Raw / gcd),
            new Measure(normalized.Dominant.Raw / gcd));
    }

    public Ratio Normalize()
    {
        if (IsTension || Dominant.Raw > 0)
        {
            return this;
        }

        return new Ratio(-Recessive, -Dominant);
    }

    public override string ToString() => string.Create(
        CultureInfo.InvariantCulture,
        $"{Recessive.Raw}/{Dominant.Raw}");

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

    private static Ratio FromBigInteger(BigInteger recessive, BigInteger dominant)
    {
        if (dominant.IsZero)
        {
            return Tension(ToMeasure(BigInteger.Abs(recessive)));
        }

        if (dominant.Sign < 0)
        {
            recessive = BigInteger.Negate(recessive);
            dominant = BigInteger.Negate(dominant);
        }

        if (Fits(recessive) && Fits(dominant))
        {
            return new Ratio(new Measure((long)recessive), new Measure((long)dominant));
        }

        var gcd = BigInteger.GreatestCommonDivisor(BigInteger.Abs(recessive), BigInteger.Abs(dominant));
        if (gcd > BigInteger.One)
        {
            recessive /= gcd;
            dominant /= gcd;
        }

        if (Fits(recessive) && Fits(dominant))
        {
            return new Ratio(new Measure((long)recessive), new Measure((long)dominant));
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
