using Gojo.Core;
using Gojo.Structures;

namespace Gojo.ConceptSurface;

/// <summary>
/// Local read value for a boundary/domain surface. The booleans are a compact
/// helper read over relation structure; they are not the source of meaning.
/// </summary>
public readonly record struct BoundaryExposure(bool LeftBoundary, bool RightBoundary)
{
    public static BoundaryExposure FullSigned => new(false, false);

    public static BoundaryExposure Positive => new(true, false);

    public static BoundaryExposure Negative => new(false, true);

    public static BoundaryExposure BoundedWhole => new(true, true);

    public bool IsFullSigned => !LeftBoundary && !RightBoundary;

    public bool IsPositive => LeftBoundary && !RightBoundary;

    public bool IsNegative => !LeftBoundary && RightBoundary;

    public bool IsBoundedWhole => LeftBoundary && RightBoundary;

    public string Label => this switch
    {
        { LeftBoundary: false, RightBoundary: false } => "PN",
        { LeftBoundary: true, RightBoundary: false } => "P",
        { LeftBoundary: false, RightBoundary: true } => "N",
        _ => "S"
    };

    /// <summary>Swaps positive and negative boundary sides while preserving full and bounded-whole domains.</summary>
    public BoundaryExposure Dual() => new(RightBoundary, LeftBoundary);

    public override string ToString() => Label;
}

/// <summary>
/// Local read value for exposed flow faces. "None" means no forward or inverse
/// flow surface is exposed by this local structure.
/// </summary>
public readonly record struct FlowExposure(bool ForwardFlow, bool InverseFlow)
{
    public static FlowExposure None => new(false, false);

    public static FlowExposure Forward => new(true, false);

    public static FlowExposure Inverse => new(false, true);

    public static FlowExposure Both => new(true, true);

    public bool IsNone => !ForwardFlow && !InverseFlow;

    public bool IsForwardOnly => ForwardFlow && !InverseFlow;

    public bool IsInverseOnly => !ForwardFlow && InverseFlow;

    public bool IsBoth => ForwardFlow && InverseFlow;

    public string Label => this switch
    {
        { ForwardFlow: false, InverseFlow: false } => "None",
        { ForwardFlow: true, InverseFlow: false } => "F",
        { ForwardFlow: false, InverseFlow: true } => "I",
        _ => "FI"
    };

    /// <summary>Swaps forward and inverse flow exposure while preserving none and both.</summary>
    public FlowExposure Invert() => new(InverseFlow, ForwardFlow);

    public override string ToString() => Label;
}

/// <summary>
/// Helper read for the sixteen boundary/flow combinations.
/// </summary>
public readonly record struct BoundaryFlowExposure(BoundaryExposure Boundary, FlowExposure Flow)
{
    public string Label => $"{Boundary}:{Flow}";

    public BoundaryFlowExposure DualBoundary() => new(Boundary.Dual(), Flow);

    public BoundaryFlowExposure InvertFlow() => new(Boundary, Flow.Invert());

    public BoundaryFlowExposure Dual() => new(Boundary.Dual(), Flow.Invert());

    public override string ToString() => Label;
}

/// <summary>
/// Local read value for the boundary side occupied by one value. This is a
/// compact read over lane-marker structure, not a concept tag.
/// </summary>
public readonly record struct BoundaryLane(bool PositiveSide, bool NegativeSide, bool WholeSegment)
{
    public static BoundaryLane Positive => new(true, false, false);

    public static BoundaryLane Negative => new(false, true, false);

    public static BoundaryLane Whole => new(false, false, true);

    public bool IsPositive => PositiveSide && !NegativeSide && !WholeSegment;

    public bool IsNegative => !PositiveSide && NegativeSide && !WholeSegment;

    public bool IsWhole => !PositiveSide && !NegativeSide && WholeSegment;

    public bool IsValid => IsPositive || IsNegative || IsWhole;

    public string Label => this switch
    {
        { PositiveSide: true, NegativeSide: false, WholeSegment: false } => "P",
        { PositiveSide: false, NegativeSide: true, WholeSegment: false } => "N",
        { PositiveSide: false, NegativeSide: false, WholeSegment: true } => "S",
        _ => "Invalid"
    };

    public override string ToString() => Label;
}

/// <summary>
/// Local read value for the flow face occupied by one value. No-flow is a real
/// occupied lane for position/partition values; it is not a missing marker.
/// </summary>
public readonly record struct FlowLane(bool ForwardFace, bool InverseFace, bool NoFlowFace)
{
    public static FlowLane None => new(false, false, true);

    public static FlowLane Forward => new(true, false, false);

    public static FlowLane Inverse => new(false, true, false);

    public bool IsNone => !ForwardFace && !InverseFace && NoFlowFace;

    public bool IsForward => ForwardFace && !InverseFace && !NoFlowFace;

    public bool IsInverse => !ForwardFace && InverseFace && !NoFlowFace;

    public bool IsValid => IsNone || IsForward || IsInverse;

    public string Label => this switch
    {
        { ForwardFace: false, InverseFace: false, NoFlowFace: true } => "None",
        { ForwardFace: true, InverseFace: false, NoFlowFace: false } => "F",
        { ForwardFace: false, InverseFace: true, NoFlowFace: false } => "I",
        _ => "Invalid"
    };

    public override string ToString() => Label;
}

/// <summary>
/// Occupied lane for a magnitude value on a boundary/flow basis. For example,
/// magnitude 5 on this lane can read as +5f, -5i, or a no-flow whole value.
/// </summary>
public readonly record struct BoundaryFlowLane(BoundaryLane Boundary, FlowLane Flow)
{
    public static BoundaryFlowLane PositiveForward => new(BoundaryLane.Positive, FlowLane.Forward);

    public static BoundaryFlowLane PositiveInverse => new(BoundaryLane.Positive, FlowLane.Inverse);

    public static BoundaryFlowLane PositiveNone => new(BoundaryLane.Positive, FlowLane.None);

    public static BoundaryFlowLane NegativeForward => new(BoundaryLane.Negative, FlowLane.Forward);

    public static BoundaryFlowLane NegativeInverse => new(BoundaryLane.Negative, FlowLane.Inverse);

    public static BoundaryFlowLane NegativeNone => new(BoundaryLane.Negative, FlowLane.None);

    public static BoundaryFlowLane WholeForward => new(BoundaryLane.Whole, FlowLane.Forward);

    public static BoundaryFlowLane WholeInverse => new(BoundaryLane.Whole, FlowLane.Inverse);

    public static BoundaryFlowLane WholeNone => new(BoundaryLane.Whole, FlowLane.None);

    public bool IsValid => Boundary.IsValid && Flow.IsValid;

    public string Label => !IsValid
        ? "Invalid"
        : Flow.IsNone
            ? $"{Boundary}:None"
            : $"{Boundary}{Flow}";

    public override string ToString() => Label;
}

/// <summary>
/// Reads the legal occupied lanes implied by a boundary/flow exposure. The
/// exposure defines the legal lane basis; individual values still carry their
/// chosen occupied lane locally.
/// </summary>
public static class BoundaryFlowLaneBasis
{
    public static BoundaryLane[] BoundaryLanes(BoundaryExposure boundary) =>
        boundary switch
        {
            { LeftBoundary: false, RightBoundary: false } => [BoundaryLane.Positive, BoundaryLane.Negative],
            { LeftBoundary: true, RightBoundary: false } => [BoundaryLane.Positive],
            { LeftBoundary: false, RightBoundary: true } => [BoundaryLane.Negative],
            _ => [BoundaryLane.Whole]
        };

