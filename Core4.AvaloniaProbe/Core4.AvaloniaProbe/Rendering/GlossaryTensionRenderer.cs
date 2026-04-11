using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;
using Core4.Numerics;

namespace Core4.AvaloniaProbe.Rendering;

internal static class GlossaryTensionRenderer
{
    private static readonly Pen RailPen = new(new SolidColorBrush(Color.Parse("#b7b4ad")), 5.8d, lineCap: PenLineCap.Round);
    private static readonly Pen RailTickPen = new(new SolidColorBrush(Color.FromArgb(115, 118, 118, 118)), 1d, lineCap: PenLineCap.Round);
    private static readonly Pen UnitPen = new(new SolidColorBrush(Color.Parse("#4d76df")), 7.6d, lineCap: PenLineCap.Round);
    private static readonly Pen UnitTickPen = new(new SolidColorBrush(Color.Parse("#3353af")), 1.05d, lineCap: PenLineCap.Round);
    private static readonly Pen HandlePen = new(new SolidColorBrush(Color.Parse("#243336")), 1.5d);
    private static readonly Pen BucketPen = new(new SolidColorBrush(Color.Parse("#d7d0c4")), 1d);
    private static readonly IBrush BucketBrush = new SolidColorBrush(Color.FromArgb(94, 255, 255, 255));
    private static readonly Pen OrthogonalGuidePen = new(new SolidColorBrush(Color.FromArgb(178, 141, 172, 220)), 2.8d, lineCap: PenLineCap.Round);
    private static readonly Pen OrthogonalGuideTickPen = new(new SolidColorBrush(Color.FromArgb(164, 141, 172, 220)), 1.05d, lineCap: PenLineCap.Round);

    public static GlossaryTensionSceneLayout Arrange(Rect bounds, GlossaryTensionDemoState state)
    {
        var scene = new Rect(
            bounds.X + 18d,
            bounds.Y + 14d,
            Math.Max(0d, bounds.Width - 36d),
            Math.Max(0d, bounds.Height - 28d));

        var bucketWidth = Math.Min(192d, Math.Max(176d, scene.Width * 0.26d));
        var columnGap = 22d;
        var interactionBounds = new Rect(
            scene.X,
            scene.Y,
            Math.Max(0d, scene.Width - bucketWidth - columnGap),
            scene.Height);

        var maxPositiveUnits = MaxPositiveUnits(state);
        var maxNegativeUnits = MaxNegativeUnits(state);
        var maxCarrierMagnitude = Math.Max(
            Math.Abs(GlossaryTensionDemoState.MinimumCarrierTicks),
            Math.Abs(GlossaryTensionDemoState.MaximumCarrierTicks));
        var leftTicks = Math.Max(12d, maxNegativeUnits * maxCarrierMagnitude);
        var rightTicks = Math.Max(12d, maxPositiveUnits * maxCarrierMagnitude);
        var pixelsPerCarrierTick = Math.Clamp(
            (interactionBounds.Width + 350d) / Math.Max(1d, leftTicks + rightTicks),
            8d,
            30d);

        var origin = new Point(
            interactionBounds.X - 150d + (leftTicks * pixelsPerCarrierTick),
            scene.Y + 148d);
        var railStart = new Point(origin.X + (GlossaryTensionDemoState.MinimumCarrierTicks * pixelsPerCarrierTick), origin.Y);
        var railEnd = new Point(origin.X + (GlossaryTensionDemoState.MaximumCarrierTicks * pixelsPerCarrierTick), origin.Y);
        var handle = new Point(origin.X + (state.UnitCarrierTicks.Raw * pixelsPerCarrierTick), origin.Y);
        var maxOrthogonalTicks = MaxOrthogonalTicks(state);
        var orthogonalPixelsPerTick = ((railEnd.X - railStart.X) * 0.23d) / Math.Max(1d, maxOrthogonalTicks);

        var values = new List<GlossaryTensionValueLayout>(state.Values.Length);
        foreach (var value in state.Values)
        {
            values.Add(new GlossaryTensionValueLayout(
                value.Id,
                PointForValue(origin, pixelsPerCarrierTick, orthogonalPixelsPerTick, state, value),
                state.IsOrthogonal ? TickLabel(value.Value.Recessive.Raw) : value.Label,
                AccentColor(value.Id)));
        }

        var stackTop = scene.Y -60d;
        var stackGap = 14d;
        var stackLabelHeight = 18d;
        var bucketHeight = 70d;
        var bucketX = interactionBounds.Right + columnGap;
        var buckets = ImmutableArray.Create(
            new GlossaryTensionBucketLayout("Dominant Orthogonal", new Rect(bucketX, stackTop, bucketWidth, bucketHeight)),
            new GlossaryTensionBucketLayout("Dominant Aligned", new Rect(bucketX, stackTop + bucketHeight + stackLabelHeight + stackGap, bucketWidth, bucketHeight)),
            new GlossaryTensionBucketLayout("Recessive Orthogonal", new Rect(bucketX, stackTop + ((bucketHeight + stackLabelHeight + stackGap) * 2d), bucketWidth, bucketHeight)),
            new GlossaryTensionBucketLayout("Recessive Aligned", new Rect(bucketX, stackTop + ((bucketHeight + stackLabelHeight + stackGap) * 3d), bucketWidth, bucketHeight)));

        return new GlossaryTensionSceneLayout(
            scene,
            railStart,
            railEnd,
            origin,
            handle,
            pixelsPerCarrierTick,
            orthogonalPixelsPerTick,
            maxOrthogonalTicks,
            values.ToImmutableArray(),
            buckets);
    }

