using System.Collections.Immutable;
using Avalonia;
using Core4.AvaloniaProbe.Models;

namespace Core4.AvaloniaProbe.Rendering;

/// <summary>
/// Fitted geometry for one output with several bottom-side orthogonal input branches.
/// </summary>
public readonly record struct MultipleInputsLayout(
    FrozenSegmentLayout SegmentLayout,
    MultipleInputsDemoState State,
    OrthogonalBranchLayout TopBranch,
    ImmutableArray<OrthogonalBranchLayout> BottomBranches);
