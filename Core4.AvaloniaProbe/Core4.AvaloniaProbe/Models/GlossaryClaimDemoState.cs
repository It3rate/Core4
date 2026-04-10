using System;
using System.Collections.Immutable;
using Avalonia;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Models;

/// <summary>
/// Scene state for the first glossary claim page. Carrier geometry is visual, while site and measure positions stay
/// on real Core4 carriers as placements expressed by ratios. Endpoint values are intentionally ordered but
/// uncalibrated.
/// </summary>
public readonly record struct GlossaryClaimDemoState(ImmutableArray<GlossaryClaimCarrierState> Carriers)
{
    private static readonly Measure DefaultTicks = new(12);

    public static GlossaryClaimDemoState CreateDefault()
        => new(
        [
            CreateCarrier(new Carrier(1), new Point(0.58d, 0.18d), new Point(0.90d, 0.34d), new Ratio(2, DefaultTicks)),
            CreateCarrier(new Carrier(2), new Point(0.22d, 0.82d), new Point(0.22d, 0.16d), new Ratio(7, DefaultTicks)),
            CreateCarrier(new Carrier(3), new Point(0.46d, 0.78d), new Point(0.86d, 0.62d), new Ratio(9, DefaultTicks))
        ]);

    public GlossaryClaimCarrierState CarrierAt(int index) => Carriers[index];

    public GlossaryClaimDemoState WithEndpoint(int index, bool updateStart, Point value)
        => UpdateCarrier(index, carrier => carrier.WithEndpoint(updateStart, value));

    public GlossaryClaimDemoState WithSiteTicks(int index, long numerator, long denominator = 12L)
        => UpdateCarrier(index, carrier => carrier.WithSite(new Ratio(new Measure(numerator), new Measure(Math.Max(1L, denominator)))));

    public GlossaryClaimDemoState AddPin(int index)
        => UpdateCarrier(index, carrier => carrier.AddPin());

    public GlossaryClaimDemoState ClearPins()
    {
        var builder = Carriers.ToBuilder();
        for (var index = 0; index < builder.Count; index++)
        {
            builder[index] = builder[index].ClearPins();
        }

        return new GlossaryClaimDemoState(builder.ToImmutable());
    }

    public GlossaryClaimDemoState ResetLayout() => CreateDefault();

    private GlossaryClaimDemoState UpdateCarrier(int index, Func<GlossaryClaimCarrierState, GlossaryClaimCarrierState> update)
    {
        if ((uint)index >= (uint)Carriers.Length)
        {
            return this;
        }

        var builder = Carriers.ToBuilder();
        builder[index] = update(builder[index]);
        return new GlossaryClaimDemoState(builder.ToImmutable());
    }

    private static GlossaryClaimCarrierState CreateCarrier(Carrier carrier, Point seedStart, Point seedEnd, Ratio siteRatio)
    {
        var (startValue, endValue) = CreateOrderedRange();
        return new GlossaryClaimCarrierState(
            carrier,
            seedStart,
            seedEnd,
            siteRatio,
            startValue,
            endValue,
            ImmutableArray<GlossaryClaimPinState>.Empty);
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

public readonly record struct GlossaryClaimCarrierState(
    Carrier Carrier,
    Point SeedStart,
    Point SeedEnd,
    Ratio SiteRatio,
    Measure StartValue,
    Measure EndValue,
    ImmutableArray<GlossaryClaimPinState> Pins)
{
    private static readonly Measure DefaultTicks = new(12);

    public Point Start { get; init; } = SeedStart;

    public Point End { get; init; } = SeedEnd;

    public Placement Site => Carrier.At(NormalizeRatio(SiteRatio));

    public Measure CurrentValue => ValueAt(Site.Ratio);

    public GlossaryClaimCarrierState WithSite(Ratio ratio) => this with
    {
        SiteRatio = NormalizeRatio(ratio)
    };

    public GlossaryClaimCarrierState WithEndpoint(bool updateStart, Point value)
    {
        var clamped = ClampPoint(value);
        return updateStart
            ? this with { Start = clamped }
            : this with { End = clamped };
    }

    public GlossaryClaimCarrierState AddPin() => this with
    {
        Pins = Pins.Add(new GlossaryClaimPinState(Site, CurrentValue))
    };

    public GlossaryClaimCarrierState ClearPins() => this with
    {
        Pins = ImmutableArray<GlossaryClaimPinState>.Empty
    };

    public Measure ValueAt(Ratio ratio)
    {
        var normalized = NormalizeRatio(ratio);
        var span = EndValue.Raw - StartValue.Raw;
        var offset = span * normalized.Recessive.Raw / Math.Max(1L, normalized.Dominant.Raw);
        return new Measure(StartValue.Raw + offset);
    }

    private static Ratio NormalizeRatio(Ratio ratio)
    {
        var denominator = ratio.Dominant.Raw == 0 ? DefaultTicks.Raw : Math.Abs(ratio.Dominant.Raw);
        var numerator = Math.Clamp(ratio.Recessive.Raw, 0L, denominator);
        return new Ratio(new Measure(numerator), new Measure(denominator));
    }

    private static Point ClampPoint(Point point) => new(
        Math.Clamp(point.X, 0.06d, 0.94d),
        Math.Clamp(point.Y, 0.08d, 0.92d));
}

public readonly record struct GlossaryClaimPinState(Placement Placement, Measure Value);
