using System.Globalization;

namespace Core4.Engine;

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
}
