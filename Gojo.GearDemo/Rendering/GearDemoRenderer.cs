using Avalonia;
using Gojo.GearDemo.Model;
using SkiaSharp;

namespace Gojo.GearDemo.Rendering;

internal static class GearDemoRenderer
{
    private const double OuterPadding = 22d;
    private const double Space = 24d;
    private const double SideFraction = 0.26d;
    private const double SideMinWidth = 286d;
    private const double SceneMinWidth = 420d;
    private const double ScenePadding = 30d;
    private const double LabelBand = 88d;
    private const double BasePixelsPerInch = 430d;

    public static GearDemoLayout Arrange(
        Rect bounds,
        GearDemoSnapshot snapshot)
    {
        var content = new Rect(
            OuterPadding,
            OuterPadding,
            Math.Max(1d, bounds.Width - (OuterPadding * 2d)),
            Math.Max(1d, bounds.Height - (OuterPadding * 2d)));

        var sideWidth = ResolveSideWidth(content.Width);
        var side = new Rect(content.X, content.Y, sideWidth, content.Height);
        var scene = new Rect(
            side.Right + Space,
            content.Y,
            Math.Max(1d, content.Width - sideWidth - Space),
            content.Height);

        var controlPad = Math.Clamp(side.Width * 0.045d, 12d, 18d);
        var controlX = side.X + controlPad;
        var controlWidth = Math.Max(1d, side.Width - (controlPad * 2d));
        var gap = 8d;
        var buttonWidth = Math.Max(76d, (controlWidth - gap) / 2d);
        var buttonY = side.Y + 2d;
        var play = new Rect(controlX, buttonY, buttonWidth, 32d);
        var reset = new Rect(play.Right + gap, buttonY, buttonWidth, 32d);

        var speed = Slider(controlX, buttonY + 58d, controlWidth, snapshot.SpeedSlider, "ticks/s", snapshot.SpeedTicksPerSecond.ToString());
        var driver = Slider(controlX, speed.Bounds.Bottom + 30d, controlWidth, snapshot.DriverTeethSlider, "driver teeth", snapshot.DriverTeeth.ToString());
        var driven = Slider(controlX, driver.Bounds.Bottom + 30d, controlWidth, snapshot.DrivenTeethSlider, "driven teeth", snapshot.DrivenTeeth.ToString());

        var gearScale = GearScale(scene, snapshot);
        var driverRadius = snapshot.DriverRadiusInches * BasePixelsPerInch * gearScale;
        var drivenRadius = snapshot.DrivenRadiusInches * BasePixelsPerInch * gearScale;
        var driverRootRadius = snapshot.DriverRootRadiusInches * BasePixelsPerInch * gearScale;
        var drivenRootRadius = snapshot.DrivenRootRadiusInches * BasePixelsPerInch * gearScale;
        var driverTipRadius = snapshot.DriverTipRadiusInches * BasePixelsPerInch * gearScale;
        var drivenTipRadius = snapshot.DrivenTipRadiusInches * BasePixelsPerInch * gearScale;
        var centerDistance = snapshot.CenterLineDistanceInches * BasePixelsPerInch * gearScale;
        var gearArea = new Rect(
            scene.X + ScenePadding,
            scene.Y + ScenePadding,
            Math.Max(1d, scene.Width - (ScenePadding * 2d)),
            Math.Max(1d, scene.Height - LabelBand - (ScenePadding * 2d)));
        var sceneCenter = gearArea.Center;
        var driverCenter = new Point(sceneCenter.X - (centerDistance * 0.5d), sceneCenter.Y);
        var drivenCenter = new Point(sceneCenter.X + (centerDistance * 0.5d), sceneCenter.Y);
        var driverPhase = 0.25d - PhaseTurns(snapshot.DriverMeshPhaseTicks, snapshot.DriverPerimeterTicks);
        var drivenPhase = 0.75d - PhaseTurns(snapshot.DrivenMeshPhaseTicks, snapshot.DrivenPerimeterTicks);
        var driverRootHalf = PhaseTurns(snapshot.HalfPitchTicks, snapshot.DriverPerimeterTicks);
        var drivenRootHalf = PhaseTurns(snapshot.HalfPitchTicks, snapshot.DrivenPerimeterTicks);
        var driverTipHalf = PhaseTurns(snapshot.QuarterPitchTicks, snapshot.DriverPerimeterTicks);
        var drivenTipHalf = PhaseTurns(snapshot.QuarterPitchTicks, snapshot.DrivenPerimeterTicks);

        return new GearDemoLayout(
            bounds,
            scene,
            side,
            play,
            reset,
            speed,
            driver,
            driven,
            new GearLayout(
                driverCenter,
                driverRadius,
                Math.Max(1d, driverRootRadius),
                Math.Max(driverRadius, driverTipRadius),
                snapshot.DriverTeeth,
                snapshot.DriverTurns,
                driverPhase,
                driverRootHalf,
                driverTipHalf,
                $"Driver {snapshot.DriverTeeth}T / dia {snapshot.DriverDiameterText}",
                $"value {snapshot.DriverValue}/{snapshot.DriverPerimeterTicks}, rotations {snapshot.DriverRotations}, tension {snapshot.DriverTension}"),
            new GearLayout(
                drivenCenter,
                drivenRadius,
                Math.Max(1d, drivenRootRadius),
                Math.Max(drivenRadius, drivenTipRadius),
                snapshot.DrivenTeeth,
                snapshot.DrivenProjectedTurns,
                drivenPhase,
                drivenRootHalf,
                drivenTipHalf,
                $"Driven {snapshot.DrivenTeeth}T / dia {snapshot.DrivenDiameterText}",
                $"projected {snapshot.DrivenProjectedTicks}/{snapshot.DrivenPerimeterTicks}, rotations {snapshot.DrivenProjectedRotations}, residue {snapshot.ProjectionResidueTicks}"));
    }

