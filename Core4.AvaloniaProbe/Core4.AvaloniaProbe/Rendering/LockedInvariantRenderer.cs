using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;

namespace Core4.AvaloniaProbe.Rendering;

/// <summary>
/// Renderer for the first locked-invariant explorer. Each primitive family keeps one revealed invariant fixed and
/// shows the compatible motion that follows when one participant is perturbed.
/// </summary>
public static class LockedInvariantRenderer
{
    private static readonly IBrush AreaFill = new SolidColorBrush(Color.Parse("#ecebe8"));
    private static readonly IBrush AntiAreaFill = new SolidColorBrush(Color.Parse("#e3ece1"));
    private static readonly IBrush TraceBrush = new SolidColorBrush(Color.Parse("#bfa778"));
    private static readonly IBrush GraphTraceBrush = new SolidColorBrush(Color.Parse("#b08a56"));
    private static readonly IBrush InsetSurface = new SolidColorBrush(Color.Parse("#f4efe6"));
    private static readonly IBrush InsetStroke = new SolidColorBrush(Color.Parse("#d8cebb"));
    private static readonly IBrush RayBrush = new SolidColorBrush(Color.Parse("#5aa055"));

    private const double DividerX = 302d;
    private const double SelfOriginX = 138d;
    private const double SelfPrimaryY = 78d;
    private const double SelfSecondaryY = 112d;
    private const double SegmentOriginX = 124d;
    private const double SegmentOriginY = 244d;
    private const double AntiComplexOriginX = 516d;
    private const double AreaOriginX = 734d;
    private const double AntiComplexOriginY = 96d;
    private const double AreaOriginY = 96d;
    private const double AntiAreaOriginX = 516d;
    private const double ComplexOriginX = 734d;
    private const double AntiAreaOriginY = 244d;
    private const double ComplexOriginY = 244d;
    private const double CrossLeft = 520d;
    private const double CrossRight = 730d;
    private const double CrossCenterX = 625d;
    private const double SelfGraphLeft = 202d;
    private const double SelfGraphTop = 28d;
    private const double SelfGraphWidth = 84d;
    private const double SelfGraphHeight = 82d;
    private const double HandleRadius = 6d;

    public static LockedInvariantLayout Arrange(Rect bounds, LockedInvariantDemoState state)
    {
        var available = new Rect(
            bounds.X + 8d,
            bounds.Y + 8d,
            Math.Max(320d, bounds.Width - 16d),
            Math.Max(160d, bounds.Height - 16d));
        var scale = Math.Min(available.Width / LockedInvariantLayout.SceneWidth, available.Height / LockedInvariantLayout.SceneHeight);
        var width = LockedInvariantLayout.SceneWidth * scale;
        var height = LockedInvariantLayout.SceneHeight * scale;
        var sceneRect = new Rect(
            available.X + ((available.Width - width) * 0.5d),
            available.Y + ((available.Height - height) * 0.5d),
            width,
            height);
        return new LockedInvariantLayout(sceneRect, scale, state);
    }

    public static void Render(DrawingContext context, Rect bounds, LockedInvariantDemoState state)
    {
        Render(context, Arrange(bounds, state));
    }

    public static void Render(DrawingContext context, LockedInvariantLayout layout)
    {
        DrawBackground(context, layout);
        DrawSelf(context, layout);
        DrawSegment(context, layout);
        DrawAntiComplex(context, layout);
        DrawArea(context, layout);
        DrawAntiArea(context, layout);
        DrawComplex(context, layout);
        DrawLabels(context, layout);
    }

    public static Point SelfPrimaryHandle(LockedInvariantLayout layout) => layout.Map(SelfPrimaryX(layout.State), SelfPrimaryY);

    public static Point SelfSecondaryHandle(LockedInvariantLayout layout) => layout.Map(SelfSecondaryX(layout.State), SelfSecondaryY);

