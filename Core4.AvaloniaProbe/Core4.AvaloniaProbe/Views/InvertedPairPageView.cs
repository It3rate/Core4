using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Formatting;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;
using Core4.Numerics;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// The inverted-pair explorer page. It renders two opposed bases on one carrier before they are fused.
/// </summary>
public sealed class InvertedPairPageView : UserControl
{
    private const long MaximumSpan = 16L;
    private readonly InvertedPairSceneControl _scene;
    private readonly MeasureSliderControl _dominantSpanSlider;
    private readonly MeasureSliderControl _recessiveSpanSlider;
    private readonly MeasureSliderControl _dominantValueSlider;
    private readonly MeasureSliderControl _recessiveValueSlider;
    private readonly TextBox _dominantSpanBox;
    private readonly TextBox _recessiveSpanBox;
    private readonly TextBox _dominantValueBox;
    private readonly TextBox _recessiveValueBox;
    private readonly TextBlock _sceneReadingText;
    private readonly TextBlock _sceneOriginText;
    private readonly TextBlock _scenePairText;
    private readonly TextBlock _freezeText;
    private readonly TextBlock _readingCardText;
    private readonly TextBlock _pairStateText;
    private readonly TextBlock _valueSitesText;
    private InvertedPairDemoState _state = InvertedPairDemoState.Default;
    private bool _syncing;

