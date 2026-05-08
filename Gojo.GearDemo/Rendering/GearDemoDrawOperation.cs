using Avalonia;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Gojo.GearDemo.Model;

namespace Gojo.GearDemo.Rendering;

internal sealed class GearDemoDrawOperation(
    Rect bounds,
    GearDemoLayout layout,
    GearDemoSnapshot snapshot,
    GearDemoInteraction interaction) : ICustomDrawOperation
{
    public Rect Bounds { get; } = bounds;

    public bool HitTest(Point p) => Bounds.Contains(p);

    public void Render(ImmediateDrawingContext context)
    {
        var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (leaseFeature is null)
        {
            return;
        }

        using var lease = leaseFeature.Lease();
        GearDemoRenderer.Render(lease.SkCanvas, layout, snapshot, interaction);
    }

    public void Dispose()
    {
    }

    public bool Equals(ICustomDrawOperation? other) => false;
}