    public static Point SegmentRecessiveHandle(LockedInvariantLayout layout) => layout.Map(SegmentLeftX(layout.State), SegmentOriginY);

    public static Point SegmentDominantHandle(LockedInvariantLayout layout) => layout.Map(SegmentRightX(layout.State), SegmentOriginY);

    public static Point AreaHorizontalHandle(LockedInvariantLayout layout) => layout.Map(AreaRightX(layout.State), AreaOriginY);

    public static Point AreaVerticalHandle(LockedInvariantLayout layout) => layout.Map(AreaOriginX, AreaTopY(layout.State));

    public static Point AntiAreaHorizontalHandle(LockedInvariantLayout layout) => layout.Map(AntiAreaLeftX(layout.State), AntiAreaOriginY);

    public static Point AntiAreaVerticalHandle(LockedInvariantLayout layout) => layout.Map(AntiAreaOriginX, AntiAreaBottomY(layout.State));

    public static Point ComplexHorizontalHandle(LockedInvariantLayout layout) => layout.Map(ComplexRightX(layout.State), ComplexOriginY);

    public static Point ComplexVerticalHandle(LockedInvariantLayout layout) => layout.Map(ComplexOriginX, ComplexBottomY(layout.State));

    public static Point AntiComplexHorizontalHandle(LockedInvariantLayout layout) => layout.Map(AntiComplexLeftX(layout.State), AntiComplexOriginY);

    public static Point AntiComplexVerticalHandle(LockedInvariantLayout layout) => layout.Map(AntiComplexOriginX, AntiComplexTopY(layout.State));

    private static void DrawBackground(DrawingContext context, LockedInvariantLayout layout)
    {
        DrawLine(context, layout, DividerX, 20d, DividerX, 312d, new Pen(new SolidColorBrush(Color.Parse("#ddd6ca")), 1.2d * layout.Scale, lineCap: PenLineCap.Round));
        DrawLine(context, layout, CrossLeft, 170d, CrossRight, 170d, new Pen(new SolidColorBrush(Color.Parse("#e4dfd4")), 1.1d * layout.Scale, lineCap: PenLineCap.Round));
        DrawLine(context, layout, CrossCenterX, 52d, CrossCenterX, 286d, new Pen(new SolidColorBrush(Color.Parse("#e4dfd4")), 1.1d * layout.Scale, lineCap: PenLineCap.Round));
    }

    private static void DrawSelf(DrawingContext context, LockedInvariantLayout layout)
    {
        DrawHorizontalTicks(context, layout, SelfOriginX, SelfPrimaryY, 1, 4);
        DrawHorizontalTicks(context, layout, SelfOriginX, SelfSecondaryY, 1, 4);
        DrawLine(context, layout, SelfOriginX - 16d, SelfPrimaryY, SelfOriginX + 152d, SelfPrimaryY, AxisPen(layout));
        DrawLine(context, layout, SelfOriginX - 16d, SelfSecondaryY, SelfOriginX + 152d, SelfSecondaryY, AxisPen(layout));
        DrawHorizontalHandleLine(context, layout, SelfOriginX, SelfPrimaryX(layout.State), SelfPrimaryY, ProbePalette.Dominant);
        DrawHorizontalHandleLine(context, layout, SelfOriginX, SelfSecondaryX(layout.State), SelfSecondaryY, ProbePalette.Origin, 3.2d);
        DrawHandle(context, layout, SelfPrimaryHandle(layout), ProbePalette.Dominant);
        DrawHandle(context, layout, SelfSecondaryHandle(layout), ProbePalette.Origin);
        DrawValueLabel(context, layout, SelfPrimaryHandle(layout), $"{layout.State.SelfPrimaryUnits:0.0}", new Vector(0d, -18d));
        DrawValueLabel(context, layout, SelfSecondaryHandle(layout), $"{layout.State.SelfSecondaryUnits:0.0}", new Vector(0d, 8d));

        DrawSelfGraph(context, layout);
    }

