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
/// The multiple-input explorer page. Several bottom-side inputs branch into one site while the aligned segment
/// stays fused and simple.
/// </summary>
public sealed class MultipleInputsPageView : UserControl
{
    private const long MaximumSpan = 16L;
    private const long MaximumOutput = 14L;
    private const long MaximumInput = 14L;
    private const long MinimumTension = -22L;
    private const long MaximumTension = 20L;

    private readonly MultipleInputsSceneControl _scene;
    private readonly MeasureSliderControl _topOutputSlider;
    private readonly MeasureSliderControl _topTensionSlider;
    private readonly MeasureSliderControl _bottomInputASlider;
    private readonly MeasureSliderControl _bottomTensionASlider;
    private readonly MeasureSliderControl _bottomInputBSlider;
    private readonly MeasureSliderControl _bottomTensionBSlider;
    private readonly TextBox _topOutputBox;
    private readonly TextBox _topTensionBox;
    private readonly TextBox _bottomInputABox;
    private readonly TextBox _bottomTensionABox;
    private readonly TextBox _bottomInputBBox;
    private readonly TextBox _bottomTensionBBox;
    private readonly TextBlock _sceneReadingText;
    private readonly TextBlock _sceneOriginText;
    private readonly TextBlock _sceneSettledText;
    private readonly TextBlock _freezeText;
    private readonly TextBlock _readingCardText;
    private readonly TextBlock _orthogonalText;
    private readonly TextBlock _settlementText;
    private MultipleInputsDemoState _state = MultipleInputsDemoState.Default;
    private bool _syncing;