    public static void Render(
        SKCanvas canvas,
        GearDemoLayout layout,
        GearDemoSnapshot snapshot,
        GearDemoInteraction interaction)
    {
        canvas.Clear(Palette.Background);
        DrawOpenSpaces(canvas, layout);
        DrawSide(canvas, layout, snapshot, interaction);
        DrawScene(canvas, layout);
    }

    private static double ResolveSideWidth(double contentWidth)
    {
        if (contentWidth >= SideMinWidth + Space + SceneMinWidth)
        {
            return Math.Min(
                Math.Max(SideMinWidth, contentWidth * SideFraction),
                contentWidth - Space - SceneMinWidth);
        }

        return Math.Max(180d, contentWidth * 0.34d);
    }

    private static double GearScale(Rect scene, GearDemoSnapshot snapshot)
    {
        var driverTipRadius = snapshot.DriverTipRadiusInches * BasePixelsPerInch;
        var drivenTipRadius = snapshot.DrivenTipRadiusInches * BasePixelsPerInch;
        var centerDistance = snapshot.CenterLineDistanceInches * BasePixelsPerInch;
        var rawWidth = centerDistance + driverTipRadius + drivenTipRadius;
        var rawHeight = Math.Max(driverTipRadius, drivenTipRadius) * 2.04d;
        var availableWidth = Math.Max(1d, scene.Width - (ScenePadding * 2d));
        var availableHeight = Math.Max(1d, scene.Height - LabelBand - (ScenePadding * 2d));
        var scale = Math.Min(1d, Math.Min(availableWidth / rawWidth, availableHeight / rawHeight));
        return Math.Max(0.28d, scale);
    }

