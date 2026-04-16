using System;
using System.Collections.Immutable;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Formatting;
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
    private const double MillisecondsPerUnit = 333d;
    private const double MinimumMotionMilliseconds = 922d;

    private readonly CarrierChainSceneControl _scene;
    private readonly Button _tJunctionButton;
    private readonly Button _lCornerButton;
    private readonly Button _yBranchButton;
    private readonly Button _diamondCrossButton;
    private readonly Button _continueButton;
    private readonly Button _leftButton;
    private readonly Button _rightButton;
    private readonly Button _forkButton;
    private readonly Button _reverseButton;
    private readonly Button _autoButton;
    private readonly TextBlock _sceneDescriptionText;
    private readonly TextBlock _metaPresetText;
    private readonly TextBlock _metaFrontierText;
    private readonly TextBlock _metaChainText;
    private readonly TextBlock _currentText;
    private readonly TextBlock _observationText;
    private readonly TextBlock _trailText;
    private readonly StackPanel _candidatePanel;
    private readonly DispatcherTimer _animationTimer;
    private TrolleyPreset _preset = TrolleyPreset.TJunction;
    private ImmutableArray<TopologyTrolley> _frontiers = ImmutableArray<TopologyTrolley>.Empty;
    private ImmutableArray<TrolleyMotion> _motions = ImmutableArray<TrolleyMotion>.Empty;
    private ImmutableArray<TopologyTrolley> _pendingFrontiers = ImmutableArray<TopologyTrolley>.Empty;
    private DateTime _animationStartedUtc;
    private TimeSpan _activeMotionDuration;
    private PropagationWave _autoWave;

    public TopologyTrolleyPageView()
    {
        _scene = new CarrierChainSceneControl
        {
            State = _preset.Scene,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        _scene.LayoutChanged += (_, _) => SyncUi();

        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _animationTimer.Tick += OnAnimationTick;

        _tJunctionButton = ProbeUi.PillButton("T Junction", true);
        _tJunctionButton.Click += (_, _) => ApplyPreset(TrolleyPreset.TJunction);

        _lCornerButton = ProbeUi.PillButton("L Corner");
        _lCornerButton.Click += (_, _) => ApplyPreset(TrolleyPreset.LCorner);

        _yBranchButton = ProbeUi.PillButton("Y Branch");
        _yBranchButton.Click += (_, _) => ApplyPreset(TrolleyPreset.YBranch);

        _diamondCrossButton = ProbeUi.PillButton("Diamond Cross");
        _diamondCrossButton.Click += (_, _) => ApplyPreset(TrolleyPreset.DiamondCross);

        _continueButton = ProbeUi.PillButton("Continue Preferred");
        _continueButton.Click += (_, _) => ContinuePreferred();

        _leftButton = ProbeUi.PillButton("Take Left");
        _leftButton.Click += (_, _) => TakeExtreme(left: true);

        _rightButton = ProbeUi.PillButton("Take Right");
        _rightButton.Click += (_, _) => TakeExtreme(left: false);

        _forkButton = ProbeUi.PillButton("Fork All");
        _forkButton.Click += (_, _) => ForkAll();

        _reverseButton = ProbeUi.PillButton("Reverse");
        _reverseButton.Click += (_, _) => ReverseAll();

        _autoButton = ProbeUi.PillButton("Auto");
        _autoButton.Click += (_, _) => ToggleAuto();

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
                        _yBranchButton,
                        _diamondCrossButton
                    }
                }, 20, new Thickness(12, 10)),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 6,
                    Children =
                    {
                        ProbeUi.Mono("MOVEMENT", 10d, ProbePalette.Muted),
                        _continueButton,
                        _leftButton,
                        _rightButton,
                        _forkButton,
                        _reverseButton,
                        _autoButton,
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
                        ProbeUi.Body("Purple side dots show the path already traversed. Green dotted lines show forward exits available from the current frontier. The trolley is external to topology: continue keeps one frontier, left/right choose one visible exit, fork all splits across every forward exit, and auto fans out until every frontier terminates before the same structural rules are read again from opposite perspective.", 10d)
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
        StopAnimation();
        StopAuto();
        _preset = preset;
        _scene.State = preset.Scene;
        ResetWalk();
    }

    private void ResetWalk()
    {
        StopAnimation();
        StopAuto();
        _frontiers = [TopologyTrolley.Start(_preset.Scene.Topology, _preset.StartSite)];
        _scene.ResetLayout();
        SyncUi();
    }

    private void ContinuePreferred()
    {
        if (HasActiveMotion)
        {
            return;
        }

        var next = ImmutableArray.CreateBuilder<TopologyTrolley>(_frontiers.Length);
        var motions = ImmutableArray.CreateBuilder<TrolleyMotion>();

        foreach (var frontier in _frontiers)
        {
            if (frontier.TryContinuePreferred(out var moved))
            {
                next.Add(moved);
                motions.Add(TrolleyMotion.Forward(frontier, moved));
            }
            else
            {
                next.Add(frontier);
            }
        }

        StartAnimation(motions.ToImmutable(), next.ToImmutable());
    }

    private void ForkAll()
    {
        if (HasActiveMotion)
        {
            return;
        }

        var next = ImmutableArray.CreateBuilder<TopologyTrolley>();
        var motions = ImmutableArray.CreateBuilder<TrolleyMotion>();

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
                motions.Add(TrolleyMotion.Forward(frontier, fork));
            }
        }

        StartAnimation(motions.ToImmutable(), next.ToImmutable());
    }

    private void TakeExtreme(bool left)
    {
        if (HasActiveMotion)
        {
            return;
        }

        var layout = _scene.CurrentLayout;
        var next = ImmutableArray.CreateBuilder<TopologyTrolley>(_frontiers.Length);
        var motions = ImmutableArray.CreateBuilder<TrolleyMotion>();

        foreach (var frontier in _frontiers)
        {
            var observation = frontier.Observe();
            var candidates = observation.ForwardContinuations;

            if (candidates.Length == 0)
            {
                next.Add(frontier);
                continue;
            }

            if (candidates.Length == 1)
            {
                var moved = frontier.Move(candidates[0]);
                next.Add(moved);
                motions.Add(TrolleyMotion.Forward(frontier, moved));
                continue;
            }

            var selected = frontier.Move(SelectExtreme(candidates, layout, left));
            next.Add(selected);
            motions.Add(TrolleyMotion.Forward(frontier, selected));
        }

        StartAnimation(motions.ToImmutable(), next.ToImmutable());
    }

    private void ReverseAll()
    {
        if (HasActiveMotion)
        {
            return;
        }

        var next = ImmutableArray.CreateBuilder<TopologyTrolley>(_frontiers.Length);
        var motions = ImmutableArray.CreateBuilder<TrolleyMotion>();

        foreach (var frontier in _frontiers)
        {
            if (frontier.TryContinuePreferred(-Core4.Numerics.Measure.One, out var reversed))
            {
                next.Add(reversed);
                motions.Add(TrolleyMotion.Move(frontier, reversed, reversed.TrailRelations[^1], reversePerspective: true));
            }
            else
            {
                next.Add(frontier);
            }
        }

        StartAnimation(motions.ToImmutable(), next.ToImmutable());
    }

    private void ToggleAuto()
    {
        if (IsAutoRunning)
        {
            StopAuto();
            SyncUi();
            return;
        }

        if (HasActiveMotion)
        {
            return;
        }

        _autoWave = PropagationWave.FromTrolleys(_frontiers, Core4.Numerics.Measure.One);
        _frontiers = _autoWave.VisibleTrolleys();
        SyncUi();
        AdvanceAuto();
    }

    private void AdvanceAuto()
    {
        if (!IsAutoRunning || HasActiveMotion)
        {
            return;
        }

        var step = _autoWave.Advance();
        _autoWave = step.After;
        _frontiers = step.Before.VisibleTrolleys();

        if (step.HasMotion)
        {
            StartAnimation(MapMoves(step.Moves), step.After.VisibleTrolleys());
            return;
        }

        _frontiers = step.After.VisibleTrolleys();
        SyncUi();

        if (IsAutoRunning)
        {
            Dispatcher.UIThread.Post(AdvanceAuto, DispatcherPriority.Background);
        }
    }

    private void TakeSingleContinuation(TopologyTrolleyContinuation continuation)
    {
        if (_frontiers.Length != 1 || HasActiveMotion)
        {
            return;
        }

        var moved = _frontiers[0].Move(continuation);
        StartAnimation([TrolleyMotion.Forward(_frontiers[0], moved)], [moved]);
    }

    private void SyncUi()
    {
        ProbeUi.ApplyPillStyle(_tJunctionButton, _preset.Label == TrolleyPreset.TJunction.Label);
        ProbeUi.ApplyPillStyle(_lCornerButton, _preset.Label == TrolleyPreset.LCorner.Label);
        ProbeUi.ApplyPillStyle(_yBranchButton, _preset.Label == TrolleyPreset.YBranch.Label);
        ProbeUi.ApplyPillStyle(_diamondCrossButton, _preset.Label == TrolleyPreset.DiamondCross.Label);

        var allowPresetChange = !HasActiveMotion && !IsAutoRunning;
        _tJunctionButton.IsEnabled = allowPresetChange;
        _lCornerButton.IsEnabled = allowPresetChange;
        _yBranchButton.IsEnabled = allowPresetChange;
        _diamondCrossButton.IsEnabled = allowPresetChange;
        _scene.IsHitTestVisible = true;
        _sceneDescriptionText.Text = _preset.Description;
        _metaPresetText.Text = _preset.Label;
        _metaFrontierText.Text = DescribeFrontierStatus();
        _metaChainText.Text = $"{_preset.Scene.Chains.Length} chains";
        _autoButton.Content = IsAutoRunning ? "Stop Auto" : "Auto";
        _scene.Overlay = BuildOverlay();
        _currentText.Text = DescribeCurrent();
        _observationText.Text = DescribeObservation();
        _trailText.Text = DescribeTrail();
        RebuildCandidateButtons();
        RefreshMovementButtons();
    }

    private CarrierChainSceneOverlay BuildOverlay()
    {
        var layout = _scene.CurrentLayout;
        var currentSites = ImmutableArray.CreateBuilder<Site>();
        var candidateSites = ImmutableArray.CreateBuilder<Site>();
        var trailRelations = ImmutableArray.CreateBuilder<Relation>();
        var candidateRelations = ImmutableArray.CreateBuilder<Relation>();
        var motionOverlays = ImmutableArray.CreateBuilder<CarrierChainOverlayMotion>();
        var readouts = ImmutableArray.CreateBuilder<CarrierChainOverlayReadout>();

        foreach (var frontier in _frontiers)
        {
            var moving = HasMotionFrom(frontier);

            if (!moving)
            {
                AddSite(currentSites, frontier.Site);

                foreach (var relation in frontier.TrailRelations)
                {
                    if (!trailRelations.Contains(relation))
                    {
                        trailRelations.Add(relation);
                    }
                }

                if (TryCreateReadout(frontier, layout, out var readout))
                {
                    AddReadout(readouts, readout);
                }
            }

            if (HasActiveMotion || IsAutoRunning)
            {
                continue;
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

        if (HasActiveMotion)
        {
            foreach (var motion in _motions)
            {
                foreach (var relation in motion.CommittedTrailRelations)
                {
                    if (!trailRelations.Contains(relation))
                    {
                        trailRelations.Add(relation);
                    }
                }

                AddMotion(
                    motionOverlays,
                    new CarrierChainOverlayMotion(
                        motion.Relation,
                        motion.FromSite,
                        motion.ToSite,
                        motion.TrailAnchor,
                        motion.TowardAnchor,
                        ShouldUseTensionPath(motion),
                        EaseOut(MotionProgress())));
            }
        }

        return new CarrierChainSceneOverlay(
            currentSites.ToImmutable(),
            candidateSites.ToImmutable(),
            trailRelations.ToImmutable(),
            candidateRelations.ToImmutable(),
            motionOverlays.ToImmutable(),
            readouts.ToImmutable());
    }

    private void RebuildCandidateButtons()
    {
        _candidatePanel.Children.Clear();

        if (IsAutoRunning)
        {
            _candidatePanel.Children.Add(ProbeUi.Body("Auto is traversing every available branch and will reverse each frontier once no forward exits remain.", 10d));
            return;
        }

        if (HasActiveMotion)
        {
            _candidatePanel.Children.Add(ProbeUi.Body("Animating the current traversal step.", 10d));
            return;
        }

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

        if (HasActiveMotion || IsAutoRunning)
        {
            _continueButton.IsEnabled = false;
            _leftButton.IsEnabled = false;
            _rightButton.IsEnabled = false;
            _forkButton.IsEnabled = false;
            _reverseButton.IsEnabled = false;
            _autoButton.IsEnabled = IsAutoRunning || !HasActiveMotion;
            return;
        }

        foreach (var frontier in _frontiers)
        {
            var observation = frontier.Observe();
            hasPreferred |= observation.TryPreferred(out _);
            hasForward |= observation.ForwardContinuations.Length > 0;
            hasReverse |= frontier.Observe(-Core4.Numerics.Measure.One).ForwardContinuationsIn(-Core4.Numerics.Measure.One).Length > 0;
        }

        _continueButton.IsEnabled = hasPreferred;
        _leftButton.IsEnabled = hasForward;
        _rightButton.IsEnabled = hasForward;
        _forkButton.IsEnabled = hasForward;
        _reverseButton.IsEnabled = hasReverse;
        _autoButton.IsEnabled = true;
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
            builder.Append(MultiplicityLabel(frontier.Site));
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
            builder.Append(MultiplicityLabel(frontier.Site));
            builder.Append('\n');
            builder.Append("carried calibration       ");
            builder.Append(frontier.Signature.Calibration);
            builder.Append('\n');
            builder.Append("forward exits             ");
            builder.Append(observation.ForwardContinuations.Length);
            builder.Append('\n');
            builder.Append("preferred                 ");
            builder.Append(observation.TryPreferred(out var preferred) ? ContinuationLabel(preferred) : "none");

            if (observation.TryPreferred(out preferred))
            {
                if (preferred.HasBasisAgreement)
                {
                    builder.Append('\n');
                    builder.Append("best agreement            ");
                    builder.Append(SupportReadFormatting.DescribeAgreement(preferred.BestBasisAgreement));
                }

                builder.Append('\n');
                builder.Append("preferred transfer        ");
                builder.Append(preferred.TransmittedCalibration);

                if (preferred.HasRetention)
                {
                    builder.Append('\n');
                    builder.Append("retained local            ");
                    builder.Append(preferred.RetainedCalibration);
                }
            }
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

    private string DescribeFrontierStatus()
    {
        var count = HasActiveMotion ? MotionFrontierCount() : _frontiers.Length;
        var label = $"{count} frontier{(count == 1 ? string.Empty : "s")}";

        if (HasActiveMotion)
        {
            label += " moving";
        }

        if (IsAutoRunning)
        {
            label += _autoWave.IsOutbound
                ? " · auto out"
                : " · auto back";
        }

        return label;
    }

    private static string ContinuationLabel(TopologyTrolleyContinuation continuation)
    {
        var label = $"{CarrierLabel(continuation.Carrier)}:{continuation.Potential.Raw} to {SiteLabel(continuation.To)}";

        if (continuation.IsPartialTransfer)
        {
            label += $" · move {continuation.TransmittedCalibration}";

            if (continuation.HasRetention)
            {
                label += $" keep {continuation.RetainedCalibration}";
            }
        }

        return label;
    }

    private string MultiplicityLabel(Site site)
    {
        if (!IsAutoRunning)
        {
            return string.Empty;
        }

        foreach (var frontier in _autoWave.VisibleFrontiers)
        {
            if (ProbeSiteFormatting.SameNode(frontier.Site, site))
            {
                var intensity = frontier.Intensity.Canonicalize();
                return intensity == new Ratio(Core4.Numerics.Measure.One, Core4.Numerics.Measure.One)
                    ? string.Empty
                    : $" x{intensity}";
            }
        }

        return string.Empty;
    }

    private static bool TryCreateReadout(
        TopologyTrolley frontier,
        CarrierChainSceneLayout layout,
        out CarrierChainOverlayReadout readout)
    {
        if (frontier.TrailRelations.IsDefaultOrEmpty)
        {
            readout = default;
            return false;
        }

        var relation = frontier.TrailRelations[^1];
        var actualMagnitude = RelationMagnitude(layout, relation);
        var atOrigin = ProbeSiteFormatting.SameNode(frontier.Site, relation.Origin);
        var position = atOrigin ? 0d : 1d;
        var recessive = actualMagnitude * position;
        var dominant = Math.Max(0d, actualMagnitude - recessive);
        readout = new CarrierChainOverlayReadout(
            frontier.Site,
            $"({recessive:0.0}i+{dominant:0.0})",
            new Vector(-44d, -28d));
        return true;
    }

    private bool HasActiveMotion => !_motions.IsDefaultOrEmpty && _motions.Length != 0;

    private bool IsAutoRunning => !_autoWave.IsComplete;

    private int MotionFrontierCount() => _pendingFrontiers.IsDefaultOrEmpty ? _frontiers.Length : _pendingFrontiers.Length;

    private bool HasMotionFrom(TopologyTrolley frontier)
    {
        foreach (var motion in _motions)
        {
            if (motion.Source == frontier)
            {
                return true;
            }
        }

        return false;
    }

    private void StartAnimation(ImmutableArray<TrolleyMotion> motions, ImmutableArray<TopologyTrolley> nextFrontiers)
    {
        if (motions.IsDefaultOrEmpty)
        {
            _frontiers = nextFrontiers;
            SyncUi();
            return;
        }

        _motions = motions;
        _pendingFrontiers = nextFrontiers;
        _animationStartedUtc = DateTime.UtcNow;
        _activeMotionDuration = ResolveMotionDuration(motions);
        _animationTimer.Start();
        SyncUi();
    }

    private void StopAnimation()
    {
        _animationTimer.Stop();
        _motions = ImmutableArray<TrolleyMotion>.Empty;
        _pendingFrontiers = ImmutableArray<TopologyTrolley>.Empty;
        _activeMotionDuration = TimeSpan.Zero;
    }

    private void StopAuto()
    {
        _autoWave = default;
    }

    private void OnAnimationTick(object? sender, EventArgs e)
    {
        if (!HasActiveMotion)
        {
            StopAnimation();
            return;
        }

        var elapsed = DateTime.UtcNow - _animationStartedUtc;
        var duration = _activeMotionDuration == TimeSpan.Zero
            ? TimeSpan.FromMilliseconds(MinimumMotionMilliseconds)
            : _activeMotionDuration;
        var t = Math.Clamp(elapsed.TotalMilliseconds / duration.TotalMilliseconds, 0d, 1d);
        _scene.Overlay = BuildOverlay();
        _metaFrontierText.Text = DescribeFrontierStatus();

        if (t < 1d)
        {
            return;
        }

        var finalFrontiers = _pendingFrontiers;
        StopAnimation();
        _frontiers = finalFrontiers;
        SyncUi();

        if (IsAutoRunning)
        {
            Dispatcher.UIThread.Post(AdvanceAuto, DispatcherPriority.Background);
        }
    }

    private static double EaseOut(double progress)
    {
        var inverse = 1d - progress;
        return 1d - (inverse * inverse);
    }

    private TimeSpan ResolveMotionDuration(ImmutableArray<TrolleyMotion> motions)
    {
        var layout = _scene.CurrentLayout;
        var longest = 0d;

        foreach (var motion in motions)
        {
            longest = Math.Max(longest, RelationMagnitude(layout, motion.Relation));
        }

        return TimeSpan.FromMilliseconds(Math.Max(MinimumMotionMilliseconds, longest * MillisecondsPerUnit));
    }

    private double MotionProgress()
    {
        if (_activeMotionDuration == TimeSpan.Zero)
        {
            return 0d;
        }

        var elapsed = DateTime.UtcNow - _animationStartedUtc;
        return Math.Clamp(elapsed.TotalMilliseconds / _activeMotionDuration.TotalMilliseconds, 0d, 1d);
    }

    private static double RelationMagnitude(CarrierChainSceneLayout layout, Relation relation)
    {
        foreach (var edge in layout.Edges)
        {
            if (edge.Relation == relation)
            {
                return Math.Max(1d, edge.ActualMagnitude);
            }
        }

        return 1d;
    }

    private static TopologyTrolleyContinuation SelectExtreme(
        ImmutableArray<TopologyTrolleyContinuation> continuations,
        CarrierChainSceneLayout layout,
        bool left)
    {
        var best = continuations[0];
        var bestPoint = ContinuationPoint(best, layout);

        for (var index = 1; index < continuations.Length; index++)
        {
            var candidate = continuations[index];
            var candidatePoint = ContinuationPoint(candidate, layout);

            if (left)
            {
                if (candidatePoint.X < bestPoint.X || (candidatePoint.X == bestPoint.X && candidatePoint.Y < bestPoint.Y))
                {
                    best = candidate;
                    bestPoint = candidatePoint;
                }
            }
            else if (candidatePoint.X > bestPoint.X || (candidatePoint.X == bestPoint.X && candidatePoint.Y < bestPoint.Y))
            {
                best = candidate;
                bestPoint = candidatePoint;
            }
        }

        return best;
    }

    private static Point ContinuationPoint(TopologyTrolleyContinuation continuation, CarrierChainSceneLayout layout)
    {
        foreach (var hub in layout.Hubs)
        {
            foreach (var site in hub.MemberSites)
            {
                if (ProbeSiteFormatting.SameNode(site, continuation.To))
                {
                    return hub.Point;
                }
            }
        }

        return default;
    }

    private static string SiteLabel(Site site) => ProbeSiteFormatting.SiteLabel(site);

    private static string CarrierLabel(Carrier carrier) => ProbeSiteFormatting.CarrierLabel(carrier);

    private static void AddSite(ImmutableArray<Site>.Builder sites, Site site)
    {
        foreach (var known in sites)
        {
            if (ProbeSiteFormatting.SameNode(known, site))
            {
                return;
            }
        }

        sites.Add(site);
    }

    private static void AddReadout(ImmutableArray<CarrierChainOverlayReadout>.Builder readouts, CarrierChainOverlayReadout readout)
    {
        foreach (var known in readouts)
        {
            if (ProbeSiteFormatting.SameNode(known.Site, readout.Site))
            {
                return;
            }
        }

        readouts.Add(readout);
    }

    private static void AddMotion(ImmutableArray<CarrierChainOverlayMotion>.Builder motions, CarrierChainOverlayMotion motion)
    {
        foreach (var known in motions)
        {
            if (known.Relation == motion.Relation
                && ProbeSiteFormatting.SameNode(known.From, motion.From)
                && ProbeSiteFormatting.SameNode(known.To, motion.To)
                && ProbeSiteFormatting.SameNode(known.TrailAnchor, motion.TrailAnchor))
            {
                return;
            }
        }

        motions.Add(motion);
    }

    private static ImmutableArray<TrolleyMotion> MapMoves(ImmutableArray<PropagationMove> moves)
    {
        var mapped = ImmutableArray.CreateBuilder<TrolleyMotion>(moves.Length);

        foreach (var move in moves)
        {
            mapped.Add(TrolleyMotion.Move(move.Source, move.Target, move.Relation, move.IsReturning));
        }

        return mapped.ToImmutable();
    }

    private bool ShouldUseTensionPath(TrolleyMotion motion)
    {
        return !motion.TowardAnchor;
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

        public static TrolleyPreset DiamondCross => new(
            "Diamond Cross",
            "The trolley rides into a diamond split, traverses both arms under auto, rejoins, and then continues through one vertical cross before the return pass reads the same structure from opposite perspective.",
            CarrierChainDemoState.DiamondCross,
            Site.Open(new Carrier(1), new Measure(0)));
    }

    private readonly record struct TrolleyMotion(
        TopologyTrolley Source,
        TopologyTrolley Target,
        Relation Relation,
        Site FromSite,
        Site ToSite,
        Site TrailAnchor,
        bool TowardAnchor,
        bool ReversePerspective,
        ImmutableArray<Relation> CommittedTrailRelations)
    {
        public static TrolleyMotion Forward(TopologyTrolley source, TopologyTrolley target) =>
            Move(source, target, target.TrailRelations[^1], reversePerspective: false);

        public static TrolleyMotion Reverse(TopologyTrolley source, TopologyTrolley target) =>
            Move(source, target, target.TrailRelations[^1], reversePerspective: true);

        public static TrolleyMotion Move(
            TopologyTrolley source,
            TopologyTrolley target,
            Relation relation,
            bool reversePerspective) =>
            new(
                source,
                target,
                relation,
                source.Site,
                target.Site,
                source.Site,
                DetermineTowardAnchor(relation, source.Site, target.Site),
                reversePerspective,
                source.TrailRelations);

        private static bool DetermineTowardAnchor(Relation relation, Site fromSite, Site toSite)
        {
            var from = fromSite.On(relation.Carrier).Normalize();
            var to = toSite.On(relation.Carrier).Normalize();
            var origin = relation.OriginPosition.Normalize();
            var anchor = relation.AnchorPosition.Normalize();

            if (from == origin && to == anchor)
            {
                return true;
            }

            if (from == anchor && to == origin)
            {
                return false;
            }

            if (from == origin)
            {
                return true;
            }

            if (from == anchor)
            {
                return false;
            }

            if (to == anchor)
            {
                return true;
            }

            if (to == origin)
            {
                return false;
            }

            return relation.Orientation.Raw >= 0L;
        }
    }
}
