using System.Collections.Immutable;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;
using Core4.Dynamics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Controls;

/// <summary>
/// Small schematic host for the first passive circuit explorer. It renders a fixed circuit layout from real Core4
/// topology plus a current relaxed potential field.
/// </summary>
public sealed class CircuitSceneControl : Control
{
    private CircuitDemoState _demo;
    private ResistiveCircuit _circuit;
    private ResistiveCircuitState _state;
    private ImmutableDictionary<Placement, Point> _positions = ImmutableDictionary<Placement, Point>.Empty;
    private Placement _positiveNode;
    private Placement _negativeNode;
    private bool _core4View;
    private bool _fractionView;

    public CircuitSceneControl()
    {
        MinHeight = 400;
        MinWidth = 680;
        ClipToBounds = true;
    }

    public CircuitDemoState Demo
    {
        get => _demo;
        set
        {
            _demo = value;
            InvalidateVisual();
        }
    }

    public ResistiveCircuit Circuit
    {
        get => _circuit;
        set
        {
            _circuit = value;
            InvalidateVisual();
        }
    }

    public ResistiveCircuitState State
    {
        get => _state;
        set
        {
            _state = value;
            InvalidateVisual();
        }
    }

    public ImmutableDictionary<Placement, Point> Positions
    {
        get => _positions;
        set
        {
            _positions = value;
            InvalidateVisual();
        }
    }

    public Placement PositiveNode
    {
        get => _positiveNode;
        set
        {
            _positiveNode = value;
            InvalidateVisual();
        }
    }

    public Placement NegativeNode
    {
        get => _negativeNode;
        set
        {
            _negativeNode = value;
            InvalidateVisual();
        }
    }

    public bool Core4View
    {
        get => _core4View;
        set
        {
            _core4View = value;
            InvalidateVisual();
        }
    }

    public bool FractionView
    {
        get => _fractionView;
        set
        {
            _fractionView = value;
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        CircuitRenderer.Render(context, Bounds, _demo, _circuit, _state, _positions, _positiveNode, _negativeNode, _core4View, _fractionView);
    }
}
