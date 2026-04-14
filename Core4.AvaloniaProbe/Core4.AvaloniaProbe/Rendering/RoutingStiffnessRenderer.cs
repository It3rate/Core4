using System;
using Avalonia;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;
using Core4.Dynamics;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Rendering;

/// <summary>
/// Schematic renderer for the routing/stiffness transport page. Structure mode emphasizes the four-site ports and
/// their roles; runtime mode keeps the same geometry but strips labels back so the current round reads more clearly.
/// </summary>
public static class RoutingStiffnessRenderer
{
    private const double HorizontalPadding = 42d;
    private const double VerticalPadding = 24d;
    private const double SupportBaseLength = 40d;
    private const double SupportVariableLength = 84d;
    private const double SiteHalfSize = 18d;
    private const double FeedDepth = 88d;
    private const double FeedLaneOffset = 52d;

    public static void Render(DrawingContext context, Rect bounds, RoutingStiffnessDemoState state, bool showStructure)
    {
        var scene = SceneRect(bounds);
        var layout = Arrange(scene);
        var field = state.CurrentField;
        var maxMagnitude = Math.Max(
            1d,
            Math.Max(
                state.SourceInput.Raw,
                Math.Max(
                    Math.Max(MaxPotential(state.CurrentSourceSite), MaxPotential(state.CurrentSinkSite)),
                    Math.Max(
                        Math.Max(field?.Published.Potential.Raw ?? 0L, field?.Delivered.Potential.Raw ?? 0L),
                        Math.Max(field?.Retained.Potential.Raw ?? 0L, Math.Max(state.SourceAbsorbed.Potential.Raw, state.SinkAbsorbed.Potential.Raw))))));

        DrawBackdrop(context, scene);
        DrawStageLabels(context, layout, state, showStructure);
        DrawSiteSkeleton(context, layout.Source, state.CurrentSourceSite.Kernel.Stiffness, "source x", showStructure);
        DrawSiteSkeleton(context, layout.Sink, state.CurrentSinkSite.Kernel.Stiffness, "listener y", showStructure);
        DrawSupportState(
            context,
            layout.Source,
            state.CurrentSourceSite,
            maxMagnitude,
            ProbePalette.DominantSoft,
            ProbePalette.Dominant,
            showStructure);
        DrawSupportState(
            context,
            layout.Sink,
            state.CurrentSinkSite,
            maxMagnitude,
            ProbePalette.DominantSoft,
            ProbePalette.Dominant,
            showStructure);
        DrawBottomInput(context, layout.Source, state.SourceInput.Absolute.Raw, maxMagnitude, "raw input", showStructure);
        DrawSelfLoop(context, layout.Source, state.SourceAbsorbed, maxMagnitude, showStructure);
        DrawCarrierTransport(context, layout.Source, layout.Sink, field, maxMagnitude, showStructure);
        DrawListenerIngress(context, layout.Sink, state, field, maxMagnitude, showStructure);
        DrawResidualBadge(context, layout.Source, state.CurrentSourceSite, showStructure);
        DrawResidualBadge(context, layout.Sink, state.CurrentSinkSite, showStructure);
        DrawSettledBadge(context, layout.Source, state.CurrentSourceSettlement, showStructure);
        DrawSettledBadge(context, layout.Sink, state.CurrentSinkSettlement, showStructure);
    }

