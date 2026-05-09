using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using Gojo.GearDemo.Adapters;
using Gojo.GearDemo.Components;
using Gojo.GearDemo.Model;
using Gojo.GearDemo.Rendering;

namespace Gojo.GearDemo;

public sealed class GearDemoCanvas : Control
{
    private readonly GearDemoModel model = new();
    private readonly GojoFrameClock frameClock = new();
    private GearDemoLayout layout;
    private bool isPlaying = true;
    private bool isAttached;
    private Point? hoverPoint;
    private Action<Point>? activeDrag;
    private bool isPublishingWeb;
    private string webButtonText = "web";
    private string? webStatusText;

    public GearDemoCanvas()
    {
        Focusable = true;
        ClipToBounds = true;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var bounds = new Rect(Bounds.Size);
        var inspection = model.Inspect();
        layout = GearDemoRenderer.Arrange(bounds, inspection);
        context.Custom(new GearDemoDrawOperation(
            bounds,
            layout,
            inspection,
            new GearDemoInteraction(
                isPlaying,
                hoverPoint,
                GearDemoHostServices.CanPublishWeb ? webButtonText : null,
                webStatusText)));
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
        frameClock.Reset();
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
            frameClock.Reset();
            CompletePointer(e);
            return;
        }

        if (GearDemoHostServices.CanPublishWeb && layout.WebButton.Contains(point))
        {
            _ = PublishWebAsync();
            CompletePointer(e);
            return;
        }

        if (layout.SpeedSlider.Bounds.Contains(point))
        {
            BeginDrag(e, p => model.SetSpeedSliderPosition(GearDemoAdapters.SliderValue(layout.SpeedSlider, p)));
            return;
        }

        if (layout.DriverTeethSlider.Bounds.Contains(point))
        {
            BeginDrag(e, p => model.SetDriverTeethSliderPosition(GearDemoAdapters.SliderValue(layout.DriverTeethSlider, p)));
            return;
        }

        if (layout.DrivenTeethSlider.Bounds.Contains(point))
        {
            BeginDrag(e, p => model.SetDrivenTeethSliderPosition(GearDemoAdapters.SliderValue(layout.DrivenTeethSlider, p)));
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
            frameClock.Reset();
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

        if (frameClock.Advance(now, isPlaying, model.AdvanceClock))
        {
            InvalidateVisual();
        }
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

    private async Task PublishWebAsync()
    {
        var publish = GearDemoHostServices.PublishWebAsync;
        if (isPublishingWeb || publish is null)
        {
            return;
        }

        isPublishingWeb = true;
        webButtonText = "web...";
        webStatusText = "publishing /gears";
        InvalidateVisual();

        try
        {
            await publish().ConfigureAwait(true);
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

    private void EnsureLayout()
    {
        if (layout.Bounds.Width <= 0d || layout.Bounds.Height <= 0d)
        {
            var bounds = new Rect(Bounds.Size);
            layout = GearDemoRenderer.Arrange(bounds, model.Inspect());
        }
    }

}
