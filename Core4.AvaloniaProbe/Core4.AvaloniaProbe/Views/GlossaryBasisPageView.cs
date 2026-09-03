using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// Third glossary page: a user interval freezes into a basis, then other temperature scales become readable through it.
/// </summary>
public sealed class GlossaryBasisPageView : UserControl
{
    private static readonly ProbeGlossary.GlossaryEntry FreezeEntry = FindEntry("Freeze");
    private static readonly ProbeGlossary.GlossaryEntry BasisEntry = FindEntry("Basis");
    private static readonly ProbeGlossary.GlossaryEntry AttachmentEntry = FindEntry("Attachment");

    private readonly GlossaryBasisSceneControl _scene;
    private readonly Canvas _sceneOverlay;
    private readonly TextBlock _metaPhaseText;
    private readonly TextBlock _metaReadingText;
    private readonly TextBlock _controlTitle;
    private readonly TextBlock _controlBody;
    private readonly GlossarySiteSliderControl _slider;
    private readonly Button _actionButton;
    private readonly Button _sceneActionButton;
    private GlossaryBasisDemoState _state = GlossaryBasisDemoState.CreateDefault();
    private bool _syncing;

    public GlossaryBasisPageView()
    {
        _scene = new GlossaryBasisSceneControl
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
        _metaPhaseText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _metaReadingText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _controlTitle = ProbeUi.Header(string.Empty, 12d);
        _controlBody = ProbeUi.Body(string.Empty, 10d);

        _slider = new GlossarySiteSliderControl
        {
            Carrier = new Carrier(1L),
            Minimum = 0,
            Maximum = GlossaryBasisDemoState.TotalTicks,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };
        _slider.ValueChanged += (_, value) =>
        {
            if (_syncing)
            {
                return;
            }

            ApplyState(_state.IsFrozen
                ? _state.WithReadTicks(value)
                : _state.WithSetupTicks(value));
        };

        _actionButton = ProbeUi.PillButton("Pin Origin");
        _actionButton.Click += (_, _) => ApplyState(_state.StepAction());

        _sceneActionButton = ProbeUi.PillButton("Pin Origin");
        _sceneActionButton.FontSize = 10d;
        _sceneActionButton.Padding = new Thickness(10, 2);
        _sceneActionButton.Click += (_, _) => ApplyState(_state.StepAction());
        _sceneOverlay.Children.Add(_sceneActionButton);

        var resetLayout = ProbeUi.PillButton("Reset Layout");
        resetLayout.Click += (_, _) => ApplyState(_state.ResetLayout());

        var leftColumn = new StackPanel
        {
            Width = 232,
            Spacing = 6,
            Children =
            {
                EntryCard(FreezeEntry),
                EntryCard(BasisEntry),
                EntryCard(AttachmentEntry),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        resetLayout
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
            Spacing = 8,
            Children =
            {
                _controlTitle,
                _controlBody,
                _slider,
                _actionButton
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
        SyncSceneButton();
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
                ProbeUi.DisplayTitle("Freezing A Temperature Basis", 18d),
                ProbeUi.Body("Choose an origin and a unit interval on one custom temperature line. Once that interval is frozen into a basis, Celsius and Fahrenheit appear beside it as different calibrations of the same experienced temperature.", 10d)
            }
        };

        var metaStack = new StackPanel
        {
            Spacing = 2,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Children =
            {
                _metaPhaseText,
                _metaReadingText
            }
        };

        grid.Children.Add(titleStack);
        grid.Children.Add(metaStack);
        Grid.SetColumn(metaStack, 1);
        return grid;
    }

    private void ApplyState(GlossaryBasisDemoState state)
    {
        _state = state;
        _scene.State = state;
        SyncUi();
    }

    private void SyncUi()
    {
        _syncing = true;
        _slider.Value = _state.IsFrozen ? _state.ReadTicks.Raw : _state.SiteTicks.Raw;
        _slider.IsVisible = _state.ShowSetupSlider || _state.ShowReadSlider;

        _actionButton.Content = _state.ActionLabel;
        _actionButton.IsVisible = _state.ShowFreezeButton || !_state.HasRelation;

        _sceneActionButton.Content = _state.ActionLabel;
        _sceneActionButton.IsVisible = _actionButton.IsVisible;

        if (_state.IsFrozen)
        {
            _controlTitle.Text = "Read Temperature";
            _controlBody.Text = $"Move one read marker across all three lines together. Drag the custom origin or unit marker above to reshape the basis. Current read: custom {_state.CustomAtTicks(_state.ReadTicks):0.0}, C {_state.CelsiusAtTicks(_state.ReadTicks):0.0}, F {_state.FahrenheitAtTicks(_state.ReadTicks):0.0}.";
            _metaPhaseText.Text = "basis frozen";
            _metaReadingText.Text = $"custom {_state.CustomAtTicks(_state.ReadTicks):0.0}";
        }
        else if (_state.HasRelation)
        {
            _controlTitle.Text = "Freeze Basis";
            _controlBody.Text = "The origin and relation are both chosen now. Freeze them into one reusable basis to reveal Celsius and Fahrenheit beside the custom calibration.";
            _metaPhaseText.Text = "relation selected";
            _metaReadingText.Text = "waiting to freeze";
        }
        else if (_state.HasOrigin)
        {
            _controlTitle.Text = "Pick Relation";
            _controlBody.Text = "Move the site to choose where unit 1 should land, then record that second point as the live relation.";
            _metaPhaseText.Text = "origin pinned";
            _metaReadingText.Text = "unit pending";
        }
        else
        {
            _controlTitle.Text = "Pin Origin";
            _controlBody.Text = "Move the site to choose the custom zero point, then pin it as the first reusable reference.";
            _metaPhaseText.Text = "setup";
            _metaReadingText.Text = "origin pending";
        }

        _syncing = false;
    }

    private void SyncSceneButton()
    {
        if (!_sceneActionButton.IsVisible || _scene.Bounds.Width <= 0d)
        {
            return;
        }

        var center = _scene.CurrentLayout.ActionButtonCenter;
        _sceneActionButton.Measure(Size.Infinity);
        var desired = _sceneActionButton.DesiredSize;
        Canvas.SetLeft(_sceneActionButton, center.X - (desired.Width * 0.5d));
        Canvas.SetTop(_sceneActionButton, center.Y - (desired.Height * 0.5d));
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
