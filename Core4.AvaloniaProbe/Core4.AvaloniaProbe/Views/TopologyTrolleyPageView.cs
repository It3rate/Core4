using System.Collections.Immutable;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;
using Core4.Inspection;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// First inspection-trolley page. It reuses the carrier-chain scene and overlays one or more external trolley
/// frontiers so continuation, branching, and explicit path choice can be explored without mutating topology.
/// </summary>
public sealed class TopologyTrolleyPageView : UserControl
{
    private readonly CarrierChainSceneControl _scene;
    private readonly Button _tJunctionButton;
    private readonly Button _lCornerButton;
    private readonly Button _yBranchButton;
    private readonly Button _continueButton;
    private readonly Button _forkButton;
    private readonly Button _reverseButton;
    private readonly TextBlock _sceneDescriptionText;
    private readonly TextBlock _metaPresetText;
    private readonly TextBlock _metaFrontierText;
    private readonly TextBlock _metaChainText;
    private readonly TextBlock _currentText;
    private readonly TextBlock _observationText;
    private readonly TextBlock _trailText;
    private readonly StackPanel _candidatePanel;
    private TrolleyPreset _preset = TrolleyPreset.TJunction;
    private ImmutableArray<TopologyTrolley> _frontiers = ImmutableArray<TopologyTrolley>.Empty;

