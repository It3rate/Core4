using Gojo.Core;
using Gojo.GearDemo.Components;

namespace Gojo.GearDemo.Model;

internal readonly record struct GearDemoInspection(
    GojoSlider SpeedSlider,
    GojoSlider DriverTeethSlider,
    GojoSlider DrivenTeethSlider,
    GojoClockDrive ClockDrive,
    Carrier DriverInputTicks,
    GearPair Pair);
