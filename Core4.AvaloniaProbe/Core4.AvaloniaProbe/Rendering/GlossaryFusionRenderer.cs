using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;

namespace Core4.AvaloniaProbe.Rendering;

internal static class GlossaryFusionRenderer
{
    private static readonly Pen SourceBoxPen = new(new SolidColorBrush(Color.Parse("#d7d0c4")), 1d);
    private static readonly Pen StagePen = new(new SolidColorBrush(Color.FromArgb(112, 197, 182, 160)), 1d);
    private static readonly IBrush StageBrush = new SolidColorBrush(Color.FromArgb(48, 255, 255, 255));
    private static readonly IBrush SourceBrush = new SolidColorBrush(Color.FromArgb(82, 255, 255, 255));
    private static readonly Pen UnitPen = new(new SolidColorBrush(Color.Parse("#3254bf")), 2.8d, lineCap: PenLineCap.Round);
    private static readonly Pen AxisPen = new(new SolidColorBrush(Color.FromArgb(108, 120, 120, 120)), 1d, lineCap: PenLineCap.Round);
    private static readonly IBrush AreaFill = new SolidColorBrush(Color.FromArgb(84, 231, 227, 214));
    private static readonly IBrush AntiAreaFill = new SolidColorBrush(Color.FromArgb(84, 225, 236, 222));

    public static GlossaryFusionSceneLayout Arrange(Rect bounds, GlossaryFusionDemoState state)
    {
        var scene = new Rect(
            bounds.X + 18d,
            bounds.Y + 14d,
            Math.Max(0d, bounds.Width - 36d),
            Math.Max(0d, bounds.Height - 28d));

        var selectorWidth = 238d;
        var selectorX = scene.Right - selectorWidth;
        var main = new Rect(scene.X, scene.Y, Math.Max(0d, selectorX - scene.X - 18d), scene.Height);

        var selectorRows = new List<GlossaryFusionSourceLayout>(state.Sources.Length);
        var rowTop = scene.Y -10d;
        var rowStep = 88d;
        var buttonWidth = 56d;
        var boxWidth = selectorWidth - buttonWidth - 12d;
        foreach (var source in state.Sources)
        {
            var y = rowTop + ((source.Id - 1L) * rowStep);
            var buttonRect = new Rect(selectorX, y + 16d, buttonWidth, 28d);
            var boxRect = new Rect(selectorX + buttonWidth + 12d, y, boxWidth, 58d);
            selectorRows.Add(new GlossaryFusionSourceLayout(source.Id, buttonRect, boxRect));
        }

        var stageY = scene.Y - 20d;
        var stageLeft = new Point(main.X + 204d, stageY);
        var stageRight = new Point(main.X + 346d, stageY);
        var stageResult = new Point((stageLeft.X + stageRight.X) * 0.5d, stageY + 10d);

        const int columns = 5;
        var gap = 12d;
        var cellWidth = (main.Width - (gap * (columns - 1))) / columns;
        var cellHeight = 112d;
        var cellTop = scene.Y + 40d;
        var cellRows = ImmutableArray.CreateBuilder<GlossaryFusionResultCellLayout>(state.Results.Length);
        for (var index = 0; index < state.Results.Length; index++)
        {
            var result = state.Results[index];
            var column = index % columns;
            var row = index / columns;
            var rect = new Rect(
                main.X + (column * (cellWidth + gap)),
                cellTop + (row * (cellHeight + 18d)),
                cellWidth,
                cellHeight);
            cellRows.Add(new GlossaryFusionResultCellLayout(result.Key, rect));
        }

        return new GlossaryFusionSceneLayout(
            scene,
            main,
            selectorRows.ToImmutableArray(),
            stageLeft,
            stageRight,
            stageResult,
            cellRows.ToImmutable());
    }

    public static void Render(DrawingContext context, Rect bounds, GlossaryFusionDemoState state)
    {
        var layout = Arrange(bounds, state);
        DrawStage(context, layout, state);
        DrawResultGallery(context, layout, state);
        DrawSelectors(context, layout, state);
        DrawAnimation(context, layout, state);
    }

