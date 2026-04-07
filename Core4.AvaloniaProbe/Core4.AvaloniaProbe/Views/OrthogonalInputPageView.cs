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
/// The orthogonal-input explorer page. One output publishes upward while one input arrives from below.
/// </summary>
public sealed class OrthogonalInputPageView : UserControl
{
    private const long MaximumSpan = 16L;
    private const long MaximumOutput = 14L;
    private const long MaximumInput = 14L;
    private const long MinimumTension = -22L;
    private const long MaximumTension = 20L;

    private readonly OrthogonalInputSceneControl _scene;
    private readonly MeasureSliderControl _topOutputSlider;
    private readonly MeasureSliderControl _topTensionSlider;
    private readonly MeasureSliderControl _bottomInputSlider;
    private readonly MeasureSliderControl _bottomTensionSlider;
    private readonly TextBox _topOutputBox;
    private readonly TextBox _topTensionBox;
    private readonly TextBox _bottomInputBox;
    private readonly TextBox _bottomTensionBox;
    private readonly TextBlock _sceneReadingText;
    private readonly TextBlock _sceneOriginText;
    private readonly TextBlock _sceneSettledText;
    private readonly TextBlock _freezeText;
    private readonly TextBlock _readingCardText;
    private readonly TextBlock _orthogonalText;
    private readonly TextBlock _settlementText;
    private OrthogonalInputDemoState _state = OrthogonalInputDemoState.Default;
    private bool _syncing;

    public OrthogonalInputPageView()
    {
        _scene = new OrthogonalInputSceneControl
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

        _topOutputSlider = ProbeUi.CreateSlider(0, MaximumOutput, _state.TopOutput.Raw, ProbePalette.Orthogonal, true);
        _topTensionSlider = ProbeUi.CreateSlider(MinimumTension, MaximumTension, _state.TopTension.Raw, ProbePalette.TensionPositive, false);
        _bottomInputSlider = ProbeUi.CreateSlider(0, MaximumInput, _state.BottomInput.Raw, ProbePalette.OrthogonalInput, true);
        _bottomTensionSlider = ProbeUi.CreateSlider(MinimumTension, MaximumTension, _state.BottomTension.Raw, ProbePalette.TensionPositive, false);

        _topOutputSlider.ValueChanged += (_, value) => ApplyIfLive(_state.WithTopOutput(new Measure(value)));
        _topTensionSlider.ValueChanged += (_, value) => ApplyIfLive(_state.WithTopTension(new Measure(value)));
        _bottomInputSlider.ValueChanged += (_, value) => ApplyIfLive(_state.WithBottomInput(new Measure(value)));
        _bottomTensionSlider.ValueChanged += (_, value) => ApplyIfLive(_state.WithBottomTension(new Measure(value)));

        _topOutputBox = ProbeUi.CreateNumberBox(_state.TopOutput.Raw.ToString());
        _topTensionBox = ProbeUi.CreateNumberBox(_state.TopTension.Raw.ToString());
        _bottomInputBox = ProbeUi.CreateNumberBox(_state.BottomInput.Raw.ToString());
        _bottomTensionBox = ProbeUi.CreateNumberBox(_state.BottomTension.Raw.ToString());

        HookBox(_topOutputBox, raw => ApplyState(_state.WithTopOutput(new Measure(raw))));
        HookBox(_topTensionBox, raw => ApplyState(_state.WithTopTension(new Measure(raw))));
        HookBox(_bottomInputBox, raw => ApplyState(_state.WithBottomInput(new Measure(raw))));
        HookBox(_bottomTensionBox, raw => ApplyState(_state.WithBottomTension(new Measure(raw))));

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
                ProbeUi.SliderCard("Top Tension", "Residual tension on the upward branch.", _topTensionSlider, _topTensionBox),
                ProbeUi.SliderCard("Bottom Input", "Recessive orthogonal input.", _bottomInputSlider, _bottomInputBox),
                ProbeUi.SliderCard("Bottom Tension", "Residual tension on the incoming branch.", _bottomTensionSlider, _bottomTensionBox)
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
                ProbeUi.DisplayTitle("Orthogonal input", 18d),
                ProbeUi.Body("The same site can publish upward while a separate orthogonal input arrives from below.", 10d)
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

    private void ApplyIfLive(OrthogonalInputDemoState state)
    {
        if (!_syncing)
        {
            ApplyState(state);
        }
    }

    private void ApplyState(OrthogonalInputDemoState state)
    {
        _state = Normalize(state);
        _scene.State = _state;
        SyncUiFromState();
    }

    private void SyncUiFromState()
    {
        _syncing = true;
        _topOutputSlider.Minimum = 0;
        _topOutputSlider.Maximum = MaximumOutput;
        _bottomInputSlider.Minimum = 0;
        _bottomInputSlider.Maximum = MaximumInput;
        _topTensionSlider.Minimum = MinimumTension;
        _topTensionSlider.Maximum = MaximumTension;
        _bottomTensionSlider.Minimum = MinimumTension;
        _bottomTensionSlider.Maximum = MaximumTension;

        _topOutputSlider.Value = _state.TopOutput.Raw;
        _topTensionSlider.Value = _state.TopTension.Raw;
        _bottomInputSlider.Value = _state.BottomInput.Raw;
        _bottomTensionSlider.Value = _state.BottomTension.Raw;

        _topOutputBox.Text = _state.TopOutput.Raw.ToString();
        _topTensionBox.Text = _state.TopTension.Raw.ToString();
        _bottomInputBox.Text = _state.BottomInput.Raw.ToString();
        _bottomTensionBox.Text = _state.BottomTension.Raw.ToString();
        _syncing = false;

        var settlement = _state.Settlement;
        _sceneReadingText.Text = _state.ComplexReading;
        _sceneOriginText.Text = $"origin {_state.Origin.Raw}";
        _sceneSettledText.Text = $"settled {settlement.Settled.Normalize()}";

        _freezeText.Text =
            $"Origin                    {_state.Origin.Raw}\n" +
            $"Dominant                  0 -> {_state.Span.Raw}\n" +
            $"Recessive                 0 -> -{_state.Span.Raw}\n" +
            "Structure                 fused segment + one input";

        _readingCardText.Text =
            $"Complex                   {_state.ComplexReading}\n" +
            $"Dominant                  {_state.DominantReading}\n" +
            $"Recessive                 {_state.RecessiveReading}\n" +
            $"Outgoing                  {_state.TopBranch.Signal.Value.Normalize()}\n" +
            $"Incoming                  {_state.BottomBranch.Signal.Value.Normalize()}";

        _orthogonalText.Text =
            $"Residual                  {_state.Orthogonal.Residual.Normalize()}\n" +
            $"Outgoing                  {_state.TopBranch.Tension.Normalize()}\n" +
            $"Incoming                  {_state.BottomBranch.Tension.Normalize()}\n" +
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

    private static OrthogonalInputDemoState Normalize(OrthogonalInputDemoState state) =>
        state
            .WithSpan(new Measure(Math.Clamp(state.Span.Raw, 1L, MaximumSpan)))
            .WithTopOutput(new Measure(Math.Clamp(state.TopOutput.Raw, 0L, MaximumOutput)))
            .WithBottomInput(new Measure(Math.Clamp(state.BottomInput.Raw, 0L, MaximumInput)))
            .WithTopTension(new Measure(Math.Clamp(state.TopTension.Raw, MinimumTension, MaximumTension)))
            .WithBottomTension(new Measure(Math.Clamp(state.BottomTension.Raw, MinimumTension, MaximumTension)));
}
