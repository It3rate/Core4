using System.Collections.Immutable;
using Core4.Calibration;
using Core4.Numerics;

namespace Core4.AvaloniaProbe.Models;

public sealed record CalibrationPatternSeries(
    CalibrationDataset Dataset,
    ImmutableArray<CalibrationPatternPoint> Points,
    Measure Minimum,
    Measure Maximum,
    Measure MaximumPressure,
    PatternState FinalState);

public readonly record struct CalibrationPatternPoint(
    int Index,
    Measure Sample,
    PatternState State);