    private static void DrawSegment(DrawingContext context, LockedInvariantLayout layout)
    {
        DrawHorizontalTicks(context, layout, SegmentOriginX, SegmentOriginY, -1, 4);
        DrawHorizontalTicks(context, layout, SegmentOriginX, SegmentOriginY, 1, 4);
        DrawLine(context, layout, SegmentOriginX - 146d, SegmentOriginY, SegmentOriginX + 146d, SegmentOriginY, AxisPen(layout));
        DrawLine(context, layout, SegmentLeftX(layout.State), SegmentOriginY, SegmentOriginX, SegmentOriginY, SoftDominantPen(layout));
        DrawLine(context, layout, SegmentOriginX, SegmentOriginY, SegmentRightX(layout.State), SegmentOriginY, DominantPen(layout));
        DrawOverlayArrow(
            context,
            layout,
            new Point(SegmentLeftX(layout.State), SegmentOriginY - 20d),
            new Point(SegmentRightX(layout.State), SegmentOriginY - 20d),
            ProbePalette.Overlay);
        DrawHandle(context, layout, SegmentRecessiveHandle(layout), ProbePalette.DominantSoft);
        DrawHandle(context, layout, SegmentDominantHandle(layout), ProbePalette.Dominant);
        DrawValueLabel(context, layout, SegmentRecessiveHandle(layout), $"{layout.State.SegmentRecessiveUnits:0.0}", new Vector(0d, 8d));
        DrawValueLabel(context, layout, SegmentDominantHandle(layout), $"{layout.State.SegmentDominantUnits:0.0}", new Vector(0d, 8d));
    }

    private static void DrawAntiComplex(DrawingContext context, LockedInvariantLayout layout)
    {
        DrawHorizontalTicks(context, layout, AntiComplexOriginX, AntiComplexOriginY, -1, 4);
        DrawVerticalTicks(context, layout, AntiComplexOriginX, AntiComplexOriginY, -1, 4);
        DrawLine(context, layout, AntiComplexOriginX - 144d, AntiComplexOriginY, AntiComplexOriginX + 18d, AntiComplexOriginY, AxisPen(layout));
        DrawLine(context, layout, AntiComplexOriginX, AntiComplexOriginY - 144d, AntiComplexOriginX, AntiComplexOriginY + 18d, AxisPen(layout));
        DrawTraceDots(context, SampleNormTrace(layout, AntiComplexOriginX, AntiComplexOriginY, left: true, up: true, layout.State.AntiComplexRadiusUnits), layout.Scale, TraceBrush);
        DrawLine(context, layout, AntiComplexOriginX, AntiComplexOriginY, AntiComplexLeftX(layout.State), AntiComplexOriginY, SoftDominantPen(layout));
        DrawLine(context, layout, AntiComplexOriginX, AntiComplexOriginY, AntiComplexOriginX, AntiComplexTopY(layout.State), OrthogonalPen(layout));
        DrawLine(context, layout, AntiComplexOriginX, AntiComplexOriginY, AntiComplexLeftX(layout.State), AntiComplexTopY(layout.State), RayPen(layout));
        DrawHandle(context, layout, AntiComplexHorizontalHandle(layout), ProbePalette.DominantSoft);
        DrawHandle(context, layout, AntiComplexVerticalHandle(layout), ProbePalette.Orthogonal);
        DrawEndpoint(context, layout.Map(AntiComplexLeftX(layout.State), AntiComplexTopY(layout.State)), RayBrush);
    }

