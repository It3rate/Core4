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
            var titleCenter = new Point(
                (start.X + end.X) * 0.5d,
                Math.Min(scene.Bottom - 48d, Math.Max(start.Y, end.Y) + 26d));
            var titleButtonCenter = new Point(titleCenter.X, Math.Min(scene.Bottom - 20d, titleCenter.Y + 24d));
            var pins = ImmutableArray.CreateBuilder<Point>(carrier.Pins.Length);
            foreach (var pin in carrier.Pins)
            {
                pins.Add(Lerp(start, end, pin.Placement.Ratio.Approximate));
            }

            carriers.Add(new GlossaryClaimCarrierLayout(index, start, end, site, titleCenter, titleButtonCenter, pins.ToImmutable(), carrier));
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
            DrawPins(context, carrier);
            DrawSite(context, layout.Scene, carrier);
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
        ScenePrimitives.DrawCenteredLabel(
            context,
            GlossaryClaimVisuals.GraphLabel(carrier.State.Carrier),
            carrier.TitleCenter,
            11d,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.UiFont);

        DrawEndpointLabels(context, carrier);
    }

    private static void DrawSite(DrawingContext context, Rect scene, GlossaryClaimCarrierLayout carrier)
    {
        GlossaryClaimVisuals.DrawSiteGlyph(context, carrier.State.Carrier, carrier.Site, carrier.State.Site.Ratio.Approximate);
        var direction = new Vector(carrier.End.X - carrier.Start.X, carrier.End.Y - carrier.Start.Y);
        var anchor = SiteBadgeAnchor(scene, carrier.Site, direction);

        context.DrawLine(
            new Pen(new SolidColorBrush(Color.FromArgb(110, 107, 118, 128)), 1d),
            carrier.Site,
            anchor);
        ScenePrimitives.DrawCenteredBadgeLabel(
            context,
            $"site\nmeasure: {carrier.State.CurrentValue.Raw}",
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

    private static Point SiteBadgeAnchor(Rect scene, Point site, Vector direction)
    {
        Vector offset;
        if (Math.Abs(direction.X) < Math.Abs(direction.Y) * 0.45d)
        {
            offset = new Vector(74d, 34d);
        }
        else if ((direction.X * direction.Y) < 0d)
        {
            offset = new Vector(72d, 28d);
        }
        else
        {
            offset = new Vector(72d, -28d);
        }

        var anchor = site + offset;
        return new Point(
            Math.Clamp(anchor.X, scene.X + 84d, scene.Right - 84d),
            Math.Clamp(anchor.Y, scene.Y + 28d, scene.Bottom - 28d));
    }

    private static void DrawPins(DrawingContext context, GlossaryClaimCarrierLayout carrier)
    {
        for (var index = 0; index < carrier.Pins.Length; index++)
        {
            var point = carrier.Pins[index];
            var sign = index % 2 == 0 ? 1d : -1d;
            var anchor = point + new Vector(14d, sign * 16d);
            GlossaryClaimVisuals.DrawMeasureGlyph(context, carrier.State.Carrier, point, carrier.State.Pins[index].Placement.Ratio.Approximate);
            context.DrawLine(
                new Pen(new SolidColorBrush(Color.FromArgb(110, 107, 118, 128)), 1d),
                point,
                anchor + new Vector(-4d, sign * 2d));
            ScenePrimitives.DrawBadgeLabel(
                context,
                carrier.State.Pins[index].Value.Raw.ToString(),
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

    private static void DrawEndpointLabels(DrawingContext context, GlossaryClaimCarrierLayout carrier)
    {
        var startPoint = carrier.Start + GlossaryClaimVisuals.EndpointLabelOffset(carrier.State.Carrier, true);
        var endPoint = carrier.End + GlossaryClaimVisuals.EndpointLabelOffset(carrier.State.Carrier, false);
        ScenePrimitives.DrawBadgeLabel(
            context,
            carrier.State.StartValue.Raw.ToString(),
            startPoint,
            9d,
            ProbePalette.Ink,
            fontFamily: ProbePalette.MonoFont,
            background: ProbePalette.Surface,
            stroke: new Pen(new SolidColorBrush(Color.FromArgb(90, 197, 182, 160)), 0.8d),
            padding: new Thickness(5, 2, 5, 2),
            cornerRadius: 8d);
        ScenePrimitives.DrawBadgeLabel(
            context,
            carrier.State.EndValue.Raw.ToString(),
            endPoint,
            9d,
            ProbePalette.Ink,
            fontFamily: ProbePalette.MonoFont,
            background: ProbePalette.Surface,
            stroke: new Pen(new SolidColorBrush(Color.FromArgb(90, 197, 182, 160)), 0.8d),
            padding: new Thickness(5, 2, 5, 2),
            cornerRadius: 8d);
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
    Point TitleCenter,
    Point TitleButtonCenter,
    ImmutableArray<Point> Pins,
    GlossaryClaimCarrierState State);