    private static void DrawStage(DrawingContext context, GlossaryFusionSceneLayout layout, GlossaryFusionDemoState state)
    {
        var stageRectA = new Rect(layout.StageLeft.X - 40d, layout.StageLeft.Y - 24d, 80d, 52d);
        var stageRectB = new Rect(layout.StageRight.X - 40d, layout.StageRight.Y - 24d, 80d, 52d);
        context.DrawRectangle(StageBrush, StagePen, stageRectA, 16d, 16d);
        context.DrawRectangle(StageBrush, StagePen, stageRectB, 16d, 16d);

        ScenePrimitives.DrawCenteredLabel(
            context,
            "staging",
            new Point((stageRectA.X + stageRectB.Right) * 0.5d, stageRectA.Y - 10d),
            9.6d,
            ProbePalette.Muted,
            FontWeight.SemiBold,
            ProbePalette.MonoFont);

        if (state.HasFirstSelection && state.TryFindSource(state.Selection.FirstSourceId, out var selected))
        {
            DrawSourceGlyph(context, layout.StageLeft, selected, showValue: false, compact: true);
        }
    }

    private static void DrawSelectors(DrawingContext context, GlossaryFusionSceneLayout layout, GlossaryFusionDemoState state)
    {
        foreach (var row in layout.Sources)
        {
            if (!state.TryFindSource(row.SourceId, out var source))
            {
                continue;
            }

            var selected = state.Selection.FirstSourceId == source.Id;
            var boxStroke = selected
                ? new Pen(new SolidColorBrush(Color.Parse("#87aaf3")), 1.2d)
                : SourceBoxPen;
            context.DrawRectangle(SourceBrush, boxStroke, row.BoxRect, 16d, 16d);

            var buttonBrush = selected
                ? new SolidColorBrush(Color.Parse("#eff4ff"))
                : ProbePalette.Surface;
            var buttonStroke = new Pen(new SolidColorBrush(selected ? Color.Parse("#87aaf3") : Color.FromArgb(96, 197, 182, 160)), 1d);
            context.DrawRectangle(buttonBrush, buttonStroke, row.ButtonRect, 14d, 14d);
            ScenePrimitives.DrawCenteredLabel(
                context,
                ButtonLabelFor(state, source.Id),
                new Point(row.ButtonRect.Center.X, row.ButtonRect.Center.Y - 7d),
                9.4d,
                ProbePalette.Ink,
                FontWeight.SemiBold,
                ProbePalette.MonoFont);

            DrawSourceGlyph(context, row.BoxRect.Center, source, showValue: true, compact: false);
            ScenePrimitives.DrawCenteredLabel(
                context,
                source.BucketLabel,
                new Point(row.BoxRect.Center.X, row.BoxRect.Bottom + 6d),
                9.4d,
                ProbePalette.Ink,
                FontWeight.SemiBold,
                ProbePalette.MonoFont);
        }
    }

    private static void DrawResultGallery(DrawingContext context, GlossaryFusionSceneLayout layout, GlossaryFusionDemoState state)
    {
        foreach (var cell in layout.ResultCells)
        {
            if (!state.TryFindResult(cell.ResultKey, out var result))
            {
                continue;
            }

            var fill = result.IsDiscovered
                ? new SolidColorBrush(Color.FromArgb(44, 255, 255, 255))
                : new SolidColorBrush(Color.FromArgb(24, 255, 255, 255));
            var stroke = result.IsDiscovered
                ? new Pen(new SolidColorBrush(Color.FromArgb(124, 197, 182, 160)), 1d)
                : new Pen(new SolidColorBrush(Color.FromArgb(72, 215, 207, 192)), 1d);
            context.DrawRectangle(fill, stroke, cell.Bounds, 18d, 18d);

            ScenePrimitives.DrawCenteredLabel(
                context,
                result.Title,
                new Point(cell.Bounds.Center.X, cell.Bounds.Bottom + 5d),
                9.4d,
                ProbePalette.Ink,
                FontWeight.SemiBold,
                ProbePalette.MonoFont);

            if (result.IsDiscovered)
            {
                DrawResultGlyph(context, cell.Bounds, result, 1d);
            }
            else
            {
                DrawResultGlyph(context, cell.Bounds, result, 0.18d);
            }
        }
    }