    private static void DrawArea(DrawingContext context, LockedInvariantLayout layout)
    {
        DrawHorizontalTicks(context, layout, AreaOriginX, AreaOriginY, 1, 4);
        DrawVerticalTicks(context, layout, AreaOriginX, AreaOriginY, -1, 4);
        DrawLine(context, layout, AreaOriginX - 18d, AreaOriginY, AreaOriginX + 144d, AreaOriginY, AxisPen(layout));
        DrawLine(context, layout, AreaOriginX, AreaOriginY - 144d, AreaOriginX, AreaOriginY + 18d, AxisPen(layout));
        DrawTraceDots(context, SampleProductTrace(layout, AreaOriginX, AreaOriginY, right: true, up: true, layout.State.AreaInvariantUnits, 2.25d, 4d), layout.Scale, TraceBrush);

        var rect = new Rect(
            layout.MapX(AreaOriginX),
            layout.MapY(AreaTopY(layout.State)),
            (AreaRightX(layout.State) - AreaOriginX) * layout.Scale,
            (AreaOriginY - AreaTopY(layout.State)) * layout.Scale);
        context.FillRectangle(AreaFill, rect);
        context.DrawRectangle(null, new Pen(new SolidColorBrush(Color.Parse("#d9ccab")), 1.1d * layout.Scale, lineCap: PenLineCap.Round), rect);
        DrawLine(context, layout, AreaOriginX, AreaOriginY, AreaRightX(layout.State), AreaOriginY, DominantPen(layout));
        DrawLine(context, layout, AreaOriginX, AreaOriginY, AreaOriginX, AreaTopY(layout.State), OrthogonalPen(layout));
        DrawHandle(context, layout, AreaHorizontalHandle(layout), ProbePalette.Dominant);
        DrawHandle(context, layout, AreaVerticalHandle(layout), ProbePalette.Orthogonal);
        DrawEndpoint(context, layout.Map(AreaRightX(layout.State), AreaTopY(layout.State)), ProbePalette.Overlay);
    }

    private static void DrawAntiArea(DrawingContext context, LockedInvariantLayout layout)
    {
        DrawHorizontalTicks(context, layout, AntiAreaOriginX, AntiAreaOriginY, -1, 4);
        DrawVerticalTicks(context, layout, AntiAreaOriginX, AntiAreaOriginY, 1, 4);
        DrawLine(context, layout, AntiAreaOriginX - 144d, AntiAreaOriginY, AntiAreaOriginX + 18d, AntiAreaOriginY, AxisPen(layout));
        DrawLine(context, layout, AntiAreaOriginX, AntiAreaOriginY - 18d, AntiAreaOriginX, AntiAreaOriginY + 144d, AxisPen(layout));
        DrawTraceDots(context, SampleProductTrace(layout, AntiAreaOriginX, AntiAreaOriginY, right: false, up: false, layout.State.AntiAreaInvariantUnits, 1d, 4d), layout.Scale, TraceBrush);

        var rect = new Rect(
            layout.MapX(AntiAreaLeftX(layout.State)),
            layout.MapY(AntiAreaOriginY),
            (AntiAreaOriginX - AntiAreaLeftX(layout.State)) * layout.Scale,
            (AntiAreaBottomY(layout.State) - AntiAreaOriginY) * layout.Scale);
        context.FillRectangle(AntiAreaFill, rect);
        context.DrawRectangle(null, new Pen(new SolidColorBrush(Color.Parse("#b8d6b4")), 1.1d * layout.Scale, lineCap: PenLineCap.Round), rect);
        DrawLine(context, layout, AntiAreaOriginX, AntiAreaOriginY, AntiAreaLeftX(layout.State), AntiAreaOriginY, SoftDominantPen(layout));
        DrawLine(context, layout, AntiAreaOriginX, AntiAreaOriginY, AntiAreaOriginX, AntiAreaBottomY(layout.State), OrthogonalInputPen(layout));
        DrawHandle(context, layout, AntiAreaHorizontalHandle(layout), ProbePalette.DominantSoft);
        DrawHandle(context, layout, AntiAreaVerticalHandle(layout), ProbePalette.OrthogonalInput);
        DrawEndpoint(context, layout.Map(AntiAreaLeftX(layout.State), AntiAreaBottomY(layout.State)), ProbePalette.Overlay);
    }

