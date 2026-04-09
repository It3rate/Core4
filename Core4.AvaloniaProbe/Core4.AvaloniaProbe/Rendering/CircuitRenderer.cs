using System;
using System.Collections.Immutable;
using Avalonia;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;
using Core4.Numerics;
using Core4.Structure;
using Core4.Types;

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
        Placement negativeNode,
        bool core4View)
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
            DrawBranch(context, branch, state, from, to, content.Center, core4View);
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
        Point to,
        Point sceneCenter,
        bool core4View)
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
        var midpoint = Midpoint(start, end);
        var normal = new Vector(-unit.Y, unit.X);
        var drop = state.DropOn(branch);
        var current = state.CurrentOn(branch);
        var dissipation = state.DissipationOn(branch);
        var leadPen = new Pen(ProbePalette.Hidden, 2d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);

        if (core4View)
        {
            DrawCore4Branch(context, branch, current, start, end);
        }
        else
        {
            DrawResistor(context, branch, start, end, leadPen);
        }

        var currentMagnitude = Math.Abs(current.Approximate);
        if (currentMagnitude > 0.0001d)
        {
            var arrowDirection = current.Approximate >= 0d ? unit : -unit;
            var arrowBrush = current.Approximate >= 0d
                ? ProbePalette.TensionPositive
                : ProbePalette.TensionNegative;
            var arrowPoint = current.Approximate >= 0d
                ? end - (unit * 22d)
                : start + (unit * 22d);
            ScenePrimitives.DrawDirectedTriangle(
                context,
                arrowPoint,
                7.6d,
                arrowBrush,
                arrowDirection,
                new Pen(arrowBrush, 1d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round));
        }

        var calloutDirection = ChooseCalloutDirection(normal, midpoint, sceneCenter);
        DrawCallout(
            context,
            midpoint,
            calloutDirection,
            $"R {Format(branch.Resistance)}  d {Format(drop)}  i {Format(current)}  h {Format(dissipation)}");
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
            9.2d,
            ProbePalette.Ink,
            FontWeight.Bold,
            ProbePalette.UiFont);
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
        ResolvedResistiveBranch branch,
        Point start,
        Point end,
        Pen leadPen)
    {
        var delta = end - start;
        var distance = Math.Sqrt((delta.X * delta.X) + (delta.Y * delta.Y));
        if (distance <= 0.01d)
        {
            return;
        }

        var unit = new Vector(delta.X / distance, delta.Y / distance);
        var normal = new Vector(-unit.Y, unit.X);
        var bodyLength = Math.Min(46d, distance * 0.28d);
        var center = Midpoint(start, end);
        var leftLead = center - (unit * (bodyLength * 0.5d));
        var rightLead = center + (unit * (bodyLength * 0.5d));

        context.DrawLine(leadPen, start, leftLead);
        context.DrawLine(leadPen, rightLead, end);

        var outlinePen = new Pen(ProbePalette.Ink, 14d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        var bodyPen = new Pen(Brushes.White, 10d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        context.DrawLine(outlinePen, leftLead, rightLead);
        context.DrawLine(bodyPen, leftLead, rightLead);

        var bandPositions = new[] { 0.39d, 0.50d, 0.61d };
        var bandBrushes = ResistanceBands(branch.Resistance);

        for (var index = 0; index < bandPositions.Length; index++)
        {
            var point = leftLead + ((rightLead - leftLead) * bandPositions[index]);
            var bandPen = new Pen(bandBrushes[index], 3.2d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
            context.DrawLine(
                bandPen,
                point - (normal * 6d),
                point + (normal * 6d));
        }
    }

    private static void DrawCore4Branch(
        DrawingContext context,
        ResolvedResistiveBranch branch,
        Ratio current,
        Point start,
        Point end)
    {
        var pen = new Pen(CarrierBrush(branch.Relation.Carrier), 4.4d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        context.DrawLine(pen, start, end);

        if (current.Approximate < -0.0001d)
        {
            ScenePrimitives.DrawWave(
                context,
                start,
                end,
                4.2d,
                new Pen(ProbePalette.TensionNegative, 2.2d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round));
        }
    }

    private static void DrawCallout(
        DrawingContext context,
        Point anchor,
        Vector direction,
        string text)
    {
        var dot = anchor + (direction * 8d);
        var background = new SolidColorBrush(Color.FromArgb(168, 255, 255, 255));
        var stroke = new Pen(new SolidColorBrush(Color.FromArgb(72, 197, 182, 160)), 0.8d);
        var formatted = new FormattedText(
            text,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(ProbePalette.MonoFont, FontStyle.Normal, FontWeight.Normal),
            9.4d,
            ProbePalette.Muted);
        var padding = new Thickness(6d, 3d, 6d, 3d);
        var bodySize = new Size(
            formatted.Width + padding.Left + padding.Right,
            formatted.Height + padding.Top + padding.Bottom);
        var bodyAnchor = dot + (direction * 20d);
        var xOffset = direction.X >= 0d ? 12d : -(bodySize.Width + 12d);
        var yOffset = direction.Y >= 0d ? 8d : -(bodySize.Height + 8d);
        var rect = new Rect(bodyAnchor + new Vector(xOffset, yOffset), bodySize);
        var target = CalloutTarget(rect, anchor, direction);
        var leadPen = new Pen(new SolidColorBrush(Color.FromArgb(120, 168, 154, 132)), 1d, lineCap: PenLineCap.Round);

        context.DrawLine(leadPen, anchor, dot);
        context.DrawLine(leadPen, dot, target);
        ScenePrimitives.DrawCircle(context, dot, 2.6d, ProbePalette.Hidden);
        context.DrawRectangle(background, stroke, rect, 6d, 6d);
        context.DrawText(formatted, new Point(rect.X + padding.Left, rect.Y + padding.Top));
    }

    private static Point CalloutTarget(Rect rect, Point anchor, Vector direction)
    {
        var insetLeft = rect.X + (rect.Width * 0.2d);
        var insetRight = rect.Right - (rect.Width * 0.2d);
        var x = Math.Clamp(anchor.X, insetLeft, insetRight);

        if (direction.Y < 0d)
        {
            return new Point(x, rect.Bottom);
        }

        return new Point(x, rect.Y);
    }

    private static Vector ChooseCalloutDirection(Vector normal, Point midpoint, Point sceneCenter)
    {
        var desiredY = midpoint.Y <= sceneCenter.Y ? -1d : 1d;
        var candidate = normal.Y * desiredY >= 0d ? normal : -normal;
        var length = Math.Sqrt((candidate.X * candidate.X) + (candidate.Y * candidate.Y));
        return length <= 0d
            ? new Vector(0d, desiredY)
            : new Vector(candidate.X / length, candidate.Y / length);
    }

    private static IBrush[] ResistanceBands(Ratio resistance)
    {
        var magnitude = (int)Math.Abs(resistance.Canonicalize().Approximate * 10d);
        var palette = new[]
        {
            new SolidColorBrush(Color.Parse("#6b3e26")),
            new SolidColorBrush(Color.Parse("#c0482d")),
            new SolidColorBrush(Color.Parse("#c38c2a")),
            new SolidColorBrush(Color.Parse("#2f8f46")),
            new SolidColorBrush(Color.Parse("#1f5da8")),
            new SolidColorBrush(Color.Parse("#6f3ba8"))
        };

        return
        [
            palette[(magnitude + 1) % palette.Length],
            palette[(magnitude + 3) % palette.Length],
            palette[(magnitude + 5) % palette.Length]
        ];
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

    private static IBrush CarrierBrush(Carrier carrier) => carrier.Id switch
    {
        1L => ProbePalette.Dominant,
        2L => ProbePalette.Orthogonal,
        3L => ProbePalette.OrthogonalInput,
        4L => ProbePalette.Overlay,
        _ => ProbePalette.Ink
    };
}
