using Core4.Numerics;

namespace Core4.Examples;

/// <summary>
/// Adjustable inputs for the numeric explorer. Recessive span of <c>0</c> means the line is still one-sided.
/// </summary>
public readonly record struct NumericExplorerPreset(
    string Name,
    string Description,
    Measure Origin,
    Measure DominantSpan,
    Measure RecessiveSpan,
    Measure ValueOffset,
    Measure Residual,
    Measure SensorInput,
    Measure SwitchInput,
    Measure Feedback);
