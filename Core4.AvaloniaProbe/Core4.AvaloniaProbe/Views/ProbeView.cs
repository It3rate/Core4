using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Core4.AvaloniaProbe.Services;
using Core4.AvaloniaProbe.Rendering;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// Top-level shell for the Avalonia probe. It hosts the banner and preset selection while each page owns its own
/// Core4 scene and peripheral controls.
/// </summary>
public sealed class ProbeView : UserControl
{
    private readonly DominantOnlyPageView _dominantOnlyPage = new();
    private readonly InvertedPairPageView _invertedPairPage = new();
    private readonly FrozenSegmentPageView _frozenSegmentPage = new();
    private readonly ContentControl _pageHost;
    private readonly Button _dominantButton;
    private readonly Button _invertedButton;
    private readonly Button _frozenButton;
    private ProbePage _activePage = ProbePage.DominantOnly;

    public ProbeView()
    {
        Background = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0d, 0d, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0d, 1d, RelativeUnit.Relative),
            GradientStops =
            [
                new GradientStop(Color.Parse("#fbf8f3"), 0d),
                new GradientStop(Color.Parse("#f1eadf"), 1d)
            ]
        };

        _dominantButton = ProbeUi.PillButton("Dominant Only", true);
        _dominantButton.Click += (_, _) => SetPage(ProbePage.DominantOnly);

        _invertedButton = ProbeUi.PillButton("Inverted Pair");
        _invertedButton.Click += (_, _) => SetPage(ProbePage.InvertedPair);

        _frozenButton = ProbeUi.PillButton("Frozen Segment");
        _frozenButton.Click += (_, _) => SetPage(ProbePage.FrozenSegment);

        var presetButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                _dominantButton,
                _invertedButton,
                _frozenButton
            }
        };

        var presetGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto")
        };
        presetGrid.Children.Add(presetButtons);

        if (!OperatingSystem.IsBrowser())
        {
            var webButton = ProbeUi.PillButton("Web");
            webButton.HorizontalAlignment = HorizontalAlignment.Right;
            webButton.Click += async (_, _) =>
            {
                webButton.IsEnabled = false;
                try
                {
                    await WebPreviewLauncher.LaunchAsync();
                }
                catch (Exception ex)
                {
                    webButton.Content = "Web failed";
                    ToolTip.SetTip(webButton, ex.Message);
                }
                finally
                {
                    webButton.IsEnabled = true;
                }
            };
            presetGrid.Children.Add(webButton);
            Grid.SetColumn(webButton, 1);
        }

        _pageHost = new ContentControl
        {
            Content = _dominantOnlyPage
        };

        Content = new ScrollViewer
        {
            Content = new StackPanel
            {
                Spacing = 10,
                Margin = new Thickness(8),
                Children =
                {
                    Banner(),
                    presetGrid,
                    _pageHost
                }
            }
        };
    }

    private Border Banner()
    {
        var titleStack = new StackPanel
        {
            Spacing = 2,
            Children =
            {
                ProbeUi.Mono("CORE4 DISPLAY", 10d, ProbePalette.Muted),
                ProbeUi.DisplayTitle("Numeric Explorer", 22d),
                ProbeUi.Body("Measure, inversion, freezing, and orthogonal tension in one compact view.", 10d)
            }
        };

        return new Border
        {
            CornerRadius = new CornerRadius(24),
            Background = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0d, 0d, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1d, 1d, RelativeUnit.Relative),
                GradientStops =
                [
                    new GradientStop(Color.Parse("#f5f0e4"), 0d),
                    new GradientStop(Color.Parse("#f8f4ec"), 0.55d),
                    new GradientStop(Color.Parse("#f3dfc6"), 1d)
                ]
            },
            BorderBrush = new SolidColorBrush(Color.Parse("#dfd7c8")),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(14, 10),
            Child = titleStack
        };
    }

    private void SetPage(ProbePage page)
    {
        if (_activePage == page)
        {
            return;
        }

        _activePage = page;
        _pageHost.Content = page switch
        {
            ProbePage.DominantOnly => _dominantOnlyPage,
            ProbePage.InvertedPair => _invertedPairPage,
            ProbePage.FrozenSegment => _frozenSegmentPage,
            _ => _dominantOnlyPage
        };
        ProbeUi.ApplyPillStyle(_dominantButton, page == ProbePage.DominantOnly);
        ProbeUi.ApplyPillStyle(_invertedButton, page == ProbePage.InvertedPair);
        ProbeUi.ApplyPillStyle(_frozenButton, page == ProbePage.FrozenSegment);
    }

    private enum ProbePage
    {
        DominantOnly,
        InvertedPair,
        FrozenSegment
    }
}
