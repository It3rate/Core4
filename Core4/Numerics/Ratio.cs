using System.Globalization;

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

    public Ratio Add(Ratio other)
    {
        if (IsTension || other.IsTension)
        {
            return Tension(Recessive + other.Recessive);
        }

        if (Dominant == other.Dominant)
        {
            return new Ratio(
                new Measure(checked(Recessive.Raw + other.Recessive.Raw)),
                Dominant).Normalize();
        }

        return new Ratio(
            new Measure(checked((Recessive.Raw * other.Dominant.Raw) + (other.Recessive.Raw * Dominant.Raw))),
            new Measure(checked(Dominant.Raw * other.Dominant.Raw))).Normalize();
    }

    public Ratio Subtract(Ratio other)
    {
        if (IsTension || other.IsTension)
        {
            return Tension(Recessive - other.Recessive);
        }

        if (Dominant == other.Dominant)
        {
            return new Ratio(
                new Measure(checked(Recessive.Raw - other.Recessive.Raw)),
                Dominant).Normalize();
        }

        return new Ratio(
            new Measure(checked((Recessive.Raw * other.Dominant.Raw) - (other.Recessive.Raw * Dominant.Raw))),
            new Measure(checked(Dominant.Raw * other.Dominant.Raw))).Normalize();
    }

    public Ratio Multiply(Ratio other)
    {
        if (IsTension || other.IsTension)
        {
            return Tension(new Measure(checked(Recessive.Raw * other.Recessive.Raw)));
        }

        return new Ratio(
            new Measure(checked(Recessive.Raw * other.Recessive.Raw)),
            new Measure(checked(Dominant.Raw * other.Dominant.Raw))).Normalize();
    }

    public Ratio Divide(Ratio other)
    {
        if (IsTension || other.IsTension || other.Recessive == Measure.Zero)
        {
            return Tension(Potential);
        }

        return new Ratio(
            new Measure(checked(Recessive.Raw * other.Dominant.Raw)),
            new Measure(checked(Dominant.Raw * other.Recessive.Raw))).Normalize();
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
}
