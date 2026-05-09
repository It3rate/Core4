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
        var bounds = new Rect(x, y, width, 42d);
        var track = new Rect(x + 4d, y + 26d, Math.Max(1d, width - 8d), 6d);
        var t = slider.Range.Ticks <= 0 ? 0d : slider.Position.Ticks / (double)slider.Range.Ticks;
        var knob = new Point(track.X + track.Width * t, track.Center.Y);
        return new SliderLayout(bounds, track, knob, slider.Minimum.Ticks, slider.Maximum.Ticks, slider.Value.Ticks, label, readout);
    }

    public static GearDemoRenderRead ToRenderRead(GearDemoSnapshot snapshot)
    {
        var driverDiameterText = FractionalInches(snapshot.DriverDiameterTwentyFourths, snapshot.DiametralPitchTicks);
        var drivenDiameterText = FractionalInches(snapshot.DrivenDiameterTwentyFourths, snapshot.DiametralPitchTicks);
        return new GearDemoRenderRead(
            ToInt(snapshot.SpeedSlider.Value.Ticks),
            Turns(snapshot.DriverInputTicks, snapshot.DriverPerimeterTicks),
            Turns(snapshot.DrivenProjectedTicks, snapshot.DrivenPerimeterTicks),
            Turns(snapshot.VisibleDrivenProjectedTicks, snapshot.DrivenPerimeterTicks),
            snapshot.DriverDiameterTwentyFourths / (double)snapshot.DiametralPitchTicks,
            snapshot.DrivenDiameterTwentyFourths / (double)snapshot.DiametralPitchTicks,
            snapshot.DriverRadiusFortyEighths / (double)snapshot.RadiusTicksPerInchTicks,
            snapshot.DrivenRadiusFortyEighths / (double)snapshot.RadiusTicksPerInchTicks,
            snapshot.DriverRootRadiusFortyEighths / (double)snapshot.RadiusTicksPerInchTicks,
            snapshot.DrivenRootRadiusFortyEighths / (double)snapshot.RadiusTicksPerInchTicks,
            snapshot.DriverTipRadiusFortyEighths / (double)snapshot.RadiusTicksPerInchTicks,
            snapshot.DrivenTipRadiusFortyEighths / (double)snapshot.RadiusTicksPerInchTicks,
            snapshot.CenterLineDistanceFortyEighths / (double)snapshot.RadiusTicksPerInchTicks,
            snapshot.ToothProfileRadiusTicks / (double)snapshot.RadiusTicksPerInchTicks,
            driverDiameterText,
            drivenDiameterText,
            FractionalTicks(snapshot.ClockDriveNumeratorResidue, GearDemoDefaults.ClockTicksPerSecond, "tick"),
            MeshLabel(snapshot.HalfPitchTicks),
            $"{driverDiameterText} / {drivenDiameterText}",
            $"segment 0 -> {snapshot.CenterLineDistanceFortyEighths}/48 in",
            $"ticks {snapshot.DriverInputTicks}, tooth {snapshot.DriverToothPosition}/{snapshot.DriverTeeth}, rotations {snapshot.DriverRotations}",
            $"ticks {snapshot.DrivenProjectedTicks}, tooth {snapshot.DrivenProjectedToothPosition}/{snapshot.DrivenTeeth}, rotations {snapshot.DrivenProjectedRotations}",
            $"{snapshot.DriverTension} -> {snapshot.ProjectionResidueTicks}",
            $"Driver {snapshot.DriverTeeth}T / dia {driverDiameterText}",
            $"value {snapshot.DriverValue}/{snapshot.DriverPerimeterTicks}, rotations {snapshot.DriverRotations}, tension {snapshot.DriverTension}",
            $"Driven {snapshot.DrivenTeeth}T / dia {drivenDiameterText}",
            $"projected {snapshot.DrivenProjectedTicks}/{snapshot.DrivenPerimeterTicks}, rotations {snapshot.DrivenProjectedRotations}, residue {snapshot.ProjectionResidueTicks}");
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
    double DriverTurns,
    double DrivenProjectedTurns,
    double VisibleDrivenTurns,
    double DriverDiameterInches,
    double DrivenDiameterInches,
    double DriverRadiusInches,
    double DrivenRadiusInches,
    double DriverRootRadiusInches,
    double DrivenRootRadiusInches,
    double DriverTipRadiusInches,
    double DrivenTipRadiusInches,
    double CenterLineDistanceInches,
    double ToothProfileInches,
    string DriverDiameterText,
    string DrivenDiameterText,
    string ClockResidueText,
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
