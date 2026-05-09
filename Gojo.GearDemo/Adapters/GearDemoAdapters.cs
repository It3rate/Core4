using Avalonia;
using Gojo.Core;
using Gojo.GearDemo.Components;
using Gojo.GearDemo.Model;
using Gojo.GearDemo.Rendering;
using SkiaSharp;

namespace Gojo.GearDemo.Adapters;

internal static class GearDemoAdapters
{
    public static (long ProjectedTicks, long ResidueTicks) ReadProjection(Relation read)
    {
        if (read.TryGetCarrierLocations(out var projected, out var residue))
        {
            return (projected.Ticks, residue.Ticks);
        }

        return (0, 0);
    }

    public static int ToInt(Carrier carrier) =>
        checked((int)carrier.Value);

    public static int ToInt(long value) =>
        checked((int)value);

    public static long ToLong(Carrier carrier) =>
        checked((long)carrier.Value);

    public static ulong ToUlong(Carrier carrier) =>
        carrier.Value;

    public static int ReadCircularValue(Carrier circularPrototype, long ticks, int cycleTicks)
    {
        if (cycleTicks <= 0)
        {
            return 0;
        }

        var circular = new Carrier(circularPrototype.UnitShape);
        circular.Attach(new Relation(
            Relation.Point(circular, cycleTicks),
            Relation.Point(circular, 0)));
        var positiveTicks = ticks;
        if (positiveTicks < 0)
        {
            var cycles = checked((-positiveTicks / cycleTicks) + 1);
            positiveTicks = checked(positiveTicks + checked(cycles * cycleTicks));
        }

        circular.Add(checked((ulong)positiveTicks));
        return ToInt(circular);
    }

    public static int SliderValue(SliderLayout slider, Point point)
    {
        var t = slider.Track.Width <= 0d
            ? 0d
            : (point.X - slider.Track.X) / slider.Track.Width;
        var value = slider.Minimum + (Math.Clamp(t, 0d, 1d) * (slider.Maximum - slider.Minimum));
        return ToInt((long)Math.Round(value, MidpointRounding.AwayFromZero));
    }

    public static SliderLayout SliderLayout(
        double x,
        double y,
        double width,
        GojoSliderSnapshot slider,
        string label,
        string readout)
    {
        var boundaries = slider.TrackSegment.BoundariesOn(slider.Track);
        var bounds = new Rect(x, y, width, 42d);
        var track = new Rect(x + 4d, y + 26d, Math.Max(1d, width - 8d), 6d);
        var t = slider.Range.Ticks <= 0 ? 0d : slider.Position.Ticks / (double)slider.Range.Ticks;
        var knob = new Point(track.X + track.Width * t, track.Center.Y);
        return new SliderLayout(
            bounds,
            track,
            knob,
            boundaries.LeftLocation.Ticks,
            boundaries.RightLocation.Ticks,
            slider.Value.Ticks,
            label,
            readout);
    }

    public static GearProjectionRead ReadGearProjection(GearDemoInspection inspection)
    {
        var pair = inspection.Pair;
        var driverPerimeterRead = pair.DriverInputToPerimeter.Project(inspection.DriverInputTicks.ValueSlot, pair.Driver.Perimeter);
        var (driverPerimeterTicks, _) = ReadProjection(driverPerimeterRead);
        var driverPerimeterLocation = new CarrierLocation(pair.Driver.Perimeter, driverPerimeterTicks);
        var drivenPerimeterRead = pair.Mesh.Project(driverPerimeterLocation, pair.Driven.Perimeter);
        var (drivenProjectedTicks, projectionResidue) = ReadProjection(drivenPerimeterRead);
        var (driverToothPosition, _) = ReadProjection(pair.Driver.PerimeterToToothPosition.Project(driverPerimeterLocation, pair.Driver.ToothPosition));
        var (driverRotations, _) = ReadProjection(pair.Driver.PerimeterToRotation.Project(driverPerimeterLocation, pair.Driver.Rotations));
        var drivenProjectedLocation = new CarrierLocation(pair.Driven.Perimeter, drivenProjectedTicks);
        var (drivenProjectedToothPosition, _) = ReadProjection(pair.Driven.PerimeterToToothPosition.Project(drivenProjectedLocation, pair.Driven.ToothPosition));
        var (drivenProjectedRotations, _) = ReadProjection(pair.Driven.PerimeterToRotation.Project(drivenProjectedLocation, pair.Driven.Rotations));

        return new GearProjectionRead(
            ToLong(inspection.DriverInputTicks),
            driverPerimeterTicks,
            ReadCircularValue(pair.Driver.ToothPosition, driverToothPosition, pair.Driver.ToothCount),
            driverRotations,
            pair.Driver.Perimeter.Tension,
            drivenProjectedTicks,
            ReadCircularValue(pair.Driven.ToothPosition, drivenProjectedToothPosition, pair.Driven.ToothCount),
            drivenProjectedRotations,
            projectionResidue);
    }

