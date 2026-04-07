using Avalonia;
using Core4.AvaloniaProbe.Models;

namespace Core4.AvaloniaProbe.Rendering;

/// <summary>
/// Fitted geometry for one orthogonal branch and its local tension trace.
/// </summary>
public readonly record struct OrthogonalBranchLayout(
    OrthogonalBranchModel Branch,
    Point Start,
    Point SignalTip,
    Point TensionTip,
    Vector Direction,
    double TensionAmplitude)
{
    public bool HasSignal => Branch.HasSignal;

    public bool HasTension => Branch.HasTension;

    public bool IsIncoming => Branch.IsIncoming;

    public bool IsOutgoing => Branch.IsOutgoing;
}
