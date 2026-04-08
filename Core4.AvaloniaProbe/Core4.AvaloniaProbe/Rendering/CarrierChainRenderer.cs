using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Avalonia;
using Avalonia.Media;
using Core4.AvaloniaProbe.Models;
using Core4.Dynamics;
using Core4.Structure;
using Core4.Topology;

namespace Core4.AvaloniaProbe.Rendering;

/// <summary>
/// Shared topology renderer for the carrier-chain explorer. It reads only real Core4 topology, chains, connectors,
/// and orthogonal flows; the only extra input is the current visual hub layout.
/// </summary>
public static class CarrierChainRenderer
{
    private const double HorizontalPadding = 48d;
    private const double VerticalPadding = 28d;

    public static CarrierChainSceneLayout Arrange(
        Rect bounds,
        CarrierChainDemoState state,
        ImmutableDictionary<Site, Point> normalizedPositions)
    {
        var content = ContentRect(bounds);
        var chains = state.Topology.Chains();
        var defaults = DefaultPositions(chains);
        var hubs = BuildHubLayouts(content, state, chains, normalizedPositions, defaults);
        var hubLookup = HubLookup(hubs);
        var edges = BuildEdgeLayouts(state, chains, hubLookup);
        return new CarrierChainSceneLayout(content, hubs, edges);
    }

    public static void Render(DrawingContext context, CarrierChainSceneLayout layout)
    {
        DrawBackdrop(context, layout.ContentRect);
        DrawHubBodies(context, layout.Hubs);

        foreach (var edge in layout.Edges)
        {
            context.DrawLine(edge.Pen, edge.Start, edge.End);
            ScenePrimitives.DrawCircle(context, edge.StartCarrierMarker, 3.8d, Brushes.White, new Pen(edge.Pen.Brush, 2.4d, lineCap: PenLineCap.Round));
            ScenePrimitives.DrawDirectedTriangle(
                context,
                edge.EndCarrierMarker,
                8.8d,
                Brushes.White,
                edge.AnchorDirection,
                new Pen(edge.Pen.Brush, 2.4d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round));
            ScenePrimitives.DrawCenteredLabel(
                context,
                edge.Label,
                Midpoint(edge.Start, edge.End) + new Vector(0d, -16d),
                10d,
                ProbePalette.Muted,
                fontFamily: ProbePalette.MonoFont);

            foreach (var flow in edge.Flows)
            {
                if (flow.Amplitude > 0d)
                {
                    ScenePrimitives.DrawWave(
                        context,
                        flow.PinPoint,
                        flow.RemotePoint,
                        flow.Amplitude,
                        flow.Pen);
                }

                if (flow.IsOutgoing)
                {
                    ScenePrimitives.DrawVerticalTriangle(
                        context,
                        flow.RemotePoint,
                        11d,
                        ProbePalette.Surface,
                        new Pen(flow.Brush, 3.2d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round),
                        pointsUp: flow.RemotePoint.Y < flow.PinPoint.Y);
                }
                else
                {
                    ScenePrimitives.DrawCircle(
                        context,
                        flow.RemotePoint,
                        5.6d,
                        ProbePalette.Surface,
                        new Pen(flow.Brush, 3d, lineCap: PenLineCap.Round));
                }
            }
        }

        DrawHubLabels(context, layout.Hubs);
    }

    public static Rect ContentRect(Rect bounds) => new(
        bounds.X + HorizontalPadding,
        bounds.Y + VerticalPadding,
        Math.Max(64d, bounds.Width - (HorizontalPadding * 2d)),
        Math.Max(64d, bounds.Height - (VerticalPadding * 2d)));

    public static Point Normalize(Rect bounds, Point point)
    {
        var content = ContentRect(bounds);
        if (content.Width <= 0d || content.Height <= 0d)
        {
            return new Point(0.5d, 0.5d);
        }

        var x = (point.X - content.X) / content.Width;
        var y = (point.Y - content.Y) / content.Height;
        return ClampNormalized(new Point(x, y));
    }

    public static Point Resolve(Rect bounds, Point normalized)
    {
        var content = ContentRect(bounds);
        var clamped = ClampNormalized(normalized);
        return ResolvePoint(content, clamped);
    }

    public static Point ClampNormalized(Point normalized) => new(
        Math.Clamp(normalized.X, 0.05d, 0.95d),
        Math.Clamp(normalized.Y, 0.08d, 0.92d));

