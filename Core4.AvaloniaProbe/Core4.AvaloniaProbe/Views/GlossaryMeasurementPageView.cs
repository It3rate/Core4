using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// First measurement glossary page: the unit chooses a denominator, incoming values negotiate against it, and the
/// surviving ratio becomes a placement on the carrier.
/// </summary>
public sealed class GlossaryMeasurementPageView : UserControl
{
    private static readonly ProbeGlossary.GlossaryEntry UnitEntry = FindEntry("Unit");
    private static readonly ProbeGlossary.GlossaryEntry RatioEntry = FindEntry("Ratio");
    private static readonly ProbeGlossary.GlossaryEntry PlacementEntry = FindEntry("Placement");
    private static readonly ProbeGlossary.GlossaryEntry ResolutionEntry = FindEntry("Resolution");

    private readonly GlossaryMeasurementSceneControl _scene;
    private readonly TextBlock _metaPhaseText;
    private readonly TextBlock _metaDecisionText;
    private readonly TextBlock _controlBody;
    private readonly TextBlock _resolutionText;
    private readonly MeasureSliderControl _resolutionSlider;
    private GlossaryMeasurementDemoState _state = GlossaryMeasurementDemoState.CreateDefault();
    private bool _syncing;

    public GlossaryMeasurementPageView()
    {
        _scene = new GlossaryMeasurementSceneControl
        {
            State = _state,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };
        _scene.StateChanged += (_, state) =>
        {
            _state = state;
            SyncUi();
        };

        _metaPhaseText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _metaDecisionText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _controlBody = ProbeUi.Body(string.Empty, 10d);
        _resolutionText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Muted);

        _resolutionSlider = ProbeUi.CreateSlider(
            GlossaryMeasurementDemoState.MinimumUnitTicks,
            GlossaryMeasurementDemoState.MaximumUnitTicks,
            _state.UnitTicks.Raw,
            ProbePalette.Dominant,
            hollow: true);
        _resolutionSlider.ValueChanged += (_, value) =>
        {
            if (_syncing)
            {
                return;
            }

            ApplyState(_state.WithUnitTicks(value));
        };

        var resetButton = ProbeUi.PillButton("Reset Scene");
        resetButton.Click += (_, _) => ApplyState(_state.Reset());

        var leftColumn = new StackPanel
        {
            Width = 232,
            Spacing = 6,
            Children =
            {
                EntryCard(UnitEntry),
                EntryCard(RatioEntry),
                EntryCard(PlacementEntry),
                EntryCard(ResolutionEntry),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        resetButton
                    }
                }, 20d, new Thickness(12, 10))
            }
        };

        var sceneCard = ProbeUi.Card(new StackPanel
        {
            Spacing = 6,
            Children =
            {
                SceneHeader(),
                _scene
            }
        }, 22d, new Thickness(12));

        var controlCard = ProbeUi.Card(new StackPanel
        {
            Spacing = 8,
            Children =
            {
                ProbeUi.Header("Choose Unit Resolution", 12d),
                _controlBody,
                _resolutionText,
                _resolutionSlider
            }
        }, 20d, new Thickness(12, 10));

        var rightColumn = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                sceneCard,
                controlCard
            }
        };

        var mainGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("232,*"),
            ColumnSpacing = 8,
            Children =
            {
                leftColumn,
                rightColumn
            }
        };
        Grid.SetColumn(rightColumn, 1);

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
                ProbeUi.DisplayTitle("Unit Resolution Negotiates Placement", 18d),
                ProbeUi.Body("Click a floating raw value and let it attach to the unit. The raw value arrives with a denser internal lattice, but the unit's chosen denominator wins, so the surviving ratio becomes the placement we keep.", 10d)
            }
        };

        var metaStack = new StackPanel
        {
            Spacing = 2,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Children =
            {
                _metaPhaseText,
                _metaDecisionText
            }
        };

        grid.Children.Add(titleStack);
        grid.Children.Add(metaStack);
        Grid.SetColumn(metaStack, 1);
        return grid;
    }

    private void ApplyState(GlossaryMeasurementDemoState state)
    {
        _state = state;
        _scene.State = state;
        SyncUi();
    }

    private void SyncUi()
    {
        _syncing = true;
        _resolutionSlider.Value = _state.UnitTicks.Raw;
        _metaPhaseText.Text = _state.IsAnimating ? _state.PhaseText : $"{_state.AvailableValues.Length} floating values";
        _metaDecisionText.Text = _state.IsAnimating
            ? $"unit { _state.UnitTicks.Raw } ticks wins"
            : $"{_state.AttachedValues.Length} values pinned at origin";

        _resolutionText.Text = $"unit 1 = {_state.UnitTicks.Raw} ticks   |   step size = 1/{_state.UnitTicks.Raw}";
        _controlBody.Text = _state.IsAnimating
            ? $"The incoming value keeps its own dense tick story for a moment, creating visible tension. As the negotiation settles, the unit's denominator {_state.UnitTicks.Raw} takes over and the measured placement snaps to {_state.DecisionText}."
            : "Move the slider to change how many ticks the unit calls 1 while keeping the unit's length fixed. Then click any floating raw fraction above to watch its finer lattice negotiate against the unit and resolve into a placement.";
        _syncing = false;
    }

    private static Border EntryCard(ProbeGlossary.GlossaryEntry entry)
        => ProbeUi.Card(new StackPanel
        {
            Spacing = 4,
            Children =
            {
                ProbeUi.Header(entry.Term, 12d),
                ProbeUi.Mono(entry.Summary, 10d, ProbePalette.Muted),
                ProbeUi.Body(entry.Description, 10d)
            }
        }, 20d, new Thickness(12, 10));

    private static ProbeGlossary.GlossaryEntry FindEntry(string term)
    {
        foreach (var section in ProbeGlossary.Sections)
        {
            var match = section.Entries.FirstOrDefault(entry => string.Equals(entry.Term, term, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(match.Term))
            {
                return match;
            }
        }

        return ProbeGlossary.Sections[0].Entries[0];
    }
}