    public static FlowLane[] FlowLanes(FlowExposure flow) =>
        flow switch
        {
            { ForwardFlow: false, InverseFlow: false } => [FlowLane.None],
            { ForwardFlow: true, InverseFlow: false } => [FlowLane.Forward],
            { ForwardFlow: false, InverseFlow: true } => [FlowLane.Inverse],
            _ => [FlowLane.Forward, FlowLane.Inverse]
        };

    public static BoundaryFlowLane[] LegalLanes(BoundaryExposure boundary, FlowExposure flow)
    {
        var boundaryLanes = BoundaryLanes(boundary);
        var flowLanes = FlowLanes(flow);
        var lanes = new List<BoundaryFlowLane>(boundaryLanes.Length * flowLanes.Length);

        foreach (var boundaryLane in boundaryLanes)
        {
            foreach (var flowLane in flowLanes)
            {
                lanes.Add(new BoundaryFlowLane(boundaryLane, flowLane));
            }
        }

        return [.. lanes];
    }

    public static BoundaryFlowLane[] LegalLanes(BoundaryFlowExposure exposure) =>
        LegalLanes(exposure.Boundary, exposure.Flow);

    public static bool Allows(BoundaryExposure boundary, FlowExposure flow, BoundaryFlowLane lane) =>
        lane.IsValid &&
        BoundaryLanes(boundary).Contains(lane.Boundary) &&
        FlowLanes(flow).Contains(lane.Flow);

    public static bool Allows(BoundaryFlowExposure exposure, BoundaryFlowLane lane) =>
        Allows(exposure.Boundary, exposure.Flow, lane);

    public static bool TryInferSingleLane(
        BoundaryExposure boundary,
        FlowExposure flow,
        out BoundaryFlowLane lane)
    {
        var lanes = LegalLanes(boundary, flow);
        if (lanes.Length == 1)
        {
            lane = lanes[0];
            return true;
        }

        lane = default;
        return false;
    }

    public static bool TryReadLegalLanes(
        Relation boundaryFlowSurface,
        out BoundaryFlowExposure exposure,
        out BoundaryFlowLane[] lanes)
    {
        if (BoundaryFlowSurface.TryRead(boundaryFlowSurface, out exposure))
        {
            lanes = LegalLanes(exposure);
            return true;
        }

        exposure = default;
        lanes = [];
        return false;
    }

    public static Relation Create(
        Carrier root,
        Relation boundaryFlowSurface,
        IElement selected)
    {
        if (!BoundaryFlowSurface.TryRead(
                boundaryFlowSurface,
                out var boundary,
                out var flow,
                out var boundarySurface,
                out var flowSurface,
                out _))
        {
            return Relation.OrderedGroup(
                ConceptSurface.RootOrNew(root),
                boundaryFlowSurface ?? ConceptSurface.UnknownPoint(),
                selected ?? ConceptSurface.UnknownPoint(),
                ConceptSurface.UnknownPoint());
        }

        var lanes = LegalLanes(boundary, flow);
        var elements = new IElement[lanes.Length + 2];
        elements[0] = boundaryFlowSurface;
        elements[1] = selected ?? ConceptSurface.UnknownPoint();

        for (var index = 0; index < lanes.Length; index++)
        {
            elements[index + 2] = BoundaryFlowLaneSurface.Create(
                new Carrier(),
                lanes[index],
                boundarySurface,
                flowSurface);
        }

        return Relation.OrderedGroup(ConceptSurface.RootOrNew(root), elements);
    }

    public static Relation Create(
        Carrier root,
        BoundaryExposure boundary,
        FlowExposure flow,
        IElement selected)
    {
        var boundaryFlowSurface = BoundaryFlowSurface.Create(new Carrier(), boundary, flow, selected);
        return Create(root, boundaryFlowSurface, selected);
    }

    public static bool TryRead(
        Relation basis,
        out BoundaryFlowExposure exposure,
        out IElement selected,
        out BoundaryFlowLane[] lanes)
    {
        if (ConceptSurfaceReader.TryReadRootElements(basis, out var elements) &&
            elements.Length >= 2 &&
            elements[0] is Relation boundaryFlowSurface &&
            BoundaryFlowSurface.TryRead(boundaryFlowSurface, out exposure))
        {
            var explicitLanes = new List<BoundaryFlowLane>();
            for (var index = 2; index < elements.Length; index++)
            {
                if (elements[index] is Relation laneSurface &&
                    BoundaryFlowLaneSurface.TryRead(laneSurface, out var lane, out _, out _))
                {
                    explicitLanes.Add(lane);
                }
            }

            selected = elements[1];
            lanes = explicitLanes.Count == 0 ? LegalLanes(exposure) : [.. explicitLanes];
            return true;
        }

        exposure = default;
        selected = default!;
        lanes = [];
        return false;
    }

    internal static bool TryReadExposure(Relation surface, out BoundaryFlowExposure exposure)
    {
        if (BoundaryFlowSurface.TryRead(surface, out exposure) ||
            TryRead(surface, out exposure, out _, out _))
        {
            return true;
        }

        if (CarrierAxisSurface.TryRead(surface, out _, out var boundary, out var flow, out _, out _))
        {
            exposure = new BoundaryFlowExposure(boundary, flow);
            return true;
        }

        exposure = default;
        return false;
    }
}

/// <summary>
/// Visible lane-marker surface for the boundary side occupied by a value.
/// </summary>
public static class BoundaryLaneSurface
{
    public static Relation Create(Carrier root, BoundaryLane lane, IElement evidence) =>
        Relation.OrderedGroup(
            ConceptSurface.RootOrNew(root),
            LaneBit(lane.PositiveSide),
            LaneBit(lane.NegativeSide),
            LaneBit(lane.WholeSegment),
            evidence ?? ConceptSurface.UnknownPoint());

    public static bool TryRead(
        Relation surface,
        out BoundaryLane lane,
        out IElement evidence)
    {
        if (ConceptSurfaceReader.TryReadRootElements(surface, out var elements) &&
            elements.Length >= 4 &&
            TryReadBit(elements[0], out var positive) &&
            TryReadBit(elements[1], out var negative) &&
            TryReadBit(elements[2], out var whole))
        {
            lane = new BoundaryLane(positive, negative, whole);
            evidence = elements[3];
            return lane.IsValid;
        }

        lane = default;
        evidence = default!;
        return false;
    }

    private static Relation LaneBit(bool isPresent) =>
        Relation.Point(new Carrier(), isPresent ? 1 : 0);

    private static bool TryReadBit(IElement element, out bool isPresent)
    {
        if (ConceptSurfaceReader.TryReadPoint(element, out var location) &&
            (location.Ticks == 0 || location.Ticks == 1))
        {
            isPresent = location.Ticks == 1;
            return true;
        }

        isPresent = false;
        return false;
    }
}

/// <summary>
/// Visible lane-marker surface for the flow face occupied by a value.
/// </summary>
public static class FlowLaneSurface
{
    public static Relation Create(Carrier root, FlowLane lane, IElement evidence) =>
        Relation.OrderedGroup(
            ConceptSurface.RootOrNew(root),
            LaneBit(lane.ForwardFace),
            LaneBit(lane.InverseFace),
            LaneBit(lane.NoFlowFace),
            evidence ?? ConceptSurface.UnknownPoint());

    public static bool TryRead(
        Relation surface,
        out FlowLane lane,
        out IElement evidence)
    {
        if (ConceptSurfaceReader.TryReadRootElements(surface, out var elements) &&
            elements.Length >= 4 &&
            TryReadBit(elements[0], out var forward) &&
            TryReadBit(elements[1], out var inverse) &&
            TryReadBit(elements[2], out var none))
        {
            lane = new FlowLane(forward, inverse, none);
            evidence = elements[3];
            return lane.IsValid;
        }

        lane = default;
        evidence = default!;
        return false;
    }

    private static Relation LaneBit(bool isPresent) =>
        Relation.Point(new Carrier(), isPresent ? 1 : 0);