    public InvertedPairPageView()
    {
        _scene = new InvertedPairSceneControl
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

        _dominantSpanSlider = ProbeUi.CreateSlider(1, 16, _state.DominantSpan.Raw, ProbePalette.Dominant, true);
        _recessiveSpanSlider = ProbeUi.CreateSlider(1, 16, _state.RecessiveSpan.Raw, ProbePalette.DominantSoft, true);
        _dominantValueSlider = ProbeUi.CreateSlider(-24, 24, _state.DominantValueOffset.Raw, ProbePalette.Dominant, false);
        _recessiveValueSlider = ProbeUi.CreateSlider(-24, 24, _state.RecessiveValueOffset.Raw, ProbePalette.DominantSoft, false);

        _dominantSpanSlider.ValueChanged += (_, value) =>
        {
            if (!_syncing)
            {
                ApplyState(_state.WithDominantSpan(new Measure(value)));
            }
        };
        _recessiveSpanSlider.ValueChanged += (_, value) =>
        {
            if (!_syncing)
            {
                ApplyState(_state.WithRecessiveSpan(new Measure(value)));
            }
        };
        _dominantValueSlider.ValueChanged += (_, value) =>
        {
            if (!_syncing)
            {
                ApplyState(_state.WithDominantValueOffset(new Measure(value)));
            }
        };
        _recessiveValueSlider.ValueChanged += (_, value) =>
        {
            if (!_syncing)
            {
                ApplyState(_state.WithRecessiveValueOffset(new Measure(value)));
            }
        };

        _dominantSpanBox = ProbeUi.CreateNumberBox(_state.DominantSpan.Raw.ToString());
        _recessiveSpanBox = ProbeUi.CreateNumberBox(_state.RecessiveSpan.Raw.ToString());
        _dominantValueBox = ProbeUi.CreateNumberBox(_state.DominantValueOffset.Raw.ToString());
        _recessiveValueBox = ProbeUi.CreateNumberBox(_state.RecessiveValueOffset.Raw.ToString());

        HookBox(_dominantSpanBox, raw => ApplyState(_state.WithDominantSpan(new Measure(raw))));
        HookBox(_recessiveSpanBox, raw => ApplyState(_state.WithRecessiveSpan(new Measure(raw))));
        HookBox(_dominantValueBox, raw => ApplyState(_state.WithDominantValueOffset(new Measure(raw))));
        HookBox(_recessiveValueBox, raw => ApplyState(_state.WithRecessiveValueOffset(new Measure(raw))));

        _sceneReadingText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _sceneOriginText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _scenePairText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _freezeText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);
        _readingCardText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);
        _pairStateText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);
        _valueSitesText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);

        var leftColumn = new StackPanel
        {
            Width = 188,
            Spacing = 6,
            Children =
            {
                ProbeUi.SliderCard("Dominant Span", "Right-facing unit reach.", _dominantSpanSlider, _dominantSpanBox),
                ProbeUi.SliderCard("Recessive Span", "Left-facing unit reach.", _recessiveSpanSlider, _recessiveSpanBox),
                ProbeUi.SliderCard("Dominant Value", "Upper lane value on the dominant unit.", _dominantValueSlider, _dominantValueBox),
                ProbeUi.SliderCard("Recessive Value", "Lower lane value on the recessive unit.", _recessiveValueSlider, _recessiveValueBox)
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
        ProbeUi.AddInfoCard(infoCards, 2, "PAIR STATE", _pairStateText);
        ProbeUi.AddInfoCard(infoCards, 3, "VALUE SITES", _valueSitesText);

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
                ProbeUi.DisplayTitle("Unfused inverted pair", 18d),
                ProbeUi.Body("Dominant and recessive intervals can coexist before they harden into one directed segment.", 10d)
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
                _scenePairText
            }
        };

        grid.Children.Add(titleStack);
        grid.Children.Add(metaStack);
        Grid.SetColumn(metaStack, 1);
        return grid;
    }

    private void ApplyState(InvertedPairDemoState state)
    {
        _state = Normalize(state);
        _scene.State = _state;
        SyncUiFromState();
    }

    private void SyncUiFromState()
    {
        _syncing = true;
        var dominantExtent = Math.Max(24L, Math.Abs(_state.DominantValueOffset.Raw) + 8L);
        var recessiveExtent = Math.Max(24L, Math.Abs(_state.RecessiveValueOffset.Raw) + 8L);
        _dominantValueSlider.Minimum = -dominantExtent;
        _dominantValueSlider.Maximum = dominantExtent;
        _recessiveValueSlider.Minimum = -recessiveExtent;
        _recessiveValueSlider.Maximum = recessiveExtent;

        _dominantSpanSlider.Value = _state.DominantSpan.Raw;
        _recessiveSpanSlider.Value = _state.RecessiveSpan.Raw;
        _dominantValueSlider.Value = _state.DominantValueOffset.Raw;
        _recessiveValueSlider.Value = _state.RecessiveValueOffset.Raw;

        _dominantSpanBox.Text = _state.DominantSpan.Raw.ToString();
        _recessiveSpanBox.Text = _state.RecessiveSpan.Raw.ToString();
        _dominantValueBox.Text = _state.DominantValueOffset.Raw.ToString();
        _recessiveValueBox.Text = _state.RecessiveValueOffset.Raw.ToString();
        _syncing = false;

        _sceneReadingText.Text = _state.ComplexReading;
        _sceneOriginText.Text = $"origin {_state.Origin.Raw}";
        _scenePairText.Text = $"pair {_state.RecessiveSpan.Raw}/{_state.DominantSpan.Raw}";

        _freezeText.Text =
            $"Origin                    {_state.Origin.Raw}\n" +
            $"Dominant                  0 -> {_state.DominantSpan.Raw}\n" +
            $"Recessive                 0 -> -{_state.RecessiveSpan.Raw}\n" +
            "Structure                 unfused inverted pair";

        _readingCardText.Text =
            $"Complex                   {_state.ComplexReading}\n" +
            $"Dominant                  {_state.DisplayDominantReading}\n" +
            $"Recessive                 {_state.DisplayRecessiveReading}\n" +
            "Mode                      Inverted pair\n" +
            $"Approx                    {_state.ApproximateReading}";

        _pairStateText.Text =
            $"Dominant basis            {_state.DominantBasis.Ratio}\n" +
            $"Recessive basis           {_state.RecessiveBasis.Ratio}\n" +
            $"Dominant value            {_state.DominantValueOffset.Raw}\n" +
            $"Recessive value           {_state.RecessiveValueOffset.Raw}";

        _valueSitesText.Text =
            $"Dominant site             {ProbeSiteFormatting.PositionLabel(_state.DominantValueSite)}\n" +
            $"Recessive site            {ProbeSiteFormatting.PositionLabel(_state.RecessiveValueSite)}\n" +
            $"Dominant anchor           {ProbeSiteFormatting.PositionLabel(_state.DominantUnitLocation)}\n" +
            $"Recessive anchor          {ProbeSiteFormatting.PositionLabel(_state.RecessiveUnitLocation)}";
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

    private static InvertedPairDemoState Normalize(InvertedPairDemoState state) =>
        state
            .WithDominantSpan(new Measure(Math.Clamp(state.DominantSpan.Raw, 1L, MaximumSpan)))
            .WithRecessiveSpan(new Measure(Math.Clamp(state.RecessiveSpan.Raw, 1L, MaximumSpan)));
}
