using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace Core4.AvaloniaProbe.Controls;

/// <summary>
/// A compact integer slider rendered directly in code so the demo can share the same immediate drawing style as
/// the scene itself.
/// </summary>
public sealed class MeasureSliderControl : Control
{
    private const double TrackInset = 14d;
    private const double TrackHeight = 3.8d;
    private const double ThumbRadius = 4.8d;

    private long _minimum;
    private long _maximum = 1;
    private long _value;
    private bool _dragging;

    public MeasureSliderControl()
    {
        Height = 16;
        MinWidth = 96;
        ClipToBounds = true;
    }

    public long Minimum
    {
        get => _minimum;
        set
        {
            if (_minimum == value)
            {
                return;
            }

            _minimum = value;
            if (_maximum < _minimum)
            {
                _maximum = _minimum;
            }

            Value = Coerce(_value);
            InvalidateVisual();
        }
    }

    public long Maximum
    {
        get => _maximum;
        set
        {
            if (_maximum == value)
            {
                return;
            }

            _maximum = Math.Max(value, _minimum);
            Value = Coerce(_value);
            InvalidateVisual();
        }
    }

    public long Value
    {
        get => _value;
        set
        {
            var coerced = Coerce(value);
            if (_value == coerced)
            {
                return;
            }

            _value = coerced;
            InvalidateVisual();
            ValueChanged?.Invoke(this, _value);
        }
    }

    public IBrush AccentBrush { get; set; } = new SolidColorBrush(Color.Parse("#0f4fa4"));

    public IBrush TrackBrush { get; set; } = new SolidColorBrush(Color.Parse("#d7e1f5"));

    public bool HollowThumb { get; set; } = true;

    public event EventHandler<long>? ValueChanged;

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var trackRect = TrackRect();
        var y = trackRect.Center.Y;
        var valueX = XFor(Value);

        context.DrawLine(new Pen(TrackBrush, TrackHeight, lineCap: PenLineCap.Round), new Point(trackRect.Left, y), new Point(trackRect.Right, y));
        context.DrawLine(new Pen(AccentBrush, TrackHeight, lineCap: PenLineCap.Round), new Point(trackRect.Left, y), new Point(valueX, y));

        var thumbStroke = new Pen(AccentBrush, HollowThumb ? 1.9d : 1.4d);
        var fill = HollowThumb ? Brushes.White : AccentBrush;
        context.DrawEllipse(fill, thumbStroke, new Point(valueX, y), ThumbRadius, ThumbRadius);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _dragging = true;
        e.Pointer.Capture(this);
        UpdateFromPoint(e.GetPosition(this).X);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!_dragging || e.Pointer.Captured != this)
        {
            return;
        }

        UpdateFromPoint(e.GetPosition(this).X);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _dragging = false;
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private void UpdateFromPoint(double x)
    {
        var track = TrackRect();
        if (track.Width <= 0d || Maximum <= Minimum)
        {
            Value = Minimum;
            return;
        }

        var clamped = Math.Clamp(x, track.Left, track.Right);
        var ratio = (clamped - track.Left) / track.Width;
        var world = Minimum + (ratio * (Maximum - Minimum));
        Value = (long)Math.Round(world, MidpointRounding.AwayFromZero);
    }

    private double XFor(long value)
    {
        var track = TrackRect();
        if (Maximum <= Minimum)
        {
            return track.Left;
        }

        var ratio = (double)(value - Minimum) / (Maximum - Minimum);
        return track.Left + (ratio * track.Width);
    }

    private Rect TrackRect() => new(TrackInset, (Bounds.Height - TrackHeight) * 0.5d, Math.Max(0d, Bounds.Width - (TrackInset * 2d)), TrackHeight);

    private long Coerce(long value) => Math.Clamp(value, Minimum, Maximum);
}
