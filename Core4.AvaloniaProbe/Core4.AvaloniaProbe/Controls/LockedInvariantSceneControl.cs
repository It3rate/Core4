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
/// Interactive scene for locked revealed invariants. Each panel is locally draggable and preserves its own frozen
/// compatible-motion rule.
/// </summary>
public sealed class LockedInvariantSceneControl : Control
{
    private const double HandleRadius = 20d;

    private LockedInvariantDemoState _state = LockedInvariantDemoState.Default;
    private DragSession _activeDrag;

    public LockedInvariantSceneControl()
    {
        MinHeight = 442;
        MinWidth = 760;
        ClipToBounds = true;
    }

    public LockedInvariantDemoState State
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

    public event EventHandler<LockedInvariantDemoState>? StateChanged;

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        LockedInvariantRenderer.Render(context, Bounds, _state);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var layout = LockedInvariantRenderer.Arrange(Bounds, _state);
        var point = e.GetPosition(this);

        if (TryBeginDrag(e, point, layout, LockedHandle.SelfPrimary, LockedInvariantRenderer.SelfPrimaryHandle(layout), SelfPrimaryTicksAt(layout, point) - _state.Self.PrimaryTicks))
        {
            return;
        }

        if (TryBeginDrag(e, point, layout, LockedHandle.SelfSecondary, LockedInvariantRenderer.SelfSecondaryHandle(layout), SelfSecondaryTicksAt(layout, point) - _state.Self.SecondaryTicks))
        {
            return;
        }

        if (TryBeginDrag(e, point, layout, LockedHandle.SegmentRecessive, LockedInvariantRenderer.SegmentRecessiveHandle(layout), SegmentRecessiveTicksAt(layout, point) - _state.Segment.PrimaryTicks))
        {
            return;
        }

        if (TryBeginDrag(e, point, layout, LockedHandle.SegmentDominant, LockedInvariantRenderer.SegmentDominantHandle(layout), SegmentDominantTicksAt(layout, point) - _state.Segment.SecondaryTicks))
        {
            return;
        }

        if (TryBeginDrag(e, point, layout, LockedHandle.AreaHorizontal, LockedInvariantRenderer.AreaHorizontalHandle(layout), AreaHorizontalTicksAt(layout, point) - _state.Area.PrimaryTicks))
        {
            return;
        }

        if (TryBeginDrag(e, point, layout, LockedHandle.AreaVertical, LockedInvariantRenderer.AreaVerticalHandle(layout), AreaVerticalTicksAt(layout, point) - _state.Area.SecondaryTicks))
        {
            return;
        }

        if (TryBeginDrag(e, point, layout, LockedHandle.AntiAreaHorizontal, LockedInvariantRenderer.AntiAreaHorizontalHandle(layout), AntiAreaHorizontalTicksAt(layout, point) - _state.AntiArea.PrimaryTicks))
        {
            return;
        }

        if (TryBeginDrag(e, point, layout, LockedHandle.AntiAreaVertical, LockedInvariantRenderer.AntiAreaVerticalHandle(layout), AntiAreaVerticalTicksAt(layout, point) - _state.AntiArea.SecondaryTicks))
        {
            return;
        }

        if (TryBeginDrag(e, point, layout, LockedHandle.ComplexHorizontal, LockedInvariantRenderer.ComplexHorizontalHandle(layout), ComplexHorizontalTicksAt(layout, point) - _state.Complex.PrimaryTicks))
        {
            return;
        }

        if (TryBeginDrag(e, point, layout, LockedHandle.ComplexVertical, LockedInvariantRenderer.ComplexVerticalHandle(layout), ComplexVerticalTicksAt(layout, point) - _state.Complex.SecondaryTicks))
        {
            return;
        }

        if (TryBeginDrag(e, point, layout, LockedHandle.AntiComplexHorizontal, LockedInvariantRenderer.AntiComplexHorizontalHandle(layout), AntiComplexHorizontalTicksAt(layout, point) - _state.AntiComplex.PrimaryTicks))
        {
            return;
        }

