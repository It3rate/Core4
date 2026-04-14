using System;
using Avalonia;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;
using Core4.Dynamics;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Rendering;

/// <summary>
/// Schematic renderer for the routing/stiffness transport page. It separates one same-round local self-return from
/// one shared-carrier publish/deliver field so the runtime transport law is visible at a glance.
/// </summary>
public static class RoutingStiffnessRenderer
{
    private const double HorizontalPadding = 42d;
    private const double VerticalPadding = 24d;
    private const double AxisHalfLength = 88d;
    private const double PortHeight = 96d;
    private const double BarHalfLength = 74d;

    public static void Render(DrawingContext context, Rect bounds, RoutingStiffnessDemoState state)
    {
        var scene = SceneRect(bounds);
        var layout = Arrange(scene);
        var field = state.CurrentField;
        var maxMagnitude = Math.Max(
            1d,
            Math.Max(
                Math.Max(state.SourceInput.Raw, MaxPotential(state.CurrentSourceSite)),
                Math.Max(
                    Math.Max(MaxPotential(state.CurrentSinkSite), field?.Published.Potential.Raw ?? 0L),
                    Math.Max(field?.Delivered.Potential.Raw ?? 0L, field?.Retained.Potential.Raw ?? 0L))));

        DrawBackdrop(context, scene);
        DrawStageLabels(context, layout, state);
        DrawSharedCarrier(context, layout, state, field);
        DrawSiteMachine(
            context,
            layout.SourceSite,
            layout.SourcePort,
            "source x",
            state.CurrentSourceSite,
            state.CurrentSourceSettlement,
            state.SourceAbsorbed,
            maxMagnitude,
            ProbePalette.Dominant,
            ProbePalette.DominantSoft,
            true);
        DrawSiteMachine(
            context,
            layout.SinkSite,
            layout.SinkPort,
            "listener y",
            state.CurrentSinkSite,
            state.CurrentSinkSettlement,
            state.SinkAbsorbed,
            maxMagnitude,
            ProbePalette.Orthogonal,
            ProbePalette.Overlay,
            false);
    }

    private static RoutingSceneLayout Arrange(Rect scene)
    {
        var sourceSite = new Point(scene.X + (scene.Width * 0.26d), scene.Y + (scene.Height * 0.68d));
        var sinkSite = new Point(scene.X + (scene.Width * 0.74d), scene.Y + (scene.Height * 0.68d));
        return new RoutingSceneLayout(
            scene,
            sourceSite,
            sinkSite,
            new Point(sourceSite.X, sourceSite.Y - PortHeight),
            new Point(sinkSite.X, sinkSite.Y - PortHeight));
    }

    private static Rect SceneRect(Rect bounds) => new(
        bounds.X + HorizontalPadding,
        bounds.Y + VerticalPadding,
        Math.Max(64d, bounds.Width - (HorizontalPadding * 2d)),
        Math.Max(64d, bounds.Height - (VerticalPadding * 2d)));

    private static void DrawBackdrop(DrawingContext context, Rect rect)
    {
        context.DrawRectangle(null, new Pen(ProbePalette.Axis, 1d), rect, 18d, 18d);

        var verticalThird = rect.Width / 3d;
        var horizontalHalf = rect.Height / 2d;

        for (var index = 1; index < 3; index++)
        {
            var x = rect.X + (verticalThird * index);
            context.DrawLine(ProbePalette.TickPen, new Point(x, rect.Y + 8d), new Point(x, rect.Bottom - 8d));
        }

        context.DrawLine(
            ProbePalette.TickPen,
            new Point(rect.X + 8d, rect.Y + horizontalHalf),
            new Point(rect.Right - 8d, rect.Y + horizontalHalf));
    }

    private static void DrawStageLabels(DrawingContext context, RoutingSceneLayout layout, RoutingStiffnessDemoState state)
    {
        ScenePrimitives.DrawCenteredLabel(
            context,
            "same-round local self-return",
            new Point(layout.SourceSite.X, layout.Scene.Y + 6d),
            10d,
            ProbePalette.Muted,
            FontWeight.SemiBold,
            ProbePalette.MonoFont);
        ScenePrimitives.DrawCenteredLabel(
            context,
            "shared carrier p",
            new Point((layout.SourcePort.X + layout.SinkPort.X) * 0.5d, layout.Scene.Y + 6d),
            10d,
            ProbePalette.Muted,
            FontWeight.SemiBold,
            ProbePalette.MonoFont);
        ScenePrimitives.DrawCenteredLabel(
            context,
            "next-round listener settlement",
            new Point(layout.SinkSite.X, layout.Scene.Y + 6d),
            10d,
            ProbePalette.Muted,
            FontWeight.SemiBold,
            ProbePalette.MonoFont);

        ScenePrimitives.DrawCenteredBadgeLabel(
            context,
            $"round {state.CurrentRound.Raw}",
            new Point(layout.Scene.Center.X, layout.Scene.Bottom - 16d),
            10d,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.MonoFont);
    }