    private static bool TryReadBit(IElement element, out bool isPresent)
    {
        if (ConceptSurfaceReader.TryReadPoint(element, out var location) &&
            (location.Ticks == 0 || location.Ticks == 1))
        {
            isPresent = location.Ticks == 1;
            return true;
        }

        isPresent = false;
        return false;
    }
}

/// <summary>
/// Visible marker that attaches one boundary lane and one flow lane.
/// </summary>
public static class BoundaryFlowLaneSurface
{
    public static Relation Create(
        Carrier root,
        BoundaryFlowLane lane,
        IElement boundaryEvidence,
        IElement flowEvidence)
    {
        var boundaryLaneSurface = BoundaryLaneSurface.Create(
            new Carrier(),
            lane.Boundary,
            boundaryEvidence ?? ConceptSurface.UnknownPoint());
        var flowLaneSurface = FlowLaneSurface.Create(
            new Carrier(),
            lane.Flow,
            flowEvidence ?? ConceptSurface.UnknownPoint());

        return Relation.OrderedGroup(
            ConceptSurface.RootOrNew(root),
            boundaryLaneSurface,
            flowLaneSurface);
    }

    public static Relation Create(Carrier root, BoundaryFlowLane lane) =>
        Create(root, lane, ConceptSurface.UnknownPoint(), ConceptSurface.UnknownPoint());

    public static bool TryRead(
        Relation surface,
        out BoundaryFlowLane lane,
        out Relation boundaryLaneSurface,
        out Relation flowLaneSurface)
    {
        if (ConceptSurfaceReader.TryReadRootElements(surface, out var elements) &&
            elements.Length >= 2 &&
            elements[0] is Relation boundaryCandidate &&
            elements[1] is Relation flowCandidate &&
            BoundaryLaneSurface.TryRead(boundaryCandidate, out var boundaryLane, out _) &&
            FlowLaneSurface.TryRead(flowCandidate, out var flowLane, out _))
        {
            lane = new BoundaryFlowLane(boundaryLane, flowLane);
            boundaryLaneSurface = boundaryCandidate;
            flowLaneSurface = flowCandidate;
            return lane.IsValid;
        }

        lane = default;
        boundaryLaneSurface = null!;
        flowLaneSurface = null!;
        return false;
    }
}

/// <summary>
/// Attaches an unsigned magnitude point to one occupied boundary/flow lane.
/// The carrier/domain surface describes what lanes are legal; this structure
/// describes where this value actually sits.
/// </summary>
public static class BoundaryFlowValueSurface
{
    public static Relation Create(
        Carrier root,
        Carrier valueCarrier,
        ulong magnitude,
        BoundaryFlowLane lane,
        IElement laneBasis,
        IElement residueOrTension)
    {
        var carrier = valueCarrier ?? Carrier.Unknown;
        var point = Relation.Point(carrier, checked((long)magnitude));
        return Create(root, point, lane, laneBasis, residueOrTension);
    }

    public static Relation Create(
        Carrier root,
        Carrier valueCarrier,
        ulong magnitude,
        BoundaryFlowLane lane,
        IElement laneBasis) =>
        Create(root, valueCarrier, magnitude, lane, laneBasis, ConceptSurface.UnknownPoint());

    public static Relation Create(
        Carrier root,
        Relation magnitudePoint,
        BoundaryFlowLane lane,
        IElement laneBasis,
        IElement residueOrTension)
    {
        var marker = BoundaryFlowLaneSurface.Create(
            new Carrier(),
            lane,
            laneBasis ?? ConceptSurface.UnknownPoint(),
            laneBasis ?? ConceptSurface.UnknownPoint());

        return Relation.OrderedGroup(
            ConceptSurface.RootOrNew(root),
            magnitudePoint ?? ConceptSurface.UnknownPoint(),
            marker,
            laneBasis ?? ConceptSurface.UnknownPoint(),
            residueOrTension ?? ConceptSurface.UnknownPoint());
    }

    public static bool TryRead(
        Relation valueSurface,
        out CarrierLocation magnitude,
        out BoundaryFlowLane lane,
        out Relation laneMarker,
        out IElement laneBasis,
        out IElement residueOrTension)
    {
        if (ConceptSurfaceReader.TryReadRootElements(valueSurface, out var elements) &&
            elements.Length >= 4 &&
            ConceptSurfaceReader.TryReadPoint(elements[0], out var magnitudeRead) &&
            elements[1] is Relation laneSurface &&
            BoundaryFlowLaneSurface.TryRead(laneSurface, out var laneRead, out _, out _))
        {
            magnitude = magnitudeRead;
            lane = laneRead;
            laneMarker = laneSurface;
            laneBasis = elements[2];
            residueOrTension = elements[3];
            return true;
        }

        magnitude = default;
        lane = default;
        laneMarker = null!;
        laneBasis = default!;
        residueOrTension = default!;
        return false;
    }

    public static bool TryReadOccupation(
        Relation valueSurface,
        Relation laneBasis,
        out CarrierLocation magnitude,
        out BoundaryFlowLane lane,
        out bool isLegal)
    {
        if (!TryRead(valueSurface, out magnitude, out lane, out _, out _, out _) ||
            !BoundaryFlowLaneBasis.TryReadExposure(laneBasis, out var exposure))
        {
            isLegal = false;
            return false;
        }

        isLegal = BoundaryFlowLaneBasis.Allows(exposure, lane);
        return true;
    }

    public static bool TryReadValue(
        Relation valueSurface,
        out BoundaryFlowValueRead read)
    {
        if (!TryRead(valueSurface, out var magnitude, out var lane, out _, out var laneBasis, out var residueOrTension) ||
            laneBasis is not Relation basisRelation ||
            !BoundaryFlowLaneBasis.TryReadExposure(basisRelation, out var exposure))
        {
            read = default;
            return false;
        }

        read = new BoundaryFlowValueRead(
            magnitude,
            lane,
            exposure,
            BoundaryFlowLaneBasis.Allows(exposure, lane),
            residueOrTension);
        return true;
    }

    public static bool TryReadValue(
        Relation valueSurface,
        Relation laneBasis,
        out BoundaryFlowValueRead read)
    {
        if (!TryRead(valueSurface, out var magnitude, out var lane, out _, out _, out var residueOrTension) ||
            !BoundaryFlowLaneBasis.TryReadExposure(laneBasis, out var exposure))
        {
            read = default;
            return false;
        }

        read = new BoundaryFlowValueRead(
            magnitude,
            lane,
            exposure,
            BoundaryFlowLaneBasis.Allows(exposure, lane),
            residueOrTension);
        return true;
    }

    public static bool TryCreateInferred(
        Carrier root,
        Relation magnitudePoint,
        Relation laneBasis,
        out Relation valueSurface)
    {
        if (!ConceptSurfaceReader.TryReadPoint(magnitudePoint, out _) ||
            !BoundaryFlowLaneBasis.TryReadExposure(laneBasis, out var exposure) ||
            !BoundaryFlowLaneBasis.TryInferSingleLane(exposure.Boundary, exposure.Flow, out var lane))
        {
            valueSurface = null!;
            return false;
        }

        valueSurface = Create(
            ConceptSurface.RootOrNew(root),
            magnitudePoint,
            lane,
            laneBasis,
            ConceptSurface.UnknownPoint());
        return true;
    }
}

