using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;

namespace Core4.AvaloniaProbe.Controls;

/// <summary>
/// Interactive scene for the pin-origin-relation glossary page.
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
        if (ScenePrimitives.Distance(point, layout.Start) <= HandleRadius)
        {
            BeginDrag(e, DragTarget.Start, point - layout.Start);
            return;
        }

        if (ScenePrimitives.Distance(point, layout.End) <= HandleRadius)
        {
            BeginDrag(e, DragTarget.End, point - layout.End);
            return;
        }

        if (ScenePrimitives.Distance(point, layout.Site) <= HandleRadius)
        {
            BeginDrag(e, DragTarget.Site, point - layout.Site);
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_activeDrag.Target == DragTarget.None || e.Pointer.Captured != this)
        {
            return;
        }

        var point = e.GetPosition(this) - _activeDrag.Offset;
        var layout = CurrentLayout;
        State = _activeDrag.Target switch
        {
            DragTarget.Start => _state.WithEndpoint(true, GlossaryRelationRenderer.Normalize(Bounds, point)),
            DragTarget.End => _state.WithEndpoint(false, GlossaryRelationRenderer.Normalize(Bounds, point)),
            DragTarget.Site => _state.WithSiteTicks(ProjectTicks(layout, point)),
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

    private void BeginDrag(PointerPressedEventArgs e, DragTarget target, Vector offset)
    {
        _activeDrag = new DragSession(target, offset);
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    private static long ProjectTicks(GlossaryRelationSceneLayout layout, Point point)
    {
        var direction = layout.End - layout.Start;
        var lengthSquared = (direction.X * direction.X) + (direction.Y * direction.Y);
        if (lengthSquared <= 0d)
        {
            return 0L;
        }

        var fromStart = point - layout.Start;
        var t = ((fromStart.X * direction.X) + (fromStart.Y * direction.Y)) / lengthSquared;
        return (long)Math.Round(Math.Clamp(t, 0d, 1d) * 100d, MidpointRounding.AwayFromZero);
    }

    private readonly record struct DragSession(DragTarget Target, Vector Offset)
    {
        public static DragSession Default => new(DragTarget.None, default);
    }

    private enum DragTarget
    {
        None,
        Start,
        End,
        Site
    }
}
