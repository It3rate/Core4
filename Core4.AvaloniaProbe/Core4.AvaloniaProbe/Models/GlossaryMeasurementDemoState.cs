using System;
using System.Collections.Immutable;
using Core4.Numerics;

namespace Core4.AvaloniaProbe.Models;

/// <summary>
/// First measurement glossary scene. A fixed unit length negotiates raw high-resolution values into placements by
/// forcing them onto the unit's chosen denominator.
/// </summary>
public readonly record struct GlossaryMeasurementDemoState(
    Measure UnitTicks,
    ImmutableArray<GlossaryMeasurementValueState> AvailableValues,
    ImmutableArray<GlossaryMeasurementValueState> AttachedValues,
    GlossaryMeasurementAnimationState Animation)
{
    public const long MinimumUnitTicks = 2L;
    public const long MaximumUnitTicks = 60L;
    public const double DisplayUnits = 4d;
    public const double AnimationDurationSeconds = 3.8d;

    public static GlossaryMeasurementDemoState CreateDefault()
        => new(
        new Measure(3L),
        [
            new GlossaryMeasurementValueState(1L, new Ratio(new Measure(29L), new Measure(36L))),
            new GlossaryMeasurementValueState(2L, new Ratio(new Measure(61L), new Measure(36L))),
            new GlossaryMeasurementValueState(3L, new Ratio(new Measure(97L), new Measure(48L))),
            new GlossaryMeasurementValueState(4L, new Ratio(new Measure(149L), new Measure(60L))),
            new GlossaryMeasurementValueState(5L, new Ratio(new Measure(239L), new Measure(72L)))
        ],
        ImmutableArray<GlossaryMeasurementValueState>.Empty,
        default);

    public bool IsAnimating => Animation.IsActive;

    public GlossaryMeasurementDemoState WithUnitTicks(long ticks)
        => this with
        {
            UnitTicks = new Measure(Math.Clamp(ticks, MinimumUnitTicks, MaximumUnitTicks))
        };

    public GlossaryMeasurementDemoState BeginAttach(long valueId)
    {
        if (IsAnimating || !TryFindValue(AvailableValues, valueId, out _))
        {
            return this;
        }

        return this with
        {
            Animation = new GlossaryMeasurementAnimationState(valueId, 0d)
        };
    }

    public GlossaryMeasurementDemoState WithAnimationProgress(double progress)
    {
        if (!IsAnimating)
        {
            return this;
        }

        return this with
        {
            Animation = Animation with { Progress = Math.Clamp(progress, 0d, 1d) }
        };
    }

    public GlossaryMeasurementDemoState CompleteAnimation()
    {
        if (!TryGetAnimatingValue(out var value))
        {
            return this with { Animation = default };
        }

        return this with
        {
            AvailableValues = RemoveValue(AvailableValues, value.Id),
            AttachedValues = AttachedValues.Insert(0, value),
            Animation = default
        };
    }

    public GlossaryMeasurementDemoState Reset() => CreateDefault();

    public bool TryGetAnimatingValue(out GlossaryMeasurementValueState value)
        => TryFindValue(AvailableValues, Animation.ValueId, out value);

    public Ratio ResolvedRatio(GlossaryMeasurementValueState value)
    {
        var denominator = Math.Max(MinimumUnitTicks, UnitTicks.Raw);
        var resolvedNumerator = (long)Math.Round(value.RawRatio.Approximate * denominator, MidpointRounding.AwayFromZero);
        return new Ratio(new Measure(Math.Max(0L, resolvedNumerator)), new Measure(denominator));
    }

    public string RawLabel(GlossaryMeasurementValueState value)
        => $"{value.RawRatio.Recessive.Raw}/{value.RawRatio.Dominant.Raw}";

    public string ResolvedLabel(GlossaryMeasurementValueState value)
    {
        var resolved = ResolvedRatio(value);
        return $"{resolved.Recessive.Raw}/{resolved.Dominant.Raw}";
    }

    public long MixedWhole(GlossaryMeasurementValueState value)
    {
        var resolved = ResolvedRatio(value);
        return resolved.Dominant.Raw == 0 ? 0L : resolved.Recessive.Raw / resolved.Dominant.Raw;
    }

    public long MixedRemainder(GlossaryMeasurementValueState value)
    {
        var resolved = ResolvedRatio(value);
        return resolved.Dominant.Raw == 0 ? 0L : resolved.Recessive.Raw % resolved.Dominant.Raw;
    }

    public string MixedLabel(GlossaryMeasurementValueState value)
    {
        var resolved = ResolvedRatio(value);
        var whole = MixedWhole(value);
        var remainder = MixedRemainder(value);
        return remainder == 0L
            ? whole.ToString()
            : $"{whole} {remainder}/{resolved.Dominant.Raw}";
    }

    public double RawUnits(GlossaryMeasurementValueState value) => value.RawRatio.Approximate;

    public double ResolvedUnits(GlossaryMeasurementValueState value) => ResolvedRatio(value).Approximate;

    public double TensionUnits(GlossaryMeasurementValueState value)
        => Math.Abs(RawUnits(value) - ResolvedUnits(value));

    public string PhaseText
    {
        get
        {
            if (!TryGetAnimatingValue(out var value))
            {
                return "waiting for selection";
            }

            return $"negotiating {RawLabel(value)}";
        }
    }

    public string DecisionText
    {
        get
        {
            if (!TryGetAnimatingValue(out var value))
            {
                return $"unit 1 uses {UnitTicks.Raw} ticks";
            }

            return $"{RawLabel(value)} -> {ResolvedLabel(value)}";
        }
    }

    private static bool TryFindValue(
        ImmutableArray<GlossaryMeasurementValueState> values,
        long valueId,
        out GlossaryMeasurementValueState value)
    {
        foreach (var candidate in values)
        {
            if (candidate.Id == valueId)
            {
                value = candidate;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static ImmutableArray<GlossaryMeasurementValueState> RemoveValue(
        ImmutableArray<GlossaryMeasurementValueState> values,
        long valueId)
    {
        var builder = values.ToBuilder();
        for (var index = 0; index < builder.Count; index++)
        {
            if (builder[index].Id != valueId)
            {
                continue;
            }

            builder.RemoveAt(index);
            break;
        }

        return builder.ToImmutable();
    }
}

public readonly record struct GlossaryMeasurementValueState(long Id, Ratio RawRatio);

public readonly record struct GlossaryMeasurementAnimationState(long ValueId, double Progress)
{
    public bool IsActive => ValueId > 0L;
}
