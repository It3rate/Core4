using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Core4.AvaloniaProbe.Controls;
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
    private readonly Button _hBridgeButton;
    private readonly Button _fanInButton;
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

        _tJunctionButton = ProbeUi.PillButton("T Junction", true);
        _tJunctionButton.Click += (_, _) => ApplyState(CarrierChainDemoState.TJunction);

        _hBridgeButton = ProbeUi.PillButton("H Bridge");
        _hBridgeButton.Click += (_, _) => ApplyState(CarrierChainDemoState.HBridge);

        _fanInButton = ProbeUi.PillButton("Fan In");
        _fanInButton.Click += (_, _) => ApplyState(CarrierChainDemoState.FanIn);

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
                        _hBridgeButton,
                        _fanInButton,
                        resetButton
                    }
                }, 20, new Thickness(12, 10)),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        ProbeUi.Mono("VISUAL LANGUAGE", 10d, ProbePalette.Muted),
                        ProbeUi.Body("Carrier spans are solid runs. Flow-backed orthogonal strands add wave overlays and endpoint markers. Drag any hub to stretch the map without changing the underlying topology.", 10d)
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
        ProbeUi.ApplyPillStyle(_hBridgeButton, _state.Label == CarrierChainDemoState.HBridge.Label);
        ProbeUi.ApplyPillStyle(_fanInButton, _state.Label == CarrierChainDemoState.FanIn.Label);

        _sceneDescriptionText.Text = _state.Description;
        _metaPresetText.Text = _state.Label;
        _metaCarrierText.Text = $"{_state.Topology.CarrierCount.Raw} carriers";
        _metaChainText.Text = $"{_state.Chains.Length} chains";
        _structureText.Text = DescribeStructure(_state);
        _focusText.Text = DescribeFocus(_state);
        _flowText.Text = DescribeFlows(_state);
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
            $"site position             {connector.SiteCarrierPosition.Normalize()}";
    }

    private static string DescribeFlows(CarrierChainDemoState state)
    {
        var builder = new StringBuilder();
        var hasAny = false;

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
                    builder.Append('\n');
                }

                hasAny = true;
                builder.Append(flow.IsIncoming ? "incoming" : "outgoing");
                builder.Append("                  ");
                builder.Append(SiteLabel(site));
                builder.Append(" -> ");
                builder.Append(CarrierLabel(relation.Carrier));
                builder.Append(':');
                builder.Append(relation.Potential.Raw);
                builder.Append("  tension ");
                builder.Append(flow.Value.Normalize());
            }
        }

        if (!hasAny)
        {
            return "flows                     none\n" +
                   "tension                   0/0";
        }

        return builder.ToString();
    }

    private static string DescribeChain(CarrierChain chain)
    {
        var builder = new StringBuilder();

        for (var index = 0; index < chain.Nodes.Length; index++)
        {
            var node = chain.Nodes[index];
            builder.Append('[');
            builder.Append(node.Location.Raw);
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

    private static string SiteLabel(Site site) => $"{CarrierLabel(site.Carrier)}{site.Location.Raw}";

    private static string CarrierLabel(Carrier carrier) => carrier.Id switch
    {
        1L => "x",
        2L => "y",
        3L => "z",
        4L => "w",
        _ => $"c{carrier.Id}"
    };
}
