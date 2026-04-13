using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;
using Core4.AvaloniaProbe.Rendering;

namespace Core4.AvaloniaProbe.Controls;

public sealed class CalibrationPatternChartControl : Control
{
    private CalibrationPatternSeries? _series;

    public CalibrationPatternChartControl()
    {
        MinHeight = 360d;
        ClipToBounds = true;
    }

    public CalibrationPatternSeries? Series
    {
        get => _series;
        set
        {
            if (ReferenceEquals(_series, value))
            {
                return;
            }

            _series = value;
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (_series is null)
        {
            return;
        }

        CalibrationPatternRenderer.Render(context, Bounds, _series);
    }
}