    public static void Render(DrawingContext context, Rect bounds, GlossaryTensionDemoState state)
    {
        var layout = Arrange(bounds, state);
        DrawRail(context, layout, state);
        DrawValues(context, layout, state);
        DrawBuckets(context, layout, state);
    }

    private static void DrawRail(DrawingContext context, GlossaryTensionSceneLayout layout, GlossaryTensionDemoState state)
    {
        context.DrawLine(RailPen, layout.RailStart, layout.RailEnd);

        for (var tick = GlossaryTensionDemoState.MinimumCarrierTicks; tick <= GlossaryTensionDemoState.MaximumCarrierTicks; tick++)
        {
            var x = layout.Origin.X + (tick * layout.PixelsPerCarrierTick);
            var major = tick % 2 == 0;
            var size = major ? 7d : 4d;
            context.DrawLine(RailTickPen, new Point(x, layout.Origin.Y - size), new Point(x, layout.Origin.Y + size));
        }

        if (state.IsOrthogonal)
        {
            DrawOrthogonalGuide(context, layout, state);
        }

        DrawUnit(context, layout, state);

        ScenePrimitives.DrawCenteredBadgeLabel(
            context,
            "origin 0",
            state.IsOrthogonal
                ? layout.Origin + new Vector(-44d, -18d)
                : layout.Origin + new Vector(0d, -34d),
            9d,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.MonoFont,
            ProbePalette.Surface,
            new Pen(new SolidColorBrush(Color.FromArgb(90, 197, 182, 160)), 0.8d),
            new Thickness(6, 3, 6, 3),
            8d);

        ScenePrimitives.DrawCircle(context, layout.Origin, 7.6d, ProbePalette.Surface, HandlePen);
        if (state.IsOrthogonal)
        {
            ScenePrimitives.DrawCircle(
                context,
                layout.Origin,
                11d,
                new SolidColorBrush(Color.FromArgb(90, 77, 118, 223)),
                HandlePen);
            ScenePrimitives.DrawCircle(context, layout.Origin, 5d, ProbePalette.Surface, HandlePen);
        }
        else
        {
            ScenePrimitives.DrawCircle(
                context,
                layout.Handle,
                8.8d,
                new SolidColorBrush(Color.Parse("#4d76df")),
                HandlePen);
            ScenePrimitives.DrawCircle(
                context,
                layout.Handle,
                4.1d,
                new SolidColorBrush(Color.FromArgb(86, 255, 255, 255)));
        }
        ScenePrimitives.DrawCenteredBadgeLabel(
            context,
            state.IsOrthogonal ? "unit 0" : $"unit {state.UnitCarrierTicks.Raw}/{GlossaryTensionDemoState.UnitTickCount}",
            state.IsOrthogonal
                ? layout.Origin + new Vector(42d, 30d)
                : layout.Handle + new Vector(0d, -34d),
            9d,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.MonoFont,
            ProbePalette.Surface,
            new Pen(new SolidColorBrush(Color.FromArgb(90, 197, 182, 160)), 0.8d),
            new Thickness(6, 3, 6, 3),
            8d);

        ScenePrimitives.DrawCenteredLabel(
            context,
            "Carrier",
            new Point((layout.RailStart.X + layout.RailEnd.X) * 0.5d, layout.Origin.Y + (state.IsOrthogonal ? 34d : 24d)),
            11d,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.UiFont);
    }

