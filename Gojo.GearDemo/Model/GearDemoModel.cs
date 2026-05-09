using Gojo.Core;
using Gojo.GearDemo.Components;

namespace Gojo.GearDemo.Model;

internal sealed class GearDemoModel
{
    public const int TicksPerTooth = GearDemoDefaults.TicksPerTooth;

    private readonly GearNumbers numbers;
    private readonly GojoSlider speedSlider;
    private readonly GojoSlider driverTeethSlider;
    private readonly GojoSlider drivenTeethSlider;
    private readonly GojoClockDrive clockDrive;
    private readonly Carrier driverInputTicks = new();
    private GearPair pair;

    public GearDemoModel()
    {
        numbers = GearNumbers.Create(
            GearDemoDefaults.TicksPerTooth,
            GearDemoDefaults.DiametralPitch,
            GearDemoDefaults.ToothProfileRadiusTicks);
        speedSlider = GojoSlider.Create(0, GearDemoDefaults.MaximumSpeedTicksPerSecond, 16);
        driverTeethSlider = GojoSlider.Create(GearDemoDefaults.MinimumTeeth, GearDemoDefaults.MaximumTeeth, 24);
        drivenTeethSlider = GojoSlider.Create(GearDemoDefaults.MinimumTeeth, GearDemoDefaults.MaximumTeeth, 16);
        clockDrive = new GojoClockDrive(GearDemoDefaults.ClockTicksPerSecond);
        pair = CreatePair();
    }

    public int DriverTeeth => pair.Driver.ToothCount;

    public int DrivenTeeth => pair.Driven.ToothCount;

    public void Drive(long ticks)
    {
        if (ticks <= 0)
        {
            return;
        }

        pair.Driver.Perimeter.Add(checked((ulong)ticks));
        driverInputTicks.Add(checked((ulong)ticks));
    }

    public void AdvanceClock(long elapsedMilliseconds) =>
        clockDrive.Advance(elapsedMilliseconds, speedSlider.Track, Drive);

    public void Reset()
    {
        pair = CreatePair();
        driverInputTicks.Set(0);
        clockDrive.Reset();
    }

    public void SetSpeedTicksPerSecond(int ticksPerSecond) =>
        speedSlider.SetPosition(ticksPerSecond);

    public void SetSpeedSliderPosition(int ticks) =>
        speedSlider.SetPosition(ticks);

    public void SetDriverTeeth(int toothCount) =>
        driverTeethSlider.SetPosition(toothCount);

    public void SetDriverTeethSliderPosition(int ticks) =>
        driverTeethSlider.SetPosition(ticks);

    public void SetDrivenTeeth(int toothCount) =>
        drivenTeethSlider.SetPosition(toothCount);

    public void SetDrivenTeethSliderPosition(int ticks) =>
        drivenTeethSlider.SetPosition(ticks);

    public GearDemoInspection Inspect() =>
        new(speedSlider, driverTeethSlider, drivenTeethSlider, clockDrive, driverInputTicks, pair);

    private GearPair CreatePair() =>
        GearPair.Create(
            driverTeethSlider.Track,
            drivenTeethSlider.Track,
            driverInputTicks,
            numbers,
            GearDemoDefaults.CenterClearanceRadiusTicks);
}