    public MultipleInputsPageView()
    {
        _scene = new MultipleInputsSceneControl
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
        _bottomInputASlider = ProbeUi.CreateSlider(0, MaximumInput, _state.BottomInputA.Raw, ProbePalette.OrthogonalInput, true);
        _bottomTensionASlider = ProbeUi.CreateSlider(MinimumTension, MaximumTension, _state.BottomTensionA.Raw, ProbePalette.TensionPositive, false);
        _bottomInputBSlider = ProbeUi.CreateSlider(0, MaximumInput, _state.BottomInputB.Raw, ProbePalette.OrthogonalInput, true);
        _bottomTensionBSlider = ProbeUi.CreateSlider(MinimumTension, MaximumTension, _state.BottomTensionB.Raw, ProbePalette.TensionPositive, false);

        _topOutputSlider.ValueChanged += (_, value) => ApplyIfLive(_state.WithTopOutput(new Measure(value)));
        _topTensionSlider.ValueChanged += (_, value) => ApplyIfLive(_state.WithTopTension(new Measure(value)));
        _bottomInputASlider.ValueChanged += (_, value) => ApplyIfLive(_state.WithBottomInputA(new Measure(value)));
        _bottomTensionASlider.ValueChanged += (_, value) => ApplyIfLive(_state.WithBottomTensionA(new Measure(value)));
        _bottomInputBSlider.ValueChanged += (_, value) => ApplyIfLive(_state.WithBottomInputB(new Measure(value)));
        _bottomTensionBSlider.ValueChanged += (_, value) => ApplyIfLive(_state.WithBottomTensionB(new Measure(value)));

        _topOutputBox = ProbeUi.CreateNumberBox(_state.TopOutput.Raw.ToString());
        _topTensionBox = ProbeUi.CreateNumberBox(_state.TopTension.Raw.ToString());
        _bottomInputABox = ProbeUi.CreateNumberBox(_state.BottomInputA.Raw.ToString());
        _bottomTensionABox = ProbeUi.CreateNumberBox(_state.BottomTensionA.Raw.ToString());
        _bottomInputBBox = ProbeUi.CreateNumberBox(_state.BottomInputB.Raw.ToString());
        _bottomTensionBBox = ProbeUi.CreateNumberBox(_state.BottomTensionB.Raw.ToString());

        HookBox(_topOutputBox, raw => ApplyState(_state.WithTopOutput(new Measure(raw))));
        HookBox(_topTensionBox, raw => ApplyState(_state.WithTopTension(new Measure(raw))));
        HookBox(_bottomInputABox, raw => ApplyState(_state.WithBottomInputA(new Measure(raw))));
        HookBox(_bottomTensionABox, raw => ApplyState(_state.WithBottomTensionA(new Measure(raw))));
        HookBox(_bottomInputBBox, raw => ApplyState(_state.WithBottomInputB(new Measure(raw))));
        HookBox(_bottomTensionBBox, raw => ApplyState(_state.WithBottomTensionB(new Measure(raw))));

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
                ProbeUi.SliderCard("Bottom Input A", "First recessive orthogonal input.", _bottomInputASlider, _bottomInputABox),
                ProbeUi.SliderCard("Bottom Tension A", "Residual tension on the first incoming branch.", _bottomTensionASlider, _bottomTensionABox),
                ProbeUi.SliderCard("Bottom Input B", "Second recessive orthogonal input.", _bottomInputBSlider, _bottomInputBBox),
                ProbeUi.SliderCard("Bottom Tension B", "Residual tension on the second incoming branch.", _bottomTensionBSlider, _bottomTensionBBox)
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
                ProbeUi.DisplayTitle("Multiple inputs", 18d),
                ProbeUi.Body("Several bottom-side inputs can branch into the same site while the aligned segment stays fused.", 10d)
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

    private void ApplyIfLive(MultipleInputsDemoState state)
    {
        if (!_syncing)
        {
            ApplyState(state);
        }
    }

    private void ApplyState(MultipleInputsDemoState state)
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
        _topTensionSlider.Minimum = MinimumTension;
        _topTensionSlider.Maximum = MaximumTension;
        _bottomInputASlider.Minimum = 0;
        _bottomInputASlider.Maximum = MaximumInput;
        _bottomTensionASlider.Minimum = MinimumTension;
        _bottomTensionASlider.Maximum = MaximumTension;
        _bottomInputBSlider.Minimum = 0;
        _bottomInputBSlider.Maximum = MaximumInput;
        _bottomTensionBSlider.Minimum = MinimumTension;
        _bottomTensionBSlider.Maximum = MaximumTension;

        _topOutputSlider.Value = _state.TopOutput.Raw;
        _topTensionSlider.Value = _state.TopTension.Raw;
        _bottomInputASlider.Value = _state.BottomInputA.Raw;
        _bottomTensionASlider.Value = _state.BottomTensionA.Raw;
        _bottomInputBSlider.Value = _state.BottomInputB.Raw;
        _bottomTensionBSlider.Value = _state.BottomTensionB.Raw;

        _topOutputBox.Text = _state.TopOutput.Raw.ToString();
        _topTensionBox.Text = _state.TopTension.Raw.ToString();
        _bottomInputABox.Text = _state.BottomInputA.Raw.ToString();
        _bottomTensionABox.Text = _state.BottomTensionA.Raw.ToString();
        _bottomInputBBox.Text = _state.BottomInputB.Raw.ToString();
        _bottomTensionBBox.Text = _state.BottomTensionB.Raw.ToString();
        _syncing = false;

        var settlement = _state.Settlement;
        _sceneReadingText.Text = _state.ComplexReading;
        _sceneOriginText.Text = $"origin {_state.Origin.Raw}";
        _sceneSettledText.Text = $"settled {settlement.Settled.Normalize()}";

        _freezeText.Text =
            $"Origin                    {_state.Origin.Raw}\n" +
            $"Dominant                  0 -> {_state.Span.Raw}\n" +
            $"Recessive                 0 -> -{_state.Span.Raw}\n" +
            "Structure                 fused segment + multi input";

        _readingCardText.Text =
            $"Complex                   {_state.ComplexReading}\n" +
            $"Dominant                  {_state.DominantReading}\n" +
            $"Recessive                 {_state.RecessiveReading}\n" +
            $"Outgoing                  {_state.TopBranch.Signal.Value.Normalize()}\n" +
            $"Approx                    {_state.ApproximateReading}";

        _orthogonalText.Text =
            $"Residual                  {_state.Orthogonal.Residual.Normalize()}\n" +
            $"Out                       {_state.TopBranch.Tension.Normalize()}\n" +
            $"In A                      {_state.BottomBranchA.Tension.Normalize()}\n" +
            $"In B                      {_state.BottomBranchB.Tension.Normalize()}\n" +
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

    private static MultipleInputsDemoState Normalize(MultipleInputsDemoState state) =>
        state
            .WithSpan(new Measure(Math.Clamp(state.Span.Raw, 1L, MaximumSpan)))
            .WithTopOutput(new Measure(Math.Clamp(state.TopOutput.Raw, 0L, MaximumOutput)))
            .WithTopTension(new Measure(Math.Clamp(state.TopTension.Raw, MinimumTension, MaximumTension)))
            .WithBottomInputA(new Measure(Math.Clamp(state.BottomInputA.Raw, 0L, MaximumInput)))
            .WithBottomTensionA(new Measure(Math.Clamp(state.BottomTensionA.Raw, MinimumTension, MaximumTension)))
            .WithBottomInputB(new Measure(Math.Clamp(state.BottomInputB.Raw, 0L, MaximumInput)))
            .WithBottomTensionB(new Measure(Math.Clamp(state.BottomTensionB.Raw, MinimumTension, MaximumTension)));
}
