using System;
using System.Collections.Immutable;
using Avalonia;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Models;

/// <summary>
/// Multi-carrier relation glossary state. Each carrier can pin its own origin and freeze later relations.
/// </summary>
public readonly record struct GlossaryRelationDemoState(ImmutableArray<GlossaryRelationCarrierState> Carriers)
{
    public static GlossaryRelationDemoState CreateDefault()
        => new(
        [
            CreateCarrier(new Carrier(1), new Point(0.56d, 0.20d), new Point(0.90d, 0.20d), 46L),
            CreateCarrier(new Carrier(2), new Point(0.18d, 0.82d), new Point(0.18d, 0.18d), 42L),
            CreateCarrier(new Carrier(3), new Point(0.56d, 0.78d), new Point(0.90d, 0.68d), 27L)
        ]);

    public GlossaryRelationCarrierState CarrierAt(int index) => Carriers[index];

    public Measure OriginCount
    {
        get
        {
            long count = 0L;
            foreach (var carrier in Carriers)
            {
                if (carrier.HasOrigin)
                {
                    count++;
                }
            }

            return new Measure(count);
        }
    }

    public Measure FrozenRelationCount
    {
        get
        {
            long count = 0L;
            foreach (var carrier in Carriers)
            {
                count += carrier.Relations.Length;
            }

            return new Measure(count);
        }
    }

    public GlossaryRelationDemoState WithEndpoint(int index, bool updateStart, Point value)
        => Update(index, carrier => carrier.WithEndpoint(updateStart, value));

    public GlossaryRelationDemoState WithSiteTicks(int index, long ticks)
        => Update(index, carrier => carrier.WithSiteTicks(ticks));

    public GlossaryRelationDemoState StepAction(int index)
        => Update(index, carrier => carrier.StepAction());

    public GlossaryRelationDemoState ClearFrozen()
    {
        var builder = Carriers.ToBuilder();
        for (var index = 0; index < builder.Count; index++)
        {
            builder[index] = builder[index].ClearFrozen();
        }

        return new GlossaryRelationDemoState(builder.ToImmutable());
    }

    public GlossaryRelationDemoState ResetLayout() => CreateDefault();

    private GlossaryRelationDemoState Update(int index, Func<GlossaryRelationCarrierState, GlossaryRelationCarrierState> update)
    {
        if ((uint)index >= (uint)Carriers.Length)
        {
            return this;
        }

        var builder = Carriers.ToBuilder();
        builder[index] = update(builder[index]);
        return new GlossaryRelationDemoState(builder.ToImmutable());
    }

    private static GlossaryRelationCarrierState CreateCarrier(Carrier carrier, Point seedStart, Point seedEnd, long siteTicks)
    {
        var (startValue, endValue) = CreateOrderedRange();
        return new GlossaryRelationCarrierState(
            carrier,
            seedStart,
            seedEnd,
            startValue,
            endValue,
            new Measure(siteTicks),
            Measure.Zero,
            false,
            ImmutableArray<GlossaryFrozenRelationState>.Empty);
    }

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

public readonly record struct GlossaryRelationCarrierState(
    Carrier Carrier,
    Point SeedStart,
    Point SeedEnd,
    Measure RawStartValue,
    Measure RawEndValue,
    Measure SiteTicks,
    Measure OriginTicks,
    bool HasOrigin,
    ImmutableArray<GlossaryFrozenRelationState> Relations)
{
    private const long TotalTicks = 100L;

    public Point Start { get; init; } = SeedStart;

    public Point End { get; init; } = SeedEnd;

    public Placement Site => Carrier.At(new Ratio(SiteTicks, new Measure(TotalTicks)));

    public Placement Origin => Carrier.At(new Ratio(OriginTicks, new Measure(TotalTicks)));

    public Measure CurrentMeasure => RawValueAt(SiteTicks);

    public Measure OriginMeasure => RawValueAt(OriginTicks);

    public Measure CurrentRelation => HasOrigin
        ? new Measure(CurrentMeasure.Raw - OriginMeasure.Raw)
        : Measure.Zero;

    public string ActionLabel => HasOrigin ? "Freeze Relation" : "Pin Origin";

    public long StartDisplayValue => HasOrigin
        ? RawStartValue.Raw - OriginMeasure.Raw
        : RawStartValue.Raw;

    public long EndDisplayValue => HasOrigin
        ? RawEndValue.Raw - OriginMeasure.Raw
        : RawEndValue.Raw;

    public GlossaryRelationCarrierState WithEndpoint(bool updateStart, Point value)
    {
        var clamped = ClampPoint(value);
        return updateStart
            ? this with { Start = clamped }
            : this with { End = clamped };
    }

    public GlossaryRelationCarrierState WithSiteTicks(long ticks) => this with
    {
        SiteTicks = new Measure(Math.Clamp(ticks, 0L, TotalTicks))
    };

    public GlossaryRelationCarrierState StepAction() => HasOrigin ? FreezeRelation() : PinOrigin();

    public GlossaryRelationCarrierState PinOrigin() => this with
    {
        OriginTicks = SiteTicks,
        HasOrigin = true,
        Relations = ImmutableArray<GlossaryFrozenRelationState>.Empty
    };

    public GlossaryRelationCarrierState FreezeRelation()
    {
        if (!HasOrigin)
        {
            return PinOrigin();
        }

        return this with
        {
            Relations = Relations.Add(new GlossaryFrozenRelationState(
                Site,
                CurrentRelation))
        };
    }

    public GlossaryRelationCarrierState ClearFrozen() => this with
    {
        HasOrigin = false,
        OriginTicks = Measure.Zero,
        Relations = ImmutableArray<GlossaryFrozenRelationState>.Empty
    };

    public Measure RawValueAt(Measure ticks)
    {
        var clamped = Math.Clamp(ticks.Raw, 0L, TotalTicks);
        var span = RawEndValue.Raw - RawStartValue.Raw;
        return new Measure(RawStartValue.Raw + ((span * clamped) / TotalTicks));
    }

    private static Point ClampPoint(Point point) => new(
        Math.Clamp(point.X, 0.06d, 0.94d),
        Math.Clamp(point.Y, 0.08d, 0.92d));
}

public readonly record struct GlossaryFrozenRelationState(Placement Placement, Measure RelativeValue);
