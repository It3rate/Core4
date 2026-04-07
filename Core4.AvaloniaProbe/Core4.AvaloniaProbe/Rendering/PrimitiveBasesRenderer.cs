using System;
using Avalonia;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;

namespace Core4.AvaloniaProbe.Rendering;

/// <summary>
/// Renderer for the six primitive basis interpretations driven by two aligned endpoint values.
/// </summary>
public static class PrimitiveBasesRenderer
{
    private static readonly IBrush AreaFill = new SolidColorBrush(Color.Parse("#ecebe8"));
    private static readonly IBrush AntiAreaFill = new SolidColorBrush(Color.Parse("#e3ece1"));
    private static readonly IBrush PrimitiveRay = new SolidColorBrush(Color.Parse("#5aa055"));
    private static readonly IBrush LabelTick = new SolidColorBrush(Color.Parse("#8f97a8"));

    private const double DividerX = 302d;
    private const double SelfOriginX = 140d;
    private const double SelfOriginY = 96d;
    private const double SegmentOriginX = 124d;
    private const double SegmentOriginY = 244d;
    private const double AntiComplexOriginX = 560d;
    private const double AreaOriginX = 770d;
    private const double AntiComplexOriginY = 96d;
    private const double AreaOriginY = 96d;
    private const double AntiAreaOriginX = 560d;
    private const double ComplexOriginX = 770d;
    private const double AntiAreaOriginY = 244d;
    private const double ComplexOriginY = 244d;
    private const double CrossLeft = 560d;
    private const double CrossRight = 770d;
    private const double CrossCenterX = 665d;

    public static PrimitiveBasesLayout Arrange(Rect bounds, PrimitiveBasesDemoState state)
    {
        var available = new Rect(
            bounds.X + 8d,
            bounds.Y + 8d,
            Math.Max(320d, bounds.Width - 16d),
            Math.Max(160d, bounds.Height - 16d));
        var scale = Math.Min(available.Width / PrimitiveBasesLayout.SceneWidth, available.Height / PrimitiveBasesLayout.SceneHeight);
        var width = PrimitiveBasesLayout.SceneWidth * scale;
        var height = PrimitiveBasesLayout.SceneHeight * scale;
        var sceneRect = new Rect(
            available.X + ((available.Width - width) * 0.5d),
            available.Y + ((available.Height - height) * 0.5d),
            width,
            height);
        return new PrimitiveBasesLayout(sceneRect, scale, state);
    }

    public static void Render(DrawingContext context, Rect bounds, PrimitiveBasesDemoState state)
    {
        var layout = Arrange(bounds, state);
        Render(context, layout);
    }

    public static void Render(DrawingContext context, PrimitiveBasesLayout layout)
    {
        DrawBackground(context, layout);
        DrawSelfInteraction(context, layout);
        DrawDirectedSegment(context, layout);
        DrawAntiComplex(context, layout);
        DrawArea(context, layout);
        DrawAntiArea(context, layout);
        DrawComplex(context, layout);
        DrawLabels(context, layout);
    }

    public static Point DominantHandle(PrimitiveBasesLayout layout) => layout.Map(SegmentDominantX(layout.State), SegmentOriginY);

    public static Point RecessiveHandle(PrimitiveBasesLayout layout) => layout.Map(SegmentRecessiveX(layout.State), SegmentOriginY);

    private static void DrawBackground(DrawingContext context, PrimitiveBasesLayout layout)
    {
        var rect = layout.SceneRect;
        context.FillRectangle(ProbePalette.Surface, rect);
        context.DrawRectangle(null, new Pen(new SolidColorBrush(Color.Parse("#ddd6ca")), 1), rect, 20 * layout.Scale, 20 * layout.Scale);
        DrawLine(context, layout, DividerX, 94d, DividerX, 310d, new Pen(new SolidColorBrush(Color.Parse("#ddd6ca")), 1.2d * layout.Scale, lineCap: PenLineCap.Round));
        DrawLine(context, layout, CrossLeft, 170d, CrossRight, 170d, new Pen(new SolidColorBrush(Color.Parse("#e4dfd4")), 1.1d * layout.Scale, lineCap: PenLineCap.Round));
        DrawLine(context, layout, CrossCenterX, 70d, CrossCenterX, 270d, new Pen(new SolidColorBrush(Color.Parse("#e4dfd4")), 1.1d * layout.Scale, lineCap: PenLineCap.Round));
    }

