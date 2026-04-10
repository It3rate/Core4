using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// First glossary page: carrier, site, and measure shown together in one scene.
/// </summary>
public sealed class GlossaryClaimPageView : UserControl
{
    private static readonly ProbeGlossary.GlossaryEntry CarrierEntry = FindEntry("Carrier");
    private static readonly ProbeGlossary.GlossaryEntry SiteEntry = FindEntry("Site");
    private static readonly ProbeGlossary.GlossaryEntry MeasureEntry = FindEntry("Measure");
    private static readonly ProbeGlossary.GlossaryEntry PinEntry = FindEntry("Pin");

    private readonly GlossaryClaimSceneControl _scene;
    private readonly Canvas _sceneOverlay;
    private readonly TextBlock _metaCarrierText;
    private readonly TextBlock _metaPinText;
    private readonly CarrierCardControls[] _controls;
    private readonly Button[] _sceneMeasureButtons;
    private GlossaryClaimDemoState _state = GlossaryClaimDemoState.CreateDefault();
    private bool _syncing;

    public GlossaryClaimPageView()
    {
        _scene = new GlossaryClaimSceneControl
        {
            State = _state,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        _scene.StateChanged += (_, state) =>
        {
            _state = state;
            SyncUi();
            SyncSceneButtons();
        };
        _scene.LayoutChanged += (_, _) => SyncSceneButtons();
        _sceneOverlay = new Canvas();

        _metaCarrierText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _metaPinText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _controls = new CarrierCardControls[_state.Carriers.Length];
        _sceneMeasureButtons = new Button[_state.Carriers.Length];

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

        var clearPins = ProbeUi.PillButton("Clear Pins");
        clearPins.Click += (_, _) => ApplyState(_state.ClearPins());

        var leftColumn = new StackPanel
        {
            Width = 232,
            Spacing = 6,
            Children =
            {
                EntryCard(CarrierEntry),
                EntryCard(SiteEntry),
                EntryCard(MeasureEntry),
                EntryCard(PinEntry),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 8,
                            Children =
                            {
                                resetLayout,
                                clearPins
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
                ProbeUi.DisplayTitle("Ordered Supports And Readings", 18d),
                ProbeUi.Body("Each carrier suggests a different ordered attribute. The slider picks a site on that carrier, and each pin button leaves a pinned uncalibrated reading at the chosen place.", 10d)
            }
        };

        var metaStack = new StackPanel
        {
            Spacing = 2,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children =
            {
                _metaCarrierText,
                _metaPinText
            }
        };

        grid.Children.Add(titleStack);
        grid.Children.Add(metaStack);
        Grid.SetColumn(metaStack, 1);
        return grid;
    }

    private Border BuildCarrierCard(int index, out CarrierCardControls controls)
    {
        var carrier = _state.CarrierAt(index).Carrier;
        var slider = new GlossarySiteSliderControl
        {
            Carrier = carrier,
            Minimum = 0,
            Maximum = 12,
            Value = (long)Math.Round(_state.CarrierAt(index).Site.Ratio.Approximate * 12d, MidpointRounding.AwayFromZero),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        slider.ValueChanged += (_, value) =>
        {
            if (!_syncing)
            {
                ApplyState(_state.WithSiteTicks(index, value, 12L));
            }
        };

        var button = ProbeUi.PillButton("Pin");
        button.Click += (_, _) => ApplyState(_state.AddPin(index));

        controls = new CarrierCardControls(slider);

        var title = ProbeUi.Header(GlossaryClaimVisuals.CardLabel(carrier), 12d);
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
                        title,
                        button
                    }
                },
                slider,
            }
        }, 20d, new Thickness(12, 10));
    }

    private void ApplyState(GlossaryClaimDemoState state)
    {
        _state = state;
        _scene.State = state;
        SyncUi();
    }

    private void SyncUi()
    {
        _syncing = true;
        var totalPins = 0;

        for (var index = 0; index < _controls.Length; index++)
        {
            var carrier = _state.CarrierAt(index);
            var ratio = carrier.Site.Ratio.Approximate;
            var ticks = (long)Math.Round(ratio * 12d, MidpointRounding.AwayFromZero);
            _controls[index].Slider.Value = ticks;
            totalPins += carrier.Pins.Length;
        }

        _metaCarrierText.Text = $"{_state.Carriers.Length} carriers";
        _metaPinText.Text = totalPins == 0
            ? "no pins placed"
            : $"{totalPins} pins placed";
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

    private void BuildSceneButtons()
    {
        _sceneOverlay.Children.Clear();
        for (var index = 0; index < _sceneMeasureButtons.Length; index++)
        {
            var button = ProbeUi.PillButton("Pin");
            button.FontSize = 10d;
            button.Padding = new Thickness(10, 2);
            var target = index;
            button.Click += (_, _) => ApplyState(_state.AddPin(target));
            _sceneMeasureButtons[index] = button;
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
        for (var index = 0; index < layout.Carriers.Length && index < _sceneMeasureButtons.Length; index++)
        {
            var button = _sceneMeasureButtons[index];
            var center = layout.Carriers[index].TitleButtonCenter;
            button.Measure(Size.Infinity);
            var desired = button.DesiredSize;
            Canvas.SetLeft(button, center.X - (desired.Width * 0.5d));
            Canvas.SetTop(button, center.Y - (desired.Height * 0.5d));
        }
    }

    private readonly record struct CarrierCardControls(GlossarySiteSliderControl Slider);
}
