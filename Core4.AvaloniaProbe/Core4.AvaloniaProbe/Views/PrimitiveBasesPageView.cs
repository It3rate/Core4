using Avalonia.Controls;
using Avalonia.Layout;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// The primitive-bases gallery page. Two aligned endpoints drive the six grade-two interpretations.
/// </summary>
public sealed class PrimitiveBasesPageView : UserControl
{
    private readonly PrimitiveBasesSceneControl _scene;
    private PrimitiveBasesDemoState _state = PrimitiveBasesDemoState.Default;

    public PrimitiveBasesPageView()
    {
        _scene = new PrimitiveBasesSceneControl
        {
            State = _state,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            MinHeight = 352
        };
        _scene.StateChanged += (_, state) => _state = state;

        Content = ProbeUi.Card(new StackPanel
        {
            Spacing = 8,
            Children =
            {
                SceneHeader(),
                ProbeUi.PlotCard(_scene)
            }
        }, 22, new Avalonia.Thickness(12));
    }

    private static Grid SceneHeader()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto")
        };

        var titleStack = new StackPanel
        {
            Spacing = 2,
            Children =
            {
                ProbeUi.Mono("INTERACTIVE SCENE", 10d, ProbePalette.Muted),
                ProbeUi.DisplayTitle("Primitive bases", 18d),
                ProbeUi.Body("One aligned pair plus one orthogonal extension yields the six primitive fused interpretations.", 10d)
            }
        };

        grid.Children.Add(titleStack);
        return grid;
    }
}
