using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;
using Core4.Dynamics;
using Core4.Numerics;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// Topology/runtime page showing the first explicit split between local self-return and shared-carrier transport.
/// </summary>
public sealed class RoutingStiffnessPageView : UserControl
{
    private readonly RoutingStiffnessSceneControl _scene;
    private readonly MeasureSliderControl _roundSlider;
    private readonly MeasureSliderControl _sourceInputSlider;
    private readonly MeasureSliderControl _sourceStiffnessSlider;
    private readonly MeasureSliderControl _sourceRoutingSlider;
    private readonly MeasureSliderControl _sourceDominantSlider;
    private readonly MeasureSliderControl _sinkStiffnessSlider;
    private readonly MeasureSliderControl _sinkRoutingSlider;
    private readonly MeasureSliderControl _sinkDominantSlider;
    private readonly TextBox _roundBox;
    private readonly TextBox _sourceInputBox;
    private readonly TextBox _sourceStiffnessBox;
    private readonly TextBox _sourceRoutingBox;
    private readonly TextBox _sourceDominantBox;
    private readonly TextBox _sinkStiffnessBox;
    private readonly TextBox _sinkRoutingBox;
    private readonly TextBox _sinkDominantBox;
    private readonly TextBlock _metaRoundText;
    private readonly TextBlock _metaFieldText;
    private readonly TextBlock _metaTransferText;
    private readonly TextBlock _sourceLocalText;
    private readonly TextBlock _carrierFieldText;
    private readonly TextBlock _sinkLocalText;
    private readonly TextBlock _timingText;
    private RoutingStiffnessDemoState _state = RoutingStiffnessDemoState.Default;
    private bool _syncing;

