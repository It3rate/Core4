using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;
using Core4.AvaloniaProbe.Rendering;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// First topology landing page. It uses a real Core4 T-like structure and reads it through carrier chains and a
/// site connector so the probe has a concrete topology destination before richer visuals arrive.
/// </summary>
public sealed class CarrierChainPageView : UserControl
{
    public CarrierChainPageView()
    {
        Carrier x = 1;
        Carrier y = 2;

        var left = Site.Open(x, new Measure(0));
        var stemStart = Site.Open(y, new Measure(0));
        var stemEnd = Site.Open(y, new Measure(4));
        var center = Site.Open(x, new Measure(5))
            .Publish(
                new Relation(stemStart, stemEnd),
                Ratio.Tension(new Measure(4)));
        var right = Site.Open(x, new Measure(10));

        var topology = ConnectedSiteTopology.Create(
            [center],
            new Relation(left, center),
            new Relation(center, right));

        var mainChain = topology.ChainsOn(x)[0];
        var stemChain = topology.ChainsOn(y)[0];
        var connector = topology.ConnectorAt(center);

        var leftColumn = new StackPanel
        {
            Width = 188,
            Spacing = 6,
            Children =
            {
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        ProbeUi.Mono("ACTIVE PRESET", 10d, ProbePalette.Muted),
                        ProbeUi.DisplayTitle("Carrier Chain", 18d),
                        ProbeUi.Body("A connected topology projected into carrier-local runs.", 10d)
                    }
                }, 20, new Thickness(12, 10)),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        ProbeUi.Mono("READING RULE", 10d, ProbePalette.Muted),
                        ProbeUi.Body("Read each chain as node, relation, node, relation, node. Wider branch structure stays attached at the nodes.", 10d)
                    }
                }, 20, new Thickness(12, 10)),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        ProbeUi.Mono("NEXT", 10d, ProbePalette.Muted),
                        ProbeUi.Body("This page is a landing scaffold for future topology visuals. The underlying chains and connector summaries are already real Core4 structures.", 10d)
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
                ProbeUi.PlotCard(new StackPanel
                {
                    Spacing = 8,
                    Margin = new Thickness(6),
                    Children =
                    {
                        ProbeUi.Mono($"carrier x    {DescribeChain(mainChain)}", 11d, ProbePalette.Ink),
                        ProbeUi.Mono($"carrier y    {DescribeChain(stemChain)}", 11d, ProbePalette.Ink),
                        ProbeUi.Mono($"connector    neighbors {connector.NeighborCount.Raw}, continuing {connector.Incidence.ContinuingCarrierCount.Raw}, terminal {connector.Incidence.TerminalCarrierCount.Raw}", 11d, ProbePalette.Muted)
                    }
                })
            }
        }, 22, new Thickness(12));

        var mainChainText = ProbeUi.Mono(DescribeChainDetails(mainChain), 10d, ProbePalette.Ink);
        var stemChainText = ProbeUi.Mono(DescribeChainDetails(stemChain), 10d, ProbePalette.Ink);
        var connectorText = ProbeUi.Mono(DescribeConnectorDetails(connector), 10d, ProbePalette.Ink);

        var infoCards = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*"),
            ColumnSpacing = 10
        };
        ProbeUi.AddInfoCard(infoCards, 0, "MAIN CHAIN", mainChainText);
        ProbeUi.AddInfoCard(infoCards, 1, "STEM CHAIN", stemChainText);
        ProbeUi.AddInfoCard(infoCards, 2, "CENTER CONNECTOR", connectorText);

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
            ColumnDefinitions = new ColumnDefinitions("188,*"),
            ColumnSpacing = 8,
            Children =
            {
                leftColumn,
                rightColumn
            }
        };
        Grid.SetColumn(rightColumn, 1);

        Content = mainGrid;
    }

    private static Grid SceneHeader()
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
                ProbeUi.Mono("TOPOLOGY SNAPSHOT", 10d, ProbePalette.Muted),
                ProbeUi.DisplayTitle("Carrier Chain Explorer", 18d),
                ProbeUi.Body("A T-like Core4 structure read as one main carrier run plus one orthogonal strand.", 10d)
            }
        };

        var metaStack = new StackPanel
        {
            Spacing = 2,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children =
            {
                ProbeUi.Mono("preset T junction", 11d, ProbePalette.Muted),
                ProbeUi.Mono("2 carriers", 11d, ProbePalette.Muted),
                ProbeUi.Mono("2 chains", 11d, ProbePalette.Muted)
            }
        };

        grid.Children.Add(titleStack);
        grid.Children.Add(metaStack);
        Grid.SetColumn(metaStack, 1);
        return grid;
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

    private static string DescribeChainDetails(CarrierChain chain)
    {
        return
            $"nodes                     {chain.NodeCount.Raw}\n" +
            $"relations                 {chain.RelationCount.Raw}\n" +
            $"span                      {chain.TotalSpan.Raw}\n" +
            $"walk                      {DescribeChain(chain)}\n" +
            $"attachments               {chain.AttachmentCount.Raw}";
    }

    private static string DescribeConnectorDetails(SiteConnector connector)
    {
        return
            $"neighbors                 {connector.NeighborCount.Raw}\n" +
            $"carriers                  {connector.Incidence.CarrierCount.Raw}\n" +
            $"continuing                {connector.Incidence.ContinuingCarrierCount.Raw}\n" +
            $"terminal                  {connector.Incidence.TerminalCarrierCount.Raw}\n" +
            $"geometry carriers         {connector.GeometryCarrierCount.Raw}\n" +
            $"site position             {connector.SiteCarrierPosition.Normalize()}";
    }
}
