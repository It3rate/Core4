using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;

namespace Core4.AvaloniaProbe.Controls;

/// <summary>
/// Interactive scene for the first glossary page. Carrier endpoints are draggable while site and measure positions
/// remain ratios on those carriers.
/// </summary>
public sealed class GlossaryClaimSceneControl : Control
{
    private const double HandleRadius = 20d;

    private GlossaryClaimDemoState _state = GlossaryClaimDemoState.Default;
    private DragSession _activeDrag = DragSession.Default;

    public GlossaryClaimSceneControl()
    {
        MinHeight = 430d;
        MinWidth = 760d;
        ClipToBounds = true;
    }

    public GlossaryClaimDemoState State
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

    public event EventHandler<GlossaryClaimDemoState>? StateChanged;

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        GlossaryClaimRenderer.Render(context, Bounds, _state);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var layout = GlossaryClaimRenderer.Arrange(Bounds, _state);
        var point = e.GetPosition(this);
        foreach (var carrier in layout.Carriers)
        {
            if (ScenePrimitives.Distance(point, carrier.Start) <= HandleRadius)
            {
                BeginDrag(e, carrier.Index, true, point - carrier.Start);
                return;
            }

            if (ScenePrimitives.Distance(point, carrier.End) <= HandleRadius)
            {
                BeginDrag(e, carrier.Index, false, point - carrier.End);
                return;
            }
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_activeDrag.Index < 0 || e.Pointer.Captured != this)
        {
            return;
        }

        var point = e.GetPosition(this) - _activeDrag.Offset;
        var normalized = GlossaryClaimRenderer.Normalize(Bounds, point);
        State = _state.WithEndpoint(_activeDrag.Index, _activeDrag.UpdateStart, normalized);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _activeDrag = DragSession.Default;
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private void BeginDrag(PointerPressedEventArgs e, int index, bool updateStart, Vector offset)
    {
        _activeDrag = new DragSession(index, updateStart, offset);
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    private readonly record struct DragSession(int Index, bool UpdateStart, Vector Offset)
    {
        public static DragSession Default => new(-1, false, default);
    }
}
