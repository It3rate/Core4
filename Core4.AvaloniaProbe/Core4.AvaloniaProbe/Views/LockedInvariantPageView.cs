using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// First pass at locked revealed invariants. Each primitive fusion keeps one newly revealed property fixed and shows
/// the compatible motion that appears when one participant is perturbed.
/// </summary>
public sealed class LockedInvariantPageView : UserControl
{
    private readonly LockedInvariantSceneControl _scene;
    private readonly TextBlock _selfMeta;
    private readonly TextBlock _segmentMeta;
    private readonly TextBlock _complexMeta;
    private LockedInvariantDemoState _state = LockedInvariantDemoState.Default;

    public LockedInvariantPageView()
    {
        _scene = new LockedInvariantSceneControl
        {
            State = _state,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            MinHeight = 442
        };
        _scene.StateChanged += (_, state) =>
        {
            _state = state;
            SyncUi();
        };

        var resetButton = ProbeUi.PillButton("Reset Locks");
        resetButton.Click += (_, _) =>
        {
            _state = LockedInvariantDemoState.Default;
            _scene.State = _state;
            _scene.ResetTrails();
            SyncUi();
        };

        _selfMeta = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _segmentMeta = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _complexMeta = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);

        var leftColumn = new StackPanel
        {
            Width = 208,
            Spacing = 6,
            Children =
            {
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        ProbeUi.Mono("ACTIVE PRESET", 10d, ProbePalette.Muted),
                        ProbeUi.DisplayTitle("Locked Invariants", 18d),
                        ProbeUi.Body("Freeze one revealed property per primitive fusion, perturb a local participant, and inspect the compatible response.", 10d)
                    }
                }, 20, new Thickness(12, 10)),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        ProbeUi.Mono("READING", 10d, ProbePalette.Muted),
                        ProbeUi.Body("Self and segment still solve locally against their lock. The four 2D families share one signed pair so the same motion can be inspected through several frozen loci.", 10d)
                    }
                }, 20, new Thickness(12, 10)),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 6,
                    Children =
                    {
                        ProbeUi.Mono("RESET", 10d, ProbePalette.Muted),
                        resetButton
                    }
                }, 20, new Thickness(12, 10))
            }
        };

        var sceneCard = ProbeUi.Card(new StackPanel
        {
            Spacing = 8,
            Children =
            {
                SceneHeader(),
                ProbeUi.PlotCard(_scene)
            }
        }, 22, new Thickness(12));

        var mainGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("208,*"),
            ColumnSpacing = 8,
            Children =
            {
                leftColumn,
                sceneCard
            }
        };
        Grid.SetColumn(sceneCard, 1);

        Content = mainGrid;
        SyncUi();
    }

    private Grid SceneHeader()
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
                ProbeUi.DisplayTitle("Compatible Motion", 18d),
                ProbeUi.Body("Drag the white handles. Self and segment preserve their local lock, while the four 2D panels share one signed probe and leave a resettable trail.", 10d)
            }
        };

        var metaStack = new StackPanel
        {
            Spacing = 2,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children =
            {
                _selfMeta,
                _segmentMeta,
                _complexMeta
            }
        };

        grid.Children.Add(titleStack);
        grid.Children.Add(metaStack);
        Grid.SetColumn(metaStack, 1);
        return grid;
    }

    private void SyncUi()
    {
        _selfMeta.Text = $"self {_state.SelfPrimaryUnits:0.0}x{_state.SelfSecondaryUnits:0.0}";
        _segmentMeta.Text = $"segment {_state.SegmentRecessiveUnits:0.0}+{_state.SegmentDominantUnits:0.0}";
        _complexMeta.Text = $"2d {_state.QuadrantPrimaryUnits:0.0}/{_state.QuadrantSecondaryUnits:0.0}";
    }
}
