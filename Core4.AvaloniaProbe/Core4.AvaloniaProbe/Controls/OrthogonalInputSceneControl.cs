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
/// Interactive render surface for the orthogonal-input page.
/// </summary>
public sealed class OrthogonalInputSceneControl : Control
{
    private const double HandleRadius = 18d;
    private const long MinimumOrigin = -16L;
    private const long MaximumOrigin = 16L;
    private const long MinimumSpan = 1L;
    private const long MaximumSpan = 16L;
    private const long MaximumOutput = 14L;
    private const long MaximumInput = 14L;

    private OrthogonalInputDemoState _state = OrthogonalInputDemoState.Default;
    private DragSession _activeDrag;

    public OrthogonalInputSceneControl()
    {
        MinHeight = 286;
        MinWidth = 680;
        ClipToBounds = true;
    }

    public OrthogonalInputDemoState State
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

    public event EventHandler<OrthogonalInputDemoState>? StateChanged;

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        OrthogonalInputRenderer.Render(context, Bounds, _state);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var layout = OrthogonalInputRenderer.Arrange(Bounds, _state);
        var point = e.GetPosition(this);

        if (ScenePrimitives.Distance(point, layout.SegmentLayout.OriginPoint) <= HandleRadius)
        {
            BeginDrag(e, DragHandle.Origin, layout.SegmentLayout.MeasureAt(point.X) - _state.Origin);
            return;
        }

        if (ScenePrimitives.Distance(point, layout.SegmentLayout.DominantUnitPoint) <= HandleRadius)
        {
            BeginDrag(e, DragHandle.Span, layout.SegmentLayout.MeasureAt(point.X) - _state.Segment.DominantUnitLocation);
            return;
        }

        if (ScenePrimitives.Distance(point, layout.SegmentLayout.DominantValuePoint) <= HandleRadius)
        {
            BeginDrag(e, DragHandle.DominantValue, layout.SegmentLayout.MeasureAt(point.X) - _state.Segment.DominantValueLocation);
            return;
        }

        if (ScenePrimitives.Distance(point, layout.SegmentLayout.RecessiveValuePoint) <= HandleRadius)
        {
            BeginDrag(e, DragHandle.RecessiveValue, layout.SegmentLayout.MeasureAt(point.X) - _state.Segment.RecessiveValueLocation);
            return;
        }

        if (layout.TopBranch.HasSignal && ScenePrimitives.Distance(point, layout.TopBranch.SignalTip) <= HandleRadius)
        {
            BeginDrag(e, DragHandle.TopOutput, MeasureAtVerticalTip(layout, point.Y) - _state.TopOutput);
            return;
        }

        if (layout.BottomBranch.HasSignal && ScenePrimitives.Distance(point, layout.BottomBranch.SignalTip) <= HandleRadius)
        {
            BeginDrag(e, DragHandle.BottomInput, MeasureAlong(layout.BottomBranch, point, layout.SegmentLayout.PixelsPerMeasure) - _state.BottomInput);
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_activeDrag.Handle == DragHandle.None || e.Pointer.Captured != this)
        {
            return;
        }

        var layout = OrthogonalInputRenderer.Arrange(Bounds, _state);
        var point = e.GetPosition(this);
        var horizontalMeasure = layout.SegmentLayout.MeasureAt(point.X) - _activeDrag.PointerOffset;

        State = _activeDrag.Handle switch
        {
            DragHandle.Origin => _state.WithOrigin(Clamp(horizontalMeasure, MinimumOrigin, MaximumOrigin)),
            DragHandle.Span => _state.WithSpan(Clamp(horizontalMeasure - _state.Origin, MinimumSpan, MaximumSpan)),
            DragHandle.DominantValue => _state.WithDominantValueOffset(horizontalMeasure - _state.Origin),
            DragHandle.RecessiveValue => _state.WithRecessiveValueOffset(_state.Origin - horizontalMeasure),
            DragHandle.TopOutput => _state.WithTopOutput(Clamp(MeasureAtVerticalTip(layout, point.Y) - _activeDrag.PointerOffset, 0L, MaximumOutput)),
            DragHandle.BottomInput => _state.WithBottomInput(Clamp(MeasureAlong(layout.BottomBranch, point, layout.SegmentLayout.PixelsPerMeasure) - _activeDrag.PointerOffset, 0L, MaximumInput)),
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

    private static Measure MeasureAtVerticalTip(OrthogonalInputLayout layout, double y)
    {
        var world = (layout.SegmentLayout.AxisY - y) / layout.SegmentLayout.PixelsPerMeasure;
        return new Measure((long)Math.Round(world, MidpointRounding.AwayFromZero));
    }

    private static Measure MeasureAlong(OrthogonalBranchLayout branch, Point point, double pixelsPerMeasure)
    {
        var delta = point - branch.Start;
        var along = (delta.X * branch.Direction.X) + (delta.Y * branch.Direction.Y);
        return new Measure((long)Math.Round(along / pixelsPerMeasure, MidpointRounding.AwayFromZero));
    }

    private static Measure Clamp(Measure measure, long minimum, long maximum) =>
        new(Math.Clamp(measure.Raw, minimum, maximum));

    private static OrthogonalInputDemoState NormalizeState(OrthogonalInputDemoState state)
    {
        var origin = Clamp(state.Origin, MinimumOrigin, MaximumOrigin);
        var span = Clamp(state.Span.Absolute, MinimumSpan, MaximumSpan);
        var topOutput = Clamp(state.TopOutput.Absolute, 0L, MaximumOutput);
        var bottomInput = Clamp(state.BottomInput.Absolute, 0L, MaximumInput);
        var topTension = Clamp(state.TopTension, -22L, 20L);
        var bottomTension = Clamp(state.BottomTension, -22L, 20L);
        return state with
        {
            Origin = origin,
            Span = span,
            TopOutput = topOutput,
            BottomInput = bottomInput,
            TopTension = topTension,
            BottomTension = bottomTension
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
        TopOutput,
        BottomInput
    }
}
