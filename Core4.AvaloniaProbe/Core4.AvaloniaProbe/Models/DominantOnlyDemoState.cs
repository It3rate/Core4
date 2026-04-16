using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Models;

/// <summary>
/// A tiny dominant-only demo state. The stored values are only the movable measures; the actual rendered structure
/// is always rebuilt as a Core4 basis plus a live site.
/// </summary>
public readonly record struct DominantOnlyDemoState(Measure Origin, Measure DominantSpan, Measure ValueOffset)
{
    private static readonly Carrier AxisCarrier = new(1);

    public static DominantOnlyDemoState Default => new(Measure.Zero, new Measure(4), new Measure(10));

    public Carrier Carrier => AxisCarrier;

    public Site OriginSite => Site.Open(Carrier, Origin);

    public Site UnitSite => Site.Open(Carrier, UnitLocation);

    public Site ValueSite => Site.Open(Carrier, ValueLocation);

    public Basis Basis => Core4.Aliases.Basis.Freeze(OriginSite, UnitSite);

    public Ratio Reading => Basis.Project(ValueSite);

    public Ratio DisplayReading => IsHiddenInverse ? Ratio.Tension(ValueOffset.Absolute) : Reading;

    public Measure UnitLocation => Origin + DominantSpan;

    public Measure ValueLocation => Origin + ValueOffset;

    public bool IsHiddenInverse => ValueOffset.Raw < 0;

    public DominantOnlyDemoState WithOrigin(Measure origin) => this with { Origin = origin };

    public DominantOnlyDemoState WithDominantSpan(Measure span)
    {
        var normalized = span.Absolute;
        return this with { DominantSpan = normalized.Raw == 0 ? Measure.One : normalized };
    }

    public DominantOnlyDemoState WithValueOffset(Measure offset) => this with { ValueOffset = offset };

    public DominantOnlyDemoState WithUnitLocation(Measure location) => WithDominantSpan(location - Origin);

    public DominantOnlyDemoState WithValueLocation(Measure location) => WithValueOffset(location - Origin);
}