    private static SliderLayout Slider(
        double x,
        double y,
        double width,
        GojoSliderSnapshot slider,
        string label,
        string readout)
    {
        var bounds = new Rect(x, y, width, 42d);
        var track = new Rect(x + 4d, y + 26d, Math.Max(1d, width - 8d), 6d);
        var t = slider.Range <= 0 ? 0d : slider.PositionTicks / (double)slider.Range;
        var knob = new Point(track.X + track.Width * t, track.Center.Y);
        return new SliderLayout(bounds, track, knob, slider.Minimum, slider.Maximum, slider.ValueTicks, label, readout);
    }

    private static void DrawOpenSpaces(SKCanvas canvas, GearDemoLayout layout)
    {
        using var separator = new SKPaint
        {
            IsAntialias = true,
            Color = Palette.Separator,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.2f
        };
        var x = layout.Side.Right + (Space * 0.5d);
        canvas.DrawLine(ToFloat(x), ToFloat(layout.Side.Y), ToFloat(x), ToFloat(layout.Side.Bottom), separator);
    }

    private static void DrawSide(
        SKCanvas canvas,
        GearDemoLayout layout,
        GearDemoSnapshot snapshot,
        GearDemoInteraction interaction)
    {
        DrawButton(canvas, layout.PlayButton, interaction.IsPlaying ? "pause" : "play", interaction.HoverPoint);
        DrawButton(canvas, layout.ResetButton, "reset", interaction.HoverPoint);

        DrawSlider(canvas, layout.SpeedSlider, interaction.HoverPoint);
        DrawSlider(canvas, layout.DriverTeethSlider, interaction.HoverPoint);
        DrawSlider(canvas, layout.DrivenTeethSlider, interaction.HoverPoint);

        var x = layout.SpeedSlider.Bounds.X;
        var y = layout.DrivenTeethSlider.Bounds.Bottom + 34d;
        DrawReadout(canvas, x, ref y, "diameter", $"{snapshot.DriverDiameterText} / {snapshot.DrivenDiameterText}");
        DrawReadout(canvas, x, ref y, "mesh", snapshot.MeshLabel);
        DrawReadout(canvas, x, ref y, "centers", $"segment 0 -> {snapshot.CenterLineDistanceFortyEighths}/48 in");
        DrawReadout(canvas, x, ref y, "driver", $"ticks {snapshot.DriverInputTicks}, tooth {snapshot.DriverToothPosition}/{snapshot.DriverTeeth}, rotations {snapshot.DriverRotations}");
        DrawReadout(canvas, x, ref y, "driven read", $"ticks {snapshot.DrivenProjectedTicks}, tooth {snapshot.DrivenProjectedToothPosition}/{snapshot.DrivenTeeth}, rotations {snapshot.DrivenProjectedRotations}");
        DrawReadout(canvas, x, ref y, "tension", $"{snapshot.DriverTension} -> {snapshot.ProjectionResidueTicks}");
    }

    private static void DrawScene(SKCanvas canvas, GearDemoLayout layout)
    {
        DrawConstructionCircles(canvas, layout.DriverGear, Palette.DriverSoft);
        DrawConstructionCircles(canvas, layout.DrivenGear, Palette.DrivenSoft);
        DrawMesh(canvas, layout);
        DrawGear(canvas, layout.DriverGear, Palette.Driver, Palette.DriverDark);
        DrawGear(canvas, layout.DrivenGear, Palette.Driven, Palette.DrivenDark);
        DrawGearCaptions(canvas, layout);
    }

    private static void DrawConstructionCircles(SKCanvas canvas, GearLayout gear, SKColor color)
    {
        using var fill = new SKPaint { IsAntialias = true, Color = color.WithAlpha(34), Style = SKPaintStyle.Fill };
        using var pitchStroke = new SKPaint { IsAntialias = true, Color = color.WithAlpha(170), Style = SKPaintStyle.Stroke, StrokeWidth = 1.35f };
        using var outerStroke = new SKPaint
        {
            IsAntialias = true,
            Color = color.WithAlpha(94),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1f,
            PathEffect = SKPathEffect.CreateDash([5f, 5f], 0f)
        };
        canvas.DrawCircle(ToFloat(gear.Center.X), ToFloat(gear.Center.Y), ToFloat(gear.Radius), fill);
        canvas.DrawCircle(ToFloat(gear.Center.X), ToFloat(gear.Center.Y), ToFloat(gear.RootRadius), outerStroke);
        canvas.DrawCircle(ToFloat(gear.Center.X), ToFloat(gear.Center.Y), ToFloat(gear.TipRadius), outerStroke);
        canvas.DrawCircle(ToFloat(gear.Center.X), ToFloat(gear.Center.Y), ToFloat(gear.Radius), pitchStroke);
    }