    private static void DrawSelfInteraction(DrawingContext context, PrimitiveBasesLayout layout)
    {
        DrawHorizontalTicks(context, layout, SelfOriginX, SelfOriginY, 1, 3);
        DrawLine(context, layout, SelfOriginX - 18d, SelfOriginY, SelfOriginX + 126d, SelfOriginY, AxisPen(layout));
        DrawLine(context, layout, SelfOriginX, SelfOriginY - 44d, SelfOriginX, SelfOriginY + 22d, AxisPen(layout));

        DrawLine(context, layout, SelfOriginX, SelfOriginY - 8d, SelfDominantInputX(layout.State), SelfOriginY - 8d, DominantLinePen(layout, layout.State.DominantIsTension));
        DrawLine(context, layout, SelfOriginX, SelfOriginY + 8d, SelfReusedInputX(layout.State), SelfOriginY + 8d, DominantLinePen(layout, layout.State.RecessiveIsTension));
        DrawOverlayArrow(context, layout, new Point(SelfOriginX, SelfOriginY - 18d), new Point(SelfResultX(layout.State), SelfOriginY - 18d), layout.State.SelfIsTension);
        DrawTriangle(context, layout, SelfDominantInputX(layout.State), SelfOriginY - 8d, ProbePalette.Dominant, layout.State.DominantIsTension, false);
        DrawTriangle(context, layout, SelfReusedInputX(layout.State), SelfOriginY + 8d, ProbePalette.Dominant, layout.State.RecessiveIsTension, false);
    }

    private static void DrawDirectedSegment(DrawingContext context, PrimitiveBasesLayout layout)
    {
        DrawHorizontalTicks(context, layout, SegmentOriginX, SegmentOriginY, -1, 3);
        DrawHorizontalTicks(context, layout, SegmentOriginX, SegmentOriginY, 1, 3);
        DrawLine(context, layout, SegmentOriginX - 104d, SegmentOriginY, SegmentOriginX + 104d, SegmentOriginY, AxisPen(layout));
        DrawLine(context, layout, SegmentOriginX, SegmentOriginY - 54d, SegmentOriginX, SegmentOriginY + 24d, AxisPen(layout));

        DrawLine(context, layout, SegmentOriginX, SegmentOriginY, SegmentRecessiveX(layout.State), SegmentOriginY, RecessiveLinePen(layout, layout.State.RecessiveIsTension));
        DrawLine(context, layout, SegmentOriginX, SegmentOriginY, SegmentDominantX(layout.State), SegmentOriginY, DominantLinePen(layout, layout.State.DominantIsTension));
        DrawOverlayArrow(context, layout, new Point(SegmentRecessiveX(layout.State), SegmentOriginY - 22d), new Point(SegmentDominantX(layout.State), SegmentOriginY - 22d), false);
        DrawCircle(context, layout, SegmentRecessiveX(layout.State), SegmentOriginY, ProbePalette.DominantSoft, layout.State.RecessiveIsTension);
        DrawTriangle(context, layout, SegmentDominantX(layout.State), SegmentOriginY, ProbePalette.Dominant, layout.State.DominantIsTension, false);
    }

    private static void DrawAntiComplex(DrawingContext context, PrimitiveBasesLayout layout)
    {
        DrawHorizontalTicks(context, layout, AntiComplexOriginX, AntiComplexOriginY, -1, 3);
        DrawVerticalTicks(context, layout, AntiComplexOriginX, AntiComplexOriginY, -1, 3);
        DrawLine(context, layout, AntiComplexOriginX - 96d, AntiComplexOriginY, AntiComplexOriginX + 20d, AntiComplexOriginY, AxisPen(layout));
        DrawLine(context, layout, AntiComplexOriginX, AntiComplexOriginY - 96d, AntiComplexOriginX, AntiComplexOriginY + 18d, AxisPen(layout));
        DrawLine(context, layout, AntiComplexOriginX, AntiComplexOriginY, AntiComplexHorizontalX(layout.State), AntiComplexOriginY, DominantQuadrantPen(layout, layout.State.DominantIsTension));
        DrawLine(context, layout, AntiComplexOriginX, AntiComplexOriginY, AntiComplexOriginX, AntiComplexTopY(layout.State), VerticalTopQuadrantPen(layout, layout.State.RecessiveIsTension));
        DrawLine(context, layout, AntiComplexOriginX, AntiComplexOriginY, AntiComplexHorizontalX(layout.State), AntiComplexTopY(layout.State), RayPen(layout, layout.State.AntiComplexIsTension));
    }

