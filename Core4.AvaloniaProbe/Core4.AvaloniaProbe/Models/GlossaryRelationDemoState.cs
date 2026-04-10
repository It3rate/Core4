using System;
using System.Collections.Immutable;
using Avalonia;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Models;

/// <summary>
/// Second glossary page: one carrier is first pinned as origin, then later relations are frozen against it.
/// </summary>
public readonly record struct GlossaryRelationDemoState(
    Carrier Carrier,
    Point SeedStart,
    Point SeedEnd,
    Measure StartValue,
    Measure EndValue,
    Measure SiteTicks,
    Measure OriginTicks,
    bool HasOrigin,
    ImmutableArray<GlossaryFrozenRelationState> Relations)
{
    private const long TotalTicks = 100L;
    private const long UnitTicks = 10L;

    public Point Start { get; init; } = SeedStart;

    public Point End { get; init; } = SeedEnd;

    public static GlossaryRelationDemoState CreateDefault()
    {
        var (startValue, endValue) = CreateOrderedRange();
        return new GlossaryRelationDemoState(
            new Carrier(1),
            new Point(0.16d, 0.66d),
            new Point(0.86d, 0.28d),
            startValue,
            endValue,
            new Measure(42),
            Measure.Zero,
            false,
            ImmutableArray<GlossaryFrozenRelationState>.Empty);
    }

    public Placement Site => Carrier.At(new Ratio(SiteTicks, new Measure(TotalTicks)));

    public Placement Origin => Carrier.At(new Ratio(OriginTicks, new Measure(TotalTicks)));

    public Measure CurrentMeasure => ValueAt(SiteTicks);

    public double CurrentRelation => HasOrigin
        ? (SiteTicks.Raw - OriginTicks.Raw) / (double)UnitTicks
        : 0d;

    public string ActionLabel => HasOrigin ? "Freeze Relation" : "Pin Origin";

    public GlossaryRelationDemoState WithEndpoint(bool updateStart, Point value)
    {
        var clamped = ClampPoint(value);
        return updateStart
            ? this with { Start = clamped }
            : this with { End = clamped };
    }

    public GlossaryRelationDemoState WithSiteTicks(long ticks) => this with
    {
        SiteTicks = new Measure(Math.Clamp(ticks, 0L, TotalTicks))
    };

    public GlossaryRelationDemoState StepAction() => HasOrigin ? FreezeRelation() : PinOrigin();

    public GlossaryRelationDemoState PinOrigin() => this with
    {
        OriginTicks = SiteTicks,
        HasOrigin = true,
        Relations = ImmutableArray<GlossaryFrozenRelationState>.Empty
    };

    public GlossaryRelationDemoState FreezeRelation()
    {
        if (!HasOrigin)
        {
            return PinOrigin();
        }

        return this with
        {
            Relations = Relations.Add(new GlossaryFrozenRelationState(
                Site,
                new Measure(SiteTicks.Raw - OriginTicks.Raw)))
        };
    }

    public GlossaryRelationDemoState ClearFrozen() => this with
    {
        HasOrigin = false,
        OriginTicks = Measure.Zero,
        Relations = ImmutableArray<GlossaryFrozenRelationState>.Empty
    };

    public GlossaryRelationDemoState ResetLayout() => CreateDefault();

    public Measure ValueAt(Measure ticks)
    {
        var clamped = Math.Clamp(ticks.Raw, 0L, TotalTicks);
        var span = EndValue.Raw - StartValue.Raw;
        return new Measure(StartValue.Raw + ((span * clamped) / TotalTicks));
    }

    private static Point ClampPoint(Point point) => new(
        Math.Clamp(point.X, 0.06d, 0.94d),
        Math.Clamp(point.Y, 0.08d, 0.92d));

    private static (Measure StartValue, Measure EndValue) CreateOrderedRange()
    {
        while (true)
        {
            var lower = Random.Shared.NextInt64(10L, 9850L);
            var maxSpan = Math.Min(4200L, 9999L - lower);
            if (maxSpan < 50L)
            {
                continue;
            }

            var span = Random.Shared.NextInt64(50L, maxSpan + 1L);
            var upper = lower + span;
            var ascending = Random.Shared.Next(0, 2) == 0;
            return ascending
                ? (new Measure(lower), new Measure(upper))
                : (new Measure(upper), new Measure(lower));
        }
    }
}

public readonly record struct GlossaryFrozenRelationState(Placement Placement, Measure RelativeTicks)
{
    public double Units => RelativeTicks.Raw / 10d;
}
