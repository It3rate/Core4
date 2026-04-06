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
    Measure SingleValueOffset,
    Measure DominantValueOffset,
    Measure RecessiveValueOffset,
    Measure Tension,
    Measure TopSignal,
    Measure BottomSignal,
    Measure BottomSignal2,
    Measure BottomTension,
    Measure BottomTension2,
    bool FuseAligned,
    NumericExplorerControls Controls);
