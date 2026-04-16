using System.Text;
using System.Collections.Immutable;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Formatting;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;
using Core4.Dynamics;
using Core4.Structure;
using Core4.Topology;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// Topology explorer page for carrier chains and connected sites. The diagram is draggable, but every chain,
/// connector reading, and flow overlay is still derived from real Core4 topology structures.
/// </summary>
public sealed class CarrierChainPageView : UserControl
{
    private readonly CarrierChainSceneControl _scene;
    private readonly Button _tJunctionButton;
    private readonly Button _lCornerButton;
    private readonly Button _yBranchButton;
    private readonly Button _hBridgeButton;
    private readonly Button _fanInButton;
    private readonly Button _triangleButton;
    private readonly Button _diamondCrossButton;
    private readonly TextBlock _sceneDescriptionText;
    private readonly TextBlock _metaPresetText;
    private readonly TextBlock _metaCarrierText;
    private readonly TextBlock _metaChainText;
    private readonly TextBlock _structureText;
    private readonly TextBlock _focusText;
    private readonly TextBlock _flowText;
    private CarrierChainDemoState _state = CarrierChainDemoState.TJunction;

    public CarrierChainPageView()
    {
        _scene = new CarrierChainSceneControl
        {
            State = _state,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        _scene.LayoutChanged += (_, _) => SyncUi();

        _tJunctionButton = ProbeUi.PillButton("T Junction", true);
        _tJunctionButton.Click += (_, _) => ApplyState(CarrierChainDemoState.TJunction);

        _lCornerButton = ProbeUi.PillButton("L Corner");
        _lCornerButton.Click += (_, _) => ApplyState(CarrierChainDemoState.LCorner);

        _yBranchButton = ProbeUi.PillButton("Y Branch");
        _yBranchButton.Click += (_, _) => ApplyState(CarrierChainDemoState.YBranch);

        _hBridgeButton = ProbeUi.PillButton("H Bridge");
        _hBridgeButton.Click += (_, _) => ApplyState(CarrierChainDemoState.HBridge);

        _fanInButton = ProbeUi.PillButton("Fan In");
        _fanInButton.Click += (_, _) => ApplyState(CarrierChainDemoState.FanIn);

        _triangleButton = ProbeUi.PillButton("Triangle");
        _triangleButton.Click += (_, _) => ApplyState(CarrierChainDemoState.Triangle);

        _diamondCrossButton = ProbeUi.PillButton("Diamond Cross");
        _diamondCrossButton.Click += (_, _) => ApplyState(CarrierChainDemoState.DiamondCross);

        var resetButton = ProbeUi.PillButton("Reset Layout");
        resetButton.Click += (_, _) => _scene.ResetLayout();

        _sceneDescriptionText = ProbeUi.Body(string.Empty, 10d);
        _metaPresetText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _metaCarrierText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _metaChainText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _structureText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);
        _focusText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);
        _flowText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);

        var leftColumn = new StackPanel
        {
            Width = 198,
            Spacing = 6,
            Children =
            {
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 6,
                    Children =
                    {
                        ProbeUi.Mono("TOPOLOGY PRESETS", 10d, ProbePalette.Muted),
                        _tJunctionButton,
                        _lCornerButton,
                        _yBranchButton,
                        _hBridgeButton,
                        _fanInButton,
                        _triangleButton,
                        _diamondCrossButton,
                        resetButton
                    }
                }, 20, new Thickness(12, 10)),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        ProbeUi.Mono("VISUAL LANGUAGE", 10d, ProbePalette.Muted),
                        ProbeUi.Body("Carrier spans are solid runs. Endpoint hubs carry one centered dot or arrow on the white site ring. Drag any hub to deform the live geometry: the underlying topology stays fixed while the wave shows desired-versus-actual tension.", 10d)
                    }
                }, 20, new Thickness(12, 10)),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        ProbeUi.Mono("READING RULE", 10d, ProbePalette.Muted),
                        ProbeUi.Body("Walk each carrier as node, relation, node. Multi-carrier junctions appear where hubs carry attached sites from more than one carrier.", 10d)
                    }
                }, 20, new Thickness(12, 10))
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
        }, 22, new Thickness(12));

        var infoCards = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*"),
            ColumnSpacing = 10
        };
        ProbeUi.AddInfoCard(infoCards, 0, "STRUCTURE", _structureText);
        ProbeUi.AddInfoCard(infoCards, 1, "FOCUS CONNECTOR", _focusText);
        ProbeUi.AddInfoCard(infoCards, 2, "FLOW PATHS", _flowText);

        var rightColumn = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                sceneCard,
                infoCards
            }
        };

        var mainGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("198,*"),
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
                ProbeUi.Mono("TOPOLOGY SCENE", 10d, ProbePalette.Muted),
                ProbeUi.DisplayTitle("Carrier Chain Explorer", 18d),
                _sceneDescriptionText
            }
        };

        var metaStack = new StackPanel
        {
            Spacing = 2,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children =
            {
                _metaPresetText,
                _metaCarrierText,
                _metaChainText
            }
        };

        grid.Children.Add(titleStack);
        grid.Children.Add(metaStack);
        Grid.SetColumn(metaStack, 1);
        return grid;
    }

    private void ApplyState(CarrierChainDemoState state)
    {
        _state = state;
        _scene.State = state;
        SyncUi();
    }

    private void SyncUi()
    {
        ProbeUi.ApplyPillStyle(_tJunctionButton, _state.Label == CarrierChainDemoState.TJunction.Label);
        ProbeUi.ApplyPillStyle(_lCornerButton, _state.Label == CarrierChainDemoState.LCorner.Label);
        ProbeUi.ApplyPillStyle(_yBranchButton, _state.Label == CarrierChainDemoState.YBranch.Label);
        ProbeUi.ApplyPillStyle(_hBridgeButton, _state.Label == CarrierChainDemoState.HBridge.Label);
        ProbeUi.ApplyPillStyle(_fanInButton, _state.Label == CarrierChainDemoState.FanIn.Label);
        ProbeUi.ApplyPillStyle(_triangleButton, _state.Label == CarrierChainDemoState.Triangle.Label);
        ProbeUi.ApplyPillStyle(_diamondCrossButton, _state.Label == CarrierChainDemoState.DiamondCross.Label);

        _sceneDescriptionText.Text = _state.Description;
        _metaPresetText.Text = _state.Label;
        _metaCarrierText.Text = $"{_state.Topology.CarrierCount.Raw} carriers";
        _metaChainText.Text = $"{_state.Chains.Length} chains";
        _structureText.Text = DescribeStructure(_state);
        _focusText.Text = DescribeFocus(_state);
        _flowText.Text = DescribeFlows(_state, _scene.CurrentLayout);
    }

    private static string DescribeStructure(CarrierChainDemoState state)
    {
        var builder = new StringBuilder();
        builder.Append("chains                    ");
        builder.Append(state.Chains.Length);
        builder.Append('\n');
        builder.Append("sites                     ");
        builder.Append(state.Topology.SiteCount.Raw);
        builder.Append('\n');
        builder.Append("shared relations          ");
        builder.Append(state.Topology.RelationCount.Raw);

        foreach (var chain in state.Chains)
        {
            builder.Append('\n');
            builder.Append(CarrierLabel(chain.Carrier));
            builder.Append(" walk                    ");
            builder.Append(DescribeChain(chain));
        }

        return builder.ToString();
    }

    private static string DescribeFocus(CarrierChainDemoState state)
    {
        var connector = state.FocusConnector;
        return
            $"focus                     {SiteLabel(state.FocusSite)}\n" +
            $"neighbors                 {connector.NeighborCount.Raw}\n" +
            $"carriers                  {connector.Incidence.CarrierCount.Raw}\n" +
            $"continuing                {connector.Incidence.ContinuingCarrierCount.Raw}\n" +
            $"terminal                  {connector.Incidence.TerminalCarrierCount.Raw}\n" +
            $"geometry carriers         {connector.GeometryCarrierCount.Raw}\n" +
            $"site position             {ProbeSiteFormatting.PositionLabel(connector.SiteCarrierPosition)}";
    }

    private static string DescribeFlows(CarrierChainDemoState state, CarrierChainSceneLayout layout)
    {
        var builder = new StringBuilder();
        var hasAny = false;
        var edgeLookup = ImmutableDictionary.CreateBuilder<Relation, CarrierChainEdgeLayout>();

        foreach (var edge in layout.Edges)
        {
            edgeLookup[edge.Relation] = edge;
        }

        foreach (var site in state.Topology.Sites)
        {
            foreach (var flow in site.Orthogonal.Flows)
            {
                if (flow.Support is not Relation relation)
                {
                    continue;
                }

                if (hasAny)
                {
                    builder.Append("\n\n");
                }

                hasAny = true;
                edgeLookup.TryGetValue(relation, out var edge);
                var edgeMagnitude = edge.Relation == relation && !string.IsNullOrWhiteSpace(edge.Label)
                    ? edge.Label[(edge.Label.IndexOf(':') + 1)..]
                    : relation.Potential.Raw.ToString();
                builder.Append(flow.IsIncoming ? "incoming" : "outgoing");
                builder.Append("                  ");
                builder.Append(SiteLabel(site));
                builder.Append(" -> ");
                builder.Append(CarrierLabel(relation.Carrier));
                builder.Append(':');
                builder.Append(edgeMagnitude);
                builder.Append('\n');
                builder.Append("stated/driven             ");
                builder.Append(flow.Value.Normalize());
                builder.Append(" / ");
                builder.Append(FormatSigned(edge.DisplayTension));
            }
        }

        if (!hasAny)
        {
            return "flows                     none\n" +
                   "stated/driven             0/0 / 0";
        }

        return builder.ToString();
    }

    private static string FormatSigned(long value)
    {
        if (value > 0L)
        {
            return $"+{value}";
        }

        return value.ToString();
    }

    private static string DescribeChain(CarrierChain chain)
    {
        var builder = new StringBuilder();

        for (var index = 0; index < chain.Nodes.Length; index++)
        {
            var node = chain.Nodes[index];
            builder.Append('[');
            builder.Append(ProbeSiteFormatting.PositionLabel(node.Site));
            builder.Append(" a");
            builder.Append(node.AttachmentCount.Raw);
            builder.Append(']');

            if (index < chain.Relations.Length)
            {
                builder.Append(" --");
                builder.Append(chain.Relations[index].Potential.Raw);
                builder.Append("-- ");
            }
        }

        return builder.ToString();
    }

    private static string SiteLabel(Site site) => ProbeSiteFormatting.SiteLabel(site);

    private static string CarrierLabel(Carrier carrier) => ProbeSiteFormatting.CarrierLabel(carrier);
}
