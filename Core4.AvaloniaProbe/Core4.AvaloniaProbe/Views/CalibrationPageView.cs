using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;

namespace Core4.AvaloniaProbe.Views;

public sealed class CalibrationPageView : UserControl
{
    private readonly Grid _framePanel;
    private readonly Button[] _datasetButtons;
    private CalibrationDataset _dataset = CalibrationDemoCatalog.Datasets[0];

    public CalibrationPageView()
    {
        _framePanel = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("208,208,208,208,208"),
            ColumnSpacing = 10d
        };

        _datasetButtons = new Button[CalibrationDemoCatalog.Datasets.Length];
        var datasetRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6d
        };

        for (var index = 0; index < CalibrationDemoCatalog.Datasets.Length; index++)
        {
            var dataset = CalibrationDemoCatalog.Datasets[index];
            var button = ProbeUi.PillButton(dataset.Label);
            var captured = dataset;
            button.Click += (_, _) => SetDataset(captured);
            _datasetButtons[index] = button;
            datasetRow.Children.Add(button);
        }

        Content = new StackPanel
        {
            Spacing = 8d,
            Children =
            {
                ProbeUi.Card(new StackPanel
                {
                    Spacing = 6d,
                    Children =
                    {
                        new WrapPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                ProbeUi.DisplayTitle("Rolling Calibrater", 18d),
                                ProbeUi.Body("A single site watches one repeated orthogonal input. The first sample becomes origin, the second becomes the unit marker, and the third becomes the projected value. Each later sample shifts the three-value window left.", 10.5d)
                            }
                        },
                        datasetRow,
                    }
                }, 22d, new Thickness(12, 10)),
                ProbeUi.Card(new ScrollViewer
                {
                    Height = 408d,
                    Content = _framePanel
                }, 22d, new Thickness(12, 12))
            }
        };

        SetDataset(_dataset);
    }

    private void SetDataset(CalibrationDataset dataset)
    {
        _dataset = dataset;

        for (var index = 0; index < CalibrationDemoCatalog.Datasets.Length; index++)
        {
            ProbeUi.ApplyPillStyle(_datasetButtons[index], CalibrationDemoCatalog.Datasets[index].Key == dataset.Key);
        }

        RebuildFrames();
    }

    private void RebuildFrames()
    {
        _framePanel.Children.Clear();
        _framePanel.RowDefinitions.Clear();

        var frames = CalibrationDemoCatalog.BuildFrames(_dataset);
        var columns = 5;
        var rows = (int)Math.Ceiling(frames.Length / (double)columns);

        for (var row = 0; row < rows; row++)
        {
            _framePanel.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }

        for (var index = 0; index < frames.Length; index++)
        {
            var frame = frames[index];
            var row = index % rows;
            var column = index / rows;
            var card = FrameCard(frame);
            _framePanel.Children.Add(card);
            Grid.SetRow(card, row);
            Grid.SetColumn(card, Math.Min(column, columns - 1));
        }
    }

    private Control FrameCard(CalibrationFrame frame)
    {
        var calibrater = frame.Calibrater;
        var rawLabel = _dataset.Format(frame.Sample);
        var ratioLabel = calibrater.HasValue
            ? calibrater.ValueProjection.IsTension
                ? "t"
                : calibrater.ValueProjection.Approximate.ToString("0.0")
            : "--";

        var graph = new CalibrationFrameControl
        {
            Calibrater = calibrater,
            RatioLabel = ratioLabel,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var status = !calibrater.HasUnit
            ? "origin"
            : !calibrater.HasValue
                ? "unit"
                : "value";

        var card = ProbeUi.Card(new StackPanel
        {
            Spacing = 1d,
            Children =
            {
                new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                    Children =
                    {
                        ProbeUi.Mono($"#{frame.Index:00}  {rawLabel}", 8.2d, ProbePalette.Ink),
                        ProbeUi.Mono(status, 8.2d, ProbePalette.Muted)
                    }
                },
                graph
            }
        }, 18d, new Thickness(6, 3));

        ((Grid)((StackPanel)card.Child!).Children[0]).Children[1].SetValue(Grid.ColumnProperty, 1);
        card.Width = 208d;
        card.Margin = new Thickness(0);
        return card;
    }
}
