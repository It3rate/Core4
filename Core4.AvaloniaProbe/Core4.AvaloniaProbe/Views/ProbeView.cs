using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;
using Core4.AvaloniaProbe.Services;
using Core4.Numerics;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// A compact dominant-only explorer page. Avalonia hosts the peripheral widgets, while the scene remains a
/// code-rendered Core4 view.
/// </summary>
public sealed class ProbeView : UserControl
{
    private const long MaximumSpan = 16L;
    private readonly DominantOnlySceneControl _scene;
    private readonly MeasureSliderControl _spanSlider;
    private readonly MeasureSliderControl _valueSlider;
    private readonly TextBox _spanBox;
    private readonly TextBox _valueBox;
    private readonly TextBlock _bannerReadingText;
    private readonly TextBlock _bannerSettledText;
    private readonly TextBlock _sceneReadingText;
    private readonly TextBlock _sceneOriginText;
    private readonly TextBlock _sceneSettledText;
    private readonly TextBlock _freezeText;
    private readonly TextBlock _readingCardText;
    private readonly TextBlock _orthogonalText;
    private readonly TextBlock _settlementText;
    private DominantOnlyDemoState _state = DominantOnlyDemoState.Default;
    private bool _syncing;

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

        _scene = new DominantOnlySceneControl
        {
            State = _state,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            MinHeight = 286
        };
        _scene.StateChanged += (_, state) =>
        {
            _state = state;
            SyncUiFromState();
        };

        _spanSlider = CreateSlider(1, 16, _state.DominantSpan.Raw, ProbePalette.DominantSoft, true);
        _spanSlider.ValueChanged += (_, value) =>
        {
            if (_syncing)
            {
                return;
            }

            ApplyState(_state.WithDominantSpan(new Measure(value)));
        };

        _valueSlider = CreateSlider(-24, 24, _state.ValueOffset.Raw, ProbePalette.Dominant, false);
        _valueSlider.ValueChanged += (_, value) =>
        {
            if (_syncing)
            {
                return;
            }

            ApplyState(_state.WithValueOffset(new Measure(value)));
        };

        _spanBox = CreateNumberBox(_state.DominantSpan.Raw.ToString());
        HookBox(_spanBox, raw => ApplyState(_state.WithDominantSpan(new Measure(raw))));

        _valueBox = CreateNumberBox(_state.ValueOffset.Raw.ToString());
        HookBox(_valueBox, raw => ApplyState(_state.WithValueOffset(new Measure(raw))));

        _bannerReadingText = Mono(string.Empty, 11d, ProbePalette.Muted);
        _bannerSettledText = Mono(string.Empty, 11d, ProbePalette.Muted);
        _sceneReadingText = Mono(string.Empty, 11d, ProbePalette.Muted);
        _sceneOriginText = Mono(string.Empty, 11d, ProbePalette.Muted);
        _sceneSettledText = Mono(string.Empty, 11d, ProbePalette.Muted);
        _freezeText = Mono(string.Empty, 10d, ProbePalette.Ink);
        _readingCardText = Mono(string.Empty, 10d, ProbePalette.Ink);
        _orthogonalText = Mono(string.Empty, 10d, ProbePalette.Ink);
        _settlementText = Mono(string.Empty, 10d, ProbePalette.Ink);

        var banner = Banner();

