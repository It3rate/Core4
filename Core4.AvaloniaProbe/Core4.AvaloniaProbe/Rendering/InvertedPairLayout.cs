using System;
using Avalonia;
using Core4.AvaloniaProbe.Models;
using Core4.Numerics;

namespace Core4.AvaloniaProbe.Rendering;

/// <summary>
/// Fitted geometry for rendering one inverted pair scene into a target rectangle.
/// </summary>
public readonly record struct InvertedPairLayout(
    Rect Plot,
    InvertedPairDemoState State,
    double PixelsPerMeasure,
    double ZeroAnchorX,
    double AxisY,
    double DominantLaneY,
    double RecessiveLaneY)
{
    public Point OriginPoint => new(XFor(State.Origin), AxisY);

    public Point DominantUnitPoint => new(XFor(State.DominantUnitLocation), AxisY);

    public Point RecessiveUnitPoint => new(XFor(State.RecessiveUnitLocation), AxisY);

    public Point DominantValuePoint => new(XFor(State.DominantValueLocation), DominantLaneY);

    public Point RecessiveValuePoint => new(XFor(State.RecessiveValueLocation), RecessiveLaneY);

    public Point DominantLaneOrigin => new(XFor(State.Origin), DominantLaneY);

    public Point RecessiveLaneOrigin => new(XFor(State.Origin), RecessiveLaneY);

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
