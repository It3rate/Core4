using System;
using Avalonia;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Rendering;

/// <summary>
/// Static anatomy renderer for one site, emphasizing the current engine mechanics and letting the user focus one slice at a time.
/// </summary>
public static class SiteAnatomyRenderer
{
    private const double HorizontalPadding = 38d;
    private const double VerticalPadding = -40d;
    private const double SiteHalfSize = 20d;
    private const double SupportLength = 122d;
    private const double InputMaxCircle = 14d;
    private const double PublishOffsetX = 74d;
    private const double ListenOffsetX = 68d;

    public static void Render(DrawingContext context, Rect bounds, SiteAnatomyDemoState state, SiteAnatomyFocus focus)
    {
        var scene = new Rect(
            bounds.X + HorizontalPadding,
            bounds.Y + VerticalPadding,
            Math.Max(64d, bounds.Width - (HorizontalPadding * 2d)),
            Math.Max(64d, bounds.Height + (VerticalPadding + 20d)));
        var center = new Point(scene.Center.X, scene.Y + (scene.Height * 0.56d));
        var site = SitePorts.FromCenter(center);

        DrawBackdrop(context, scene);
        DrawSectionLabels(context, scene, center);
        DrawSupport(context, site, state, focus);
        DrawResidualIn(context, site, state, focus);
        DrawListenPorts(context, site, state, focus);
        DrawResidualOut(context, site, state, focus);
        DrawPublishPorts(context, site, state, focus);
        DrawSite(context, site, state, focus);
        DrawSelfRoute(context, site, state, focus);
        DrawCallouts(context, site, state, focus);
    }

    private static void DrawBackdrop(DrawingContext context, Rect rect)
    {
        context.DrawRectangle(null, new Pen(ProbePalette.Axis, 1d), rect, 18d, 18d);
    }

    private static void DrawSectionLabels(DrawingContext context, Rect rect, Point center)
    {
        ScenePrimitives.DrawCenteredLabel(
            context,
            "carrier publish ports",
            new Point(center.X, rect.Y + 6d),
            10d,
            ProbePalette.Muted,
            FontWeight.SemiBold,
            ProbePalette.MonoFont);
        ScenePrimitives.DrawCenteredLabel(
            context,
            "site state",
            new Point(center.X, center.Y - 82d),
            10d,
            ProbePalette.Muted,
            FontWeight.SemiBold,
            ProbePalette.MonoFont);
        ScenePrimitives.DrawCenteredLabel(
            context,
            "carrier inbound ports",
            new Point(center.X, rect.Bottom - 28d),
            10d,
            ProbePalette.Muted,
            FontWeight.SemiBold,
            ProbePalette.MonoFont);
    }