        TryBeginDrag(e, point, layout, LockedHandle.AntiComplexVertical, LockedInvariantRenderer.AntiComplexVerticalHandle(layout), AntiComplexVerticalTicksAt(layout, point) - _state.AntiComplex.SecondaryTicks);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_activeDrag.Handle == LockedHandle.None || e.Pointer.Captured != this)
        {
            return;
        }

        var layout = LockedInvariantRenderer.Arrange(Bounds, _state);
        var point = e.GetPosition(this);

        State = _activeDrag.Handle switch
        {
            LockedHandle.SelfPrimary => _state.WithSelfPrimaryTicks(SelfPrimaryTicksAt(layout, point) - _activeDrag.PointerOffset),
            LockedHandle.SelfSecondary => _state.WithSelfSecondaryTicks(SelfSecondaryTicksAt(layout, point) - _activeDrag.PointerOffset),
            LockedHandle.SegmentRecessive => _state.WithSegmentRecessiveTicks(SegmentRecessiveTicksAt(layout, point) - _activeDrag.PointerOffset),
            LockedHandle.SegmentDominant => _state.WithSegmentDominantTicks(SegmentDominantTicksAt(layout, point) - _activeDrag.PointerOffset),
            LockedHandle.AreaHorizontal => _state.WithAreaHorizontalTicks(AreaHorizontalTicksAt(layout, point) - _activeDrag.PointerOffset),
            LockedHandle.AreaVertical => _state.WithAreaVerticalTicks(AreaVerticalTicksAt(layout, point) - _activeDrag.PointerOffset),
            LockedHandle.AntiAreaHorizontal => _state.WithAntiAreaHorizontalTicks(AntiAreaHorizontalTicksAt(layout, point) - _activeDrag.PointerOffset),
            LockedHandle.AntiAreaVertical => _state.WithAntiAreaVerticalTicks(AntiAreaVerticalTicksAt(layout, point) - _activeDrag.PointerOffset),
            LockedHandle.ComplexHorizontal => _state.WithComplexHorizontalTicks(ComplexHorizontalTicksAt(layout, point) - _activeDrag.PointerOffset),
            LockedHandle.ComplexVertical => _state.WithComplexVerticalTicks(ComplexVerticalTicksAt(layout, point) - _activeDrag.PointerOffset),
            LockedHandle.AntiComplexHorizontal => _state.WithAntiComplexHorizontalTicks(AntiComplexHorizontalTicksAt(layout, point) - _activeDrag.PointerOffset),
            LockedHandle.AntiComplexVertical => _state.WithAntiComplexVerticalTicks(AntiComplexVerticalTicksAt(layout, point) - _activeDrag.PointerOffset),
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

    private bool TryBeginDrag(PointerPressedEventArgs e, Point point, LockedInvariantLayout layout, LockedHandle handle, Point handlePoint, Measure offset)
    {
        if (ScenePrimitives.Distance(point, handlePoint) > HandleRadius)
        {
            return false;
        }

        _activeDrag = new DragSession(handle, offset);
        e.Pointer.Capture(this);
        e.Handled = true;
        return true;
    }

    private static Measure SelfPrimaryTicksAt(LockedInvariantLayout layout, Point point)
    {
        var scene = layout.ToScene(point);
        return LockedInvariantDemoState.Ticks((scene.X - 138d) / LockedInvariantLayout.UnitPixels);
    }

    private static Measure SelfSecondaryTicksAt(LockedInvariantLayout layout, Point point)
    {
        var scene = layout.ToScene(point);
        return LockedInvariantDemoState.Ticks((scene.X - 138d) / LockedInvariantLayout.UnitPixels);
    }

    private static Measure SegmentRecessiveTicksAt(LockedInvariantLayout layout, Point point)
    {
        var scene = layout.ToScene(point);
        return LockedInvariantDemoState.Ticks((124d - scene.X) / LockedInvariantLayout.UnitPixels);
    }

    private static Measure SegmentDominantTicksAt(LockedInvariantLayout layout, Point point)
    {
        var scene = layout.ToScene(point);
        return LockedInvariantDemoState.Ticks((scene.X - 124d) / LockedInvariantLayout.UnitPixels);
    }

    private static Measure AreaHorizontalTicksAt(LockedInvariantLayout layout, Point point)
    {
        var scene = layout.ToScene(point);
        return LockedInvariantDemoState.Ticks((scene.X - 734d) / LockedInvariantLayout.UnitPixels);
    }

    private static Measure AreaVerticalTicksAt(LockedInvariantLayout layout, Point point)
    {
        var scene = layout.ToScene(point);
        return LockedInvariantDemoState.Ticks((96d - scene.Y) / LockedInvariantLayout.UnitPixels);
    }

    private static Measure AntiAreaHorizontalTicksAt(LockedInvariantLayout layout, Point point)
    {
        var scene = layout.ToScene(point);
        return LockedInvariantDemoState.Ticks((516d - scene.X) / LockedInvariantLayout.UnitPixels);
    }

    private static Measure AntiAreaVerticalTicksAt(LockedInvariantLayout layout, Point point)
    {
        var scene = layout.ToScene(point);
        return LockedInvariantDemoState.Ticks((scene.Y - 244d) / LockedInvariantLayout.UnitPixels);
    }

    private static Measure ComplexHorizontalTicksAt(LockedInvariantLayout layout, Point point)
    {
        var scene = layout.ToScene(point);
        return LockedInvariantDemoState.Ticks((scene.X - 734d) / LockedInvariantLayout.UnitPixels);
    }

    private static Measure ComplexVerticalTicksAt(LockedInvariantLayout layout, Point point)
    {
        var scene = layout.ToScene(point);
        return LockedInvariantDemoState.Ticks((scene.Y - 244d) / LockedInvariantLayout.UnitPixels);
    }

    private static Measure AntiComplexHorizontalTicksAt(LockedInvariantLayout layout, Point point)
    {
        var scene = layout.ToScene(point);
        return LockedInvariantDemoState.Ticks((516d - scene.X) / LockedInvariantLayout.UnitPixels);
    }

    private static Measure AntiComplexVerticalTicksAt(LockedInvariantLayout layout, Point point)
    {
        var scene = layout.ToScene(point);
        return LockedInvariantDemoState.Ticks((96d - scene.Y) / LockedInvariantLayout.UnitPixels);
    }

    private readonly record struct DragSession(LockedHandle Handle, Measure PointerOffset);

    private enum LockedHandle
    {
        None,
        SelfPrimary,
        SelfSecondary,
        SegmentRecessive,
        SegmentDominant,
        AreaHorizontal,
        AreaVertical,
        AntiAreaHorizontal,
        AntiAreaVertical,
        ComplexHorizontal,
        ComplexVertical,
        AntiComplexHorizontal,
        AntiComplexVertical
    }
}
