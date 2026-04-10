using System;
using System.Collections.Immutable;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;
using Core4.Inspection;
using Core4.Numerics;
using Core4.Structure;
using Core4.Topology;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// Dedicated boundary and loop page. It keeps the carrier-scene visual language from the trolley explorer, but narrows
/// the presets to clamp, reflection, and closure so basis encounter can be inspected directly.
/// </summary>
public sealed class BoundaryLoopPageView : UserControl
{
    private const double MillisecondsPerUnit = 320d;
    private const double MinimumMotionMilliseconds = 880d;

    private readonly CarrierChainSceneControl _scene;
    private readonly Button _clampButton;
    private readonly Button _reflectButton;
    private readonly Button _closureButton;
    private readonly Button _invertedClosureButton;
    private readonly Button _continueButton;
    private readonly Button _autoButton;
    private readonly TextBlock _sceneDescriptionText;
    private readonly TextBlock _metaPresetText;
    private readonly TextBlock _metaFrontierText;
    private readonly TextBlock _metaChainText;
    private readonly TextBlock _currentText;
    private readonly TextBlock _encounterText;
    private readonly TextBlock _trailText;
    private readonly DispatcherTimer _animationTimer;
    private BoundaryPreset _preset = BoundaryPreset.ClampInterval;
    private ImmutableArray<TopologyTrolley> _frontiers = ImmutableArray<TopologyTrolley>.Empty;
    private ImmutableArray<BoundaryMotion> _motions = ImmutableArray<BoundaryMotion>.Empty;
    private ImmutableArray<TopologyTrolley> _pendingFrontiers = ImmutableArray<TopologyTrolley>.Empty;
    private DateTime _animationStartedUtc;
    private TimeSpan _activeMotionDuration;
    private PropagationWave _autoWave;

    public BoundaryLoopPageView()
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

        _clampButton = ProbeUi.PillButton("Clamp", true);
        _clampButton.Click += (_, _) => ApplyPreset(BoundaryPreset.ClampInterval);

        _reflectButton = ProbeUi.PillButton("Reflective");
        _reflectButton.Click += (_, _) => ApplyPreset(BoundaryPreset.ReflectiveInterval);

        _closureButton = ProbeUi.PillButton("Closure");
        _closureButton.Click += (_, _) => ApplyPreset(BoundaryPreset.ClosureLoop);

        _invertedClosureButton = ProbeUi.PillButton("Inverted Closure");
        _invertedClosureButton.Click += (_, _) => ApplyPreset(BoundaryPreset.InvertedClosure);

        _continueButton = ProbeUi.PillButton("Continue");
        _continueButton.Click += (_, _) => ContinuePreferred();

        _autoButton = ProbeUi.PillButton("Auto");
        _autoButton.Click += (_, _) => ToggleAuto();

        var resetButton = ProbeUi.PillButton("Reset");
        resetButton.Click += (_, _) => ResetWalk();

