using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Models;
using Core4.Numerics;

namespace Core4.AvaloniaProbe.Views;

/// <summary>
/// A tiny code-only shell around the render surface. Avalonia widgets stay at the periphery, while the scene itself
/// owns the draw calls and pointer interaction.
/// </summary>
public sealed class ProbeView : UserControl
{
    private readonly DirectedSegmentSceneControl _scene;
    private readonly TextBox _lengthBox;
    private readonly TextBlock _readingText;
    private readonly TextBlock _segmentText;
    private DirectedSegmentSceneState _state = DirectedSegmentSceneState.Default;

    public ProbeView()
    {
        _scene = new DirectedSegmentSceneControl
        {
            State = _state,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        _scene.StateChanged += (_, state) =>
        {
            _state = state;
            SyncUiFromState();
        };

        _lengthBox = new TextBox
        {
            Text = _state.Length.Raw.ToString(CultureInfo.InvariantCulture),
            Watermark = "total length",
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var applyButton = new Button
        {
            Content = "Apply Length",
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        applyButton.Click += (_, _) => ApplyTypedLength();

        var resetButton = new Button
        {
            Content = "Reset",
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        resetButton.Click += (_, _) =>
        {
            _state = DirectedSegmentSceneState.Default;
            _scene.State = _state;
            SyncUiFromState();
        };

        _readingText = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            FontSize = 15,
            Foreground = new SolidColorBrush(Color.Parse("#2c3a44"))
        };

        _segmentText = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.Parse("#697882"))
        };

        var sideBar = new StackPanel
        {
            Width = 270,
            Spacing = 12,
            Margin = new Thickness(0, 0, 16, 0),
            Children =
            {
                Header("Core4 Avalonia Probe", 28),
                Body("This is a throwaway test: one code-driven render surface, two draggable endpoints, and one textbox/button path back into the shared Core4 state."),
                Header("Set Total Length", 18),
                _lengthBox,
                applyButton,
                resetButton,
                Header("Current Core4 Reading", 18),
                _readingText,
                Header("Structure", 18),
                _segmentText
            }
        };

        var sceneHost = new Border
        {
            CornerRadius = new CornerRadius(18),
            Background = new SolidColorBrush(Color.Parse("#f7f1e4")),
            BorderBrush = new SolidColorBrush(Color.Parse("#dfd7c6")),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(12),
            Child = _scene
        };

        var layout = new Grid
        {
            Margin = new Thickness(20),
            ColumnDefinitions = new ColumnDefinitions("Auto,*")
        };
        layout.Children.Add(sideBar);
        Grid.SetColumn(sideBar, 0);

        layout.Children.Add(sceneHost);
        Grid.SetColumn(sceneHost, 1);

        Content = layout;
        SyncUiFromState();
    }

    private void ApplyTypedLength()
    {
        if (!long.TryParse(_lengthBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var raw))
        {
            return;
        }

        _state = _state.WithTotalLength(new Measure(raw));
        _scene.State = _state;
        SyncUiFromState();
    }

    private void SyncUiFromState()
    {
        _lengthBox.Text = _state.Length.Raw.ToString(CultureInfo.InvariantCulture);
        _readingText.Text = $"{_state.RecessiveReading}r + {_state.DominantReading}d\napprox ({_state.Reading.Approximate:F2})";
        _segmentText.Text =
            $"segment ratio {_state.SegmentBasis.Ratio}\n" +
            $"recessive anchor {_state.RecessiveAnchor.Raw}, dominant anchor {_state.DominantAnchor.Raw}\n" +
            $"recessive position {_state.RecessivePosition.Raw}, dominant position {_state.DominantPosition.Raw}";
    }

    private static TextBlock Header(string text, double size) => new()
    {
        Text = text,
        FontSize = size,
        FontWeight = FontWeight.SemiBold,
        TextWrapping = TextWrapping.Wrap,
        Foreground = new SolidColorBrush(Color.Parse("#21303a"))
    };

    private static TextBlock Body(string text) => new()
    {
        Text = text,
        FontSize = 14,
        TextWrapping = TextWrapping.Wrap,
        Foreground = new SolidColorBrush(Color.Parse("#697882"))
    };
}
