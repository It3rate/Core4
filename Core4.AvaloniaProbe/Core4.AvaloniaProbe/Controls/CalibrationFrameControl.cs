using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Core4.AvaloniaProbe.Rendering;
using Core4.Calibration;

namespace Core4.AvaloniaProbe.Controls;

public sealed class CalibrationFrameControl : Control
{
    private Calibrater _calibrater = Calibrater.Start(1);
    private string _ratioLabel = "--";

    public CalibrationFrameControl()
    {
        Width = 188d;
        Height = 28d;
        ClipToBounds = true;
    }

    public Calibrater Calibrater
    {
        get => _calibrater;
        set
        {
            if (ReferenceEquals(_calibrater, value))
            {
                return;
            }

            _calibrater = value;
            InvalidateVisual();
        }
    }

    public string RatioLabel
    {
        get => _ratioLabel;
        set
        {
            if (_ratioLabel == value)
            {
                return;
            }

            _ratioLabel = value;
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        CalibrationRenderer.Render(context, Bounds, _calibrater, _ratioLabel);
    }
}
