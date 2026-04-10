using System;
using System.Collections.Immutable;
using Avalonia;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;

namespace Core4.AvaloniaProbe.Rendering;

internal static class GlossaryRelationRenderer
{
    public static GlossaryRelationSceneLayout Arrange(Rect bounds, GlossaryRelationDemoState state)
    {
        var scene = new Rect(
            bounds.X + 26d,
            bounds.Y + 22d,
            bounds.Width - 52d,
            bounds.Height - 44d);

        var start = FromNormalized(scene, state.Start);
        var end = FromNormalized(scene, state.End);
        var site = Lerp(start, end, state.Site.Ratio.Approximate);
        var origin = Lerp(start, end, state.Origin.Ratio.Approximate);
        var titleCenter = new Point((start.X + end.X) * 0.5d, Math.Min(scene.Bottom - 48d, Math.Max(start.Y, end.Y) + 28d));
        var buttonCenter = new Point(titleCenter.X, Math.Min(scene.Bottom - 18d, titleCenter.Y + 25d));
        var relations = ImmutableArray.CreateBuilder<Point>(state.Relations.Length);
        foreach (var relation in state.Relations)
        {
            relations.Add(Lerp(start, end, relation.Placement.Ratio.Approximate));
        }

        return new GlossaryRelationSceneLayout(scene, start, end, site, origin, titleCenter, buttonCenter, relations.ToImmutable(), state);
    }

    public static void Render(DrawingContext context, Rect bounds, GlossaryRelationDemoState state)
    {
        var layout = Arrange(bounds, state);
        DrawBackdrop(context, layout.Scene);
        GlossaryClaimVisuals.DrawCarrier(context, layout.State.Carrier, layout.Start, layout.End);

        if (layout.State.HasOrigin)
        {
            DrawResolutionTicks(context, layout);
            DrawOrigin(context, layout);
        }

        DrawRelations(context, layout);
        DrawSite(context, layout);
        DrawHandles(context, layout);
        DrawLabels(context, layout);
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

    private static void DrawResolutionTicks(DrawingContext context, GlossaryRelationSceneLayout layout)
    {
        var direction = layout.End - layout.Start;
        var length = Math.Sqrt((direction.X * direction.X) + (direction.Y * direction.Y));
        if (length <= 1d)
        {
            return;
        }

        var unit = direction / length;
        var normal = new Vector(-unit.Y, unit.X);
        var pen = new Pen(new SolidColorBrush(Color.FromArgb(120, 47, 106, 63)), 1d);
        for (var step = 0; step <= 10; step++)
        {
            var t = step / 10d;
            var point = Lerp(layout.Start, layout.End, t);
            var size = step % 5 == 0 ? 7d : 4d;
            context.DrawLine(pen, point - (normal * size), point + (normal * size));
        }
    }

    private static void DrawOrigin(DrawingContext context, GlossaryRelationSceneLayout layout)
    {
        ScenePrimitives.DrawCircle(
            context,
            layout.Origin,
            8d,
            ProbePalette.Surface,
            new Pen(new SolidColorBrush(Color.Parse("#243336")), 1.5d));
        var anchor = layout.Origin + new Vector(0d, -36d);
        context.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(110, 107, 118, 128)), 1d), layout.Origin, anchor);
        ScenePrimitives.DrawCenteredBadgeLabel(
            context,
            "origin\n0.0",
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

    private static void DrawRelations(DrawingContext context, GlossaryRelationSceneLayout layout)
    {
        for (var index = 0; index < layout.Relations.Length; index++)
        {
            var point = layout.Relations[index];
            var relation = layout.State.Relations[index];
            var sign = index % 2 == 0 ? 1d : -1d;
            var anchor = point + new Vector(16d, sign * 18d);
            GlossaryClaimVisuals.DrawMeasureGlyph(context, layout.State.Carrier, point, relation.Placement.Ratio.Approximate);
            context.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(110, 107, 118, 128)), 1d), point, anchor + new Vector(-3d, sign * 3d));
            ScenePrimitives.DrawBadgeLabel(
                context,
                $"relation\n{relation.Units:0.0}",
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
    }

    private static void DrawSite(DrawingContext context, GlossaryRelationSceneLayout layout)
    {
        GlossaryClaimVisuals.DrawSiteGlyph(context, layout.State.Carrier, layout.Site, layout.State.Site.Ratio.Approximate);
        var anchor = layout.State.HasOrigin
            ? layout.Site + new Vector(18d, -38d)
            : layout.Site + new Vector(18d, -38d);
        context.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(110, 107, 118, 128)), 1d), layout.Site, anchor);
        var text = layout.State.HasOrigin
            ? $"site\nrelation: {layout.State.CurrentRelation:0.0}"
            : $"site\nmeasure: {layout.State.CurrentMeasure.Raw}";
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

    private static void DrawHandles(DrawingContext context, GlossaryRelationSceneLayout layout)
    {
        ScenePrimitives.DrawCircle(context, layout.Start, 6d, ProbePalette.Surface, GlossaryClaimVisuals.HandlePen);
        ScenePrimitives.DrawCircle(context, layout.End, 6d, ProbePalette.Surface, GlossaryClaimVisuals.HandlePen);
    }

    private static void DrawLabels(DrawingContext context, GlossaryRelationSceneLayout layout)
    {
        ScenePrimitives.DrawCenteredLabel(
            context,
            "Carrier",
            layout.TitleCenter,
            11d,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.UiFont);
        DrawEndpointLabels(context, layout);
    }

    private static void DrawEndpointLabels(DrawingContext context, GlossaryRelationSceneLayout layout)
    {
        var startPoint = layout.Start + GlossaryClaimVisuals.EndpointLabelOffset(layout.State.Carrier, true);
        var endPoint = layout.End + GlossaryClaimVisuals.EndpointLabelOffset(layout.State.Carrier, false);
        ScenePrimitives.DrawBadgeLabel(
            context,
            layout.State.StartValue.Raw.ToString(),
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
            layout.State.EndValue.Raw.ToString(),
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

internal readonly record struct GlossaryRelationSceneLayout(
    Rect Scene,
    Point Start,
    Point End,
    Point Site,
    Point Origin,
    Point TitleCenter,
    Point TitleButtonCenter,
    ImmutableArray<Point> Relations,
    GlossaryRelationDemoState State);