/// <summary>
/// Compact read of a value surface through a lane basis. The raw magnitude and
/// occupied lane remain visible in the source structure.
/// </summary>
public readonly record struct BoundaryFlowValueRead(
    CarrierLocation Magnitude,
    BoundaryFlowLane Lane,
    BoundaryFlowExposure Basis,
    bool IsLegal,
    IElement ResidueOrTension)
{
    public bool IsUnsignedMagnitude => Magnitude.Ticks >= 0;

    public bool IsSignedBoundary => Lane.Boundary.IsPositive || Lane.Boundary.IsNegative;

    public bool IsWholeBoundary => Lane.Boundary.IsWhole;

    public bool IsNoFlow => Lane.Flow.IsNone;

    public bool IsForwardFlow => Lane.Flow.IsForward;

    public bool IsInverseFlow => Lane.Flow.IsInverse;

    public long SignedTicks => Lane.Boundary.IsNegative
        ? checked(-Magnitude.Ticks)
        : Magnitude.Ticks;

    public string Notation
    {
        get
        {
            var sign = Lane.Boundary.IsNegative ? "-" : string.Empty;
            var suffix = Lane.Flow.IsInverse
                ? "i"
                : Lane.Flow.IsForward
                    ? "f"
                    : Lane.Boundary.IsWhole
                        ? "s"
                        : string.Empty;

            return $"{sign}{Magnitude.Ticks}{suffix}";
        }
    }
}

/// <summary>
/// Small lane-settlement experiment. It applies a signed delta to the visible
/// value read without changing core carrier settlement.
/// </summary>
public static class BoundaryFlowValueSettlement
{
    public static bool TryApplySignedDelta(
        Relation valueSurface,
        Relation laneBasis,
        long deltaTicks,
        out Relation settledValue)
    {
        if (!BoundaryFlowValueSurface.TryReadValue(valueSurface, laneBasis, out var read) ||
            !read.IsUnsignedMagnitude ||
            !read.IsSignedBoundary)
        {
            settledValue = null!;
            return false;
        }

        var target = checked(read.SignedTicks + deltaTicks);
        var targetLane = new BoundaryFlowLane(
            target < 0 ? BoundaryLane.Negative : BoundaryLane.Positive,
            read.Lane.Flow);
        var targetMagnitude = AbsAsLong(target);

        if (BoundaryFlowLaneBasis.Allows(read.Basis, targetLane))
        {
            settledValue = BoundaryFlowValueSurface.Create(
                new Carrier(),
                Relation.Point(read.Magnitude.Carrier, targetMagnitude),
                targetLane,
                laneBasis,
                Relation.Point(read.Magnitude.Carrier, 0));
            return true;
        }

        var heldLane = new BoundaryFlowLane(read.Lane.Boundary, read.Lane.Flow);
        settledValue = BoundaryFlowValueSurface.Create(
            new Carrier(),
            Relation.Point(read.Magnitude.Carrier, 0),
            heldLane,
            laneBasis,
            Relation.Point(read.Magnitude.Carrier, target));
        return true;
    }

    private static long AbsAsLong(long value)
    {
        if (value == long.MinValue)
        {
            return long.MaxValue;
        }

        return Math.Abs(value);
    }
}

/// <summary>
/// Lane-aware route projection between value surfaces. This is a route-adjacent
/// experiment: the projection relation moves the signed/whole location, while
/// the target lane basis decides whether the resulting occupied lane is legal.
/// </summary>
public static class BoundaryFlowValueRoute
{
    public static Relation Create(
        Carrier root,
        Relation sourceValue,
        Relation projectionRoute,
        Relation targetBasis,
        Relation targetValue,
        IElement residueOrTension)
    {
        return Relation.OrderedGroup(
            ConceptSurface.RootOrNew(root),
            sourceValue ?? ConceptSurface.UnknownPoint(),
            projectionRoute ?? ConceptSurface.UnknownPoint(),
            targetBasis ?? ConceptSurface.UnknownPoint(),
            targetValue ?? ConceptSurface.UnknownPoint(),
            residueOrTension ?? ConceptSurface.UnknownPoint());
    }

    public static bool TryProject(
        Relation sourceValue,
        Relation projectionRoute,
        Relation targetBasis,
        Carrier targetCarrier,
        out Relation routeSurface)
    {
        if (!BoundaryFlowValueSurface.TryReadValue(sourceValue, out var sourceRead) ||
            !sourceRead.IsLegal ||
            projectionRoute is null ||
            targetBasis is null ||
            targetCarrier is null ||
            targetCarrier.IsUnknown ||
            !projectionRoute.TryProject(ProjectedSourceLocation(sourceRead), targetCarrier, out var projected) ||
            !projected.TryGetCarrierLocations(out var targetLocation, out var projectionResidue))
        {
            routeSurface = null!;
            return false;
        }

        var targetLane = ProjectedTargetLane(sourceRead, targetLocation, targetBasis);
        var isAllowed = BoundaryFlowLaneBasis.TryReadExposure(targetBasis, out var targetExposure) &&
            BoundaryFlowLaneBasis.Allows(targetExposure, targetLane);
        var residueOrTension = isAllowed
            ? Relation.Point(projectionResidue.Carrier, projectionResidue.Ticks)
            : Relation.Point(targetLocation.Carrier, targetLocation.Ticks);
        var targetValue = BoundaryFlowValueSurface.Create(
            new Carrier(),
            Relation.Point(targetLocation.Carrier, AbsAsLong(targetLocation.Ticks)),
            targetLane,
            targetBasis,
            residueOrTension);

        routeSurface = Create(
            new Carrier(),
            sourceValue,
            projectionRoute,
            targetBasis,
            targetValue,
            residueOrTension);
        return true;
    }

    public static bool TryRead(
        Relation routeSurface,
        out Relation sourceValue,
        out Relation projectionRoute,
        out Relation targetBasis,
        out Relation targetValue,
        out IElement residueOrTension)
    {
        if (ConceptSurfaceReader.TryReadRootElements(routeSurface, out var elements) &&
            elements.Length >= 5 &&
            elements[0] is Relation source &&
            elements[1] is Relation projection &&
            elements[2] is Relation basis &&
            elements[3] is Relation target)
        {
            sourceValue = source;
            projectionRoute = projection;
            targetBasis = basis;
            targetValue = target;
            residueOrTension = elements[4];
            return true;
        }

        sourceValue = null!;
        projectionRoute = null!;
        targetBasis = null!;
        targetValue = null!;
        residueOrTension = default!;
        return false;
    }

    public static bool TryReadTargetValue(
        Relation routeSurface,
        out BoundaryFlowValueRead targetRead)
    {
        if (TryRead(
                routeSurface,
                out _,
                out _,
                out var targetBasis,
                out var targetValue,
                out _))
        {
            return BoundaryFlowValueSurface.TryReadValue(targetValue, targetBasis, out targetRead);
        }

        targetRead = default;
        return false;
    }

    private static CarrierLocation ProjectedSourceLocation(BoundaryFlowValueRead sourceRead) =>
        sourceRead.IsWholeBoundary
            ? sourceRead.Magnitude
            : new CarrierLocation(sourceRead.Magnitude.Carrier, sourceRead.SignedTicks);

    private static BoundaryFlowLane ProjectedTargetLane(
        BoundaryFlowValueRead sourceRead,
        CarrierLocation targetLocation,
        Relation targetBasis)
    {
        var boundary = sourceRead.IsWholeBoundary
            ? BoundaryLane.Whole
            : targetLocation.Ticks < 0
                ? BoundaryLane.Negative
                : targetLocation.Ticks > 0
                    ? BoundaryLane.Positive
                    : ZeroBoundaryLane(sourceRead.Lane.Boundary, targetBasis);
        return new BoundaryFlowLane(boundary, sourceRead.Lane.Flow);
    }

