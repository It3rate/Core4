using System;
using System.Collections.Immutable;
using Avalonia;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;
using Core4.Dynamics;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Rendering;

/// <summary>
/// Small schematic-style renderer for the first passive circuit page. It keeps the drawing intentionally simple:
/// nodes show relaxed potentials, relations draw as resistor spans, and branch labels expose resistance, drop,
/// current-like transfer, and dissipation from the Core4 relaxation state.
/// </summary>
public static class CircuitRenderer
{
    private const double HorizontalPadding = 44d;
    private const double VerticalPadding = 28d;

    public static void Render(
        DrawingContext context,
        Rect bounds,
        CircuitDemoState demo,
        ResistiveCircuit circuit,
        ResistiveCircuitState state,
        ImmutableDictionary<Placement, Point> normalizedPositions,
        Placement positiveNode,
        Placement negativeNode)
    {
        var content = new Rect(
            bounds.X + HorizontalPadding,
            bounds.Y + VerticalPadding,
            Math.Max(64d, bounds.Width - (HorizontalPadding * 2d)),
            Math.Max(64d, bounds.Height - (VerticalPadding * 2d)));

        DrawBackdrop(context, content);

        foreach (var branch in circuit.Branches)
        {
            if (!normalizedPositions.TryGetValue(branch.From, out var fromNormalized)
                || !normalizedPositions.TryGetValue(branch.To, out var toNormalized))
            {
                continue;
            }

            var from = Resolve(content, fromNormalized);
            var to = Resolve(content, toNormalized);
            DrawBranch(context, branch, state, from, to);
        }

        foreach (var node in circuit.Nodes)
        {
            if (!normalizedPositions.TryGetValue(node.Key, out var normalized))
            {
                continue;
            }

            DrawNode(
                context,
                demo,
                node,
                state.PotentialAt(node.Key),
                Resolve(content, normalized),
                node.Key == positiveNode,
                node.Key == negativeNode);
        }
    }

    private static void DrawBackdrop(DrawingContext context, Rect rect)
    {
        context.DrawRectangle(null, new Pen(ProbePalette.Axis, 1d), rect, 18d, 18d);

        var verticalStep = rect.Width / 6d;
        var horizontalStep = rect.Height / 4d;

        for (var index = 1; index < 6; index++)
        {
            var x = rect.X + (verticalStep * index);
            context.DrawLine(ProbePalette.TickPen, new Point(x, rect.Y + 10d), new Point(x, rect.Bottom - 10d));
        }

        for (var index = 1; index < 4; index++)
        {
            var y = rect.Y + (horizontalStep * index);
            context.DrawLine(ProbePalette.TickPen, new Point(rect.X + 10d, y), new Point(rect.Right - 10d, y));
        }
    }

