using System;
using Avalonia;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Rendering;

/// <summary>
/// A fitted draw layout for one dominant-only basis view. It is derived entirely from the basis, the live site,
/// and the target rectangle.
/// </summary>
public readonly record struct DominantOnlyLayout(
    Rect Plot,
    Relation Relation,
    Site ValueSite,
    Ratio Reading,
    double PixelsPerMeasure,
    double ZeroAnchorX,
    double AxisY,
    double ValueLaneY)
{
    public Measure OriginLocation => Relation.Origin.Location;

    public Measure UnitLocation => Relation.Anchor.Location;

    public Measure ValueLocation => ValueSite.Location;

    public Measure UnitSpan => Relation.Potential.Raw == 0 ? Measure.One : Relation.Potential;

    public bool IsHiddenInverse => Relation.Orientation.Raw > 0
        ? ValueLocation.Raw < OriginLocation.Raw
        : ValueLocation.Raw > OriginLocation.Raw;

    public Point OriginPoint => new(XFor(OriginLocation), AxisY);

    public Point UnitPoint => new(XFor(UnitLocation), AxisY);

    public Point ValuePoint => new(XFor(ValueLocation), ValueLaneY);

    public Point ValueLaneOrigin => new(XFor(OriginLocation), ValueLaneY);

    public Rect HiddenZone => new(
        Math.Min(ValuePoint.X, OriginPoint.X) - 26d,
        ValueLaneY - 40d,
        Math.Abs(ValuePoint.X - OriginPoint.X) + 52d,
        (AxisY - ValueLaneY) + 70d);

    public double XFor(Measure measure) => ZeroAnchorX + (measure.Raw * PixelsPerMeasure);

    public Measure MeasureAt(double x)
    {
        if (PixelsPerMeasure <= 0d)
        {
            return Measure.Zero;
        }

        var world = (x - ZeroAnchorX) / PixelsPerMeasure;
        return new Measure((long)Math.Round(world, MidpointRounding.AwayFromZero));
    }
}
