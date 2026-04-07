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
/// Interactive render surface for the primitive-bases gallery. Only the directed-segment endpoints are draggable.
/// </summary>
public sealed class PrimitiveBasesSceneControl : Control
{
    private const double HandleRadius = 18d;
    private const long MinimumTicks = -24L;
    private const long MaximumTicks = 24L;
    private const double SegmentOriginX = 124d;
    private const double SelfOriginX = 140d;

    private PrimitiveBasesDemoState _state = PrimitiveBasesDemoState.Default;
    private DragSession _activeDrag;

    public PrimitiveBasesSceneControl()
    {
        MinHeight = 442;
        MinWidth = 760;
        ClipToBounds = true;
    }

    public PrimitiveBasesDemoState State
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

    public event EventHandler<PrimitiveBasesDemoState>? StateChanged;

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        PrimitiveBasesRenderer.Render(context, Bounds, _state);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var layout = PrimitiveBasesRenderer.Arrange(Bounds, _state);
        var point = e.GetPosition(this);

        if (ScenePrimitives.Distance(point, PrimitiveBasesRenderer.DominantHandle(layout)) <= HandleRadius)
        {
            BeginDrag(e, DragHandle.Dominant, DominantTicksAt(layout, point) - _state.DominantTicks);
            return;
        }

        if (ScenePrimitives.Distance(point, PrimitiveBasesRenderer.SelfDominantHandle(layout)) <= HandleRadius)
        {
            BeginDrag(e, DragHandle.SelfDominant, SelfTicksAt(layout, point) - _state.DominantTicks);
            return;
        }

        if (ScenePrimitives.Distance(point, PrimitiveBasesRenderer.RecessiveHandle(layout)) <= HandleRadius)
        {
            BeginDrag(e, DragHandle.Recessive, RecessiveTicksAt(layout, point) - _state.RecessiveTicks);
            return;
        }

        if (ScenePrimitives.Distance(point, PrimitiveBasesRenderer.SelfRecessiveHandle(layout)) <= HandleRadius)
        {
            BeginDrag(e, DragHandle.SelfRecessive, SelfTicksAt(layout, point) - _state.RecessiveTicks);
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_activeDrag.Handle == DragHandle.None || e.Pointer.Captured != this)
        {
            return;
        }

        var layout = PrimitiveBasesRenderer.Arrange(Bounds, _state);
        var point = e.GetPosition(this);

        State = _activeDrag.Handle switch
        {
            DragHandle.Dominant => _state.WithDominantTicks(Clamp(DominantTicksAt(layout, point) - _activeDrag.PointerOffset)),
            DragHandle.Recessive => _state.WithRecessiveTicks(Clamp(RecessiveTicksAt(layout, point) - _activeDrag.PointerOffset)),
            DragHandle.SelfDominant => _state.WithDominantTicks(Clamp(SelfTicksAt(layout, point) - _activeDrag.PointerOffset)),
            DragHandle.SelfRecessive => _state.WithRecessiveTicks(Clamp(SelfTicksAt(layout, point) - _activeDrag.PointerOffset)),
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

    private static Measure DominantTicksAt(PrimitiveBasesLayout layout, Point point)
    {
        var scene = layout.ToScene(point);
        var ticks = (scene.X - SegmentOriginX) / PrimitiveBasesLayout.TickPixels;
        return new Measure((long)Math.Round(ticks, MidpointRounding.AwayFromZero));
    }

    private static Measure RecessiveTicksAt(PrimitiveBasesLayout layout, Point point)
    {
        var scene = layout.ToScene(point);
        var ticks = (SegmentOriginX - scene.X) / PrimitiveBasesLayout.TickPixels;
        return new Measure((long)Math.Round(ticks, MidpointRounding.AwayFromZero));
    }

    private static Measure SelfTicksAt(PrimitiveBasesLayout layout, Point point)
    {
        var scene = layout.ToScene(point);
        var ticks = (scene.X - SelfOriginX) / PrimitiveBasesLayout.TickPixels;
        return new Measure((long)Math.Round(ticks, MidpointRounding.AwayFromZero));
    }

    private static Measure Clamp(Measure value) => new(Math.Clamp(value.Raw, MinimumTicks, MaximumTicks));

    private static PrimitiveBasesDemoState NormalizeState(PrimitiveBasesDemoState state) => state with
    {
        DominantTicks = Clamp(state.DominantTicks),
        RecessiveTicks = Clamp(state.RecessiveTicks)
    };

    private readonly record struct DragSession(DragHandle Handle, Measure PointerOffset);

    private enum DragHandle
    {
        None,
        Dominant,
        Recessive,
        SelfDominant,
        SelfRecessive
    }
}
