using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using System.Collections.Generic;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// Single-site legend page that separates the current engine's local claim machine from its orthogonal carrier ports.
/// </summary>
public sealed class SiteAnatomyPageView : UserControl
{
    private readonly SiteAnatomyDemoState _state = SiteAnatomyDemoState.Default;
    private readonly SiteAnatomySceneControl _scene;
    private readonly List<(Button Button, SiteAnatomyFocus Focus)> _focusButtons = [];
    private SiteAnatomyFocus _focus = SiteAnatomyFocus.All;

    public SiteAnatomyPageView()
    {
        _scene = new SiteAnatomySceneControl
        {
            State = _state,
            SelectedFocus = _focus,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top
        };

        var leftColumn = new StackPanel
        {
            Width = 222,
            Spacing = 6,
            Children =
            {
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 5,
                    Children =
                    {
                        ProbeUi.Mono("HIGHLIGHT", 10d, ProbePalette.Muted),
                        FocusButtons(),
                        ProbeUi.Body("These buttons brighten one mechanical slice at a time without changing the site itself.", 10d)
                    }
                }, 20, new Thickness(12, 10)),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 5,
                    Children =
                    {
                        ProbeUi.Mono("CURRENT ENGINE RULE", 10d, ProbePalette.Muted),
                        ProbeUi.Body("Aligned dominant is the local claim state. Cross-site listening currently happens through orthogonal carrier ports, not through a separate aligned transport channel.", 10d)
                    }
                }, 20, new Thickness(12, 10)),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 5,
                    Children =
                    {
                        ProbeUi.Mono("SITE OPERATORS", 10d, ProbePalette.Muted),
                        ProbeUi.Mono("Resolve(ratio)         aligned dominant claim", 9.4d, ProbePalette.Ink),
                        ProbeUi.Mono("Harden(ratio)         stiffness / lock", 9.4d, ProbePalette.Ink),
                        ProbeUi.Mono("Route(ratio)          local self-return share", 9.4d, ProbePalette.Ink),
                        ProbeUi.Mono("BiasToDominant(ratio) split routed share", 9.4d, ProbePalette.Ink),
                        ProbeUi.Mono("WithResidual(ratio)   orth residual", 9.4d, ProbePalette.Ink),
                        ProbeUi.Mono("Publish(carrier, r)   self-driven port weight", 9.4d, ProbePalette.Ink),
                        ProbeUi.Mono("Request(carrier, r)   other-driven port weight", 9.4d, ProbePalette.Ink)
                    }
                }, 20, new Thickness(12, 10)),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 5,
                    Children =
                    {
                        ProbeUi.Mono("MECHANICAL NOTE", 10d, ProbePalette.Muted),
                        ProbeUi.Body("Carrier publish and request flows live in the orthogonal reservoir. The carrier field later uses their weights to route residual between sites. The flow value is a port weight in the current engine, not a separate direct payload channel.", 10d)
                    }
                }, 20, new Thickness(12, 10))
            }
        };

        var alignedText = ProbeUi.Mono(
            $"recessive                {_state.Recessive}\n" +
            $"dominant claim           {_state.Dominant}\n" +
            $"stiffness                {_state.Stiffness}",
            10d,
            ProbePalette.Ink);

        var orthogonalText = ProbeUi.Mono(
            $"residual                 {_state.Residual}\n" +
            $"route                    {_state.Route}\n" +
            $"bias                     {_state.Bias}",
            10d,
            ProbePalette.Ink);

        var portsText = ProbeUi.Mono(
            $"publish p                {_state.PublishPorts[0].Value.Normalize()}\n" +
            $"publish q                {_state.PublishPorts[1].Value.Normalize()}\n" +
            $"listen p                 {_state.ListenPorts[0].Value.Normalize()}\n" +
            $"listen q                 {_state.ListenPorts[1].Value.Normalize()}",
            10d,
            ProbePalette.Ink);

        var sceneCardBody = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,Auto"),
            RowSpacing = 4,
            Children =
            {
                SceneHeader(_state),
                _scene
            }
        };
        Grid.SetRow(_scene, 1);

        var sceneCard = ProbeUi.Card(sceneCardBody, 22, new Thickness(10, 6, 10, 10));
        sceneCard.HorizontalAlignment = HorizontalAlignment.Stretch;

        var infoCards = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*"),
            ColumnSpacing = 10
        };
        ProbeUi.AddInfoCard(infoCards, 0, "ALIGNED", alignedText);
        ProbeUi.AddInfoCard(infoCards, 1, "ORTHOGONAL", orthogonalText);
        ProbeUi.AddInfoCard(infoCards, 2, "PORTS", portsText);

        var rightColumn = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,Auto"),
            RowSpacing = 10,
            Children =
            {
                sceneCard,
                infoCards
            }
        };
        Grid.SetRow(infoCards, 1);

        var mainGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("222,*"),
            ColumnSpacing = 8,
            Children =
            {
                leftColumn,
                rightColumn
            }
        };
        Grid.SetColumn(rightColumn, 1);

        Content = mainGrid;
        SyncFocusButtons();
    }

    private static Grid SceneHeader(SiteAnatomyDemoState state)
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
                ProbeUi.Mono("ONE SITE", 10d, ProbePalette.Muted),
                ProbeUi.DisplayTitle("Single Site Anatomy", 18d),
                ProbeUi.Body("This page labels what one site can currently hold and how that state is represented mechanically in the engine.", 10d)
            }
        };

        var metaStack = new StackPanel
        {
            Spacing = 2,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children =
            {
                ProbeUi.Mono($"claim {state.Dominant}", 11d, ProbePalette.Muted),
                ProbeUi.Mono($"residual {state.Residual}", 11d, ProbePalette.Muted),
                ProbeUi.Mono($"{state.PublishPorts.Length + state.ListenPorts.Length} carrier ports", 11d, ProbePalette.Muted)
            }
        };

        grid.Children.Add(titleStack);
        grid.Children.Add(metaStack);
        Grid.SetColumn(metaStack, 1);
        return grid;
    }

    private Control FocusButtons()
    {
        var rows = new StackPanel
        {
            Spacing = 6
        };

        rows.Children.Add(FocusRow(
            CreateFocusButton("All", SiteAnatomyFocus.All),
            CreateFocusButton("Aligned", SiteAnatomyFocus.Aligned)));
        rows.Children.Add(FocusRow(
            CreateFocusButton("Claim", SiteAnatomyFocus.Claim),
            CreateFocusButton("Self Route", SiteAnatomyFocus.SelfRoute)));
        rows.Children.Add(FocusRow(
            CreateFocusButton("Orth In", SiteAnatomyFocus.OrthogonalIn),
            CreateFocusButton("Orth Out", SiteAnatomyFocus.OrthogonalOut)));
        rows.Children.Add(FocusRow(
            CreateFocusButton("Ports", SiteAnatomyFocus.Ports),
            new Border { Opacity = 0d }));

        return rows;
    }

    private Control FocusRow(Button left, Control right)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            ColumnSpacing = 6,
            Children =
            {
                left,
                right
            }
        };
        Grid.SetColumn(right, 1);
        return grid;
    }

    private Button CreateFocusButton(string text, SiteAnatomyFocus focus)
    {
        var button = ProbeUi.SectionButton(text);
        button.Click += (_, _) =>
        {
            _focus = focus;
            _scene.SelectedFocus = _focus;
            SyncFocusButtons();
        };

        _focusButtons.Add((button, focus));
        return button;
    }

    private void SyncFocusButtons()
    {
        foreach (var (button, focus) in _focusButtons)
        {
            ProbeUi.ApplySectionStyle(button, focus == _focus);
        }
    }
}
