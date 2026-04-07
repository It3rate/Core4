using System;
using System.Linq;
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
/// The orthogonal-output explorer page. The fused aligned segment remains in place while one top-side output
/// publishes from the same site.
/// </summary>
public sealed class OrthogonalOutputPageView : UserControl
{
    private const long MaximumSpan = 16L;
    private const long MaximumOutput = 16L;
    private readonly OrthogonalOutputSceneControl _scene;
    private readonly MeasureSliderControl _topOutputSlider;
    private readonly MeasureSliderControl _topTensionSlider;
    private readonly TextBox _topOutputBox;
    private readonly TextBox _topTensionBox;
    private readonly TextBlock _sceneReadingText;
    private readonly TextBlock _sceneOriginText;
    private readonly TextBlock _sceneSettledText;
    private readonly TextBlock _freezeText;
    private readonly TextBlock _readingCardText;
    private readonly TextBlock _orthogonalText;
    private readonly TextBlock _settlementText;
    private OrthogonalOutputDemoState _state = OrthogonalOutputDemoState.Default;
    private bool _syncing;

    public OrthogonalOutputPageView()
    {
        _scene = new OrthogonalOutputSceneControl
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

        _topOutputSlider = ProbeUi.CreateSlider(0, 16, _state.TopOutput.Raw, ProbePalette.Orthogonal, true);
        _topTensionSlider = ProbeUi.CreateSlider(-16, 16, _state.TopTension.Raw, ProbePalette.TensionPositive, false);

        _topOutputSlider.ValueChanged += (_, value) =>
        {
            if (!_syncing)
            {
                ApplyState(_state.WithTopOutput(new Measure(value)));
            }
        };
        _topTensionSlider.ValueChanged += (_, value) =>
        {
            if (!_syncing)
            {
                ApplyState(_state.WithTopTension(new Measure(value)));
            }
        };

        _topOutputBox = ProbeUi.CreateNumberBox(_state.TopOutput.Raw.ToString());
        _topTensionBox = ProbeUi.CreateNumberBox(_state.TopTension.Raw.ToString());
        HookBox(_topOutputBox, raw => ApplyState(_state.WithTopOutput(new Measure(raw))));
        HookBox(_topTensionBox, raw => ApplyState(_state.WithTopTension(new Measure(raw))));

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
                ProbeUi.SliderCard("Top Output", "Dominant orthogonal publication.", _topOutputSlider, _topOutputBox),
                ProbeUi.SliderCard("Top Tension", "Residual orthogonal tension around the published line.", _topTensionSlider, _topTensionBox)
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
                ProbeUi.DisplayTitle("Orthogonal output", 18d),
                ProbeUi.Body("A top-side publication rises from the same frozen segment while the aligned basis stays simple.", 10d)
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

    private void ApplyState(OrthogonalOutputDemoState state)
    {
        _state = Normalize(state);
        _scene.State = _state;
        SyncUiFromState();
    }

    private void SyncUiFromState()
    {
        _syncing = true;
        var tensionExtent = Math.Max(16L, Math.Abs(_state.TopTension.Raw) + 8L);
        _topTensionSlider.Minimum = -tensionExtent;
        _topTensionSlider.Maximum = tensionExtent;
        _topOutputSlider.Value = _state.TopOutput.Raw;
        _topTensionSlider.Value = _state.TopTension.Raw;
        _topOutputBox.Text = _state.TopOutput.Raw.ToString();
        _topTensionBox.Text = _state.TopTension.Raw.ToString();
        _syncing = false;

        var settlement = _state.Settlement;
        _sceneReadingText.Text = _state.ComplexReading;
        _sceneOriginText.Text = $"origin {_state.Origin.Raw}";
        _sceneSettledText.Text = $"settled {settlement.Settled.Normalize()}";

        _freezeText.Text =
            $"Origin                    {_state.Origin.Raw}\n" +
            $"Dominant                  0 -> {_state.Span.Raw}\n" +
            $"Recessive                 0 -> -{_state.Span.Raw}\n" +
            "Structure                 fused segment + top output";

        _readingCardText.Text =
            $"Complex                   {_state.ComplexReading}\n" +
            $"Dominant                  {_state.DominantReading}\n" +
            $"Recessive                 {_state.RecessiveReading}\n" +
            $"Outgoing                  {_state.OutputRatio}\n" +
            $"Approx                    {_state.ApproximateReading}";

        _orthogonalText.Text =
            $"Tension                   {_state.Orthogonal.Residual.Normalize()}\n" +
            $"Incoming                  {_state.Orthogonal.Incoming().Normalize()}\n" +
            $"Outgoing                  {_state.Orthogonal.Outgoing().Normalize()}\n" +
            $"Connections               {_state.Orthogonal.ConnectionCount.Raw}";

        _settlementText.Text = string.Join(
            "\n",
            settlement.Terms.Select(term => $"{term.Order.Raw} {term.Phase,-25}{term.Value.Normalize()}"));
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

    private static OrthogonalOutputDemoState Normalize(OrthogonalOutputDemoState state) =>
        state
            .WithSpan(new Measure(Math.Clamp(state.Span.Raw, 1L, MaximumSpan)))
            .WithTopOutput(new Measure(Math.Clamp(state.TopOutput.Raw, 0L, MaximumOutput)));
}
