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
/// Interactive render surface for the unfused inverted pair page.
/// </summary>
public sealed class InvertedPairSceneControl : Control
{
    private const double HandleRadius = 18d;
    private const long MinimumOrigin = -16L;
    private const long MaximumOrigin = 16L;
    private const long MinimumSpan = 1L;
    private const long MaximumSpan = 16L;

    private InvertedPairDemoState _state = InvertedPairDemoState.Default;
    private DragSession _activeDrag;

    public InvertedPairSceneControl()
    {
        MinHeight = 286;
        MinWidth = 680;
        ClipToBounds = true;
    }

    public InvertedPairDemoState State
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

    public event EventHandler<InvertedPairDemoState>? StateChanged;

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        InvertedPairRenderer.Render(context, Bounds, _state);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var layout = InvertedPairRenderer.Arrange(Bounds, _state);
        var point = e.GetPosition(this);

        if (ScenePrimitives.Distance(point, layout.OriginPoint) <= HandleRadius)
        {
            BeginDrag(e, DragHandle.Origin, layout.MeasureAt(point.X) - _state.Origin);
            return;
        }

        if (ScenePrimitives.Distance(point, layout.DominantUnitPoint) <= HandleRadius)
        {
            BeginDrag(e, DragHandle.DominantUnit, layout.MeasureAt(point.X) - _state.DominantUnitLocation);
            return;
        }

        if (ScenePrimitives.Distance(point, layout.RecessiveUnitPoint) <= HandleRadius)
        {
            BeginDrag(e, DragHandle.RecessiveUnit, layout.MeasureAt(point.X) - _state.RecessiveUnitLocation);
            return;
        }

        if (ScenePrimitives.Distance(point, layout.DominantValuePoint) <= HandleRadius)
        {
            BeginDrag(e, DragHandle.DominantValue, layout.MeasureAt(point.X) - _state.DominantValueLocation);
            return;
        }

        if (ScenePrimitives.Distance(point, layout.RecessiveValuePoint) <= HandleRadius)
        {
            BeginDrag(e, DragHandle.RecessiveValue, layout.MeasureAt(point.X) - _state.RecessiveValueLocation);
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_activeDrag.Handle == DragHandle.None || e.Pointer.Captured != this)
        {
            return;
        }

        var layout = InvertedPairRenderer.Arrange(Bounds, _state);
        var measure = layout.MeasureAt(e.GetPosition(this).X) - _activeDrag.PointerOffset;

        State = _activeDrag.Handle switch
        {
            DragHandle.Origin => _state.WithOrigin(Clamp(measure, MinimumOrigin, MaximumOrigin)),
            DragHandle.DominantUnit => _state.WithDominantSpan(Clamp(measure - _state.Origin, MinimumSpan, MaximumSpan)),
            DragHandle.RecessiveUnit => _state.WithRecessiveSpan(Clamp(_state.Origin - measure, MinimumSpan, MaximumSpan)),
            DragHandle.DominantValue => _state.WithDominantValueOffset(measure - _state.Origin),
            DragHandle.RecessiveValue => _state.WithRecessiveValueOffset(_state.Origin - measure),
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

    private static InvertedPairDemoState NormalizeState(InvertedPairDemoState state)
    {
        var origin = Clamp(state.Origin, MinimumOrigin, MaximumOrigin);
        var dominantSpan = Clamp(state.DominantSpan.Absolute, MinimumSpan, MaximumSpan);
        var recessiveSpan = Clamp(state.RecessiveSpan.Absolute, MinimumSpan, MaximumSpan);
        return state with
        {
            Origin = origin,
            DominantSpan = dominantSpan,
            RecessiveSpan = recessiveSpan
        };
    }

    private readonly record struct DragSession(DragHandle Handle, Measure PointerOffset);

    private enum DragHandle
    {
        None,
        Origin,
        DominantUnit,
        RecessiveUnit,
        DominantValue,
        RecessiveValue
    }
}