    public static GearDemoRenderRead ToRenderRead(GearDemoInspection inspection)
    {
        var pair = inspection.Pair;
        var projection = ReadGearProjection(inspection);
        var speed = inspection.SpeedSlider.Read();
        var driverDiameterText = FractionalInches(pair.Driver.DiameterTwentyFourths, pair.Numbers.DiametralPitchTicks);
        var drivenDiameterText = FractionalInches(pair.Driven.DiameterTwentyFourths, pair.Numbers.DiametralPitchTicks);
        return new GearDemoRenderRead(
            ToInt(speed.Value.Ticks),
            pair.Driver.ToothCount,
            pair.Driven.ToothCount,
            pair.Driver.PerimeterTicks,
            pair.Driven.PerimeterTicks,
            pair.Numbers.HalfPitchTicksValue,
            pair.Numbers.QuarterPitchTicksValue,
            pair.DriverMeshPhaseTicks,
            pair.DrivenMeshPhaseTicks,
            Turns(projection.DriverInputTicks, pair.Driver.PerimeterTicks),
            Turns(projection.DrivenProjectedTicks, pair.Driven.PerimeterTicks),
            pair.Driver.RadiusFortyEighths / (double)pair.Numbers.RadiusTicksPerInchTicks,
            pair.Driven.RadiusFortyEighths / (double)pair.Numbers.RadiusTicksPerInchTicks,
            pair.Driver.RootRadiusFortyEighths / (double)pair.Numbers.RadiusTicksPerInchTicks,
            pair.Driven.RootRadiusFortyEighths / (double)pair.Numbers.RadiusTicksPerInchTicks,
            pair.Driver.TipRadiusFortyEighths / (double)pair.Numbers.RadiusTicksPerInchTicks,
            pair.Driven.TipRadiusFortyEighths / (double)pair.Numbers.RadiusTicksPerInchTicks,
            pair.CenterLineDistanceFortyEighths / (double)pair.Numbers.RadiusTicksPerInchTicks,
            MeshLabel(pair.Numbers.HalfPitchTicksValue),
            $"{driverDiameterText} / {drivenDiameterText}",
            $"segment 0 -> {pair.CenterLineDistanceFortyEighths}/48 in",
            $"ticks {projection.DriverInputTicks}, tooth {projection.DriverToothPosition}/{pair.Driver.ToothCount}, rotations {projection.DriverRotations}",
            $"ticks {projection.DrivenProjectedTicks}, tooth {projection.DrivenProjectedToothPosition}/{pair.Driven.ToothCount}, rotations {projection.DrivenProjectedRotations}",
            $"{projection.DriverTension} -> {projection.ProjectionResidueTicks}",
            $"Driver {pair.Driver.ToothCount}T / dia {driverDiameterText}",
            $"value {projection.DriverPerimeterTicks}/{pair.Driver.PerimeterTicks}, rotations {projection.DriverRotations}, tension {projection.DriverTension}",
            $"Driven {pair.Driven.ToothCount}T / dia {drivenDiameterText}",
            $"projected {projection.DrivenProjectedTicks}/{pair.Driven.PerimeterTicks}, rotations {projection.DrivenProjectedRotations}, residue {projection.ProjectionResidueTicks}");
    }