    private static void DrawMesh(SKCanvas canvas, GearDemoLayout layout)
    {
        using var stroke = new SKPaint
        {
            IsAntialias = true,
            Color = Palette.Mesh.WithAlpha(180),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2f,
            PathEffect = SKPathEffect.CreateDash([8f, 7f], 0f)
        };
        canvas.DrawLine(ToPoint(layout.DriverGear.Center), ToPoint(layout.DrivenGear.Center), stroke);

        var contact = Interpolate(
            layout.DriverGear.Center,
            layout.DrivenGear.Center,
            layout.DriverGear.Radius / (layout.DriverGear.Radius + layout.DrivenGear.Radius));
        using var fill = new SKPaint { IsAntialias = true, Color = Palette.Mesh, Style = SKPaintStyle.Fill };
        canvas.DrawCircle(ToFloat(contact.X), ToFloat(contact.Y), 5.5f, fill);
    }

    private static void DrawGear(SKCanvas canvas, GearLayout gear, SKColor fill, SKColor strokeColor)
    {
        using var path = GearPath(gear);
        using var paint = new SKPaint { IsAntialias = true, Color = fill, Style = SKPaintStyle.Fill };
        using var stroke = new SKPaint { IsAntialias = true, Color = strokeColor, Style = SKPaintStyle.Stroke, StrokeWidth = 2.4f, StrokeJoin = SKStrokeJoin.Round };
        canvas.DrawPath(path, paint);
        canvas.DrawPath(path, stroke);

        using var pitch = new SKPaint { IsAntialias = true, Color = strokeColor.WithAlpha(88), Style = SKPaintStyle.Stroke, StrokeWidth = 1.25f };
        canvas.DrawCircle(ToFloat(gear.Center.X), ToFloat(gear.Center.Y), ToFloat(gear.Radius), pitch);

        for (var index = 0; index < gear.ToothCount; index++)
        {
            var angle = GearAngle(gear, index / (double)gear.ToothCount);
            var from = Polar(gear.Center, gear.RootRadius + 4d, angle);
            var to = Polar(gear.Center, gear.TipRadius - 3d, angle);
            canvas.DrawLine(ToPoint(from), ToPoint(to), pitch);
        }

        DrawSpokes(canvas, gear, strokeColor);
        DrawCurrentTick(canvas, gear, strokeColor);
    }

    private static SKPath GearPath(GearLayout gear)
    {
        var path = new SKPath();
        var steps = Math.Max(gear.ToothCount, 3);

        for (var index = 0; index < steps; index++)
        {
            var center = index / (double)steps;
            var a0 = GearAngle(gear, center - gear.RootHalfTurns);
            var a1 = GearAngle(gear, center - gear.TipHalfTurns);
            var a2 = GearAngle(gear, center + gear.TipHalfTurns);
            var a3 = GearAngle(gear, center + gear.RootHalfTurns);
            var p0 = Polar(gear.Center, gear.RootRadius, a0);
            var p1 = Polar(gear.Center, gear.TipRadius, a1);
            var p2 = Polar(gear.Center, gear.TipRadius, a2);
            var p3 = Polar(gear.Center, gear.RootRadius, a3);

            if (index == 0)
            {
                path.MoveTo(ToPoint(p0));
            }
            else
            {
                path.LineTo(ToPoint(p0));
            }

            path.LineTo(ToPoint(p1));
            path.LineTo(ToPoint(p2));
            path.LineTo(ToPoint(p3));
        }

        path.Close();
        return path;
    }