    private static void DrawArea(DrawingContext context, PrimitiveBasesLayout layout)
    {
        DrawHorizontalTicks(context, layout, AreaOriginX, AreaOriginY, 1, 3);
        DrawVerticalTicks(context, layout, AreaOriginX, AreaOriginY, -1, 3);
        DrawLine(context, layout, AreaOriginX - 20d, AreaOriginY, AreaOriginX + 96d, AreaOriginY, AxisPen(layout));
        DrawLine(context, layout, AreaOriginX, AreaOriginY - 96d, AreaOriginX, AreaOriginY + 18d, AxisPen(layout));

        var left = Math.Min(AreaOriginX, AreaRightX(layout.State));
        var top = Math.Min(AreaOriginY, AreaTopY(layout.State));
        var width = Math.Abs(AreaRightX(layout.State) - AreaOriginX);
        var height = Math.Abs(AreaTopY(layout.State) - AreaOriginY);
        if (!layout.State.AreaIsTension && width > 0 && height > 0)
        {
            context.FillRectangle(AreaFill, new Rect(layout.MapX(left), layout.MapY(top), width * layout.Scale, height * layout.Scale));
        }

        DrawLine(context, layout, AreaOriginX, AreaOriginY, AreaRightX(layout.State), AreaOriginY, DominantQuadrantPen(layout, layout.State.DominantIsTension));
        DrawLine(context, layout, AreaOriginX, AreaOriginY, AreaOriginX, AreaTopY(layout.State), VerticalTopQuadrantPen(layout, layout.State.RecessiveIsTension));
    }

    private static void DrawAntiArea(DrawingContext context, PrimitiveBasesLayout layout)
    {
        DrawHorizontalTicks(context, layout, AntiAreaOriginX, AntiAreaOriginY, -1, 3);
        DrawVerticalTicks(context, layout, AntiAreaOriginX, AntiAreaOriginY, 1, 3);
        DrawLine(context, layout, AntiAreaOriginX - 96d, AntiAreaOriginY, AntiAreaOriginX + 20d, AntiAreaOriginY, AxisPen(layout));
        DrawLine(context, layout, AntiAreaOriginX, AntiAreaOriginY - 18d, AntiAreaOriginX, AntiAreaOriginY + 96d, AxisPen(layout));

        var left = Math.Min(AntiAreaOriginX, AntiAreaLeftX(layout.State));
        var top = Math.Min(AntiAreaOriginY, AntiAreaBottomY(layout.State));
        var width = Math.Abs(AntiAreaLeftX(layout.State) - AntiAreaOriginX);
        var height = Math.Abs(AntiAreaBottomY(layout.State) - AntiAreaOriginY);
        if (!layout.State.AntiAreaIsTension && width > 0 && height > 0)
        {
            context.FillRectangle(AntiAreaFill, new Rect(layout.MapX(left), layout.MapY(top), width * layout.Scale, height * layout.Scale));
        }

        DrawLine(context, layout, AntiAreaOriginX, AntiAreaOriginY, AntiAreaLeftX(layout.State), AntiAreaOriginY, DominantQuadrantPen(layout, layout.State.DominantIsTension));
        DrawLine(context, layout, AntiAreaOriginX, AntiAreaOriginY, AntiAreaOriginX, AntiAreaBottomY(layout.State), VerticalBottomQuadrantPen(layout, layout.State.RecessiveIsTension));
    }

    private static void DrawComplex(DrawingContext context, PrimitiveBasesLayout layout)
    {
        DrawHorizontalTicks(context, layout, ComplexOriginX, ComplexOriginY, 1, 3);
        DrawVerticalTicks(context, layout, ComplexOriginX, ComplexOriginY, 1, 3);
        DrawLine(context, layout, ComplexOriginX - 20d, ComplexOriginY, ComplexOriginX + 96d, ComplexOriginY, AxisPen(layout));
        DrawLine(context, layout, ComplexOriginX, ComplexOriginY - 18d, ComplexOriginX, ComplexOriginY + 96d, AxisPen(layout));
        DrawLine(context, layout, ComplexOriginX, ComplexOriginY, ComplexRightX(layout.State), ComplexOriginY, DominantQuadrantPen(layout, layout.State.DominantIsTension));
        DrawLine(context, layout, ComplexOriginX, ComplexOriginY, ComplexOriginX, ComplexBottomY(layout.State), VerticalBottomQuadrantPen(layout, layout.State.RecessiveIsTension));
        DrawLine(context, layout, ComplexOriginX, ComplexOriginY, ComplexRightX(layout.State), ComplexBottomY(layout.State), RayPen(layout, layout.State.ComplexIsTension));
    }

