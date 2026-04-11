using System;
using System.Collections.Immutable;
using Core4.Numerics;

namespace Core4.AvaloniaProbe.Models;

/// <summary>
/// Tension / fusion / orthogonal glossary scene. One signed unit is dragged across a horizontal control line; values
/// remain numerically stable against that unit, but their visible direction changes as the unit becomes positive,
/// orthogonal, or negative.
/// </summary>
public readonly record struct GlossaryTensionDemoState(
    Measure UnitCarrierTicks,
    Measure LastAlignedMagnitudeTicks,
    ImmutableArray<GlossaryTensionValueState> Values,
    ImmutableArray<GlossaryDetachedValueState> Detached)
{
    public const long UnitTickCount = 4L;
    public const long MinimumCarrierTicks = -12L;
    public const long MaximumCarrierTicks = 12L;

    public static GlossaryTensionDemoState CreateDefault()
        => new(
        new Measure(5L),
        new Measure(5L),
        [
            new GlossaryTensionValueState(1L, new Ratio(new Measure(-6L), new Measure(UnitTickCount))),
            new GlossaryTensionValueState(2L, new Ratio(new Measure(-2L), new Measure(UnitTickCount))),
            new GlossaryTensionValueState(3L, new Ratio(new Measure(3L), new Measure(UnitTickCount))),
            new GlossaryTensionValueState(4L, new Ratio(new Measure(7L), new Measure(UnitTickCount)))
        ],
        ImmutableArray<GlossaryDetachedValueState>.Empty);

    public bool IsOrthogonal => UnitCarrierTicks.Raw == 0L;

    public Measure AxisSign => new(Math.Sign(UnitCarrierTicks.Raw));

    public Measure EffectiveMagnitudeTicks => IsOrthogonal
        ? new Measure(Math.Max(1L, LastAlignedMagnitudeTicks.Raw))
        : new Measure(Math.Abs(UnitCarrierTicks.Raw));

    public GlossaryTensionDemoState WithUnitCarrierTicks(long ticks)
    {
        var clamped = Math.Clamp(ticks, MinimumCarrierTicks, MaximumCarrierTicks);
        return this with
        {
            UnitCarrierTicks = new Measure(clamped),
            LastAlignedMagnitudeTicks = clamped == 0L
                ? LastAlignedMagnitudeTicks
                : new Measure(Math.Abs(clamped))
        };
    }

    public GlossaryTensionDemoState Detach(long id)
    {
        if (!TryFindValue(Values, id, out var value))
        {
            return this;
        }

        return this with
        {
            Values = RemoveValue(Values, id),
            Detached = Detached.Add(new GlossaryDetachedValueState(
                value,
                AxisSign,
                IsOrthogonal,
                EffectiveMagnitudeTicks))
        };
    }

    public GlossaryTensionDemoState Reset() => CreateDefault();

    public string StateText => IsOrthogonal
        ? $"unit 0 | orthogonal tension | magnitude {EffectiveMagnitudeTicks.Raw}"
        : $"unit {UnitCarrierTicks.Raw}/{UnitTickCount}";

    public string HintText => IsOrthogonal
        ? "positive values stand upward, negative values downward"
        : UnitCarrierTicks.Raw > 0L
            ? "positive values align to the right"
            : "positive values align to the left";

    public static bool TryFindValue(
        ImmutableArray<GlossaryTensionValueState> values,
        long id,
        out GlossaryTensionValueState value)
    {
        foreach (var candidate in values)
        {
            if (candidate.Id == id)
            {
                value = candidate;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static ImmutableArray<GlossaryTensionValueState> RemoveValue(
        ImmutableArray<GlossaryTensionValueState> values,
        long id)
    {
        var builder = values.ToBuilder();
        for (var index = 0; index < builder.Count; index++)
        {
            if (builder[index].Id != id)
            {
                continue;
            }

            builder.RemoveAt(index);
            break;
        }

        return builder.ToImmutable();
    }
}

public readonly record struct GlossaryTensionValueState(long Id, Ratio Value)
{
    public Measure Sign => new(Math.Sign(Value.Recessive.Raw));

    public string Label
    {
        get
        {
            var numerator = Value.Recessive.Raw;
            var denominator = Math.Max(1L, Value.Dominant.Raw);
            var absolute = Math.Abs(numerator);
            var whole = absolute / denominator;
            var remainder = absolute % denominator;
            var core = remainder == 0L
                ? whole.ToString()
                : whole == 0L
                    ? $"{remainder}/{denominator}"
                    : $"{whole} {remainder}/{denominator}";
            return numerator < 0L ? $"-{core}" : core;
        }
    }
}

public readonly record struct GlossaryDetachedValueState(
    GlossaryTensionValueState Value,
    Measure AxisSign,
    bool IsOrthogonal,
    Measure MagnitudeTicks)
{
    public bool IsDominant => Value.Sign.Raw >= 0L;

    public string Bucket => (IsDominant, IsOrthogonal) switch
    {
        (true, false) => "Dominant Aligned",
        (true, true) => "Dominant Orthogonal",
        (false, false) => "Recessive Aligned",
        _ => "Recessive Orthogonal"
    };
}