    private static void DrawSpokes(SKCanvas canvas, GearLayout gear, SKColor color)
    {
        const int spokeCount = 5;
        using var stroke = new SKPaint { IsAntialias = true, Color = color.WithAlpha(124), Style = SKPaintStyle.Stroke, StrokeWidth = 5.2f, StrokeCap = SKStrokeCap.Round };
        using var squareFill = new SKPaint { IsAntialias = true, Color = Palette.Background, Style = SKPaintStyle.Fill };
        using var squareStroke = new SKPaint { IsAntialias = true, Color = color.WithAlpha(175), Style = SKPaintStyle.Stroke, StrokeWidth = 1.8f };

        for (var index = 0; index < spokeCount; index++)
        {
            var angle = GearAngle(gear, index / (double)spokeCount);
            var to = Polar(gear.Center, gear.Radius * 0.66d, angle);
            canvas.DrawLine(ToPoint(gear.Center), ToPoint(to), stroke);
            DrawRotatedSquare(canvas, to, Math.Clamp(gear.Radius * 0.13d, 12d, 20d), angle, squareFill, squareStroke);
        }

        using var hub = new SKPaint { IsAntialias = true, Color = Palette.Background, Style = SKPaintStyle.Fill };
        using var hubStroke = new SKPaint { IsAntialias = true, Color = color, Style = SKPaintStyle.Stroke, StrokeWidth = 2.2f };
        canvas.DrawCircle(ToFloat(gear.Center.X), ToFloat(gear.Center.Y), ToFloat(Math.Max(16d, gear.Radius * 0.16d)), hub);
        canvas.DrawCircle(ToFloat(gear.Center.X), ToFloat(gear.Center.Y), ToFloat(Math.Max(16d, gear.Radius * 0.16d)), hubStroke);
    }

    private static void DrawCurrentTick(SKCanvas canvas, GearLayout gear, SKColor color)
    {
        var angle = GearAngle(gear, 0d);
        var from = Polar(gear.Center, gear.Radius * 0.28d, angle);
        var to = Polar(gear.Center, gear.TipRadius + 14d, angle);
        using var marker = new SKPaint { IsAntialias = true, Color = color, Style = SKPaintStyle.Stroke, StrokeWidth = 4.4f, StrokeCap = SKStrokeCap.Round };
        canvas.DrawLine(ToPoint(from), ToPoint(to), marker);
        using var fill = new SKPaint { IsAntialias = true, Color = Palette.Background, Style = SKPaintStyle.Fill };
        using var stroke = new SKPaint { IsAntialias = true, Color = color, Style = SKPaintStyle.Stroke, StrokeWidth = 2.2f };
        DrawRotatedSquare(canvas, to, 14d, angle, fill, stroke);
    }

    private static void DrawRotatedSquare(SKCanvas canvas, Point center, double size, double angle, SKPaint fill, SKPaint stroke)
    {
        var half = ToFloat(size * 0.5d);
        canvas.Save();
        canvas.Translate(ToFloat(center.X), ToFloat(center.Y));
        canvas.RotateDegrees(ToFloat(angle * 180d / Math.PI));
        var rect = new SKRect(-half, -half, half, half);
        canvas.DrawRoundRect(rect, 2f, 2f, fill);
        canvas.DrawRoundRect(rect, 2f, 2f, stroke);
        canvas.Restore();
    }

    private static void DrawGearCaptions(SKCanvas canvas, GearDemoLayout layout)
    {
        var y = layout.Scene.Bottom - 64d;
        var half = layout.Scene.Width * 0.5d;
        DrawCaption(canvas, new Rect(layout.Scene.X, y, half - 10d, 54d), layout.DriverGear.Caption, layout.DriverGear.State);
        DrawCaption(canvas, new Rect(layout.Scene.X + half + 10d, y, half - 10d, 54d), layout.DrivenGear.Caption, layout.DrivenGear.State);
    }