    public static string MeshLabel(int drivenPhaseTicks) =>
        $"{GearDemoDefaults.TicksPerTooth} ticks/tooth, driven phase +{drivenPhaseTicks}";

    public static double PhaseTurns(long ticks, int perimeterTicks) =>
        perimeterTicks == 0 ? 0d : ticks / (double)perimeterTicks;

    public static double GearAngle(GearLayout gear, double localTurns) =>
        -Math.PI * 0.5d + ((gear.Turns + gear.PhaseTurns + localTurns) * Math.PI * 2d);

    public static Point Polar(Point center, double radius, double angle) =>
        new(center.X + Math.Cos(angle) * radius, center.Y + Math.Sin(angle) * radius);

    public static Point Interpolate(Point from, Point to, double t) =>
        new(from.X + (to.X - from.X) * t, from.Y + (to.Y - from.Y) * t);

    public static SKRect ToRect(Rect rect) =>
        new(ToFloat(rect.X), ToFloat(rect.Y), ToFloat(rect.Right), ToFloat(rect.Bottom));

    public static SKPoint ToPoint(Point point) =>
        new(ToFloat(point.X), ToFloat(point.Y));

    public static float ToFloat(double value) =>
        checked((float)value);

    private static double Turns(long ticks, int perimeterTicks) =>
        perimeterTicks == 0 ? 0d : ticks / (double)perimeterTicks;

    private static string FractionalInches(int numerator, int denominator) =>
        FractionalTicks(numerator, denominator, "in");

    private static string FractionalTicks(long numerator, int denominator, string suffix)
    {
        var sign = numerator < 0 ? "-" : string.Empty;
        var magnitude = Math.Abs(numerator);
        var whole = magnitude / denominator;
        var remainder = magnitude % denominator;
        if (remainder == 0)
        {
            return $"{sign}{whole} {suffix}";
        }

        var divisor = GreatestCommonDivisor(ToInt(remainder), denominator);
        var fraction = $"{remainder / divisor}/{denominator / divisor}";
        return whole == 0
            ? $"{sign}{fraction} {suffix}"
            : $"{sign}{whole} {fraction} {suffix}";
    }

    private static int GreatestCommonDivisor(int left, int right)
    {
        left = Math.Abs(left);
        right = Math.Abs(right);
        while (right != 0)
        {
            var next = left % right;
            left = right;
            right = next;
        }

        return left == 0 ? 1 : left;
    }
}

internal readonly record struct GearDemoRenderRead(
    int SpeedTicksPerSecond,
    int DriverTeeth,
    int DrivenTeeth,
    int DriverPerimeterTicks,
    int DrivenPerimeterTicks,
    int HalfPitchTicks,
    int QuarterPitchTicks,
    long DriverMeshPhaseTicks,
    long DrivenMeshPhaseTicks,
    double DriverTurns,
    double DrivenProjectedTurns,
    double DriverRadiusInches,
    double DrivenRadiusInches,
    double DriverRootRadiusInches,
    double DrivenRootRadiusInches,
    double DriverTipRadiusInches,
    double DrivenTipRadiusInches,
    double CenterLineDistanceInches,
    string MeshLabel,
    string DiameterReadout,
    string CenterReadout,
    string DriverReadout,
    string DrivenReadout,
    string TensionReadout,
    string DriverCaption,
    string DriverState,
    string DrivenCaption,
    string DrivenState);

internal readonly record struct GearProjectionRead(
    long DriverInputTicks,
    long DriverPerimeterTicks,
    int DriverToothPosition,
    long DriverRotations,
    long DriverTension,
    long DrivenProjectedTicks,
    int DrivenProjectedToothPosition,
    long DrivenProjectedRotations,
    long ProjectionResidueTicks);