    public TopologyTrolleyPageView()
    {
        _scene = new CarrierChainSceneControl
        {
            State = _preset.Scene,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        _scene.LayoutChanged += (_, _) => SyncUi();

        _tJunctionButton = ProbeUi.PillButton("T Junction", true);
        _tJunctionButton.Click += (_, _) => ApplyPreset(TrolleyPreset.TJunction);

        _lCornerButton = ProbeUi.PillButton("L Corner");
        _lCornerButton.Click += (_, _) => ApplyPreset(TrolleyPreset.LCorner);

        _yBranchButton = ProbeUi.PillButton("Y Branch");
        _yBranchButton.Click += (_, _) => ApplyPreset(TrolleyPreset.YBranch);

        _continueButton = ProbeUi.PillButton("Continue Preferred");
        _continueButton.Click += (_, _) => ContinuePreferred();

        _forkButton = ProbeUi.PillButton("Fork All");
        _forkButton.Click += (_, _) => ForkAll();

        _reverseButton = ProbeUi.PillButton("Reverse");
        _reverseButton.Click += (_, _) => ReverseAll();

        var resetWalkButton = ProbeUi.PillButton("Reset Walk");
        resetWalkButton.Click += (_, _) => ResetWalk();

        _sceneDescriptionText = ProbeUi.Body(string.Empty, 10d);
        _metaPresetText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _metaFrontierText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _metaChainText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _currentText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);
        _observationText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);
        _trailText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);
        _candidatePanel = new StackPanel { Spacing = 6 };

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
                        ProbeUi.Mono("TROLLEY PRESETS", 10d, ProbePalette.Muted),
                        _tJunctionButton,
                        _lCornerButton,
                        _yBranchButton
                    }
                }, 20, new Thickness(12, 10)),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 6,
                    Children =
                    {
                        ProbeUi.Mono("MOVEMENT", 10d, ProbePalette.Muted),
                        _continueButton,
                        _forkButton,
                        _reverseButton,
                        resetWalkButton
                    }
                }, 20, new Thickness(12, 10)),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        ProbeUi.Mono("AVAILABLE EXITS", 10d, ProbePalette.Muted),
                        _candidatePanel
                    }
                }, 20, new Thickness(12, 10)),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        ProbeUi.Mono("INSPECTION RULE", 10d, ProbePalette.Muted),
                        ProbeUi.Body("The trolley is external to topology. By default it continues on one frontier. At a junction it exposes admissible next legs, then you can continue straight, choose one exit, or fork across all forward exits.", 10d)
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
        ProbeUi.AddInfoCard(infoCards, 0, "CURRENT", _currentText);
        ProbeUi.AddInfoCard(infoCards, 1, "OBSERVATION", _observationText);
        ProbeUi.AddInfoCard(infoCards, 2, "TRAIL", _trailText);

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
        ResetWalk();
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
                ProbeUi.Mono("TOPOLOGY INSPECTION", 10d, ProbePalette.Muted),
                ProbeUi.DisplayTitle("Trolley Explorer", 18d),
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
                _metaFrontierText,
                _metaChainText
            }
        };

        grid.Children.Add(titleStack);
        grid.Children.Add(metaStack);
        Grid.SetColumn(metaStack, 1);
        return grid;
    }

    private void ApplyPreset(TrolleyPreset preset)
    {
        _preset = preset;
        _scene.State = preset.Scene;
        ResetWalk();
    }

    private void ResetWalk()
    {
        _frontiers = [TopologyTrolley.Start(_preset.Scene.Topology, _preset.StartSite)];
        _scene.ResetLayout();
        SyncUi();
    }

    private void ContinuePreferred()
    {
        var next = ImmutableArray.CreateBuilder<TopologyTrolley>(_frontiers.Length);

        foreach (var frontier in _frontiers)
        {
            next.Add(frontier.TryContinuePreferred(out var moved) ? moved : frontier);
        }

        _frontiers = next.ToImmutable();
        SyncUi();
    }

    private void ForkAll()
    {
        var next = ImmutableArray.CreateBuilder<TopologyTrolley>();

        foreach (var frontier in _frontiers)
        {
            var forks = frontier.ForkAll();

            if (forks.IsDefaultOrEmpty)
            {
                next.Add(frontier);
                continue;
            }

            foreach (var fork in forks)
            {
                next.Add(fork);
            }
        }

        _frontiers = next.ToImmutable();
        SyncUi();
    }

    private void ReverseAll()
    {
        var next = ImmutableArray.CreateBuilder<TopologyTrolley>(_frontiers.Length);

        foreach (var frontier in _frontiers)
        {
            next.Add(frontier.TryReverse(out var reversed) ? reversed : frontier);
        }

        _frontiers = next.ToImmutable();
        SyncUi();
    }

    private void TakeSingleContinuation(TopologyTrolleyContinuation continuation)
    {
        if (_frontiers.Length != 1)
        {
            return;
        }

        _frontiers = [_frontiers[0].Move(continuation)];
        SyncUi();
    }

    private void SyncUi()
    {
        ProbeUi.ApplyPillStyle(_tJunctionButton, _preset.Label == TrolleyPreset.TJunction.Label);
        ProbeUi.ApplyPillStyle(_lCornerButton, _preset.Label == TrolleyPreset.LCorner.Label);
        ProbeUi.ApplyPillStyle(_yBranchButton, _preset.Label == TrolleyPreset.YBranch.Label);

        _sceneDescriptionText.Text = _preset.Description;
        _metaPresetText.Text = _preset.Label;
        _metaFrontierText.Text = $"{_frontiers.Length} frontier{(_frontiers.Length == 1 ? string.Empty : "s")}";
        _metaChainText.Text = $"{_preset.Scene.Chains.Length} chains";
        _scene.Overlay = BuildOverlay();
        _currentText.Text = DescribeCurrent();
        _observationText.Text = DescribeObservation();
        _trailText.Text = DescribeTrail();
        RebuildCandidateButtons();
        RefreshMovementButtons();
    }

    private CarrierChainSceneOverlay BuildOverlay()
    {
        var currentSites = ImmutableArray.CreateBuilder<Site>();
        var candidateSites = ImmutableArray.CreateBuilder<Site>();
        var trailRelations = ImmutableArray.CreateBuilder<Relation>();
        var candidateRelations = ImmutableArray.CreateBuilder<Relation>();

        foreach (var frontier in _frontiers)
        {
            AddSite(currentSites, frontier.Site);

            foreach (var relation in frontier.TrailRelations)
            {
                if (!trailRelations.Contains(relation))
                {
                    trailRelations.Add(relation);
                }
            }

            var observation = frontier.Observe();

            foreach (var continuation in observation.ForwardContinuations)
            {
                AddSite(candidateSites, continuation.To);

                if (!candidateRelations.Contains(continuation.Relation))
                {
                    candidateRelations.Add(continuation.Relation);
                }
            }
        }

        return new CarrierChainSceneOverlay(
            currentSites.ToImmutable(),
            candidateSites.ToImmutable(),
            trailRelations.ToImmutable(),
            candidateRelations.ToImmutable());
    }

    private void RebuildCandidateButtons()
    {
        _candidatePanel.Children.Clear();

        if (_frontiers.Length != 1)
        {
            _candidatePanel.Children.Add(ProbeUi.Body("Select one frontier by avoiding fork mode, or continue/reverse all current frontiers together.", 10d));
            return;
        }

        var observation = _frontiers[0].Observe();

        if (observation.ForwardContinuations.Length == 0)
        {
            _candidatePanel.Children.Add(ProbeUi.Body("No forward exits from the current site.", 10d));
            return;
        }

        if (observation.ForwardContinuations.Length == 1)
        {
            _candidatePanel.Children.Add(ProbeUi.Body("Only one forward exit is available here, so preferred continuation will take it automatically.", 10d));
            return;
        }

        foreach (var continuation in observation.ForwardContinuations)
        {
            var button = ProbeUi.PillButton($"Take {ContinuationLabel(continuation)}");
            var captured = continuation;
            button.Click += (_, _) => TakeSingleContinuation(captured);
            _candidatePanel.Children.Add(button);
        }
    }

    private void RefreshMovementButtons()
    {
        var hasPreferred = false;
        var hasForward = false;
        var hasReverse = false;

        foreach (var frontier in _frontiers)
        {
            var observation = frontier.Observe();
            hasPreferred |= observation.TryPreferred(out _);
            hasForward |= observation.ForwardContinuations.Length > 0;
            hasReverse |= frontier.TryReverse(out _);
        }

        _continueButton.IsEnabled = hasPreferred;
        _forkButton.IsEnabled = hasForward;
        _reverseButton.IsEnabled = hasReverse;
    }

    private string DescribeCurrent()
    {
        var builder = new StringBuilder();
        builder.Append("start                     ");
        builder.Append(SiteLabel(_preset.StartSite));

        for (var index = 0; index < _frontiers.Length; index++)
        {
            var frontier = _frontiers[index];
            var observation = frontier.Observe();
            builder.Append('\n');
            builder.Append("frontier ");
            builder.Append(index + 1);
            builder.Append("               ");
            builder.Append(SiteLabel(frontier.Site));
            builder.Append('\n');
            builder.Append("carrier exits             ");
            builder.Append(observation.Connector.Carriers.Length);
        }

        return builder.ToString();
    }

    private string DescribeObservation()
    {
        var builder = new StringBuilder();

        for (var index = 0; index < _frontiers.Length; index++)
        {
            var frontier = _frontiers[index];
            var observation = frontier.Observe();

            if (index > 0)
            {
                builder.Append("\n\n");
            }

            builder.Append("site                      ");
            builder.Append(SiteLabel(frontier.Site));
            builder.Append('\n');
            builder.Append("forward exits             ");
            builder.Append(observation.ForwardContinuations.Length);
            builder.Append('\n');
            builder.Append("preferred                 ");
            builder.Append(observation.TryPreferred(out var preferred) ? ContinuationLabel(preferred) : "none");
        }

        return builder.ToString();
    }

    private string DescribeTrail()
    {
        var builder = new StringBuilder();

        for (var index = 0; index < _frontiers.Length; index++)
        {
            var frontier = _frontiers[index];

            if (index > 0)
            {
                builder.Append("\n\n");
            }

            builder.Append("frontier ");
            builder.Append(index + 1);
            builder.Append("               ");

            if (frontier.TrailSites.Length == 0)
            {
                builder.Append("empty");
                continue;
            }

            for (var siteIndex = 0; siteIndex < frontier.TrailSites.Length; siteIndex++)
            {
                if (siteIndex > 0)
                {
                    builder.Append(" -> ");
                }

                builder.Append(SiteLabel(frontier.TrailSites[siteIndex]));
            }
        }

        return builder.ToString();
    }

    private static string ContinuationLabel(TopologyTrolleyContinuation continuation) =>
        $"{CarrierLabel(continuation.Carrier)}:{continuation.Potential.Raw} to {SiteLabel(continuation.To)}";

    private static string SiteLabel(Site site) => $"{CarrierLabel(site.Carrier)}{site.Location.Raw}";

    private static string CarrierLabel(Carrier carrier) => carrier.Id switch
    {
        1L => "x",
        2L => "y",
        3L => "z",
        4L => "w",
        _ => $"c{carrier.Id}"
    };

    private static void AddSite(ImmutableArray<Site>.Builder sites, Site site)
    {
        foreach (var known in sites)
        {
            if (known.Carrier == site.Carrier && known.Location == site.Location)
            {
                return;
            }
        }

        sites.Add(site);
    }

    private readonly record struct TrolleyPreset(
        string Label,
        string Description,
        CarrierChainDemoState Scene,
        Site StartSite)
    {
        public static TrolleyPreset TJunction => new(
            "T Junction",
            "Ride in from the left, watch the trolley prefer the same carrier through the junction, or choose to branch onto the stem.",
            CarrierChainDemoState.TJunction,
            Site.Open(new Carrier(1), new Measure(0)));

        public static TrolleyPreset LCorner => new(
            "L Corner",
            "A simple terminal turn. The trolley arrives on one carrier, then automatically takes the only forward exit onto the corner strand.",
            CarrierChainDemoState.LCorner,
            Site.Open(new Carrier(1), new Measure(0)));

        public static TrolleyPreset YBranch => new(
            "Y Branch",
            "A stem rises into a true branch point. Preferred continuation stops making guesses here, so you can either choose one exit or fork across both.",
            CarrierChainDemoState.YBranch,
            Site.Open(new Carrier(2), new Measure(-6)));
    }
}
