using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;

namespace Core4.AvaloniaProbe.Controls;

/// <summary>
/// Interactive scene for pair fusion. Clicking source buttons stages one element, then a second click fuses the pair
/// and records the result in the gallery.
/// </summary>
public sealed class GlossaryFusionSceneControl : Control
{
    private readonly DispatcherTimer _animationTimer;
    private DateTimeOffset _animationStartedAtUtc;
    private GlossaryFusionDemoState _state = GlossaryFusionDemoState.CreateDefault();

    public GlossaryFusionSceneControl()
    {
        MinHeight = 430d;
        MinWidth = 900d;
        ClipToBounds = true;

        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33d)
        };
        _animationTimer.Tick += (_, _) => AdvanceAnimation();
    }

    public GlossaryFusionDemoState State
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

    public event EventHandler<GlossaryFusionDemoState>? StateChanged;

    internal GlossaryFusionSceneLayout CurrentLayout => GlossaryFusionRenderer.Arrange(Bounds, _state);

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        GlossaryFusionRenderer.Render(context, Bounds, _state);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (_state.IsAnimating)
        {
            return;
        }

        var point = e.GetPosition(this);
        foreach (var source in CurrentLayout.Sources)
        {
            if (!source.BoxRect.Contains(point))
            {
                continue;
            }

            StartSelection(source.SourceId);
            e.Handled = true;
            return;
        }
    }

    private void StartSelection(long sourceId)
    {
        State = _state.BeginSelect(sourceId);
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

        var progress = (DateTimeOffset.UtcNow - _animationStartedAtUtc).TotalSeconds / _state.CurrentAnimationDurationSeconds;
        if (progress >= 1d)
        {
            State = _state.CompleteAnimation();
            _animationTimer.Stop();
            return;
        }

        State = _state.WithAnimationProgress(progress);
    }
}
