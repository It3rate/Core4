using System.Collections.Immutable;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;
using Core4.Dynamics;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// First real-world topology exercise: a passive resistive circuit settled by repeated local relaxation instead of
/// closed-form solving. The topology remains ordinary Core4 structure; the page only renders one schematic stance on it.
/// </summary>
public sealed class CircuitPageView : UserControl
{
    private readonly CircuitSceneControl _scene;
    private readonly Button _balancedButton;
    private readonly Button _unevenButton;
    private readonly TextBlock _sceneDescriptionText;
    private readonly TextBlock _metaPresetText;
    private readonly TextBlock _metaIterationText;
    private readonly TextBlock _metaResidualText;
    private readonly TextBlock _nodeText;
    private readonly TextBlock _branchText;
    private readonly TextBlock _solverText;

    private CircuitDemoState _demo = CircuitDemoState.BalancedLoop;
    private bool _reversed;
    private ResistiveCircuit _circuit;
    private ResistiveCircuitState _state;
    private Placement _positiveNode;
    private Placement _negativeNode;

    public CircuitPageView()
    {
        _scene = new CircuitSceneControl
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        _balancedButton = ProbeUi.PillButton("Balanced Loop", true);
        _balancedButton.Click += (_, _) => ApplyDemo(CircuitDemoState.BalancedLoop);

        _unevenButton = ProbeUi.PillButton("Uneven Loop");
        _unevenButton.Click += (_, _) => ApplyDemo(CircuitDemoState.UnevenLoop);

        var relaxOneButton = ProbeUi.PillButton("Relax 1");
        relaxOneButton.Click += (_, _) => Relax(new Measure(1));

        var relaxEightButton = ProbeUi.PillButton("Relax 8");
        relaxEightButton.Click += (_, _) => Relax(new Measure(8));

        var settleButton = ProbeUi.PillButton("Settle");
        settleButton.Click += (_, _) => Settle();

        var flipButton = ProbeUi.PillButton("Flip Source");
        flipButton.Click += (_, _) => FlipSource();

        var resetButton = ProbeUi.PillButton("Reset Guess");
        resetButton.Click += (_, _) => ResetGuess();

        _sceneDescriptionText = ProbeUi.Body(string.Empty, 10d);
        _metaPresetText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _metaIterationText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _metaResidualText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _nodeText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);
        _branchText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);
        _solverText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);

        var leftColumn = new StackPanel
        {
            Width = 208,
            Spacing = 6,
            Children =
            {
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 6,
                    Children =
                    {
                        ProbeUi.Mono("CIRCUIT PRESETS", 10d, ProbePalette.Muted),
                        _balancedButton,
                        _unevenButton
                    }
                }, 20, new Thickness(12, 10)),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 6,
                    Children =
                    {
                        ProbeUi.Mono("RELAXATION", 10d, ProbePalette.Muted),
                        relaxOneButton,
                        relaxEightButton,
                        settleButton,
                        flipButton,
                        resetButton
                    }
                }, 20, new Thickness(12, 10)),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        ProbeUi.Mono("MAPPING", 10d, ProbePalette.Muted),
                        ProbeUi.Body("Sites act like node potentials, resistive relations explain part of the branch tension locally, and repeated weighted averaging lowers the remaining mismatch until the network settles.", 10d)
                    }
                }, 20, new Thickness(12, 10)),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        ProbeUi.Mono("RULE", 10d, ProbePalette.Muted),
                        ProbeUi.Body("This is a passive DC first pass: fixed source nodes impose a potential difference, conductance weights decide how strongly a branch transmits, and the page just plays repeated local relaxation instead of solving symbolically.", 10d)
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
        ProbeUi.AddInfoCard(infoCards, 0, "NODE POTENTIALS", _nodeText);
        ProbeUi.AddInfoCard(infoCards, 1, "BRANCH RESPONSE", _branchText);
        ProbeUi.AddInfoCard(infoCards, 2, "SOLVER", _solverText);

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
            ColumnDefinitions = new ColumnDefinitions("208,*"),
            ColumnSpacing = 8,
            Children =
            {
                leftColumn,
                rightColumn
            }
        };
        Grid.SetColumn(rightColumn, 1);

        Content = mainGrid;
        ApplyDemo(_demo);
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
                ProbeUi.Mono("CIRCUIT RELAXATION", 10d, ProbePalette.Muted),
                ProbeUi.DisplayTitle("Resistive Explorer", 18d),
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
                _metaIterationText,
                _metaResidualText
            }
        };

        grid.Children.Add(titleStack);
        grid.Children.Add(metaStack);
        Grid.SetColumn(metaStack, 1);
        return grid;
    }

    private void ApplyDemo(CircuitDemoState demo)
    {
        _demo = demo;
        _reversed = false;
        RebuildCircuit(settled: true);
    }

    private void FlipSource()
    {
        _reversed = !_reversed;
        RebuildCircuit(settled: true);
    }

    private void ResetGuess()
    {
        _circuit = _demo.CreateCircuit(_reversed);
        _state = _circuit.Seed();
        ApplyScene();
        SyncUi();
    }

    private void Relax(Measure steps)
    {
        _state = _state.Relax(steps);
        ApplyScene();
        SyncUi();
    }

    private void Settle()
    {
        _state = _state.Settle(new Measure(64));
        ApplyScene();
        SyncUi();
    }

    private void RebuildCircuit(bool settled)
    {
        _circuit = _demo.CreateCircuit(_reversed);
        _state = settled
            ? _circuit.Seed().Settle(new Measure(64))
            : _circuit.Seed();
        _positiveNode = _circuit.NodeKeyOf(_reversed ? _demo.NegativeSite : _demo.PositiveSite);
        _negativeNode = _circuit.NodeKeyOf(_reversed ? _demo.PositiveSite : _demo.NegativeSite);
        _scene.Demo = _demo;
        _scene.Circuit = _circuit;
        _scene.Positions = _demo.NodePositions(_circuit);
        ApplyScene();
        SyncUi();
    }

    private void ApplyScene()
    {
        _scene.Circuit = _circuit;
        _scene.State = _state;
        _scene.PositiveNode = _positiveNode;
        _scene.NegativeNode = _negativeNode;
    }

    private void SyncUi()
    {
        ProbeUi.ApplyPillStyle(_balancedButton, _demo.Label == CircuitDemoState.BalancedLoop.Label);
        ProbeUi.ApplyPillStyle(_unevenButton, _demo.Label == CircuitDemoState.UnevenLoop.Label);

        _sceneDescriptionText.Text = _demo.Description;
        _metaPresetText.Text = _reversed ? $"{_demo.Label} · reversed" : _demo.Label;
        _metaIterationText.Text = $"iteration {_state.Iteration.Raw}";
        _metaResidualText.Text = $"max residual {Format(_state.MaxResidual)}";
        _nodeText.Text = DescribeNodes();
        _branchText.Text = DescribeBranches();
        _solverText.Text = DescribeSolver();
    }

    private string DescribeNodes()
    {
        var builder = new StringBuilder();

        foreach (var node in _circuit.Nodes)
        {
            builder.Append(_demo.NodeLabel(node));
            builder.Append("                     ");
            builder.Append(Format(_state.PotentialAt(node.Key)));

            if (node.Key == _positiveNode)
            {
                builder.Append("  +");
            }
            else if (node.Key == _negativeNode)
            {
                builder.Append("  -");
            }

            builder.Append('\n');
        }

        return builder.ToString().TrimEnd();
    }

    private string DescribeBranches()
    {
        var builder = new StringBuilder();

        foreach (var branch in _circuit.Branches)
        {
            builder.Append(NodeLabel(branch.From));
            builder.Append(" -> ");
            builder.Append(NodeLabel(branch.To));
            builder.Append('\n');
            builder.Append("R                       ");
            builder.Append(Format(branch.Resistance));
            builder.Append('\n');
            builder.Append("drop / current / heat   ");
            builder.Append(Format(_state.DropOn(branch)));
            builder.Append(" / ");
            builder.Append(Format(_state.CurrentOn(branch)));
            builder.Append(" / ");
            builder.Append(Format(_state.DissipationOn(branch)));
            builder.Append("\n\n");
        }

        return builder.ToString().TrimEnd();
    }

    private string DescribeSolver()
    {
        var builder = new StringBuilder();
        builder.Append("fixed nodes              ");
        builder.Append(CountNodes(_circuit.Nodes, fixedOnly: true));
        builder.Append('\n');
        builder.Append("relaxed nodes            ");
        builder.Append(CountNodes(_circuit.Nodes, fixedOnly: false));
        builder.Append('\n');
        builder.Append("branches                 ");
        builder.Append(_circuit.Branches.Length);
        builder.Append('\n');
        builder.Append("source                   ");
        builder.Append(_reversed ? "negative -> positive" : "positive -> negative");
        builder.Append('\n');
        builder.Append("residual                 ");
        builder.Append(Format(_state.MaxResidual));

        return builder.ToString();
    }

    private string NodeLabel(Placement key)
    {
        foreach (var node in _circuit.Nodes)
        {
            if (node.Key == key)
            {
                return _demo.NodeLabel(node);
            }
        }

        return key.On(key.PrimaryCarrier).Whole.Raw.ToString();
    }

    private static string Format(Ratio value)
    {
        var normalized = value.Canonicalize();

        if (normalized.IsTension)
        {
            return $"{normalized.Potential.Raw}/0";
        }

        return normalized.Dominant == Core4.Numerics.Measure.One
            ? normalized.Recessive.Raw.ToString()
            : $"{normalized.Recessive.Raw}/{normalized.Dominant.Raw}";
    }

    private static int CountNodes(ImmutableArray<ResistiveNode> nodes, bool fixedOnly)
    {
        var total = 0;

        foreach (var node in nodes)
        {
            if (node.IsFixed == fixedOnly)
            {
                total++;
            }
        }

        return total;
    }
}