        var presetGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto")
        };
        var presetPills = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                Pill("Dominant Only", true)
            }
        };
        presetGrid.Children.Add(presetPills);

        if (!OperatingSystem.IsBrowser())
        {
            var webButton = PillButton("Web");
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

        var leftColumn = new StackPanel
        {
            Width = 188,
            Spacing = 6,
            Children =
            {
                SliderCard("Dominant Span", "Visible dominant reach.", _spanSlider, _spanBox),
                SliderCard("Value Offset", "Live reading from the origin.", _valueSlider, _valueBox)
            }
        };

        var sceneCard = Card(new StackPanel
        {
            Spacing = 8,
            Children =
            {
                SceneHeader(),
                PlotCard(_scene)
            }
        }, 22, new Thickness(12));

        var infoCards = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*,*"),
            ColumnSpacing = 10
        };
        AddInfoCard(infoCards, 0, "FREEZE LADDER", _freezeText);
        AddInfoCard(infoCards, 1, "READING", _readingCardText);
        AddInfoCard(infoCards, 2, "ORTHOGONAL FRONTIER", _orthogonalText);
        AddInfoCard(infoCards, 3, "SETTLEMENT TRACE", _settlementText);

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

        Content = new ScrollViewer
        {
            Content = new StackPanel
            {
                Spacing = 10,
                Margin = new Thickness(8),
                Children =
                {
                    banner,
                    presetGrid,
                    mainGrid
                }
            }
        };

        SyncUiFromState();
    }

    private Border Banner()
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
                Mono("CORE4 DISPLAY", 10d, ProbePalette.Muted),
                DisplayTitle("Numeric Explorer", 22d),
                Body("Measure, inversion, freezing, and orthogonal tension in one compact view.", 10d)
            }
        };

        var metaStack = new StackPanel
        {
            Spacing = 2,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children =
            {
                Mono("mode One-sided half-line", 11d, ProbePalette.Muted),
                _bannerReadingText,
                _bannerSettledText
            }
        };

        grid.Children.Add(titleStack);
        grid.Children.Add(metaStack);
        Grid.SetColumn(metaStack, 1);

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
            Child = grid
        };
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
                Mono("INTERACTIVE SCENE", 10d, ProbePalette.Muted),
                DisplayTitle("One-sided half-line", 18d),
                Body("A one-sided interval that reads only to the dominant side.", 10d)
            }
        };

        var metaStack = new StackPanel
        {
            Spacing = 2,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children =
            {
                _sceneReadingText,
                _sceneOriginText,
                _sceneSettledText
            }
        };

        grid.Children.Add(titleStack);
        grid.Children.Add(metaStack);
        Grid.SetColumn(metaStack, 1);
        return grid;
    }

    private void ApplyState(DominantOnlyDemoState state)
    {
        _state = Normalize(state);
        _scene.State = _state;
        SyncUiFromState();
    }

    private void SyncUiFromState()
    {
        _syncing = true;
        var valueExtent = Math.Max(24L, Math.Abs(_state.ValueOffset.Raw) + 8L);
        _valueSlider.Minimum = -valueExtent;
        _valueSlider.Maximum = valueExtent;
        _spanSlider.Value = _state.DominantSpan.Raw;
        _valueSlider.Value = _state.ValueOffset.Raw;
        _spanBox.Text = _state.DominantSpan.Raw.ToString();
        _valueBox.Text = _state.ValueOffset.Raw.ToString();
        _syncing = false;

        var dominantRatio = _state.IsHiddenInverse
            ? Ratio.Tension((_state.Origin - _state.ValueLocation).Absolute)
            : _state.Reading;
        var dominantApprox = _state.DominantSpan.Raw == 0 ? 0d : (double)_state.ValueOffset.Raw / _state.DominantSpan.Raw;
        var settled = Ratio.Tension(_state.ValueOffset.Absolute);

        _bannerReadingText.Text = $"reading 0/0r + {dominantRatio}d";
        _bannerSettledText.Text = $"settled {settled}";

        _sceneReadingText.Text = $"0/0r + {dominantRatio}d";
        _sceneOriginText.Text = $"origin {_state.Origin.Raw}";
        _sceneSettledText.Text = $"settled {settled}";

        _freezeText.Text =
            $"Origin                    {_state.Origin.Raw}\n" +
            $"Dominant                  0 -> {_state.DominantSpan.Raw}\n" +
            "Recessive                 not present\n" +
            "Structure                 single dominant interval";

        _readingCardText.Text =
            $"Complex                   0/0r + {dominantRatio}d\n" +
            $"Dominant                  {dominantRatio}\n" +
            "Recessive                 unavailable\n" +
            "Mode                      One-sided half-line\n" +
            $"Approx                    (0.0i + {dominantApprox:F1})";

        _orthogonalText.Text =
            "Tension                   0/0\n" +
            "Incoming                  0/0\n" +
            "Outgoing                  0/0\n" +
            "Connections               0";

        _settlementText.Text =
            "1 Anchor                  0/1\n" +
            "2 Incoming                0/0\n" +
            "3 Provisional             0/0\n" +
            $"4 Published               {settled}\n" +
            "5 Feedback                0/0\n" +
            $"6 Settled                 {settled}\n" +
            "7 Remainder               0/0";
    }

    private void HookBox(TextBox box, Action<long> apply)
    {
        box.LostFocus += (_, _) => TryApplyBox(box, apply);
        box.KeyUp += (_, args) =>
        {
            if (args.Key == Key.Enter)
            {
                TryApplyBox(box, apply);
            }
        };
    }

    private void TryApplyBox(TextBox box, Action<long> apply)
    {
        if (_syncing || !long.TryParse(box.Text, out var raw))
        {
            return;
        }

        apply(raw);
    }

    private static DominantOnlyDemoState Normalize(DominantOnlyDemoState state) =>
        state.WithDominantSpan(new Measure(Math.Clamp(state.DominantSpan.Raw, 1L, MaximumSpan)));

    private static MeasureSliderControl CreateSlider(long minimum, long maximum, long value, IBrush accent, bool hollow)
    {
        return new MeasureSliderControl
        {
            Minimum = minimum,
            Maximum = maximum,
            Value = value,
            AccentBrush = accent,
            TrackBrush = new SolidColorBrush(Color.Parse("#cddaf7")),
            HollowThumb = hollow,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
    }

    private static TextBox CreateNumberBox(string value) => new()
    {
        Text = value,
        Background = Brushes.Transparent,
        BorderThickness = new Thickness(0),
        FontSize = 11,
        FontFamily = ProbePalette.MonoFont,
        Padding = new Thickness(7, 2),
        Height = 22,
        HorizontalContentAlignment = HorizontalAlignment.Left,
        VerticalContentAlignment = VerticalAlignment.Center
    };

    private static Border SliderCard(string title, string description, MeasureSliderControl slider, TextBox inputBox) => new()
    {
        Background = new SolidColorBrush(Color.Parse("#f3ecdf")),
        BorderBrush = new SolidColorBrush(Color.Parse("#d8d2c6")),
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(16),
        Padding = new Thickness(8, 6),
        Child = new StackPanel
        {
            Spacing = 2,
            Children =
            {
                Header(title, 11d),
                Body(description, 9d),
                new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("*,52"),
                    ColumnSpacing = 6,
                    Children =
                    {
                        slider,
                        InputHost(inputBox)
                    }
                }
            }
        }
    };

    private static Border InputHost(TextBox inputBox)
    {
        var host = new Border
        {
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.Parse("#d4d8df")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Height = 24,
            Child = inputBox
        };

        Grid.SetColumn(host, 1);
        return host;
    }

    private static Border PlotCard(Control child) => new()
    {
        Background = ProbePalette.Surface,
        BorderBrush = new SolidColorBrush(Color.Parse("#ddd6ca")),
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(24),
        Padding = new Thickness(6),
        Child = child
    };

    private static Border Card(Control child, double radius = 22d, Thickness? padding = null) => new()
    {
        Background = ProbePalette.Surface,
        BorderBrush = new SolidColorBrush(Color.Parse("#ddd6ca")),
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(radius),
        Padding = padding ?? new Thickness(9),
        Child = child
    };

    private static Border Pill(string text, bool active) => new()
    {
        Background = active
            ? new SolidColorBrush(Color.Parse("#e8f1fd"))
            : ProbePalette.Surface,
        BorderBrush = active
            ? new SolidColorBrush(Color.Parse("#bcd4f8"))
            : new SolidColorBrush(Color.Parse("#d7d0c4")),
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(15),
        Padding = new Thickness(12, 5),
        Child = new TextBlock
        {
            Text = text,
            FontSize = 11,
            FontFamily = ProbePalette.UiFont,
            FontWeight = FontWeight.SemiBold,
            Foreground = ProbePalette.Ink
        }
    };

    private static Button PillButton(string text) => new()
    {
        Content = text,
        FontSize = 11,
        FontFamily = ProbePalette.UiFont,
        FontWeight = FontWeight.SemiBold,
        Background = ProbePalette.Surface,
        Foreground = ProbePalette.Ink,
        BorderBrush = new SolidColorBrush(Color.Parse("#d7d0c4")),
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(14),
        Padding = new Thickness(10, 5),
        HorizontalAlignment = HorizontalAlignment.Right
    };

    private static void AddInfoCard(Grid grid, int column, string title, TextBlock body)
    {
        var card = Card(new StackPanel
        {
            Spacing = 6,
            Children =
            {
                Mono(title, 10d, ProbePalette.Muted),
                body
            }
        }, 20, new Thickness(12, 10));

        grid.Children.Add(card);
        Grid.SetColumn(card, column);
    }

    private static TextBlock DisplayTitle(string text, double size) => new()
    {
        Text = text,
        FontSize = size,
        FontWeight = FontWeight.SemiBold,
        TextWrapping = TextWrapping.Wrap,
        Foreground = ProbePalette.Ink,
        FontFamily = ProbePalette.DisplayFont,
        LineHeight = size * 1.05d
    };

    private static TextBlock Body(string text, double size) => new()
    {
        Text = text,
        FontSize = size,
        TextWrapping = TextWrapping.Wrap,
        Foreground = ProbePalette.Muted,
        FontFamily = ProbePalette.UiFont
    };

    private static TextBlock Header(string text, double size) => new()
    {
        Text = text,
        FontSize = size,
        FontWeight = FontWeight.SemiBold,
        TextWrapping = TextWrapping.Wrap,
        Foreground = ProbePalette.Ink,
        FontFamily = ProbePalette.UiFont
    };

    private static TextBlock Mono(string text, double size, IBrush brush) => new()
    {
        Text = text,
        FontSize = size,
        FontFamily = ProbePalette.MonoFont,
        TextWrapping = TextWrapping.Wrap,
        Foreground = brush
    };
}
