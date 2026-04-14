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
    private readonly TextBlock _focusBlurbTitle;
    private readonly TextBlock _focusBlurbBody;
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
        _focusBlurbTitle = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);
        _focusBlurbBody = ProbeUi.Body(string.Empty, 10d);

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
            ColumnDefinitions = new ColumnDefinitions("92,*"),
            ColumnSpacing = 8,
            RowSpacing = 4,
            Children =
            {
                SceneHeader(_state),
                new StackPanel
                {
                    Spacing = 5,
                    Margin = new Thickness(6, 8, 0, 0),
                    Children =
                    {
                        _focusBlurbTitle,
                        _focusBlurbBody
                    }
                },
                _scene
            }
        };
        Grid.SetColumnSpan(sceneCardBody.Children[0], 2);
        Grid.SetRow(sceneCardBody.Children[1], 1);
        Grid.SetColumn(_scene, 1);
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
        SyncFocusBlurb();
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
            SyncFocusBlurb();
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

    private void SyncFocusBlurb()
    {
        var (title, body) = FocusBlurb(_focus);
        _focusBlurbTitle.Text = title;
        _focusBlurbBody.Text = body;
    }

    private static (string Title, string Body) FocusBlurb(SiteAnatomyFocus focus) => focus switch
    {
        SiteAnatomyFocus.Aligned => (
            "Aligned Carrier",
            "The aligned carrier is the site-local ordered state. Recessive holds local support, while dominant is the current published claim this site is trying to maintain."),
        SiteAnatomyFocus.Claim => (
            "Claim And Stiffness",
            "The dominant claim is a local preferred value. Stiffness hardens that claim so incoming tension moves it less easily instead of simply overwriting it."),
        SiteAnatomyFocus.SelfRoute => (
            "Self Return",
            "A share of softened orthogonal tension can route back into the site on the same round. Bias then decides how much of that routed share supports recessive versus dominant."),
        SiteAnatomyFocus.OrthogonalIn => (
            "Orthogonal In",
            "Inbound orthogonal lines are unresolved local evidence. They arrive from below because the site has not yet explained them in its own aligned carrier language."),
        SiteAnatomyFocus.OrthogonalOut => (
            "Orthogonal Out",
            "Outbound orthogonal lines are residual tension the site did not absorb. In the current engine they leave upward and remain available for carrier-level communication."),
        SiteAnatomyFocus.Ports => (
            "Carrier Ports",
            "Publish and inbound ports are orthogonal carrier-weight attachments. Other sites listen through these port weights rather than reading this site's aligned dominant directly."),
        _ => (
            "Whole Site",
            "One site currently carries aligned support and claim state, local hardening and routing controls, unresolved orthogonal residual, and carrier-backed publish or inbound port weights.")
    };
}
