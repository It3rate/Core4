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
/// Interactive render surface for the orthogonal-output page.
/// </summary>
public sealed class OrthogonalOutputSceneControl : Control
{
    private const double HandleRadius = 18d;
    private const long MinimumOrigin = -16L;
    private const long MaximumOrigin = 16L;
    private const long MinimumSpan = 1L;
    private const long MaximumSpan = 16L;
    private const long MaximumOutput = 14L;

    private OrthogonalOutputDemoState _state = OrthogonalOutputDemoState.Default;
    private DragSession _activeDrag;

    public OrthogonalOutputSceneControl()
    {
        MinHeight = 286;
        MinWidth = 680;
        ClipToBounds = true;
    }

    public OrthogonalOutputDemoState State
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

    public event EventHandler<OrthogonalOutputDemoState>? StateChanged;

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        OrthogonalOutputRenderer.Render(context, Bounds, _state);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var layout = OrthogonalOutputRenderer.Arrange(Bounds, _state);
        var point = e.GetPosition(this);

        if (ScenePrimitives.Distance(point, layout.OriginPoint) <= HandleRadius)
        {
            BeginDrag(e, DragHandle.Origin, layout.SegmentLayout.MeasureAt(point.X) - _state.Origin);
            return;
        }

        if (ScenePrimitives.Distance(point, layout.SegmentLayout.DominantUnitPoint) <= HandleRadius)
        {
            BeginDrag(e, DragHandle.Span, layout.SegmentLayout.MeasureAt(point.X) - _state.DominantUnitLocation);
            return;
        }

        if (ScenePrimitives.Distance(point, layout.SegmentLayout.DominantValuePoint) <= HandleRadius)
        {
            BeginDrag(e, DragHandle.DominantValue, layout.SegmentLayout.MeasureAt(point.X) - _state.DominantValueLocation);
            return;
        }

        if (ScenePrimitives.Distance(point, layout.SegmentLayout.RecessiveValuePoint) <= HandleRadius)
        {
            BeginDrag(e, DragHandle.RecessiveValue, layout.SegmentLayout.MeasureAt(point.X) - _state.RecessiveValueLocation);
            return;
        }

        if (layout.HasOutput && ScenePrimitives.Distance(point, layout.OutputTip) <= HandleRadius)
        {
            BeginDrag(e, DragHandle.TopOutput, layout.OutputAt(point.Y) - _state.TopOutput);
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_activeDrag.Handle == DragHandle.None || e.Pointer.Captured != this)
        {
            return;
        }

        var layout = OrthogonalOutputRenderer.Arrange(Bounds, _state);
        var point = e.GetPosition(this);
        var horizontalMeasure = layout.SegmentLayout.MeasureAt(point.X) - _activeDrag.PointerOffset;
        var verticalMeasure = layout.OutputAt(point.Y) - _activeDrag.PointerOffset;

        State = _activeDrag.Handle switch
        {
            DragHandle.Origin => _state.WithOrigin(Clamp(horizontalMeasure, MinimumOrigin, MaximumOrigin)),
            DragHandle.Span => _state.WithSpan(Clamp(horizontalMeasure - _state.Origin, MinimumSpan, MaximumSpan)),
            DragHandle.DominantValue => _state.WithDominantValueOffset(horizontalMeasure - _state.Origin),
            DragHandle.RecessiveValue => _state.WithRecessiveValueOffset(_state.Origin - horizontalMeasure),
            DragHandle.TopOutput => _state.WithTopOutput(Clamp(verticalMeasure, 0L, MaximumOutput)),
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

    private static OrthogonalOutputDemoState NormalizeState(OrthogonalOutputDemoState state)
    {
        var origin = Clamp(state.Origin, MinimumOrigin, MaximumOrigin);
        var span = Clamp(state.Span.Absolute, MinimumSpan, MaximumSpan);
        var output = Clamp(state.TopOutput.Absolute, 0L, MaximumOutput);
        var tension = Clamp(state.TopTension, -22L, 20L);
        return state with
        {
            Origin = origin,
            Span = span,
            TopOutput = output,
            TopTension = tension
        };
    }

    private readonly record struct DragSession(DragHandle Handle, Measure PointerOffset);

    private enum DragHandle
    {
        None,
        Origin,
        Span,
        DominantValue,
        RecessiveValue,
        TopOutput
    }
}
