using System;
using Avalonia;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Rendering;

internal static class GlossaryBasisRenderer
{
    private static readonly Carrier HeatCarrier = new(1L);

    public static GlossaryBasisSceneLayout Arrange(Rect bounds, GlossaryBasisDemoState state)
    {
        var scene = new Rect(
            bounds.X + 26d,
            bounds.Y + 22d,
            bounds.Width - 52d,
            bounds.Height - 44d);

        var left = scene.X + (scene.Width * 0.09d);
        var right = scene.X + (scene.Width * 0.91d);
        var custom = CreateScale(left, right, scene.Y + (scene.Height * 0.18d), "Custom Basis");
        var celsius = CreateScale(left, right, scene.Y + (scene.Height * 0.50d), "Celsius");
        var fahrenheit = CreateScale(left, right, scene.Y + (scene.Height * 0.82d), "Fahrenheit");
        return new GlossaryBasisSceneLayout(
            scene,
            custom,
            celsius,
            fahrenheit,
            new Point((left + right) * 0.5d, custom.TitleCenter.Y + 24d));
    }

    public static void Render(DrawingContext context, Rect bounds, GlossaryBasisDemoState state)
    {
        var layout = Arrange(bounds, state);
        DrawBackdrop(context, layout.Scene);

        DrawCustomScale(context, layout.Custom, state);
        if (state.IsFrozen)
        {
            DrawDerivedScale(
                context,
                layout.Celsius,
                "origin 0",
                GlossaryBasisDemoState.CelsiusTicks(0d).Raw,
                "unit 1",
                GlossaryBasisDemoState.CelsiusTicks(1d).Raw,
                $"temp {FormatScaleValue(state.CelsiusAtTicks(state.ReadTicks), 1)}C",
                state.ReadTicks.Raw);
            DrawDerivedScale(
                context,
                layout.Fahrenheit,
                "origin 0",
                GlossaryBasisDemoState.FahrenheitTicks(0d).Raw,
                "unit 1",
                GlossaryBasisDemoState.FahrenheitTicks(1d).Raw,
                $"temp {FormatScaleValue(state.FahrenheitAtTicks(state.ReadTicks), 1)}F",
                state.ReadTicks.Raw);
        }
    }

    private static GlossaryTemperatureScaleLayout CreateScale(double left, double right, double y, string label)
        => new(
            new Point(left, y),
            new Point(right, y),
            new Point((left + right) * 0.5d, y + 34d),
            label);