    private static void DrawCaption(SKCanvas canvas, Rect rect, string line1, string line2)
    {
        DrawCenteredLine(canvas, line1, rect.Center.X, rect.Y, 14f, Palette.Ink, true);
        DrawCenteredLine(canvas, line2, rect.Center.X, rect.Y + 24d, 12f, Palette.Muted, false);
    }

    private static void DrawReadout(SKCanvas canvas, double x, ref double y, string label, string value)
    {
        DrawText(canvas, label, x, y, 11f, Palette.Muted, false);
        y += 17d;
        DrawText(canvas, value, x, y, 13f, Palette.Ink, true);
        y += 30d;
    }

    private static void DrawButton(SKCanvas canvas, Rect rect, string text, Point? hover)
    {
        var active = hover is { } point && rect.Contains(point);
        using var fill = new SKPaint { IsAntialias = true, Color = active ? Palette.ButtonHot : Palette.Button, Style = SKPaintStyle.Fill };
        using var stroke = new SKPaint { IsAntialias = true, Color = active ? Palette.DriverDark : Palette.Separator, Style = SKPaintStyle.Stroke, StrokeWidth = 1.2f };
        canvas.DrawRoundRect(ToRect(rect), 8f, 8f, fill);
        canvas.DrawRoundRect(ToRect(rect), 8f, 8f, stroke);
        DrawCenteredText(canvas, text, rect, 13f, Palette.Ink, true);
    }

    private static void DrawSlider(SKCanvas canvas, SliderLayout slider, Point? hover)
    {
        DrawText(canvas, slider.Label, slider.Bounds.X, slider.Bounds.Y, 12f, Palette.Muted, false);
        DrawText(canvas, slider.Readout, slider.Bounds.Right - 42d, slider.Bounds.Y, 13f, Palette.Ink, true);

        using var track = new SKPaint { IsAntialias = true, Color = Palette.SliderTrack, Style = SKPaintStyle.Stroke, StrokeWidth = ToFloat(slider.Track.Height), StrokeCap = SKStrokeCap.Round };
        using var fill = new SKPaint { IsAntialias = true, Color = Palette.Driver, Style = SKPaintStyle.Stroke, StrokeWidth = ToFloat(slider.Track.Height), StrokeCap = SKStrokeCap.Round };
        canvas.DrawLine(ToFloat(slider.Track.Left), ToFloat(slider.Track.Center.Y), ToFloat(slider.Track.Right), ToFloat(slider.Track.Center.Y), track);
        canvas.DrawLine(ToFloat(slider.Track.Left), ToFloat(slider.Track.Center.Y), ToFloat(slider.Knob.X), ToFloat(slider.Knob.Y), fill);

        var hot = hover is { } point && slider.Bounds.Contains(point);
        using var knob = new SKPaint { IsAntialias = true, Color = hot ? Palette.ButtonHot : Palette.Background, Style = SKPaintStyle.Fill };
        using var ring = new SKPaint { IsAntialias = true, Color = Palette.DriverDark, Style = SKPaintStyle.Stroke, StrokeWidth = 2.1f };
        canvas.DrawCircle(ToFloat(slider.Knob.X), ToFloat(slider.Knob.Y), hot ? 8.2f : 7.2f, knob);
        canvas.DrawCircle(ToFloat(slider.Knob.X), ToFloat(slider.Knob.Y), hot ? 8.2f : 7.2f, ring);
    }

    private static void DrawCenteredText(SKCanvas canvas, string text, Rect rect, float size, SKColor color, bool bold)
    {
        using var textStyle = TextStyle.Create(size, color, bold);
        var bounds = new SKRect();
        textStyle.Font.MeasureText(text, out bounds, textStyle.Paint);
        var x = rect.Center.X - (bounds.Width * 0.5d);
        var y = rect.Center.Y - (bounds.Height * 0.5d) - bounds.Top;
        canvas.DrawText(text, ToFloat(x), ToFloat(y), SKTextAlign.Left, textStyle.Font, textStyle.Paint);
    }

