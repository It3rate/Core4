using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;
using Core4.Numerics;

namespace Core4.AvaloniaProbe.Controls;

/// <summary>
/// A code-driven render surface for the first real Core4 demo page. The control owns only interaction and passes
/// real Core4 objects into the dominant-only renderer.
/// </summary>
public sealed class DominantOnlySceneControl : Control
{
    private const double HandleRadius = 18d;
    private const long MinimumOrigin = -16L;
    private const long MaximumOrigin = 16L;
    private const long MinimumSpan = 1L;
    private const long MaximumSpan = 16L;

    private DominantOnlyDemoState _state = DominantOnlyDemoState.Default;
    private DragSession _activeDrag;

    public DominantOnlySceneControl()
    {
        MinHeight = 420;
        MinWidth = 680;
        ClipToBounds = true;
    }

    public DominantOnlyDemoState State
    {
        get => _state;
        set
        {
            var normalized = NormalizeState(value);
            if (_state == normalized)
            {
                return;
            }

            _state = normalized;
            InvalidateVisual();
            StateChanged?.Invoke(this, _state);
        }
    }

    public event EventHandler<DominantOnlyDemoState>? StateChanged;

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        DominantOnlyRenderer.Render(context, Bounds, _state.Basis, _state.ValueSite);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var layout = DominantOnlyRenderer.Arrange(Bounds, _state.Basis, _state.ValueSite);
        var point = e.GetPosition(this);

        if (ScenePrimitives.Distance(point, layout.OriginPoint) <= HandleRadius)
        {
            BeginDrag(e, DragHandle.Origin, layout.MeasureAt(point.X) - _state.Origin);
            return;
        }

        if (ScenePrimitives.Distance(point, layout.UnitPoint) <= HandleRadius)
        {
            BeginDrag(e, DragHandle.Unit, layout.MeasureAt(point.X) - _state.UnitLocation);
            return;
        }

        if (ScenePrimitives.Distance(point, layout.ValuePoint) <= HandleRadius)
        {
            BeginDrag(e, DragHandle.Value, layout.MeasureAt(point.X) - _state.ValueLocation);
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_activeDrag.Handle == DragHandle.None || e.Pointer.Captured != this)
        {
            return;
        }

        var layout = DominantOnlyRenderer.Arrange(Bounds, _state.Basis, _state.ValueSite);
        var measure = layout.MeasureAt(e.GetPosition(this).X) - _activeDrag.PointerOffset;

        State = _activeDrag.Handle switch
        {
            DragHandle.Origin => _state.WithOrigin(Clamp(measure, MinimumOrigin, MaximumOrigin)),
            DragHandle.Unit => _state.WithDominantSpan(Clamp(measure - _state.Origin, MinimumSpan, MaximumSpan)),
            DragHandle.Value => _state.WithValueOffset(measure - _state.Origin),
            _ => _state
        };

        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _activeDrag = default;
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private void BeginDrag(PointerPressedEventArgs e, DragHandle handle, Measure offset)
    {
        _activeDrag = new DragSession(handle, offset);
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    private static Measure Clamp(Measure measure, long minimum, long maximum) =>
        new(Math.Clamp(measure.Raw, minimum, maximum));

    private static DominantOnlyDemoState NormalizeState(DominantOnlyDemoState state)
    {
        var origin = Clamp(state.Origin, MinimumOrigin, MaximumOrigin);
        var span = Clamp(state.DominantSpan.Absolute, MinimumSpan, MaximumSpan);
        return state with
        {
            Origin = origin,
            DominantSpan = span
        };
    }

    private readonly record struct DragSession(DragHandle Handle, Measure PointerOffset);

    private enum DragHandle
    {
        None,
        Origin,
        Unit,
        Value
    }
}