    private static void DrawAnimation(DrawingContext context, GlossaryFusionSceneLayout layout, GlossaryFusionDemoState state)
    {
        if (state.Animation.Mode == 1L && state.TryFindSource(state.Animation.FirstSourceId, out var firstSource))
        {
            var start = CenterForSource(layout, firstSource.Id);
            var end = layout.StageLeft;
            var staging = Smooth(state.Animation.Progress);
            DrawSourceGlyph(context, Lerp(start, end, staging), firstSource, showValue: false, compact: true);
            return;
        }

        if (state.Animation.Mode != 2L
            || !state.TryFindSource(state.Animation.FirstSourceId, out var sourceA)
            || !state.TryFindSource(state.Animation.SecondSourceId, out var sourceB)
            || !state.TryFindResult(state.Animation.ResultKey, out var result))
        {
            return;
        }

        DrawSourceGlyph(context, layout.StageLeft, sourceA, showValue: false, compact: true);

        var progress = state.Animation.Progress;
        if (progress < 0.42d)
        {
            var local = Smooth(progress / 0.42d);
            var moving = Lerp(CenterForSource(layout, sourceB.Id), layout.StageRight, local);
            DrawSourceGlyph(context, moving, sourceB, showValue: false, compact: true);
            return;
        }

        DrawSourceGlyph(context, layout.StageRight, sourceB, showValue: false, compact: true);

        if (progress < 0.70d)
        {
            var pulse = Math.Sin(((progress - 0.42d) / 0.28d) * Math.PI);
            var wavePen = new Pen(new SolidColorBrush(Color.FromArgb((byte)(84 + (pulse * 90d)), 156, 110, 214)), 1.5d);
            ScenePrimitives.DrawWave(context, layout.StageLeft, layout.StageRight, 10d + (pulse * 4d), wavePen, minimumCycles: 4);
            DrawResultGlyphAt(context, layout.StageResult, 38d + (pulse * 6d), result, 0.55d + (pulse * 0.25d));
            return;
        }

        var target = CenterForResult(layout, result.Key);
        var carry = Smooth((progress - 0.70d) / 0.30d);
        DrawResultGlyphAt(context, Lerp(layout.StageResult, target, carry), 40d, result, 1d);
    }

    private static void DrawSourceGlyph(
        DrawingContext context,
        Point center,
        GlossaryFusionSourceState source,
        bool showValue,
        bool compact)
    {
        var accent = SourceAccent(source);
        var accentBrush = new SolidColorBrush(accent);
        var unitRadius = compact ? 3.8d : 4.8d;
        var span = compact ? 20d : 26d;
        var valueOffset = compact ? 14d : 18d;

        if (source.IsOrthogonal)
        {
            var direction = source.IsDominant ? -1d : 1d;
            var origin = center + new Vector(0d, compact ? 12d : 14d);
            var stub = origin + new Vector(0d, direction * (compact ? 9d : 12d));
            var tip = origin + new Vector(0d, direction * span);
            context.DrawLine(UnitPen, origin, stub);
            context.DrawLine(new Pen(accentBrush, compact ? 2.5d : 2.8d, lineCap: PenLineCap.Round), stub, tip);
            ScenePrimitives.DrawVerticalTriangle(context, tip, compact ? 8d : 9d, accentBrush, pointsUp: source.IsDominant);
            ScenePrimitives.DrawCircle(context, origin, unitRadius, ProbePalette.Surface, new Pen(new SolidColorBrush(Color.Parse("#3254bf")), 1.8d));

            if (showValue)
            {
                DrawTextLabel(context, source.ValueLabel, new Point(center.X - 10d, center.Y - 8d), vertical: true);
            }
        }
        else
        {
            var direction = source.IsDominant ? 1d : -1d;
            var origin = center + new Vector(direction > 0d ? -16d : 16d, 4d);
            var stub = origin + new Vector(direction * (compact ? 10d : 12d), 0d);
            var tip = origin + new Vector(direction * span, 0d);
            context.DrawLine(UnitPen, origin, stub);
            context.DrawLine(new Pen(accentBrush, compact ? 2.5d : 2.8d, lineCap: PenLineCap.Round), stub, tip);
            ScenePrimitives.DrawDirectedTriangle(context, tip, compact ? 8d : 9d, accentBrush, new Vector(direction, 0d));
            ScenePrimitives.DrawCircle(context, origin, unitRadius, ProbePalette.Surface, new Pen(new SolidColorBrush(Color.Parse("#3254bf")), 1.8d));

            if (showValue)
            {
                var labelPoint = direction > 0d
                    ? new Point(origin.X + 2d, origin.Y - valueOffset)
                    : new Point(origin.X - 36d, origin.Y - valueOffset);
                DrawTextLabel(context, source.ValueLabel, labelPoint, vertical: false);
            }
        }
    }

