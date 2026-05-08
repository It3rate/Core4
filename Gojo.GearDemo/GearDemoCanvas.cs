using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using Gojo.GearDemo.Model;
using Gojo.GearDemo.Rendering;

namespace Gojo.GearDemo;

public sealed class GearDemoCanvas : Control
{
    private readonly GearDemoModel model = new();
    private GearDemoLayout layout;
    private bool isPlaying = true;
    private bool isAttached;
    private TimeSpan? lastFrameTime;
    private double speedTicksPerSecond = 64d;
    private double pendingDriveTicks;
    private Point? hoverPoint;
    private Action<Point>? activeDrag;

    public GearDemoCanvas()
    {
        Focusable = true;
        ClipToBounds = true;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var bounds = new Rect(Bounds.Size);
        var snapshot = model.Read();
        layout = GearDemoRenderer.Arrange(bounds, snapshot, speedTicksPerSecond);
        context.Custom(new GearDemoDrawOperation(
            bounds,
            layout,
            snapshot,
            new GearDemoInteraction(isPlaying, hoverPoint)));
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        isAttached = true;
        RequestFrame();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        isAttached = false;
        lastFrameTime = null;
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        var point = e.GetPosition(this);
        hoverPoint = point;

        if (activeDrag is not null)
        {
            activeDrag(point);
            e.Handled = true;
        }

        InvalidateVisual();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        hoverPoint = null;
        InvalidateVisual();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();
        var point = e.GetPosition(this);
        hoverPoint = point;
        EnsureLayout();

        if (layout.PlayButton.Contains(point))
        {
            isPlaying = !isPlaying;
            CompletePointer(e);
            return;
        }

        if (layout.StepButton.Contains(point))
        {
            model.Drive(1);
            CompletePointer(e);
            return;
        }

        if (layout.BurstButton.Contains(point))
        {
            model.Drive(GearDemoModel.TicksPerTooth);
            CompletePointer(e);
            return;
        }

        if (layout.ResetButton.Contains(point))
        {
            model.Reset();
            pendingDriveTicks = 0d;
            CompletePointer(e);
            return;
        }

        if (layout.SpeedSlider.Bounds.Contains(point))
        {
            BeginDrag(e, p => speedTicksPerSecond = SliderValue(layout.SpeedSlider, p));
            return;
        }

        if (layout.DriverTeethSlider.Bounds.Contains(point))
        {
            BeginDrag(e, p => model.SetDriverTeeth((int)Math.Round(SliderValue(layout.DriverTeethSlider, p), MidpointRounding.AwayFromZero)));
            return;
        }

        if (layout.DrivenTeethSlider.Bounds.Contains(point))
        {
            BeginDrag(e, p => model.SetDrivenTeeth((int)Math.Round(SliderValue(layout.DrivenTeethSlider, p), MidpointRounding.AwayFromZero)));
            return;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        activeDrag = null;
        e.Pointer.Capture(null);
        e.Handled = true;
        InvalidateVisual();
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        var ticks = Math.Max(1, (long)Math.Round(Math.Abs(e.Delta.Y), MidpointRounding.AwayFromZero));
        model.Drive(ticks);
        e.Handled = true;
        InvalidateVisual();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Space)
        {
            isPlaying = !isPlaying;
            e.Handled = true;
        }
        else if (e.Key == Key.Right)
        {
            model.Drive(1);
            e.Handled = true;
        }
        else if (e.Key == Key.R)
        {
            model.Reset();
            pendingDriveTicks = 0d;
            e.Handled = true;
        }

        if (e.Handled)
        {
            InvalidateVisual();
        }
    }

    private void RequestFrame()
    {
        if (!isAttached)
        {
            return;
        }

        TopLevel.GetTopLevel(this)?.RequestAnimationFrame(OnAnimationFrame);
    }

    private void OnAnimationFrame(TimeSpan now)
    {
        if (!isAttached)
        {
            return;
        }

        if (lastFrameTime is { } last)
        {
            var seconds = Math.Clamp((now - last).TotalSeconds, 0d, 0.1d);
            if (isPlaying && speedTicksPerSecond > 0d)
            {
                pendingDriveTicks += speedTicksPerSecond * seconds;
                var wholeTicks = (long)Math.Floor(pendingDriveTicks);
                if (wholeTicks > 0)
                {
                    model.Drive(wholeTicks);
                    pendingDriveTicks -= wholeTicks;
                    InvalidateVisual();
                }
            }
        }

        lastFrameTime = now;
        RequestFrame();
    }

    private void BeginDrag(PointerPressedEventArgs e, Action<Point> drag)
    {
        activeDrag = point =>
        {
            drag(point);
            InvalidateVisual();
        };
        e.Pointer.Capture(this);
        activeDrag(e.GetPosition(this));
        e.Handled = true;
    }

    private void CompletePointer(PointerPressedEventArgs e)
    {
        e.Handled = true;
        InvalidateVisual();
    }

    private void EnsureLayout()
    {
        if (layout.Bounds.Width <= 0d || layout.Bounds.Height <= 0d)
        {
            var bounds = new Rect(Bounds.Size);
            layout = GearDemoRenderer.Arrange(bounds, model.Read(), speedTicksPerSecond);
        }
    }

    private static double SliderValue(SliderLayout slider, Point point)
    {
        var t = slider.Track.Width <= 0d
            ? 0d
            : (point.X - slider.Track.X) / slider.Track.Width;
        return slider.Minimum + (Math.Clamp(t, 0d, 1d) * (slider.Maximum - slider.Minimum));
    }
}
