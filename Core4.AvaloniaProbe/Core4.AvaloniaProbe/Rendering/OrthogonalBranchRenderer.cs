using System;
using Avalonia;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;

namespace Core4.AvaloniaProbe.Rendering;

/// <summary>
/// Shared renderer for orthogonal branches so output, input, and multi-input pages use one angle and tension law.
/// </summary>
public static class OrthogonalBranchRenderer
{
    private const double BranchNominalDepthMultiplier = 6d;
    private const double BranchSpread = 54d;

    public static OrthogonalBranchLayout Arrange(
        Point origin,
        double pixelsPerMeasure,
        OrthogonalBranchModel branch,
        bool recessive,
        int slotIndex = 0,
        int slotCount = 1)
    {
        var direction = Direction(recessive, slotIndex, slotCount, pixelsPerMeasure);
        var signalLength = branch.SignalMagnitude.Raw * pixelsPerMeasure;
        var tensionLength = branch.HasTension
            ? Math.Max(signalLength, Math.Max(pixelsPerMeasure * 0.92d, Math.Abs(branch.TensionValue.Raw) * 1.4d))
            : signalLength;
        var signalTip = origin + (direction * signalLength);
        var tensionTip = origin + (direction * tensionLength);
        var amplitude = branch.HasTension
            ? Math.Max(3.8d, Math.Min(24d, Math.Abs(branch.TensionValue.Raw) * 1.2d))
            : 0d;

        return new OrthogonalBranchLayout(
            branch,
            origin,
            signalTip,
            tensionTip,
            direction,
            amplitude);
    }

    public static void Render(DrawingContext context, OrthogonalBranchLayout layout)
    {
        if (layout.HasTension)
        {
            ScenePrimitives.DrawWave(
                context,
                layout.Start,
                layout.TensionTip,
                layout.TensionAmplitude,
                layout.Branch.TensionValue.Raw < 0
                    ? ProbePalette.TensionNegativePen
                    : ProbePalette.TensionPositivePen);
        }

        if (!layout.HasSignal)
        {
            return;
        }

        if (layout.IsIncoming)
        {
            DrawIncoming(context, layout);
        }
        else
        {
            DrawOutgoing(context, layout);
        }
    }

    private static void DrawOutgoing(DrawingContext context, OrthogonalBranchLayout layout)
    {
        var arrowSize = 14d;
        var basePoint = layout.SignalTip - (layout.Direction * (arrowSize * 0.94d));
        context.DrawLine(ProbePalette.OrthogonalPen, layout.Start, basePoint);
        ScenePrimitives.DrawVerticalTriangle(
            context,
            layout.SignalTip,
            arrowSize,
            ProbePalette.Surface,
            new Pen(ProbePalette.Orthogonal, 3.8d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round),
            pointsUp: true);
    }

    private static void DrawIncoming(DrawingContext context, OrthogonalBranchLayout layout)
    {
        context.DrawLine(ProbePalette.OrthogonalInputPen, layout.Start, layout.SignalTip);
        ScenePrimitives.DrawCircle(
            context,
            layout.SignalTip,
            5.8d,
            ProbePalette.Surface,
            new Pen(ProbePalette.OrthogonalInput, 3.3d, lineCap: PenLineCap.Round));
    }

    private static Vector Direction(bool recessive, int slotIndex, int slotCount, double pixelsPerMeasure)
    {
        if (!recessive)
        {
            return new Vector(0d, -1d);
        }

        if (slotCount <= 1)
        {
            return new Vector(0d, 1d);
        }

        var spread = (slotIndex - ((slotCount - 1) / 2d)) * BranchSpread;
        var nominalDepth = pixelsPerMeasure * BranchNominalDepthMultiplier;
        var dx = spread;
        var dy = nominalDepth;
        var length = Math.Sqrt((dx * dx) + (dy * dy));
        return length <= 0d
            ? new Vector(0d, 1d)
            : new Vector(dx / length, dy / length);
    }
}