    private static void DrawSharedCarrier(
        DrawingContext context,
        RoutingSceneLayout layout,
        RoutingStiffnessDemoState state,
        CarrierFieldState? field)
    {
        context.DrawLine(ProbePalette.AxisPen, layout.SourcePort, layout.SinkPort);
        context.DrawLine(ProbePalette.AxisPen, layout.SourceSite, layout.SourcePort);
        context.DrawLine(ProbePalette.AxisPen, layout.SinkSite, layout.SinkPort);

        ScenePrimitives.DrawCenteredLabel(
            context,
            "p",
            new Point((layout.SourcePort.X + layout.SinkPort.X) * 0.5d, layout.SourcePort.Y - 16d),
            11d,
            ProbePalette.Ink,
            FontWeight.Bold,
            ProbePalette.MonoFont);

        if (field is not { } currentField || currentField.Published.Potential == Measure.Zero)
        {
            ScenePrimitives.DrawCenteredBadgeLabel(
                context,
                "no field publish",
                new Point((layout.SourcePort.X + layout.SinkPort.X) * 0.5d, layout.SourcePort.Y + 2d),
                9.5d,
                ProbePalette.Muted,
                fontFamily: ProbePalette.MonoFont);
            return;
        }

        var publishedPen = new Pen(ProbePalette.Orthogonal, 4.2d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        var deliveredPen = new Pen(ProbePalette.OrthogonalInput, 2.4d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        context.DrawLine(publishedPen, layout.SourcePort, layout.SinkPort);
        ScenePrimitives.DrawDirectedTriangle(
            context,
            layout.SinkPort,
            8.8d,
            ProbePalette.Orthogonal,
            layout.SinkPort - layout.SourcePort,
            new Pen(ProbePalette.Orthogonal, 1d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round));

        if (currentField.Delivered.Potential != Measure.Zero)
        {
            var insetStart = new Point(layout.SourcePort.X + 18d, layout.SourcePort.Y);
            var insetEnd = new Point(layout.SinkPort.X - 18d, layout.SinkPort.Y);
            context.DrawLine(deliveredPen, insetStart, insetEnd);
        }

        ScenePrimitives.DrawCenteredBadgeLabel(
            context,
            $"pub {Format(currentField.Published)}  del {Format(currentField.Delivered)}",
            new Point((layout.SourcePort.X + layout.SinkPort.X) * 0.5d, layout.SourcePort.Y - 2d),
            9.2d,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.MonoFont);

        if (currentField.Retained.Potential != Measure.Zero)
        {
            ScenePrimitives.DrawCenteredBadgeLabel(
                context,
                $"ret {Format(currentField.Retained)}",
                new Point((layout.SourcePort.X + layout.SinkPort.X) * 0.5d, layout.SourcePort.Y + 26d),
                8.8d,
                ProbePalette.Muted,
                fontFamily: ProbePalette.MonoFont);
        }
    }

    private static void DrawSiteMachine(
        DrawingContext context,
        Point sitePoint,
        Point portPoint,
        string label,
        Site site,
        SiteSettlement? settlement,
        Ratio absorbed,
        double maxMagnitude,
        IBrush dominantBrush,
        IBrush recessiveBrush,
        bool sourceSide)
    {
        var axisStart = new Point(sitePoint.X - AxisHalfLength, sitePoint.Y);
        var axisEnd = new Point(sitePoint.X + AxisHalfLength, sitePoint.Y);
        context.DrawLine(ProbePalette.AxisPen, axisStart, axisEnd);

        DrawAlignedBar(
            context,
            sitePoint,
            -1d,
            MagnitudeLength(site.Kernel.Recessive, maxMagnitude),
            recessiveBrush,
            sourceSide ? "support" : "anchor");
        DrawAlignedBar(
            context,
            sitePoint,
            1d,
            MagnitudeLength(site.Kernel.Dominant, maxMagnitude),
            dominantBrush,
            sourceSide ? "claim" : "claim");

        if (site.Orthogonal.Residual.Potential != Measure.Zero)
        {
            ScenePrimitives.DrawCircle(
                context,
                sitePoint,
                16d,
                Brushes.Transparent,
                new Pen(ProbePalette.TensionPositive, 2.1d, dashStyle: new DashStyle([0.2d, 4d], 0d), lineCap: PenLineCap.Round));
            ScenePrimitives.DrawBadgeLabel(
                context,
                $"res {Format(site.Orthogonal.Residual)}",
                sitePoint + new Vector(-26d, 18d),
                8.8d,
                ProbePalette.Ink,
                fontFamily: ProbePalette.MonoFont);
        }

        ScenePrimitives.DrawCircle(context, sitePoint, 12d, Brushes.White, new Pen(ProbePalette.Ink, 2.2d, lineCap: PenLineCap.Round));
        ScenePrimitives.DrawCircle(context, sitePoint, 4.2d, sourceSide ? ProbePalette.Origin : ProbePalette.OrthogonalInput);

        ScenePrimitives.DrawCenteredLabel(
            context,
            label,
            new Point(sitePoint.X, sitePoint.Y + 16d),
            10d,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.MonoFont);

        if (settlement is { } currentSettlement && absorbed.Potential != Measure.Zero)
        {
            DrawReturnLoop(context, sitePoint, absorbed, sourceSide ? ProbePalette.TensionPositive : ProbePalette.OrthogonalInput);
            ScenePrimitives.DrawCenteredBadgeLabel(
                context,
                $"local {Format(absorbed)}",
                new Point(sitePoint.X, sitePoint.Y - 50d),
                8.8d,
                ProbePalette.Ink,
                fontFamily: ProbePalette.MonoFont);

            ScenePrimitives.DrawBadgeLabel(
                context,
                $"in {Format(currentSettlement.Incoming)}",
                sitePoint + new Vector(-30d, -2d),
                8.6d,
                ProbePalette.Muted,
                fontFamily: ProbePalette.MonoFont);
        }

        var portBrush = sourceSide ? ProbePalette.Orthogonal : ProbePalette.OrthogonalInput;
        var portPen = new Pen(portBrush, 2.6d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        context.DrawLine(portPen, sitePoint + new Vector(0d, -12d), portPoint);
        ScenePrimitives.DrawCircle(context, portPoint, 5d, Brushes.White, new Pen(portBrush, 1.8d, lineCap: PenLineCap.Round));
    }

    private static void DrawAlignedBar(
        DrawingContext context,
        Point center,
        double direction,
        double length,
        IBrush brush,
        string label)
    {
        if (length <= 0.01d)
        {
            return;
        }

        var end = center + new Vector(direction * length, 0d);
        var pen = new Pen(brush, 4.2d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        context.DrawLine(pen, center, end);

        if (direction > 0d)
        {
            ScenePrimitives.DrawDirectedTriangle(
                context,
                end,
                8.2d,
                brush,
                new Vector(1d, 0d),
                new Pen(brush, 1d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round));
        }
        else
        {
            ScenePrimitives.DrawDirectedTriangle(
                context,
                end,
                8.2d,
                brush,
                new Vector(-1d, 0d),
                new Pen(brush, 1d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round));
        }

        ScenePrimitives.DrawCenteredLabel(
            context,
            label,
            new Point((center.X + end.X) * 0.5d, center.Y + 10d),
            8.4d,
            ProbePalette.Muted,
            fontFamily: ProbePalette.MonoFont);
    }

    private static void DrawReturnLoop(DrawingContext context, Point sitePoint, Ratio absorbed, IBrush brush)
    {
        var start = sitePoint + new Vector(-22d, -8d);
        var end = sitePoint + new Vector(0d, -8d);
        var controlLeft = sitePoint + new Vector(-26d, -46d);
        var controlRight = sitePoint + new Vector(24d, -46d);
        var geometry = new StreamGeometry();

        using (var stream = geometry.Open())
        {
            stream.BeginFigure(start, false);
            stream.CubicBezierTo(controlLeft, controlRight, end);
            stream.EndFigure(false);
        }

        context.DrawGeometry(null, new Pen(brush, 2d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round), geometry);
        ScenePrimitives.DrawDirectedTriangle(
            context,
            end + new Vector(0d, 6d),
            7.4d,
            brush,
            new Vector(0d, 1d),
            new Pen(brush, 1d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round));
    }

    private static double MagnitudeLength(Ratio value, double maxMagnitude)
    {
        var magnitude = Math.Max(0d, value.Potential.Raw);
        return BarHalfLength * (magnitude / maxMagnitude);
    }

    private static long MaxPotential(Site site) =>
        Math.Max(
            site.Orthogonal.Residual.Potential.Raw,
            Math.Max(site.Kernel.Recessive.Potential.Raw, site.Kernel.Dominant.Potential.Raw));

    private static string Format(Ratio value) => value.Normalize().ToString();

    private readonly record struct RoutingSceneLayout(
        Rect Scene,
        Point SourceSite,
        Point SinkSite,
        Point SourcePort,
        Point SinkPort);
}