    private static void DrawResultGlyph(DrawingContext context, Rect bounds, GlossaryFusionResultState result, double opacity)
    {
        DrawResultGlyphAt(context, bounds.Center, Math.Min(bounds.Width, bounds.Height) * 0.44d, result, opacity);
    }

    private static void DrawResultGlyphAt(
        DrawingContext context,
        Point center,
        double size,
        GlossaryFusionResultState result,
        double opacity)
    {
        var alpha = (byte)Math.Clamp((int)Math.Round(opacity * 255d), 0, 255);
        var blue = new SolidColorBrush(Color.FromArgb(alpha, 50, 84, 191));
        var green = new SolidColorBrush(Color.FromArgb(alpha, 54, 165, 101));
        var orange = new SolidColorBrush(Color.FromArgb(alpha, 219, 136, 44));
        var red = new SolidColorBrush(Color.FromArgb(alpha, 219, 77, 71));
        var purple = new SolidColorBrush(Color.FromArgb(alpha, 139, 95, 209));
        var lineBlue = new Pen(new SolidColorBrush(Color.FromArgb(alpha, 50, 84, 191)), 2.6d, lineCap: PenLineCap.Round);
        var faintAxis = new Pen(new SolidColorBrush(Color.FromArgb((byte)(alpha * 0.45d), 120, 120, 120)), 0.9d, lineCap: PenLineCap.Round);
        var areaFill = result.Kind == 7L
            ? new SolidColorBrush(Color.FromArgb((byte)(alpha * 0.28d), 205, 234, 201))
            : new SolidColorBrush(Color.FromArgb((byte)(alpha * 0.28d), 232, 227, 213));

        switch (result.Kind)
        {
            case 1L:
                DrawParallelAligned(context, center, size, 1d, purple, opacity);
                break;
            case 2L:
                DrawDirectedSegment(context, center, size, horizontal: true, upwardsOrPositive: true, blue);
                break;
            case 3L:
                DrawAreaLike(context, center, size, right: true, up: true, lineBlue, orange, areaFill, faintAxis, opacity);
                break;
            case 4L:
                DrawComplexLike(context, center, size, right: true, up: false, lineBlue, orange, green, faintAxis);
                break;
            case 5L:
                DrawParallelAligned(context, center, size, -1d, red, opacity);
                break;
            case 6L:
                DrawComplexLike(context, center, size, right: false, up: true, lineBlue, green, green, faintAxis);
                break;
            case 7L:
                DrawAreaLike(context, center, size, right: false, up: false, lineBlue, orange, areaFill, faintAxis, opacity);
                break;
            case 8L:
                DrawParallelOrthogonal(context, center, size, true, green, opacity);
                break;
            case 9L:
                DrawDirectedSegment(context, center, size, horizontal: false, upwardsOrPositive: true, blue);
                break;
            case 10L:
                DrawParallelOrthogonal(context, center, size, false, red, opacity);
                break;
        }
    }

    private static void DrawParallelAligned(DrawingContext context, Point center, double size, double direction, IBrush accent, double opacity)
    {
        var unitPen = new Pen(new SolidColorBrush(Color.FromArgb((byte)(opacity * 255d), 50, 84, 191)), 2.4d, lineCap: PenLineCap.Round);
        var accentPen = new Pen(accent, 2.5d, lineCap: PenLineCap.Round);
        var upperY = center.Y - 10d;
        var lowerY = center.Y + 10d;
        DrawAlignedArrow(context, new Point(center.X, upperY), size * 0.78d, direction, unitPen, accentPen, accent);
        DrawAlignedArrow(context, new Point(center.X, lowerY), size * 0.48d, direction, unitPen, accentPen, accent);
    }

    private static void DrawParallelOrthogonal(DrawingContext context, Point center, double size, bool up, IBrush accent, double opacity)
    {
        var unitPen = new Pen(new SolidColorBrush(Color.FromArgb((byte)(opacity * 255d), 50, 84, 191)), 2.4d, lineCap: PenLineCap.Round);
        var accentPen = new Pen(accent, 2.5d, lineCap: PenLineCap.Round);
        var leftX = center.X - 10d;
        var rightX = center.X + 10d;
        DrawOrthogonalArrow(context, new Point(leftX, center.Y), size * 0.76d, up, unitPen, accentPen, accent);
        DrawOrthogonalArrow(context, new Point(rightX, center.Y), size * 0.46d, up, unitPen, accentPen, accent);
    }

