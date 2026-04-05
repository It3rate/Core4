using Core4.Numerics;

namespace Core4.Examples;

/// <summary>
/// UI-level visibility and ranges for one numeric explorer preset.
/// </summary>
public readonly record struct NumericExplorerControls(
    bool ShowDominantSpan,
    bool ShowRecessiveSpan,
    bool ShowValueOffset,
    bool ShowResidual,
    bool ShowTopSignal,
    bool ShowBottomSignal,
    bool ShowBottomSignal2,
    Measure ValueMin,
    Measure ValueMax);