    public RoutingStiffnessPageView()
    {
        _scene = new RoutingStiffnessSceneControl
        {
            State = _state,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        _roundSlider = ProbeUi.CreateSlider(0, RoutingStiffnessDemoState.MaxRound, _state.CurrentRound.Raw, ProbePalette.Overlay, true);
        _sourceInputSlider = ProbeUi.CreateSlider(0, RoutingStiffnessDemoState.MaxInput, _state.SourceInput.Raw, ProbePalette.Orthogonal, false);
        _sourceStiffnessSlider = ProbeUi.CreateSlider(0, RoutingStiffnessDemoState.MaxPercent, _state.SourceStiffnessPercent.Raw, ProbePalette.Origin, true);
        _sourceRoutingSlider = ProbeUi.CreateSlider(0, RoutingStiffnessDemoState.MaxPercent, _state.SourceRoutingPercent.Raw, ProbePalette.TensionPositive, false);
        _sourceDominantSlider = ProbeUi.CreateSlider(0, RoutingStiffnessDemoState.MaxPercent, _state.SourceDominantPercent.Raw, ProbePalette.Dominant, true);
        _sinkStiffnessSlider = ProbeUi.CreateSlider(0, RoutingStiffnessDemoState.MaxPercent, _state.SinkStiffnessPercent.Raw, ProbePalette.Origin, true);
        _sinkRoutingSlider = ProbeUi.CreateSlider(0, RoutingStiffnessDemoState.MaxPercent, _state.SinkRoutingPercent.Raw, ProbePalette.TensionPositive, false);
        _sinkDominantSlider = ProbeUi.CreateSlider(0, RoutingStiffnessDemoState.MaxPercent, _state.SinkDominantPercent.Raw, ProbePalette.OrthogonalInput, true);

        _roundBox = ProbeUi.CreateNumberBox(_state.CurrentRound.Raw.ToString());
        _sourceInputBox = ProbeUi.CreateNumberBox(_state.SourceInput.Raw.ToString());
        _sourceStiffnessBox = ProbeUi.CreateNumberBox(_state.SourceStiffnessPercent.Raw.ToString());
        _sourceRoutingBox = ProbeUi.CreateNumberBox(_state.SourceRoutingPercent.Raw.ToString());
        _sourceDominantBox = ProbeUi.CreateNumberBox(_state.SourceDominantPercent.Raw.ToString());
        _sinkStiffnessBox = ProbeUi.CreateNumberBox(_state.SinkStiffnessPercent.Raw.ToString());
        _sinkRoutingBox = ProbeUi.CreateNumberBox(_state.SinkRoutingPercent.Raw.ToString());
        _sinkDominantBox = ProbeUi.CreateNumberBox(_state.SinkDominantPercent.Raw.ToString());

        _roundSlider.ValueChanged += (_, value) => ApplyIfLive(() => _state.WithRound(new Measure(value)));
        _sourceInputSlider.ValueChanged += (_, value) => ApplyIfLive(() => _state.WithSourceInput(new Measure(value)));
        _sourceStiffnessSlider.ValueChanged += (_, value) => ApplyIfLive(() => _state.WithSourceStiffnessPercent(new Measure(value)));
        _sourceRoutingSlider.ValueChanged += (_, value) => ApplyIfLive(() => _state.WithSourceRoutingPercent(new Measure(value)));
        _sourceDominantSlider.ValueChanged += (_, value) => ApplyIfLive(() => _state.WithSourceDominantPercent(new Measure(value)));
        _sinkStiffnessSlider.ValueChanged += (_, value) => ApplyIfLive(() => _state.WithSinkStiffnessPercent(new Measure(value)));
        _sinkRoutingSlider.ValueChanged += (_, value) => ApplyIfLive(() => _state.WithSinkRoutingPercent(new Measure(value)));
        _sinkDominantSlider.ValueChanged += (_, value) => ApplyIfLive(() => _state.WithSinkDominantPercent(new Measure(value)));

        HookBox(_roundBox, raw => ApplyState(_state.WithRound(new Measure(raw))));
        HookBox(_sourceInputBox, raw => ApplyState(_state.WithSourceInput(new Measure(raw))));
        HookBox(_sourceStiffnessBox, raw => ApplyState(_state.WithSourceStiffnessPercent(new Measure(raw))));
        HookBox(_sourceRoutingBox, raw => ApplyState(_state.WithSourceRoutingPercent(new Measure(raw))));
        HookBox(_sourceDominantBox, raw => ApplyState(_state.WithSourceDominantPercent(new Measure(raw))));
        HookBox(_sinkStiffnessBox, raw => ApplyState(_state.WithSinkStiffnessPercent(new Measure(raw))));
        HookBox(_sinkRoutingBox, raw => ApplyState(_state.WithSinkRoutingPercent(new Measure(raw))));
        HookBox(_sinkDominantBox, raw => ApplyState(_state.WithSinkDominantPercent(new Measure(raw))));

        _metaRoundText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _metaFieldText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _metaTransferText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _sourceLocalText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);
        _carrierFieldText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);
        _sinkLocalText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);
        _timingText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);

        var resetButton = ProbeUi.PillButton("Reset Demo");
        resetButton.Click += (_, _) => ApplyState(RoutingStiffnessDemoState.Default);

        var leftColumn = new StackPanel
        {
            Width = 212,
            Spacing = 6,
            Children =
            {
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 6,
                    Children =
                    {
                        ProbeUi.Mono("ROUND CONTROL", 10d, ProbePalette.Muted),
                        ProbeUi.Body("Round 0 is the seed. Round 1 shows same-round source settling plus carrier publish. Round 2 is where the listener first absorbs what the field delivered.", 9.5d),
                        SliderRow("Round", _roundSlider, _roundBox),
                        resetButton
                    }
                }, 20, new Thickness(12, 10)),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 6,
                    Children =
                    {
                        ProbeUi.Mono("SOURCE SITE", 10d, ProbePalette.Muted),
                        ProbeUi.Body("The source starts with raw orthogonal residual and can self-return part of it before publishing the remainder onto the shared carrier.", 9.5d),
                        SliderRow("Input", _sourceInputSlider, _sourceInputBox),
                        SliderRow("Stiff", _sourceStiffnessSlider, _sourceStiffnessBox),
                        SliderRow("Route", _sourceRoutingSlider, _sourceRoutingBox),
                        SliderRow("To claim", _sourceDominantSlider, _sourceDominantBox)
                    }
                }, 20, new Thickness(12, 10)),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 6,
                    Children =
                    {
                        ProbeUi.Mono("LISTENER SITE", 10d, ProbePalette.Muted),
                        ProbeUi.Body("The listener knows only its own claim machine and a shared-carrier port. It does not receive direct carrier-state writes from the source.", 9.5d),
                        SliderRow("Stiff", _sinkStiffnessSlider, _sinkStiffnessBox),
                        SliderRow("Route", _sinkRoutingSlider, _sinkRoutingBox),
                        SliderRow("To claim", _sinkDominantSlider, _sinkDominantBox)
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

        var infoCards = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*,*"),
            ColumnSpacing = 10
        };
        ProbeUi.AddInfoCard(infoCards, 0, "SOURCE LOCAL", _sourceLocalText);
        ProbeUi.AddInfoCard(infoCards, 1, "CARRIER FIELD", _carrierFieldText);
        ProbeUi.AddInfoCard(infoCards, 2, "LISTENER LOCAL", _sinkLocalText);
        ProbeUi.AddInfoCard(infoCards, 3, "TIMING", _timingText);

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
            ColumnDefinitions = new ColumnDefinitions("212,*"),
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
                ProbeUi.Mono("RUNTIME SCENE", 10d, ProbePalette.Muted),
                ProbeUi.DisplayTitle("Routing / Stiffness", 18d),
                ProbeUi.Body("One site self-returns tension locally, then the shared carrier transports only the orthogonal remainder for next-round absorption elsewhere.", 10d)
            }
        };

        var metaStack = new StackPanel
        {
            Spacing = 2,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children =
            {
                _metaRoundText,
                _metaFieldText,
                _metaTransferText
            }
        };

        grid.Children.Add(titleStack);
        grid.Children.Add(metaStack);
        Grid.SetColumn(metaStack, 1);
        return grid;
    }

    private void ApplyIfLive(Func<RoutingStiffnessDemoState> next)
    {
        if (_syncing)
        {
            return;
        }

        ApplyState(next());
    }

    private void ApplyState(RoutingStiffnessDemoState state)
    {
        _state = state;
        _scene.State = _state;
        SyncUiFromState();
    }

    private void SyncUiFromState()
    {
        _syncing = true;
        _roundSlider.Value = _state.CurrentRound.Raw;
        _sourceInputSlider.Value = _state.SourceInput.Raw;
        _sourceStiffnessSlider.Value = _state.SourceStiffnessPercent.Raw;
        _sourceRoutingSlider.Value = _state.SourceRoutingPercent.Raw;
        _sourceDominantSlider.Value = _state.SourceDominantPercent.Raw;
        _sinkStiffnessSlider.Value = _state.SinkStiffnessPercent.Raw;
        _sinkRoutingSlider.Value = _state.SinkRoutingPercent.Raw;
        _sinkDominantSlider.Value = _state.SinkDominantPercent.Raw;
        _roundBox.Text = _state.CurrentRound.Raw.ToString();
        _sourceInputBox.Text = _state.SourceInput.Raw.ToString();
        _sourceStiffnessBox.Text = _state.SourceStiffnessPercent.Raw.ToString();
        _sourceRoutingBox.Text = _state.SourceRoutingPercent.Raw.ToString();
        _sourceDominantBox.Text = _state.SourceDominantPercent.Raw.ToString();
        _sinkStiffnessBox.Text = _state.SinkStiffnessPercent.Raw.ToString();
        _sinkRoutingBox.Text = _state.SinkRoutingPercent.Raw.ToString();
        _sinkDominantBox.Text = _state.SinkDominantPercent.Raw.ToString();
        _syncing = false;

        var field = _state.CurrentField;
        _metaRoundText.Text = $"round {_state.CurrentRound.Raw}";
        _metaFieldText.Text = field is { } currentField
            ? $"field pub {currentField.Published.Normalize()}"
            : "field pub 0/1";
        _metaTransferText.Text = $"max transfer {_state.CurrentState.MaxTransfer.Normalize()}";

        _sourceLocalText.Text = DescribeLocal(
            "source",
            _state.CurrentSourceSite,
            _state.CurrentSourceSettlement,
            _state.SourceStiffness,
            _state.SourceRoutingGain,
            _state.SourceDominantBias,
            _state.SourceAbsorbed);
        _carrierFieldText.Text = DescribeField(_state, field);
        _sinkLocalText.Text = DescribeLocal(
            "listener",
            _state.CurrentSinkSite,
            _state.CurrentSinkSettlement,
            _state.SinkStiffness,
            _state.SinkRoutingGain,
            _state.SinkDominantBias,
            _state.SinkAbsorbed);
        _timingText.Text =
            $"round                    {_state.CurrentRound.Raw}\n" +
            "law                      local -> carrier -> next local\n" +
            $"source residual          {_state.CurrentSourceSite.Orthogonal.Residual.Normalize()}\n" +
            $"listener residual        {_state.CurrentSinkSite.Orthogonal.Residual.Normalize()}\n" +
            $"note                     {_state.RoundDescription}";
    }

    private static string DescribeLocal(
        string label,
        Core4.Structure.Site site,
        SiteSettlement? settlement,
        Ratio stiffness,
        Ratio routingGain,
        Ratio dominantBias,
        Ratio absorbed)
    {
        if (settlement is not { } currentSettlement)
        {
            return
                $"{label}                    seed only\n" +
                $"stiffness                {stiffness.Normalize()}\n" +
                $"routing gain             {routingGain.Normalize()}\n" +
                $"dominant bias            {dominantBias.Normalize()}\n" +
                $"support                  {site.Kernel.Recessive.Normalize()}\n" +
                $"claim                    {site.Kernel.Dominant.Normalize()}\n" +
                $"residual                 {site.Orthogonal.Residual.Normalize()}";
        }

        return
            $"{label}                    settled\n" +
            $"stiffness                {stiffness.Normalize()}\n" +
            $"routing gain             {routingGain.Normalize()}\n" +
            $"dominant bias            {dominantBias.Normalize()}\n" +
            $"incoming                 {currentSettlement.Incoming.Normalize()}\n" +
            $"absorbed                 {absorbed.Normalize()}\n" +
            $"support                  {site.Kernel.Recessive.Normalize()}\n" +
            $"claim                    {site.Kernel.Dominant.Normalize()}\n" +
            $"remainder                {currentSettlement.Remainder.Normalize()}";
    }

    private static string DescribeField(RoutingStiffnessDemoState state, CarrierFieldState? field)
    {
        if (field is not { } currentField)
        {
            return
                "carrier                  p\n" +
                "published                0/1\n" +
                "delivered                0/1\n" +
                "retained                 0/1\n" +
                "transfers                0";
        }

        return
            $"carrier                  p ({currentField.Carrier.Id})\n" +
            $"published                {currentField.Published.Normalize()}\n" +
            $"delivered                {currentField.Delivered.Normalize()}\n" +
            $"retained                 {currentField.Retained.Normalize()}\n" +
            $"emit/listen weight       {currentField.EmitterWeight.Raw} / {currentField.ListenerWeight.Raw}\n" +
            $"transfers                {currentField.TransferCount.Raw}";
    }

    private static Control SliderRow(string label, MeasureSliderControl slider, TextBox inputBox)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("58,*,52"),
            ColumnSpacing = 6,
            Children =
            {
                ProbeUi.Mono(label, 10d, ProbePalette.Ink),
                slider,
                ProbeUi.InputHost(inputBox)
            }
        };

        Grid.SetColumn(slider, 1);
        Grid.SetColumn(grid.Children[2], 2);
        return grid;
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
}
