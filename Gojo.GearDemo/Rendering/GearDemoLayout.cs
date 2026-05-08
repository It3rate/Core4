using Avalonia;

namespace Gojo.GearDemo.Rendering;

internal readonly record struct GearDemoLayout(
    Rect Bounds,
    Rect Scene,
    Rect Side,
    Rect PlayButton,
    Rect ResetButton,
    SliderLayout SpeedSlider,
    SliderLayout DriverTeethSlider,
    SliderLayout DrivenTeethSlider,
    GearLayout DriverGear,
    GearLayout DrivenGear);

internal readonly record struct SliderLayout(
    Rect Bounds,
    Rect Track,
    Point Knob,
    double Minimum,
    double Maximum,
    double Value,
    string Label,
    string Readout);

internal readonly record struct GearLayout(
    Point Center,
    double Radius,
    double RootRadius,
    double TipRadius,
    int ToothCount,
    double Turns,
    string Caption,
    string State);