    private static void DrawBranch(
        DrawingContext context,
        ResolvedResistiveBranch branch,
        ResistiveCircuitState state,
        Point from,
        Point to)
    {
        var delta = to - from;
        var distance = Math.Sqrt((delta.X * delta.X) + (delta.Y * delta.Y));
        if (distance <= 0.01d)
        {
            return;
        }

        var unit = new Vector(delta.X / distance, delta.Y / distance);
        var start = from + (unit * 15d);
        var end = to - (unit * 15d);
        var drop = state.DropOn(branch);
        var current = state.CurrentOn(branch);
        var dissipation = state.DissipationOn(branch);
        var resistorPen = new Pen(ProbePalette.Ink, 2.2d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        var leadPen = new Pen(ProbePalette.Hidden, 2d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);

        DrawResistor(context, start, end, leadPen, resistorPen);

        var currentMagnitude = Math.Abs(current.Approximate);
        if (currentMagnitude > 0.0001d)
        {
            var arrowDirection = current.Approximate >= 0d ? unit : -unit;
            var arrowPoint = Midpoint(start, end) + (arrowDirection * 16d);
            ScenePrimitives.DrawDirectedTriangle(
                context,
                arrowPoint,
                7.6d,
                ProbePalette.TensionPositive,
                arrowDirection,
                new Pen(ProbePalette.TensionPositive, 1d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round));
        }

        var midpoint = Midpoint(start, end);
        ScenePrimitives.DrawBadgeLabel(
            context,
            $"R {Format(branch.Resistance)}",
            midpoint + new Vector(-24d, -28d),
            10d,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.MonoFont);
        ScenePrimitives.DrawBadgeLabel(
            context,
            $"d {Format(drop)}  i {Format(current)}  h {Format(dissipation)}",
            midpoint + new Vector(-58d, 10d),
            9.4d,
            ProbePalette.Muted,
            fontFamily: ProbePalette.MonoFont,
            background: new SolidColorBrush(Color.FromArgb(128, 255, 255, 255)),
            stroke: new Pen(new SolidColorBrush(Color.FromArgb(64, 197, 182, 160)), 0.8d));
    }

    private static void DrawNode(
        DrawingContext context,
        CircuitDemoState demo,
        ResistiveNode node,
        Ratio potential,
        Point center,
        bool isPositiveBoundary,
        bool isNegativeBoundary)
    {
        var ringBrush = isPositiveBoundary
            ? ProbePalette.Origin
            : isNegativeBoundary
                ? ProbePalette.Dominant
                : ProbePalette.Overlay;
        var ringPen = new Pen(ringBrush, node.IsFixed ? 3d : 2.1d, lineCap: PenLineCap.Round);

        ScenePrimitives.DrawCircle(context, center, node.IsFixed ? 13d : 11d, Brushes.White, ringPen);
        ScenePrimitives.DrawCircle(context, center, 4.2d, ringBrush);

        if (isPositiveBoundary || isNegativeBoundary)
        {
            ScenePrimitives.DrawCenteredLabel(
                context,
                isPositiveBoundary ? "+" : "-",
                center + new Vector(0d, -3d),
                11d,
                Brushes.White,
                FontWeight.Bold,
                ProbePalette.MonoFont);
        }

        ScenePrimitives.DrawBadgeLabel(
            context,
            $"V {Format(potential)}",
            center + new Vector(-22d, -32d),
            10d,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.MonoFont);
        ScenePrimitives.DrawCenteredLabel(
            context,
            demo.NodeLabel(node),
            center + new Vector(0d, 14d),
            10d,
            ProbePalette.Ink,
            FontWeight.SemiBold,
            ProbePalette.MonoFont);
    }

    private static void DrawResistor(
        DrawingContext context,
        Point start,
        Point end,
        Pen leadPen,
        Pen resistorPen)
    {
        var delta = end - start;
        var distance = Math.Sqrt((delta.X * delta.X) + (delta.Y * delta.Y));
        if (distance <= 0.01d)
        {
            return;
        }

        var unit = new Vector(delta.X / distance, delta.Y / distance);
        var normal = new Vector(-unit.Y, unit.X);
        var leftLead = start + (unit * Math.Min(18d, distance * 0.22d));
        var rightLead = end - (unit * Math.Min(18d, distance * 0.22d));

        context.DrawLine(leadPen, start, leftLead);
        context.DrawLine(leadPen, rightLead, end);

        var zigzag = new StreamGeometry();
        using var stream = zigzag.Open();
        var segment = (rightLead - leftLead) / 6d;
        stream.BeginFigure(leftLead, false);

        for (var index = 1; index <= 5; index++)
        {
            var basePoint = leftLead + (segment * index);
            var offset = index == 5
                ? Vector.Zero
                : normal * (index % 2 == 0 ? -7d : 7d);
            stream.LineTo(basePoint + offset);
        }

        stream.EndFigure(false);
        context.DrawGeometry(null, resistorPen, zigzag);
    }

    private static Point Resolve(Rect rect, Point normalized) => new(
        rect.X + (rect.Width * normalized.X),
        rect.Y + (rect.Height * normalized.Y));

    private static Point Midpoint(Point left, Point right) =>
        new((left.X + right.X) * 0.5d, (left.Y + right.Y) * 0.5d);

    private static string Format(Ratio value)
    {
        var normalized = value.Canonicalize();

        if (normalized.IsTension)
        {
            return $"{normalized.Potential.Raw}/0";
        }

        return normalized.Dominant == Measure.One
            ? normalized.Recessive.Raw.ToString()
            : $"{normalized.Recessive.Raw}/{normalized.Dominant.Raw}";
    }
}
