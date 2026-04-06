using Core4.Numerics;

namespace Core4.Examples;

/// <summary>
/// UI-level visibility and ranges for one numeric explorer preset.
/// </summary>
public readonly record struct NumericExplorerControls(
    bool ShowFusedSpan,
    bool ShowDominantSpan,
    bool ShowRecessiveSpan,
    bool ShowSingleValue,
    bool ShowDominantValue,
    bool ShowRecessiveValue,
    bool ShowAlignedControls,
    bool ShowTension,
    bool ShowTopSignal,
    bool ShowBottomSignal,
    bool ShowBottomSignal2,
    Measure SingleValueMin,
    Measure SingleValueMax,
    Measure PairValueMin,
    Measure PairValueMax);
