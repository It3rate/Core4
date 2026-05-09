using Avalonia;

namespace Gojo.GearDemo.Components;

internal readonly record struct SliderLayout(
    Rect Bounds,
    Rect Track,
    Point Knob,
    double Minimum,
    double Maximum,
    double Value,
    string Label,
    string Readout);

internal readonly record struct GearDemoInteraction(
    bool IsPlaying,
    Point? HoverPoint,
    string? WebButtonText,
    string? WebStatusText);