    private static void DrawComplex(DrawingContext context, LockedInvariantLayout layout)
    {
        DrawHorizontalTicks(context, layout, ComplexOriginX, ComplexOriginY, 1, 4);
        DrawVerticalTicks(context, layout, ComplexOriginX, ComplexOriginY, 1, 4);
        DrawLine(context, layout, ComplexOriginX - 18d, ComplexOriginY, ComplexOriginX + 144d, ComplexOriginY, AxisPen(layout));
        DrawLine(context, layout, ComplexOriginX, ComplexOriginY - 18d, ComplexOriginX, ComplexOriginY + 144d, AxisPen(layout));
        DrawTraceDots(context, SampleNormTrace(layout, ComplexOriginX, ComplexOriginY, left: false, up: false, layout.State.ComplexRadiusUnits), layout.Scale, TraceBrush);
        DrawLine(context, layout, ComplexOriginX, ComplexOriginY, ComplexRightX(layout.State), ComplexOriginY, DominantPen(layout));
        DrawLine(context, layout, ComplexOriginX, ComplexOriginY, ComplexOriginX, ComplexBottomY(layout.State), OrthogonalInputPen(layout));
        DrawLine(context, layout, ComplexOriginX, ComplexOriginY, ComplexRightX(layout.State), ComplexBottomY(layout.State), RayPen(layout));
        DrawHandle(context, layout, ComplexHorizontalHandle(layout), ProbePalette.Dominant);
        DrawHandle(context, layout, ComplexVerticalHandle(layout), ProbePalette.OrthogonalInput);
        DrawEndpoint(context, layout.Map(ComplexRightX(layout.State), ComplexBottomY(layout.State)), RayBrush);
    }

    private static void DrawSelfGraph(DrawingContext context, LockedInvariantLayout layout)
    {
        var rect = new Rect(
            layout.MapX(SelfGraphLeft),
            layout.MapY(SelfGraphTop),
            SelfGraphWidth * layout.Scale,
            SelfGraphHeight * layout.Scale);
        context.DrawRectangle(InsetSurface, new Pen(InsetStroke, 1d * layout.Scale), rect, 10d * layout.Scale, 10d * layout.Scale);

        var axisLeft = SelfGraphLeft + 10d;
        var axisBottom = SelfGraphTop + SelfGraphHeight - 10d;
        var axisRight = SelfGraphLeft + SelfGraphWidth - 10d;
        var axisTop = SelfGraphTop + 10d;
        DrawLine(context, layout, axisLeft, axisBottom, axisRight, axisBottom, new Pen(ProbePalette.Axis, 1d * layout.Scale, lineCap: PenLineCap.Round));
        DrawLine(context, layout, axisLeft, axisBottom, axisLeft, axisTop, new Pen(ProbePalette.Axis, 1d * layout.Scale, lineCap: PenLineCap.Round));

        var samples = new List<Point>();
        for (var index = 0; index <= 32; index++)
        {
            var xUnits = 1.5d + ((4.0d - 1.5d) * (index / 32d));
            var yUnits = layout.State.SelfInvariantUnits / xUnits;
            samples.Add(MapSelfGraphPoint(layout, xUnits, yUnits));
        }

        DrawTraceDots(context, samples, layout.Scale, GraphTraceBrush, 1.55d);
        var current = MapSelfGraphPoint(layout, layout.State.SelfPrimaryUnits, layout.State.SelfSecondaryUnits);
        DrawEndpoint(context, current, ProbePalette.Origin, 4.2d * layout.Scale);
        ScenePrimitives.DrawBadgeLabel(
            context,
            $"{layout.State.SelfPrimaryUnits:0.0}, {layout.State.SelfSecondaryUnits:0.0}",
            current + new Vector(8d * layout.Scale, -18d * layout.Scale),
            9.5d * layout.Scale,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.MonoFont);
    }

