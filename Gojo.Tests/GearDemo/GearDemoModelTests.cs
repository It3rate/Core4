using Gojo.GearDemo.Adapters;
using Gojo.GearDemo.Components;
using Gojo.GearDemo.Model;
using Gojo.Core;

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

        var inspection = model.Inspect();
        Assert.Equal(1, GearDemoAdapters.ToLong(inspection.DriverInputTicks));
        Assert.Equal(920, inspection.ClockDrive.Read().DriveNumeratorResidue);

        model.AdvanceClock(40);

        inspection = model.Inspect();
        Assert.Equal(2, GearDemoAdapters.ToLong(inspection.DriverInputTicks));
        Assert.Equal(0, inspection.ClockDrive.Read().DriveNumeratorResidue);
    }

    [Fact]
    public void AdvanceClockAtOneTickPerSecondNeedsOneFullSecond()
    {
        var model = new GearDemoModel();
        model.SetSpeedTicksPerSecond(1);

        model.AdvanceClock(999);

        Assert.Equal(0, GearDemoAdapters.ToLong(model.Inspect().DriverInputTicks));

        model.AdvanceClock(1);

        Assert.Equal(1, GearDemoAdapters.ToLong(model.Inspect().DriverInputTicks));
    }

    [Fact]
    public void GearReadsToothPositionAndProjectedRotationThroughRelations()
    {
        var model = new GearDemoModel();

        model.Drive(GearDemoModel.TicksPerTooth);

        var inspection = model.Inspect();
        var projection = GearDemoAdapters.ReadGearProjection(inspection);
        Assert.Equal(1, projection.DriverToothPosition);
        Assert.Equal(inspection.Pair.Driven.ToothCount - 1, projection.DrivenProjectedToothPosition);

        model.Drive(15 * GearDemoModel.TicksPerTooth);

        inspection = model.Inspect();
        projection = GearDemoAdapters.ReadGearProjection(inspection);
        Assert.Equal(0, projection.DrivenProjectedToothPosition);
        Assert.Equal(-1, projection.DrivenProjectedRotations);
    }

    [Fact]
    public void DemoScaleValuesAreReadFromGojoCarriers()
    {
        var model = new GearDemoModel();

        var inspection = model.Inspect();
        var pair = inspection.Pair;

        Assert.Equal(96, inspection.SpeedSlider.Read().Range.Ticks);
        Assert.Equal(24, pair.Numbers.DiametralPitchTicks);
        Assert.Equal(48, pair.Numbers.RadiusTicksPerInchTicks);
        Assert.Equal(1, pair.Numbers.ToothProfileRadiusTicksValue);
        Assert.Equal(4, pair.Numbers.HalfPitchTicksValue);
        Assert.Equal(2, pair.Numbers.QuarterPitchTicksValue);
        Assert.Equal(2, GearDemoAdapters.ReadProjection(
            pair.Numbers.HalfPitchToQuarterPitch.Project(pair.Numbers.HalfPitchTicks.ValueSlot, pair.Numbers.QuarterPitchTicks)).ProjectedTicks);
        Assert.Equal(0, pair.DriverMeshPhaseTicks);
        Assert.Equal(pair.Numbers.HalfPitchTicksValue, pair.DrivenMeshPhaseTicks);
        Assert.Equal(192, pair.Driver.PerimeterTicks);
        Assert.Equal(24, pair.Driver.RadiusFortyEighths);
        Assert.Equal(23, pair.Driver.RootRadiusFortyEighths);
        Assert.Equal(25, pair.Driver.TipRadiusFortyEighths);
        Assert.Equal("8 ticks/tooth, driven phase +4", GearDemoAdapters.MeshLabel(pair.Numbers.HalfPitchTicksValue));
        Assert.NotNull(inspection.SpeedSlider.TrackSegment);
    }

    [Fact]
    public void GearMechanismKeepsDerivedCircularStructureConnectedToSourceTicks()
    {
        var model = new GearDemoModel();
        var pair = model.Inspect().Pair;
        var driver = pair.Driver;

        Assert.Same(driver.Perimeter, driver.PitchCircle);
        Assert.True(driver.PitchCircleSource.LeftLocation.SameTickRefAs(driver.RadiusFortyEighthsCarrier.ValueSlot));
        Assert.True(driver.RootCircleSource.LeftLocation.SameTickRefAs(driver.RootRadiusFortyEighthsCarrier.ValueSlot));
        Assert.True(driver.TipCircleSource.LeftLocation.SameTickRefAs(driver.TipRadiusFortyEighthsCarrier.ValueSlot));
        Assert.True(((Relation)driver.Partition.Left).LeftLocation.SameTickRefAs(driver.PerimeterTicksCarrier.ValueSlot));
        Assert.True(((Relation)driver.ToothPartition.Left).LeftLocation.SameTickRefAs(driver.ToothCountCarrier.ValueSlot));
        Assert.True(((Relation)pair.CenterLineSegment.Right).LeftLocation.SameTickRefAs(pair.CenterDistance.ValueSlot));

        AssertContainsCarrier(driver.Structure, driver.ToothCountCarrier);
        AssertContainsCarrier(driver.Structure, driver.ShapeCenter);
        AssertContainsCarrier(driver.Structure, driver.Perimeter);
        AssertContainsCarrier(driver.Structure, driver.RootCircle);
        AssertContainsCarrier(driver.Structure, driver.TipCircle);
        AssertContainsCarrier(pair.Structure, pair.Driver.Perimeter);
        AssertContainsCarrier(pair.Structure, pair.Driven.Perimeter);
        AssertContainsCarrier(pair.Structure, pair.CenterLine);
        AssertContainsCarrier(pair.Structure, pair.CenterDistance);
    }

    [Fact]
    public void GojoSliderSitsOnTheCarrierItRepresents()
    {
        var toothCount = new Carrier();
        toothCount.Set(24);
        var slider = GojoSlider.Create(toothCount, 6, 48);

        var snapshot = slider.Read();
        var boundaries = snapshot.TrackSegment.BoundariesOn(snapshot.Track);

        Assert.Same(toothCount, slider.Track);
        Assert.Same(snapshot.Track, snapshot.Range.Carrier);
        Assert.Same(snapshot.Track, snapshot.Position.Carrier);
        Assert.Same(snapshot.Track, snapshot.Value.Carrier);
        Assert.True(snapshot.PositionSegment.RightLocation.SameTickRefAs(toothCount.ValueSlot));
        Assert.Equal(6, boundaries.LeftLocation.Ticks);
        Assert.Equal(48, boundaries.RightLocation.Ticks);
        Assert.Equal(42, snapshot.Range.Ticks);
        Assert.Equal(18, snapshot.Position.Ticks);
        Assert.Equal(24, snapshot.Value.Ticks);
        Assert.All(snapshot.TrackSegment.CarrierLocations(), location => Assert.Same(snapshot.Track, location.Carrier));
        Assert.All(snapshot.PositionSegment.CarrierLocations(), location => Assert.Same(snapshot.Track, location.Carrier));
    }

    [Fact]
    public void GojoSliderValueSlotUsesTheRepresentedCarrierTickRef()
    {
        var represented = new Carrier();
        var valueLocation = represented.ValueSlot;
        var slider = GojoSlider.Create(represented, 0, 100);

        slider.SetPosition(25);
        var snapshot = slider.Read();

        Assert.Equal(25, valueLocation.Ticks);
        Assert.Equal(25, represented.ValueSlot.Ticks);
        Assert.Equal(25, snapshot.Value.Ticks);
        Assert.Equal(25, snapshot.Position.Ticks);

        represented.Set(80);
        snapshot = slider.Read();

        Assert.Equal(80, valueLocation.Ticks);
        Assert.Equal(80, snapshot.Value.Ticks);
        Assert.Equal(80, snapshot.Position.Ticks);
    }

    private static void AssertContainsCarrier(Relation structure, Carrier carrier) =>
        Assert.Contains(carrier, structure.Carriers());
}
