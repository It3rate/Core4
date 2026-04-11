using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Rendering;

internal static class GlossaryMeasurementRenderer
{
    private static readonly Carrier VolumeCarrier = new(11L);
    private static readonly Pen CarrierPen = new(new SolidColorBrush(Color.Parse("#b7b4ad")), 6.2d, lineCap: PenLineCap.Round);
    private static readonly Pen UnitPen = new(new SolidColorBrush(Color.Parse("#4d76df")), 8.2d, lineCap: PenLineCap.Round);
    private static readonly Pen UnitTickPen = new(new SolidColorBrush(Color.Parse("#3353af")), 1.15d, lineCap: PenLineCap.Round);
    private static readonly Pen RangeTickPen = new(new SolidColorBrush(Color.FromArgb(120, 112, 112, 112)), 1d, lineCap: PenLineCap.Round);
    private static readonly Pen MinorRangeTickPen = new(new SolidColorBrush(Color.FromArgb(75, 118, 118, 118)), 0.9d, lineCap: PenLineCap.Round);
    private static readonly Pen TensionPen = new(new SolidColorBrush(Color.FromArgb(190, 214, 117, 45)), 1.2d, lineCap: PenLineCap.Round);
    private static readonly IBrush TensionBadgeBrush = new SolidColorBrush(Color.FromArgb(168, 255, 248, 240));
    private static readonly Pen TensionBadgePen = new(new SolidColorBrush(Color.FromArgb(92, 214, 117, 45)), 0.9d);

    public static GlossaryMeasurementSceneLayout Arrange(Rect bounds, GlossaryMeasurementDemoState state)
    {
        const double verticalLift = 50d;
        var scene = new Rect(
            bounds.X + 12d,
            bounds.Y + 8d,
            Math.Max(0d, bounds.Width - 24d),
            Math.Max(0d, bounds.Height - 16d));

        var origin = new Point(scene.X + 84d, scene.Y + (scene.Height * 0.60d) - verticalLift);
        var unitLength = Math.Min(172d, (scene.Width - 156d) / GlossaryMeasurementDemoState.DisplayUnits);
        var carrierEnd = new Point(origin.X + (unitLength * GlossaryMeasurementDemoState.DisplayUnits), origin.Y);
        var contactStart = new Point(origin.X, origin.Y - 82d);
        var contactEnd = new Point(origin.X + unitLength, origin.Y - 82d);
        var stackRawAnchor = new Point(origin.X + 52d, origin.Y + 30d);
        var stackResolvedAnchor = new Point(stackRawAnchor.X + 94d, stackRawAnchor.Y);

        var layouts = new List<GlossaryMeasurementCandidateLayout>(state.AvailableValues.Length);
        var gap = 10d;
        var width = Math.Min(114d, (scene.Width - 40d - ((Math.Max(0, state.AvailableValues.Length - 1)) * gap)) / Math.Max(1, state.AvailableValues.Length));
        var cardHeight = 48d;
        var left = scene.X + 6d;
        var top = scene.Y -40d;

        for (var index = 0; index < state.AvailableValues.Length; index++)
        {
            var rect = new Rect(left + ((width + gap) * index), top, width, cardHeight);
            layouts.Add(new GlossaryMeasurementCandidateLayout(
                state.AvailableValues[index].Id,
                rect,
                new Point(rect.X + 10d, rect.Center.Y - 7d),
                new Point(rect.Right - 10d, rect.Center.Y - 7d),
                new Point(rect.Center.X, rect.Bottom - 12d)));
        }

        return new GlossaryMeasurementSceneLayout(
            scene,
            origin,
            carrierEnd,
            unitLength,
            contactStart,
            contactEnd,
            stackRawAnchor,
            stackResolvedAnchor,
            layouts.ToArray());
    }

    public static void Render(DrawingContext context, Rect bounds, GlossaryMeasurementDemoState state)
    {
        var layout = Arrange(bounds, state);
        DrawCarrier(context, layout, state);
        DrawAvailableValues(context, layout, state);
        DrawAttachedValues(context, layout, state);

        if (state.TryGetAnimatingValue(out var activeValue))
        {
            DrawActiveAnimation(context, layout, state, activeValue);
        }
    }

