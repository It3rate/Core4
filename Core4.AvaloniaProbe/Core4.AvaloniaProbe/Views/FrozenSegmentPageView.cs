using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Formatting;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;
using Core4.Numerics;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// The frozen-segment explorer page. It reuses the opposed pair but reads it as one fused directed segment.
/// </summary>
public sealed class FrozenSegmentPageView : UserControl
{
    private const long MaximumSpan = 16L;
    private readonly FrozenSegmentSceneControl _scene;
    private readonly MeasureSliderControl _spanSlider;
    private readonly MeasureSliderControl _dominantValueSlider;
    private readonly MeasureSliderControl _recessiveValueSlider;
    private readonly TextBox _spanBox;
    private readonly TextBox _dominantValueBox;
    private readonly TextBox _recessiveValueBox;
    private readonly TextBlock _sceneReadingText;
    private readonly TextBlock _sceneOriginText;
    private readonly TextBlock _scenePairText;
    private readonly TextBlock _freezeText;
    private readonly TextBlock _readingCardText;
    private readonly TextBlock _segmentText;
    private readonly TextBlock _valueSitesText;
    private FrozenSegmentDemoState _state = FrozenSegmentDemoState.Default;
    private bool _syncing;

    public FrozenSegmentPageView()
    {
        _scene = new FrozenSegmentSceneControl
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

        _spanSlider = ProbeUi.CreateSlider(1, 16, _state.Span.Raw, ProbePalette.Dominant, true);
        _dominantValueSlider = ProbeUi.CreateSlider(-24, 24, _state.DominantValueOffset.Raw, ProbePalette.Dominant, false);
        _recessiveValueSlider = ProbeUi.CreateSlider(-24, 24, _state.RecessiveValueOffset.Raw, ProbePalette.DominantSoft, false);

        _spanSlider.ValueChanged += (_, value) =>
        {
            if (!_syncing)
            {
                ApplyState(_state.WithSpan(new Measure(value)));
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

        _spanBox = ProbeUi.CreateNumberBox(_state.Span.Raw.ToString());
        _dominantValueBox = ProbeUi.CreateNumberBox(_state.DominantValueOffset.Raw.ToString());
        _recessiveValueBox = ProbeUi.CreateNumberBox(_state.RecessiveValueOffset.Raw.ToString());

        HookBox(_spanBox, raw => ApplyState(_state.WithSpan(new Measure(raw))));
        HookBox(_dominantValueBox, raw => ApplyState(_state.WithDominantValueOffset(new Measure(raw))));
        HookBox(_recessiveValueBox, raw => ApplyState(_state.WithRecessiveValueOffset(new Measure(raw))));

        _sceneReadingText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _sceneOriginText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _scenePairText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _freezeText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);
        _readingCardText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);
        _segmentText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);
        _valueSitesText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);

        var leftColumn = new StackPanel
        {
            Width = 188,
            Spacing = 6,
            Children =
            {
                ProbeUi.SliderCard("Span", "Shared fused unit on both sides.", _spanSlider, _spanBox),
                ProbeUi.SliderCard("Dominant Value", "Upper lane value on the fused segment.", _dominantValueSlider, _dominantValueBox),
                ProbeUi.SliderCard("Recessive Value", "Lower lane value on the fused segment.", _recessiveValueSlider, _recessiveValueBox)
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
        ProbeUi.AddInfoCard(infoCards, 2, "SEGMENT", _segmentText);
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
                ProbeUi.DisplayTitle("Fused directed segment", 18d),
                ProbeUi.Body("The opposed pair is frozen together, so both values can now cross the origin as one segment.", 10d)
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

    private void ApplyState(FrozenSegmentDemoState state)
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

        _spanSlider.Value = _state.Span.Raw;
        _dominantValueSlider.Value = _state.DominantValueOffset.Raw;
        _recessiveValueSlider.Value = _state.RecessiveValueOffset.Raw;

        _spanBox.Text = _state.Span.Raw.ToString();
        _dominantValueBox.Text = _state.DominantValueOffset.Raw.ToString();
        _recessiveValueBox.Text = _state.RecessiveValueOffset.Raw.ToString();
        _syncing = false;

        _sceneReadingText.Text = _state.ComplexReading;
        _sceneOriginText.Text = $"origin {_state.Origin.Raw}";
        _scenePairText.Text = $"segment {_state.Span.Raw}/{_state.Span.Raw}";

        _freezeText.Text =
            $"Origin                    {_state.Origin.Raw}\n" +
            $"Dominant                  0 -> {_state.Span.Raw}\n" +
            $"Recessive                 0 -> -{_state.Span.Raw}\n" +
            "Structure                 fused directed segment";

        _readingCardText.Text =
            $"Complex                   {_state.ComplexReading}\n" +
            $"Dominant                  {_state.DominantReading}\n" +
            $"Recessive                 {_state.RecessiveReading}\n" +
            "Mode                      Frozen segment\n" +
            $"Approx                    {_state.ApproximateReading}";

        _segmentText.Text =
            $"Segment basis             {_state.SegmentBasis.Ratio}\n" +
            $"Dominant basis            {_state.DominantBasis.Ratio}\n" +
            $"Recessive basis           {_state.RecessiveBasis.Ratio}\n" +
            $"Overlay                   {Math.Abs(_state.DominantValueLocation.Raw - _state.RecessiveValueLocation.Raw)}";

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

    private static FrozenSegmentDemoState Normalize(FrozenSegmentDemoState state) =>
        state.WithSpan(new Measure(Math.Clamp(state.Span.Raw, 1L, MaximumSpan)));
}
