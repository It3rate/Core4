using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;

namespace Core4.AvaloniaProbe.Controls;

/// <summary>
/// Interactive scene for the first measurement glossary page. Clicking a floating raw value starts a short
/// negotiation animation that snaps it onto the current unit denominator.
/// </summary>
public sealed class GlossaryMeasurementSceneControl : Control
{
    private readonly DispatcherTimer _animationTimer;
    private DateTimeOffset _animationStartedAtUtc;
    private GlossaryMeasurementDemoState _state = GlossaryMeasurementDemoState.CreateDefault();

    public GlossaryMeasurementSceneControl()
    {
        MinHeight = 348d;
        MinWidth = 760d;
        ClipToBounds = true;

        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33d)
        };
        _animationTimer.Tick += (_, _) => AdvanceAnimation();
    }

    public GlossaryMeasurementDemoState State
    {
        get => _state;
        set
        {
            if (_state == value)
            {
                return;
            }

            _state = value;
            if (!_state.IsAnimating)
            {
                _animationTimer.Stop();
            }

            InvalidateVisual();
            StateChanged?.Invoke(this, _state);
        }
    }

    public event EventHandler<GlossaryMeasurementDemoState>? StateChanged;

    internal GlossaryMeasurementSceneLayout CurrentLayout => GlossaryMeasurementRenderer.Arrange(Bounds, _state);

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        GlossaryMeasurementRenderer.Render(context, Bounds, _state);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (_state.IsAnimating)
        {
            return;
        }

        var point = e.GetPosition(this);
        foreach (var candidate in CurrentLayout.Candidates)
        {
            if (!candidate.Card.Contains(point))
            {
                continue;
            }

            StartAnimation(candidate.ValueId);
            e.Handled = true;
            return;
        }
    }

    private void StartAnimation(long valueId)
    {
        State = _state.BeginAttach(valueId);
        if (!_state.IsAnimating)
        {
            return;
        }

        _animationStartedAtUtc = DateTimeOffset.UtcNow;
        _animationTimer.Start();
    }

    private void AdvanceAnimation()
    {
        if (!_state.IsAnimating)
        {
            _animationTimer.Stop();
            return;
        }

        var progress = (DateTimeOffset.UtcNow - _animationStartedAtUtc).TotalSeconds / GlossaryMeasurementDemoState.AnimationDurationSeconds;
        if (progress >= 1d)
        {
            State = _state.CompleteAnimation();
            _animationTimer.Stop();
            return;
        }

        State = _state.WithAnimationProgress(progress);
    }
}
