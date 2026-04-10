using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;

namespace Core4.AvaloniaProbe.Controls;

/// <summary>
/// Interactive scene for the pin-origin-relation glossary page across multiple carriers.
/// </summary>
public sealed class GlossaryRelationSceneControl : Control
{
    private const double HandleRadius = 20d;

    private GlossaryRelationDemoState _state = GlossaryRelationDemoState.CreateDefault();
    private DragSession _activeDrag = DragSession.Default;

    public event EventHandler? LayoutChanged;

    public GlossaryRelationSceneControl()
    {
        MinHeight = 430d;
        MinWidth = 760d;
        ClipToBounds = true;
    }

    public GlossaryRelationDemoState State
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
            LayoutChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler<GlossaryRelationDemoState>? StateChanged;

    internal GlossaryRelationSceneLayout CurrentLayout => GlossaryRelationRenderer.Arrange(Bounds, _state);

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        GlossaryRelationRenderer.Render(context, Bounds, _state);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var layout = CurrentLayout;
        var point = e.GetPosition(this);
        foreach (var carrier in layout.Carriers)
        {
            if (ScenePrimitives.Distance(point, carrier.Start) <= HandleRadius)
            {
                BeginDrag(e, carrier.Index, DragTarget.Start, point - carrier.Start);
                return;
            }

            if (ScenePrimitives.Distance(point, carrier.End) <= HandleRadius)
            {
                BeginDrag(e, carrier.Index, DragTarget.End, point - carrier.End);
                return;
            }

            if (ScenePrimitives.Distance(point, carrier.Site) <= HandleRadius)
            {
                BeginDrag(e, carrier.Index, DragTarget.Site, point - carrier.Site);
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
        var layout = CurrentLayout;
        var carrier = layout.Carriers[_activeDrag.Index];
        State = _activeDrag.Target switch
        {
            DragTarget.Start => _state.WithEndpoint(_activeDrag.Index, true, GlossaryRelationRenderer.Normalize(Bounds, point)),
            DragTarget.End => _state.WithEndpoint(_activeDrag.Index, false, GlossaryRelationRenderer.Normalize(Bounds, point)),
            DragTarget.Site => _state.WithSiteTicks(_activeDrag.Index, ProjectTicks(carrier, point)),
            _ => _state
        };
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _activeDrag = DragSession.Default;
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        LayoutChanged?.Invoke(this, EventArgs.Empty);
    }

    private void BeginDrag(PointerPressedEventArgs e, int index, DragTarget target, Vector offset)
    {
        _activeDrag = new DragSession(index, target, offset);
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    private static long ProjectTicks(GlossaryRelationCarrierLayout carrier, Point point)
    {
        var direction = carrier.End - carrier.Start;
        var lengthSquared = (direction.X * direction.X) + (direction.Y * direction.Y);
        if (lengthSquared <= 0d)
        {
            return 0L;
        }

        var fromStart = point - carrier.Start;
        var t = ((fromStart.X * direction.X) + (fromStart.Y * direction.Y)) / lengthSquared;
        return (long)Math.Round(Math.Clamp(t, 0d, 1d) * 100d, MidpointRounding.AwayFromZero);
    }

    private readonly record struct DragSession(int Index, DragTarget Target, Vector Offset)
    {
        public static DragSession Default => new(-1, DragTarget.None, default);
    }

    private enum DragTarget
    {
        None,
        Start,
        End,
        Site
    }
}