    private static Point MapSelfGraphPoint(LockedInvariantLayout layout, double xUnits, double yUnits)
    {
        const double min = 1.5d;
        const double max = 4.0d;
        var x = SelfGraphLeft + 10d + (((xUnits - min) / (max - min)) * (SelfGraphWidth - 20d));
        var y = SelfGraphTop + SelfGraphHeight - 10d - (((yUnits - min) / (max - min)) * (SelfGraphHeight - 20d));
        return layout.Map(x, y);
    }

    private static void DrawLabels(DrawingContext context, LockedInvariantLayout layout)
    {
        DrawPanelTitle(context, layout, "SELF-INTERACTION", "lock product", $"{layout.State.SelfInvariantUnits:0.0}", 124d, 144d);
        DrawPanelTitle(context, layout, "DIRECTED SEGMENT", "lock span", $"{layout.State.SegmentInvariantUnits:0.0}", 124d, 286d);
        DrawPanelTitle(context, layout, "ANTI-COMPLEX", "lock length", $"{layout.State.AntiComplexRadiusUnits:0.0}", CrossCenterX - 12d, 144d, rightAlign: true);
        DrawPanelTitle(context, layout, "AREA", "lock area", $"{layout.State.AreaInvariantUnits:0.0}", CrossCenterX + 12d, 144d);
        DrawPanelTitle(context, layout, "ANTI-AREA", "lock area", $"{layout.State.AntiAreaInvariantUnits:0.0}", CrossCenterX - 12d, 194d, rightAlign: true);
        DrawPanelTitle(context, layout, "COMPLEX", "lock length", $"{layout.State.ComplexRadiusUnits:0.0}", CrossCenterX + 12d, 194d);
        DrawMono(context, layout, "drag white handles to preserve the locked invariant", 460d, 322d, 11d, ProbePalette.Muted, centered: true);
    }

    private static void DrawPanelTitle(
        DrawingContext context,
        LockedInvariantLayout layout,
        string title,
        string lockLabel,
        string value,
        double x,
        double y,
        bool rightAlign = false)
    {
        DrawMono(context, layout, title, x, y, 11d, ProbePalette.Ink, FontWeight.ExtraBold, rightAlign);
        DrawMono(context, layout, $"{lockLabel} {value}", x, y + 14d, 9.5d, ProbePalette.Muted, FontWeight.SemiBold, rightAlign);
    }

    private static void DrawMono(
        DrawingContext context,
        LockedInvariantLayout layout,
        string text,
        double x,
        double y,
        double size,
        IBrush brush,
        FontWeight weight = FontWeight.Normal,
        bool rightAlign = false,
        bool centered = false)
    {
        var formatted = new FormattedText(
            text,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(ProbePalette.MonoFont, FontStyle.Normal, weight),
            size * layout.Scale,
            brush);
        var point = layout.Map(x, y);
        point = rightAlign
            ? new Point(point.X - formatted.Width, point.Y - (formatted.Height * 0.5d))
            : centered
                ? new Point(point.X - (formatted.Width * 0.5d), point.Y - (formatted.Height * 0.5d))
                : new Point(point.X, point.Y - (formatted.Height * 0.5d));
        context.DrawText(formatted, point);
    }

    private static void DrawValueLabel(DrawingContext context, LockedInvariantLayout layout, Point point, string label, Vector offset)
    {
        ScenePrimitives.DrawBadgeLabel(
            context,
            label,
            point + (offset * layout.Scale),
            9d * layout.Scale,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.MonoFont);
    }

    private static void DrawTraceDots(DrawingContext context, IEnumerable<Point> points, double scale, IBrush brush, double radius = 1.8d)
    {
        foreach (var point in points)
        {
            ScenePrimitives.DrawCircle(context, point, radius * scale, brush);
        }
    }

