using System;
using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;
using Core4.Calibration;
using Core4.Numerics;

namespace Core4.AvaloniaProbe.Rendering;

internal static class CalibrationPatternRenderer
{
    private static readonly Pen AxisPen = new(new SolidColorBrush(Color.Parse("#cbc4b8")), 1.6d, lineCap: PenLineCap.Round);
    private static readonly Pen GridPen = new(new SolidColorBrush(Color.FromArgb(120, 214, 206, 195)), 1d);
    private static readonly Pen RawPen = new(new SolidColorBrush(Color.Parse("#8a8478")), 2d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
    private static readonly Pen AveragePen = new(new SolidColorBrush(Color.Parse("#2f62d2")), 2.3d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
    private static readonly Pen AdditivePen = new(new SolidColorBrush(Color.Parse("#2f62d2")), 1.8d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
    private static readonly Pen MultiplicativePen = new(new SolidColorBrush(Color.Parse("#d07a2d")), 1.8d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
    private static readonly Pen ReversalPen = new(new SolidColorBrush(Color.Parse("#c84232")), 1.2d, lineCap: PenLineCap.Round);
    private static readonly Pen OriginPen = new(new SolidColorBrush(Color.Parse("#7b1ee0")), 1.4d);
    private static readonly Pen UnitPen = new(new SolidColorBrush(Color.Parse("#3d6fe0")), 1.4d);
    private static readonly Pen ValuePen = new(new SolidColorBrush(Color.Parse("#d07a2d")), 1.2d);
    private static readonly IBrush RawFill = new SolidColorBrush(Color.Parse("#f7f1e4"));
    private static readonly IBrush AverageFill = new SolidColorBrush(Color.Parse("#2f62d2"));
    private static readonly IBrush OriginFill = new SolidColorBrush(Color.Parse("#7b1ee0"));
    private static readonly IBrush UnitFill = new SolidColorBrush(Color.Parse("#3d6fe0"));
    private static readonly IBrush ValueFill = new SolidColorBrush(Color.Parse("#d07a2d"));

    public static void Render(DrawingContext context, Rect bounds, CalibrationPatternSeries series)
    {
        if (series.Points.Length == 0)
        {
            return;
        }

        var scene = new Rect(bounds.X + 8d, bounds.Y + 8d, Math.Max(0d, bounds.Width - 16d), Math.Max(0d, bounds.Height - 16d));
        var mainChart = new Rect(scene.X + 8d, scene.Y + 12d, Math.Max(0d, scene.Width - 16d), Math.Max(0d, scene.Height * 0.64d));
        var pressureChart = new Rect(scene.X + 8d, mainChart.Bottom + 28d, Math.Max(0d, scene.Width - 16d), Math.Max(0d, scene.Height - (mainChart.Height + 44d)));

        DrawMainChart(context, mainChart, series);
        DrawPressureChart(context, pressureChart, series);
        DrawLegend(context, scene);
    }

    private static void DrawMainChart(DrawingContext context, Rect rect, CalibrationPatternSeries series)
    {
        DrawChartFrame(context, rect);

        var left = rect.X + 24d;
        var right = rect.Right - 12d;
        var top = rect.Y + 8d;
        var bottom = rect.Bottom - 16d;
        var min = Math.Min(series.Minimum.Raw, series.Maximum.Raw);
        var max = Math.Max(series.Minimum.Raw, series.Maximum.Raw);
        var lastIndex = Math.Max(1, series.Points.Length - 1);

        var rawGeometry = new StreamGeometry();
        var averageGeometry = new StreamGeometry();
        using var rawStream = rawGeometry.Open();
        using var averageStream = averageGeometry.Open();

        for (var index = 0; index < series.Points.Length; index++)
        {
            var point = series.Points[index];
            var x = Map(index, 0, lastIndex, left, right);
            var rawY = Map(point.Sample.Raw, min, max, bottom, top);
            var averageY = Map(point.State.RunningAverage.Raw, min, max, bottom, top);

            if (index == 0)
            {
                rawStream.BeginFigure(new Point(x, rawY), false);
                averageStream.BeginFigure(new Point(x, averageY), false);
            }
            else
            {
                rawStream.LineTo(new Point(x, rawY));
                averageStream.LineTo(new Point(x, averageY));
            }
        }

        rawStream.EndFigure(false);
        averageStream.EndFigure(false);
        context.DrawGeometry(null, RawPen, rawGeometry);
        context.DrawGeometry(null, AveragePen, averageGeometry);

        for (var index = 0; index < series.Points.Length; index++)
        {
            var point = series.Points[index];
            var x = Map(index, 0, lastIndex, left, right);
            var rawY = Map(point.Sample.Raw, min, max, bottom, top);
            ScenePrimitives.DrawCircle(context, new Point(x, rawY), 2.8d, RawFill, new Pen(new SolidColorBrush(Color.Parse("#8a8478")), 0.9d));
        }

        HighlightWindow(context, series, left, right, top, bottom, min, max);

        ScenePrimitives.DrawLabel(context, "raw observed", new Point(left, rect.Y - 2d), 9d, ProbePalette.Muted, FontWeight.SemiBold, ProbePalette.MonoFont);
        ScenePrimitives.DrawLabel(context, "running center", new Point(left + 88d, rect.Y - 2d), 9d, ProbePalette.Dominant, FontWeight.SemiBold, ProbePalette.MonoFont);
        ScenePrimitives.DrawLabel(context, FormatValue(series.Maximum), new Point(rect.X - 2d, top - 2d), 8.4d, ProbePalette.Muted, fontFamily: ProbePalette.MonoFont);
        ScenePrimitives.DrawLabel(context, FormatValue(series.Minimum), new Point(rect.X - 2d, bottom - 10d), 8.4d, ProbePalette.Muted, fontFamily: ProbePalette.MonoFont);
    }

    private static void HighlightWindow(
        DrawingContext context,
        CalibrationPatternSeries series,
        double left,
        double right,
        double top,
        double bottom,
        long min,
        long max)
    {
        if (series.Points.Length < 3)
        {
            return;
        }

        var lastIndex = Math.Max(1, series.Points.Length - 1);
        var originPoint = series.Points[^3];
        var unitPoint = series.Points[^2];
        var valuePoint = series.Points[^1];

        DrawHighlightPoint(context, originPoint, left, right, top, bottom, min, max, lastIndex, OriginFill, OriginPen, "origin");
        DrawHighlightPoint(context, unitPoint, left, right, top, bottom, min, max, lastIndex, UnitFill, UnitPen, "unit");
        DrawHighlightPoint(context, valuePoint, left, right, top, bottom, min, max, lastIndex, ValueFill, ValuePen, "value");
    }

    private static void DrawHighlightPoint(
        DrawingContext context,
        CalibrationPatternPoint point,
        double left,
        double right,
        double top,
        double bottom,
        long min,
        long max,
        int lastIndex,
        IBrush fill,
        Pen stroke,
        string label)
    {
        var x = Map(point.Index - 1, 0, lastIndex, left, right);
        var y = Map(point.Sample.Raw, min, max, bottom, top);
        ScenePrimitives.DrawCircle(context, new Point(x, y), 4.1d, fill, stroke);
        ScenePrimitives.DrawBadgeLabel(context, label, new Point(x + 7d, y - 16d), 7.4d, ProbePalette.Ink, FontWeight.SemiBold, ProbePalette.MonoFont);
    }

    private static void DrawPressureChart(DrawingContext context, Rect rect, CalibrationPatternSeries series)
    {
        DrawChartFrame(context, rect);

        var left = rect.X + 24d;
        var right = rect.Right - 12d;
        var top = rect.Y + 8d;
        var bottom = rect.Bottom - 14d;
        var maxPressure = Math.Max(1L, series.MaximumPressure.Raw);
        var lastIndex = Math.Max(1, series.Points.Length - 1);

        var additiveGeometry = new StreamGeometry();
        var multiplicativeGeometry = new StreamGeometry();
        using var additiveStream = additiveGeometry.Open();
        using var multiplicativeStream = multiplicativeGeometry.Open();

        for (var index = 0; index < series.Points.Length; index++)
        {
            var point = series.Points[index];
            var x = Map(index, 0, lastIndex, left, right);
            var additiveY = Map(point.State.AdditiveFitPressure.Raw, 0, maxPressure, bottom, top);
            var multiplicativeY = Map(point.State.MultiplicativeFitPressure.Raw, 0, maxPressure, bottom, top);

            if (index == 0)
            {
                additiveStream.BeginFigure(new Point(x, additiveY), false);
                multiplicativeStream.BeginFigure(new Point(x, multiplicativeY), false);
            }
            else
            {
                additiveStream.LineTo(new Point(x, additiveY));
                multiplicativeStream.LineTo(new Point(x, multiplicativeY));
            }

            var reversalHeight = Map(point.State.ReversalStrength.Raw, 0, maxPressure, 0, bottom - top);
            context.DrawLine(
                ReversalPen,
                new Point(x, bottom),
                new Point(x, bottom - reversalHeight));
        }

        additiveStream.EndFigure(false);
        multiplicativeStream.EndFigure(false);
        context.DrawGeometry(null, AdditivePen, additiveGeometry);
        context.DrawGeometry(null, MultiplicativePen, multiplicativeGeometry);

        ScenePrimitives.DrawLabel(context, "additive fit", new Point(left, rect.Y - 2d), 8.8d, ProbePalette.Dominant, FontWeight.SemiBold, ProbePalette.MonoFont);
        ScenePrimitives.DrawLabel(context, "multiplicative fit", new Point(left + 94d, rect.Y - 2d), 8.8d, ProbePalette.OrthogonalInput, FontWeight.SemiBold, ProbePalette.MonoFont);
        ScenePrimitives.DrawLabel(context, "reversal", new Point(left + 220d, rect.Y - 2d), 8.8d, ProbePalette.TensionNegative, FontWeight.SemiBold, ProbePalette.MonoFont);
        ScenePrimitives.DrawLabel(context, maxPressure.ToString(CultureInfo.InvariantCulture), new Point(rect.X - 2d, top - 2d), 8.2d, ProbePalette.Muted, fontFamily: ProbePalette.MonoFont);
        ScenePrimitives.DrawLabel(context, "0", new Point(rect.X + 8d, bottom - 10d), 8.2d, ProbePalette.Muted, fontFamily: ProbePalette.MonoFont);
    }

    private static void DrawChartFrame(DrawingContext context, Rect rect)
    {
        context.DrawRectangle(null, AxisPen, rect, 14d, 14d);
        var third = rect.Height / 3d;
        context.DrawLine(GridPen, new Point(rect.X + 18d, rect.Y + third), new Point(rect.Right - 12d, rect.Y + third));
        context.DrawLine(GridPen, new Point(rect.X + 18d, rect.Y + (third * 2d)), new Point(rect.Right - 12d, rect.Y + (third * 2d)));
    }

    private static void DrawLegend(DrawingContext context, Rect scene)
    {
        ScenePrimitives.DrawCenteredBadgeLabel(
            context,
            "pattern page: raw stream, running center, and local fit pressures",
            new Point(scene.Right - 180d, scene.Bottom - 10d),
            8d,
            ProbePalette.Ink,
            FontWeight.Normal,
            ProbePalette.MonoFont,
            new SolidColorBrush(Color.FromArgb(110, 255, 255, 255)));
    }

    private static double Map(long value, long minimum, long maximum, double start, double end)
    {
        if (maximum == minimum)
        {
            return (start + end) * 0.5d;
        }

        var t = (value - minimum) / (double)(maximum - minimum);
        return start + ((end - start) * t);
    }

    private static string FormatValue(Measure value)
    {
        var raw = value.Raw;
        if (Math.Abs(raw) >= 1000000L)
        {
            return $"{raw / 1000000d:0.#}m";
        }

        if (Math.Abs(raw) >= 1000L)
        {
            return $"{raw / 1000d:0.#}k";
        }

        return raw.ToString(CultureInfo.InvariantCulture);
    }
}
