using Gojo.GearDemo.Adapters;
using Gojo.GearDemo.Components;
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

        Assert.Equal(96, snapshot.SpeedSlider.Range.Ticks);
        Assert.Equal(24, snapshot.DiametralPitchTicks);
        Assert.Equal(48, snapshot.RadiusTicksPerInchTicks);
        Assert.Equal(1, snapshot.ToothProfileRadiusTicks);
        Assert.Equal(4, snapshot.HalfPitchTicks);
        Assert.Equal(2, snapshot.QuarterPitchTicks);
        Assert.Equal(0, snapshot.DriverMeshPhaseTicks);
        Assert.Equal(snapshot.HalfPitchTicks, snapshot.DrivenMeshPhaseTicks);
        Assert.Equal(192, snapshot.DriverPerimeterTicks);
        Assert.Equal(24, snapshot.DriverRadiusFortyEighths);
        Assert.Equal(23, snapshot.DriverRootRadiusFortyEighths);
        Assert.Equal(25, snapshot.DriverTipRadiusFortyEighths);
        Assert.Equal("8 ticks/tooth, driven phase +4", GearDemoAdapters.MeshLabel(snapshot.HalfPitchTicks));
        Assert.NotNull(snapshot.SpeedSlider.RangeRelation);
    }

    [Fact]
    public void GojoSliderUsesOneCarrierAsTrackValueAndRangeBasis()
    {
        var slider = GojoSlider.Create(6, 48, 24);

        var snapshot = slider.Read();

        Assert.Same(snapshot.Track, slider.Output);
        Assert.Same(snapshot.Track, snapshot.Minimum.Carrier);
        Assert.Same(snapshot.Track, snapshot.Maximum.Carrier);
        Assert.Same(snapshot.Track, snapshot.Range.Carrier);
        Assert.Same(snapshot.Track, snapshot.Position.Carrier);
        Assert.Same(snapshot.Track, snapshot.Value.Carrier);
        Assert.Equal(6, snapshot.Minimum.Ticks);
        Assert.Equal(48, snapshot.Maximum.Ticks);
        Assert.Equal(42, snapshot.Range.Ticks);
        Assert.Equal(18, snapshot.Position.Ticks);
        Assert.Equal(24, snapshot.Value.Ticks);
        Assert.All(snapshot.TrackSegment.CarrierLocations(), location => Assert.Same(snapshot.Track, location.Carrier));
        Assert.All(snapshot.RangeRelation.CarrierLocations(), location => Assert.Same(snapshot.Track, location.Carrier));
    }
}