    private static void DrawCarrier(DrawingContext context, GlossaryMeasurementSceneLayout layout, GlossaryMeasurementDemoState state)
    {
        context.DrawLine(CarrierPen, layout.Origin, layout.CarrierEnd);
        DrawRangeTicks(context, layout);

        var unitEnd = new Point(layout.Origin.X + layout.UnitLength, layout.Origin.Y);
        context.DrawLine(UnitPen, layout.Origin, unitEnd);
        DrawUnitTicks(context, layout, state.UnitTicks.Raw, 8.5d);

        ScenePrimitives.DrawCircle(
            context,
            layout.Origin,
            8d,
            ProbePalette.Surface,
            new Pen(new SolidColorBrush(Color.Parse("#243336")), 1.6d));

        ScenePrimitives.DrawCenteredBadgeLabel(
            context,
            "origin 0",
            layout.Origin + new Vector(0d, -38d),
            9d,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.MonoFont,
            ProbePalette.Surface,
            new Pen(new SolidColorBrush(Color.FromArgb(90, 197, 182, 160)), 0.8d),
            new Thickness(6, 3, 6, 3),
            8d);

        ScenePrimitives.DrawCenteredBadgeLabel(
            context,
            "unit 1",
            unitEnd + new Vector(0d, -38d),
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
            new Point((layout.Origin.X + layout.CarrierEnd.X) * 0.5d, layout.Origin.Y + 28d),
            11d,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.UiFont);
    }

    private static void DrawAvailableValues(
        DrawingContext context,
        GlossaryMeasurementSceneLayout layout,
        GlossaryMeasurementDemoState state)
    {
        foreach (var candidate in layout.Candidates)
        {
            if (state.IsAnimating && state.Animation.ValueId == candidate.ValueId)
            {
                DrawCandidateCard(context, candidate, state.AvailableValues, faded: true);
                continue;
            }

            DrawCandidateCard(context, candidate, state.AvailableValues, faded: false);
        }
    }

    private static void DrawCandidateCard(
        DrawingContext context,
        GlossaryMeasurementCandidateLayout candidate,
        IReadOnlyList<GlossaryMeasurementValueState> values,
        bool faded)
    {
        GlossaryMeasurementValueState? match = null;
        foreach (var value in values)
        {
            if (value.Id == candidate.ValueId)
            {
                match = value;
                break;
            }
        }

        if (match is null)
        {
            return;
        }

        var accent = AccentColor(candidate.ValueId);
        var fill = faded
            ? new SolidColorBrush(Color.FromArgb(48, 255, 255, 255))
            : new SolidColorBrush(Color.FromArgb(94, 255, 255, 255));
        var stroke = faded
            ? new Pen(new SolidColorBrush(Color.FromArgb(40, accent.R, accent.G, accent.B)), 0.8d)
            : new Pen(new SolidColorBrush(Color.FromArgb(118, accent.R, accent.G, accent.B)), 0.95d);
        context.DrawRectangle(fill, stroke, candidate.Card, 14d, 14d);

        var basePen = new Pen(new SolidColorBrush(Color.FromArgb(faded ? (byte)38 : (byte)78, 160, 160, 160)), 2d, lineCap: PenLineCap.Round);
        var accentPen = new Pen(new SolidColorBrush(Color.FromArgb(faded ? (byte)40 : (byte)175, accent.R, accent.G, accent.B)), 2.8d, lineCap: PenLineCap.Round);
        context.DrawLine(basePen, candidate.BarStart, candidate.BarEnd);

        var midLeft = Lerp(candidate.BarStart, candidate.BarEnd, 0.28d);
        var midRight = Lerp(candidate.BarStart, candidate.BarEnd, 0.72d);
        context.DrawLine(accentPen, midLeft, midRight);
        DrawDenseTicks(
            context,
            candidate.BarStart,
            candidate.BarEnd,
            match.Value.RawRatio.Dominant.Raw,
            accent,
            faded);

        ScenePrimitives.DrawCenteredLabel(
            context,
            $"{match.Value.RawRatio.Recessive.Raw}/{match.Value.RawRatio.Dominant.Raw}",
            candidate.LabelCenter,
            8.8d,
            faded ? new SolidColorBrush(Color.FromArgb(85, 103, 117, 122)) : ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.MonoFont);
    }

    private static void DrawAttachedValues(
        DrawingContext context,
        GlossaryMeasurementSceneLayout layout,
        GlossaryMeasurementDemoState state)
    {
        if (state.AttachedValues.Length == 0)
        {
            return;
        }

        var stackBottom = layout.StackRawAnchor.Y + ((state.AttachedValues.Length - 1) * 21d);
        context.DrawLine(
            new Pen(new SolidColorBrush(Color.FromArgb(90, 120, 120, 120)), 1d),
            layout.Origin + new Vector(0d, 10d),
            new Point(layout.Origin.X, stackBottom));

        for (var index = 0; index < state.AttachedValues.Length; index++)
        {
            var value = state.AttachedValues[index];
            var color = AccentColor(value.Id);
            var placement = PlacementPoint(layout, state.ResolvedUnits(value));
            var badgeOffset = index % 2 == 0 ? -42d : -58d;

            ScenePrimitives.DrawCircle(
                context,
                placement,
                6.2d,
                new SolidColorBrush(color),
                new Pen(new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)), 1.1d));

