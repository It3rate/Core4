using System;
using System.Collections.Immutable;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Controls;

/// <summary>
/// Draggable topology scene for carrier chains. Hubs move visually as a unit, while all visible chains and strand
/// overlays continue to be rendered from the underlying Core4 topology.
/// </summary>
public sealed class CarrierChainSceneControl : Control
{
    private const double HandleRadius = 18d;

    private CarrierChainDemoState _state = CarrierChainDemoState.TJunction;
    private CarrierChainSceneOverlay _overlay = CarrierChainSceneOverlay.Empty;
    private ImmutableDictionary<Site, Point> _positions = ImmutableDictionary<Site, Point>.Empty;
    private DragSession _activeDrag;

    public event EventHandler? LayoutChanged;

    public CarrierChainSceneControl()
    {
        MinHeight = 360;
        MinWidth = 680;
        ClipToBounds = true;
        ResetLayout();
    }

    public CarrierChainDemoState State
    {
        get => _state;
        set
        {
            if (_state == value)
            {
                return;
            }

            _state = value;
            ResetLayout();
        }
    }

    public CarrierChainSceneLayout CurrentLayout => CarrierChainRenderer.Arrange(Bounds, _state, _positions);

    public CarrierChainSceneOverlay Overlay
    {
        get => _overlay;
        set
        {
            _overlay = value;
            InvalidateVisual();
        }
    }

    public void ResetLayout()
    {
        _positions = InitializePositions(_state);
        InvalidateVisual();
        LayoutChanged?.Invoke(this, EventArgs.Empty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        CarrierChainRenderer.Render(context, CurrentLayout, _overlay);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var layout = CarrierChainRenderer.Arrange(Bounds, _state, _positions);
        var point = e.GetPosition(this);

        foreach (var hub in layout.Hubs)
        {
            if (ScenePrimitives.Distance(point, hub.Point) > HandleRadius)
            {
                continue;
            }

            BeginDrag(e, hub.MemberSites, point - hub.Point);
            return;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_activeDrag.Sites.IsDefaultOrEmpty || e.Pointer.Captured != this)
        {
            return;
        }

        var point = e.GetPosition(this) - _activeDrag.PointerOffset;
        var normalized = CarrierChainRenderer.Normalize(Bounds, point);
        var builder = _positions.ToBuilder();

        foreach (var site in _activeDrag.Sites)
        {
            builder[site] = normalized;
        }

        _positions = builder.ToImmutable();
        InvalidateVisual();
        LayoutChanged?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _activeDrag = default;
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private void BeginDrag(PointerPressedEventArgs e, ImmutableArray<Site> sites, Vector pointerOffset)
    {
        _activeDrag = new DragSession(sites, pointerOffset);
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        LayoutChanged?.Invoke(this, EventArgs.Empty);
    }

    private static ImmutableDictionary<Site, Point> InitializePositions(CarrierChainDemoState state)
        => state.SeedPositions;

    private readonly record struct DragSession(ImmutableArray<Site> Sites, Vector PointerOffset);
}
