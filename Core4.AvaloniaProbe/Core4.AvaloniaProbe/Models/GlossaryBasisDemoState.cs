using System;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Models;

/// <summary>
/// Thermal basis glossary state. One shared physical temperature domain is expressed through three different
/// calibrations once the custom interval has been frozen into a basis.
/// </summary>
public readonly record struct GlossaryBasisDemoState(
    Measure Phase,
    Measure SiteTicks,
    Measure OriginTicks,
    Measure RelationTicks,
    Measure ReadTicks)
{
    public const long TotalTicks = 250L;
    public const double MinCelsius = -20d;
    public const double MaxCelsius = 5d;
    public const double CelsiusRange = MaxCelsius - MinCelsius;
    public const double TicksPerCelsius = TotalTicks / CelsiusRange;

    public static GlossaryBasisDemoState CreateDefault()
        => new(new Measure(0L), new Measure(130L), new Measure(130L), new Measure(170L), CelsiusTicks(0d));

    public Carrier Carrier => new(1L);

    public bool HasOrigin => Phase.Raw >= 1L;

    public bool HasRelation => Phase.Raw >= 2L;

    public bool IsFrozen => Phase.Raw >= 3L;

    public bool ShowSetupSlider => Phase.Raw <= 1L;

    public bool ShowFreezeButton => Phase.Raw == 2L;

    public bool ShowReadSlider => IsFrozen;

    public string ActionLabel => Phase.Raw switch
    {
        <= 0L => "Pin Origin",
        1L => "Pick Relation",
        2L => "Freeze Basis",
        _ => string.Empty
    };

    public Placement Site => Carrier.At(new Ratio(SiteTicks, new Measure(TotalTicks)));

    public Placement Origin => Carrier.At(new Ratio(OriginTicks, new Measure(TotalTicks)));

    public Placement Relation => Carrier.At(new Ratio(RelationTicks, new Measure(TotalTicks)));

    public Placement Read => Carrier.At(new Ratio(ReadTicks, new Measure(TotalTicks)));

    public Measure CelsiusOriginTicks => CelsiusTicks(0d);

    public Measure CelsiusUnitTicks => CelsiusTicks(1d);

    public Measure FahrenheitOriginTicks => FahrenheitTicks(0d);

    public Measure FahrenheitUnitTicks => FahrenheitTicks(1d);

    public GlossaryBasisDemoState WithSetupTicks(long ticks) => this with
    {
        SiteTicks = ClampTicks(ticks)
    };

    public GlossaryBasisDemoState WithReadTicks(long ticks) => this with
    {
        ReadTicks = ClampTicks(ticks)
    };

    public GlossaryBasisDemoState WithOriginTicks(long ticks)
    {
        var candidate = ClampTicks(ticks);
        if (HasRelation)
        {
            candidate = Separate(candidate, RelationTicks);
        }

        return this with { OriginTicks = candidate };
    }

    public GlossaryBasisDemoState WithRelationTicks(long ticks)
        => this with { RelationTicks = Separate(ClampTicks(ticks), OriginTicks) };

    public GlossaryBasisDemoState StepAction() => Phase.Raw switch
    {
        <= 0L => this with
        {
            Phase = new Measure(1L),
            OriginTicks = SiteTicks
        },
        1L => this with
        {
            Phase = new Measure(2L),
            RelationTicks = Separate(SiteTicks, OriginTicks)
        },
        2L => this with
        {
            Phase = new Measure(3L),
            ReadTicks = CelsiusTicks(0d)
        },
        _ => this
    };

    public GlossaryBasisDemoState ResetLayout() => CreateDefault();

    public double CelsiusAtTicks(Measure ticks) => MinCelsius + (ticks.Raw / TicksPerCelsius);

    public double FahrenheitAtTicks(Measure ticks) => 32d + (CelsiusAtTicks(ticks) * 9d / 5d);

    public double CustomAtTicks(Measure ticks)
    {
        var denominator = CelsiusAtTicks(RelationTicks) - CelsiusAtTicks(OriginTicks);
        if (Math.Abs(denominator) < 0.000001d)
        {
            return 0d;
        }

        return (CelsiusAtTicks(ticks) - CelsiusAtTicks(OriginTicks)) / denominator;
    }

    public double CustomFreezingValue => CustomAtCelsius(0d);

    public double CustomBoilingValue => CustomAtCelsius(100d);

    public double CustomMinimumVisibleValue => CustomAtCelsius(MinCelsius);

    public double CustomMaximumVisibleValue => CustomAtCelsius(MaxCelsius);

    public double CustomAtCelsius(double celsius)
    {
        var denominator = CelsiusAtTicks(RelationTicks) - CelsiusAtTicks(OriginTicks);
        if (Math.Abs(denominator) < 0.000001d)
        {
            return 0d;
        }

        return (celsius - CelsiusAtTicks(OriginTicks)) / denominator;
    }

    public static Measure CelsiusTicks(double celsius)
        => new((long)Math.Round((celsius - MinCelsius) * TicksPerCelsius, MidpointRounding.AwayFromZero));

    public static Measure FahrenheitTicks(double fahrenheit)
        => CelsiusTicks((fahrenheit - 32d) * 5d / 9d);

    public double CelsiusForCustomValue(double value)
        => CelsiusAtTicks(OriginTicks) + (value * (CelsiusAtTicks(RelationTicks) - CelsiusAtTicks(OriginTicks)));

    public double TicksForCustomValue(double value)
        => CelsiusTicks(CelsiusForCustomValue(value)).Raw;

    private static Measure ClampTicks(long ticks) => new(Math.Clamp(ticks, 0L, TotalTicks));

    private static Measure Separate(Measure candidate, Measure other)
    {
        if (candidate.Raw != other.Raw)
        {
            return candidate;
        }

        return other.Raw < TotalTicks
            ? new Measure(other.Raw + 1L)
            : new Measure(Math.Max(0L, other.Raw - 1L));
    }
}
