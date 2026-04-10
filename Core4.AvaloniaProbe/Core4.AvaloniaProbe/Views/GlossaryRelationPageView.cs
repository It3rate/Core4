using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// Second glossary page: pin, origin, and relation.
/// </summary>
public sealed class GlossaryRelationPageView : UserControl
{
    private static readonly ProbeGlossary.GlossaryEntry PinEntry = FindEntry("Pin");
    private static readonly ProbeGlossary.GlossaryEntry OriginEntry = FindEntry("Origin");
    private static readonly ProbeGlossary.GlossaryEntry RelationEntry = FindEntry("Relation");

    private readonly GlossaryRelationSceneControl _scene;
    private readonly Canvas _sceneOverlay;
    private readonly GlossarySiteSliderControl _slider;
    private readonly TextBlock _metaStateText;
    private readonly TextBlock _metaRelationText;
    private readonly Button _cardActionButton;
    private readonly Button _sceneActionButton;
    private GlossaryRelationDemoState _state = GlossaryRelationDemoState.CreateDefault();
    private bool _syncing;

    public GlossaryRelationPageView()
    {
        _scene = new GlossaryRelationSceneControl
        {
            State = _state,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };
        _scene.StateChanged += (_, state) =>
        {
            _state = state;
            SyncUi();
            SyncSceneButton();
        };
        _scene.LayoutChanged += (_, _) => SyncSceneButton();

        _sceneOverlay = new Canvas();
        _metaStateText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _metaRelationText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);

        _slider = new GlossarySiteSliderControl
        {
            Carrier = _state.Carrier,
            Minimum = 0,
            Maximum = 100,
            Value = _state.SiteTicks.Raw,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };
        _slider.ValueChanged += (_, value) =>
        {
            if (!_syncing)
            {
                ApplyState(_state.WithSiteTicks(value));
            }
        };

        _cardActionButton = ProbeUi.PillButton(_state.ActionLabel);
        _cardActionButton.Click += (_, _) => ApplyState(_state.StepAction());

        _sceneActionButton = ProbeUi.PillButton(_state.ActionLabel);
        _sceneActionButton.FontSize = 10d;
        _sceneActionButton.Padding = new Thickness(10, 2);
        _sceneActionButton.Click += (_, _) => ApplyState(_state.StepAction());
        _sceneOverlay.Children.Add(_sceneActionButton);

        var resetLayout = ProbeUi.PillButton("Reset Layout");
        resetLayout.Click += (_, _) => ApplyState(_state.ResetLayout());
        var clearFrozen = ProbeUi.PillButton("Clear Frozen");
        clearFrozen.Click += (_, _) => ApplyState(_state.ClearFrozen());

        var leftColumn = new StackPanel
        {
            Width = 232,
            Spacing = 6,
            Children =
            {
                EntryCard(PinEntry),
                EntryCard(OriginEntry),
                EntryCard(RelationEntry),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        new StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            Spacing = 8,
                            Children =
                            {
                                resetLayout,
                                clearFrozen
                            }
                        }
                    }
                }, 20d, new Thickness(12, 10))
            }
        };

        var sceneCard = ProbeUi.Card(new StackPanel
        {
            Spacing = 8,
            Children =
            {
                SceneHeader(),
                ProbeUi.PlotCard(new Grid
                {
                    Children =
                    {
                        _scene,
                        _sceneOverlay
                    }
                })
            }
        }, 22d, new Thickness(12));

        var controlCard = ProbeUi.Card(new StackPanel
        {
            Spacing = 7,
            Children =
            {
                new Grid
                {
                    ColumnDefinitions = new Avalonia.Controls.ColumnDefinitions("*,Auto"),
                    ColumnSpacing = 8,
                    Children =
                    {
                        ProbeUi.Header("Site", 12d),
                        _cardActionButton
                    }
                },
                _slider
            }
        }, 20d, new Thickness(12, 10));
        Grid.SetColumn(_cardActionButton, 1);

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
            ColumnDefinitions = new Avalonia.Controls.ColumnDefinitions("232,*"),
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
        SyncSceneButton();
    }

    private Grid SceneHeader()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new Avalonia.Controls.ColumnDefinitions("*,Auto")
        };

        var titleStack = new StackPanel
        {
            Spacing = 2,
            Children =
            {
                ProbeUi.Mono("INTERACTIVE SCENE", 10d, ProbePalette.Muted),
                ProbeUi.DisplayTitle("Pin Origin And Freeze Relation", 18d),
                ProbeUi.Body("Pin one site as the reusable origin. After that, the same movable site is read as relation against that origin in tenths, and you can freeze relation markers along the carrier.", 10d)
            }
        };

        var metaStack = new StackPanel
        {
            Spacing = 2,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Children =
            {
                _metaStateText,
                _metaRelationText
            }
        };

        grid.Children.Add(titleStack);
        grid.Children.Add(metaStack);
        Grid.SetColumn(metaStack, 1);
        return grid;
    }

    private void ApplyState(GlossaryRelationDemoState state)
    {
        _state = state;
        _scene.State = state;
        SyncUi();
    }

    private void SyncUi()
    {
        _syncing = true;
        _slider.Value = _state.SiteTicks.Raw;
        _cardActionButton.Content = _state.ActionLabel;
        _sceneActionButton.Content = _state.ActionLabel;
        _metaStateText.Text = _state.HasOrigin ? $"origin {_state.OriginTicks.Raw}/100" : "origin unset";
        _metaRelationText.Text = _state.HasOrigin
            ? $"live relation {_state.CurrentRelation:0.0}"
            : $"measure {_state.CurrentMeasure.Raw}";
        _syncing = false;
    }

    private void SyncSceneButton()
    {
        if (_scene.Bounds.Width <= 0d)
        {
            return;
        }

        var layout = _scene.CurrentLayout;
        _sceneActionButton.Measure(Size.Infinity);
        var desired = _sceneActionButton.DesiredSize;
        Canvas.SetLeft(_sceneActionButton, layout.TitleButtonCenter.X - (desired.Width * 0.5d));
        Canvas.SetTop(_sceneActionButton, layout.TitleButtonCenter.Y - (desired.Height * 0.5d));
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