    private static void DrawCenteredLine(SKCanvas canvas, string text, double centerX, double y, float size, SKColor color, bool bold)
    {
        using var textStyle = TextStyle.Create(size, color, bold);
        var width = textStyle.Font.MeasureText(text, textStyle.Paint);
        canvas.DrawText(text, ToFloat(centerX - width * 0.5d), ToFloat(y - textStyle.Font.Metrics.Ascent), SKTextAlign.Left, textStyle.Font, textStyle.Paint);
    }

    private static void DrawText(SKCanvas canvas, string text, double x, double y, float size, SKColor color, bool bold)
    {
        using var textStyle = TextStyle.Create(size, color, bold);
        canvas.DrawText(text, ToFloat(x), ToFloat(y - textStyle.Font.Metrics.Ascent), SKTextAlign.Left, textStyle.Font, textStyle.Paint);
    }

    private sealed class TextStyle : IDisposable
    {
        private readonly SKTypeface typeface;

        private TextStyle(float size, SKColor color, bool bold)
        {
            typeface = SKTypeface.FromFamilyName("Inter", bold ? SKFontStyle.Bold : SKFontStyle.Normal);
            Font = new SKFont(typeface, size)
            {
                Subpixel = true
            };
            Paint = new SKPaint
            {
                IsAntialias = true,
                Color = color
            };
        }

        public SKFont Font { get; }

        public SKPaint Paint { get; }

        public static TextStyle Create(float size, SKColor color, bool bold) =>
            new(size, color, bold);

        public void Dispose()
        {
            Paint.Dispose();
            Font.Dispose();
            typeface.Dispose();
        }
    }

    private static double GearAngle(GearLayout gear, double localTurns) =>
        -Math.PI * 0.5d + ((gear.Turns + gear.PhaseTurns + localTurns) * Math.PI * 2d);

    private static double PhaseTurns(long ticks, int perimeterTicks) =>
        perimeterTicks == 0 ? 0d : ticks / (double)perimeterTicks;

    private static Point Polar(Point center, double radius, double angle) =>
        new(center.X + Math.Cos(angle) * radius, center.Y + Math.Sin(angle) * radius);

    private static Point Interpolate(Point from, Point to, double t) =>
        new(from.X + (to.X - from.X) * t, from.Y + (to.Y - from.Y) * t);

    private static SKRect ToRect(Rect rect) =>
        new(ToFloat(rect.X), ToFloat(rect.Y), ToFloat(rect.Right), ToFloat(rect.Bottom));

    private static SKPoint ToPoint(Point point) =>
        new(ToFloat(point.X), ToFloat(point.Y));

    private static float ToFloat(double value) =>
        checked((float)value);

    private static class Palette
    {
        public static readonly SKColor Background = new(249, 247, 242);
        public static readonly SKColor Separator = new(210, 204, 194);
        public static readonly SKColor Ink = new(35, 48, 52);
        public static readonly SKColor Muted = new(94, 106, 110);
        public static readonly SKColor Button = new(239, 244, 247);
        public static readonly SKColor ButtonHot = new(219, 236, 244);
        public static readonly SKColor SliderTrack = new(212, 221, 224);
        public static readonly SKColor Driver = new(36, 124, 156);
        public static readonly SKColor DriverDark = new(20, 88, 116);
        public static readonly SKColor DriverSoft = new(78, 164, 185);
        public static readonly SKColor Driven = new(194, 88, 68);
        public static readonly SKColor DrivenDark = new(135, 52, 42);
        public static readonly SKColor DrivenSoft = new(224, 125, 96);
        public static readonly SKColor Mesh = new(96, 88, 160);
    }
}

internal readonly record struct GearDemoInteraction(bool IsPlaying, Point? HoverPoint);
