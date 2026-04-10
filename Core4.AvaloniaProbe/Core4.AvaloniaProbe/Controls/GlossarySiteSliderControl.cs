using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Core4.AvaloniaProbe.Rendering;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Controls;

/// <summary>
/// A glossary-specific site slider that renders the moving site glyph instead of a plain numeric thumb.
/// </summary>
public sealed class GlossarySiteSliderControl : Control
{
    private const double TrackInset = 11d;
    private const double TrackHeight = 4.4d;

    private long _minimum;
    private long _maximum = 12L;
    private long _value;
    private bool _dragging;

    public GlossarySiteSliderControl()
    {
        Height = 22d;
        MinWidth = 156d;
        ClipToBounds = true;
    }

    public Carrier Carrier { get; set; } = new(1);

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

            _maximum = Math.Max(_minimum, value);
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

    public event EventHandler<long>? ValueChanged;

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var track = TrackRect();
        var y = track.Center.Y;
        var valueX = XFor(Value);
        context.DrawLine(new Pen(new SolidColorBrush(Color.Parse("#d8d0c4")), TrackHeight, lineCap: PenLineCap.Round), new Point(track.Left, y), new Point(track.Right, y));
        context.DrawLine(new Pen(GlossaryClaimVisuals.TrackBrush(Carrier), TrackHeight, lineCap: PenLineCap.Round), new Point(track.Left, y), new Point(valueX, y));
        GlossaryClaimVisuals.DrawSiteGlyph(context, Carrier, new Point(valueX, y), Ratio(), 0.78d);
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

    private Rect TrackRect() => new(TrackInset, (Bounds.Height - TrackHeight) * 0.5d, Math.Max(0d, Bounds.Width - (TrackInset * 2d)), TrackHeight);

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

    private double Ratio() => Maximum <= Minimum ? 0d : (double)(Value - Minimum) / (Maximum - Minimum);

    private long Coerce(long value) => Math.Clamp(value, Minimum, Maximum);
}