    private static void DrawOrthogonalGuide(DrawingContext context, GlossaryTensionSceneLayout layout, GlossaryTensionDemoState state)
    {
        var guideExtent = layout.MaxOrthogonalTicks * layout.OrthogonalPixelsPerTick;
        var guideTop = layout.Origin + new Vector(0d, -guideExtent);
        var guideBottom = layout.Origin + new Vector(0d, guideExtent);
        context.DrawLine(OrthogonalGuidePen, guideTop, guideBottom);

        for (var tick = -layout.MaxOrthogonalTicks; tick <= layout.MaxOrthogonalTicks; tick++)
        {
            var point = new Point(layout.Origin.X, layout.Origin.Y - (tick * layout.OrthogonalPixelsPerTick));
            var major = tick == 0 || Math.Abs(tick) == layout.MaxOrthogonalTicks || tick % 2 == 0;
            var halfWidth = major ? 7d : 4d;
            context.DrawLine(
                OrthogonalGuideTickPen,
                point + new Vector(-halfWidth, 0d),
                point + new Vector(halfWidth, 0d));
        }
    }

    private static void DrawUnit(DrawingContext context, GlossaryTensionSceneLayout layout, GlossaryTensionDemoState state)
    {
        if (state.IsOrthogonal)
        {
            return;
        }

        var unitEnd = state.IsOrthogonal
            ? layout.Origin + new Vector(0d, -(state.EffectiveMagnitudeTicks.Raw * layout.PixelsPerCarrierTick))
            : layout.Handle;
        context.DrawLine(UnitPen, layout.Origin, unitEnd);

        for (var tick = 0; tick <= GlossaryTensionDemoState.UnitTickCount; tick++)
        {
            var t = tick / (double)GlossaryTensionDemoState.UnitTickCount;
            var point = Lerp(layout.Origin, unitEnd, t);
            if (state.IsOrthogonal)
            {
                context.DrawLine(UnitTickPen, point + new Vector(-6d, 0d), point + new Vector(6d, 0d));
            }
            else
            {
                context.DrawLine(UnitTickPen, point + new Vector(0d, -7d), point + new Vector(0d, 7d));
            }
        }
    }

    private static void DrawValues(DrawingContext context, GlossaryTensionSceneLayout layout, GlossaryTensionDemoState state)
    {
        foreach (var value in layout.Values)
        {
            ScenePrimitives.DrawCircle(
                context,
                value.Center,
                6.5d,
                new SolidColorBrush(value.Accent),
                new Pen(new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)), 1.1d));