    private static BoundaryLane ZeroBoundaryLane(BoundaryLane sourceBoundary, Relation targetBasis)
    {
        if (BoundaryFlowLaneBasis.TryReadExposure(targetBasis, out var exposure) &&
            BoundaryFlowLaneBasis.BoundaryLanes(exposure.Boundary).Contains(sourceBoundary))
        {
            return sourceBoundary;
        }

        if (BoundaryFlowLaneBasis.TryReadExposure(targetBasis, out exposure) &&
            BoundaryFlowLaneBasis.BoundaryLanes(exposure.Boundary) is [var only])
        {
            return only;
        }

        return BoundaryLane.Positive;
    }

    private static long AbsAsLong(long value)
    {
        if (value == long.MinValue)
        {
            return long.MaxValue;
        }

        return Math.Abs(value);
    }
}

/// <summary>
/// Accepts or blocks a lane-aware projected route value. The source route
/// remains visible; this surface adds accepted value, blocked value, and the
/// capacity or gate witness used by the local read.
/// </summary>
public static class BoundaryFlowValueTransfer
{
    public static Relation Create(
        Carrier root,
        Relation routeSurface,
        IElement acceptedValue,
        IElement blockedValue,
        IElement capacityOrGate)
    {
        return Relation.OrderedGroup(
            ConceptSurface.RootOrNew(root),
            routeSurface ?? ConceptSurface.UnknownPoint(),
            acceptedValue ?? ConceptSurface.UnknownPoint(),
            blockedValue ?? ConceptSurface.UnknownPoint(),
            capacityOrGate ?? ConceptSurface.UnknownPoint());
    }

    public static bool TryAccept(
        Relation routeSurface,
        out Relation transferSurface) =>
        TryAccept(routeSurface, null, out transferSurface);

    public static bool TryAccept(
        Relation routeSurface,
        Relation? capacity,
        out Relation transferSurface)
    {
        if (!BoundaryFlowValueRoute.TryRead(
                routeSurface,
                out _,
                out _,
                out var targetBasis,
                out var projectedValue,
                out _) ||
            !BoundaryFlowValueSurface.TryReadValue(projectedValue, targetBasis, out var projectedRead) ||
            !projectedRead.IsUnsignedMagnitude)
        {
            transferSurface = null!;
            return false;
        }

        var capacityTicks = ReadCapacity(capacity, projectedRead.Magnitude.Ticks);
        if (!projectedRead.IsLegal)
        {
            var blocked = CreateTransferValue(
                projectedRead,
                projectedRead.Magnitude.Ticks,
                SignedResidueTicks(projectedRead.Lane, projectedRead.Magnitude.Ticks),
                targetBasis);
            transferSurface = Create(
                new Carrier(),
                routeSurface,
                ConceptSurface.UnknownPoint(),
                blocked,
                capacity ?? ConceptSurface.UnknownPoint());
            return true;
        }

        var acceptedTicks = Math.Min(projectedRead.Magnitude.Ticks, capacityTicks);
        var blockedTicks = checked(projectedRead.Magnitude.Ticks - acceptedTicks);
        var accepted = CreateTransferValue(projectedRead, acceptedTicks, 0, targetBasis);
        var blockedValue = CreateTransferValue(
            projectedRead,
            blockedTicks,
            SignedResidueTicks(projectedRead.Lane, blockedTicks),
            targetBasis);

        transferSurface = Create(
            new Carrier(),
            routeSurface,
            accepted,
            blockedValue,
            capacity ?? ConceptSurface.UnknownPoint());
        return true;
    }

    public static bool TryRead(
        Relation transferSurface,
        out Relation routeSurface,
        out IElement acceptedValue,
        out IElement blockedValue,
        out IElement capacityOrGate)
    {
        if (ConceptSurfaceReader.TryReadRootElements(transferSurface, out var elements) &&
            elements.Length >= 4 &&
            elements[0] is Relation route)
        {
            routeSurface = route;
            acceptedValue = elements[1];
            blockedValue = elements[2];
            capacityOrGate = elements[3];
            return true;
        }

        routeSurface = null!;
        acceptedValue = default!;
        blockedValue = default!;
        capacityOrGate = default!;
        return false;
    }

    public static bool TryReadAcceptedValue(
        Relation transferSurface,
        out BoundaryFlowValueRead acceptedRead)
    {
        if (TryRead(transferSurface, out _, out var accepted, out _, out _) &&
            accepted is Relation acceptedRelation)
        {
            return BoundaryFlowValueSurface.TryReadValue(acceptedRelation, out acceptedRead);
        }

        acceptedRead = default;
        return false;
    }

    public static bool TryReadBlockedValue(
        Relation transferSurface,
        out BoundaryFlowValueRead blockedRead)
    {
        if (TryRead(transferSurface, out _, out _, out var blocked, out _) &&
            blocked is Relation blockedRelation)
        {
            return BoundaryFlowValueSurface.TryReadValue(blockedRelation, out blockedRead);
        }

        blockedRead = default;
        return false;
    }

    public static bool TryReadSummary(
        Relation transferSurface,
        out BoundaryFlowTransferRead read)
    {
        var hasAccepted = TryReadAcceptedValue(transferSurface, out var accepted);
        if (!TryReadBlockedValue(transferSurface, out var blocked))
        {
            read = default;
            return false;
        }

        read = new BoundaryFlowTransferRead(
            hasAccepted ? accepted : default,
            blocked,
            hasAccepted);
        return true;
    }

    private static Relation CreateTransferValue(
        BoundaryFlowValueRead projectedRead,
        long magnitudeTicks,
        long residueTicks,
        Relation targetBasis) =>
        BoundaryFlowValueSurface.Create(
            new Carrier(),
            Relation.Point(projectedRead.Magnitude.Carrier, magnitudeTicks),
            projectedRead.Lane,
            targetBasis,
            Relation.Point(projectedRead.Magnitude.Carrier, residueTicks));

    private static long ReadCapacity(Relation? capacity, long fallback)
    {
        if (capacity is null ||
            !ConceptSurfaceReader.TryReadPoint(capacity, out var capacityLocation))
        {
            return fallback;
        }

        return Math.Max(0, capacityLocation.Ticks);
    }

    private static long SignedResidueTicks(BoundaryFlowLane lane, long magnitudeTicks) =>
        lane.Boundary.IsNegative ? checked(-magnitudeTicks) : magnitudeTicks;
}

public readonly record struct BoundaryFlowTransferRead(
    BoundaryFlowValueRead Accepted,
    BoundaryFlowValueRead Blocked,
    bool HasAccepted)
{
    public bool IsFullyAccepted =>
        HasAccepted &&
        Accepted.IsLegal &&
        Blocked.Magnitude.Ticks == 0;

    public bool IsPartiallyAccepted =>
        HasAccepted &&
        Accepted.Magnitude.Ticks > 0 &&
        Blocked.Magnitude.Ticks > 0;

    public bool IsFullyBlocked =>
        !HasAccepted ||
        Accepted.Magnitude.Ticks == 0 && Blocked.Magnitude.Ticks > 0;
}

/// <summary>
/// Applies an accepted transfer to an existing target value and records the
/// transition as a time-step witness. This is still a concept-surface
/// settlement prototype; it does not mutate carrier state.
/// </summary>
public static class BoundaryFlowTargetSettlement
{
    public static Relation Create(
        Carrier root,
        Relation targetBefore,
        Relation transferSurface,
        Relation targetAfter,
        IElement residue,
        Relation timeline)
    {
        return Relation.OrderedGroup(
            ConceptSurface.RootOrNew(root),
            targetBefore ?? ConceptSurface.UnknownPoint(),
            transferSurface ?? ConceptSurface.UnknownPoint(),
            targetAfter ?? ConceptSurface.UnknownPoint(),
            residue ?? ConceptSurface.UnknownPoint(),
            timeline ?? ConceptSurface.UnknownPoint());
    }

