using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;
using Core4.Numerics;

namespace Core4.AvaloniaProbe.Controls;

/// <summary>
/// A fully code-driven directed-segment scene. Avalonia supplies the window/input shell, while this control owns
/// its own draw calls and pointer interaction.
/// </summary>
public sealed class DirectedSegmentSceneControl : Control
{
    private const double UnitPixels = 28d;
    private const double HandleRadius = 14d;
    private static readonly IBrush SurfaceBrush = new SolidColorBrush(Color.Parse("#f7f1e4"));
    private static readonly IBrush AxisBrush = new SolidColorBrush(Color.Parse("#c8c4b8"));
    private static readonly IBrush TickBrush = new SolidColorBrush(Color.Parse("#d9d5ca"));
    private static readonly IBrush DominantBrush = new SolidColorBrush(Color.Parse("#1655ad"));
    private static readonly IBrush RecessiveBrush = new SolidColorBrush(Color.Parse("#4a72f3"));
    private static readonly IBrush OriginBrush = new SolidColorBrush(Color.Parse("#b316b8"));
    private static readonly IBrush SegmentBrush = new SolidColorBrush(Color.Parse("#d9bf81"));
    private static readonly IBrush TextBrush = new SolidColorBrush(Color.Parse("#2c3a44"));
    private static readonly IBrush SubtleTextBrush = new SolidColorBrush(Color.Parse("#697882"));

    private DirectedSegmentSceneState _state = DirectedSegmentSceneState.Default;
    private DragHandle _activeHandle;

    public DirectedSegmentSceneControl()
    {
        MinHeight = 420;
        MinWidth = 640;
        ClipToBounds = true;
    }

    public DirectedSegmentSceneState State
    {
        get => _state;
        set
        {
            if (_state == value)
            {
                return;
            }

            _state = value;
            InvalidateVisual();
            StateChanged?.Invoke(this, _state);
        }
    }

    public event EventHandler<DirectedSegmentSceneState>? StateChanged;

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds;
        context.FillRectangle(SurfaceBrush, bounds);

        var axisY = bounds.Height * 0.62d;
        var originX = bounds.Width * 0.48d + (_state.Origin.Raw * UnitPixels);
        var left = 48d;
        var right = Math.Max(left + 40d, bounds.Width - 48d);
        var unitPen = new Pen(AxisBrush, 1.8d);
        var dominantPen = new Pen(DominantBrush, 5d, lineCap: PenLineCap.Round);
        var recessivePen = new Pen(RecessiveBrush, 5d, lineCap: PenLineCap.Round);
        var overlayPen = new Pen(SegmentBrush, 10d, lineCap: PenLineCap.Round);

        context.DrawLine(unitPen, new Point(left, axisY), new Point(right, axisY));
        DrawTicks(context, originX, axisY, left, right);

        var recessiveAnchorX = originX - (_state.RecessiveSpan.Raw * UnitPixels);
        var dominantAnchorX = originX + (_state.DominantSpan.Raw * UnitPixels);
        var recessiveValueX = originX - (_state.RecessiveValue.Raw * UnitPixels);
        var dominantValueX = originX + (_state.DominantValue.Raw * UnitPixels);
        var overlayY = axisY - 52d;

        context.DrawLine(recessivePen, new Point(originX, axisY), new Point(recessiveAnchorX, axisY));
        context.DrawLine(dominantPen, new Point(originX, axisY), new Point(dominantAnchorX, axisY));
        DrawHollowCircle(context, new Point(recessiveAnchorX, axisY), 10d, RecessiveBrush, 4.4d);
        DrawHollowTriangle(context, new Point(dominantAnchorX, axisY), 13d, DominantBrush, 4d);

        context.DrawLine(recessivePen, new Point(originX, axisY), new Point(recessiveValueX, axisY));
        context.DrawLine(dominantPen, new Point(originX, axisY), new Point(dominantValueX, axisY));
        context.DrawLine(overlayPen, new Point(recessiveValueX, overlayY), new Point(dominantValueX, overlayY));

        DrawSolidCircle(context, new Point(recessiveValueX, axisY), 8.4d, RecessiveBrush);
        DrawSolidTriangle(context, new Point(dominantValueX, axisY), 11d, DominantBrush);
        DrawSolidCircle(context, new Point(recessiveValueX, overlayY), 7.4d, SegmentBrush);
        DrawSolidTriangle(context, new Point(dominantValueX, overlayY), 12d, SegmentBrush);
        DrawOrigin(context, new Point(originX, axisY));