            var offset = state.IsOrthogonal
                ? new Vector(56d, value.Center.Y <= layout.Origin.Y ? -6d : -6d)
                : new Vector(0d, value.Center.Y <= layout.Origin.Y ? -40d : 28d);
            var labelCenter = value.Center + offset;
            context.DrawLine(
                new Pen(new SolidColorBrush(Color.FromArgb(88, value.Accent.R, value.Accent.G, value.Accent.B)), 1d),
                value.Center,
                state.IsOrthogonal
                    ? new Point(labelCenter.X - 22d, labelCenter.Y)
                    : new Point(labelCenter.X, labelCenter.Y + (offset.Y < 0d ? 10d : -10d)));
            ScenePrimitives.DrawCenteredBadgeLabel(
                context,
                value.Label,
                labelCenter,
                8.8d,
                ProbePalette.Ink,
                FontWeight.SemiBold,
                ProbePalette.MonoFont,
                new SolidColorBrush(Color.FromArgb(216, 255, 255, 255)),
                new Pen(new SolidColorBrush(Color.FromArgb(140, value.Accent.R, value.Accent.G, value.Accent.B)), 1d),
                new Thickness(6, 3, 6, 3),
                8d);
        }
    }

    private static void DrawBuckets(DrawingContext context, GlossaryTensionSceneLayout layout, GlossaryTensionDemoState state)
    {
        foreach (var bucket in layout.Buckets)
        {
            context.DrawRectangle(BucketBrush, BucketPen, bucket.Bounds, 18d, 18d);
            ScenePrimitives.DrawCenteredLabel(
                context,
                bucket.Label,
                new Point(bucket.Bounds.Center.X, bucket.Bounds.Bottom + 6d),
                10d,
                ProbePalette.Ink,
                FontWeight.SemiBold,
                ProbePalette.MonoFont);

            var items = BucketItems(state.Detached, bucket.Label);
            if (bucket.Label.Contains("Orthogonal", StringComparison.Ordinal))
            {
                DrawOrthogonalBucket(context, bucket.Bounds, items);
            }
            else
            {
                DrawAlignedBucket(context, bucket.Bounds, items, bucket.Label.StartsWith("Dominant", StringComparison.Ordinal));
            }
        }
    }

    private static void DrawOrthogonalBucket(
        DrawingContext context,
        Rect bounds,
        ImmutableArray<GlossaryDetachedValueState> items)
    {
        if (items.IsDefaultOrEmpty)
        {
            return;
        }

        var maxColumns = Math.Max(1, Math.Min(3, items.Length));
        var columnSpacing = Math.Min(46d, (bounds.Width - 40d) / maxColumns);
        for (var index = 0; index < items.Length; index++)
        {
            var item = items[index];
            var accent = AccentColor(item.Value.Id);
            var column = index % maxColumns;
            var x = bounds.X + 26d + (column * columnSpacing);
            var upward = item.IsDominant;
            var origin = upward
                ? new Point(x, bounds.Bottom - 14d)
                : new Point(x, bounds.Y + 14d);
            var magnitudeUnits = Math.Abs(item.Value.Value.Approximate);
            var totalLength = 12d + (magnitudeUnits * 18d);
            var stubLength = 10d;
            var sign = upward ? -1d : 1d;
            var stubEnd = origin + new Vector(0d, sign * stubLength);
            var tip = origin + new Vector(0d, sign * totalLength);

            var axisPen = new Pen(new SolidColorBrush(accent), 2.4d, lineCap: PenLineCap.Round);
            var accentBrush = new SolidColorBrush(accent);
                context.DrawLine(new Pen(new SolidColorBrush(Color.Parse("#3254bf")), 2.6d, lineCap: PenLineCap.Round), origin, stubEnd);
                context.DrawLine(axisPen, stubEnd, tip);
                ScenePrimitives.DrawVerticalTriangle(context, tip, 9d, accentBrush, pointsUp: upward);
                ScenePrimitives.DrawCircle(context, origin, 4.2d, ProbePalette.Surface, new Pen(new SolidColorBrush(Color.Parse("#3254bf")), 1.8d));

                DrawRotatedLabel(
                    context,
                    TickLabel(item.Value.Value.Recessive.Raw),
                    new Point(origin.X - 16d, bounds.Center.Y + (upward ? -2d : 2d)),
                    9.2d,
                    ProbePalette.Ink,
                FontWeight.SemiBold);
        }
    }

    private static void DrawAlignedBucket(
        DrawingContext context,
        Rect bounds,
        ImmutableArray<GlossaryDetachedValueState> items,
        bool dominant)
    {
        if (items.IsDefaultOrEmpty)
        {
            return;
        }

        var maxRows = Math.Max(1, Math.Min(3, items.Length));
        var rowSpacing = Math.Min(22d, (bounds.Height - 24d) / maxRows);
        for (var index = 0; index < items.Length; index++)
        {
            var item = items[index];
            var accent = AccentColor(item.Value.Id);
            var y = bounds.Y + 18d + (index * rowSpacing);
            var magnitudeUnits = Math.Abs(item.Value.Value.Approximate);
            var totalLength = 26d + (magnitudeUnits * 18d);
            var unitLength = 14d;
            var accentBrush = new SolidColorBrush(accent);

            if (dominant)
            {
                var origin = new Point(bounds.X + 18d, y);
                var stubEnd = origin + new Vector(unitLength, 0d);
                var tip = origin + new Vector(Math.Min(bounds.Width - 38d, totalLength), 0d);

                context.DrawLine(new Pen(new SolidColorBrush(Color.Parse("#3254bf")), 2.8d, lineCap: PenLineCap.Round), origin, stubEnd);
                context.DrawLine(new Pen(accentBrush, 2.8d, lineCap: PenLineCap.Round), stubEnd, tip);
                ScenePrimitives.DrawDirectedTriangle(context, tip, 9d, accentBrush, new Vector(1d, 0d));
                ScenePrimitives.DrawCircle(context, origin, 4.2d, ProbePalette.Surface, new Pen(new SolidColorBrush(Color.Parse("#3254bf")), 1.8d));
                DrawLeftAlignedLabel(context, MagnitudeLabel(item.Value.Value), new Point(origin.X + 2d, y - 18d));
            }
            else
            {
                var origin = new Point(bounds.Right - 18d, y);
                var stubStart = origin - new Vector(unitLength, 0d);
                var tip = origin - new Vector(Math.Min(bounds.Width - 38d, totalLength), 0d);

                context.DrawLine(new Pen(accentBrush, 2.8d, lineCap: PenLineCap.Round), tip, stubStart);
                context.DrawLine(new Pen(new SolidColorBrush(Color.Parse("#3254bf")), 2.8d, lineCap: PenLineCap.Round), stubStart, origin);
                ScenePrimitives.DrawDirectedTriangle(context, tip, 9d, accentBrush, new Vector(-1d, 0d));
                ScenePrimitives.DrawCircle(context, origin, 4.2d, ProbePalette.Surface, new Pen(new SolidColorBrush(Color.Parse("#3254bf")), 1.8d));
                DrawRightAlignedLabel(context, MagnitudeLabel(item.Value.Value), new Point(origin.X - 4d, y - 18d));
            }
        }
    }

    private static void DrawRotatedLabel(
        DrawingContext context,
        string text,
        Point center,
        double size,
        IBrush brush,
        FontWeight weight)
    {
        var formatted = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(ProbePalette.MonoFont, FontStyle.Normal, weight),
            size,
            brush);
        var origin = new Point(center.X - (formatted.Width * 0.5d), center.Y - (formatted.Height * 0.5d));
        using (context.PushTransform(new RotateTransform(-90d, center.X, center.Y).Value))
        {
            context.DrawText(formatted, origin);
        }
    }

    private static void DrawLeftAlignedLabel(DrawingContext context, string text, Point point)
    {
        ScenePrimitives.DrawLabel(
            context,
            text,
            point,
            9.2d,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.MonoFont);
    }

    private static void DrawRightAlignedLabel(DrawingContext context, string text, Point rightTop)
    {
        var formatted = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(ProbePalette.MonoFont, FontStyle.Normal, FontWeight.SemiBold),
            9.2d,
            ProbePalette.Ink);
        context.DrawText(formatted, new Point(rightTop.X - formatted.Width, rightTop.Y));
    }

    private static ImmutableArray<GlossaryDetachedValueState> BucketItems(
        ImmutableArray<GlossaryDetachedValueState> detached,
        string bucket)
    {
        var builder = ImmutableArray.CreateBuilder<GlossaryDetachedValueState>();
        foreach (var item in detached)
        {
            if (string.Equals(item.Bucket, bucket, StringComparison.Ordinal))
            {
                builder.Add(item);
            }
        }

        return builder.ToImmutable();
    }

    private static Point PointForValue(
        Point origin,
        double pixelsPerCarrierTick,
        double orthogonalPixelsPerTick,
        GlossaryTensionDemoState state,
        GlossaryTensionValueState value)
    {
        if (state.IsOrthogonal)
        {
            return new Point(origin.X, origin.Y - (value.Value.Recessive.Raw * orthogonalPixelsPerTick));
        }

        return new Point(origin.X + (value.Value.Approximate * state.UnitCarrierTicks.Raw * pixelsPerCarrierTick), origin.Y);
    }

    private static double MaxPositiveUnits(GlossaryTensionDemoState state)
    {
        var max = 0d;
        foreach (var value in state.Values)
        {
            max = Math.Max(max, Math.Max(0d, value.Value.Approximate));
        }

        foreach (var detached in state.Detached)
        {
            max = Math.Max(max, Math.Max(0d, detached.Value.Value.Approximate));
        }

        return Math.Max(1d, max);
    }

    private static double MaxNegativeUnits(GlossaryTensionDemoState state)
    {
        var max = 0d;
        foreach (var value in state.Values)
        {
            max = Math.Max(max, Math.Max(0d, -value.Value.Approximate));
        }

        foreach (var detached in state.Detached)
        {
            max = Math.Max(max, Math.Max(0d, -detached.Value.Value.Approximate));
        }

        return Math.Max(1d, max);
    }

    private static long MaxOrthogonalTicks(GlossaryTensionDemoState state)
    {
        var max = 1L;
        foreach (var value in state.Values)
        {
            max = Math.Max(max, Math.Abs(value.Value.Recessive.Raw));
        }

        foreach (var detached in state.Detached)
        {
            max = Math.Max(max, Math.Abs(detached.Value.Value.Recessive.Raw));
        }

        return max;
    }

    private static string MagnitudeLabel(Ratio ratio)
    {
        var numerator = Math.Abs(ratio.Recessive.Raw);
        var denominator = Math.Max(1L, ratio.Dominant.Raw);
        var whole = numerator / denominator;
        var remainder = numerator % denominator;
        return remainder == 0L
            ? whole.ToString(CultureInfo.InvariantCulture)
            : whole == 0L
                ? $"{remainder}/{denominator}"
                : $"{whole} {remainder}/{denominator}";
    }

    private static string TickLabel(long ticks)
    {
        var magnitude = Math.Abs(ticks);
        return magnitude == 1L
            ? "1 tick"
            : $"{magnitude.ToString(CultureInfo.InvariantCulture)} ticks";
    }

    private static Color AccentColor(long id) => id switch
    {
        1L => Color.Parse("#5a8dee"),
        2L => Color.Parse("#da7b34"),
        3L => Color.Parse("#41a565"),
        4L => Color.Parse("#8b5fd1"),
        _ => Color.Parse("#243336")
    };

    private static Point Lerp(Point start, Point end, double t)
        => new(
            start.X + ((end.X - start.X) * t),
            start.Y + ((end.Y - start.Y) * t));
}

internal readonly record struct GlossaryTensionSceneLayout(
    Rect Scene,
    Point RailStart,
    Point RailEnd,
    Point Origin,
    Point Handle,
    double PixelsPerCarrierTick,
    double OrthogonalPixelsPerTick,
    long MaxOrthogonalTicks,
    ImmutableArray<GlossaryTensionValueLayout> Values,
    ImmutableArray<GlossaryTensionBucketLayout> Buckets);

internal readonly record struct GlossaryTensionValueLayout(long Id, Point Center, string Label, Color Accent);

internal readonly record struct GlossaryTensionBucketLayout(string Label, Rect Bounds);