    private static void DrawHubBodies(DrawingContext context, ImmutableArray<CarrierChainHubLayout> hubs)
    {
        foreach (var hub in hubs)
        {
            var ringBrush = hub.IsFocus
                ? ProbePalette.Origin
                : hub.IsJunction
                    ? ProbePalette.Overlay
                    : new SolidColorBrush(Color.Parse("#c5b6a0"));
            var ringPen = new Pen(ringBrush, hub.IsFocus ? 3.2d : 2.2d, lineCap: PenLineCap.Round);
            ScenePrimitives.DrawCircle(context, hub.Point, hub.Radius, Brushes.White, ringPen);

            if (hub.AttachmentCount > 0)
            {
                ScenePrimitives.DrawCircle(
                    context,
                    hub.Point,
                    hub.IsJunction ? 5.8d : 5.1d,
                    hub.IsFocus ? ProbePalette.Origin : ProbePalette.Overlay);
            }

            if (hub.CarrierCount > 1)
            {
                ScenePrimitives.DrawSquare(
                    context,
                    hub.Point,
                    8.4d,
                    Brushes.Transparent,
                    new Pen(ProbePalette.Ink, 1.6d, lineCap: PenLineCap.Round),
                    3d);
            }
        }
    }

    private static void DrawHubLabels(DrawingContext context, ImmutableArray<CarrierChainHubLayout> hubs)
    {
        foreach (var hub in hubs)
        {
            ScenePrimitives.DrawCenteredLabel(
                context,
                hub.Label,
                hub.Point + new Vector(0d, 13d),
                11d,
                ProbePalette.Ink,
                FontWeight.SemiBold,
                ProbePalette.MonoFont);
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
            context.DrawLine(ProbePalette.TickPen, new Point(x, rect.Y + 8d), new Point(x, rect.Bottom - 8d));
        }

        for (var index = 1; index < 4; index++)
        {
            var y = rect.Y + (horizontalStep * index);
            context.DrawLine(ProbePalette.TickPen, new Point(rect.X + 8d, y), new Point(rect.Right - 8d, y));
        }
    }

    private static ImmutableDictionary<Site, Point> DefaultPositions(ImmutableArray<CarrierChain> chains)
    {
        var builder = ImmutableDictionary.CreateBuilder<Site, Point>();
        var chainCount = Math.Max(1, chains.Length);

        for (var chainIndex = 0; chainIndex < chains.Length; chainIndex++)
        {
            var chain = chains[chainIndex];
            var y = chainCount == 1
                ? 0.5d
                : 0.2d + (0.6d * (chainIndex / (double)(chainCount - 1)));

            for (var nodeIndex = 0; nodeIndex < chain.Nodes.Length; nodeIndex++)
            {
                var x = chain.Nodes.Length == 1
                    ? 0.5d
                    : 0.12d + (0.76d * (nodeIndex / (double)(chain.Nodes.Length - 1)));

                builder[chain.Nodes[nodeIndex].Site] = new Point(x, y);
            }
        }

        return builder.ToImmutable();
    }

    private static ImmutableArray<CarrierChainHubLayout> BuildHubLayouts(
        Rect content,
        CarrierChainDemoState state,
        ImmutableArray<CarrierChain> chains,
        ImmutableDictionary<Site, Point> normalizedPositions,
        ImmutableDictionary<Site, Point> defaults)
    {
        var groups = BuildHubGroups(chains);
        var hubs = ImmutableArray.CreateBuilder<CarrierChainHubLayout>(groups.Length);

        foreach (var group in groups)
        {
            var normalized = AveragePoint(group, normalizedPositions, defaults);
            var point = ResolvePoint(content, normalized);
            var carriers = ImmutableHashSet.CreateBuilder<Carrier>();
            var attached = ImmutableHashSet.CreateBuilder<Site>();
            var labels = new HashSet<string>();
            var memberSites = ImmutableArray.CreateBuilder<Site>();
            var isFocus = false;

            foreach (var node in group)
            {
                carriers.Add(node.Carrier);
                memberSites.Add(node.Site);
                labels.Add(SiteLabel(node.Site));

                foreach (var site in node.AttachedSites)
                {
                    attached.Add(site);
                    isFocus |= site == state.FocusSite;
                }
            }

            hubs.Add(new CarrierChainHubLayout(
                memberSites.ToImmutable(),
                [.. attached],
                carriers.ToImmutable(),
                point,
                string.Join(" / ", labels),
                attached.Count,
                carriers.Count > 1 || attached.Count > 1 || group.Count > 1,
                isFocus));
        }

        return hubs.ToImmutable();
    }