    private static void DrawEndpoint(DrawingContext context, Point point, IBrush brush, double radius = 4.8d)
    {
        ScenePrimitives.DrawCircle(context, point, radius, brush, new Pen(Brushes.White, 1.4d));
    }

    private static void DrawHandle(DrawingContext context, LockedInvariantLayout layout, Point center, IBrush brush)
    {
        ScenePrimitives.DrawCircle(
            context,
            center,
            HandleRadius * layout.Scale,
            Brushes.White,
            new Pen(brush, 2.4d * layout.Scale, lineCap: PenLineCap.Round));
    }

    private static IEnumerable<Point> SampleProductTrace(
        LockedInvariantLayout layout,
        double originX,
        double originY,
        bool right,
        bool up,
        double invariant,
        double minimum,
        double maximum)
    {
        for (var index = 0; index <= 28; index++)
        {
            var xUnits = minimum + ((maximum - minimum) * (index / 28d));
            var yUnits = invariant / xUnits;
            var x = originX + ((right ? 1d : -1d) * xUnits * LockedInvariantLayout.UnitPixels);
            var y = originY + ((up ? -1d : 1d) * yUnits * LockedInvariantLayout.UnitPixels);
            yield return layout.Map(x, y);
        }
    }

    private static IEnumerable<Point> SampleNormTrace(
        LockedInvariantLayout layout,
        double originX,
        double originY,
        bool left,
        bool up,
        double radius)
    {
        for (var index = 0; index <= 32; index++)
        {
            var horizontal = 0.5d + ((3.5d - 0.5d) * (index / 32d));
            var vertical = Math.Sqrt(Math.Max(0d, (radius * radius) - (horizontal * horizontal)));
            var x = originX + ((left ? -1d : 1d) * horizontal * LockedInvariantLayout.UnitPixels);
            var y = originY + ((up ? -1d : 1d) * vertical * LockedInvariantLayout.UnitPixels);
            yield return layout.Map(x, y);
        }
    }

    private static void DrawHorizontalTicks(DrawingContext context, LockedInvariantLayout layout, double originX, double originY, int direction, int units)
    {
        for (var index = 1; index <= units * 8; index++)
        {
            var x = originX + (index * LockedInvariantLayout.TickPixels * direction);
            var major = index % 8 == 0;
            var half = major ? 4d : 2.4d;
            DrawLine(context, layout, x, originY - half, x, originY + half, TickPen(layout, major));
        }
    }

    private static void DrawVerticalTicks(DrawingContext context, LockedInvariantLayout layout, double originX, double originY, int direction, int units)
    {
        for (var index = 1; index <= units * 8; index++)
        {
            var y = originY + (index * LockedInvariantLayout.TickPixels * direction);
            var major = index % 8 == 0;
            var half = major ? 4d : 2.4d;
            DrawLine(context, layout, originX - half, y, originX + half, y, TickPen(layout, major));
        }
    }

    private static void DrawOverlayArrow(DrawingContext context, LockedInvariantLayout layout, Point startScene, Point endScene, IBrush brush)
    {
        var start = layout.Map(startScene.X, startScene.Y);
        var end = layout.Map(endScene.X, endScene.Y);
        var directionRight = end.X >= start.X;
        var size = 17d * layout.Scale;
        var lineEnd = directionRight
            ? new Point(end.X - (size * 0.94d), end.Y)
            : new Point(end.X + (size * 0.94d), end.Y);
        context.DrawLine(new Pen(brush, 2.8d * layout.Scale, lineCap: PenLineCap.Round), start, lineEnd);
        ScenePrimitives.DrawTriangle(context, end, size, brush, null, pointsLeft: !directionRight);
    }

    private static void DrawHorizontalHandleLine(DrawingContext context, LockedInvariantLayout layout, double x1, double x2, double y, IBrush brush, double thickness = 3.4d)
    {
        var direction = x2 >= x1 ? 1d : -1d;
        var trim = 11d;
        var endX = x2 - (direction * trim);
        if ((direction > 0d && endX < x1) || (direction < 0d && endX > x1))
        {
            endX = x1;
        }

        DrawLine(context, layout, x1, y, endX, y, new Pen(brush, thickness * layout.Scale, lineCap: PenLineCap.Round));
    }

