using Gojo.Core;
using Gojo.GearDemo.Adapters;
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
        clockDrive.Advance(elapsedMilliseconds, speedSlider.Output, Drive);

    public void Reset()
    {
        pair = CreatePair();
        driverInputTicks.Set(0);
        clockDrive.Reset();
    }

    public void SetSpeedTicksPerSecond(int ticksPerSecond) =>
        speedSlider.SetOutput(ticksPerSecond);

    public void SetDriverTeeth(int toothCount)
    {
        driverTeethSlider.SetOutput(toothCount);
        RebuildPair();
    }

    public void SetDrivenTeeth(int toothCount)
    {
        drivenTeethSlider.SetOutput(toothCount);
        RebuildPair();
    }

    public GearDemoSnapshot Read()
    {
        var clockRead = clockDrive.Read();
        var driverPerimeterRead = pair.DriverInputToPerimeter.Project(driverInputTicks.ValueSlot, pair.Driver.Perimeter);
        var (driverPerimeterTicks, _) = GearDemoAdapters.ReadProjection(driverPerimeterRead);
        var totalRead = pair.Mesh.Project(new CarrierLocation(pair.Driver.Perimeter, driverPerimeterTicks), pair.Driven.Perimeter);
        var visibleRead = pair.Mesh.Project(pair.Driver.Perimeter.ValueSlot, pair.Driven.Perimeter);
        var (projectedTicks, projectionResidue) = GearDemoAdapters.ReadProjection(totalRead);
        var (visibleProjectedTicks, visibleResidue) = GearDemoAdapters.ReadProjection(visibleRead);
        var (driverToothPosition, _) = GearDemoAdapters.ReadProjection(pair.Driver.PerimeterToToothPosition.Project(pair.Driver.Perimeter.ValueSlot, pair.Driver.ToothPosition));
        var drivenProjectedLocation = new CarrierLocation(pair.Driven.Perimeter, projectedTicks);
        var (drivenProjectedToothPosition, _) = GearDemoAdapters.ReadProjection(pair.Driven.PerimeterToToothPosition.Project(drivenProjectedLocation, pair.Driven.ToothPosition));
        var (drivenProjectedRotations, _) = GearDemoAdapters.ReadProjection(pair.Driven.PerimeterToRotation.Project(drivenProjectedLocation, pair.Driven.Rotations));
        var driverCircularToothPosition = GearDemoAdapters.ReadCircularValue(pair.Driver.ToothPosition, driverToothPosition, pair.Driver.ToothCount);
        var drivenCircularToothPosition = GearDemoAdapters.ReadCircularValue(pair.Driven.ToothPosition, drivenProjectedToothPosition, pair.Driven.ToothCount);

        return new GearDemoSnapshot(
            speedSlider.Read(),
            driverTeethSlider.Read(),
            drivenTeethSlider.Read(),
            clockRead.ElapsedMilliseconds,
            clockRead.ProjectedDriveTicks,
            clockRead.DriveNumeratorResidue,
            pair.Driver.ToothCount,
            pair.Driven.ToothCount,
            pair.Driver.PerimeterTicks,
            pair.Driven.PerimeterTicks,
            GearDemoAdapters.ToLong(driverInputTicks),
            GearDemoAdapters.ToUlong(pair.Driver.Perimeter),
            driverCircularToothPosition,
            pair.Driver.Perimeter.Tension,
            GearDemoAdapters.ToUlong(pair.Driver.Rotations),
            projectedTicks,
            drivenCircularToothPosition,
            drivenProjectedRotations,
            projectionResidue,
            visibleProjectedTicks,
            visibleResidue,
            GearDemoAdapters.ToUlong(pair.Driven.Perimeter),
            pair.Driven.Perimeter.Tension,
            GearDemoAdapters.ToUlong(pair.Driven.Rotations),
            pair.Driver.DiameterTwentyFourths,
            pair.Driven.DiameterTwentyFourths,
            pair.Driver.RadiusFortyEighths,
            pair.Driven.RadiusFortyEighths,
            pair.Driver.RootRadiusFortyEighths,
            pair.Driven.RootRadiusFortyEighths,
            pair.Driver.TipRadiusFortyEighths,
            pair.Driven.TipRadiusFortyEighths,
            pair.CenterLineDistanceFortyEighths,
            pair.Numbers.DiametralPitchTicks,
            pair.Numbers.RadiusTicksPerInchTicks,
            pair.Numbers.ToothProfileRadiusTicksValue,
            pair.Numbers.HalfPitchTicksValue,
            pair.Numbers.QuarterPitchTicksValue,
            pair.DriverMeshPhaseTicks,
            pair.DrivenMeshPhaseTicks,
            pair.CenterLineSegment);
    }

    private void RebuildPair()
    {
        var ticks = GearDemoAdapters.ToLong(driverInputTicks);
        pair = CreatePair();
        pair.Driver.Perimeter.Add(checked((ulong)ticks));
    }

    private GearPair CreatePair() =>
        GearPair.Create(
            driverTeethSlider.Output,
            drivenTeethSlider.Output,
            driverInputTicks,
            numbers,
            GearDemoDefaults.CenterClearanceRadiusTicks);
}