    private static ImmutableArray<List<CarrierChainNode>> BuildHubGroups(ImmutableArray<CarrierChain> chains)
    {
        var groups = new List<List<CarrierChainNode>>();

        foreach (var chain in chains)
        {
            foreach (var node in chain.Nodes)
            {
                groups.Add([node]);
            }
        }

        for (var index = 0; index < groups.Count; index++)
        {
            for (var other = index + 1; other < groups.Count; other++)
            {
                if (!SharesAttachment(groups[index], groups[other]))
                {
                    continue;
                }

                groups[index].AddRange(groups[other]);
                groups.RemoveAt(other);
                other--;
            }
        }

        return [.. groups];
    }

    private static bool SharesAttachment(List<CarrierChainNode> left, List<CarrierChainNode> right)
    {
        foreach (var leftNode in left)
        {
            foreach (var rightNode in right)
            {
                foreach (var leftSite in leftNode.AttachedSites)
                {
                    foreach (var rightSite in rightNode.AttachedSites)
                    {
                        if (leftSite == rightSite)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    private static Point AveragePoint(
        List<CarrierChainNode> group,
        ImmutableDictionary<Site, Point> normalizedPositions,
        ImmutableDictionary<Site, Point> defaults)
    {
        var totalX = 0d;
        var totalY = 0d;

        foreach (var node in group)
        {
            if (!normalizedPositions.TryGetValue(node.Site, out var point))
            {
                point = defaults[node.Site];
            }

            totalX += point.X;
            totalY += point.Y;
        }

        return ClampNormalized(new Point(totalX / group.Count, totalY / group.Count));
    }

    private static ImmutableDictionary<Site, CarrierChainHubLayout> HubLookup(ImmutableArray<CarrierChainHubLayout> hubs)
    {
        var builder = ImmutableDictionary.CreateBuilder<Site, CarrierChainHubLayout>();

        foreach (var hub in hubs)
        {
            foreach (var site in hub.MemberSites)
            {
                builder[site] = hub;
            }
        }

        return builder.ToImmutable();
    }

    private static ImmutableArray<CarrierChainEdgeLayout> BuildEdgeLayouts(
        CarrierChainDemoState state,
        ImmutableArray<CarrierChain> chains,
        ImmutableDictionary<Site, CarrierChainHubLayout> hubs)
    {
        var edges = ImmutableArray.CreateBuilder<CarrierChainEdgeLayout>();
        var carriers = ImmutableArray.CreateBuilder<Carrier>();

        foreach (var chain in chains)
        {
            if (!carriers.Contains(chain.Carrier))
            {
                carriers.Add(chain.Carrier);
            }
        }

        foreach (var chain in chains)
        {
            var color = CarrierBrush(carriers.IndexOf(chain.Carrier));
            var pen = new Pen(color, 4.4d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);

            foreach (var relation in chain.Relations)
            {
                var originHub = hubs[relation.Origin];
                var anchorHub = hubs[relation.Anchor];
                var spanStart = OffsetFromHub(originHub.Point, anchorHub.Point, originHub.Radius + 2d);
                var spanEnd = OffsetFromHub(anchorHub.Point, originHub.Point, anchorHub.Radius + 2d);
                var originMarker = OffsetFromHub(originHub.Point, anchorHub.Point, originHub.Radius + 9d);
                var anchorMarker = OffsetFromHub(anchorHub.Point, originHub.Point, anchorHub.Radius + 9d);

                edges.Add(new CarrierChainEdgeLayout(
                    relation,
                    spanStart,
                    spanEnd,
                    originMarker,
                    anchorMarker,
                    anchorHub.Point - anchorMarker,
                    pen,
                    $"{CarrierLabel(chain.Carrier)}:{relation.Potential.Raw}",
                    BuildFlowLayouts(state, relation, hubs)));
            }
        }

        return edges.ToImmutable();
    }

    private static ImmutableArray<CarrierChainFlowLayout> BuildFlowLayouts(
        CarrierChainDemoState state,
        Relation relation,
        ImmutableDictionary<Site, CarrierChainHubLayout> hubs)
    {
        var flows = ImmutableArray.CreateBuilder<CarrierChainFlowLayout>();

        foreach (var site in state.Topology.Sites)
        {
            foreach (var flow in site.Orthogonal.Flows)
            {
                if (flow.Support is not Relation support || support != relation)
                {
                    continue;
                }

                var pin = state.Topology.GeometryAt(site, relation.Carrier).Pin.Normalize();
                if (!pin.IsResolved)
                {
                    continue;
                }

                var origin = relation.OriginPosition.Normalize();
                var anchor = relation.AnchorPosition.Normalize();
                var originHub = hubs[relation.Origin];
                var anchorHub = hubs[relation.Anchor];
                var pinPoint = pin == origin
                    ? OffsetFromHub(originHub.Point, anchorHub.Point, originHub.Radius + 2d)
                    : pin == anchor
                        ? OffsetFromHub(anchorHub.Point, originHub.Point, anchorHub.Radius + 2d)
                        : Midpoint(originHub.Point, anchorHub.Point);
                var remotePoint = pin == origin
                    ? OffsetFromHub(anchorHub.Point, originHub.Point, anchorHub.Radius + 8d)
                    : pin == anchor
                        ? OffsetFromHub(originHub.Point, anchorHub.Point, originHub.Radius + 8d)
                        : flow.IsOutgoing
                            ? OffsetFromHub(anchorHub.Point, originHub.Point, anchorHub.Radius + 8d)
                            : OffsetFromHub(originHub.Point, anchorHub.Point, originHub.Radius + 8d);

                flows.Add(new CarrierChainFlowLayout(
                    pinPoint,
                    remotePoint,
                    FlowBrush(flow),
                    FlowPen(flow),
                    Math.Max(4d, Math.Min(18d, flow.Value.Potential.Raw * 1.1d)),
                    flow.IsOutgoing));
            }
        }

        return flows.ToImmutable();
    }

    private static IBrush CarrierBrush(int index) => index switch
    {
        0 => ProbePalette.Dominant,
        1 => ProbePalette.Orthogonal,
        2 => ProbePalette.OrthogonalInput,
        3 => ProbePalette.TensionPositive,
        _ => ProbePalette.Ink
    };

    private static IBrush FlowBrush(Flow flow) => flow.IsIncoming
        ? ProbePalette.OrthogonalInput
        : ProbePalette.Orthogonal;

    private static Pen FlowPen(Flow flow)
    {
        var brush = flow.Value.Recessive.Raw < 0 ? ProbePalette.TensionNegative : ProbePalette.TensionPositive;
        return new Pen(brush, 2.8d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
    }

    private static string SiteLabel(Site site) => $"{CarrierLabel(site.Carrier)}{site.Location.Raw}";

    private static string CarrierLabel(Carrier carrier) => carrier.Id switch
    {
        1L => "x",
        2L => "y",
        3L => "z",
        4L => "w",
        _ => $"c{carrier.Id}"
    };

    private static Point ResolvePoint(Rect content, Point normalized) => new(
        content.X + (content.Width * normalized.X),
        content.Y + (content.Height * normalized.Y));

    private static Point OffsetFromHub(Point hub, Point target, double distance)
    {
        var delta = target - hub;
        var length = Math.Sqrt((delta.X * delta.X) + (delta.Y * delta.Y));
        if (length <= 0d)
        {
            return hub;
        }

        var unit = new Vector(delta.X / length, delta.Y / length);
        return hub + (unit * distance);
    }

    private static Point Midpoint(Point left, Point right) => new((left.X + right.X) * 0.5d, (left.Y + right.Y) * 0.5d);
}

public readonly record struct CarrierChainSceneLayout(
    Rect ContentRect,
    ImmutableArray<CarrierChainHubLayout> Hubs,
    ImmutableArray<CarrierChainEdgeLayout> Edges);

public readonly record struct CarrierChainHubLayout(
    ImmutableArray<Site> MemberSites,
    ImmutableArray<Site> AttachedSites,
    ImmutableHashSet<Carrier> Carriers,
    Point Point,
    string Label,
    int AttachmentCount,
    bool IsJunction,
    bool IsFocus)
{
    public int CarrierCount => Carriers.Count;

    public double Radius => IsJunction ? 14d : 12d;
}

public readonly record struct CarrierChainEdgeLayout(
    Relation Relation,
    Point Start,
    Point End,
    Point StartCarrierMarker,
    Point EndCarrierMarker,
    Vector AnchorDirection,
    Pen Pen,
    string Label,
    ImmutableArray<CarrierChainFlowLayout> Flows);

public readonly record struct CarrierChainFlowLayout(
    Point PinPoint,
    Point RemotePoint,
    IBrush Brush,
    Pen Pen,
    double Amplitude,
    bool IsOutgoing);