    private static void DrawLabels(DrawingContext context, PrimitiveBasesLayout layout)
    {
        DrawReading(context, layout, layout.State.ComplexReading, layout.State.ApproximateReading);
        DrawLabel(context, layout, "SELF-INTERACTION", 124d, 160d, 11d, FontWeight.ExtraBold, ProbePalette.Ink, centered: true);
        DrawLabel(context, layout, "DIRECTED SEGMENT", 124d, 286d, 11d, FontWeight.ExtraBold, ProbePalette.Ink, centered: true);
        DrawLabel(context, layout, "ANTI-COMPLEX", CrossCenterX - 12d, 146d, 11d, FontWeight.ExtraBold, ProbePalette.Ink, rightAlign: true);
        DrawLabel(context, layout, "AREA", CrossCenterX + 12d, 146d, 11d, FontWeight.ExtraBold, ProbePalette.Ink);
        DrawLabel(context, layout, "ANTI-AREA", CrossCenterX - 12d, 194d, 11d, FontWeight.ExtraBold, ProbePalette.Ink, rightAlign: true);
        DrawLabel(context, layout, "COMPLEX", CrossCenterX + 12d, 194d, 11d, FontWeight.ExtraBold, ProbePalette.Ink);
        DrawLabel(context, layout, "drag endpoints", SegmentOriginX, 316d, 12d, FontWeight.SemiBold, ProbePalette.Muted, centered: true);
    }

    private static void DrawReading(DrawingContext context, PrimitiveBasesLayout layout, string complex, string approximate)
    {
        var readingX = DividerX + 28d;
        var readingY = 24d;
        var approxY = 44d;
        var parts = complex.Split(" + ");
        var left = MeasureText(parts[0], 16d * layout.Scale, ProbePalette.DominantSoft, FontWeight.Bold);
        var right = MeasureText($" + {parts[1]}", 16d * layout.Scale, ProbePalette.Dominant, FontWeight.Bold);
        var totalWidth = left.Width + right.Width;
        var start = layout.Map(readingX, readingY);
        start = new Point(start.X - totalWidth, start.Y);
        context.DrawText(left, start);
        context.DrawText(right, new Point(start.X + left.Width, start.Y));

        var approx = MeasureText(approximate, 13d * layout.Scale, ProbePalette.Muted, FontWeight.SemiBold);
        var approxPoint = layout.Map(readingX, approxY);
        context.DrawText(approx, new Point(approxPoint.X - approx.Width, approxPoint.Y));
    }

    private static void DrawHorizontalTicks(DrawingContext context, PrimitiveBasesLayout layout, double originX, double originY, int direction, int units)
    {
        for (var index = 1; index <= units * 8; index++)
        {
            var x = originX + (index * PrimitiveBasesLayout.TickPixels * direction);
            var major = index % 8 == 0;
            var half = major ? 4d : 2.4d;
            DrawLine(context, layout, x, originY - half, x, originY + half, TickPen(layout, major));
            if (major)
            {
                var labelY = originY + (direction > 0 ? 14d : -10d);
                DrawLabel(context, layout, (index / 8).ToString(), x, labelY, 8d, FontWeight.Normal, LabelTick, centered: true);
            }
        }
    }

    private static void DrawVerticalTicks(DrawingContext context, PrimitiveBasesLayout layout, double originX, double originY, int direction, int units)
    {
        for (var index = 1; index <= units * 8; index++)
        {
            var y = originY + (index * PrimitiveBasesLayout.TickPixels * direction);
            var major = index % 8 == 0;
            var half = major ? 4d : 2.4d;
            DrawLine(context, layout, originX - half, y, originX + half, y, TickPen(layout, major));
            if (major)
            {
                DrawLabel(context, layout, (index / 8).ToString(), originX + 12d, y + 3d, 8d, FontWeight.Normal, LabelTick);
            }
        }
    }