    private static RoutingSceneLayout Arrange(Rect scene)
    {
        var sourceCenter = new Point(scene.X + (scene.Width * 0.28d), scene.Y + (scene.Height * 0.58d));
        var sinkCenter = new Point(scene.X + (scene.Width * 0.74d), scene.Y + (scene.Height * 0.58d));
        return new RoutingSceneLayout(
            scene,
            SitePorts.FromCenter(sourceCenter),
            SitePorts.FromCenter(sinkCenter));
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

    private static void DrawStageLabels(DrawingContext context, RoutingSceneLayout layout, RoutingStiffnessDemoState state, bool showStructure)
    {
        var topY = layout.Scene.Y + 6d;
        ScenePrimitives.DrawCenteredLabel(
            context,
            showStructure ? "structure: local ports and carrier shape" : "runtime: one local pass",
            new Point(layout.Source.Center.X, topY),
            9.6d,
            ProbePalette.Muted,
            FontWeight.SemiBold,
            ProbePalette.MonoFont);
        ScenePrimitives.DrawCenteredLabel(
            context,
            showStructure ? "shared orthogonal carrier" : "carrier publish / deliver",
            new Point((layout.Source.Top.X + layout.Sink.Top.X) * 0.5d, topY),
            9.6d,
            ProbePalette.Muted,
            FontWeight.SemiBold,
            ProbePalette.MonoFont);
        ScenePrimitives.DrawCenteredLabel(
            context,
            showStructure ? "structure: listener ports" : "runtime: next-round uptake",
            new Point(layout.Sink.Center.X, topY),
            9.6d,
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

    private static void DrawSiteSkeleton(
        DrawingContext context,
        SitePorts site,
        Ratio stiffness,
        string label,
        bool showStructure)
    {
        var supportGhostPen = new Pen(new SolidColorBrush(Color.Parse("#d4e2fb")), 2.4d, lineCap: PenLineCap.Round);
        context.DrawLine(supportGhostPen, site.Left, site.Left - new Vector(SupportBaseLength + 10d, 0d));
        context.DrawLine(supportGhostPen, site.Right, site.Right + new Vector(SupportBaseLength + 10d, 0d));

        var orthGhostOut = new Pen(new SolidColorBrush(Color.FromArgb(136, 209, 13, 31)), 2.2d, lineCap: PenLineCap.Round);
        var orthGhostIn = new Pen(new SolidColorBrush(Color.FromArgb(128, 255, 122, 0)), 2.2d, lineCap: PenLineCap.Round);
        context.DrawLine(orthGhostOut, site.Top, site.Top - new Vector(0d, 30d));
        context.DrawLine(orthGhostIn, site.Bottom + new Vector(0d, 30d), site.Bottom);

        var outerPen = new Pen(ProbePalette.Dominant, 2.2d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        var innerPen = new Pen(ProbePalette.OrthogonalInput, 1.2d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        context.DrawRectangle(Brushes.White, outerPen, site.OuterRect, 4d, 4d);
        context.DrawRectangle(Brushes.Transparent, innerPen, site.InnerRect, 3d, 3d);

        ScenePrimitives.DrawSquare(context, site.Left, 3.8d, Brushes.White, new Pen(ProbePalette.DominantSoft, 1.6d, lineCap: PenLineCap.Round));
        ScenePrimitives.DrawSquare(context, site.Right, 3.8d, Brushes.White, new Pen(ProbePalette.Dominant, 1.6d, lineCap: PenLineCap.Round));
        ScenePrimitives.DrawSquare(context, site.Top, 3.8d, Brushes.White, new Pen(ProbePalette.Orthogonal, 1.6d, lineCap: PenLineCap.Round));
        ScenePrimitives.DrawSquare(context, site.Bottom, 3.8d, Brushes.White, new Pen(ProbePalette.OrthogonalInput, 1.6d, lineCap: PenLineCap.Round));

        DrawStiffnessLock(context, site, stiffness);

        ScenePrimitives.DrawCenteredLabel(
            context,
            label,
            new Point(site.Center.X, site.Center.Y + 62d),
            10d,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.MonoFont);

        if (!showStructure)
        {
            return;
        }

        ScenePrimitives.DrawCenteredLabel(
            context,
            "orth out",
            new Point(site.Top.X, site.Top.Y - 46d),
            8.4d,
            ProbePalette.Orthogonal,
            fontFamily: ProbePalette.MonoFont);
        ScenePrimitives.DrawCenteredLabel(
            context,
            "orth in",
            new Point(site.Bottom.X, site.Bottom.Y + 30d),
            8.4d,
            ProbePalette.OrthogonalInput,
            fontFamily: ProbePalette.MonoFont);
        ScenePrimitives.DrawCenteredLabel(
            context,
            "recessive",
            new Point(site.Left.X - 56d, site.Left.Y + 8d),
            8.4d,
            ProbePalette.DominantSoft,
            fontFamily: ProbePalette.MonoFont);
        ScenePrimitives.DrawCenteredLabel(
            context,
            "dominant",
            new Point(site.Right.X + 56d, site.Right.Y + 8d),
            8.4d,
            ProbePalette.Dominant,
            fontFamily: ProbePalette.MonoFont);
    }

    private static void DrawSupportState(
        DrawingContext context,
        SitePorts site,
        Site current,
        double maxMagnitude,
        IBrush recessiveBrush,
        IBrush dominantBrush,
        bool showStructure)
    {
        DrawSupportRay(
            context,
            site.Left,
            -1d,
            current.Kernel.Recessive.Potential.Raw,
            maxMagnitude,
            recessiveBrush);
        DrawSupportRay(
            context,
            site.Right,
            1d,
            current.Kernel.Dominant.Potential.Raw,
            maxMagnitude,
            dominantBrush);
    }

    private static void DrawStiffnessLock(
        DrawingContext context,
        SitePorts site,
        Ratio stiffness)
    {
        var ratio = Math.Clamp(stiffness.Approximate, 0d, 1d);
        if (ratio <= 0d)
        {
            return;
        }

        var alpha = (byte)Math.Round(255d * ratio, MidpointRounding.AwayFromZero);
        var brush = new SolidColorBrush(Color.FromArgb(alpha, 176, 15, 179));
        var stroke = new Pen(brush, 1.2d + (1.2d * ratio), lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        var shackleTop = new Point(site.Center.X, site.Center.Y - 3d);
        var bodyRect = new Rect(site.Center.X - 5d, site.Center.Y + 1d, 10d, 8d);
        var shackle = new StreamGeometry();
        using (var stream = shackle.Open())
        {
            stream.BeginFigure(new Point(site.Center.X - 4d, site.Center.Y + 2d), false);
            stream.CubicBezierTo(
                new Point(site.Center.X - 4d, shackleTop.Y),
                new Point(site.Center.X + 4d, shackleTop.Y),
                new Point(site.Center.X + 4d, site.Center.Y + 2d));
            stream.EndFigure(false);
        }

        context.DrawGeometry(null, stroke, shackle);
        context.DrawRectangle(new SolidColorBrush(Color.FromArgb((byte)Math.Round(alpha * 0.22d), 176, 15, 179)), stroke, bodyRect, 2d, 2d);
    }

    private static void DrawSupportRay(
        DrawingContext context,
        Point port,
        double direction,
        long magnitude,
        double maxMagnitude,
        IBrush brush)
    {
        if (magnitude <= 0L)
        {
            return;
        }

        var length = SupportBaseLength + (SupportVariableLength * Math.Clamp(magnitude / Math.Max(1d, maxMagnitude), 0d, 1d));
        var end = port + new Vector(direction * length, 0d);
        var pen = DynamicPen(brush, magnitude, maxMagnitude, 2.2d, 7.2d);
        context.DrawLine(pen, port, end);
        ScenePrimitives.DrawDirectedTriangle(
            context,
            end,
            8.4d,
            DynamicBrush(brush, magnitude, maxMagnitude),
            new Vector(direction, 0d),
            new Pen(DynamicBrush(brush, magnitude, maxMagnitude), 1d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round));
    }

    private static void DrawBottomInput(
        DrawingContext context,
        SitePorts source,
        long inputMagnitude,
        double maxMagnitude,
        string label,
        bool showStructure)
    {
        if (inputMagnitude <= 0L)
        {
            return;
        }

        var start = source.Bottom + new Vector(-FeedLaneOffset, FeedDepth);
        DrawFeedCurve(
            context,
            start,
            source.Bottom,
            inputMagnitude,
            maxMagnitude,
            ProbePalette.OrthogonalInput,
            label,
            showStructure);
    }

    private static void DrawListenerIngress(
        DrawingContext context,
        SitePorts sink,
        RoutingStiffnessDemoState state,
        CarrierFieldState? field,
        double maxMagnitude,
        bool showStructure)
    {
        var pending = field?.Delivered ?? Ratio.Zero(Measure.Zero);
        var currentIncoming = state.CurrentSinkSettlement?.Incoming ?? Ratio.Zero(Measure.Zero);
        var shown = currentIncoming.Potential != Measure.Zero ? currentIncoming : pending;

        if (shown.Potential == Measure.Zero)
        {
            return;
        }

        var start = sink.Bottom + new Vector(FeedLaneOffset, FeedDepth);
        DrawFeedCurve(
            context,
            start,
            sink.Bottom,
            shown.Potential.Raw,
            maxMagnitude,
            ProbePalette.OrthogonalInput,
            currentIncoming.Potential != Measure.Zero ? "listener in" : "pending",
            showStructure);
    }

    private static void DrawSelfLoop(
        DrawingContext context,
        SitePorts site,
        Ratio absorbed,
        double maxMagnitude,
        bool showStructure)
    {
        if (absorbed.Potential == Measure.Zero)
        {
            return;
        }

        var start = site.Top;
        var end = site.Right;
        var geometry = new StreamGeometry();

        using (var stream = geometry.Open())
        {
            stream.BeginFigure(start, false);
            stream.CubicBezierTo(
                start + new Vector(0d, -46d),
                end + new Vector(42d, -28d),
                end);
            stream.EndFigure(false);
        }

        var pen = DynamicPen(ProbePalette.TensionPositive, absorbed.Potential.Raw, maxMagnitude, 2.4d, 8d);
        context.DrawGeometry(null, pen, geometry);
        ScenePrimitives.DrawDirectedTriangle(
            context,
            end,
            8.2d,
            DynamicBrush(ProbePalette.TensionPositive, absorbed.Potential.Raw, maxMagnitude),
            new Vector(-1d, 0d),
            new Pen(DynamicBrush(ProbePalette.TensionPositive, absorbed.Potential.Raw, maxMagnitude), 1d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round));

        if (showStructure)
        {
            ScenePrimitives.DrawCenteredBadgeLabel(
                context,
                $"route {Format(absorbed)}",
                new Point(site.Center.X + 44d, site.Top.Y - 54d),
                8.2d,
                ProbePalette.Ink,
                fontFamily: ProbePalette.MonoFont);
        }
    }

    private static void DrawCarrierTransport(
        DrawingContext context,
        SitePorts source,
        SitePorts sink,
        CarrierFieldState? field,
        double maxMagnitude,
        bool showStructure)
    {
        var published = field?.Published ?? Ratio.Zero(Measure.Zero);
        var delivered = field?.Delivered ?? Ratio.Zero(Measure.Zero);

        var handoff = new Point(sink.Bottom.X - 74d, sink.Bottom.Y + 26d);
        ScenePrimitives.DrawCenteredLabel(
            context,
            "carrier p",
            new Point((source.Top.X + sink.Bottom.X) * 0.5d, source.Top.Y - 54d),
            8.8d,
            ProbePalette.Muted,
            FontWeight.SemiBold,
            ProbePalette.MonoFont);

        if (published.Potential == Measure.Zero)
        {
            if (showStructure)
            {
                DrawGhostTransport(context, source.Top, handoff, sink.Bottom);
            }

            return;
        }

        var firstHalf = new StreamGeometry();
        using (var stream = firstHalf.Open())
        {
            stream.BeginFigure(source.Top, false);
            stream.CubicBezierTo(
                source.Top + new Vector(0d, -62d),
                new Point(sink.Center.X - 154d, sink.Bottom.Y + 42d),
                handoff);
            stream.EndFigure(false);
        }

        var secondHalf = new StreamGeometry();
        using (var stream = secondHalf.Open())
        {
            stream.BeginFigure(handoff, false);
            stream.CubicBezierTo(
                new Point(sink.Bottom.X - 40d, sink.Bottom.Y + 40d),
                new Point(sink.Bottom.X - 8d, sink.Bottom.Y + 22d),
                sink.Bottom);
            stream.EndFigure(false);
        }

        context.DrawGeometry(null, DynamicPen(ProbePalette.Orthogonal, published.Potential.Raw, maxMagnitude, 2.6d, 8.4d), firstHalf);
        context.DrawGeometry(null, DynamicPen(ProbePalette.OrthogonalInput, delivered.Potential.Raw == 0 ? published.Potential.Raw : delivered.Potential.Raw, maxMagnitude, 2.6d, 8.4d), secondHalf);

        ScenePrimitives.DrawDirectedTriangle(
            context,
            handoff,
            8.2d,
            DynamicBrush(ProbePalette.Orthogonal, published.Potential.Raw, maxMagnitude),
            handoff - source.Top,
            new Pen(DynamicBrush(ProbePalette.Orthogonal, published.Potential.Raw, maxMagnitude), 1d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round));
        ScenePrimitives.DrawDirectedTriangle(
            context,
            sink.Bottom,
            8.2d,
            DynamicBrush(ProbePalette.OrthogonalInput, delivered.Potential.Raw == 0 ? published.Potential.Raw : delivered.Potential.Raw, maxMagnitude),
            sink.Bottom - handoff,
            new Pen(DynamicBrush(ProbePalette.OrthogonalInput, delivered.Potential.Raw == 0 ? published.Potential.Raw : delivered.Potential.Raw, maxMagnitude), 1d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round));

        ScenePrimitives.DrawCenteredBadgeLabel(
            context,
            $"pub {Format(published)}",
            new Point((source.Top.X + handoff.X) * 0.5d, source.Top.Y - 22d),
            8.6d,
            ProbePalette.Ink,
            fontFamily: ProbePalette.MonoFont);

        if (delivered.Potential != Measure.Zero)
        {
            ScenePrimitives.DrawCenteredBadgeLabel(
                context,
                $"del {Format(delivered)}",
                new Point((handoff.X + sink.Bottom.X) * 0.5d, sink.Bottom.Y + 18d),
                8.6d,
                ProbePalette.Ink,
                fontFamily: ProbePalette.MonoFont);
        }
    }

    private static void DrawGhostTransport(DrawingContext context, Point start, Point handoff, Point end)
    {
        var ghostRedPen = new Pen(new SolidColorBrush(Color.FromArgb(112, 209, 13, 31)), 2d, lineCap: PenLineCap.Round);
        var ghostOrangePen = new Pen(new SolidColorBrush(Color.FromArgb(96, 255, 122, 0)), 2d, lineCap: PenLineCap.Round);
        var first = new StreamGeometry();
        using (var stream = first.Open())
        {
            stream.BeginFigure(start, false);
            stream.CubicBezierTo(
                start + new Vector(0d, -62d),
                new Point(end.X - 154d, end.Y + 42d),
                handoff);
            stream.EndFigure(false);
        }

        var second = new StreamGeometry();
        using (var stream = second.Open())
        {
            stream.BeginFigure(handoff, false);
            stream.CubicBezierTo(
                new Point(end.X - 40d, end.Y + 40d),
                new Point(end.X - 8d, end.Y + 22d),
                end);
            stream.EndFigure(false);
        }

        context.DrawGeometry(null, ghostRedPen, first);
        context.DrawGeometry(null, ghostOrangePen, second);
    }

    private static void DrawFeedCurve(
        DrawingContext context,
        Point start,
        Point end,
        long magnitude,
        double maxMagnitude,
        IBrush brush,
        string label,
        bool showStructure)
    {
        var geometry = new StreamGeometry();

        using (var stream = geometry.Open())
        {
            stream.BeginFigure(start, false);
            stream.CubicBezierTo(
                new Point(start.X, start.Y - 42d),
                new Point(end.X - ((start.X < end.X) ? 18d : -18d), end.Y + 36d),
                end);
            stream.EndFigure(false);
        }

        var pen = InputPen(brush, magnitude, maxMagnitude);
        context.DrawGeometry(null, pen, geometry);
        ScenePrimitives.DrawDirectedTriangle(
            context,
            end,
            8.2d,
            InputBrush(brush, magnitude, maxMagnitude),
            new Vector(0d, -1d),
            new Pen(InputBrush(brush, magnitude, maxMagnitude), 1d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round));

        if (showStructure)
        {
            ScenePrimitives.DrawCenteredBadgeLabel(
                context,
                label,
                new Point(start.X, start.Y + 8d),
                8.2d,
                ProbePalette.Ink,
                fontFamily: ProbePalette.MonoFont);
        }
    }

    private static void DrawResidualBadge(DrawingContext context, SitePorts site, Site current, bool showStructure)
    {
        if (current.Orthogonal.Residual.Potential == Measure.Zero)
        {
            return;
        }

        ScenePrimitives.DrawCenteredBadgeLabel(
            context,
            $"res {Format(current.Orthogonal.Residual)}",
            new Point(site.Center.X, site.Center.Y - 72d),
            8.2d,
            showStructure ? ProbePalette.Ink : ProbePalette.Muted,
            fontFamily: ProbePalette.MonoFont);
    }

    private static void DrawSettledBadge(DrawingContext context, SitePorts site, SiteSettlement? settlement, bool showStructure)
    {
        if (settlement is not { } current)
        {
            return;
        }

        ScenePrimitives.DrawCenteredBadgeLabel(
            context,
            $"settled {Format(current.Settled)}",
            new Point(site.Center.X, site.Center.Y + 82d),
            8.2d,
            showStructure ? ProbePalette.Muted : ProbePalette.Ink,
            fontFamily: ProbePalette.MonoFont);
    }

    private static Pen DynamicPen(IBrush baseBrush, double magnitude, double maxMagnitude, double minimumWidth, double maximumWidth, bool dashed = false)
    {
        var ratio = Math.Clamp(magnitude / Math.Max(1d, maxMagnitude), 0d, 1d);
        return new Pen(
            DynamicBrush(baseBrush, magnitude, maxMagnitude),
            minimumWidth + ((maximumWidth - minimumWidth) * ratio),
            dashStyle: dashed ? new DashStyle([0.4d, 5.2d], 0d) : null,
            lineCap: PenLineCap.Round,
            lineJoin: PenLineJoin.Round);
    }

    private static Pen InputPen(IBrush baseBrush, double magnitude, double maxMagnitude)
    {
        var ratio = Math.Clamp(magnitude / Math.Max(1d, maxMagnitude), 0d, 1d);
        return new Pen(
            InputBrush(baseBrush, magnitude, maxMagnitude),
            2.2d + (4.2d * ratio),
            lineCap: PenLineCap.Round,
            lineJoin: PenLineJoin.Round);
    }

    private static IBrush DynamicBrush(IBrush baseBrush, double magnitude, double maxMagnitude)
    {
        if (baseBrush is not ISolidColorBrush solid)
        {
            return baseBrush;
        }

        var ratio = Math.Clamp(magnitude / Math.Max(1d, maxMagnitude), 0d, 1d);
        var alpha = (byte)Math.Round(128d + (127d * ratio), MidpointRounding.AwayFromZero);
        var color = solid.Color;
        return new SolidColorBrush(Color.FromArgb(alpha, color.R, color.G, color.B));
    }

    private static IBrush InputBrush(IBrush baseBrush, double magnitude, double maxMagnitude)
    {
        if (baseBrush is not ISolidColorBrush solid)
        {
            return baseBrush;
        }

        var ratio = Math.Clamp(magnitude / Math.Max(1d, maxMagnitude), 0d, 1d);
        var alpha = (byte)Math.Round(128d + (36d * ratio), MidpointRounding.AwayFromZero);
        var color = solid.Color;
        return new SolidColorBrush(Color.FromArgb(alpha, color.R, color.G, color.B));
    }

    private static long MaxPotential(Site site) =>
        Math.Max(
            site.Orthogonal.Residual.Potential.Raw,
            Math.Max(site.Kernel.Recessive.Potential.Raw, site.Kernel.Dominant.Potential.Raw));

    private static string Format(Ratio value) => value.Normalize().ToString();

    private readonly record struct RoutingSceneLayout(Rect Scene, SitePorts Source, SitePorts Sink);

    private readonly record struct SitePorts(
        Point Center,
        Point Left,
        Point Right,
        Point Top,
        Point Bottom,
        Rect OuterRect,
        Rect InnerRect)
    {
        public static SitePorts FromCenter(Point center)
        {
            var outer = new Rect(center.X - SiteHalfSize, center.Y - SiteHalfSize, SiteHalfSize * 2d, SiteHalfSize * 2d);
            var inner = new Rect(center.X - (SiteHalfSize - 5d), center.Y - (SiteHalfSize - 5d), (SiteHalfSize - 5d) * 2d, (SiteHalfSize - 5d) * 2d);
            return new SitePorts(
                center,
                new Point(center.X - SiteHalfSize, center.Y),
                new Point(center.X + SiteHalfSize, center.Y),
                new Point(center.X, center.Y - SiteHalfSize),
                new Point(center.X, center.Y + SiteHalfSize),
                outer,
                inner);
        }
    }
}
