using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// Second glossary page: pin, origin, and relation across the same three carriers.
/// </summary>
public sealed class GlossaryRelationPageView : UserControl
{
    private static readonly ProbeGlossary.GlossaryEntry PinEntry = FindEntry("Pin");
    private static readonly ProbeGlossary.GlossaryEntry OriginEntry = FindEntry("Origin");
    private static readonly ProbeGlossary.GlossaryEntry RelationEntry = FindEntry("Relation");

    private readonly GlossaryRelationSceneControl _scene;
    private readonly Canvas _sceneOverlay;
    private readonly TextBlock _metaOriginText;
    private readonly TextBlock _metaRelationText;
    private readonly CarrierControls[] _controls;
    private readonly Button[] _sceneActionButtons;
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
            SyncSceneButtons();
        };
        _scene.LayoutChanged += (_, _) => SyncSceneButtons();

        _sceneOverlay = new Canvas();
        _metaOriginText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _metaRelationText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _controls = new CarrierControls[_state.Carriers.Length];
        _sceneActionButtons = new Button[_state.Carriers.Length];

        var controlGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*"),
            ColumnSpacing = 10
        };

        for (var index = 0; index < _state.Carriers.Length; index++)
        {
            var card = BuildCarrierCard(index, out _controls[index]);
            controlGrid.Children.Add(card);
            Grid.SetColumn(card, index);
        }

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

        var rightColumn = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                sceneCard,
                controlGrid
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
        BuildSceneButtons();
        SyncUi();
        SyncSceneButtons();
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
                ProbeUi.DisplayTitle("Pin Origin And Freeze Relation", 18d),
                ProbeUi.Body("Each carrier can pin its own origin. After that, the same movable site is read in the carrier's own ticks with the chosen origin re-zeroed to 0, and frozen relation markers accumulate against that same support.", 10d)
            }
        };

        var metaStack = new StackPanel
        {
            Spacing = 2,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Children =
            {
                _metaOriginText,
                _metaRelationText
            }
        };

        grid.Children.Add(titleStack);
        grid.Children.Add(metaStack);
        Grid.SetColumn(metaStack, 1);
        return grid;
    }

    private Border BuildCarrierCard(int index, out CarrierControls controls)
    {
        var carrier = _state.CarrierAt(index);
        var slider = new GlossarySiteSliderControl
        {
            Carrier = carrier.Carrier,
            Minimum = 0,
            Maximum = 100,
            Value = carrier.SiteTicks.Raw,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };
        slider.ValueChanged += (_, value) =>
        {
            if (!_syncing)
            {
                ApplyState(_state.WithSiteTicks(index, value));
            }
        };

        var button = ProbeUi.PillButton(carrier.ActionLabel);
        var target = index;
        button.Click += (_, _) => ApplyState(_state.StepAction(target));

        controls = new CarrierControls(slider, button);
        Grid.SetColumn(button, 1);

        return ProbeUi.Card(new StackPanel
        {
            Spacing = 7,
            Children =
            {
                new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                    ColumnSpacing = 8,
                    Children =
                    {
                        ProbeUi.Header(GlossaryClaimVisuals.CardLabel(carrier.Carrier), 12d),
                        button
                    }
                },
                slider
            }
        }, 20d, new Thickness(12, 10));
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
        for (var index = 0; index < _controls.Length; index++)
        {
            var carrier = _state.CarrierAt(index);
            _controls[index].Slider.Value = carrier.SiteTicks.Raw;
            _controls[index].ActionButton.Content = carrier.ActionLabel;
            _sceneActionButtons[index].Content = carrier.ActionLabel;
        }

        _metaOriginText.Text = $"{_state.OriginCount.Raw} origins pinned";
        _metaRelationText.Text = _state.FrozenRelationCount.Raw == 0
            ? "no relations frozen"
            : $"{_state.FrozenRelationCount.Raw} relations frozen";
        _syncing = false;
    }

    private void BuildSceneButtons()
    {
        _sceneOverlay.Children.Clear();
        for (var index = 0; index < _sceneActionButtons.Length; index++)
        {
            var button = ProbeUi.PillButton(_state.CarrierAt(index).ActionLabel);
            button.FontSize = 10d;
            button.Padding = new Thickness(10, 2);
            var target = index;
            button.Click += (_, _) => ApplyState(_state.StepAction(target));
            _sceneActionButtons[index] = button;
            _sceneOverlay.Children.Add(button);
        }
    }

    private void SyncSceneButtons()
    {
        if (_scene.Bounds.Width <= 0d)
        {
            return;
        }

        var layout = _scene.CurrentLayout;
        for (var index = 0; index < layout.Carriers.Length && index < _sceneActionButtons.Length; index++)
        {
            var button = _sceneActionButtons[index];
            var center = layout.Carriers[index].TitleButtonCenter;
            button.Measure(Size.Infinity);
            var desired = button.DesiredSize;
            Canvas.SetLeft(button, center.X - (desired.Width * 0.5d));
            Canvas.SetTop(button, center.Y - (desired.Height * 0.5d));
        }
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

    private readonly record struct CarrierControls(GlossarySiteSliderControl Slider, Button ActionButton);
}
