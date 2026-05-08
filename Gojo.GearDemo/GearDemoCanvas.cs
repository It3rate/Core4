using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using Gojo.GearDemo.Model;
using Gojo.GearDemo.Rendering;
#if !GOJO_GEAR_BROWSER
using Gojo.GearDemo.Services;
#endif

namespace Gojo.GearDemo;

public sealed class GearDemoCanvas : Control
{
    private readonly GearDemoModel model = new();
    private GearDemoLayout layout;
    private bool isPlaying = true;
    private bool isAttached;
    private TimeSpan? lastFrameTime;
    private double pendingFrameMilliseconds;
    private Point? hoverPoint;
    private Action<Point>? activeDrag;
#if !GOJO_GEAR_BROWSER
    private bool isPublishingWeb;
    private string webButtonText = "web";
    private string? webStatusText;
#endif

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
        layout = GearDemoRenderer.Arrange(bounds, snapshot);
        context.Custom(new GearDemoDrawOperation(
            bounds,
            layout,
            snapshot,
            new GearDemoInteraction(
                isPlaying,
                hoverPoint,
#if GOJO_GEAR_BROWSER
                null,
                null)));
#else
                webButtonText,
                webStatusText)));
#endif
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

        if (layout.ResetButton.Contains(point))
        {
            model.Reset();
            pendingFrameMilliseconds = 0d;
            CompletePointer(e);
            return;
        }

#if !GOJO_GEAR_BROWSER
        if (layout.WebButton.Contains(point))
        {
            _ = PublishWebAsync();
            CompletePointer(e);
            return;
        }
#endif

        if (layout.SpeedSlider.Bounds.Contains(point))
        {
            BeginDrag(e, p => model.SetSpeedTicksPerSecond((int)Math.Round(SliderValue(layout.SpeedSlider, p), MidpointRounding.AwayFromZero)));
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
            pendingFrameMilliseconds = 0d;
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
            if (isPlaying)
            {
                pendingFrameMilliseconds += seconds * 1000d;
                var wholeMilliseconds = (long)Math.Floor(pendingFrameMilliseconds);
                if (wholeMilliseconds > 0)
                {
                    model.AdvanceClock(wholeMilliseconds);
                    pendingFrameMilliseconds -= wholeMilliseconds;
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

#if !GOJO_GEAR_BROWSER
    private async Task PublishWebAsync()
    {
        if (isPublishingWeb)
        {
            return;
        }

        isPublishingWeb = true;
        webButtonText = "web...";
        webStatusText = "publishing /gears";
        InvalidateVisual();

        try
        {
            await GearDemoWebPublisher.PublishDeployAndOpenAsync().ConfigureAwait(true);
            webButtonText = "web";
            webStatusText = "deployed /gears";
        }
        catch (Exception exception)
        {
            (webButtonText, webStatusText) = WebFailure(exception);
        }
        finally
        {
            isPublishingWeb = false;
            InvalidateVisual();
        }
    }

    private static (string Button, string Status) WebFailure(Exception exception)
    {
        var message = exception.Message;
        if (message.Contains("SWA_CLI_DEPLOYMENT_TOKEN", StringComparison.OrdinalIgnoreCase))
        {
            return ("token", "local build ready; deploy token missing");
        }

        if (message.Contains("npx", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("Node.js", StringComparison.OrdinalIgnoreCase))
        {
            return ("node", "local build ready; node/npx missing");
        }

        return ("failed", TrimStatus(message));
    }

    private static string TrimStatus(string message)
    {
        const int maxLength = 52;
        var singleLine = message.ReplaceLineEndings(" ").Trim();
        return singleLine.Length <= maxLength
            ? singleLine
            : string.Concat(singleLine.AsSpan(0, maxLength - 3), "...");
    }
#endif

    private void EnsureLayout()
    {
        if (layout.Bounds.Width <= 0d || layout.Bounds.Height <= 0d)
        {
            var bounds = new Rect(Bounds.Size);
            layout = GearDemoRenderer.Arrange(bounds, model.Read());
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
