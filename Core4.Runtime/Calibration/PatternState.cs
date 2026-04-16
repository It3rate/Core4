using System.Collections.Immutable;

namespace Core4.Calibration;

/// <summary>
/// Rolling three-measure local pattern state. It keeps a tiny Core4-native memory of one incoming stream:
/// three recent samples, a running center, signed directional streak, and numeric residuals that indicate how
/// well the recent window fits simple additive, multiplicative, oscillatory, flat, or shock-like behavior.
/// </summary>
public sealed class PatternState
{
    private static readonly Ratio DefaultEmaWeight = new(1, 4);
    private readonly ImmutableArray<Measure> _samples;

    private PatternState(
        Carrier carrier,
        ImmutableArray<Measure> samples,
        Measure runningAverage,
        Measure additiveFitPressure,
        Measure multiplicativeFitPressure,
        Measure directionStreak,
        Measure stepsSinceLastReversal)
    {
        Carrier = carrier;
        _samples = samples;
        RunningAverage = runningAverage;
        AdditiveFitPressure = additiveFitPressure;
        MultiplicativeFitPressure = multiplicativeFitPressure;
        DirectionStreak = directionStreak;
        StepsSinceLastReversal = stepsSinceLastReversal;
    }

    public Carrier Carrier { get; }

    public ImmutableArray<Measure> Samples => _samples;

    public Measure RunningAverage { get; }

    public Measure AdditiveFitPressure { get; }

    public Measure MultiplicativeFitPressure { get; }

    public Measure FitSkew => AdditiveFitPressure - MultiplicativeFitPressure;

    public Measure DirectionStreak { get; }

    public Measure StepsSinceLastReversal { get; }

    public bool HasOrigin => _samples.Length >= 1;

    public bool HasFirstSpan => _samples.Length >= 2;

    public bool HasWindow => _samples.Length >= 3;

    public Measure OriginMeasure => HasOrigin ? _samples[0] : Measure.Zero;

    public Measure UnitMeasure => HasFirstSpan ? _samples[1] : Measure.Zero;

    public Measure ValueMeasure => HasWindow ? _samples[2] : Measure.Zero;

    public Measure FirstDelta => HasFirstSpan
        ? UnitMeasure - OriginMeasure
        : Measure.Zero;

    public Measure SecondDelta => HasWindow
        ? ValueMeasure - UnitMeasure
        : Measure.Zero;

    public Measure LastSlope => HasWindow ? SecondDelta : FirstDelta;

    public Measure Momentum => HasWindow
        ? SecondDelta - FirstDelta
        : Measure.Zero;

    public Measure WindowRange
    {
        get
        {
            if (!HasWindow)
            {
                return Measure.Zero;
            }

            var minimum = Min(OriginMeasure, Min(UnitMeasure, ValueMeasure));
            var maximum = Max(OriginMeasure, Max(UnitMeasure, ValueMeasure));
            return maximum - minimum;
        }
    }

    public Ratio FirstValueRatio => HasFirstSpan
        ? ResolveRatio(UnitMeasure, OriginMeasure, FirstDelta.Absolute)
        : Ratio.Zero(Measure.Zero);

    public Ratio SecondValueRatio => HasWindow
        ? ResolveRatio(ValueMeasure, UnitMeasure, SecondDelta.Absolute)
        : Ratio.Zero(Measure.Zero);

    public Ratio GeometricResidual => HasWindow
        ? SecondValueRatio.Subtract(FirstValueRatio).Normalize()
        : Ratio.Zero(Measure.Zero);

    public Measure GeometricResidualPotential => GeometricResidual.Canonicalize().Potential;

    public Measure LinearResidual => Momentum.Absolute;

    public Measure FlatResidual => Max(FirstDelta.Absolute, SecondDelta.Absolute);

    public Measure ReversalStrength => HasWindow && AreOpposed(FirstDelta, SecondDelta)
        ? Min(FirstDelta.Absolute, SecondDelta.Absolute)
        : Measure.Zero;

