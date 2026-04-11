using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// Next glossary page: one signed unit crosses positive, orthogonal, and negative interpretation while detached
/// values preserve whichever orientation they had when clicked.
/// </summary>
public sealed class GlossaryTensionPageView : UserControl
{
    private static readonly ProbeGlossary.GlossaryEntry TensionEntry = FindEntry("Tension");
    private static readonly ProbeGlossary.GlossaryEntry FusionEntry = FindEntry("Fusion");
    private static readonly ProbeGlossary.GlossaryEntry OrthogonalEntry = FindEntry("Orthogonal");

    private readonly GlossaryTensionSceneControl _scene;
    private readonly TextBlock _metaStateText;
    private readonly TextBlock _metaHintText;
    private readonly TextBlock _bodyText;
    private GlossaryTensionDemoState _state = GlossaryTensionDemoState.CreateDefault();

    public GlossaryTensionPageView()
    {
        _scene = new GlossaryTensionSceneControl
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

        _metaStateText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _metaHintText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _bodyText = ProbeUi.Body(string.Empty, 10d);

        var resetButton = ProbeUi.PillButton("Reset Scene");
        resetButton.Click += (_, _) => ApplyState(_state.Reset());

        var leftColumn = new StackPanel
        {
            Width = 232,
            Spacing = 6,
            Children =
            {
                EntryCard(TensionEntry),
                EntryCard(FusionEntry),
                EntryCard(OrthogonalEntry),
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

        var noteCard = ProbeUi.Card(new StackPanel
        {
            Spacing = 6,
            Children =
            {
                ProbeUi.Header("How To Read This", 12d),
                _bodyText
            }
        }, 20d, new Thickness(12, 10));

        var rightColumn = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                sceneCard,
                noteCard
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
                ProbeUi.DisplayTitle("Tension, Fusion, And Orthogonal Drift", 18d),
                ProbeUi.Body("Drag the blue unit handle left or right. The same values stay numerically stable against that unit, but their visible direction changes: aligned when the unit is positive or negative, and orthogonal when the unit reaches 0.", 10d)
            }
        };

        var metaStack = new StackPanel
        {
            Spacing = 2,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Children =
            {
                _metaStateText,
                _metaHintText
            }
        };

        grid.Children.Add(titleStack);
        grid.Children.Add(metaStack);
        Grid.SetColumn(metaStack, 1);
        return grid;
    }

    private void ApplyState(GlossaryTensionDemoState state)
    {
        _state = state;
        _scene.State = state;
        SyncUi();
    }

    private void SyncUi()
    {
        _metaStateText.Text = _state.StateText;
        _metaHintText.Text = $"{_state.Values.Length} live values | {_state.Detached.Length} detached";
        _bodyText.Text = _state.IsOrthogonal
            ? "At unit 0 the current carrier can no longer explain forward or reverse continuation, so the values stand off orthogonally instead. Clicking any value now preserves it as an orthogonal reading in the lower grid."
            : _state.UnitCarrierTicks.Raw > 0L
                ? "Positive unit means the carrier reads in the familiar direction: dominant to the right and recessive to the left. Click any value to preserve it as a detached aligned reading."
                : "Negative unit keeps value 1, but flips which horizontal side counts as positive. The values do not change numerically; only the carrier's interpretation changes.";
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
