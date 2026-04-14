using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;

namespace Core4.AvaloniaProbe.Controls;

/// <summary>
/// Render surface for the routing/stiffness transport page.
/// </summary>
public sealed class RoutingStiffnessSceneControl : Control
{
    private RoutingStiffnessDemoState _state = RoutingStiffnessDemoState.Default;
    private bool _showStructure = true;

    public RoutingStiffnessSceneControl()
    {
        MinWidth = 700;
        MinHeight = 360;
        ClipToBounds = true;
    }

    public RoutingStiffnessDemoState State
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
        }
    }

    public bool ShowStructure
    {
        get => _showStructure;
        set
        {
            if (_showStructure == value)
            {
                return;
            }

            _showStructure = value;
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        RoutingStiffnessRenderer.Render(context, Bounds, _state, _showStructure);
    }
}