    public static bool TrySettle(
        Relation targetBefore,
        Relation transferSurface,
        out Relation settlementSurface)
    {
        if (!BoundaryFlowValueSurface.TryRead(
                targetBefore,
                out _,
                out _,
                out _,
                out var laneBasis,
                out _) ||
            laneBasis is not Relation targetBasis ||
            !BoundaryFlowValueTransfer.TryReadSummary(transferSurface, out var transferRead))
        {
            settlementSurface = null!;
            return false;
        }

        var residue = transferRead.Blocked.Magnitude.Ticks == 0
            ? transferRead.Accepted.ResidueOrTension
            : BlockedResidue(transferSurface);
        var targetAfter = targetBefore;

        if (transferRead.HasAccepted &&
            transferRead.Accepted.Magnitude.Ticks != 0)
        {
            if (!CanApplyAcceptedToTarget(targetBefore, transferRead.Accepted, out var deltaTicks) ||
                !BoundaryFlowValueSettlement.TryApplySignedDelta(
                    targetBefore,
                    targetBasis,
                    deltaTicks,
                    out targetAfter))
            {
                targetAfter = targetBefore;
                residue = transferRead.Accepted.ResidueOrTension;
            }
        }

        var time = new Carrier();
        var step = TimeStepSequence.Step(
            new Carrier(),
            time,
            0,
            1,
            targetBefore,
            transferSurface,
            targetAfter,
            residue);
        var timeline = TimeStepSequence.Create(new Carrier(), time, targetBefore, step);

        settlementSurface = Create(
            new Carrier(),
            targetBefore,
            transferSurface,
            targetAfter,
            residue,
            timeline);
        return true;
    }

    public static bool TryRead(
        Relation settlementSurface,
        out Relation targetBefore,
        out Relation transferSurface,
        out Relation targetAfter,
        out IElement residue,
        out Relation timeline)
    {
        if (ConceptSurfaceReader.TryReadRootElements(settlementSurface, out var elements) &&
            elements.Length >= 5 &&
            elements[0] is Relation before &&
            elements[1] is Relation transfer &&
            elements[2] is Relation after &&
            elements[4] is Relation time)
        {
            targetBefore = before;
            transferSurface = transfer;
            targetAfter = after;
            residue = elements[3];
            timeline = time;
            return true;
        }

        targetBefore = null!;
        transferSurface = null!;
        targetAfter = null!;
        residue = default!;
        timeline = null!;
        return false;
    }

    public static bool TryReadAfter(
        Relation settlementSurface,
        out BoundaryFlowValueRead afterRead)
    {
        if (TryRead(settlementSurface, out _, out _, out var targetAfter, out _, out _))
        {
            return BoundaryFlowValueSurface.TryReadValue(targetAfter, out afterRead);
        }

        afterRead = default;
        return false;
    }

    public static bool TryReadTimeline(Relation settlementSurface, out Relation timeline)
    {
        if (TryRead(settlementSurface, out _, out _, out _, out _, out var candidate) &&
            TimeStepSequence.TryRead(candidate, out _, out _, out _))
        {
            timeline = candidate;
            return true;
        }

        timeline = null!;
        return false;
    }

    private static bool CanApplyAcceptedToTarget(
        Relation targetBefore,
        BoundaryFlowValueRead accepted,
        out long deltaTicks)
    {
        if (!BoundaryFlowValueSurface.TryReadValue(targetBefore, out var before) ||
            !before.IsLegal ||
            !accepted.IsLegal ||
            !before.IsSignedBoundary ||
            !accepted.IsSignedBoundary ||
            before.Lane.Flow != accepted.Lane.Flow)
        {
            deltaTicks = 0;
            return false;
        }

        deltaTicks = accepted.SignedTicks;
        return true;
    }

    private static IElement BlockedResidue(Relation transferSurface)
    {
        if (BoundaryFlowValueTransfer.TryRead(
                transferSurface,
                out _,
                out _,
                out var blocked,
                out _))
        {
            return blocked;
        }

        return ConceptSurface.UnknownPoint();
    }
}

/// <summary>
/// Builds and reads boundary/domain exposure surfaces. A boundary surface
/// records whether left and right boundary sides are present, plus the
/// selected whole/axis being described.
/// </summary>
public static class BoundarySurface
{
    public static Relation Create(
        Carrier root,
        BoundaryExposure exposure,
        IElement wholeOrAxis,
        IElement leftBoundary,
        IElement rightBoundary)
    {
        return Relation.OrderedGroup(
            ConceptSurface.RootOrNew(root),
            BoundaryBit(exposure.LeftBoundary),
            BoundaryBit(exposure.RightBoundary),
            wholeOrAxis ?? ConceptSurface.UnknownPoint(),
            exposure.LeftBoundary ? leftBoundary ?? ConceptSurface.UnknownPoint() : ConceptSurface.UnknownPoint(),
            exposure.RightBoundary ? rightBoundary ?? ConceptSurface.UnknownPoint() : ConceptSurface.UnknownPoint());
    }

    public static Relation Create(
        Carrier root,
        BoundaryExposure exposure,
        IElement wholeOrAxis) =>
        Create(root, exposure, wholeOrAxis, ConceptSurface.UnknownPoint(), ConceptSurface.UnknownPoint());

    public static Relation FullSigned(Carrier root, IElement axis) =>
        Create(root, BoundaryExposure.FullSigned, axis);

    public static Relation Positive(Carrier root, IElement axis, IElement leftBoundary) =>
        Create(root, BoundaryExposure.Positive, axis, leftBoundary, ConceptSurface.UnknownPoint());

    public static Relation Negative(Carrier root, IElement axis, IElement rightBoundary) =>
        Create(root, BoundaryExposure.Negative, axis, ConceptSurface.UnknownPoint(), rightBoundary);

    public static Relation BoundedWhole(
        Carrier root,
        IElement whole,
        IElement leftBoundary,
        IElement rightBoundary) =>
        Create(root, BoundaryExposure.BoundedWhole, whole, leftBoundary, rightBoundary);

    public static bool TryRead(
        Relation surface,
        out BoundaryExposure exposure,
        out IElement wholeOrAxis,
        out IElement leftBoundary,
        out IElement rightBoundary)
    {
        if (ConceptSurfaceReader.TryReadRootElements(surface, out var elements) &&
            elements.Length >= 5 &&
            TryReadBit(elements[0], out var hasLeft) &&
            TryReadBit(elements[1], out var hasRight))
        {
            exposure = new BoundaryExposure(hasLeft, hasRight);
            wholeOrAxis = elements[2];
            leftBoundary = elements[3];
            rightBoundary = elements[4];
            return true;
        }

        exposure = default;
        wholeOrAxis = default!;
        leftBoundary = default!;
        rightBoundary = default!;
        return false;
    }

    private static Relation BoundaryBit(bool isPresent) =>
        Relation.Point(new Carrier(), isPresent ? 1 : 0);

    private static bool TryReadBit(IElement element, out bool isPresent)
    {
        if (ConceptSurfaceReader.TryReadPoint(element, out var location) &&
            (location.Ticks == 0 || location.Ticks == 1))
        {
            isPresent = location.Ticks == 1;
            return true;
        }

        isPresent = false;
        return false;
    }
}

/// <summary>
/// Builds and reads flow exposure surfaces. Flow exposure is a local
/// structural read over forward and inverse flow evidence surfaces.
/// </summary>
public static class FlowSurface
{
    public static Relation Create(
        Carrier root,
        FlowExposure exposure,
        IElement selected,
        IElement forwardFlow,
        IElement inverseFlow)
    {
        return Relation.OrderedGroup(
            ConceptSurface.RootOrNew(root),
            FlowBit(exposure.ForwardFlow),
            FlowBit(exposure.InverseFlow),
            selected ?? ConceptSurface.UnknownPoint(),
            exposure.ForwardFlow ? forwardFlow ?? ConceptSurface.UnknownPoint() : ConceptSurface.UnknownPoint(),
            exposure.InverseFlow ? inverseFlow ?? ConceptSurface.UnknownPoint() : ConceptSurface.UnknownPoint());
    }