        DrawLabel(context, "Core4 directed segment", new Point(left, 24d), 24d, FontWeight.SemiBold, TextBrush);
        DrawLabel(context, "Drag the blue endpoints. Type a total length and apply it through the shared Core4 state.", new Point(left, 58d), 14d, FontWeight.Normal, SubtleTextBrush);
        DrawLabel(context, $"{_state.RecessiveReading}r + {_state.DominantReading}d", new Point(left, bounds.Height - 78d), 18d, FontWeight.SemiBold, TextBrush);
        DrawLabel(context, $"basis {_state.SegmentBasis.Ratio}   length {_state.Length.Raw}", new Point(left, bounds.Height - 50d), 14d, FontWeight.Normal, SubtleTextBrush);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var point = e.GetPosition(this);
        if (Distance(point, RecessiveValuePoint()) <= HandleRadius)
        {
            _activeHandle = DragHandle.RecessiveValue;
            e.Pointer.Capture(this);
            e.Handled = true;
            return;
        }

        if (Distance(point, DominantValuePoint()) <= HandleRadius)
        {
            _activeHandle = DragHandle.DominantValue;
            e.Pointer.Capture(this);
            e.Handled = true;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_activeHandle == DragHandle.None || e.Pointer.Captured != this)
        {
            return;
        }

        var point = e.GetPosition(this);
        var originX = OriginPoint().X;
        var units = (long)Math.Round((point.X - originX) / UnitPixels, MidpointRounding.AwayFromZero);

        State = _activeHandle switch
        {
            DragHandle.DominantValue => _state with { DominantValue = new Measure(units) },
            DragHandle.RecessiveValue => _state with { RecessiveValue = new Measure(-units) },
            _ => _state
        };

        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _activeHandle = DragHandle.None;
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private Point OriginPoint() => new(
        Bounds.Width * 0.48d + (_state.Origin.Raw * UnitPixels),
        Bounds.Height * 0.62d);

    private Point RecessiveValuePoint() => new(
        OriginPoint().X - (_state.RecessiveValue.Raw * UnitPixels),
        OriginPoint().Y);

    private Point DominantValuePoint() => new(
        OriginPoint().X + (_state.DominantValue.Raw * UnitPixels),
        OriginPoint().Y);

    private static double Distance(Point a, Point b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }

    private static void DrawTicks(DrawingContext context, double originX, double axisY, double left, double right)
    {
        for (var offset = -12L; offset <= 12L; offset++)
        {
            var x = originX + (offset * UnitPixels);
            if (x < left || x > right)
            {
                continue;
            }

            var height = offset == 0 ? 14d : 8d;
            var pen = new Pen(offset == 0 ? AxisBrush : TickBrush, offset == 0 ? 1.8d : 1.2d);
            context.DrawLine(pen, new Point(x, axisY - height), new Point(x, axisY + height));

            if (offset != 0)
            {
                DrawStaticLabel(context, offset.ToString(CultureInfo.InvariantCulture), new Point(x - 5d, axisY + 14d), 12d, TickBrush);
            }
        }
    }

    private static void DrawOrigin(DrawingContext context, Point center)
    {
        var rect = new Rect(center.X - 10d, center.Y - 10d, 20d, 20d);
        context.DrawRectangle(null, new Pen(OriginBrush, 5d), rect);
    }

    private static void DrawHollowCircle(DrawingContext context, Point center, double radius, IBrush stroke, double thickness)
    {
        context.DrawEllipse(null, new Pen(stroke, thickness), center, radius, radius);
    }

    private static void DrawSolidCircle(DrawingContext context, Point center, double radius, IBrush fill)
    {
        context.DrawEllipse(fill, null, center, radius, radius);
    }

    private static void DrawHollowTriangle(DrawingContext context, Point tip, double size, IBrush stroke, double thickness)
    {
        context.DrawGeometry(null, new Pen(stroke, thickness), TriangleGeometry(tip, size));
    }

    private static void DrawSolidTriangle(DrawingContext context, Point tip, double size, IBrush fill)
    {
        context.DrawGeometry(fill, null, TriangleGeometry(tip, size));
    }

    private static StreamGeometry TriangleGeometry(Point tip, double size)
    {
        var geometry = new StreamGeometry();
        using var stream = geometry.Open();
        stream.BeginFigure(tip, true);
        stream.LineTo(new Point(tip.X - size, tip.Y - (size * 0.58d)));
        stream.LineTo(new Point(tip.X - size, tip.Y + (size * 0.58d)));
        stream.EndFigure(true);
        return geometry;
    }

    private void DrawLabel(DrawingContext context, string text, Point point, double size, FontWeight weight, IBrush brush)
    {
        var formatted = CreateText(text, size, weight, brush);
        context.DrawText(formatted, point);
    }

    private static void DrawStaticLabel(DrawingContext context, string text, Point point, double size, IBrush brush)
    {
        var formatted = CreateText(text, size, FontWeight.Normal, brush);
        context.DrawText(formatted, point);
    }

    private static FormattedText CreateText(string text, double size, FontWeight weight, IBrush brush) =>
        new(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily.Default, FontStyle.Normal, weight),
            size,
            brush);

    private enum DragHandle
    {
        None,
        RecessiveValue,
        DominantValue
    }
}