        _sceneDescriptionText = ProbeUi.Body(string.Empty, 10d);
        _metaPresetText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _metaFrontierText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _metaChainText = ProbeUi.Mono(string.Empty, 11d, ProbePalette.Muted);
        _currentText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);
        _encounterText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);
        _trailText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);

        var leftColumn = new StackPanel
        {
            Width = 212,
            Spacing = 6,
            Children =
            {
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 6,
                    Children =
                    {
                        ProbeUi.Mono("BOUNDARY PRESETS", 10d, ProbePalette.Muted),
                        _clampButton,
                        _reflectButton,
                        _closureButton,
                        _invertedClosureButton
                    }
                }, 20, new Thickness(12, 10)),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 6,
                    Children =
                    {
                        ProbeUi.Mono("MOVEMENT", 10d, ProbePalette.Muted),
                        _continueButton,
                        _autoButton,
                        resetButton
                    }
                }, 20, new Thickness(12, 10)),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        ProbeUi.Mono("READING", 10d, ProbePalette.Muted),
                        ProbeUi.Body("Pinned basis is still frozen structure. At a boundary, the same basis can be encountered actionably: retain the carried calibration, replace it, compose through local agreement, or stop and hold the unresolved remainder as local tension.", 10d)
                    }
                }, 20, new Thickness(12, 10)),
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        ProbeUi.Mono("NEXT", 10d, ProbePalette.Muted),
                        ProbeUi.Body("Leaky loop is intentionally held for the next pass. The first four presets keep the page centered on clamp, reflection, preserved closure, and inverted closure.", 10d)
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
        ProbeUi.AddInfoCard(infoCards, 1, "ENCOUNTER", _encounterText);
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
            ColumnDefinitions = new ColumnDefinitions("212,*"),
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
                ProbeUi.Mono("BOUNDARY AND LOOP", 10d, ProbePalette.Muted),
                ProbeUi.DisplayTitle("Boundary Explorer", 18d),
                _sceneDescriptionText
            }
        };

        var metaStack = new StackPanel
        {
            Spacing = 2,
            Width = 140,
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

    private void ApplyPreset(BoundaryPreset preset)
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
        var motions = ImmutableArray.CreateBuilder<BoundaryMotion>();

        foreach (var frontier in _frontiers)
        {
            if (frontier.TryContinuePreferred(out var moved))
            {
                next.Add(moved);
                motions.Add(BoundaryMotion.Forward(frontier, moved));
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

        if (!step.HasMotion && step.After.IsReturning)
        {
            // This page is about local boundary law, not the general outward-then-return wave.
            // If outbound motion has truly quiesced into a return phase, stop at the reached boundary.
            _autoWave = new PropagationWave(
                Core4.Numerics.Measure.Zero,
                Core4.Numerics.Measure.Zero,
                ImmutableArray<PropagationFrontier>.Empty,
                step.After.Active);
            _frontiers = _autoWave.VisibleTrolleys();
            SyncUi();
            return;
        }

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

    private void SyncUi()
    {
        ProbeUi.ApplyPillStyle(_clampButton, _preset.Label == BoundaryPreset.ClampInterval.Label);
        ProbeUi.ApplyPillStyle(_reflectButton, _preset.Label == BoundaryPreset.ReflectiveInterval.Label);
        ProbeUi.ApplyPillStyle(_closureButton, _preset.Label == BoundaryPreset.ClosureLoop.Label);
        ProbeUi.ApplyPillStyle(_invertedClosureButton, _preset.Label == BoundaryPreset.InvertedClosure.Label);

        var allowPresetChange = !HasActiveMotion && !IsAutoRunning;
        _clampButton.IsEnabled = allowPresetChange;
        _reflectButton.IsEnabled = allowPresetChange;
        _closureButton.IsEnabled = allowPresetChange;
        _invertedClosureButton.IsEnabled = allowPresetChange;
        _sceneDescriptionText.Text = _preset.Description;
        _metaPresetText.Text = _preset.Label;
        _metaFrontierText.Text = DescribeFrontierStatus();
        _metaChainText.Text = $"{_preset.Scene.Chains.Length} chains";
        _autoButton.Content = IsAutoRunning ? "Stop Auto" : "Auto";
        _scene.Overlay = BuildOverlay();
        _currentText.Text = DescribeCurrent();
        _encounterText.Text = DescribeEncounter();
        _trailText.Text = DescribeTrail();
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

        foreach (var annotation in _preset.Annotations)
        {
            AddReadout(readouts, new CarrierChainOverlayReadout(annotation.Site, annotation.Label, annotation.Offset));
        }

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

    private void RefreshMovementButtons()
    {
        if (HasActiveMotion || IsAutoRunning)
        {
            _continueButton.IsEnabled = false;
            _autoButton.IsEnabled = IsAutoRunning || !HasActiveMotion;
            return;
        }

        var hasPreferred = false;

        foreach (var frontier in _frontiers)
        {
            hasPreferred |= frontier.Observe().TryPreferred(out _);
        }

        _continueButton.IsEnabled = hasPreferred;
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
            builder.Append('\n');
            builder.Append("frontier ");
            builder.Append(index + 1);
            builder.Append("               ");
            builder.Append(SiteLabel(frontier.Site));
            builder.Append(MultiplicityLabel(frontier.Site));
            builder.Append('\n');
            builder.Append("direction                 ");
            builder.Append(frontier.Direction);
            builder.Append('\n');
            builder.Append("carried calibration       ");
            builder.Append(frontier.Signature.Calibration);
        }

        return builder.ToString();
    }

    private string DescribeEncounter()
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
            builder.Append("local basis               ");
            builder.Append(frontier.BasisEncounters.Length);

            foreach (var encounter in frontier.BasisEncounters)
            {
                builder.Append('\n');
                builder.Append(" - ");
                builder.Append(DescribeEncounter(encounter));
            }

            builder.Append('\n');
            builder.Append("forward continuation      ");

            if (!observation.TryPreferred(out var preferred))
            {
                builder.Append("none");
                continue;
            }

            builder.Append(ContinuationLabel(preferred));

            if (preferred.HasBasisAgreement)
            {
                builder.Append('\n');
                builder.Append("best agreement            ");
                builder.Append(DescribeAgreement(preferred.BestBasisAgreement));
            }

            builder.Append('\n');
            builder.Append("transfer                  ");
            builder.Append(preferred.TransmittedCalibration);

            if (preferred.HasRetention)
            {
                builder.Append('\n');
                builder.Append("retained local            ");
                builder.Append(preferred.RetainedCalibration);
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

            for (var siteIndex = 0; siteIndex < frontier.TrailSites.Length; siteIndex++)
            {
                if (siteIndex > 0)
                {
                    builder.Append(" -> ");
                }

                builder.Append(SiteLabel(frontier.TrailSites[siteIndex]));
            }

            if (frontier.ExactReentryPeriod != Core4.Numerics.Measure.Zero)
            {
                builder.Append('\n');
                builder.Append("exact re-entry            ");
                builder.Append(frontier.ExactReentryPeriod);
            }
        }

        if (_autoWave.IsPeriodic)
        {
            builder.Append("\n\nperiodic                 ");
            builder.Append(_autoWave.Period);
        }

        return builder.ToString();
    }

    private string DescribeFrontierStatus()
    {
        if (_autoWave.IsPeriodic)
        {
            return $"period {_autoWave.Period}";
        }

        var count = HasActiveMotion ? MotionFrontierCount() : _frontiers.Length;
        var label = count.ToString();

        if (HasActiveMotion)
        {
            return $"{label} moving";
        }

        if (IsAutoRunning)
        {
            return _autoWave.IsOutbound
                ? $"{label} out"
                : $"{label} back";
        }

        return label;
    }

    private static string DescribeEncounter(SiteBasisEncounter encounter)
    {
        var kind = encounter.IsExplicitPinnedBasis
            ? "explicit"
            : encounter.IsBilateral
                ? "bilateral"
                : "local";
        var sidedness = encounter.HasRecessiveCalibration && encounter.HasDominantCalibration
            ? "dual"
            : encounter.HasDominantCalibration
                ? "dominant"
                : encounter.HasRecessiveCalibration
                    ? "recessive"
                    : "blocked";
        return $"{kind} {sidedness} c {encounter.Commitment}";
    }

    private static string DescribeAgreement(BasisEncounterAgreement agreement)
    {
        var continuation = agreement.PreservesDirection
            ? "preserve"
            : agreement.InvertsDirection
                ? "invert"
                : "block";
        var capability = agreement.IsBilateral ? "bilateral" : agreement.IsOneSided ? "one-sided" : "none";
        return $"{continuation} {capability} c {agreement.Commitment}";
    }

    private static string ContinuationLabel(TopologyTrolleyContinuation continuation)
    {
        var label = $"{CarrierLabel(continuation.Carrier)}:{continuation.Potential.Raw} to {SiteLabel(continuation.To)}";

        if (continuation.IsPartialTransfer)
        {
            label += $" move {continuation.TransmittedCalibration}";

            if (continuation.HasRetention)
            {
                label += $" keep {continuation.RetainedCalibration}";
            }
        }

        return label;
    }

    private string MultiplicityLabel(Site site)
    {
        if (!IsAutoRunning && !_autoWave.IsPeriodic)
        {
            return string.Empty;
        }

        foreach (var frontier in _autoWave.VisibleFrontiers)
        {
            if (frontier.Site.Carrier == site.Carrier && frontier.Site.Location == site.Location)
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
        var atOrigin = frontier.Site.Carrier == relation.Origin.Carrier
            && frontier.Site.Location == relation.Origin.Location;
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

    private void StartAnimation(ImmutableArray<BoundaryMotion> motions, ImmutableArray<TopologyTrolley> nextFrontiers)
    {
        if (motions.IsDefaultOrEmpty)
        {
            _frontiers = nextFrontiers;
            SyncUi();

            if (IsAutoRunning)
            {
                Dispatcher.UIThread.Post(AdvanceAuto, DispatcherPriority.Background);
            }

            return;
        }

        var visible = FilterVisibleMotions(_scene.CurrentLayout, motions);

        if (visible.IsDefaultOrEmpty)
        {
            _frontiers = nextFrontiers;
            SyncUi();

            if (IsAutoRunning)
            {
                Dispatcher.UIThread.Post(AdvanceAuto, DispatcherPriority.Background);
            }

            return;
        }

        _motions = visible;
        _pendingFrontiers = nextFrontiers;
        _animationStartedUtc = DateTime.UtcNow;
        _activeMotionDuration = ResolveMotionDuration(visible);
        _animationTimer.Start();
        SyncUi();
    }

    private void StopAnimation()
    {
        _animationTimer.Stop();
        _motions = ImmutableArray<BoundaryMotion>.Empty;
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

    private TimeSpan ResolveMotionDuration(ImmutableArray<BoundaryMotion> motions)
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
        if (_activeMotionDuration <= TimeSpan.Zero)
        {
            return 1d;
        }

        var elapsed = DateTime.UtcNow - _animationStartedUtc;
        return Math.Clamp(elapsed.TotalMilliseconds / _activeMotionDuration.TotalMilliseconds, 0d, 1d);
    }

    private static void AddSite(ImmutableArray<Site>.Builder sites, Site site)
    {
        foreach (var existing in sites)
        {
            if (existing.Carrier == site.Carrier && existing.Location == site.Location)
            {
                return;
            }
        }

        sites.Add(site);
    }

    private static void AddReadout(
        ImmutableArray<CarrierChainOverlayReadout>.Builder readouts,
        CarrierChainOverlayReadout readout)
    {
        foreach (var existing in readouts)
        {
            if (existing.Site.Carrier == readout.Site.Carrier
                && existing.Site.Location == readout.Site.Location
                && existing.Label == readout.Label)
            {
                return;
            }
        }

        readouts.Add(readout);
    }

    private static void AddMotion(
        ImmutableArray<CarrierChainOverlayMotion>.Builder motions,
        CarrierChainOverlayMotion motion)
    {
        for (var index = 0; index < motions.Count; index++)
        {
            var existing = motions[index];

            if (existing.Relation == motion.Relation
                && existing.From == motion.From
                && existing.To == motion.To)
            {
                motions[index] = motion;
                return;
            }
        }

        motions.Add(motion);
    }

    private static ImmutableArray<BoundaryMotion> MapMoves(ImmutableArray<PropagationMove> moves)
    {
        var mapped = ImmutableArray.CreateBuilder<BoundaryMotion>(moves.Length);

        foreach (var move in moves)
        {
            mapped.Add(BoundaryMotion.Move(
                move.Source,
                move.Target,
                move.Relation,
                reversePerspective: move.Perspective.Raw < 0L));
        }

        return mapped.ToImmutable();
    }

    private static ImmutableArray<BoundaryMotion> FilterVisibleMotions(
        CarrierChainSceneLayout layout,
        ImmutableArray<BoundaryMotion> motions)
    {
        var visible = ImmutableArray.CreateBuilder<BoundaryMotion>(motions.Length);

        foreach (var motion in motions)
        {
            if (HasVisibleEdge(layout, motion.Relation))
            {
                visible.Add(motion);
            }
        }

        return visible.ToImmutable();
    }

    private static bool ShouldUseTensionPath(BoundaryMotion motion) => !motion.TowardAnchor;

    private static double RelationMagnitude(CarrierChainSceneLayout layout, Relation relation)
    {
        foreach (var edge in layout.Edges)
        {
            if (edge.Relation == relation)
            {
                return edge.ActualMagnitude;
            }
        }

        if (TryFindHub(layout, relation.Origin, out var fromHub)
            && TryFindHub(layout, relation.Anchor, out var toHub))
        {
            return ScenePrimitives.Distance(fromHub.Point, toHub.Point) / 72d;
        }

        return Math.Abs(relation.Potential.Raw);
    }

    private static bool TryFindHub(CarrierChainSceneLayout layout, Site site, out CarrierChainHubLayout hub)
    {
        foreach (var candidate in layout.Hubs)
        {
            foreach (var member in candidate.MemberSites)
            {
                if (member.Carrier == site.Carrier && member.Location == site.Location)
                {
                    hub = candidate;
                    return true;
                }
            }
        }

        hub = default;
        return false;
    }

    private static bool HasVisibleEdge(CarrierChainSceneLayout layout, Relation relation)
    {
        foreach (var edge in layout.Edges)
        {
            if (edge.Relation == relation)
            {
                return true;
            }
        }

        return false;
    }

    private static string CarrierLabel(Carrier carrier) => carrier.Id switch
    {
        1L => "x",
        2L => "y",
        3L => "z",
        4L => "w",
        _ => $"c{carrier.Id}"
    };

    private static string SiteLabel(Site site) => $"{CarrierLabel(site.Carrier)}{site.Location.Raw}";

    private readonly record struct BoundaryPreset(
        string Label,
        string Description,
        CarrierChainDemoState Scene,
        Site StartSite,
        ImmutableArray<BoundaryAnnotation> Annotations)
    {
        public static BoundaryPreset ClampInterval => new(
            "Clamp Interval",
            "One plain bounded span. The far end exposes no explicit pinned basis, so the trolley can arrive there but not continue through local basis encounter.",
            CarrierChainDemoState.ClampInterval,
            Site.Open(new Carrier(1), new Measure(0)),
            [new BoundaryAnnotation(Site.Open(new Carrier(1), new Measure(10)), "open", new Vector(0d, -28d))]);

        public static BoundaryPreset ReflectiveInterval => new(
            "Reflective Interval",
            "Both ends carry inward local basis. The encountered basis can replace or invert the trolley's local reading so the same span behaves like a football-field shuttle.",
            CarrierChainDemoState.ReflectiveInterval,
            Site.Open(new Carrier(1), new Measure(0)),
            [
                new BoundaryAnnotation(Site.Open(new Carrier(1), new Measure(0)), "basis ->", new Vector(-10d, -30d)),
                new BoundaryAnnotation(Site.Open(new Carrier(1), new Measure(10)), "basis <-", new Vector(2d, -30d))
            ]);

        public static BoundaryPreset ClosureLoop => new(
            "Closure Loop",
            "The far boundary carries a known remap target. Instead of stopping or reflecting, the trolley closes onto that site and the walk can complete as exact periodic re-entry.",
            CarrierChainDemoState.ClosureLoop,
            Site.Open(new Carrier(1), new Measure(0)),
            [new BoundaryAnnotation(Site.Open(new Carrier(1), new Measure(10)), "wrap -> x0", new Vector(10d, -30d))]);

        public static BoundaryPreset InvertedClosure => new(
            "Inverted Closure",
            "Closure still remaps onto a known site, but the encountered basis aligns with the arriving run. The closure is real, yet the trolley's own perspective flips at the seam.",
            CarrierChainDemoState.InvertedClosure,
            Site.Open(new Carrier(1), new Measure(0)),
            [
                new BoundaryAnnotation(Site.Open(new Carrier(1), new Measure(10)), "wrap + invert", new Vector(18d, -30d)),
                new BoundaryAnnotation(Site.Open(new Carrier(1), new Measure(14)), "target", new Vector(0d, -28d))
            ]);
    }

    private readonly record struct BoundaryAnnotation(Site Site, string Label, Vector Offset);

    private readonly record struct BoundaryMotion(
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
        public static BoundaryMotion Forward(TopologyTrolley source, TopologyTrolley target) =>
            Move(source, target, target.TrailRelations[^1], reversePerspective: false);

        public static BoundaryMotion Move(
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