    public static Relation Create(
        Carrier root,
        FlowExposure exposure,
        IElement selected) =>
        Create(root, exposure, selected, ConceptSurface.UnknownPoint(), ConceptSurface.UnknownPoint());

    public static Relation None(Carrier root, IElement selected) =>
        Create(root, FlowExposure.None, selected);

    public static Relation Forward(Carrier root, IElement selected, IElement forwardFlow) =>
        Create(root, FlowExposure.Forward, selected, forwardFlow, ConceptSurface.UnknownPoint());

    public static Relation Inverse(Carrier root, IElement selected, IElement inverseFlow) =>
        Create(root, FlowExposure.Inverse, selected, ConceptSurface.UnknownPoint(), inverseFlow);

    public static Relation Both(
        Carrier root,
        IElement selected,
        IElement forwardFlow,
        IElement inverseFlow) =>
        Create(root, FlowExposure.Both, selected, forwardFlow, inverseFlow);

    public static bool TryRead(
        Relation surface,
        out FlowExposure exposure,
        out IElement selected,
        out IElement forwardFlow,
        out IElement inverseFlow)
    {
        if (ConceptSurfaceReader.TryReadRootElements(surface, out var elements) &&
            elements.Length >= 5 &&
            TryReadBit(elements[0], out var hasForward) &&
            TryReadBit(elements[1], out var hasInverse))
        {
            exposure = new FlowExposure(hasForward, hasInverse);
            selected = elements[2];
            forwardFlow = elements[3];
            inverseFlow = elements[4];
            return true;
        }

        exposure = default;
        selected = default!;
        forwardFlow = default!;
        inverseFlow = default!;
        return false;
    }

    private static Relation FlowBit(bool isPresent) =>
        Relation.Point(new Carrier(), isPresent ? 1 : 0);

    private static bool TryReadBit(IElement element, out bool isPresent)
    {
        if (ConceptSurfaceReader.TryReadPoint(element, out var location) &&
            (location.Ticks == 0 || location.Ticks == 1))
        {
            isPresent = location.Ticks == 1;
            return true;
        }

        isPresent = false;
        return false;
    }
}

/// <summary>
/// Pairs a boundary surface and a flow surface without collapsing their axes.
/// </summary>
public static class BoundaryFlowSurface
{
    public static Relation Create(
        Carrier root,
        Relation boundarySurface,
        Relation flowSurface,
        IElement witness)
    {
        return Relation.OrderedGroup(
            ConceptSurface.RootOrNew(root),
            boundarySurface ?? ConceptSurface.UnknownPoint(),
            flowSurface ?? ConceptSurface.UnknownPoint(),
            witness ?? ConceptSurface.UnknownPoint());
    }

    public static Relation Create(
        Carrier root,
        Relation boundarySurface,
        Relation flowSurface) =>
        Create(root, boundarySurface, flowSurface, ConceptSurface.UnknownPoint());

    public static Relation Create(
        Carrier root,
        BoundaryExposure boundary,
        FlowExposure flow,
        IElement selected)
    {
        var boundarySurface = BoundarySurface.Create(new Carrier(), boundary, selected);
        var flowSurface = FlowSurface.Create(new Carrier(), flow, selected);
        return Create(root, boundarySurface, flowSurface);
    }

    public static bool TryRead(
        Relation surface,
        out BoundaryExposure boundary,
        out FlowExposure flow,
        out Relation boundarySurface,
        out Relation flowSurface,
        out IElement witness)
    {
        if (ConceptSurfaceReader.TryReadRootElements(surface, out var elements) &&
            elements.Length >= 3 &&
            elements[0] is Relation boundaryCandidate &&
            elements[1] is Relation flowCandidate &&
            BoundarySurface.TryRead(boundaryCandidate, out var boundaryExposure, out _, out _, out _) &&
            FlowSurface.TryRead(flowCandidate, out var flowExposure, out _, out _, out _))
        {
            boundary = boundaryExposure;
            flow = flowExposure;
            boundarySurface = boundaryCandidate;
            flowSurface = flowCandidate;
            witness = elements[2];
            return true;
        }

        boundary = default;
        flow = default;
        boundarySurface = null!;
        flowSurface = null!;
        witness = default!;
        return false;
    }

    public static bool TryRead(
        Relation surface,
        out BoundaryFlowExposure exposure)
    {
        if (TryRead(surface, out var boundary, out var flow, out _, out _, out _))
        {
            exposure = new BoundaryFlowExposure(boundary, flow);
            return true;
        }

        exposure = default;
        return false;
    }
}

/// <summary>
/// Packages the boundary/domain exposure that belongs to a carrier-like axis.
/// This surface intentionally does not own flow exposure.
/// </summary>
public static class CarrierBoundarySurface
{
    public static Relation Create(
        Carrier root,
        Carrier carrier,
        BoundaryExposure boundary,
        IElement leftBoundary,
        IElement rightBoundary,
        IElement partition)
    {
        var axisCarrier = carrier ?? Carrier.Unknown;
        var selected = axisCarrier.IsUnknown
            ? (IElement)ConceptSurface.UnknownPoint()
            : axisCarrier;
        var boundarySurface = BoundarySurface.Create(
            new Carrier(),
            boundary,
            selected,
            leftBoundary ?? ConceptSurface.UnknownPoint(),
            rightBoundary ?? ConceptSurface.UnknownPoint());

        return Relation.OrderedGroup(
            ConceptSurface.RootOrNew(root),
            selected,
            boundarySurface,
            partition ?? ConceptSurface.UnknownPoint());
    }

    public static Relation Create(
        Carrier root,
        Carrier carrier,
        BoundaryExposure boundary) =>
        Create(
            root,
            carrier,
            boundary,
            ConceptSurface.UnknownPoint(),
            ConceptSurface.UnknownPoint(),
            ConceptSurface.UnknownPoint());

    public static Relation FullSigned(Carrier root, Carrier carrier) =>
        Create(root, carrier, BoundaryExposure.FullSigned);

    public static Relation Positive(Carrier root, Carrier carrier, IElement leftBoundary) =>
        Create(root, carrier, BoundaryExposure.Positive, leftBoundary, ConceptSurface.UnknownPoint(), ConceptSurface.UnknownPoint());

    public static Relation Negative(Carrier root, Carrier carrier, IElement rightBoundary) =>
        Create(root, carrier, BoundaryExposure.Negative, ConceptSurface.UnknownPoint(), rightBoundary, ConceptSurface.UnknownPoint());

    public static Relation BoundedWhole(
        Carrier root,
        Carrier carrier,
        IElement leftBoundary,
        IElement rightBoundary,
        IElement partition) =>
        Create(root, carrier, BoundaryExposure.BoundedWhole, leftBoundary, rightBoundary, partition);

    public static bool TryRead(
        Relation surface,
        out Carrier carrier,
        out BoundaryExposure boundary,
        out Relation boundarySurface,
        out IElement partition)
    {
        if (ConceptSurfaceReader.TryReadRootElements(surface, out var elements) &&
            elements.Length >= 3 &&
            elements[0] is Carrier selectedCarrier &&
            elements[1] is Relation boundaryCandidate &&
            BoundarySurface.TryRead(boundaryCandidate, out var boundaryRead, out _, out _, out _))
        {
            carrier = selectedCarrier;
            boundary = boundaryRead;
            boundarySurface = boundaryCandidate;
            partition = elements[2];
            return true;
        }

        carrier = null!;
        boundary = default;
        boundarySurface = null!;
        partition = default!;
        return false;
    }
}