    private static void DrawOverlayArrow(DrawingContext context, PrimitiveBasesLayout layout, Point startScene, Point endScene, bool tension)
    {
        var start = layout.Map(startScene.X, startScene.Y);
        var end = layout.Map(endScene.X, endScene.Y);
        var directionRight = end.X >= start.X;
        var size = 18d * layout.Scale;
        var lineEnd = directionRight
            ? new Point(end.X - (size * 0.94d), end.Y)
            : new Point(end.X + (size * 0.94d), end.Y);
        var pen = tension
            ? DashedPen(ProbePalette.Overlay, 3.2d * layout.Scale, layout.Scale)
            : new Pen(ProbePalette.Overlay, 3.2d * layout.Scale, lineCap: PenLineCap.Round);
        context.DrawLine(pen, start, lineEnd);
        ScenePrimitives.DrawTriangle(context, end, size, ProbePalette.Overlay, pointsLeft: !directionRight);
    }

    private static void DrawTriangle(DrawingContext context, PrimitiveBasesLayout layout, double x, double y, IBrush brush, bool tension, bool pointsLeft)
    {
        var pen = tension ? new Pen(brush, 1.2d * layout.Scale, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round) : null;
        ScenePrimitives.DrawTriangle(context, layout.Map(x, y), 12d * layout.Scale, brush, pen, pointsLeft);
    }

    private static void DrawCircle(DrawingContext context, PrimitiveBasesLayout layout, double x, double y, IBrush brush, bool tension)
    {
        var stroke = new Pen(brush, 3d * layout.Scale, lineCap: PenLineCap.Round);
        ScenePrimitives.DrawCircle(context, layout.Map(x, y), 5d * layout.Scale, ProbePalette.Surface, stroke);
    }

    private static void DrawLine(DrawingContext context, PrimitiveBasesLayout layout, double x1, double y1, double x2, double y2, Pen pen) =>
        context.DrawLine(pen, layout.Map(x1, y1), layout.Map(x2, y2));

    private static void DrawLabel(DrawingContext context, PrimitiveBasesLayout layout, string text, double x, double y, double size, FontWeight weight, IBrush brush, bool rightAlign = false, bool centered = false)
    {
        var formatted = MeasureText(text, size * layout.Scale, brush, weight);
        var point = layout.Map(x, y);
        point = rightAlign
            ? new Point(point.X - formatted.Width, point.Y - (formatted.Height * 0.5d))
            : centered
                ? new Point(point.X - (formatted.Width * 0.5d), point.Y - (formatted.Height * 0.5d))
                : new Point(point.X, point.Y - (formatted.Height * 0.5d));
        context.DrawText(formatted, point);
    }

    private static FormattedText MeasureText(string text, double size, IBrush brush, FontWeight weight) =>
        new(
            text,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(ProbePalette.MonoFont, FontStyle.Normal, weight),
            size,
            brush);

    private static Pen AxisPen(PrimitiveBasesLayout layout) => new(new SolidColorBrush(Color.Parse("#d4cfc4")), 1.2d * layout.Scale, lineCap: PenLineCap.Round);

    private static Pen TickPen(PrimitiveBasesLayout layout, bool major) => new(
        major ? new SolidColorBrush(Color.Parse("#c4cbd7")) : new SolidColorBrush(Color.Parse("#dcd8cf")),
        1d * layout.Scale,
        lineCap: PenLineCap.Round);

