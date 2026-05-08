using Gojo.GearDemo.Model;

namespace Gojo.Tests.GearDemo;

public sealed class GearDemoModelTests
{
    [Fact]
    public void AdvanceClockPreservesSubTickResidueWithoutAmplifyingMilliseconds()
    {
        var model = new GearDemoModel();
        model.SetSpeedTicksPerSecond(2);

        for (var frame = 0; frame < 60; frame++)
        {
            model.AdvanceClock(16);
        }

        var snapshot = model.Read();
        Assert.Equal(1, snapshot.DriverInputTicks);
        Assert.Equal(920, snapshot.ClockDriveNumeratorResidue);

        model.AdvanceClock(40);

        snapshot = model.Read();
        Assert.Equal(2, snapshot.DriverInputTicks);
        Assert.Equal(0, snapshot.ClockDriveNumeratorResidue);
    }

    [Fact]
    public void AdvanceClockAtOneTickPerSecondNeedsOneFullSecond()
    {
        var model = new GearDemoModel();
        model.SetSpeedTicksPerSecond(1);

        model.AdvanceClock(999);

        Assert.Equal(0, model.Read().DriverInputTicks);

        model.AdvanceClock(1);

        Assert.Equal(1, model.Read().DriverInputTicks);
    }

    [Fact]
    public void GearReadsToothPositionAndProjectedRotationThroughRelations()
    {
        var model = new GearDemoModel();

        model.Drive(GearDemoModel.TicksPerTooth);

        var snapshot = model.Read();
        Assert.Equal(1, snapshot.DriverToothPosition);
        Assert.Equal(snapshot.DrivenTeeth - 1, snapshot.DrivenProjectedToothPosition);

        model.Drive(15 * GearDemoModel.TicksPerTooth);

        snapshot = model.Read();
        Assert.Equal(0, snapshot.DrivenProjectedToothPosition);
        Assert.Equal(-1, snapshot.DrivenProjectedRotations);
    }

    [Fact]
    public void DemoScaleValuesAreReadFromGojoCarriers()
    {
        var model = new GearDemoModel();

        var snapshot = model.Read();

        Assert.Equal(96, snapshot.SpeedSlider.Range);
        Assert.Equal(24, snapshot.DiametralPitchTicks);
        Assert.Equal(48, snapshot.RadiusTicksPerInchTicks);
        Assert.Equal(1, snapshot.ToothProfileRadiusTicks);
        Assert.Equal(4, snapshot.HalfPitchTicks);
        Assert.Equal(192, snapshot.DriverPerimeterTicks);
        Assert.NotNull(snapshot.SpeedSlider.RangeOperation);
    }
}