/// <summary>
/// Packages flow exposure that belongs to a relation/traversal surface. This
/// surface intentionally does not own carrier boundary/domain meaning.
/// </summary>
public static class RelationFlowSurface
{
    public static Relation Create(
        Carrier root,
        Relation relation,
        FlowExposure flow,
        IElement forwardFlow,
        IElement inverseFlow,
        IElement gateOrCapacity)
    {
        var selected = relation ?? ConceptSurface.UnknownPoint();
        var flowSurface = FlowSurface.Create(
            new Carrier(),
            flow,
            selected,
            forwardFlow ?? ConceptSurface.UnknownPoint(),
            inverseFlow ?? ConceptSurface.UnknownPoint());

        return Relation.OrderedGroup(
            ConceptSurface.RootOrNew(root),
            selected,
            flowSurface,
            gateOrCapacity ?? ConceptSurface.UnknownPoint());
    }

    public static Relation Create(Carrier root, Relation relation, FlowExposure flow) =>
        Create(root, relation, flow, ConceptSurface.UnknownPoint(), ConceptSurface.UnknownPoint(), ConceptSurface.UnknownPoint());

    public static Relation None(Carrier root, Relation relation) =>
        Create(root, relation, FlowExposure.None);

    public static Relation Forward(Carrier root, Relation relation, IElement forwardFlow) =>
        Create(root, relation, FlowExposure.Forward, forwardFlow, ConceptSurface.UnknownPoint(), ConceptSurface.UnknownPoint());

    public static Relation Inverse(Carrier root, Relation relation, IElement inverseFlow) =>
        Create(root, relation, FlowExposure.Inverse, ConceptSurface.UnknownPoint(), inverseFlow, ConceptSurface.UnknownPoint());

    public static Relation Both(
        Carrier root,
        Relation relation,
        IElement forwardFlow,
        IElement inverseFlow,
        IElement gateOrCapacity) =>
        Create(root, relation, FlowExposure.Both, forwardFlow, inverseFlow, gateOrCapacity);

    public static bool TryRead(
        Relation surface,
        out Relation relation,
        out FlowExposure flow,
        out Relation flowSurface,
        out IElement forwardFlow,
        out IElement inverseFlow,
        out IElement gateOrCapacity)
    {
        if (ConceptSurfaceReader.TryReadRootElements(surface, out var elements) &&
            elements.Length >= 3 &&
            elements[0] is Relation selectedRelation &&
            elements[1] is Relation flowCandidate &&
            FlowSurface.TryRead(
                flowCandidate,
                out var flowRead,
                out _,
                out var forward,
                out var inverse))
        {
            relation = selectedRelation;
            flow = flowRead;
            flowSurface = flowCandidate;
            forwardFlow = forward;
            inverseFlow = inverse;
            gateOrCapacity = elements[2];
            return true;
        }

        relation = null!;
        flow = default;
        flowSurface = null!;
        forwardFlow = default!;
        inverseFlow = default!;
        gateOrCapacity = default!;
        return false;
    }

    public static bool TryInvert(Relation surface, out Relation inverted)
    {
        if (!TryRead(
                surface,
                out var relation,
                out var flow,
                out _,
                out var forwardFlow,
                out var inverseFlow,
                out var gateOrCapacity))
        {
            inverted = null!;
            return false;
        }

        inverted = Create(
            new Carrier(),
            relation,
            flow.Invert(),
            inverseFlow,
            forwardFlow,
            gateOrCapacity);
        return true;
    }
}

/// <summary>
/// Packages the boundary/domain and flow exposure of a carrier-like axis as
/// inspectable relation structure. Current <see cref="Carrier"/> instances are
/// still materialized faces; this surface records the lower-resolution axis
/// view around one selected carrier without changing carrier identity.
/// </summary>
public static class CarrierAxisSurface
{
    public static Relation Create(
        Carrier root,
        Carrier carrier,
        BoundaryExposure boundary,
        FlowExposure flow,
        IElement boundaryWitness,
        IElement flowWitness)
    {
        var axisCarrier = carrier ?? Carrier.Unknown;
        var selected = axisCarrier.IsUnknown
            ? (IElement)ConceptSurface.UnknownPoint()
            : axisCarrier;
        var boundarySurface = BoundarySurface.Create(
            new Carrier(),
            boundary,
            selected,
            boundaryWitness ?? ConceptSurface.UnknownPoint(),
            boundaryWitness ?? ConceptSurface.UnknownPoint());
        var flowSurface = FlowSurface.Create(
            new Carrier(),
            flow,
            selected,
            flowWitness ?? ConceptSurface.UnknownPoint(),
            flowWitness ?? ConceptSurface.UnknownPoint());

        return Relation.OrderedGroup(
            ConceptSurface.RootOrNew(root),
            selected,
            boundarySurface,
            flowSurface);
    }

    public static Relation Create(
        Carrier root,
        Carrier carrier,
        BoundaryExposure boundary,
        FlowExposure flow) =>
        Create(root, carrier, boundary, flow, ConceptSurface.UnknownPoint(), ConceptSurface.UnknownPoint());

    public static Relation PositiveUnit(Carrier root, Carrier carrier) =>
        Create(root, carrier, BoundaryExposure.Positive, FlowExposure.Forward);

    public static Relation NegativeUnit(Carrier root, Carrier carrier) =>
        Create(root, carrier, BoundaryExposure.Negative, FlowExposure.Forward);

    public static Relation SignedForward(Carrier root, Carrier carrier) =>
        Create(root, carrier, BoundaryExposure.FullSigned, FlowExposure.Forward);

    public static Relation SignedBothFlows(Carrier root, Carrier carrier) =>
        Create(root, carrier, BoundaryExposure.FullSigned, FlowExposure.Both);

    public static Relation BoundedWholeNoFlow(Carrier root, Carrier carrier) =>
        Create(root, carrier, BoundaryExposure.BoundedWhole, FlowExposure.None);

    public static bool TryRead(
        Relation surface,
        out Carrier carrier,
        out BoundaryExposure boundary,
        out FlowExposure flow,
        out Relation boundarySurface,
        out Relation flowSurface)
    {
        if (ConceptSurfaceReader.TryReadRootElements(surface, out var elements) &&
            elements.Length >= 3 &&
            elements[0] is Carrier selectedCarrier &&
            elements[1] is Relation boundaryCandidate &&
            elements[2] is Relation flowCandidate &&
            BoundarySurface.TryRead(boundaryCandidate, out var boundaryRead, out _, out _, out _) &&
            FlowSurface.TryRead(flowCandidate, out var flowRead, out _, out _, out _))
        {
            carrier = selectedCarrier;
            boundary = boundaryRead;
            flow = flowRead;
            boundarySurface = boundaryCandidate;
            flowSurface = flowCandidate;
            return true;
        }

        carrier = null!;
        boundary = default;
        flow = default;
        boundarySurface = null!;
        flowSurface = null!;
        return false;
    }

    public static bool TryReadLegalLanes(
        Relation surface,
        out BoundaryFlowLane[] lanes)
    {
        if (TryRead(surface, out _, out var boundary, out var flow, out _, out _))
        {
            lanes = BoundaryFlowLaneBasis.LegalLanes(boundary, flow);
            return true;
        }

        lanes = [];
        return false;
    }

    /// <summary>
    /// Reads a structurally inverted axis surface without mutating the source.
    /// Boundary sides are dualed and flow exposure is inverted; no-flow and
    /// both-flow remain stable under inversion.
    /// </summary>
    public static bool TryInvert(Relation surface, out Relation inverted)
    {
        if (!TryRead(surface, out var carrier, out var boundary, out var flow, out _, out _))
        {
            inverted = null!;
            return false;
        }

        inverted = Create(new Carrier(), carrier, boundary.Dual(), flow.Invert());
        return true;
    }
}
