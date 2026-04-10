using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;

namespace Core4.AvaloniaProbe.Controls;

/// <summary>
/// Interactive scene for the basis glossary page. The custom heat line can pin an origin, choose a relation, and
/// later drag those markers while the derived scales stay synchronized.
/// </summary>
public sealed class GlossaryBasisSceneControl : Control
{
    private const double HandleRadius = 18d;

    private GlossaryBasisDemoState _state = GlossaryBasisDemoState.CreateDefault();
    private DragSession _activeDrag = DragSession.Default;

    public event EventHandler? LayoutChanged;

    public GlossaryBasisSceneControl()
    {
        MinHeight = 430d;
        MinWidth = 760d;
        ClipToBounds = true;
    }

    public GlossaryBasisDemoState State
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

    public event EventHandler<GlossaryBasisDemoState>? StateChanged;

    internal GlossaryBasisSceneLayout CurrentLayout => GlossaryBasisRenderer.Arrange(Bounds, _state);

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        GlossaryBasisRenderer.Render(context, Bounds, _state);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var point = e.GetPosition(this);
        var layout = CurrentLayout;
        if (_state.IsFrozen)
        {
            var customRead = ReadPoint(layout.Custom);
            if (ScenePrimitives.Distance(point, customRead) <= HandleRadius)
            {
                BeginDrag(e, DragTarget.Read, point - customRead);
                return;
            }

            var celsiusRead = ReadPoint(layout.Celsius);
            if (ScenePrimitives.Distance(point, celsiusRead) <= HandleRadius)
            {
                BeginDrag(e, DragTarget.Read, point - celsiusRead);
                return;
            }

            var fahrenheitRead = ReadPoint(layout.Fahrenheit);
            if (ScenePrimitives.Distance(point, fahrenheitRead) <= HandleRadius)
            {
                BeginDrag(e, DragTarget.Read, point - fahrenheitRead);
                return;
            }
        }

        if (_state.IsFrozen && ScenePrimitives.Distance(point, RelationPoint(layout.Custom)) <= HandleRadius)
        {
            BeginDrag(e, DragTarget.Relation, point - RelationPoint(layout.Custom));
            return;
        }

        if (_state.IsFrozen && ScenePrimitives.Distance(point, OriginPoint(layout.Custom)) <= HandleRadius)
        {
            BeginDrag(e, DragTarget.Origin, point - OriginPoint(layout.Custom));
            return;
        }

        if (!_state.HasRelation && ScenePrimitives.Distance(point, SitePoint(layout.Custom)) <= HandleRadius)
        {
            BeginDrag(e, DragTarget.Site, point - SitePoint(layout.Custom));
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
        var ticks = ProjectTicks(CurrentLayout.Custom, point);
        State = _activeDrag.Target switch
        {
            DragTarget.Site => _state.WithSetupTicks(ticks),
            DragTarget.Origin => _state.WithOriginTicks(ticks),
            DragTarget.Relation => _state.WithRelationTicks(ticks),
            DragTarget.Read => _state.WithReadTicks(ticks),
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

    private static long ProjectTicks(GlossaryTemperatureScaleLayout scale, Point point)
    {
        var width = scale.End.X - scale.Start.X;
        if (Math.Abs(width) <= 0.001d)
        {
            return 0L;
        }

        var t = (point.X - scale.Start.X) / width;
        return (long)Math.Round(Math.Clamp(t, 0d, 1d) * GlossaryBasisDemoState.TotalTicks, MidpointRounding.AwayFromZero);
    }

    private Point SitePoint(GlossaryTemperatureScaleLayout scale)
        => PointAtTicks(scale, _state.SiteTicks.Raw);

    private Point OriginPoint(GlossaryTemperatureScaleLayout scale)
        => PointAtTicks(scale, _state.OriginTicks.Raw);

    private Point RelationPoint(GlossaryTemperatureScaleLayout scale)
        => PointAtTicks(scale, _state.RelationTicks.Raw);

    private Point ReadPoint(GlossaryTemperatureScaleLayout scale)
        => PointAtTicks(scale, _state.ReadTicks.Raw);

    private static Point PointAtTicks(GlossaryTemperatureScaleLayout scale, double ticks)
        => new(
            scale.Start.X + ((scale.End.X - scale.Start.X) * (ticks / GlossaryBasisDemoState.TotalTicks)),
            scale.Start.Y);

    private readonly record struct DragSession(DragTarget Target, Vector Offset)
    {
        public static DragSession Default => new(DragTarget.None, default);
    }

    private enum DragTarget
    {
        None,
        Site,
        Origin,
        Relation,
        Read
    }
}
