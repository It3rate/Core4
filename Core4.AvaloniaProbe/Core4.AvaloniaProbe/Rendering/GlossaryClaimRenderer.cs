using System;
using System.Collections.Immutable;
using Avalonia;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;

namespace Core4.AvaloniaProbe.Rendering;

internal static class GlossaryClaimRenderer
{
    public static GlossaryClaimSceneLayout Arrange(Rect bounds, GlossaryClaimDemoState state)
    {
        var scene = new Rect(
            bounds.X + 26d,
            bounds.Y + 22d,
            bounds.Width - 52d,
            bounds.Height - 44d);

        var carriers = ImmutableArray.CreateBuilder<GlossaryClaimCarrierLayout>(state.Carriers.Length);
        for (var index = 0; index < state.Carriers.Length; index++)
        {
            var carrier = state.CarrierAt(index);
            var start = FromNormalized(scene, carrier.Start);
            var end = FromNormalized(scene, carrier.End);
            var site = Lerp(start, end, carrier.Site.Ratio.Approximate);
            var measures = ImmutableArray.CreateBuilder<Point>(carrier.Measures.Length);
            foreach (var measure in carrier.Measures)
            {
                measures.Add(Lerp(start, end, measure.Ratio.Approximate));
            }

            carriers.Add(new GlossaryClaimCarrierLayout(index, start, end, site, measures.ToImmutable(), carrier));
        }

        return new GlossaryClaimSceneLayout(scene, carriers.ToImmutable());
    }

    public static void Render(DrawingContext context, Rect bounds, GlossaryClaimDemoState state)
    {
        var layout = Arrange(bounds, state);
        DrawBackdrop(context, layout.Scene);

        foreach (var carrier in layout.Carriers)
        {
            GlossaryClaimVisuals.DrawCarrier(context, carrier.State.Carrier, carrier.Start, carrier.End);
        }

        foreach (var carrier in layout.Carriers)
        {
            DrawMeasures(context, carrier);
            DrawSite(context, carrier);
            DrawHandles(context, carrier);
            DrawLabels(context, carrier);
        }
    }

    public static Point Normalize(Rect bounds, Point point)
    {
        var scene = new Rect(
            bounds.X + 26d,
            bounds.Y + 22d,
            bounds.Width - 52d,
            bounds.Height - 44d);
        return new Point(
            (point.X - scene.X) / scene.Width,
            (point.Y - scene.Y) / scene.Height);
    }

    private static void DrawBackdrop(DrawingContext context, Rect scene)
    {
        var gridPen = new Pen(new SolidColorBrush(Color.FromArgb(38, 135, 121, 98)), 1d);
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

        for (var row = 1; row < 4; row++)
        {
            var y = scene.Y + (scene.Height * row / 4d);
            context.DrawLine(gridPen, new Point(scene.X + 18d, y), new Point(scene.Right - 18d, y));
        }

        for (var column = 1; column < 5; column++)
        {
            var x = scene.X + (scene.Width * column / 5d);
            context.DrawLine(gridPen, new Point(x, scene.Y + 16d), new Point(x, scene.Bottom - 16d));
        }
    }

    private static void DrawLabels(DrawingContext context, GlossaryClaimCarrierLayout carrier)
    {
        var midpoint = Lerp(carrier.Start, carrier.End, 0.5d);
        var direction = new Vector(carrier.End.X - carrier.Start.X, carrier.End.Y - carrier.Start.Y);
        var length = Math.Sqrt((direction.X * direction.X) + (direction.Y * direction.Y));
        var unit = length <= 0d ? new Vector(1d, 0d) : direction / length;
        var normal = new Vector(-unit.Y, unit.X);
        var labelPoint = midpoint + (normal * 28d);
        var subPoint = labelPoint + new Vector(0d, 18d);

        ScenePrimitives.DrawBadgeLabel(
            context,
            GlossaryClaimVisuals.Label(carrier.State.Carrier),
            labelPoint,
            11d,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.UiFont,
            ProbePalette.Surface,
            new Pen(new SolidColorBrush(Color.FromArgb(86, 197, 182, 160)), 0.9d));
        ScenePrimitives.DrawLabel(
            context,
            GlossaryClaimVisuals.Summary(carrier.State.Carrier),
            subPoint,
            10d,
            ProbePalette.Muted,
            fontFamily: ProbePalette.UiFont);
    }

    private static void DrawSite(DrawingContext context, GlossaryClaimCarrierLayout carrier)
    {
        GlossaryClaimVisuals.DrawSiteGlyph(context, carrier.State.Carrier, carrier.Site, carrier.State.Site.Ratio.Approximate);
        ScenePrimitives.DrawBadgeLabel(
            context,
            "site",
            carrier.Site + new Vector(13d, -24d),
            9d,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.MonoFont,
            ProbePalette.Surface,
            new Pen(new SolidColorBrush(Color.FromArgb(90, 197, 182, 160)), 0.8d),
            new Thickness(5, 2, 5, 2),
            8d);
    }

    private static void DrawMeasures(DrawingContext context, GlossaryClaimCarrierLayout carrier)
    {
        for (var index = 0; index < carrier.Measures.Length; index++)
        {
            var point = carrier.Measures[index];
            var sign = index % 2 == 0 ? 1d : -1d;
            var anchor = point + new Vector(14d, sign * 16d);
            GlossaryClaimVisuals.DrawMeasureGlyph(context, carrier.State.Carrier, point, carrier.State.Measures[index].Ratio.Approximate);
            context.DrawLine(
                new Pen(new SolidColorBrush(Color.FromArgb(110, 107, 118, 128)), 1d),
                point,
                anchor + new Vector(-4d, sign * 2d));
            ScenePrimitives.DrawBadgeLabel(
                context,
                GlossaryClaimVisuals.MeasureLabel(carrier.State.Carrier),
                anchor,
                9d,
                ProbePalette.Ink,
                fontFamily: ProbePalette.UiFont,
                background: ProbePalette.Surface,
                stroke: new Pen(new SolidColorBrush(Color.FromArgb(90, 197, 182, 160)), 0.8d),
                padding: new Thickness(5, 2, 5, 2),
                cornerRadius: 7d);
        }
    }

    private static void DrawHandles(DrawingContext context, GlossaryClaimCarrierLayout carrier)
    {
        ScenePrimitives.DrawCircle(context, carrier.Start, 6d, ProbePalette.Surface, GlossaryClaimVisuals.HandlePen);
        ScenePrimitives.DrawCircle(context, carrier.End, 6d, ProbePalette.Surface, GlossaryClaimVisuals.HandlePen);
    }

    private static Point FromNormalized(Rect scene, Point point) => new(
        scene.X + (scene.Width * point.X),
        scene.Y + (scene.Height * point.Y));

    private static Point Lerp(Point start, Point end, double t) => new(
        start.X + ((end.X - start.X) * t),
        start.Y + ((end.Y - start.Y) * t));
}

internal readonly record struct GlossaryClaimSceneLayout(Rect Scene, ImmutableArray<GlossaryClaimCarrierLayout> Carriers);

internal readonly record struct GlossaryClaimCarrierLayout(
    int Index,
    Point Start,
    Point End,
    Point Site,
    ImmutableArray<Point> Measures,
    GlossaryClaimCarrierState State);