    private static Pen DominantLinePen(PrimitiveBasesLayout layout, bool tension) => tension ? DashedPen(ProbePalette.Dominant, 3.4d * layout.Scale, layout.Scale) : new Pen(ProbePalette.Dominant, 3.4d * layout.Scale, lineCap: PenLineCap.Round);
    private static Pen RecessiveLinePen(PrimitiveBasesLayout layout, bool tension) => tension ? DashedPen(ProbePalette.DominantSoft, 3.4d * layout.Scale, layout.Scale) : new Pen(ProbePalette.DominantSoft, 3.4d * layout.Scale, lineCap: PenLineCap.Round);
    private static Pen VerticalTopPen(PrimitiveBasesLayout layout, bool tension) => tension ? DashedPen(ProbePalette.Orthogonal, 3.4d * layout.Scale, layout.Scale) : new Pen(ProbePalette.Orthogonal, 3.4d * layout.Scale, lineCap: PenLineCap.Round);
    private static Pen VerticalBottomPen(PrimitiveBasesLayout layout, bool tension) => tension ? DashedPen(ProbePalette.OrthogonalInput, 3.4d * layout.Scale, layout.Scale) : new Pen(ProbePalette.OrthogonalInput, 3.4d * layout.Scale, lineCap: PenLineCap.Round);
    private static Pen DominantQuadrantPen(PrimitiveBasesLayout layout, bool tension) => tension ? DashedPen(ProbePalette.Dominant, 4.4d * layout.Scale, layout.Scale) : new Pen(ProbePalette.Dominant, 4.4d * layout.Scale, lineCap: PenLineCap.Round);
    private static Pen VerticalTopQuadrantPen(PrimitiveBasesLayout layout, bool tension) => tension ? DashedPen(ProbePalette.Orthogonal, 4.4d * layout.Scale, layout.Scale) : new Pen(ProbePalette.Orthogonal, 4.4d * layout.Scale, lineCap: PenLineCap.Round);
    private static Pen VerticalBottomQuadrantPen(PrimitiveBasesLayout layout, bool tension) => tension ? DashedPen(ProbePalette.OrthogonalInput, 4.4d * layout.Scale, layout.Scale) : new Pen(ProbePalette.OrthogonalInput, 4.4d * layout.Scale, lineCap: PenLineCap.Round);
    private static Pen RayPen(PrimitiveBasesLayout layout, bool tension) => tension ? DashedPen(PrimitiveRay, 3.2d * layout.Scale, layout.Scale) : new Pen(PrimitiveRay, 3.2d * layout.Scale, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);

    private static Pen DashedPen(IBrush brush, double thickness, double scale) => new(
        brush,
        thickness,
        dashStyle: new DashStyle(new[] { 2d * scale, 2d * scale }, 0d),
        lineCap: PenLineCap.Round,
        lineJoin: PenLineJoin.Round);

    private static double SlotOffsetX(double units, int normalDirection) => units >= 0d ? units * PrimitiveBasesLayout.UnitPixels * normalDirection : Math.Abs(units) * PrimitiveBasesLayout.UnitPixels * -normalDirection;
    private static double SlotOffsetY(double units, int normalDirection) => units >= 0d ? units * PrimitiveBasesLayout.UnitPixels * normalDirection : Math.Abs(units) * PrimitiveBasesLayout.UnitPixels * -normalDirection;
    private static double SelfDominantInputX(PrimitiveBasesDemoState state) => SelfOriginX + SlotOffsetX(state.DominantUnits, 1);
    private static double SelfReusedInputX(PrimitiveBasesDemoState state) => SelfOriginX + SlotOffsetX(state.RecessiveUnits, 1);
    private static double SelfResultX(PrimitiveBasesDemoState state) => SelfOriginX + (state.SelfUnits * PrimitiveBasesLayout.UnitPixels);
    private static double SegmentRecessiveX(PrimitiveBasesDemoState state) => SegmentOriginX + SlotOffsetX(state.RecessiveUnits, -1);
    private static double SegmentDominantX(PrimitiveBasesDemoState state) => SegmentOriginX + SlotOffsetX(state.DominantUnits, 1);
    private static double AreaRightX(PrimitiveBasesDemoState state) => AreaOriginX + SlotOffsetX(state.DominantUnits, 1);
    private static double AreaTopY(PrimitiveBasesDemoState state) => AreaOriginY + SlotOffsetY(state.RecessiveUnits, -1);
    private static double AntiAreaLeftX(PrimitiveBasesDemoState state) => AntiAreaOriginX + SlotOffsetX(state.DominantUnits, -1);
    private static double AntiAreaBottomY(PrimitiveBasesDemoState state) => AntiAreaOriginY + SlotOffsetY(state.RecessiveUnits, 1);
    private static double AntiComplexHorizontalX(PrimitiveBasesDemoState state) => AntiComplexOriginX + SlotOffsetX(state.DominantUnits, -1);
    private static double AntiComplexTopY(PrimitiveBasesDemoState state) => AntiComplexOriginY + SlotOffsetY(state.RecessiveUnits, -1);
    private static double ComplexRightX(PrimitiveBasesDemoState state) => ComplexOriginX + SlotOffsetX(state.DominantUnits, 1);
    private static double ComplexBottomY(PrimitiveBasesDemoState state) => ComplexOriginY + SlotOffsetY(state.RecessiveUnits, 1);
}
