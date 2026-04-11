using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// Pair-fusion glossary page. Two chosen aligned/orthogonal elements harden into a reusable fused result.
/// </summary>
public sealed class GlossaryFusionPageView : UserControl
{
    private static readonly ProbeGlossary.GlossaryEntry FusionEntry = FindEntry("Fusion");
    private static readonly ProbeGlossary.GlossaryEntry DirectedSegmentEntry = FindEntry("Directed Segment");
    private static readonly ProbeGlossary.GlossaryEntry OrthogonalPairEntry = FindEntry("Orthogonal Pair");
    private static readonly ProbeGlossary.GlossaryEntry SelfPairEntry = FindEntry("Self Pair");

    private readonly GlossaryFusionSceneControl _scene;
    private readonly TextBlock _bodyText;
    private GlossaryFusionDemoState _state = GlossaryFusionDemoState.CreateDefault();

    public GlossaryFusionPageView()
    {
        _scene = new GlossaryFusionSceneControl
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

        _bodyText = ProbeUi.Body(string.Empty, 10d);

        var resetButton = ProbeUi.PillButton("Reset Scene");
        resetButton.Click += (_, _) => ApplyState(_state.Reset());

        var leftColumn = new StackPanel
        {
            Width = 232,
            Spacing = 6,
            Children =
            {
                EntryCard(FusionEntry),
                EntryCard(DirectedSegmentEntry),
                EntryCard(OrthogonalPairEntry),
                EntryCard(SelfPairEntry),
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
        => new Grid
        {
            Children =
            {
                new StackPanel
                {
                    Spacing = 2,
                    Children =
                    {
                        ProbeUi.Mono("INTERACTIVE SCENE", 10d, ProbePalette.Muted),
                        ProbeUi.DisplayTitle("Pair Two Elements And Fuse A Basis", 18d),
                        ProbeUi.Body("Pick any two of the four source elements on the right. They stage, negotiate, and then harden into a fused result: a self pair, a directed segment, an area, or a complex-style ray.", 10d)
                    }
                }
            }
        };

    private void ApplyState(GlossaryFusionDemoState state)
    {
        _state = state;
        _scene.State = state;
        SyncUi();
    }

    private void SyncUi()
    {
        if (_state.IsAnimating)
        {
            _bodyText.Text = "The first element stages, the second joins it, and the pair hardens into one carried result. Mixed aligned-plus-orthogonal pairs reuse the aligned unit ticks when the fused basis becomes inspectable.";
            return;
        }

        if (_state.HasFirstSelection && _state.TryFindSource(_state.Selection.FirstSourceId, out var selected))
        {
            _bodyText.Text = $"{selected.BucketLabel} is staged. Pick any second source, including the same one again, to show that every pair can harden into a reusable fused form.";
            return;
        }

        if (_state.LastResultKey != 0L && _state.TryFindResult(_state.LastResultKey, out var result))
        {
            _bodyText.Text = $"{result.Title} is the most recent fusion. Aligned + aligned stays horizontal, orthogonal + orthogonal stays vertical, and mixed pairs open the area / complex family depending on their dominant or recessive orientation.";
            return;
        }

        _bodyText.Text = "Use the four right-side source elements as inputs. This page is about hardening: two separate carried readings are selected, fused, and then kept as one discoverable basis result in the gallery.";
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
