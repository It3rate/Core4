using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;
using Core4.Numerics;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// The primitive-bases gallery page. Two aligned sliders drive the directed segment and every derived quadrant.
/// </summary>
public sealed class PrimitiveBasesPageView : UserControl
{
    private const long MinimumTicks = -24L;
    private const long MaximumTicks = 24L;

    private readonly PrimitiveBasesSceneControl _scene;
    private readonly MeasureSliderControl _recessiveSlider;
    private readonly MeasureSliderControl _dominantSlider;
    private readonly TextBox _recessiveBox;
    private readonly TextBox _dominantBox;
    private readonly TextBlock _sceneSegmentText;
    private readonly TextBlock _sceneSelfText;
    private readonly TextBlock _sceneComplexText;
    private PrimitiveBasesDemoState _state = PrimitiveBasesDemoState.Default;
    private bool _syncing;

    public PrimitiveBasesPageView()
    {
        _scene = new PrimitiveBasesSceneControl
        {
            State = _state,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            MinHeight = 442
        };
        _scene.StateChanged += (_, state) =>
        {
            _state = state;
            SyncUiFromState();
        };

        _recessiveSlider = ProbeUi.CreateSlider(MinimumTicks, MaximumTicks, _state.RecessiveTicks.Raw, ProbePalette.DominantSoft, false);
        _dominantSlider = ProbeUi.CreateSlider(MinimumTicks, MaximumTicks, _state.DominantTicks.Raw, ProbePalette.Dominant, false);
        _recessiveSlider.ValueChanged += (_, value) => ApplyIfLive(_state.WithRecessiveTicks(new Measure(value)));
        _dominantSlider.ValueChanged += (_, value) => ApplyIfLive(_state.WithDominantTicks(new Measure(value)));

        _recessiveBox = ProbeUi.CreateNumberBox(_state.RecessiveTicks.Raw.ToString());
        _dominantBox = ProbeUi.CreateNumberBox(_state.DominantTicks.Raw.ToString());
        HookBox(_recessiveBox, raw => ApplyState(_state.WithRecessiveTicks(new Measure(raw))));
        HookBox(_dominantBox, raw => ApplyState(_state.WithDominantTicks(new Measure(raw))));

        _sceneSegmentText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _sceneSelfText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _sceneComplexText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);

        var leftColumn = new StackPanel
        {
            Width = 188,
            Spacing = 6,
            Children =
            {
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        ProbeUi.Mono("ACTIVE PRESET", 10d, ProbePalette.Muted),
                        ProbeUi.DisplayTitle("Primitive Bases", 18d),
                        ProbeUi.Body("Six primitive fused forms built from one aligned pair and one orthogonal pair.", 10d)
                    }
                }, 20, new Thickness(12, 10)),
                ProbeUi.SliderCard("Recessive", "Directed-segment recessive value.", _recessiveSlider, _recessiveBox),
                ProbeUi.SliderCard("Dominant", "Directed-segment dominant value.", _dominantSlider, _dominantBox)
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

        var rightColumn = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                sceneCard
            }
        };

        var mainGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("188,*"),
            ColumnSpacing = 8,
            Children =
            {
                leftColumn,
                rightColumn
            }
        };
        Grid.SetColumn(rightColumn, 1);

        Content = mainGrid;
        SyncUiFromState();
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
                ProbeUi.DisplayTitle("Six Primitive Fusions", 18d),
                ProbeUi.Body("Six primitive fused forms built from one aligned pair and one orthogonal pair.", 10d)
            }
        };

        var metaStack = new StackPanel
        {
            Spacing = 2,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children =
            {
                _sceneSegmentText,
                _sceneSelfText,
                _sceneComplexText
            }
        };

        grid.Children.Add(titleStack);
        grid.Children.Add(metaStack);
        Grid.SetColumn(metaStack, 1);
        return grid;
    }

    private void ApplyIfLive(PrimitiveBasesDemoState state)
    {
        if (!_syncing)
        {
            ApplyState(state);
        }
    }

    private void ApplyState(PrimitiveBasesDemoState state)
    {
        _state = Normalize(state);
        _scene.State = _state;
        SyncUiFromState();
    }

    private void SyncUiFromState()
    {
        _syncing = true;
        _recessiveSlider.Minimum = MinimumTicks;
        _recessiveSlider.Maximum = MaximumTicks;
        _dominantSlider.Minimum = MinimumTicks;
        _dominantSlider.Maximum = MaximumTicks;
        _recessiveSlider.Value = _state.RecessiveTicks.Raw;
        _dominantSlider.Value = _state.DominantTicks.Raw;
        _recessiveBox.Text = _state.RecessiveTicks.Raw.ToString();
        _dominantBox.Text = _state.DominantTicks.Raw.ToString();
        _syncing = false;

        _sceneSegmentText.Text = $"segment {_state.SegmentUnits:0.###}";
        _sceneSelfText.Text = $"self {_state.SelfUnits:0.###}";
        _sceneComplexText.Text = $"complex {_state.Complex.Normalize().Approximate:0.###}";
    }

    private void HookBox(TextBox box, Action<long> apply)
    {
        box.LostFocus += (_, _) => TryApplyBox(box, apply);
        box.KeyUp += (_, args) =>
        {
            if (args.Key == Key.Enter)
            {
                TryApplyBox(box, apply);
            }
        };
    }

    private void TryApplyBox(TextBox box, Action<long> apply)
    {
        if (_syncing || !long.TryParse(box.Text, out var raw))
        {
            return;
        }

        apply(raw);
    }

    private static PrimitiveBasesDemoState Normalize(PrimitiveBasesDemoState state) => state with
    {
        DominantTicks = new Measure(Math.Clamp(state.DominantTicks.Raw, MinimumTicks, MaximumTicks)),
        RecessiveTicks = new Measure(Math.Clamp(state.RecessiveTicks.Raw, MinimumTicks, MaximumTicks))
    };
}
