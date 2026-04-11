using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;

namespace Core4.AvaloniaProbe.Controls;

/// <summary>
/// Interactive scene for tension / fusion / orthogonal interpretation. The unit handle is dragged left/right on the
/// rail, and clicking a value detaches it into the appropriate orientation bucket.
/// </summary>
public sealed class GlossaryTensionSceneControl : Control
{
    private const double HitRadius = 14d;

    private GlossaryTensionDemoState _state = GlossaryTensionDemoState.CreateDefault();
    private bool _draggingHandle;

    public GlossaryTensionSceneControl()
    {
        MinHeight = 420d;
        MinWidth = 760d;
        ClipToBounds = true;
    }

    public GlossaryTensionDemoState State
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

    public event EventHandler<GlossaryTensionDemoState>? StateChanged;

    internal GlossaryTensionSceneLayout CurrentLayout => GlossaryTensionRenderer.Arrange(Bounds, _state);

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        GlossaryTensionRenderer.Render(context, Bounds, _state);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var point = e.GetPosition(this);
        var layout = CurrentLayout;
        if (ScenePrimitives.Distance(point, layout.Handle) <= HitRadius)
        {
            _draggingHandle = true;
            e.Pointer.Capture(this);
            e.Handled = true;
            return;
        }

        foreach (var value in layout.Values)
        {
            if (ScenePrimitives.Distance(point, value.Center) > HitRadius)
            {
                continue;
            }

            State = _state.Detach(value.Id);
            e.Handled = true;
            return;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (!_draggingHandle || e.Pointer.Captured != this)
        {
            return;
        }

        var point = e.GetPosition(this);
        var layout = CurrentLayout;
        var rawTicks = (long)Math.Round((point.X - layout.Origin.X) / layout.PixelsPerCarrierTick, MidpointRounding.AwayFromZero);
        State = _state.WithUnitCarrierTicks(rawTicks);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _draggingHandle = false;
        e.Pointer.Capture(null);
        e.Handled = true;
    }
}