    private static void DrawDirectedSegment(
        DrawingContext context,
        Point center,
        double size,
        bool horizontal,
        bool upwardsOrPositive,
        IBrush blue)
    {
        var brush = blue;
        var stroke = new Pen(brush, 2.2d, lineCap: PenLineCap.Round);
        if (horizontal)
        {
            var start = new Point(center.X - (size * 0.5d), center.Y);
            var end = new Point(center.X + (size * 0.5d), center.Y);
            context.DrawLine(stroke, start, end);
            ScenePrimitives.DrawCircle(context, start, 4.2d, ProbePalette.Surface, new Pen(brush, 1.6d));
            ScenePrimitives.DrawCircle(context, end, 4.2d, ProbePalette.Surface, new Pen(brush, 1.6d));
            context.DrawLine(new Pen(brush, 1.2d), new Point(center.X, center.Y - 8d), new Point(center.X, center.Y + 8d));
        }
        else
        {
            var sign = upwardsOrPositive ? -1d : 1d;
            var start = new Point(center.X, center.Y - (sign * size * 0.5d));
            var end = new Point(center.X, center.Y + (sign * size * 0.5d));
            context.DrawLine(stroke, start, end);
            ScenePrimitives.DrawCircle(context, start, 4.2d, ProbePalette.Surface, new Pen(brush, 1.6d));
            ScenePrimitives.DrawCircle(context, end, 4.2d, ProbePalette.Surface, new Pen(brush, 1.6d));
            context.DrawLine(new Pen(brush, 1.2d), new Point(center.X - 8d, center.Y), new Point(center.X + 8d, center.Y));
        }
    }

    private static void DrawAreaLike(
        DrawingContext context,
        Point center,
        double size,
        bool right,
        bool up,
        Pen horizontalPen,
        IBrush verticalBrush,
        IBrush fill,
        Pen axisPen,
        double opacity)
    {
        var horizontalExtent = size * 0.76d;
        var verticalExtent = size * 0.62d;
        var hx = right ? horizontalExtent : -horizontalExtent;
        var vy = up ? -verticalExtent : verticalExtent;
        var origin = center;
        var horizontalEnd = origin + new Vector(hx, 0d);
        var verticalEnd = origin + new Vector(0d, vy);
        var rect = new Rect(
            Math.Min(origin.X, horizontalEnd.X),
            Math.Min(origin.Y, verticalEnd.Y),
            Math.Abs(horizontalEnd.X - origin.X),
            Math.Abs(verticalEnd.Y - origin.Y));

        context.FillRectangle(fill, rect);
        context.DrawLine(axisPen, origin + new Vector(-18d, 0d), origin + new Vector(18d, 0d));
        context.DrawLine(axisPen, origin + new Vector(0d, -18d), origin + new Vector(0d, 18d));
        context.DrawLine(horizontalPen, origin, horizontalEnd);
        context.DrawLine(new Pen(verticalBrush, 2.4d, lineCap: PenLineCap.Round), origin, verticalEnd);
    }

    private static void DrawComplexLike(
        DrawingContext context,
        Point center,
        double size,
        bool right,
        bool up,
        Pen horizontalPen,
        IBrush verticalBrush,
        IBrush rayBrush,
        Pen axisPen)
    {
        var horizontalExtent = size * 0.76d;
        var verticalExtent = size * 0.62d;
        var hx = right ? horizontalExtent : -horizontalExtent;
        var vy = up ? -verticalExtent : verticalExtent;
        var origin = center;
        var horizontalEnd = origin + new Vector(hx, 0d);
        var verticalEnd = origin + new Vector(0d, vy);
        var rayEnd = origin + new Vector(hx, vy);

        context.DrawLine(axisPen, origin + new Vector(-18d, 0d), origin + new Vector(18d, 0d));
        context.DrawLine(axisPen, origin + new Vector(0d, -18d), origin + new Vector(0d, 18d));
        context.DrawLine(horizontalPen, origin, horizontalEnd);
        context.DrawLine(new Pen(verticalBrush, 2.4d, lineCap: PenLineCap.Round), origin, verticalEnd);
        context.DrawLine(new Pen(rayBrush, 2.4d, lineCap: PenLineCap.Round), origin, rayEnd);
    }