    private static void DrawSupport(DrawingContext context, SitePorts site, SiteAnatomyDemoState state, SiteAnatomyFocus focus)
    {
        var alignedFocused = IsFocused(focus, SiteAnatomyFocus.Aligned);
        var claimFocused = IsFocused(focus, SiteAnatomyFocus.Claim);
        var recessiveStart = site.Left - new Vector(SupportLength, 0d);
        var dominantTip = site.Right + new Vector(SupportLength, 0d);
        var alignedSoft = PresentedBrush(ProbePalette.DominantSoft, focus, SiteAnatomyFocus.Aligned);
        var alignedStrong = PresentedBrush(ProbePalette.Dominant, focus, SiteAnatomyFocus.Aligned);
        var claimBrush = PresentedBrush(ProbePalette.Dominant, focus, SiteAnatomyFocus.Claim);
        var routeBrush = PresentedBrush(ProbePalette.TensionPositive, focus, SiteAnatomyFocus.SelfRoute);

        DrawArrowLine(
            context,
            site.Right,
            dominantTip,
            alignedStrong,
            5.1d,
            alignedFocused || claimFocused,
            true);

        var routedShare = Math.Clamp(state.Route.Approximate * state.Bias.Approximate, 0d, 1d);
        if (routedShare > 0d)
        {
            var overlayTip = site.Right + new Vector(Math.Max(18d, SupportLength * routedShare), 0d);
            DrawArrowLine(
                context,
                site.Right,
                overlayTip,
                routeBrush,
                5.2d,
                IsFocused(focus, SiteAnatomyFocus.SelfRoute) || claimFocused,
                false);
        }

        var claimX = dominantTip.X;
        var claimPen = new Pen(claimBrush, 2.4d, lineCap: PenLineCap.Round);
        if (claimFocused)
        {
            context.DrawLine(new Pen(BrushWithAlpha(BrushColor(ProbePalette.Surface), 240), 8d, lineCap: PenLineCap.Round), new Point(claimX, site.Right.Y - 14d), new Point(claimX, site.Right.Y + 14d));
        }

        context.DrawLine(claimPen, new Point(claimX, site.Right.Y - 11d), new Point(claimX, site.Right.Y + 11d));

        var recessivePen = new Pen(alignedSoft, 5.1d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        if (alignedFocused)
        {
            context.DrawLine(new Pen(BrushWithAlpha(BrushColor(ProbePalette.Surface), 240), 10d, lineCap: PenLineCap.Round), recessiveStart, site.Left);
        }

        context.DrawLine(recessivePen, recessiveStart, site.Left);
        ScenePrimitives.DrawCircle(
            context,
            recessiveStart,
            6.8d,
            Brushes.White,
            new Pen(alignedSoft, 2.2d, lineCap: PenLineCap.Round));
    }

    private static void DrawResidualIn(DrawingContext context, SitePorts site, SiteAnatomyDemoState state, SiteAnatomyFocus focus)
    {
        var incomingFocused = IsFocused(focus, SiteAnatomyFocus.OrthogonalIn);
        var cloudCenter = site.Bottom + new Vector(0d, 98d);
        DrawIncomingCircleBranch(
            context,
            cloudCenter,
            site.Bottom,
            state.Residual.Potential.Raw,
            6L,
            true,
            PresentedBrush(ProbePalette.OrthogonalInput, focus, SiteAnatomyFocus.OrthogonalIn),
            incomingFocused);
    }

    private static void DrawListenPorts(DrawingContext context, SitePorts site, SiteAnatomyDemoState state, SiteAnatomyFocus focus)
    {
        var incomingFocused = IsFocused(focus, SiteAnatomyFocus.OrthogonalIn) || IsFocused(focus, SiteAnatomyFocus.Ports);
        var leftStart = new Point(site.Center.X - ListenOffsetX, site.Bottom.Y + 82d);
        var rightStart = new Point(site.Center.X + ListenOffsetX, site.Bottom.Y + 82d);
        var brush = PresentedBrush(ProbePalette.OrthogonalInput, focus, SiteAnatomyFocus.OrthogonalIn, SiteAnatomyFocus.Ports);
        DrawIncomingCircleBranch(
            context,
            leftStart,
            site.Bottom + new Vector(-10d, 2d),
            state.ListenPorts[0].Value.Potential.Raw,
            6L,
            false,
            brush,
            incomingFocused);
        DrawIncomingCircleBranch(
            context,
            rightStart,
            site.Bottom + new Vector(10d, 2d),
            state.ListenPorts[1].Value.Potential.Raw,
            6L,
            false,
            brush,
            incomingFocused);
    }

    private static void DrawResidualOut(DrawingContext context, SitePorts site, SiteAnatomyDemoState state, SiteAnatomyFocus focus)
    {
        var outgoingFocused = IsFocused(focus, SiteAnatomyFocus.OrthogonalOut);
        var tip = site.Top - new Vector(0d, 68d);
        var geometry = CurvedOut(site.Top, tip, 0d);
        DrawArrowGeometry(context, geometry, tip, new Vector(0d, -1d), PresentedBrush(ProbePalette.Orthogonal, focus, SiteAnatomyFocus.OrthogonalOut), 3.8d, outgoingFocused, 174);
    }

    private static void DrawPublishPorts(DrawingContext context, SitePorts site, SiteAnatomyDemoState state, SiteAnatomyFocus focus)
    {
        var outgoingFocused = IsFocused(focus, SiteAnatomyFocus.OrthogonalOut) || IsFocused(focus, SiteAnatomyFocus.Ports);
        var leftTip = new Point(site.Center.X - PublishOffsetX, site.Top.Y - 88d);
        var rightTip = new Point(site.Center.X + PublishOffsetX, site.Top.Y - 88d);
        var brush = PresentedBrush(ProbePalette.Orthogonal, focus, SiteAnatomyFocus.OrthogonalOut, SiteAnatomyFocus.Ports);

        var leftGeometry = CurvedOut(site.Top + new Vector(-3d, -6d), leftTip, -46d);
        var rightGeometry = CurvedOut(site.Top + new Vector(3d, -6d), rightTip, 46d);
        DrawArrowGeometry(context, leftGeometry, leftTip, new Vector(-1d, -0.35d), brush, 3d, outgoingFocused, 150);
        DrawArrowGeometry(context, rightGeometry, rightTip, new Vector(1d, -0.35d), brush, 3d, outgoingFocused, 150);
    }

    private static void DrawSite(DrawingContext context, SitePorts site, SiteAnatomyDemoState state, SiteAnatomyFocus focus)
    {
        if (IsFocused(focus, SiteAnatomyFocus.Aligned) || IsFocused(focus, SiteAnatomyFocus.Claim))
        {
            context.DrawRectangle(
                null,
                new Pen(BrushWithAlpha(BrushColor(ProbePalette.Surface), 240), 8d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round),
                new Rect(site.OuterRect.X - 6d, site.OuterRect.Y - 6d, site.OuterRect.Width + 12d, site.OuterRect.Height + 12d),
                9d,
                9d);
        }

        context.DrawRectangle(Brushes.White, new Pen(PresentedBrush(ProbePalette.Dominant, focus, SiteAnatomyFocus.Aligned), 2.4d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round), site.OuterRect, 5d, 5d);
        context.DrawRectangle(Brushes.Transparent, new Pen(PresentedBrush(ProbePalette.OrthogonalInput, focus, SiteAnatomyFocus.OrthogonalIn), 1.2d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round), site.InnerRect, 3d, 3d);
        ScenePrimitives.DrawSquare(context, site.Left, 4d, Brushes.White, new Pen(PresentedBrush(ProbePalette.DominantSoft, focus, SiteAnatomyFocus.Aligned), 1.8d, lineCap: PenLineCap.Round));
        ScenePrimitives.DrawSquare(context, site.Right, 4d, Brushes.White, new Pen(PresentedBrush(ProbePalette.Dominant, focus, SiteAnatomyFocus.Aligned), 1.8d, lineCap: PenLineCap.Round));
        ScenePrimitives.DrawSquare(context, site.Top, 4d, Brushes.White, new Pen(PresentedBrush(ProbePalette.Orthogonal, focus, SiteAnatomyFocus.OrthogonalOut), 1.8d, lineCap: PenLineCap.Round));
        ScenePrimitives.DrawSquare(context, site.Bottom, 4d, Brushes.White, new Pen(PresentedBrush(ProbePalette.OrthogonalInput, focus, SiteAnatomyFocus.OrthogonalIn), 1.8d, lineCap: PenLineCap.Round));
        DrawLock(context, site.Center, state.Stiffness, PresentedBrush(ProbePalette.Origin, focus, SiteAnatomyFocus.Claim), IsFocused(focus, SiteAnatomyFocus.Claim));
        ScenePrimitives.DrawCenteredLabel(
            context,
            "site",
            new Point(site.Center.X, site.Center.Y + 38d),
            10d,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.MonoFont);
    }

    private static void DrawLock(DrawingContext context, Point center, Ratio stiffness, IBrush baseBrush, bool highlighted)
    {
        var alpha = (byte)Math.Round(128d + (127d * stiffness.Approximate), MidpointRounding.AwayFromZero);
        var color = BrushColor(baseBrush);
        var brush = new SolidColorBrush(Color.FromArgb(alpha, color.R, color.G, color.B));
        var stroke = new Pen(brush, 1.6d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        if (highlighted)
        {
            var halo = new Pen(BrushWithAlpha(BrushColor(ProbePalette.Surface), 240), 7d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
            var haloRect = new Rect(center.X - 7d, center.Y - 1d, 14d, 14d);
            context.DrawRectangle(null, halo, haloRect, 4d, 4d);
        }

        var bodyRect = new Rect(center.X - 5d, center.Y + 1d, 10d, 8d);
        var shackle = new StreamGeometry();
        using (var stream = shackle.Open())
        {
            stream.BeginFigure(new Point(center.X - 4d, center.Y + 2d), false);
            stream.CubicBezierTo(
                new Point(center.X - 4d, center.Y - 4d),
                new Point(center.X + 4d, center.Y - 4d),
                new Point(center.X + 4d, center.Y + 2d));
            stream.EndFigure(false);
        }

        context.DrawGeometry(null, stroke, shackle);
        context.DrawRectangle(new SolidColorBrush(Color.FromArgb((byte)Math.Round(alpha * 0.22d), color.R, color.G, color.B)), stroke, bodyRect, 2d, 2d);
    }

    private static void DrawSelfRoute(DrawingContext context, SitePorts site, SiteAnatomyDemoState state, SiteAnatomyFocus focus)
    {
        var selfFocused = IsFocused(focus, SiteAnatomyFocus.SelfRoute);
        var geometry = new StreamGeometry();
        using (var stream = geometry.Open())
        {
            stream.BeginFigure(site.Top, false);
            stream.CubicBezierTo(
                site.Top + new Vector(8d, -58d),
                site.Right + new Vector(52d, -28d),
                site.Right + new Vector(2d, 0d));
            stream.EndFigure(false);
        }

        DrawArrowGeometry(context, geometry, site.Right + new Vector(2d, 0d), new Vector(-1d, 0d), PresentedBrush(ProbePalette.TensionPositive, focus, SiteAnatomyFocus.SelfRoute), 4.4d, selfFocused, 200);
    }

    private static void DrawCallouts(DrawingContext context, SitePorts site, SiteAnatomyDemoState state, SiteAnatomyFocus focus)
    {
        var leftStart = site.Left - new Vector(SupportLength, 0d);
        var dominantTip = site.Right + new Vector(SupportLength, 0d);
        var claimX = dominantTip.X;
        var leftListen = new Point(site.Center.X - ListenOffsetX, site.Bottom.Y + 82d);
        var rightListen = new Point(site.Center.X + ListenOffsetX, site.Bottom.Y + 82d);
        var leftPublish = new Point(site.Center.X - PublishOffsetX, site.Top.Y - 88d);
        var rightPublish = new Point(site.Center.X + PublishOffsetX, site.Top.Y - 88d);
        var residualIn = site.Bottom + new Vector(0d, 98d);
        var residualOut = site.Top - new Vector(0d, 68d);

        DrawCallout(
            context,
            $"aligned recessive {Format(state.Recessive)}",
            new Point(site.Center.X - 206d, site.Center.Y - 40d),
            leftStart + new Vector(0d, -2d),
            ProbePalette.DominantSoft,
            IsFocused(focus, SiteAnatomyFocus.Aligned));
        DrawCallout(
            context,
            $"dominant aligned {Format(state.Dominant)}",
            new Point(site.Center.X + 214d, site.Center.Y - 44d),
            dominantTip + new Vector(0d, -2d),
            ProbePalette.Dominant,
            IsFocused(focus, SiteAnatomyFocus.Aligned));
        DrawCallout(
            context,
            $"claim marker {Format(state.Dominant)}",
            new Point(site.Center.X + 218d, site.Center.Y - 4d),
            new Point(claimX, site.Right.Y),
            ProbePalette.Dominant,
            IsFocused(focus, SiteAnatomyFocus.Claim));
        DrawCallout(
            context,
            $"stiffness {Format(state.Stiffness)}",
            new Point(site.Center.X - 182d, site.Center.Y + 32d),
            site.Center,
            ProbePalette.Origin,
            IsFocused(focus, SiteAnatomyFocus.Claim));
        DrawCallout(
            context,
            $"self-route {Format(state.Route)}",
            new Point(site.Center.X + 104d, site.Center.Y - 54d),
            site.Right + new Vector(18d, -18d),
            ProbePalette.TensionPositive,
            IsFocused(focus, SiteAnatomyFocus.SelfRoute));
        DrawCallout(
            context,
            $"orth residual out {Format(state.Residual)}",
            new Point(site.Center.X, site.Center.Y - 150d),
            residualOut,
            ProbePalette.Orthogonal,
            IsFocused(focus, SiteAnatomyFocus.OrthogonalOut));
        DrawCallout(
            context,
            $"publish p {Format(state.PublishPorts[0].Value)}",
            new Point(site.Center.X - 214d, site.Center.Y - 134d),
            leftPublish,
            ProbePalette.Orthogonal,
            IsFocused(focus, SiteAnatomyFocus.Ports) || IsFocused(focus, SiteAnatomyFocus.OrthogonalOut));
        DrawCallout(
            context,
            $"publish q {Format(state.PublishPorts[1].Value)}",
            new Point(site.Center.X + 214d, site.Center.Y - 134d),
            rightPublish,
            ProbePalette.Orthogonal,
            IsFocused(focus, SiteAnatomyFocus.Ports) || IsFocused(focus, SiteAnatomyFocus.OrthogonalOut));
        DrawCallout(
            context,
            $"listen p {Format(state.ListenPorts[0].Value)}",
            new Point(site.Center.X - 210d, site.Center.Y + 146d),
            leftListen,
            ProbePalette.OrthogonalInput,
            IsFocused(focus, SiteAnatomyFocus.Ports) || IsFocused(focus, SiteAnatomyFocus.OrthogonalIn));
        DrawCallout(
            context,
            $"listen q {Format(state.ListenPorts[1].Value)}",
            new Point(site.Center.X + 210d, site.Center.Y + 146d),
            rightListen,
            ProbePalette.OrthogonalInput,
            IsFocused(focus, SiteAnatomyFocus.Ports) || IsFocused(focus, SiteAnatomyFocus.OrthogonalIn));
        DrawCallout(
            context,
            $"residual in {Format(state.Residual)}",
            new Point(site.Center.X, site.Center.Y + 174d),
            residualIn,
            ProbePalette.OrthogonalInput,
            IsFocused(focus, SiteAnatomyFocus.OrthogonalIn));
    }

    private static void DrawCallout(
        DrawingContext context,
        string text,
        Point center,
        Point target,
        IBrush brush,
        bool highlighted)
    {
        var calloutPen = new Pen(
            BrushWithAlpha(Color.Parse("#b6b3ab"), highlighted ? (byte)196 : (byte)152),
            highlighted ? 1.35d : 1.05d,
            lineCap: PenLineCap.Round,
            lineJoin: PenLineJoin.Round);
        context.DrawLine(calloutPen, target, center);
        ScenePrimitives.DrawCircle(context, target, 2.3d, BrushWithAlpha(Color.Parse("#c9c5bc"), 220));

        ScenePrimitives.DrawCenteredBadgeLabel(
            context,
            text,
            center,
            8.2d,
            highlighted ? brush : ProbePalette.Ink,
            fontFamily: ProbePalette.MonoFont,
            background: highlighted ? new SolidColorBrush(Color.FromArgb(240, 255, 255, 255)) : new SolidColorBrush(Color.FromArgb(184, 255, 255, 255)),
            stroke: new Pen(highlighted ? BrushWithAlpha(BrushColor(brush), 196) : new SolidColorBrush(Color.FromArgb(92, 197, 182, 160)), highlighted ? 1.4d : 0.9d));
    }

    private static void DrawIncomingCircleBranch(
        DrawingContext context,
        Point start,
        Point tip,
        long magnitude,
        long maxMagnitude,
        bool central,
        IBrush baseBrush,
        bool highlighted)
    {
        var geometry = central
            ? CurvedIn(start, tip, 0d)
            : CurvedIn(start, tip, start.X < tip.X ? -32d : 32d);

        var radius = IncomingCircleRadius(magnitude, maxMagnitude);
        var baseColor = BrushColor(baseBrush);
        var alpha = IncomingAlpha(magnitude, maxMagnitude);
        var brush = new SolidColorBrush(Color.FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B));
        var fill = new SolidColorBrush(Color.FromArgb((byte)Math.Round(alpha * 0.18d), baseColor.R, baseColor.G, baseColor.B));
        var width = 2.2d + (2.2d * Math.Clamp(magnitude / Math.Max(1d, (double)maxMagnitude), 0d, 1d));

        if (highlighted)
        {
            context.DrawGeometry(null, new Pen(BrushWithAlpha(BrushColor(ProbePalette.Surface), 240), width + 5d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round), geometry);
        }

        context.DrawGeometry(null, new Pen(brush, width, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round), geometry);
        ScenePrimitives.DrawCircle(context, start, radius, fill, new Pen(brush, Math.Max(1.4d, width * 0.55d), lineCap: PenLineCap.Round));
        DrawArrowHead(context, tip, new Vector(0d, -1d), brush, width, highlighted);
    }

    private static void DrawArrowLine(
        DrawingContext context,
        Point start,
        Point tip,
        IBrush brush,
        double width,
        bool highlighted,
        bool circleAtStart)
    {
        var direction = tip - start;
        var unit = Normalize(direction);
        var arrowTip = tip;
        var lineEnd = tip - (unit * 4.2d);
        var pen = new Pen(brush, width, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        if (highlighted)
        {
            context.DrawLine(new Pen(BrushWithAlpha(BrushColor(ProbePalette.Surface), 240), width + 5d, lineCap: PenLineCap.Round), start, lineEnd);
        }

        context.DrawLine(pen, start, lineEnd);

        if (circleAtStart)
        {
            ScenePrimitives.DrawCircle(context, start, 6.2d, Brushes.White, new Pen(brush, 2.2d, lineCap: PenLineCap.Round));
        }

        DrawArrowHead(context, arrowTip, unit, brush, width, highlighted);
    }

    private static void DrawArrowGeometry(
        DrawingContext context,
        Geometry geometry,
        Point tip,
        Vector direction,
        IBrush brush,
        double width,
        bool highlighted,
        byte alpha)
    {
        var shaded = BrushWithAlpha(BrushColor(brush), alpha);
        var pen = new Pen(shaded, width, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        if (highlighted)
        {
            context.DrawGeometry(null, new Pen(BrushWithAlpha(BrushColor(ProbePalette.Surface), 240), width + 5d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round), geometry);
        }

        context.DrawGeometry(null, pen, geometry);
        DrawArrowHead(context, tip, direction, shaded, width, highlighted);
    }

    private static void DrawArrowHead(DrawingContext context, Point tip, Vector direction, IBrush brush, double width, bool highlighted)
    {
        if (highlighted)
        {
            ScenePrimitives.DrawDirectedTriangle(
                context,
                tip,
                9.2d,
                BrushWithAlpha(BrushColor(ProbePalette.Surface), 240),
                direction,
                new Pen(BrushWithAlpha(BrushColor(ProbePalette.Surface), 240), width + 3d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round));
        }

        ScenePrimitives.DrawDirectedTriangle(
            context,
            tip,
            8.4d,
            brush,
            direction,
            new Pen(brush, Math.Max(1d, width * 0.28d), lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round));
    }

    private static StreamGeometry CurvedOut(Point start, Point tip, double horizontalDrift)
    {
        var geometry = new StreamGeometry();
        using (var stream = geometry.Open())
        {
            stream.BeginFigure(start, false);
            stream.CubicBezierTo(
                start + new Vector(horizontalDrift * 0.18d, -48d),
                tip + new Vector(-horizontalDrift * 0.4d, 26d),
                tip);
            stream.EndFigure(false);
        }

        return geometry;
    }

    private static StreamGeometry CurvedIn(Point start, Point tip, double horizontalDrift)
    {
        var geometry = new StreamGeometry();
        using (var stream = geometry.Open())
        {
            stream.BeginFigure(start, false);
            stream.CubicBezierTo(
                start + new Vector(horizontalDrift * 0.24d, -38d),
                tip + new Vector(-horizontalDrift * 0.2d, 34d),
                tip);
            stream.EndFigure(false);
        }

        return geometry;
    }

    private static bool IsFocused(SiteAnatomyFocus current, SiteAnatomyFocus area) =>
        current == SiteAnatomyFocus.All || current == area;

    private static IBrush PresentedBrush(IBrush baseBrush, SiteAnatomyFocus current, params SiteAnatomyFocus[] areas)
    {
        if (current == SiteAnatomyFocus.All || Array.IndexOf(areas, current) >= 0)
        {
            return baseBrush;
        }

        var color = BrushColor(baseBrush);
        var muted = Blend(color, Color.Parse("#9e9b95"), 0.72d);
        return new SolidColorBrush(Color.FromArgb(168, muted.R, muted.G, muted.B));
    }

    private static byte IncomingAlpha(long magnitude, long maxMagnitude)
    {
        var ratio = Math.Clamp(magnitude / Math.Max(1d, (double)maxMagnitude), 0d, 1d);
        return (byte)Math.Round(92d + (96d * ratio), MidpointRounding.AwayFromZero);
    }

    private static double IncomingCircleRadius(long magnitude, long maxMagnitude)
    {
        var ratio = Math.Clamp(magnitude / Math.Max(1d, (double)maxMagnitude), 0d, 1d);
        return 6d + ((InputMaxCircle - 6d) * ratio);
    }

    private static Color Blend(Color a, Color b, double weightToB)
    {
        var wb = Math.Clamp(weightToB, 0d, 1d);
        var wa = 1d - wb;
        return Color.FromArgb(
            255,
            (byte)Math.Round((a.R * wa) + (b.R * wb), MidpointRounding.AwayFromZero),
            (byte)Math.Round((a.G * wa) + (b.G * wb), MidpointRounding.AwayFromZero),
            (byte)Math.Round((a.B * wa) + (b.B * wb), MidpointRounding.AwayFromZero));
    }

    private static Vector Normalize(Vector value)
    {
        var length = Math.Sqrt((value.X * value.X) + (value.Y * value.Y));
        return length <= 0d
            ? new Vector(1d, 0d)
            : new Vector(value.X / length, value.Y / length);
    }

    private static Color BrushColor(IBrush brush) => brush is ISolidColorBrush solid
        ? solid.Color
        : Color.Parse("#243336");

    private static SolidColorBrush BrushWithAlpha(Color color, byte alpha) => new(Color.FromArgb(alpha, color.R, color.G, color.B));

    private static string Format(Ratio ratio) => ratio.Normalize().ToString();

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
