using System.Globalization;

namespace Core4.Engine;

public readonly record struct Ratio(Measure Numerator, Measure Denominator)
{
    public bool IsDefined => Denominator.Raw != 0;

    public Measure Sign
    {
        get
        {
            if (!IsDefined || Numerator.Raw == 0)
            {
                return Measure.Zero;
            }

            return Numerator.Sign == Denominator.Sign ? new Measure(1) : new Measure(-1);
        }
    }

    public Measure Whole => IsDefined ? new Measure(Numerator.Raw / Denominator.Raw) : Measure.Zero;

    public Measure Remainder => IsDefined ? new Measure(Numerator.Raw % Denominator.Raw) : Measure.Zero;

    public double Approximate => IsDefined ? (double)Numerator.Raw / Denominator.Raw : 0d;

    public Ratio Normalize()
    {
        if (!IsDefined || Denominator.Raw > 0)
        {
            return this;
        }

        return new Ratio(-Numerator, -Denominator);
    }

    public override string ToString() => string.Create(
        CultureInfo.InvariantCulture,
        $"{Numerator.Raw}/{Denominator.Raw}");
}