    private static void DrawBackdrop(DrawingContext context, Rect scene)
    {
        context.DrawRectangle(
            new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0d, 0d, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1d, 1d, RelativeUnit.Relative),
                GradientStops =
                [
                    new GradientStop(Color.Parse("#faf7f0"), 0d),
                    new GradientStop(Color.Parse("#f2eadf"), 1d)
                ]
            },
            null,
            scene,
            20d,
            20d);
    }

    private static void DrawCustomScale(DrawingContext context, GlossaryTemperatureScaleLayout scale, GlossaryBasisDemoState state)
    {
        DrawCarrier(context, scale);
        DrawTitle(context, scale);

        if (state.HasOrigin && state.HasRelation)
        {
            DrawBasisIndicator(
                context,
                PointAtTicks(scale, state.OriginTicks.Raw),
                PointAtTicks(scale, state.RelationTicks.Raw));
        }

        if (state.IsFrozen)
        {
            DrawRulerTicks(
                context,
                scale,
                state.CustomMinimumVisibleValue,
                state.CustomMaximumVisibleValue,
                state.TicksForCustomValue);
        }

        if (state.HasOrigin)
        {
            DrawFixedMarker(context, scale, state.OriginTicks.Raw, "origin 0", true);
        }

        if (state.HasRelation)
        {
            DrawFixedMarker(context, scale, state.RelationTicks.Raw, state.IsFrozen ? "unit 1" : "relation 1", false);
        }

        if (!state.HasOrigin)
        {
            GlossaryClaimVisuals.DrawSiteGlyph(
                context,
                HeatCarrier,
                PointAtTicks(scale, state.SiteTicks.Raw),
                state.Site.Ratio.Approximate);
        }
        else if (!state.HasRelation)
        {
            var site = PointAtTicks(scale, state.SiteTicks.Raw);
            GlossaryClaimVisuals.DrawSiteGlyph(
                context,
                HeatCarrier,
                site,
                state.Site.Ratio.Approximate);
            DrawCandidateCallout(context, site, "relation 1");
        }

        if (state.IsFrozen)
        {
            DrawReadMarker(
                context,
                scale,
                state.ReadTicks.Raw,
                $"temp {FormatScaleValue(state.CustomAtTicks(state.ReadTicks), 1)}");
        }
    }

    private static void DrawDerivedScale(
        DrawingContext context,
        GlossaryTemperatureScaleLayout scale,
        string originLabel,
        double originTicks,
        string unitLabel,
        double unitTicks,
        string readText,
        double readTicks)
    {
        DrawCarrier(context, scale);
        DrawTitle(context, scale);
        DrawRulerTicks(
            context,
            scale,
            scale.Label == "Celsius" ? GlossaryBasisDemoState.MinCelsius : (32d + (GlossaryBasisDemoState.MinCelsius * 9d / 5d)),
            scale.Label == "Celsius" ? GlossaryBasisDemoState.MaxCelsius : (32d + (GlossaryBasisDemoState.MaxCelsius * 9d / 5d)),
            value => scale.Label == "Celsius"
                ? GlossaryBasisDemoState.CelsiusTicks(value).Raw
                : GlossaryBasisDemoState.FahrenheitTicks(value).Raw);
        DrawFixedMarker(context, scale, originTicks, originLabel, true, true);
        DrawFixedMarker(context, scale, unitTicks, unitLabel, false, true);
        DrawReadMarker(context, scale, readTicks, readText);
    }

    private static void DrawCarrier(DrawingContext context, GlossaryTemperatureScaleLayout scale)
        => context.DrawLine(
            new Pen(GlossaryClaimVisuals.TrackBrush(HeatCarrier), 10.5d, lineCap: PenLineCap.Round),
            scale.Start,
            scale.End);

    private static void DrawBasisIndicator(DrawingContext context, Point origin, Point relation)
    {
        var direction = relation - origin;
        var length = Math.Sqrt((direction.X * direction.X) + (direction.Y * direction.Y));
        if (length <= 0.001d)
        {
            return;
        }

        var unit = direction / length;
        var tip = relation - (unit * 10d);
        var arrowBase = tip - (unit * 8d);
        context.DrawLine(
            new Pen(new SolidColorBrush(Color.FromArgb(240, 250, 248, 243)), 3d, lineCap: PenLineCap.Round),
            origin,
            arrowBase);

        var normal = new Vector(-unit.Y, unit.X);
        var geometry = new StreamGeometry();
        using (var stream = geometry.Open())
        {
            stream.BeginFigure(tip, true);
            stream.LineTo(arrowBase + (normal * 4d));
            stream.LineTo(arrowBase - (normal * 4d));
            stream.EndFigure(true);
        }

        context.DrawGeometry(new SolidColorBrush(Color.FromArgb(240, 250, 248, 243)), null, geometry);
    }

    private static void DrawTitle(DrawingContext context, GlossaryTemperatureScaleLayout scale)
    {
        ScenePrimitives.DrawCenteredLabel(
            context,
            scale.Label,
            scale.TitleCenter,
            11d,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.UiFont);
    }

    private static void DrawFixedMarker(DrawingContext context, GlossaryTemperatureScaleLayout scale, double ticks, string text, bool above, bool showCenterDot = false)
    {
        var point = PointAtTicks(scale, ticks);
        if (ticks < 0d || ticks > GlossaryBasisDemoState.TotalTicks)
        {
            DrawEdgeIndicator(context, scale, ticks < 0d, text, above);
            return;
        }

        ScenePrimitives.DrawCircle(
            context,
            point,
            8d,
            ProbePalette.Surface,
            new Pen(new SolidColorBrush(Color.Parse("#243336")), 1.5d));
        if (showCenterDot)
        {
            ScenePrimitives.DrawCircle(
                context,
                point,
                2.5d,
                new SolidColorBrush(Color.Parse("#8b8b8b")));
        }

        var anchor = point + new Vector(0d, above ? -34d : 34d);
        context.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(110, 107, 118, 128)), 1d), point, anchor);
        ScenePrimitives.DrawCenteredBadgeLabel(
            context,
            text,
            anchor,
            9d,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.MonoFont,
            ProbePalette.Surface,
            new Pen(new SolidColorBrush(Color.FromArgb(90, 197, 182, 160)), 0.8d),
            new Thickness(6, 3, 6, 3),
            8d);
    }

    private static void DrawReadMarker(
        DrawingContext context,
        GlossaryTemperatureScaleLayout scale,
        double ticks,
        string text)
    {
        if (ticks < 0d || ticks > GlossaryBasisDemoState.TotalTicks)
        {
            DrawEdgeIndicator(context, scale, ticks < 0d, text, true);
            return;
        }

        var point = PointAtTicks(scale, ticks);
        GlossaryClaimVisuals.DrawSiteGlyph(context, HeatCarrier, point, Math.Clamp(ticks / GlossaryBasisDemoState.TotalTicks, 0d, 1d), 0.92d);
        var anchor = point + new Vector(62d, -50d);
        context.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(110, 107, 118, 128)), 1d), point, anchor);
        ScenePrimitives.DrawCenteredBadgeLabel(
            context,
            text,
            anchor,
            9d,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.MonoFont,
            ProbePalette.Surface,
            new Pen(new SolidColorBrush(Color.FromArgb(90, 197, 182, 160)), 0.8d),
            new Thickness(6, 3, 6, 3),
            8d);
    }

    private static void DrawCandidateCallout(DrawingContext context, Point point, string text)
    {
        var anchor = point + new Vector(62d, -28d);
        context.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(110, 107, 118, 128)), 1d), point, anchor);
        ScenePrimitives.DrawCenteredBadgeLabel(
            context,
            text,
            anchor,
            9d,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.MonoFont,
            ProbePalette.Surface,
            new Pen(new SolidColorBrush(Color.FromArgb(90, 197, 182, 160)), 0.8d),
            new Thickness(6, 3, 6, 3),
            8d);
    }

    private static void DrawEdgeIndicator(DrawingContext context, GlossaryTemperatureScaleLayout scale, bool left, string text, bool above)
    {
        var point = left ? scale.Start : scale.End;
        var arrowPoint = point + new Vector(left ? -12d : 12d, 0d);
        var tri = left
            ? new[] { point + new Vector(-2d, 0d), arrowPoint + new Vector(6d, -4d), arrowPoint + new Vector(6d, 4d) }
            : new[] { point + new Vector(2d, 0d), arrowPoint + new Vector(-6d, -4d), arrowPoint + new Vector(-6d, 4d) };
        var geometry = new StreamGeometry();
        using (var stream = geometry.Open())
        {
            stream.BeginFigure(tri[0], true);
            stream.LineTo(tri[1]);
            stream.LineTo(tri[2]);
            stream.EndFigure(true);
        }

        context.DrawGeometry(new SolidColorBrush(Color.Parse("#243336")), null, geometry);
        var anchor = point + new Vector(left ? 46d : -46d, above ? -26d : 26d);
        ScenePrimitives.DrawCenteredBadgeLabel(
            context,
            text,
            anchor,
            9d,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.MonoFont,
            ProbePalette.Surface,
            new Pen(new SolidColorBrush(Color.FromArgb(90, 197, 182, 160)), 0.8d),
            new Thickness(6, 3, 6, 3),
            8d);
    }

    private static void DrawRulerTicks(
        DrawingContext context,
        GlossaryTemperatureScaleLayout scale,
        double minValue,
        double maxValue,
        Func<double, double> valueToTicks)
    {
        var low = Math.Min(minValue, maxValue);
        var high = Math.Max(minValue, maxValue);
        var majorStep = NiceStep((high - low) / 6d);
        var minorStep = majorStep / 5d;
        var minorPen = new Pen(new SolidColorBrush(Color.FromArgb(100, 94, 94, 94)), 1d);
        var majorPen = new Pen(new SolidColorBrush(Color.FromArgb(155, 72, 72, 72)), 1.1d);
        var labelBrush = new SolidColorBrush(Color.FromArgb(150, 118, 118, 118));
        var start = Math.Ceiling(low / minorStep) * minorStep;
        for (var value = start; value <= high + (minorStep * 0.25d); value += minorStep)
        {
            var ticks = valueToTicks(value);
            if (ticks < 0d || ticks > GlossaryBasisDemoState.TotalTicks)
            {
                continue;
            }

            var point = PointAtTicks(scale, ticks);
            var major = Math.Abs((value / majorStep) - Math.Round(value / majorStep)) < 0.0001d;
            var size = major ? 7d : 4d;
            context.DrawLine(
                major ? majorPen : minorPen,
                point + new Vector(0d, -size),
                point + new Vector(0d, size));

            if (!major)
            {
                continue;
            }

            ScenePrimitives.DrawCenteredLabel(
                context,
                FormatScaleValue(value, 0),
                point + new Vector(0d, 16d),
                8.2d,
                labelBrush,
                FontWeight.Medium,
                ProbePalette.MonoFont);
        }
    }

    private static double NiceStep(double target)
    {
        var candidates = new[] { 0.1d, 0.2d, 0.5d, 1d, 2d, 5d, 10d, 20d, 50d, 100d };
        foreach (var candidate in candidates)
        {
            if (candidate >= target)
            {
                return candidate;
            }
        }

        return candidates[^1];
    }

    internal static string FormatScaleValue(double value, int decimals = 2)
        => Math.Abs(value - Math.Round(value)) < 0.000001d
            ? Math.Round(value).ToString("0")
            : value.ToString($"0.{new string('0', decimals)}");

    private static Point PointAtTicks(GlossaryTemperatureScaleLayout scale, double ticks)
        => new(
            scale.Start.X + ((scale.End.X - scale.Start.X) * (ticks / GlossaryBasisDemoState.TotalTicks)),
            scale.Start.Y);
}

internal readonly record struct GlossaryBasisSceneLayout(
    Rect Scene,
    GlossaryTemperatureScaleLayout Custom,
    GlossaryTemperatureScaleLayout Celsius,
    GlossaryTemperatureScaleLayout Fahrenheit,
    Point ActionButtonCenter);

internal readonly record struct GlossaryTemperatureScaleLayout(
    Point Start,
    Point End,
    Point TitleCenter,
    string Label);
