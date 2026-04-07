using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;
using Core4.Numerics;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// The dominant-only explorer page. The shell is Avalonia, while the scene is rendered directly from Core4 data.
/// </summary>
public sealed class DominantOnlyPageView : UserControl
{
    private const long MaximumSpan = 16L;
    private readonly DominantOnlySceneControl _scene;
    private readonly MeasureSliderControl _spanSlider;
    private readonly MeasureSliderControl _valueSlider;
    private readonly TextBox _spanBox;
    private readonly TextBox _valueBox;
    private readonly TextBlock _sceneReadingText;
    private readonly TextBlock _sceneOriginText;
    private readonly TextBlock _sceneSettledText;
    private readonly TextBlock _freezeText;
    private readonly TextBlock _readingCardText;
    private readonly TextBlock _orthogonalText;
    private readonly TextBlock _settlementText;
    private DominantOnlyDemoState _state = DominantOnlyDemoState.Default;
    private bool _syncing;

    public DominantOnlyPageView()
    {
        _scene = new DominantOnlySceneControl
        {
            State = _state,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            MinHeight = 286
        };
        _scene.StateChanged += (_, state) =>
        {
            _state = state;
            SyncUiFromState();
        };

        _spanSlider = ProbeUi.CreateSlider(1, 16, _state.DominantSpan.Raw, ProbePalette.DominantSoft, true);
        _spanSlider.ValueChanged += (_, value) =>
        {
            if (!_syncing)
            {
                ApplyState(_state.WithDominantSpan(new Measure(value)));
            }
        };

        _valueSlider = ProbeUi.CreateSlider(-24, 24, _state.ValueOffset.Raw, ProbePalette.Dominant, false);
        _valueSlider.ValueChanged += (_, value) =>
        {
            if (!_syncing)
            {
                ApplyState(_state.WithValueOffset(new Measure(value)));
            }
        };

        _spanBox = ProbeUi.CreateNumberBox(_state.DominantSpan.Raw.ToString());
        HookBox(_spanBox, raw => ApplyState(_state.WithDominantSpan(new Measure(raw))));

        _valueBox = ProbeUi.CreateNumberBox(_state.ValueOffset.Raw.ToString());
        HookBox(_valueBox, raw => ApplyState(_state.WithValueOffset(new Measure(raw))));

        _sceneReadingText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _sceneOriginText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _sceneSettledText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _freezeText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);
        _readingCardText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);
        _orthogonalText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);
        _settlementText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);

        var leftColumn = new StackPanel
        {
            Width = 188,
            Spacing = 6,
            Children =
            {
                ProbeUi.SliderCard("Dominant Span", "Visible dominant reach.", _spanSlider, _spanBox),
                ProbeUi.SliderCard("Value Offset", "Live reading from the origin.", _valueSlider, _valueBox)
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

        var infoCards = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*,*"),
            ColumnSpacing = 10
        };
        ProbeUi.AddInfoCard(infoCards, 0, "FREEZE LADDER", _freezeText);
        ProbeUi.AddInfoCard(infoCards, 1, "READING", _readingCardText);
        ProbeUi.AddInfoCard(infoCards, 2, "ORTHOGONAL FRONTIER", _orthogonalText);
        ProbeUi.AddInfoCard(infoCards, 3, "SETTLEMENT TRACE", _settlementText);

        var rightColumn = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                sceneCard,
                infoCards
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
                ProbeUi.DisplayTitle("One-sided half-line", 18d),
                ProbeUi.Body("A one-sided interval that reads only to the dominant side.", 10d)
            }
        };

        var metaStack = new StackPanel
        {
            Spacing = 2,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children =
            {
                _sceneReadingText,
                _sceneOriginText,
                _sceneSettledText
            }
        };

        grid.Children.Add(titleStack);
        grid.Children.Add(metaStack);
        Grid.SetColumn(metaStack, 1);
        return grid;
    }

    private void ApplyState(DominantOnlyDemoState state)
    {
        _state = Normalize(state);
        _scene.State = _state;
        SyncUiFromState();
    }

    private void SyncUiFromState()
    {
        _syncing = true;
        var valueExtent = Math.Max(24L, Math.Abs(_state.ValueOffset.Raw) + 8L);
        _valueSlider.Minimum = -valueExtent;
        _valueSlider.Maximum = valueExtent;
        _spanSlider.Value = _state.DominantSpan.Raw;
        _valueSlider.Value = _state.ValueOffset.Raw;
        _spanBox.Text = _state.DominantSpan.Raw.ToString();
        _valueBox.Text = _state.ValueOffset.Raw.ToString();
        _syncing = false;

        var dominantRatio = _state.IsHiddenInverse
            ? Ratio.Tension((_state.Origin - _state.ValueLocation).Absolute)
            : _state.Reading;
        var dominantApprox = (double)_state.ValueOffset.Raw / _state.DominantSpan.Raw;
        var settled = Ratio.Tension(_state.ValueOffset.Absolute);

        _sceneReadingText.Text = $"0/0r + {dominantRatio}d";
        _sceneOriginText.Text = $"origin {_state.Origin.Raw}";
        _sceneSettledText.Text = $"settled {settled}";

        _freezeText.Text =
            $"Origin                    {_state.Origin.Raw}\n" +
            $"Dominant                  0 -> {_state.DominantSpan.Raw}\n" +
            "Recessive                 not present\n" +
            "Structure                 single dominant interval";

        _readingCardText.Text =
            $"Complex                   0/0r + {dominantRatio}d\n" +
            $"Dominant                  {dominantRatio}\n" +
            "Recessive                 unavailable\n" +
            "Mode                      One-sided half-line\n" +
            $"Approx                    (0.0i + {dominantApprox:F1})";

        _orthogonalText.Text =
            "Tension                   0/0\n" +
            "Incoming                  0/0\n" +
            "Outgoing                  0/0\n" +
            "Connections               0";

        _settlementText.Text =
            "1 Anchor                  0/1\n" +
            "2 Incoming                0/0\n" +
            "3 Provisional             0/0\n" +
            $"4 Published               {settled}\n" +
            "5 Feedback                0/0\n" +
            $"6 Settled                 {settled}\n" +
            "7 Remainder               0/0";
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

    private static DominantOnlyDemoState Normalize(DominantOnlyDemoState state) =>
        state.WithDominantSpan(new Measure(Math.Clamp(state.DominantSpan.Raw, 1L, MaximumSpan)));
}