    private static void DrawAlignedArrow(
        DrawingContext context,
        Point center,
        double length,
        double direction,
        Pen unitPen,
        Pen accentPen,
        IBrush accent)
    {
        var origin = center + new Vector(direction > 0d ? -length * 0.5d : length * 0.5d, 0d);
        var stub = origin + new Vector(direction * 12d, 0d);
        var tip = origin + new Vector(direction * length, 0d);
        context.DrawLine(unitPen, origin, stub);
        context.DrawLine(accentPen, stub, tip);
        ScenePrimitives.DrawDirectedTriangle(context, tip, 8d, accent, new Vector(direction, 0d));
        ScenePrimitives.DrawCircle(context, origin, 3.8d, ProbePalette.Surface, new Pen(new SolidColorBrush(Color.Parse("#3254bf")), 1.6d));
    }

    private static void DrawOrthogonalArrow(
        DrawingContext context,
        Point center,
        double length,
        bool up,
        Pen unitPen,
        Pen accentPen,
        IBrush accent)
    {
        var sign = up ? -1d : 1d;
        var origin = center + new Vector(0d, -sign * length * 0.5d);
        var stub = origin + new Vector(0d, sign * 12d);
        var tip = origin + new Vector(0d, sign * length);
        context.DrawLine(unitPen, origin, stub);
        context.DrawLine(accentPen, stub, tip);
        ScenePrimitives.DrawVerticalTriangle(context, tip, 8d, accent, pointsUp: up);
        ScenePrimitives.DrawCircle(context, origin, 3.8d, ProbePalette.Surface, new Pen(new SolidColorBrush(Color.Parse("#3254bf")), 1.6d));
    }

    private static void DrawTextLabel(DrawingContext context, string text, Point point, bool vertical)
    {
        var formatted = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(ProbePalette.MonoFont, FontStyle.Normal, FontWeight.SemiBold),
            9.4d,
            ProbePalette.Ink);
        if (!vertical)
        {
            context.DrawText(formatted, point);
            return;
        }

        var center = new Point(point.X, point.Y);
        var origin = new Point(center.X - (formatted.Width * 0.5d), center.Y - (formatted.Height * 0.5d));
        using (context.PushTransform(new RotateTransform(-90d, center.X, center.Y).Value))
        {
            context.DrawText(formatted, origin);
        }
    }

    private static string ButtonLabelFor(GlossaryFusionDemoState state, long sourceId)
    {
        if (state.IsAnimating)
        {
            return "busy";
        }

        if (state.Selection.FirstSourceId == sourceId)
        {
            return "picked";
        }

        return state.HasFirstSelection ? "fuse" : "select";
    }

    private static Point CenterForSource(GlossaryFusionSceneLayout layout, long sourceId)
    {
        foreach (var source in layout.Sources)
        {
            if (source.SourceId == sourceId)
            {
                return source.BoxRect.Center;
            }
        }

        return layout.StageLeft;
    }

    private static Point CenterForResult(GlossaryFusionSceneLayout layout, long resultKey)
    {
        foreach (var cell in layout.ResultCells)
        {
            if (cell.ResultKey == resultKey)
            {
                return cell.Bounds.Center;
            }
        }

        return layout.StageResult;
    }

    private static Color SourceAccent(GlossaryFusionSourceState source)
        => source.Id switch
        {
            1L => Color.Parse("#e05547"),
            2L => Color.Parse("#d99622"),
            3L => Color.Parse("#2d9c59"),
            4L => Color.Parse("#d43f56"),
            _ => Color.Parse("#243336")
        };

    private static double Smooth(double t)
    {
        var clamped = Math.Clamp(t, 0d, 1d);
        return clamped * clamped * (3d - (2d * clamped));
    }

    private static Point Lerp(Point start, Point end, double t)
        => new(
            start.X + ((end.X - start.X) * t),
            start.Y + ((end.Y - start.Y) * t));
}

internal readonly record struct GlossaryFusionSceneLayout(
    Rect Scene,
    Rect Main,
    ImmutableArray<GlossaryFusionSourceLayout> Sources,
    Point StageLeft,
    Point StageRight,
    Point StageResult,
    ImmutableArray<GlossaryFusionResultCellLayout> ResultCells);

internal readonly record struct GlossaryFusionSourceLayout(long SourceId, Rect ButtonRect, Rect BoxRect);

internal readonly record struct GlossaryFusionResultCellLayout(long ResultKey, Rect Bounds);
