using System;
using Avalonia;
using Avalonia.Media;
using Core4.Calibration;
using Core4.Numerics;

namespace Core4.AvaloniaProbe.Rendering;

internal static class CalibrationRenderer
{
    private static readonly Pen AxisPen = new(new SolidColorBrush(Color.Parse("#bcb6aa")), 2.2d, lineCap: PenLineCap.Round);
    private static readonly Pen RulerPen = new(new SolidColorBrush(Color.Parse("#d7d0c5")), 1d, lineCap: PenLineCap.Round);
    private static readonly Pen OriginPen = new(new SolidColorBrush(Color.Parse("#243336")), 2d, lineCap: PenLineCap.Round);
    private static readonly Pen UnitPen = new(new SolidColorBrush(Color.Parse("#3d6fe0")), 4d, lineCap: PenLineCap.Round);
    private static readonly Pen ValuePen = new(new SolidColorBrush(Color.Parse("#d07a2d")), 3.4d, lineCap: PenLineCap.Round);
    private static readonly Pen PendingPen = new(new SolidColorBrush(Color.Parse("#c9c1b4")), 2d, lineCap: PenLineCap.Round);
    private static readonly IBrush RatioFill = new SolidColorBrush(Color.FromArgb(220, 249, 246, 239));
    private static readonly Pen RatioStroke = new(new SolidColorBrush(Color.FromArgb(90, 182, 170, 150)), 0.8d);

    public static void Render(DrawingContext context, Rect bounds, Calibrater calibrater, string ratioLabel)
    {
        var scene = new Rect(bounds.X + 2d, bounds.Y + 2d, Math.Max(0d, bounds.Width - 4d), Math.Max(0d, bounds.Height - 4d));
        var chart = new Rect(scene.X + 2d, scene.Y + 1d, Math.Max(0d, scene.Width - 42d), Math.Max(0d, scene.Height - 6d));
        var axisY = chart.Y + (chart.Height * 0.34d);
        var left = chart.X + 6d;
        var right = chart.Right - 6d;

        if (!calibrater.HasOrigin)
        {
            ScenePrimitives.DrawCenteredLabel(
                context,
                "waiting",
                scene.Center,
                7.8d,
                ProbePalette.Muted,
                FontWeight.SemiBold,
                ProbePalette.MonoFont);
            return;
        }

        var (minimum, maximum) = Range(calibrater);
        var x = new Func<Measure, double>(measure => Map(measure.Raw, minimum, maximum, left, right));

        context.DrawLine(AxisPen, new Point(left, axisY), new Point(right, axisY));
        DrawRuler(context, calibrater, minimum, maximum, axisY, x);

        var origin = new Point(x(calibrater.OriginMeasure), axisY);
        ScenePrimitives.DrawCircle(context, origin, 3.9d, ProbePalette.Surface, OriginPen);

        if (calibrater.HasUnit)
        {
            var unit = new Point(x(calibrater.UnitMeasure), axisY);
            context.DrawLine(UnitPen, origin, unit);
            ScenePrimitives.DrawCircle(
                context,
                unit,
                3.9d,
                ProbePalette.Surface,
                new Pen(new SolidColorBrush(Color.Parse("#3d6fe0")), 1.3d));

            if (calibrater.HasValue)
            {
                var value = new Point(x(calibrater.ValueMeasure), axisY);
                context.DrawLine(ValuePen, unit, value);
                ScenePrimitives.DrawCircle(
                    context,
                    value,
                    3.5d,
                    new SolidColorBrush(Color.Parse("#d07a2d")),
                    new Pen(new SolidColorBrush(Color.FromArgb(190, 255, 255, 255)), 0.9d));
            }
        }
        else
        {
            context.DrawLine(PendingPen, origin + new Vector(8d, 0d), origin + new Vector(24d, 0d));
        }

        context.DrawRectangle(RatioFill, RatioStroke, new Rect(scene.Right - 34d, axisY - 11d, 30d, 14d), 7d, 7d);
        ScenePrimitives.DrawCenteredLabel(
            context,
            ratioLabel,
            new Point(scene.Right - 19d, axisY - 9d),
            7.8d,
            ProbePalette.Ink,
            FontWeight.Bold,
            ProbePalette.MonoFont);
    }

    private static void DrawRuler(
        DrawingContext context,
        Calibrater calibrater,
        long minimum,
        long maximum,
        double axisY,
        Func<Measure, double> x)
    {
        if (!calibrater.HasUnit || calibrater.TickSize == Measure.Zero)
        {
            context.DrawLine(RulerPen, new Point(x(calibrater.OriginMeasure), axisY - 5d), new Point(x(calibrater.OriginMeasure), axisY + 5d));
            return;
        }

        var step = calibrater.TickSize.Raw;
        var startOffset = (long)Math.Floor((minimum - calibrater.OriginMeasure.Raw) / (double)step) - 1L;
        var endOffset = (long)Math.Ceiling((maximum - calibrater.OriginMeasure.Raw) / (double)step) + 1L;

        for (var offset = startOffset; offset <= endOffset; offset++)
        {
            var tick = new Measure(calibrater.OriginMeasure.Raw + (offset * step));
            var size = offset == 0L ? 6d : 3.5d;
            context.DrawLine(RulerPen, new Point(x(tick), axisY - size), new Point(x(tick), axisY + size));
        }
    }

    private static (long Minimum, long Maximum) Range(Calibrater calibrater)
    {
        var minimum = calibrater.OriginMeasure.Raw;
        var maximum = calibrater.OriginMeasure.Raw;

        if (calibrater.HasUnit)
        {
            minimum = Math.Min(minimum, calibrater.UnitMeasure.Raw);
            maximum = Math.Max(maximum, calibrater.UnitMeasure.Raw);
        }

        if (calibrater.HasValue)
        {
            minimum = Math.Min(minimum, calibrater.ValueMeasure.Raw);
            maximum = Math.Max(maximum, calibrater.ValueMeasure.Raw);
        }

        var padding = Math.Max(1L, calibrater.TickSize.Raw == 0L ? Math.Max(4L, (maximum - minimum).Absolute().Raw / 2L) : calibrater.TickSize.Raw);
        if (minimum == maximum)
        {
            minimum -= padding;
            maximum += padding;
        }
        else
        {
            minimum -= padding;
            maximum += padding;
        }

        return (minimum, maximum);
    }

    private static double Map(long value, long minimum, long maximum, double left, double right)
    {
        if (maximum == minimum)
        {
            return (left + right) * 0.5d;
        }

        var t = (value - minimum) / (double)(maximum - minimum);
        return left + ((right - left) * t);
    }

    private static Measure Absolute(this long value) => new(value < 0L ? -value : value);
}
