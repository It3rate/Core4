using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;
using Core4.Calibration;
using Core4.Numerics;

namespace Core4.AvaloniaProbe.Views;

public sealed class CalibrationPageView : UserControl
{
    private readonly Grid _framePanel;
    private readonly Button[] _datasetButtons;
    private readonly ComboBox _strategyBox;
    private readonly MeasureSliderControl _shiftSlider;
    private readonly TextBlock _finalUnitText;
    private CalibrationDataset _dataset = CalibrationDemoCatalog.Datasets[0];
    private long _shiftPercent = 10;
    private bool _sorted;

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

        _strategyBox = new ComboBox
        {
            Width = 220d,
            ItemsSource = new[] { "Shift Basis Forward" },
            SelectedIndex = 0,
            FontSize = 11d,
            FontFamily = ProbePalette.UiFont,
            Background = ProbePalette.Surface,
            BorderBrush = new SolidColorBrush(Color.Parse("#d7d0c4")),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10, 4),
            CornerRadius = new CornerRadius(12)
        };

        _finalUnitText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Muted);

        _shiftSlider = ProbeUi.CreateSlider(1, 20, _shiftPercent, ProbePalette.Dominant, hollow: true);
        _shiftSlider.Width = 180d;
        _shiftSlider.ValueChanged += (_, value) =>
        {
            _shiftPercent = value;
            RebuildFrames();
        };

        var sortButton = ProbeUi.PillButton("Sort");
        sortButton.Click += (_, _) =>
        {
            _sorted = true;
            RebuildFrames();
        };

        var resetButton = ProbeUi.PillButton("Reset");
        resetButton.Click += (_, _) =>
        {
            _sorted = false;
            RebuildFrames();
        };

        var strategyRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,180,Auto,*,Auto"),
            ColumnSpacing = 8d,
            Children =
            {
                _strategyBox,
                _shiftSlider,
                sortButton,
                _finalUnitText,
                resetButton
            }
        };
        Grid.SetColumn(_shiftSlider, 1);
        Grid.SetColumn(sortButton, 2);
        Grid.SetColumn(_finalUnitText, 3);
        Grid.SetColumn(resetButton, 4);

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
                        strategyRow
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
        _sorted = false;

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

        var frames = BuildDisplayFrames();
        var columns = 5;
        var rows = (int)Math.Ceiling(frames.Count / (double)columns);

        for (var row = 0; row < rows; row++)
        {
            _framePanel.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }

        for (var index = 0; index < frames.Count; index++)
        {
            var frame = frames[index];
            var row = index % rows;
            var column = index / rows;
            var card = FrameCard(frame);
            _framePanel.Children.Add(card);
            Grid.SetRow(card, row);
            Grid.SetColumn(card, Math.Min(column, columns - 1));
        }

        var final = FinalCalibrater();
        _finalUnitText.Text = FinalSummary(final, frames);
    }

    private Control FrameCard(CalibrationDisplayFrame frame)
    {
        var calibrater = frame.Calibrater;
        var rawLabel = _sorted
            ? SortedValueLabel(frame.Calibrater)
            : _dataset.Format(frame.Sample);
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
                : frame.Status;

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
        if (_sorted)
        {
            card.BorderThickness = new Thickness(1.6d);
            card.BorderBrush = OutlineFor(frame.SortDistance);
        }
        return card;
    }

    private IReadOnlyList<CalibrationDisplayFrame> BuildDisplayFrames()
    {
        var progressive = BuildProgressiveFrames();
        if (!_sorted)
        {
            return progressive
                .Select(frame => new CalibrationDisplayFrame(frame.Index, frame.Sample, frame.Calibrater, frame.Stage, 0))
                .ToArray();
        }

        var final = progressive[^1].Calibrater;
        var idealOrder = _dataset.Values
            .Select((value, index) => new { value, index })
            .OrderBy(item => item.value.Raw)
            .ThenBy(item => item.index)
            .Select((item, order) => new { item.index, order })
            .ToDictionary(item => item.index, item => item.order);

        var sorted = new List<CalibrationDisplayFrame>(_dataset.Values.Length);
        for (var index = 0; index < _dataset.Values.Length; index++)
        {
            var sample = _dataset.Values[index];
            var projected = final.WithValue(sample);
            sorted.Add(new CalibrationDisplayFrame(index + 1, sample, projected, string.Empty, 0));
        }

        sorted = sorted
            .Select((frame, index) => new { frame, index })
            .OrderBy(item => SortKey(item.frame.Calibrater))
            .ThenBy(item => item.index)
            .Select(item => item.frame)
            .ToList();

        for (var displayIndex = 0; displayIndex < sorted.Count; displayIndex++)
        {
            var frame = sorted[displayIndex];
            var ideal = idealOrder[frame.Index - 1];
            var distance = Math.Abs(displayIndex - ideal);
            sorted[displayIndex] = frame with
            {
                Status = distance == 0 ? "ok" : $"off {distance}",
                SortDistance = distance
            };
        }

        return sorted;
    }

    private IReadOnlyList<CalibrationProgressiveFrame> BuildProgressiveFrames()
    {
        var calibrater = Calibrater.Start(_dataset.Carrier);
        var frames = new List<CalibrationProgressiveFrame>(_dataset.Values.Length);
        var shift = new Ratio(new Measure(_shiftPercent), new Measure(100));

        for (var index = 0; index < _dataset.Values.Length; index++)
        {
            var sample = _dataset.Values[index];
            calibrater = index < 3
                ? calibrater.Input(sample)
                : calibrater.InputShifted(sample, shift);
            frames.Add(new CalibrationProgressiveFrame(index + 1, sample, calibrater, StageFor(calibrater)));
        }

        return frames;
    }

    private Calibrater FinalCalibrater() => BuildProgressiveFrames()[^1].Calibrater;

    private static double SortKey(Calibrater calibrater)
    {
        if (!calibrater.HasValue)
        {
            return double.PositiveInfinity;
        }

        return calibrater.ValueProjection.IsTension
            ? double.PositiveInfinity
            : calibrater.ValueProjection.Approximate;
    }

    private static string StageFor(Calibrater calibrater)
        => !calibrater.HasUnit
            ? "origin"
            : !calibrater.HasValue
                ? "unit"
                : "value";

    private static IBrush OutlineFor(int distance)
    {
        if (distance == 0)
        {
            return new SolidColorBrush(Color.Parse("#8fcd8f"));
        }

        var alpha = (byte)Math.Min(220, 80 + (distance * 28));
        return new SolidColorBrush(Color.FromArgb(alpha, 204, 88, 77));
    }

    private static string SortedValueLabel(Calibrater calibrater)
    {
        if (!calibrater.HasValue)
        {
            return "--";
        }

        return calibrater.ValueProjection.IsTension
            ? "tension"
            : $"{calibrater.ValueProjection.Approximate:0.0}u";
    }

    private string FinalSummary(Calibrater final, IReadOnlyList<CalibrationDisplayFrame> frames)
    {
        var unitText = final.HasUnit
            ? $"final unit = {final.TickSize.Raw} ticks   origin = {final.OriginMeasure.Raw}"
            : "final unit = --";

        if (frames.Count == 0)
        {
            return unitText;
        }

        var resolved = frames
            .Where(frame => frame.Calibrater.HasValue && !frame.Calibrater.ValueProjection.IsTension)
            .Select(frame => frame.Calibrater.ValueProjection.Approximate)
            .OrderBy(value => value)
            .ToArray();

        if (!_sorted || resolved.Length == 0)
        {
            return unitText;
        }

        var minimum = resolved[0];
        var maximum = resolved[^1];
        return $"{unitText}   range {minimum:0.0} to {maximum:0.0}   span {(maximum - minimum):0.0}";
    }

    private readonly record struct CalibrationDisplayFrame(
        int Index,
        Measure Sample,
        Calibrater Calibrater,
        string Status,
        int SortDistance);

    private readonly record struct CalibrationProgressiveFrame(
        int Index,
        Measure Sample,
        Calibrater Calibrater,
        string Stage);
}
