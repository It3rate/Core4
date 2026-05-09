using Avalonia;
using Gojo.GearDemo.Adapters;
using Gojo.GearDemo.Components;
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

    public static GearDemoLayout Arrange(Rect bounds, GearDemoInspection inspection)
    {
        var read = GearDemoAdapters.ToRenderRead(inspection);
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
        var buttonWidth = Math.Max(62d, (controlWidth - (gap * 2d)) / 3d);
        var buttonY = side.Y + 2d;
        var play = new Rect(controlX, buttonY, buttonWidth, 32d);
        var reset = new Rect(play.Right + gap, buttonY, buttonWidth, 32d);
        var web = new Rect(reset.Right + gap, buttonY, buttonWidth, 32d);

        var speed = GearDemoAdapters.SliderLayout(controlX, buttonY + 58d, controlWidth, inspection.SpeedSlider.Read(), "ticks/s", read.SpeedTicksPerSecond.ToString());
        var driver = GearDemoAdapters.SliderLayout(controlX, speed.Bounds.Bottom + 30d, controlWidth, inspection.DriverTeethSlider.Read(), "driver teeth", read.DriverTeeth.ToString());
        var driven = GearDemoAdapters.SliderLayout(controlX, driver.Bounds.Bottom + 30d, controlWidth, inspection.DrivenTeethSlider.Read(), "driven teeth", read.DrivenTeeth.ToString());

        var gearScale = GearScale(scene, read);
        var driverRadius = read.DriverRadiusInches * BasePixelsPerInch * gearScale;
        var drivenRadius = read.DrivenRadiusInches * BasePixelsPerInch * gearScale;
        var driverRootRadius = read.DriverRootRadiusInches * BasePixelsPerInch * gearScale;
        var drivenRootRadius = read.DrivenRootRadiusInches * BasePixelsPerInch * gearScale;
        var driverTipRadius = read.DriverTipRadiusInches * BasePixelsPerInch * gearScale;
        var drivenTipRadius = read.DrivenTipRadiusInches * BasePixelsPerInch * gearScale;
        var centerDistance = read.CenterLineDistanceInches * BasePixelsPerInch * gearScale;
        var gearArea = new Rect(
            scene.X + ScenePadding,
            scene.Y + ScenePadding,
            Math.Max(1d, scene.Width - (ScenePadding * 2d)),
            Math.Max(1d, scene.Height - LabelBand - (ScenePadding * 2d)));
        var sceneCenter = gearArea.Center;
        var driverCenter = new Point(sceneCenter.X - (centerDistance * 0.5d), sceneCenter.Y);
        var drivenCenter = new Point(sceneCenter.X + (centerDistance * 0.5d), sceneCenter.Y);
        var driverPhase = 0.25d - GearDemoAdapters.PhaseTurns(read.DriverMeshPhaseTicks, read.DriverPerimeterTicks);
        var drivenPhase = 0.75d - GearDemoAdapters.PhaseTurns(read.DrivenMeshPhaseTicks, read.DrivenPerimeterTicks);
        var driverRootHalf = GearDemoAdapters.PhaseTurns(read.HalfPitchTicks, read.DriverPerimeterTicks);
        var drivenRootHalf = GearDemoAdapters.PhaseTurns(read.HalfPitchTicks, read.DrivenPerimeterTicks);
        var driverTipHalf = GearDemoAdapters.PhaseTurns(read.QuarterPitchTicks, read.DriverPerimeterTicks);
        var drivenTipHalf = GearDemoAdapters.PhaseTurns(read.QuarterPitchTicks, read.DrivenPerimeterTicks);

        return new GearDemoLayout(
            bounds,
            scene,
            side,
            play,
            reset,
            web,
            speed,
            driver,
            driven,
            new GearLayout(
                driverCenter,
                driverRadius,
                Math.Max(1d, driverRootRadius),
                Math.Max(driverRadius, driverTipRadius),
                read.DriverTeeth,
                read.DriverTurns,
                driverPhase,
                driverRootHalf,
                driverTipHalf,
                read.DriverCaption,
                read.DriverState),
            new GearLayout(
                drivenCenter,
                drivenRadius,
                Math.Max(1d, drivenRootRadius),
                Math.Max(drivenRadius, drivenTipRadius),
                read.DrivenTeeth,
                read.DrivenProjectedTurns,
                drivenPhase,
                drivenRootHalf,
                drivenTipHalf,
                read.DrivenCaption,
                read.DrivenState));
    }

    public static void Render(
        SKCanvas canvas,
        GearDemoLayout layout,
        GearDemoInspection inspection,
        GearDemoInteraction interaction)
    {
        canvas.Clear(GojoDemoUi.Palette.Background);
        DrawOpenSpaces(canvas, layout);
        DrawSide(canvas, layout, GearDemoAdapters.ToRenderRead(inspection), interaction);
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

    private static double GearScale(Rect scene, GearDemoRenderRead read)
    {
        var driverTipRadius = read.DriverTipRadiusInches * BasePixelsPerInch;
        var drivenTipRadius = read.DrivenTipRadiusInches * BasePixelsPerInch;
        var centerDistance = read.CenterLineDistanceInches * BasePixelsPerInch;
        var rawWidth = centerDistance + driverTipRadius + drivenTipRadius;
        var rawHeight = Math.Max(driverTipRadius, drivenTipRadius) * 2.04d;
        var availableWidth = Math.Max(1d, scene.Width - (ScenePadding * 2d));
        var availableHeight = Math.Max(1d, scene.Height - LabelBand - (ScenePadding * 2d));
        var scale = Math.Min(1d, Math.Min(availableWidth / rawWidth, availableHeight / rawHeight));
        return Math.Max(0.28d, scale);
    }

    private static void DrawOpenSpaces(SKCanvas canvas, GearDemoLayout layout)
    {
        using var separator = new SKPaint
        {
            IsAntialias = true,
            Color = GojoDemoUi.Palette.Separator,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.2f
        };
        var x = layout.Side.Right + (Space * 0.5d);
        canvas.DrawLine(GearDemoAdapters.ToFloat(x), GearDemoAdapters.ToFloat(layout.Side.Y), GearDemoAdapters.ToFloat(x), GearDemoAdapters.ToFloat(layout.Side.Bottom), separator);
    }

    private static void DrawSide(
        SKCanvas canvas,
        GearDemoLayout layout,
        GearDemoRenderRead read,
        GearDemoInteraction interaction)
    {
        GojoDemoUi.DrawButton(canvas, layout.PlayButton, interaction.IsPlaying ? "pause" : "play", interaction.HoverPoint);
        GojoDemoUi.DrawButton(canvas, layout.ResetButton, "reset", interaction.HoverPoint);
        if (interaction.WebButtonText is { } webButtonText)
        {
            GojoDemoUi.DrawButton(canvas, layout.WebButton, webButtonText, interaction.HoverPoint);
        }

        if (!string.IsNullOrWhiteSpace(interaction.WebStatusText))
        {
            GojoDemoUi.DrawText(canvas, interaction.WebStatusText, layout.PlayButton.X, layout.PlayButton.Bottom + 8d, 11f, GojoDemoUi.Palette.Muted, false);
        }

        GojoDemoUi.DrawSlider(canvas, layout.SpeedSlider, interaction.HoverPoint);
        GojoDemoUi.DrawSlider(canvas, layout.DriverTeethSlider, interaction.HoverPoint);
        GojoDemoUi.DrawSlider(canvas, layout.DrivenTeethSlider, interaction.HoverPoint);

        var x = layout.SpeedSlider.Bounds.X;
        var y = layout.DrivenTeethSlider.Bounds.Bottom + 34d;
        GojoDemoUi.DrawReadout(canvas, x, ref y, "diameter", read.DiameterReadout);
        GojoDemoUi.DrawReadout(canvas, x, ref y, "mesh", read.MeshLabel);
        GojoDemoUi.DrawReadout(canvas, x, ref y, "centers", read.CenterReadout);
        GojoDemoUi.DrawReadout(canvas, x, ref y, "driver", read.DriverReadout);
        GojoDemoUi.DrawReadout(canvas, x, ref y, "driven read", read.DrivenReadout);
        GojoDemoUi.DrawReadout(canvas, x, ref y, "tension", read.TensionReadout);
    }

    private static void DrawScene(SKCanvas canvas, GearDemoLayout layout)
    {
        DrawConstructionCircles(canvas, layout.DriverGear, GojoDemoUi.Palette.DriverSoft);
        DrawConstructionCircles(canvas, layout.DrivenGear, GojoDemoUi.Palette.DrivenSoft);
        DrawMesh(canvas, layout);
        DrawGear(canvas, layout.DriverGear, GojoDemoUi.Palette.Driver, GojoDemoUi.Palette.DriverDark);
        DrawGear(canvas, layout.DrivenGear, GojoDemoUi.Palette.Driven, GojoDemoUi.Palette.DrivenDark);
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
        canvas.DrawCircle(GearDemoAdapters.ToFloat(gear.Center.X), GearDemoAdapters.ToFloat(gear.Center.Y), GearDemoAdapters.ToFloat(gear.Radius), fill);
        canvas.DrawCircle(GearDemoAdapters.ToFloat(gear.Center.X), GearDemoAdapters.ToFloat(gear.Center.Y), GearDemoAdapters.ToFloat(gear.RootRadius), outerStroke);
        canvas.DrawCircle(GearDemoAdapters.ToFloat(gear.Center.X), GearDemoAdapters.ToFloat(gear.Center.Y), GearDemoAdapters.ToFloat(gear.TipRadius), outerStroke);
        canvas.DrawCircle(GearDemoAdapters.ToFloat(gear.Center.X), GearDemoAdapters.ToFloat(gear.Center.Y), GearDemoAdapters.ToFloat(gear.Radius), pitchStroke);
    }

    private static void DrawMesh(SKCanvas canvas, GearDemoLayout layout)
    {
        using var stroke = new SKPaint
        {
            IsAntialias = true,
            Color = GojoDemoUi.Palette.Mesh.WithAlpha(180),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2f,
            PathEffect = SKPathEffect.CreateDash([8f, 7f], 0f)
        };
        canvas.DrawLine(GearDemoAdapters.ToPoint(layout.DriverGear.Center), GearDemoAdapters.ToPoint(layout.DrivenGear.Center), stroke);

        var contact = GearDemoAdapters.Interpolate(
            layout.DriverGear.Center,
            layout.DrivenGear.Center,
            layout.DriverGear.Radius / (layout.DriverGear.Radius + layout.DrivenGear.Radius));
        using var fill = new SKPaint { IsAntialias = true, Color = GojoDemoUi.Palette.Mesh, Style = SKPaintStyle.Fill };
        canvas.DrawCircle(GearDemoAdapters.ToFloat(contact.X), GearDemoAdapters.ToFloat(contact.Y), 5.5f, fill);
    }

    private static void DrawGear(SKCanvas canvas, GearLayout gear, SKColor fill, SKColor strokeColor)
    {
        using var path = GearPath(gear);
        using var paint = new SKPaint { IsAntialias = true, Color = fill, Style = SKPaintStyle.Fill };
        using var stroke = new SKPaint { IsAntialias = true, Color = strokeColor, Style = SKPaintStyle.Stroke, StrokeWidth = 2.4f, StrokeJoin = SKStrokeJoin.Round };
        canvas.DrawPath(path, paint);
        canvas.DrawPath(path, stroke);

        using var pitch = new SKPaint { IsAntialias = true, Color = strokeColor.WithAlpha(88), Style = SKPaintStyle.Stroke, StrokeWidth = 1.25f };
        canvas.DrawCircle(GearDemoAdapters.ToFloat(gear.Center.X), GearDemoAdapters.ToFloat(gear.Center.Y), GearDemoAdapters.ToFloat(gear.Radius), pitch);

        for (var index = 0; index < gear.ToothCount; index++)
        {
            var angle = GearDemoAdapters.GearAngle(gear, index / (double)gear.ToothCount);
            var from = GearDemoAdapters.Polar(gear.Center, gear.RootRadius + 4d, angle);
            var to = GearDemoAdapters.Polar(gear.Center, gear.TipRadius - 3d, angle);
            canvas.DrawLine(GearDemoAdapters.ToPoint(from), GearDemoAdapters.ToPoint(to), pitch);
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
            var a0 = GearDemoAdapters.GearAngle(gear, center - gear.RootHalfTurns);
            var a1 = GearDemoAdapters.GearAngle(gear, center - gear.TipHalfTurns);
            var a2 = GearDemoAdapters.GearAngle(gear, center + gear.TipHalfTurns);
            var a3 = GearDemoAdapters.GearAngle(gear, center + gear.RootHalfTurns);
            var p0 = GearDemoAdapters.Polar(gear.Center, gear.RootRadius, a0);
            var p1 = GearDemoAdapters.Polar(gear.Center, gear.TipRadius, a1);
            var p2 = GearDemoAdapters.Polar(gear.Center, gear.TipRadius, a2);
            var p3 = GearDemoAdapters.Polar(gear.Center, gear.RootRadius, a3);

            if (index == 0)
            {
                path.MoveTo(GearDemoAdapters.ToPoint(p0));
            }
            else
            {
                path.LineTo(GearDemoAdapters.ToPoint(p0));
            }

            path.LineTo(GearDemoAdapters.ToPoint(p1));
            path.LineTo(GearDemoAdapters.ToPoint(p2));
            path.LineTo(GearDemoAdapters.ToPoint(p3));
        }

        path.Close();
        return path;
    }

    private static void DrawSpokes(SKCanvas canvas, GearLayout gear, SKColor color)
    {
        const int spokeCount = 5;
        using var stroke = new SKPaint { IsAntialias = true, Color = color.WithAlpha(124), Style = SKPaintStyle.Stroke, StrokeWidth = 5.2f, StrokeCap = SKStrokeCap.Round };
        using var squareFill = new SKPaint { IsAntialias = true, Color = GojoDemoUi.Palette.Background, Style = SKPaintStyle.Fill };
        using var squareStroke = new SKPaint { IsAntialias = true, Color = color.WithAlpha(175), Style = SKPaintStyle.Stroke, StrokeWidth = 1.8f };

        for (var index = 0; index < spokeCount; index++)
        {
            var angle = GearDemoAdapters.GearAngle(gear, index / (double)spokeCount);
            var to = GearDemoAdapters.Polar(gear.Center, gear.Radius * 0.66d, angle);
            canvas.DrawLine(GearDemoAdapters.ToPoint(gear.Center), GearDemoAdapters.ToPoint(to), stroke);
            DrawRotatedSquare(canvas, to, Math.Clamp(gear.Radius * 0.13d, 12d, 20d), angle, squareFill, squareStroke);
        }

        using var hub = new SKPaint { IsAntialias = true, Color = GojoDemoUi.Palette.Background, Style = SKPaintStyle.Fill };
        using var hubStroke = new SKPaint { IsAntialias = true, Color = color, Style = SKPaintStyle.Stroke, StrokeWidth = 2.2f };
        canvas.DrawCircle(GearDemoAdapters.ToFloat(gear.Center.X), GearDemoAdapters.ToFloat(gear.Center.Y), GearDemoAdapters.ToFloat(Math.Max(16d, gear.Radius * 0.16d)), hub);
        canvas.DrawCircle(GearDemoAdapters.ToFloat(gear.Center.X), GearDemoAdapters.ToFloat(gear.Center.Y), GearDemoAdapters.ToFloat(Math.Max(16d, gear.Radius * 0.16d)), hubStroke);
    }

    private static void DrawCurrentTick(SKCanvas canvas, GearLayout gear, SKColor color)
    {
        var angle = GearDemoAdapters.GearAngle(gear, 0d);
        var from = GearDemoAdapters.Polar(gear.Center, gear.Radius * 0.28d, angle);
        var to = GearDemoAdapters.Polar(gear.Center, gear.TipRadius + 14d, angle);
        using var marker = new SKPaint { IsAntialias = true, Color = color, Style = SKPaintStyle.Stroke, StrokeWidth = 4.4f, StrokeCap = SKStrokeCap.Round };
        canvas.DrawLine(GearDemoAdapters.ToPoint(from), GearDemoAdapters.ToPoint(to), marker);
        using var fill = new SKPaint { IsAntialias = true, Color = GojoDemoUi.Palette.Background, Style = SKPaintStyle.Fill };
        using var stroke = new SKPaint { IsAntialias = true, Color = color, Style = SKPaintStyle.Stroke, StrokeWidth = 2.2f };
        DrawRotatedSquare(canvas, to, 14d, angle, fill, stroke);
    }

    private static void DrawRotatedSquare(SKCanvas canvas, Point center, double size, double angle, SKPaint fill, SKPaint stroke)
    {
        var half = GearDemoAdapters.ToFloat(size * 0.5d);
        canvas.Save();
        canvas.Translate(GearDemoAdapters.ToFloat(center.X), GearDemoAdapters.ToFloat(center.Y));
        canvas.RotateDegrees(GearDemoAdapters.ToFloat(angle * 180d / Math.PI));
        var rect = new SKRect(-half, -half, half, half);
        canvas.DrawRoundRect(rect, 2f, 2f, fill);
        canvas.DrawRoundRect(rect, 2f, 2f, stroke);
        canvas.Restore();
    }

    private static void DrawGearCaptions(SKCanvas canvas, GearDemoLayout layout)
    {
        var y = layout.Scene.Bottom - 64d;
        var half = layout.Scene.Width * 0.5d;
        GojoDemoUi.DrawCaption(canvas, new Rect(layout.Scene.X, y, half - 10d, 54d), layout.DriverGear.Caption, layout.DriverGear.State);
        GojoDemoUi.DrawCaption(canvas, new Rect(layout.Scene.X + half + 10d, y, half - 10d, 54d), layout.DrivenGear.Caption, layout.DrivenGear.State);
    }
}
