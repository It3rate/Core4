using System;
using System.Collections.Immutable;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Core4.AvaloniaProbe.Controls;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;
using Core4.Calibration;
using Core4.Numerics;

namespace Core4.AvaloniaProbe.Views;

public sealed class CalibrationPatternPageView : UserControl
{
    private readonly CalibrationPatternChartControl _chart;
    private readonly Button[] _datasetButtons;
    private readonly TextBlock _sceneDescriptionText;
    private readonly TextBlock _metaPointText;
    private readonly TextBlock _metaRangeText;
    private readonly TextBlock _metaSkewText;
    private readonly TextBlock _windowText;
    private readonly TextBlock _trendText;
    private readonly TextBlock _fitText;
    private CalibrationDataset _dataset = CalibrationDemoCatalog.PatternDatasets[0];

    public CalibrationPatternPageView()
    {
        _chart = new CalibrationPatternChartControl
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        _datasetButtons = new Button[CalibrationDemoCatalog.PatternDatasets.Length];
        var datasetRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6d
        };

        for (var index = 0; index < CalibrationDemoCatalog.PatternDatasets.Length; index++)
        {
            var dataset = CalibrationDemoCatalog.PatternDatasets[index];
            var button = ProbeUi.PillButton(dataset.Label);
            var captured = dataset;
            button.Click += (_, _) => SetDataset(captured);
            _datasetButtons[index] = button;
            datasetRow.Children.Add(button);
        }

        _sceneDescriptionText = ProbeUi.Body(string.Empty, 10d);
        _metaPointText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Muted);
        _metaRangeText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Muted);
        _metaSkewText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Muted);
        _windowText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);
        _trendText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);
        _fitText = ProbeUi.Mono(string.Empty, 10d, ProbePalette.Ink);

        var sceneCard = ProbeUi.Card(new StackPanel
        {
            Spacing = 8d,
            Children =
            {
                SceneHeader(),
                ProbeUi.PlotCard(_chart)
            }
        }, 22d, new Thickness(12));

        var infoCards = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*"),
            ColumnSpacing = 10d
        };
        ProbeUi.AddInfoCard(infoCards, 0, "WINDOW", _windowText);
        ProbeUi.AddInfoCard(infoCards, 1, "TREND", _trendText);
        ProbeUi.AddInfoCard(infoCards, 2, "FIT PRESSURE", _fitText);

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
                                ProbeUi.DisplayTitle("Pattern State", 18d),
                                ProbeUi.Body("A rolling three-point state reads local shape directly: observed stream, running center, and slow additive-versus-multiplicative fit pressure. Ordered datasets stay in their native arrival order on this page.", 10.5d)
                            }
                        },
                        datasetRow
                    }
                }, 22d, new Thickness(12, 10)),
                sceneCard,
                infoCards
            }
        };

        SetDataset(_dataset);
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
                ProbeUi.Mono("PATTERN SCENE", 10d, ProbePalette.Muted),
                ProbeUi.DisplayTitle("Observed Stream And Local Fit", 18d),
                _sceneDescriptionText
            }
        };

        var metaStack = new StackPanel
        {
            Spacing = 2,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children =
            {
                _metaPointText,
                _metaRangeText,
                _metaSkewText
            }
        };

        grid.Children.Add(titleStack);
        grid.Children.Add(metaStack);
        Grid.SetColumn(metaStack, 1);
        return grid;
    }

    private void SetDataset(CalibrationDataset dataset)
    {
        _dataset = dataset;

        for (var index = 0; index < CalibrationDemoCatalog.PatternDatasets.Length; index++)
        {
            ProbeUi.ApplyPillStyle(_datasetButtons[index], CalibrationDemoCatalog.PatternDatasets[index].Key == dataset.Key);
        }

        var series = BuildSeries(dataset);
        _chart.Series = series;
        SyncUi(series);
    }

    private void SyncUi(CalibrationPatternSeries series)
    {
        var final = series.FinalState;
        _sceneDescriptionText.Text = _dataset.Summary;
        _metaPointText.Text = $"{series.Points.Length} points";
        _metaRangeText.Text = $"range {series.Minimum.Raw} .. {series.Maximum.Raw}";
        _metaSkewText.Text = $"fit skew {final.FitSkew.Raw}";

        _windowText.Text =
            $"origin                    {final.OriginMeasure.Raw}\n" +
            $"unit                      {final.UnitMeasure.Raw}\n" +
            $"value                     {final.ValueMeasure.Raw}\n" +
            $"window range              {final.WindowRange.Raw}";

        _trendText.Text =
            $"last slope                {final.LastSlope.Raw}\n" +
            $"momentum                  {final.Momentum.Raw}\n" +
            $"direction streak          {final.DirectionStreak.Raw}\n" +
            $"steps since reversal      {final.StepsSinceLastReversal.Raw}";

        _fitText.Text =
            $"additive                  {final.AdditiveFitPressure.Raw}\n" +
            $"multiplicative            {final.MultiplicativeFitPressure.Raw}\n" +
            $"reversal                  {final.ReversalStrength.Raw}\n" +
            $"shock                     {final.ShockResidual.Raw}";
    }

    private static CalibrationPatternSeries BuildSeries(CalibrationDataset dataset)
    {
        var state = PatternState.Start(dataset.Carrier);
        var points = ImmutableArray.CreateBuilder<CalibrationPatternPoint>(dataset.Values.Length);
        long minimum = long.MaxValue;
        long maximum = long.MinValue;
        long maxPressure = 1;

        for (var index = 0; index < dataset.Values.Length; index++)
        {
            var sample = dataset.Values[index];
            state = state.Input(sample);
            points.Add(new CalibrationPatternPoint(index + 1, sample, state));

            minimum = Math.Min(minimum, Math.Min(sample.Raw, state.RunningAverage.Raw));
            maximum = Math.Max(maximum, Math.Max(sample.Raw, state.RunningAverage.Raw));
            maxPressure = Math.Max(
                maxPressure,
                Math.Max(
                    state.AdditiveFitPressure.Raw,
                    Math.Max(state.MultiplicativeFitPressure.Raw, state.ReversalStrength.Raw)));
        }

        if (points.Count == 0)
        {
            minimum = 0;
            maximum = 1;
        }

        return new CalibrationPatternSeries(
            dataset,
            points.MoveToImmutable(),
            new Measure(minimum),
            new Measure(maximum),
            new Measure(Math.Max(1, maxPressure)),
            state);
    }
}