            var mixedAnchor = placement + new Vector(0d, badgeOffset);
            context.DrawLine(
                new Pen(new SolidColorBrush(Color.FromArgb(88, color.R, color.G, color.B)), 1d),
                placement,
                mixedAnchor + new Vector(0d, 12d));
            ScenePrimitives.DrawCenteredBadgeLabel(
                context,
                state.MixedLabel(value),
                mixedAnchor,
                8.6d,
                ProbePalette.Ink,
                FontWeight.SemiBold,
                ProbePalette.MonoFont,
                new SolidColorBrush(Color.FromArgb(215, 255, 255, 255)),
                new Pen(new SolidColorBrush(Color.FromArgb(132, color.R, color.G, color.B)), 1d),
                new Thickness(6, 3, 6, 3),
                8d);

            var rawCenter = new Point(layout.StackRawAnchor.X, layout.StackRawAnchor.Y + (index * 21d));
            var resolvedCenter = new Point(layout.StackResolvedAnchor.X, rawCenter.Y);

            context.DrawLine(
                new Pen(new SolidColorBrush(Color.FromArgb(72, color.R, color.G, color.B)), 1d),
                new Point(layout.Origin.X, rawCenter.Y),
                new Point(rawCenter.X - 24d, rawCenter.Y));

            DrawRawBadge(context, rawCenter, state.ResolvedLabel(value), color);
            DrawResolveArrow(context, rawCenter, resolvedCenter, color);
            DrawMixedReadout(context, resolvedCenter, state.MixedWhole(value), state.MixedRemainder(value), state.ResolvedRatio(value).Dominant.Raw, color);
        }
    }

    private static void DrawActiveAnimation(
        DrawingContext context,
        GlossaryMeasurementSceneLayout layout,
        GlossaryMeasurementDemoState state,
        GlossaryMeasurementValueState value)
    {
        var progress = state.Animation.Progress;
        var floatProgress = Ease(Math.Clamp(progress / 0.32d, 0d, 1d));
        var tensionProgress = Ease(Math.Clamp((progress - 0.32d) / 0.38d, 0d, 1d));
        var resolveProgress = Ease(Math.Clamp((progress - 0.70d) / 0.30d, 0d, 1d));

        var candidateLayout = FindCandidate(layout, value.Id);
        if (candidateLayout is null)
        {
            return;
        }

        var accent = AccentColor(value.Id);
        var movingStart = Lerp(candidateLayout.Value.BarStart, layout.ContactStart, floatProgress);
        var movingEnd = Lerp(candidateLayout.Value.BarEnd, layout.ContactEnd, floatProgress);
        var basePen = new Pen(new SolidColorBrush(Color.FromArgb(60, 160, 160, 160)), 2d, lineCap: PenLineCap.Round);
        var accentPen = new Pen(new SolidColorBrush(Color.FromArgb(136, accent.R, accent.G, accent.B)), 3.1d, lineCap: PenLineCap.Round);
        context.DrawLine(basePen, movingStart, movingEnd);
        context.DrawLine(accentPen, Lerp(movingStart, movingEnd, 0.28d), Lerp(movingStart, movingEnd, 0.72d));
        DrawDenseTicks(context, movingStart, movingEnd, value.RawRatio.Dominant.Raw, accent, faded: false);

        var unitEnd = new Point(layout.Origin.X + layout.UnitLength, layout.Origin.Y);
        context.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(130, 77, 118, 223)), 9d, lineCap: PenLineCap.Round), layout.Origin, unitEnd);

        if (tensionProgress > 0d)
        {
            DrawNegotiationLines(context, layout, state, value, tensionProgress);
            ScenePrimitives.DrawCenteredBadgeLabel(
                context,
                $"tension {value.RawRatio.Dominant.Raw} -> {state.UnitTicks.Raw} ticks",
                new Point((layout.ContactStart.X + layout.ContactEnd.X) * 0.5d, layout.ContactStart.Y - 24d),
                8.8d,
                ProbePalette.Ink,
                FontWeight.SemiBold,
                ProbePalette.MonoFont,
                TensionBadgeBrush,
                TensionBadgePen,
                new Thickness(6, 3, 6, 3),
                8d);
        }

        if (resolveProgress > 0d)
        {
            var startPoint = new Point(layout.ContactEnd.X - 4d, layout.Origin.Y);
            var endPoint = PlacementPoint(layout, state.ResolvedUnits(value));
            var point = Lerp(startPoint, endPoint, resolveProgress);
            ScenePrimitives.DrawCircle(
                context,
                point,
                6.2d,
                new SolidColorBrush(accent),
                new Pen(new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)), 1.1d));

            var rawCenter = Lerp(
                new Point(layout.ContactEnd.X - 24d, layout.ContactStart.Y - 8d),
                layout.StackRawAnchor,
                resolveProgress);
            var resolvedCenter = Lerp(
                new Point(layout.ContactEnd.X + 52d, layout.ContactStart.Y - 8d),
                layout.StackResolvedAnchor,
                resolveProgress);
            DrawRawBadge(context, rawCenter, state.ResolvedLabel(value), accent);
            DrawResolveArrow(context, rawCenter, resolvedCenter, accent);
            DrawMixedReadout(context, resolvedCenter, state.MixedWhole(value), state.MixedRemainder(value), state.ResolvedRatio(value).Dominant.Raw, accent);
        }
    }

    private static void DrawNegotiationLines(
        DrawingContext context,
        GlossaryMeasurementSceneLayout layout,
        GlossaryMeasurementDemoState state,
        GlossaryMeasurementValueState value,
        double progress)
    {
        var rawTicks = Math.Max(2L, value.RawRatio.Dominant.Raw);
        var unitTicks = Math.Max(2L, state.UnitTicks.Raw);
        var sampleStep = Math.Max(1L, rawTicks / 16L);

        for (long tick = 0L; tick <= rawTicks; tick += sampleStep)
        {
            var rawFraction = tick / (double)rawTicks;
            var targetTick = (long)Math.Round(rawFraction * unitTicks, MidpointRounding.AwayFromZero);
            var targetFraction = targetTick / (double)unitTicks;

            var top = new Point(layout.ContactStart.X + (layout.UnitLength * rawFraction), layout.ContactStart.Y + 6d);
            var startBottom = new Point(layout.Origin.X + (layout.UnitLength * rawFraction), layout.Origin.Y - 6d);
            var targetBottom = new Point(layout.Origin.X + (layout.UnitLength * targetFraction), layout.Origin.Y - 6d);
            var bottom = Lerp(startBottom, targetBottom, progress);
            context.DrawLine(TensionPen, top, bottom);
        }
    }

    private static void DrawRangeTicks(DrawingContext context, GlossaryMeasurementSceneLayout layout)
    {
        var maxTicks = (int)(GlossaryMeasurementDemoState.DisplayUnits * 4d);
        for (var index = 0; index <= maxTicks; index++)
        {
            var t = index / 4d;
            var x = layout.Origin.X + (layout.UnitLength * t);
            var major = index % 4 == 0;
            var pen = major ? RangeTickPen : MinorRangeTickPen;
            var size = major ? 7d : 4d;
            context.DrawLine(pen, new Point(x, layout.Origin.Y - size), new Point(x, layout.Origin.Y + size));
            if (!major)
            {
                continue;
            }

            ScenePrimitives.DrawCenteredLabel(
                context,
                (index / 4).ToString(),
                new Point(x, layout.Origin.Y + 16d),
                8.4d,
                new SolidColorBrush(Color.FromArgb(145, 112, 112, 112)),
                FontWeight.Medium,
                ProbePalette.MonoFont);
        }
    }

    private static void DrawUnitTicks(DrawingContext context, GlossaryMeasurementSceneLayout layout, long count, double size)
    {
        for (long index = 0L; index <= count; index++)
        {
            var t = index / (double)count;
            var x = layout.Origin.X + (layout.UnitLength * t);
            context.DrawLine(UnitTickPen, new Point(x, layout.Origin.Y - size), new Point(x, layout.Origin.Y + size));
        }
    }

    private static void DrawDenseTicks(
        DrawingContext context,
        Point start,
        Point end,
        long tickCount,
        Color accent,
        bool faded)
    {
        var width = end.X - start.X;
        if (Math.Abs(width) <= 0.001d)
        {
            return;
        }

        var tickPen = new Pen(new SolidColorBrush(Color.FromArgb(faded ? (byte)42 : (byte)86, accent.R, accent.G, accent.B)), 0.8d, lineCap: PenLineCap.Round);
        for (long index = 0L; index <= tickCount; index++)
        {
            var t = index / (double)tickCount;
            var x = start.X + (width * t);
            context.DrawLine(tickPen, new Point(x, start.Y - 5d), new Point(x, start.Y + 5d));
        }
    }

    private static void DrawRawBadge(DrawingContext context, Point center, string text, Color color)
    {
        ScenePrimitives.DrawCenteredBadgeLabel(
            context,
            text,
            center,
            8.7d,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.MonoFont,
            new SolidColorBrush(Color.FromArgb(214, 255, 255, 255)),
            new Pen(new SolidColorBrush(Color.FromArgb(156, color.R, color.G, color.B)), 1.1d),
            new Thickness(6, 3, 6, 3),
            8d);
    }

    private static void DrawResolveArrow(DrawingContext context, Point rawCenter, Point resolvedCenter, Color color)
    {
        var start = rawCenter + new Vector(22d, 0d);
        var end = resolvedCenter - new Vector(28d, 0d);
        var pen = new Pen(new SolidColorBrush(Color.FromArgb(126, color.R, color.G, color.B)), 1.1d, lineCap: PenLineCap.Round);
        context.DrawLine(pen, start, end);
        ScenePrimitives.DrawDirectedTriangle(
            context,
            end,
            5d,
            new SolidColorBrush(Color.FromArgb(180, color.R, color.G, color.B)),
            new Vector(1d, 0d));
    }

    private static void DrawMixedReadout(
        DrawingContext context,
        Point center,
        long whole,
        long remainder,
        long denominator,
        Color color)
    {
        var wholeCenter = center;
        ScenePrimitives.DrawCircle(
            context,
            wholeCenter,
            11d,
            new SolidColorBrush(Color.FromArgb(215, 255, 255, 255)),
            new Pen(new SolidColorBrush(Color.FromArgb(156, color.R, color.G, color.B)), 1.1d));
        ScenePrimitives.DrawCenteredLabel(
            context,
            whole.ToString(),
            new Point(wholeCenter.X, wholeCenter.Y - 7d),
            9.4d,
            ProbePalette.Ink,
            FontWeight.Bold,
            ProbePalette.MonoFont);

        if (remainder == 0L)
        {
            return;
        }

        ScenePrimitives.DrawCenteredBadgeLabel(
            context,
            $"{remainder}/{denominator}",
            center + new Vector(38d, 0d),
            8.6d,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.MonoFont,
            new SolidColorBrush(Color.FromArgb(214, 255, 255, 255)),
            new Pen(new SolidColorBrush(Color.FromArgb(156, color.R, color.G, color.B)), 1.1d),
            new Thickness(6, 3, 6, 3),
            8d);
    }

    private static Point PlacementPoint(GlossaryMeasurementSceneLayout layout, double units)
        => new(layout.Origin.X + (layout.UnitLength * units), layout.Origin.Y);

    private static GlossaryMeasurementCandidateLayout? FindCandidate(GlossaryMeasurementSceneLayout layout, long valueId)
    {
        foreach (var candidate in layout.Candidates)
        {
            if (candidate.ValueId == valueId)
            {
                return candidate;
            }
        }

        return null;
    }

    private static Color AccentColor(long id) => id switch
    {
        1L => Color.Parse("#5a8dee"),
        2L => Color.Parse("#da7b34"),
        3L => Color.Parse("#41a565"),
        4L => Color.Parse("#8b5fd1"),
        5L => Color.Parse("#cf4d7d"),
        _ => Color.Parse("#243336")
    };

    private static Point Lerp(Point start, Point end, double t)
        => new(
            start.X + ((end.X - start.X) * t),
            start.Y + ((end.Y - start.Y) * t));

    private static double Ease(double t) => 1d - Math.Pow(1d - Math.Clamp(t, 0d, 1d), 3d);
}

internal readonly record struct GlossaryMeasurementSceneLayout(
    Rect Scene,
    Point Origin,
    Point CarrierEnd,
    double UnitLength,
    Point ContactStart,
    Point ContactEnd,
    Point StackRawAnchor,
    Point StackResolvedAnchor,
    IReadOnlyList<GlossaryMeasurementCandidateLayout> Candidates);

internal readonly record struct GlossaryMeasurementCandidateLayout(
    long ValueId,
    Rect Card,
    Point BarStart,
    Point BarEnd,
    Point LabelCenter);
