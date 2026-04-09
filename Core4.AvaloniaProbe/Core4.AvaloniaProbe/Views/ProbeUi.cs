using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Rendering;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// Shared small UI builders for the Avalonia probe pages so each page can stay focused on Core4 scene state.
/// </summary>
internal static class ProbeUi
{
    public static MeasureSliderControl CreateSlider(long minimum, long maximum, long value, IBrush accent, bool hollow)
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

    public static TextBox CreateNumberBox(string value) => new()
    {
        Text = value,
        Background = Brushes.Transparent,
        BorderThickness = new Thickness(0),
        FontSize = 11,
        FontFamily = ProbePalette.MonoFont,
        Padding = new Thickness(10.5, 2, 4.5, 2),
        Height = 22,
        HorizontalContentAlignment = HorizontalAlignment.Left,
        VerticalContentAlignment = VerticalAlignment.Center
    };

    public static Border SliderCard(string title, string description, MeasureSliderControl slider, TextBox inputBox) => new()
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

    public static Border InputHost(TextBox inputBox)
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

    public static Border PlotCard(Control child) => new()
    {
        Background = ProbePalette.Surface,
        BorderBrush = new SolidColorBrush(Color.Parse("#ddd6ca")),
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(24),
        Padding = new Thickness(6),
        Child = child
    };

    public static Border Card(Control child, double radius = 22d, Thickness? padding = null) => new()
    {
        Background = ProbePalette.Surface,
        BorderBrush = new SolidColorBrush(Color.Parse("#ddd6ca")),
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(radius),
        Padding = padding ?? new Thickness(9),
        Child = child
    };

    public static Button PillButton(string text, bool active = false)
    {
        var button = new Button
        {
            Content = text,
            FontSize = 11,
            FontFamily = ProbePalette.UiFont,
            FontWeight = FontWeight.SemiBold,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(11, 3)
        };

        ApplyPillStyle(button, active);
        return button;
    }

    public static Button SectionButton(string text, bool active = false)
    {
        var button = new Button
        {
            Content = text,
            FontSize = 12,
            FontFamily = ProbePalette.UiFont,
            FontWeight = FontWeight.SemiBold,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(12, 4)
        };

        ApplySectionStyle(button, active);
        return button;
    }

    public static void ApplyPillStyle(Button button, bool active)
    {
        button.Background = active
            ? new SolidColorBrush(Color.Parse("#e8f1fd"))
            : ProbePalette.Surface;
        button.BorderBrush = active
            ? new SolidColorBrush(Color.Parse("#bcd4f8"))
            : new SolidColorBrush(Color.Parse("#d7d0c4"));
        button.Foreground = ProbePalette.Ink;
    }

    public static void ApplySectionStyle(Button button, bool active)
    {
        button.Background = active
            ? ProbePalette.Ink
            : new SolidColorBrush(Color.Parse("#efe6d9"));
        button.BorderBrush = active
            ? new SolidColorBrush(Color.Parse("#243336"))
            : new SolidColorBrush(Color.Parse("#ccbda8"));
        button.Foreground = active
            ? ProbePalette.Surface
            : ProbePalette.Ink;
    }

    public static void AddInfoCard(Grid grid, int column, string title, TextBlock body)
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

    public static TextBlock DisplayTitle(string text, double size) => new()
    {
        Text = text,
        FontSize = size,
        FontWeight = FontWeight.SemiBold,
        TextWrapping = TextWrapping.Wrap,
        Foreground = ProbePalette.Ink,
        FontFamily = ProbePalette.DisplayFont,
        LineHeight = size * 1.05d
    };

    public static TextBlock Body(string text, double size) => new()
    {
        Text = text,
        FontSize = size,
        TextWrapping = TextWrapping.Wrap,
        Foreground = ProbePalette.Muted,
        FontFamily = ProbePalette.UiFont
    };

    public static TextBlock Header(string text, double size) => new()
    {
        Text = text,
        FontSize = size,
        FontWeight = FontWeight.SemiBold,
        TextWrapping = TextWrapping.Wrap,
        Foreground = ProbePalette.Ink,
        FontFamily = ProbePalette.UiFont
    };

    public static TextBlock Mono(string text, double size, IBrush brush) => new()
    {
        Text = text,
        FontSize = size,
        FontFamily = ProbePalette.MonoFont,
        TextWrapping = TextWrapping.Wrap,
        Foreground = brush
    };
}
