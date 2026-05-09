using Avalonia;
using Gojo.GearDemo.Components;

namespace Gojo.GearDemo.Rendering;

internal readonly record struct GearDemoLayout(
    Rect Bounds,
    Rect Scene,
    Rect Side,
    Rect PlayButton,
    Rect ResetButton,
    Rect WebButton,
    SliderLayout SpeedSlider,
    SliderLayout DriverTeethSlider,
    SliderLayout DrivenTeethSlider,
    GearLayout DriverGear,
    GearLayout DrivenGear);

internal readonly record struct GearLayout(
    Point Center,
    double Radius,
    double RootRadius,
    double TipRadius,
    int ToothCount,
    double Turns,
    double PhaseTurns,
    double RootHalfTurns,
    double TipHalfTurns,
    string Caption,
    string State);
