using System;
using System.Collections.Immutable;
using System.Linq;
using Avalonia;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Models;

/// <summary>
/// Scene state for the first glossary claim page. Carrier geometry is visual, while site and measure positions stay
/// on real Core4 carriers as placements expressed by ratios.
/// </summary>
public readonly record struct GlossaryClaimDemoState(ImmutableArray<GlossaryClaimCarrierState> Carriers)
{
    private static readonly Measure DefaultTicks = new(12);

    public static GlossaryClaimDemoState Default { get; } =
        new(
        [
            new GlossaryClaimCarrierState(
                new Carrier(1),
                new Point(0.58d, 0.18d),
                new Point(0.90d, 0.34d),
                new Ratio(2, DefaultTicks),
                ImmutableArray<Placement>.Empty),
            new GlossaryClaimCarrierState(
                new Carrier(2),
                new Point(0.18d, 0.82d),
                new Point(0.26d, 0.16d),
                new Ratio(7, DefaultTicks),
                ImmutableArray<Placement>.Empty),
            new GlossaryClaimCarrierState(
                new Carrier(3),
                new Point(0.46d, 0.78d),
                new Point(0.86d, 0.62d),
                new Ratio(9, DefaultTicks),
                ImmutableArray<Placement>.Empty)
        ]);

    public GlossaryClaimCarrierState CarrierAt(int index) => Carriers[index];

    public GlossaryClaimDemoState WithEndpoint(int index, bool updateStart, Point value)
        => UpdateCarrier(index, carrier => carrier.WithEndpoint(updateStart, value));

    public GlossaryClaimDemoState WithSiteTicks(int index, long numerator, long denominator = 12L)
        => UpdateCarrier(index, carrier => carrier.WithSite(new Ratio(new Measure(numerator), new Measure(Math.Max(1L, denominator)))));

    public GlossaryClaimDemoState AddMeasure(int index)
        => UpdateCarrier(index, carrier => carrier.AddMeasure());

    public GlossaryClaimDemoState ClearMeasures()
        => new(Carriers.Select(static carrier => carrier.ClearMeasures()).ToImmutableArray());

    public GlossaryClaimDemoState ResetLayout()
        => new(Carriers.Select(static carrier => carrier.ResetLayout()).ToImmutableArray());

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
}

public readonly record struct GlossaryClaimCarrierState(
    Carrier Carrier,
    Point SeedStart,
    Point SeedEnd,
    Ratio SiteRatio,
    ImmutableArray<Placement> Measures)
{
    private static readonly Measure DefaultTicks = new(12);

    public Point Start { get; init; } = SeedStart;

    public Point End { get; init; } = SeedEnd;

    public Placement Site => Carrier.At(NormalizeRatio(SiteRatio));

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

    public GlossaryClaimCarrierState AddMeasure() => this with
    {
        Measures = Measures.Add(Site)
    };

    public GlossaryClaimCarrierState ClearMeasures() => this with
    {
        Measures = ImmutableArray<Placement>.Empty
    };

    public GlossaryClaimCarrierState ResetLayout() => this with
    {
        Start = SeedStart,
        End = SeedEnd
    };

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