    private static void DrawLine(DrawingContext context, LockedInvariantLayout layout, double x1, double y1, double x2, double y2, Pen pen) =>
        context.DrawLine(pen, layout.Map(x1, y1), layout.Map(x2, y2));

    private static Pen AxisPen(LockedInvariantLayout layout) => new(ProbePalette.Axis, 1.2d * layout.Scale, lineCap: PenLineCap.Round);

    private static Pen TickPen(LockedInvariantLayout layout, bool major) => new(
        major ? ProbePalette.Axis : ProbePalette.Tick,
        1d * layout.Scale,
        lineCap: PenLineCap.Round);

    private static Pen DominantPen(LockedInvariantLayout layout) => new(ProbePalette.Dominant, 3.8d * layout.Scale, lineCap: PenLineCap.Round);

    private static Pen SoftDominantPen(LockedInvariantLayout layout) => new(ProbePalette.DominantSoft, 3.8d * layout.Scale, lineCap: PenLineCap.Round);

    private static Pen OrthogonalPen(LockedInvariantLayout layout) => new(ProbePalette.Orthogonal, 3.8d * layout.Scale, lineCap: PenLineCap.Round);

    private static Pen OrthogonalInputPen(LockedInvariantLayout layout) => new(ProbePalette.OrthogonalInput, 3.8d * layout.Scale, lineCap: PenLineCap.Round);

    private static Pen RayPen(LockedInvariantLayout layout) => new(RayBrush, 3d * layout.Scale, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);

    private static double SelfPrimaryX(LockedInvariantDemoState state) => SelfOriginX + (state.SelfPrimaryUnits * LockedInvariantLayout.UnitPixels);

    private static double SelfSecondaryX(LockedInvariantDemoState state) => SelfOriginX + (state.SelfSecondaryUnits * LockedInvariantLayout.UnitPixels);

    private static double SegmentLeftX(LockedInvariantDemoState state) => SegmentOriginX - (state.SegmentRecessiveUnits * LockedInvariantLayout.UnitPixels);

    private static double SegmentRightX(LockedInvariantDemoState state) => SegmentOriginX + (state.SegmentDominantUnits * LockedInvariantLayout.UnitPixels);

    private static double AreaRightX(LockedInvariantDemoState state) => AreaOriginX + (state.AreaHorizontalUnits * LockedInvariantLayout.UnitPixels);

    private static double AreaTopY(LockedInvariantDemoState state) => AreaOriginY - (state.AreaVerticalUnits * LockedInvariantLayout.UnitPixels);

    private static double AntiAreaLeftX(LockedInvariantDemoState state) => AntiAreaOriginX - (state.AntiAreaHorizontalUnits * LockedInvariantLayout.UnitPixels);

    private static double AntiAreaBottomY(LockedInvariantDemoState state) => AntiAreaOriginY + (state.AntiAreaVerticalUnits * LockedInvariantLayout.UnitPixels);

    private static double ComplexRightX(LockedInvariantDemoState state) => ComplexOriginX + (state.ComplexHorizontalUnits * LockedInvariantLayout.UnitPixels);

    private static double ComplexBottomY(LockedInvariantDemoState state) => ComplexOriginY + (state.ComplexVerticalUnits * LockedInvariantLayout.UnitPixels);

    private static double AntiComplexLeftX(LockedInvariantDemoState state) => AntiComplexOriginX - (state.AntiComplexHorizontalUnits * LockedInvariantLayout.UnitPixels);

    private static double AntiComplexTopY(LockedInvariantDemoState state) => AntiComplexOriginY - (state.AntiComplexVerticalUnits * LockedInvariantLayout.UnitPixels);
}
