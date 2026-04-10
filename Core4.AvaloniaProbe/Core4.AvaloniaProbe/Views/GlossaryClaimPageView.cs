using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// First glossary page: carrier, site, and measure shown together in one scene.
/// </summary>
public sealed class GlossaryClaimPageView : UserControl
{
    private readonly GlossaryClaimSceneControl _scene;
    private readonly TextBlock _metaCarrierText;
    private readonly TextBlock _metaMeasureText;
    private readonly CarrierCardControls[] _controls;
    private GlossaryClaimDemoState _state = GlossaryClaimDemoState.Default;
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
        };

        _metaCarrierText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _metaMeasureText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _controls = new CarrierCardControls[_state.Carriers.Length];

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

        var clearMeasures = ProbeUi.PillButton("Clear Measures");
        clearMeasures.Click += (_, _) => ApplyState(_state.ClearMeasures());

        var leftColumn = new StackPanel
        {
            Width = 224,
            Spacing = 6,
            Children =
            {
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        ProbeUi.Mono("GLOSSARY / CLAIM", 10d, ProbePalette.Muted),
                        ProbeUi.DisplayTitle("Carrier / Site / Measure", 18d),
                        ProbeUi.Body("Start with ordered support, then a place on that support, then a reading taken there. This page shows those three together before pinning or freezing anything.", 10d)
                    }
                }, 20d, new Thickness(12, 10)),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 5,
                    Children =
                    {
                        ProbeUi.Mono("READING ORDER", 10d, ProbePalette.Muted),
                        GlossaryRow("Carrier", "ordered support"),
                        GlossaryRow("Site", "place where relation may occur"),
                        GlossaryRow("Measure", "reading of that site")
                    }
                }, 20d, new Thickness(12, 10)),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        ProbeUi.Mono("TRY IT", 10d, ProbePalette.Muted),
                        ProbeUi.Body("Drag carrier endpoints to reshape the ordered supports. Then move the site slider for each carrier and click Measure to leave a reading behind on the line.", 10d),
                        new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 8,
                            Children =
                            {
                                resetLayout,
                                clearMeasures
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
                ProbeUi.PlotCard(_scene)
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
            ColumnDefinitions = new ColumnDefinitions("224,*"),
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
                ProbeUi.DisplayTitle("Ordered Supports And Readings", 18d),
                ProbeUi.Body("Each carrier suggests a different ordered attribute. The slider picks a site on that carrier, and each measure button leaves a reading at the chosen place.", 10d)
            }
        };

        var metaStack = new StackPanel
        {
            Spacing = 2,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children =
            {
                _metaCarrierText,
                _metaMeasureText
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

        var caption = ProbeUi.Body(string.Empty, 10d);
        var measureCount = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Muted);
        var button = ProbeUi.PillButton("Measure");
        button.Click += (_, _) => ApplyState(_state.AddMeasure(index));

        controls = new CarrierCardControls(slider, caption, measureCount);

        return ProbeUi.Card(new StackPanel
        {
            Spacing = 7,
            Children =
            {
                ProbeUi.Header(GlossaryClaimVisuals.Label(carrier), 12d),
                ProbeUi.Body(GlossaryClaimVisuals.Summary(carrier), 10d),
                slider,
                caption,
                new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("Auto,*"),
                    ColumnSpacing = 8,
                    Children =
                    {
                        button,
                        measureCount
                    }
                }
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
        var totalMeasures = 0;

        for (var index = 0; index < _controls.Length; index++)
        {
            var carrier = _state.CarrierAt(index);
            var ratio = carrier.Site.Ratio.Approximate;
            var ticks = (long)Math.Round(ratio * 12d, MidpointRounding.AwayFromZero);
            _controls[index].Slider.Value = ticks;
            _controls[index].Caption.Text = GlossaryClaimVisuals.SiteCaption(carrier.Carrier, ratio);
            _controls[index].MeasureCount.Text = carrier.Measures.Length == 0
                ? "no measures yet"
                : $"{carrier.Measures.Length} measure{(carrier.Measures.Length == 1 ? string.Empty : "s")}";
            totalMeasures += carrier.Measures.Length;
        }

        _metaCarrierText.Text = $"{_state.Carriers.Length} carriers";
        _metaMeasureText.Text = totalMeasures == 0
            ? "no readings placed"
            : $"{totalMeasures} readings placed";
        _syncing = false;
    }

    private static Grid GlossaryRow(string term, string summary)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            ColumnSpacing = 8
        };
        var termBlock = ProbeUi.Header(term, 11d);
        var summaryBlock = ProbeUi.Mono(summary, 10d, ProbePalette.Muted);
        grid.Children.Add(termBlock);
        grid.Children.Add(summaryBlock);
        Grid.SetColumn(summaryBlock, 1);
        return grid;
    }

    private readonly record struct CarrierCardControls(
        GlossarySiteSliderControl Slider,
        TextBlock Caption,
        TextBlock MeasureCount);
}
