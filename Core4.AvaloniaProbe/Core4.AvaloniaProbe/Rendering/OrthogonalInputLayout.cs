using Avalonia;
using Core4.AvaloniaProbe.Models;

namespace Core4.AvaloniaProbe.Rendering;

/// <summary>
/// Fitted geometry for a single-output single-input orthogonal scene.
/// </summary>
public readonly record struct OrthogonalInputLayout(
    FrozenSegmentLayout SegmentLayout,
    OrthogonalInputDemoState State,
    OrthogonalBranchLayout TopBranch,
    OrthogonalBranchLayout BottomBranch);
