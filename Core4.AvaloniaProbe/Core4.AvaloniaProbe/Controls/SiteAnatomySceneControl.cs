using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;

namespace Core4.AvaloniaProbe.Controls;

/// <summary>
/// Render surface for the static single-site anatomy page.
/// </summary>
public sealed class SiteAnatomySceneControl : Control
{
    private SiteAnatomyDemoState _state = SiteAnatomyDemoState.Default;
    private SiteAnatomyFocus _selectedFocus = SiteAnatomyFocus.All;

    public SiteAnatomySceneControl()
    {
        MinWidth = 560;
        MinHeight = 380;
        ClipToBounds = true;
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
    }

    public SiteAnatomyDemoState State
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

    public SiteAnatomyFocus SelectedFocus
    {
        get => _selectedFocus;
        set
        {
            if (_selectedFocus == value)
            {
                return;
            }

            _selectedFocus = value;
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        SiteAnatomyRenderer.Render(context, Bounds, _state, _selectedFocus);
    }
}