    public Measure OscillationResidual
    {
        get
        {
            if (!HasWindow)
            {
                return Measure.Zero;
            }

            return AreOpposed(FirstDelta, SecondDelta)
                ? (FirstDelta.Absolute - SecondDelta.Absolute).Absolute
                : FirstDelta.Absolute + SecondDelta.Absolute;
        }
    }

    public Measure ShockResidual => HasWindow
        ? (SecondDelta.Absolute - FirstDelta.Absolute).Absolute
        : Measure.Zero;

    public static PatternState Start(Carrier carrier) =>
        new(carrier, ImmutableArray<Measure>.Empty, Measure.Zero, Measure.Zero, Measure.Zero, Measure.Zero, Measure.Zero);

    public static PatternState Start(long carrier) => Start(new Carrier(carrier));

    public PatternState Input(Measure value) => Input(value, DefaultEmaWeight);

    public PatternState Input(Measure value, Ratio emaWeight)
    {
        var nextSamples = _samples.Length switch
        {
            < 3 => _samples.Add(value),
            _ => ImmutableArray.Create(_samples[1], _samples[2], value)
        };

        var nextAverage = HasOrigin && CanApply(emaWeight)
            ? Nudge(RunningAverage, value, emaWeight)
            : value;

        var priorSlope = LastSlope;
        var provisional = new PatternState(
            Carrier,
            nextSamples,
            nextAverage,
            AdditiveFitPressure,
            MultiplicativeFitPressure,
            DirectionStreak,
            StepsSinceLastReversal);
        var nextSlope = provisional.LastSlope;
        var nextStreak = AdvanceStreak(DirectionStreak, nextSlope);
        var nextReversalAge = AdvanceReversalAge(StepsSinceLastReversal, priorSlope, nextSlope, provisional.HasWindow);
        var nextAdditiveFit = provisional.HasWindow && CanApply(emaWeight)
            ? Nudge(AdditiveFitPressure, provisional.LinearResidual, emaWeight)
            : AdditiveFitPressure;
        var nextMultiplicativeFit = provisional.HasWindow && CanApply(emaWeight)
            ? Nudge(MultiplicativeFitPressure, provisional.GeometricResidualPotential, emaWeight)
            : MultiplicativeFitPressure;

        return new PatternState(
            Carrier,
            nextSamples,
            nextAverage,
            nextAdditiveFit,
            nextMultiplicativeFit,
            nextStreak,
            nextReversalAge);
    }

    private static Measure AdvanceStreak(Measure current, Measure slope)
    {
        if (slope == Measure.Zero)
        {
            return Measure.Zero;
        }

        if (current == Measure.Zero || current.Sign != slope.Sign)
        {
            return slope.Sign;
        }

        return new Measure(current.Raw + slope.Sign.Raw);
    }

    private static Measure AdvanceReversalAge(Measure current, Measure priorSlope, Measure nextSlope, bool hasWindow)
    {
        if (!hasWindow || nextSlope == Measure.Zero)
        {
            return current;
        }

        if (priorSlope != Measure.Zero && AreOpposed(priorSlope, nextSlope))
        {
            return Measure.Zero;
        }

        return new Measure(current.Raw + 1);
    }

    private static Ratio ResolveRatio(Measure numerator, Measure denominator, Measure tensionFallback)
        => denominator == Measure.Zero
            ? Ratio.Tension(tensionFallback)
            : new Ratio(numerator, denominator).Normalize();

    private static Measure Nudge(Measure from, Measure toward, Ratio weight)
    {
        var delta = toward.Raw - from.Raw;
        var moved = delta * weight.Recessive.Raw / weight.Dominant.Raw;
        return new Measure(from.Raw + moved);
    }

    private static bool CanApply(Ratio ratio) => !ratio.IsTension && ratio.Dominant != Measure.Zero;

    private static bool AreOpposed(Measure left, Measure right)
        => left.Sign != Measure.Zero && right.Sign != Measure.Zero && left.Sign != right.Sign;

    private static Measure Min(Measure left, Measure right) => left.Raw <= right.Raw ? left : right;

    private static Measure Max(Measure left, Measure right) => left.Raw >= right.Raw ? left : right;
}
