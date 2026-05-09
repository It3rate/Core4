namespace Gojo.Core;

/// <summary>
/// Primitive ordered Gojo structure. A relation can be a point, segment, port,
/// operation surface, route, or nested structure depending on its elements;
/// behavior is derived from that relation shape rather than from operation
/// tags.
/// </summary>
public sealed class Relation : IElement, IEquatable<Relation>
{
    private static ulong idCounter = 1;

    /// <summary>Creates an ordered relation from left element to right element.</summary>
    public Relation(IElement left, IElement right)
    {
        Id = NextId();
        Left = left;
        Right = right;
    }

    /// <summary>Creates a point by grounding a carrier location on its carrier.</summary>
    public static Relation Point(Carrier carrier, long ticks) =>
        new(new CarrierLocation(carrier, ticks), carrier);

    /// <summary>Creates a directed segment from two carrier-grounded points.</summary>
    public static Relation Segment(Carrier carrier, long start, long end) =>
        new(Point(carrier, start), Point(carrier, end));

    /// <summary>Creates the uncalibrated orthogonal outlet surface for a source.</summary>
    public static Relation OrthogonalOutlet(IElement source) =>
        new(source ?? throw new ArgumentNullException(nameof(source)), Segment(Carrier.Unknown, 0, 0));

    public ulong Id { get; }

    public IElement Left { get; private set; }

    public IElement Right { get; private set; }

    #region Identity and Duality

    /// <summary>Duals a point by negating its location and other relations recursively.</summary>
    public Relation Dual() => IsPoint(out var location, out var carrier)
        ? new(location.Dual(), carrier)
        : new(Left.Dual(), Right.Dual());

    /// <summary>Swaps traversal order without changing either side's duality.</summary>
    public Relation Reverse() => new(Right, Left);

    /// <summary>Returns the structural dual as an element.</summary>
    IElement IElement.Dual() => Dual();

    /// <summary>Compares physical relation identity rather than structural equality.</summary>
    public bool SameIdentityAs(Relation other) => ReferenceEquals(this, other);

    #endregion

    #region Structural Perturbation and Settlement

    /// <summary>Moves or scales carrier locations participating in this relation.</summary>
    public bool Perturb(IElement perturbation)
    {
        if (perturbation is CarrierLocation delta)
        {
            return TryTransformLocations(location =>
                location.Carrier.SameIdentityAs(delta.Carrier)
                    ? LocationTransform.To(new CarrierLocation(location.Carrier, checked(location.Ticks + delta.Ticks)))
                    : LocationTransform.To(location));
        }

        if (perturbation is Relation relation &&
            TryReadBasis(relation.Left, out var source) &&
            TryReadBasis(relation.Right, out var target) &&
            source.Ticks != 0 &&
            target.Ticks != 0)
        {
            return TryTransformLocations(location =>
                TryScale(location, source.Ticks, target.Ticks, out var scaled)
                    ? LocationTransform.To(scaled)
                    : LocationTransform.Blocked);
        }

        return false;
    }

    /// <summary>Applies this relation as settlement structure in an attached world.</summary>
    internal bool Apply()
    {
        var left = CarrierSitesOf(Left);
        var right = CarrierSitesOf(Right);
        if (!TryReadOneCarrier(left, out var leftSites) ||
            !TryReadOneCarrier(right, out var rightSites))
        {
            return false;
        }

        if (leftSites.Carrier.SameIdentityAs(rightSites.Carrier))
        {
            return AccountLocal(leftSites.Carrier, leftSites.Max, rightSites.Min);
        }

        if (leftSites.Carrier.IsInverse != rightSites.Carrier.IsInverse &&
            rightSites.IsOrigin)
        {
            return leftSites.Carrier.BalanceOpposedValueWith(rightSites.Carrier) |
                RouteNegativeTension(leftSites, rightSites);
        }

        return rightSites.IsOrigin && RoutePositiveTension(leftSites, rightSites);
    }

    #endregion

    #region Local State Reads

    /// <summary>Reads boolean-like support participation from a carrier frame.</summary>
    public Relation SupportOn(Carrier reader)
    {
        var (reverse, forward) = ReadStatePair(reader);
        return SlotPair(reader, reverse, forward);
    }

    /// <summary>Reads overlapping opposed support as tension participation.</summary>
    public Relation TensionOn(Carrier reader)
    {
        var (reverse, forward) = ReadStatePair(reader);
        var tension = Math.Min(reverse, forward);
        return SlotPair(reader, tension, tension);
    }

    /// <summary>Reads non-overlapped support as exclusive participation.</summary>
    public Relation ExclusiveOn(Carrier reader)
    {
        var (reverse, forward) = ReadStatePair(reader);
        return forward >= reverse
            ? SlotPair(reader, 0, forward - reverse)
            : SlotPair(reader, reverse - forward, 0);
    }

    /// <summary>Reads this relation's sorted boundary sites in a carrier frame.</summary>
    public Relation BoundariesOn(Carrier reader)
    {
        if (!TryReadFrame(this, out var frame) || !FrameAccepts(frame, reader))
        {
            var zero = new CarrierLocation(reader, 0);
            return new(zero, zero);
        }

        frame = ResolveFrameCarrier(frame, reader);
        var leftTicks = frame.Start;
        var rightTicks = frame.End;
        var start = Math.Min(leftTicks, rightTicks);
        var end = Math.Max(leftTicks, rightTicks);
        return new(new CarrierLocation(reader, start), new CarrierLocation(reader, end));
    }

    /// <summary>Reads the unsigned span between sorted boundaries.</summary>
    public CarrierLocation SpanOn(Carrier reader)
    {
        var boundaries = BoundariesOn(reader);
        return new CarrierLocation(
            boundaries.LeftLocation.Carrier,
            boundaries.RightLocation.Ticks - boundaries.LeftLocation.Ticks);
    }

    /// <summary>Reads directed flow from left site to right site in a carrier frame.</summary>
    public CarrierLocation FlowOn(Carrier reader)
    {
        if (!TryReadFrame(this, out var frame) || !FrameAccepts(frame, reader))
        {
            return new CarrierLocation(reader, 0);
        }

        frame = ResolveFrameCarrier(frame, reader);
        return new CarrierLocation(reader, frame.Extent);
    }

    /// <summary>Reads traversal start/end states carrying boundary point plus flow.</summary>
    public Relation TraversalSitesOn(Carrier reader)
    {
        if (!TryReadFrame(this, out var frame) || !FrameAccepts(frame, reader))
        {
            var zero = new CarrierLocation(reader, 0);
            return new(new Relation(zero, zero), new Relation(zero, zero));
        }

        frame = ResolveFrameCarrier(frame, reader);
        var flow = FlowOn(reader);
        if (flow.Ticks == 0)
        {
            var zero = new CarrierLocation(reader, 0);
            return new(new Relation(zero, zero), new Relation(zero, zero));
        }

        var zeroFlow = new CarrierLocation(flow.Carrier, 0);
        return new(
            new Relation(new CarrierLocation(flow.Carrier, frame.Start), zeroFlow),
            new Relation(new CarrierLocation(flow.Carrier, frame.End), flow));
    }

    #endregion

    #region Projection and Ports

    /// <summary>Projects a location through this relation, falling back to zero plus residue.</summary>
    public Relation Project(CarrierLocation location, Carrier reader)
    {
        return TryProject(location, reader, out var projected)
            ? projected
            : ProjectionResult(new CarrierLocation(reader, 0), location);
    }

    /// <summary>Projects only when this relation surface accepts source and reader frames.</summary>
    public bool TryProject(CarrierLocation location, Carrier reader, out Relation projected)
    {
        if (TryProjectBetweenSurfaces(Left, Right, location, reader, out projected))
        {
            return true;
        }

        projected = ProjectionResult(new CarrierLocation(reader, 0), location);
        return false;
    }

    /// <summary>Projects between two relation surfaces in whichever direction matches.</summary>
    private static bool TryProjectBetweenSurfaces(
        IElement left,
        IElement right,
        CarrierLocation location,
        Carrier reader,
        out Relation projected)
    {
        if (!TryReadFrame(left, out var leftFrame) ||
            !TryReadFrame(right, out var rightFrame))
        {
            projected = ProjectionResult(new CarrierLocation(reader, 0), location);
            return false;
        }

        if (FrameAccepts(leftFrame, location.Carrier) &&
            FrameAccepts(rightFrame, reader))
        {
            projected = ProjectBetween(
                location,
                reader,
                ResolveFrameCarrier(leftFrame, location.Carrier),
                ResolveFrameCarrier(rightFrame, reader));
            return true;
        }

        if (FrameAccepts(rightFrame, location.Carrier) &&
            FrameAccepts(leftFrame, reader))
        {
            projected = ProjectBetween(
                location,
                reader,
                ResolveFrameCarrier(rightFrame, location.Carrier),
                ResolveFrameCarrier(leftFrame, reader));
            return true;
        }

        projected = ProjectionResult(new CarrierLocation(reader, 0), location);
        return false;
    }

    /// <summary>Imports value input through this relation as a slot port.</summary>
    public bool TrySetFrom(IElement input)
    {
        var imports = new List<SlotImport>();
        if (!TryCollectSlotImports(Left, Right, input, imports))
        {
            return false;
        }

        foreach (var import in imports)
        {
            import.Apply();
        }

        return imports.Count != 0;
    }

    /// <summary>Imports input by retargeting exposed structural coordinates.</summary>
    public bool TrySetStructureFrom(IElement input)
    {
        var imports = new List<StructuralSlotImport>();
        if (!TryCollectStructuralSlotImports(Left, Right, input, imports))
        {
            return false;
        }

        foreach (var import in imports)
        {
            import.Apply();
        }

        return imports.Count != 0;
    }

    #endregion

    #region Operation Surfaces and Crystallization

    /// <summary>Reads an operation surface over port input without mutating carriers.</summary>
    public bool TryReadOperationFrom(IElement input, Carrier reader, out Relation read) =>
        TryReadOperationFrom(input, reader, bindOutputToReader: false, out read);

    /// <summary>Reads an operation surface, optionally binding output to the reader.</summary>
    private bool TryReadOperationFrom(
        IElement input,
        Carrier reader,
        bool bindOutputToReader,
        out Relation read)
    {
        var operands = new List<OperationOperand>();
        if (!TryCollectOperationOperands(Left, input, reader, operands) ||
            operands.Count == 0)
        {
            read = default!;
            return false;
        }

        if (TryReadPointFrame(Right, out var pointFrame) &&
            TryResolveOperationTargetFrame(pointFrame, reader, bindOutputToReader, out var pointTargetFrame))
        {
            if (pointTargetFrame.Extent == 0)
            {
                read = default!;
                return false;
            }

            var result = operands.Aggregate(0L, (ticks, operand) => checked(ticks + operand.Ticks));
            read = OperationResult(pointTargetFrame, result, operands);
            return true;
        }

        if (TryReadFrame(Right, out var segmentFrame) &&
            TryResolveOperationTargetFrame(segmentFrame, reader, bindOutputToReader, out var segmentTargetFrame))
        {
            if (segmentTargetFrame.Extent == 0)
            {
                read = default!;
                return false;
            }

            var result = operands.Aggregate(1L, (ticks, operand) => checked(ticks * operand.Ticks));
            read = OperationResult(segmentTargetFrame, result, operands);
            return true;
        }

        read = default!;
        return false;
    }

    /// <summary>Calculates the unit shape exposed by this relation surface.</summary>
    public bool TryReadUnitShape(out UnitShape unitShape)
    {
        if (TryReadOperationUnitShape(out unitShape))
        {
            return true;
        }

        if (TryReadOrthogonalUnitShape(out unitShape))
        {
            return true;
        }

        if (TryReadPortSurface(this, out var port) &&
            TryReadPortUnitShape(port, out unitShape))
        {
            return true;
        }

        if (TryReadFrame(this, out var frame))
        {
            unitShape = frame.Carrier.UnitShape;
            return true;
        }

        unitShape = null!;
        return false;
    }

    /// <summary>Reads a source-to-zero-segment surface as an orthogonal shape unit.</summary>
    private bool TryReadOrthogonalUnitShape(out UnitShape unitShape)
    {
        if (UnitShape.TryReadOrthogonalSource(this, out var source))
        {
            unitShape = UnitShape.OrthogonalOf(source);
            return true;
        }

        unitShape = null!;
        return false;
    }

    /// <summary>Creates a carrier with this relation's derived unit shape only.</summary>
    public bool TryCrystallize(out Carrier carrier)
    {
        if (!TryReadUnitShape(out var unitShape))
        {
            carrier = null!;
            return false;
        }

        carrier = new Carrier(unitShape);
        return true;
    }

    /// <summary>Creates a crystal carrier and projects compatible current value into it.</summary>
    public bool TryCrystallize(IElement input, out Carrier carrier, out Relation read)
    {
        if (!TryReadUnitShape(out var unitShape))
        {
            carrier = null!;
            read = null!;
            return false;
        }

        var crystal = new Carrier(unitShape);
        if (!TryProjectCrystallizedValueFrom(input, crystal, out read))
        {
            carrier = null!;
            return false;
        }

        carrier = crystal;
        return true;
    }

    /// <summary>Projects compatible current operation value onto an existing crystal.</summary>
    public bool TryProjectCrystallizedValueFrom(IElement input, Carrier crystal, out Relation read)
    {
        if (!TryReadUnitShape(out var unitShape) ||
            !crystal.UnitShape.CompatibleWith(unitShape) ||
            !CanProjectOperationValueAsSingleCrystal() ||
            !TryReadOperationFrom(input, crystal, bindOutputToReader: true, out read) ||
            read.Left is not CarrierLocation target ||
            !target.Carrier.SameIdentityAs(crystal))
        {
            read = null!;
            return false;
        }

        crystal.Set(target.Ticks);
        return true;
    }

    #endregion

    #region Structure Inspection

    /// <summary>Returns all carrier locations contained by this relation.</summary>
    public CarrierLocation[] CarrierLocations()
    {
        return [.. LeftCarrierLocations(), .. RightCarrierLocations()];
    }

    /// <summary>Returns all carriers referenced by this relation or nested elements.</summary>
    public Carrier[] Carriers() => [.. CarriersOf(Left), .. CarriersOf(Right)];

    /// <summary>Returns carrier locations from the left element.</summary>
    public CarrierLocation[] LeftCarrierLocations() => CarrierLocationsOf(Left);

    /// <summary>Returns carrier locations from the right element.</summary>
    public CarrierLocation[] RightCarrierLocations() => CarrierLocationsOf(Right);

    /// <summary>Reads this relation as two direct carrier-location slots.</summary>
    public bool TryGetCarrierLocations(out CarrierLocation left, out CarrierLocation right)
    {
        if (Left is CarrierLocation leftLocation && Right is CarrierLocation rightLocation)
        {
            left = leftLocation;
            right = rightLocation;
            return true;
        }

        left = default;
        right = default;
        return false;
    }

    /// <summary>Returns the direct left carrier-location slot or throws.</summary>
    public CarrierLocation LeftLocation => Left is CarrierLocation location
        ? location
        : throw new InvalidOperationException("The left relation slot is not a carrier location.");

    /// <summary>Returns the direct right carrier-location slot or throws.</summary>
    public CarrierLocation RightLocation => Right is CarrierLocation location
        ? location
        : throw new InvalidOperationException("The right relation slot is not a carrier location.");

    #endregion

    #region Equality and Formatting

    /// <summary>Compares relation shape and element values, not physical identity.</summary>
    public bool Equals(Relation? other) =>
        other is not null &&
        Equals(Left, other.Left) &&
        Equals(Right, other.Right);

    public override bool Equals(object? obj) => obj is Relation other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Left, Right);

    public override string ToString() => $"{Left} -> {Right}";

    #endregion

    #region Frame Readers

    /// <summary>Reads this relation as a point relation grounded on its carrier.</summary>
    private bool IsPoint(out CarrierLocation location, out Carrier carrier)
    {
        if (Left is CarrierLocation leftLocation &&
            Right is Carrier rightCarrier &&
            leftLocation.Carrier.SameIdentityAs(rightCarrier))
        {
            location = leftLocation;
            carrier = rightCarrier;
            return true;
        }

        location = default;
        carrier = null!;
        return false;
    }

    /// <summary>Reads two locations only when both are visible in reader identity.</summary>
    private static bool TryReadInCarrier(
        Carrier reader,
        CarrierLocation left,
        CarrierLocation right,
        out long leftTicks,
        out long rightTicks)
    {
        if (!left.Carrier.SameIdentityAs(reader) || !right.Carrier.SameIdentityAs(reader))
        {
            leftTicks = 0;
            rightTicks = 0;
            return false;
        }

        leftTicks = left.Ticks;
        rightTicks = right.Ticks;
        return true;
    }

    /// <summary>Reads a frame's extent as a basis carrier location.</summary>
    private static bool TryReadBasis(IElement element, out CarrierLocation basis)
    {
        if (TryReadFrame(element, out var frame))
        {
            basis = new CarrierLocation(frame.Carrier, frame.Extent);
            return true;
        }

        basis = default;
        return false;
    }

    /// <summary>Reads a location, point, or segment-like surface as a carrier frame.</summary>
    private static bool TryReadFrame(IElement element, out CarrierFrame frame)
    {
        if (element is CarrierLocation location)
        {
            frame = new CarrierFrame(location.Carrier, 0, location.Ticks);
            return true;
        }

        if (element is not Relation relation)
        {
            frame = default;
            return false;
        }

        if (relation.IsPoint(out var pointLocation, out _))
        {
            frame = new CarrierFrame(pointLocation.Carrier, 0, pointLocation.Ticks);
            return true;
        }

        if (relation.TryGetCarrierLocations(out var left, out var right) &&
            left.Carrier.SameIdentityAs(right.Carrier))
        {
            frame = new CarrierFrame(left.Carrier, left.Ticks, right.Ticks);
            return true;
        }

        if (relation.Left is Relation leftPoint &&
            relation.Right is Relation rightPoint &&
            leftPoint.IsPoint(out var leftPointLocation, out _) &&
            rightPoint.IsPoint(out var rightPointLocation, out _) &&
            leftPointLocation.Carrier.SameIdentityAs(rightPointLocation.Carrier))
        {
            frame = new CarrierFrame(
                leftPointLocation.Carrier,
                leftPointLocation.Ticks,
                rightPointLocation.Ticks);
            return true;
        }

        frame = default;
        return false;
    }

    /// <summary>Reads only point surfaces as origin-to-point frames.</summary>
    private static bool TryReadPointFrame(IElement element, out CarrierFrame frame)
    {
        if (element is Relation relation && relation.IsPoint(out var pointLocation, out _))
        {
            frame = new CarrierFrame(pointLocation.Carrier, 0, pointLocation.Ticks);
            return true;
        }

        frame = default;
        return false;
    }

    #endregion

    #region Slot Import Helpers

    /// <summary>Collects value-port imports after validating the whole input shape.</summary>
    private static bool TryCollectSlotImports(
        IElement sourceSurface,
        IElement targetSurface,
        IElement input,
        List<SlotImport> imports)
    {
        if (input is Relation inputGroup &&
            sourceSurface is Relation sourceGroup &&
            targetSurface is Relation targetGroup)
        {
            return TryCollectSlotImports(sourceGroup.Left, targetGroup.Left, inputGroup.Left, imports) &&
                TryCollectSlotImports(sourceGroup.Right, targetGroup.Right, inputGroup.Right, imports);
        }

        if (input is not CarrierLocation inputLocation ||
            !TryProjectToTargetSurface(sourceSurface, targetSurface, inputLocation, out var projection))
        {
            return false;
        }

        imports.Add(new SlotImport(projection.Target.Carrier, projection.Target.Ticks, projection.Residue));
        return true;
    }

    /// <summary>Collects structural imports that retarget existing exposed coordinates.</summary>
    private static bool TryCollectStructuralSlotImports(
        IElement sourceSurface,
        IElement targetSurface,
        IElement input,
        List<StructuralSlotImport> imports)
    {
        if (input is Relation inputGroup &&
            sourceSurface is Relation sourceGroup &&
            targetSurface is Relation targetGroup &&
            IsGroupSurface(sourceSurface) &&
            IsGroupSurface(targetSurface) &&
            IsGroupSurface(input))
        {
            return TryCollectStructuralSlotImports(sourceGroup.Left, targetGroup.Left, inputGroup.Left, imports) &&
                TryCollectStructuralSlotImports(sourceGroup.Right, targetGroup.Right, inputGroup.Right, imports);
        }

        if (input is Relation && TryCollectAtomicStructuralFrameImport(sourceSurface, targetSurface, input, imports))
        {
            return true;
        }

        if (input is not CarrierLocation inputLocation ||
            !TryReadStructuralTarget(targetSurface, out var targetSite) ||
            !TryProjectToTargetSurface(sourceSurface, targetSurface, inputLocation, out var projection))
        {
            return false;
        }

        imports.Add(new StructuralSlotImport(targetSite, projection.Target.Ticks, projection.Residue));
        return true;
    }

    /// <summary>Imports a frame-readable structural value as one atomic extent.</summary>
    private static bool TryCollectAtomicStructuralFrameImport(
        IElement sourceSurface,
        IElement targetSurface,
        IElement input,
        List<StructuralSlotImport> imports)
    {
        if (!TryReadAtomicFrameInput(input, sourceSurface, out var inputExtent) ||
            !TryReadStructuralTarget(targetSurface, out var targetSite) ||
            !TryProjectToTargetSurface(sourceSurface, targetSurface, inputExtent, out var projection))
        {
            return false;
        }

        imports.Add(new StructuralSlotImport(targetSite, projection.Target.Ticks, projection.Residue));
        return true;
    }

    /// <summary>True for relation-shaped groups that are not simple frames.</summary>
    private static bool IsGroupSurface(IElement element) =>
        element is Relation && !TryReadFrame(element, out _);

    #endregion

    #region Operation and Unit Shape Helpers

    /// <summary>Collects operation operands by matching input shape to port surfaces.</summary>
    private static bool TryCollectOperationOperands(
        IElement portSurface,
        IElement input,
        Carrier reader,
        List<OperationOperand> operands)
    {
        if (TryReadPortSurface(portSurface, out var port))
        {
            return TryCollectOperationOperand(port, input, reader, operands);
        }

        if (input is Relation inputGroup &&
            portSurface is Relation portGroup)
        {
            return TryCollectOperationOperands(portGroup.Left, inputGroup.Left, reader, operands) &&
                TryCollectOperationOperands(portGroup.Right, inputGroup.Right, reader, operands);
        }

        return false;
    }

    /// <summary>Collects one operation operand from a port and input element.</summary>
    private static bool TryCollectOperationOperand(
        Relation port,
        IElement input,
        Carrier reader,
        List<OperationOperand> operands)
    {
        if (input is CarrierLocation location)
        {
            return TryCollectOperationOperand(port, location, reader, operands);
        }

        if (input is Relation &&
            TryReadAtomicFrameInput(input, port.Left, out var inputExtent))
        {
            return TryCollectOperationOperand(port, inputExtent, reader, operands);
        }

        return false;
    }

    /// <summary>Projects a concrete location through one operation operand port.</summary>
    private static bool TryCollectOperationOperand(
        Relation port,
        CarrierLocation input,
        Carrier reader,
        List<OperationOperand> operands)
    {
        if (!TryProjectThrough(port.Left, port.Right, input, reader, out var projection))
        {
            return false;
        }

        operands.Add(new OperationOperand(projection.Target.Ticks, projection.Residue));
        return true;
    }

    /// <summary>Projects input to a concrete target surface carrier.</summary>
    private static bool TryProjectToTargetSurface(
        IElement sourceSurface,
        IElement targetSurface,
        CarrierLocation input,
        out SurfaceProjection projection)
    {
        if (!TryReadFrame(targetSurface, out var targetFrame) ||
            targetFrame.Carrier.IsUnknown)
        {
            projection = default;
            return false;
        }

        return TryProjectThrough(sourceSurface, targetSurface, input, targetFrame.Carrier, out projection);
    }

    /// <summary>Projects input through surfaces and returns target plus residue slots.</summary>
    private static bool TryProjectThrough(
        IElement sourceSurface,
        IElement targetSurface,
        CarrierLocation input,
        Carrier reader,
        out SurfaceProjection projection)
    {
        if (!TryProjectBetweenSurfaces(sourceSurface, targetSurface, input, reader, out var projected) ||
            !projected.TryGetCarrierLocations(out var target, out var residue) ||
            !target.Carrier.SameIdentityAs(reader))
        {
            projection = default;
            return false;
        }

        projection = new SurfaceProjection(target, residue);
        return true;
    }

    /// <summary>Reads relation-shaped input as an atomic extent relative to a source frame.</summary>
    private static bool TryReadAtomicFrameInput(
        IElement input,
        IElement sourceSurface,
        out CarrierLocation inputExtent)
    {
        if (!TryReadFrame(input, out var inputFrame) ||
            !TryReadFrame(sourceSurface, out var sourceFrame))
        {
            inputExtent = default;
            return false;
        }

        inputExtent = new CarrierLocation(inputFrame.Carrier, checked(sourceFrame.Start + inputFrame.Extent));
        return true;
    }

    /// <summary>Reads an exposed port surface as source frame to target frame.</summary>
    private static bool TryReadPortSurface(IElement element, out Relation port)
    {
        if (element is Relation relation &&
            !TryReadFrame(relation, out _) &&
            TryReadFrame(relation.Left, out _) &&
            TryReadFrame(relation.Right, out _))
        {
            port = relation;
            return true;
        }

        port = null!;
        return false;
    }

    /// <summary>Calculates operation-result unit shape from operand port shapes.</summary>
    private bool TryReadOperationUnitShape(out UnitShape unitShape)
    {
        var operands = new List<UnitShape>();
        if (!TryCollectOperationUnitShapes(Left, operands) ||
            operands.Count == 0)
        {
            unitShape = null!;
            return false;
        }

        if (TryReadPointFrame(Right, out var pointFrame) &&
            pointFrame.Extent != 0)
        {
            unitShape = UnitShape.Additive(operands);
            return true;
        }

        if (TryReadFrame(Right, out var segmentFrame) &&
            segmentFrame.Extent != 0)
        {
            unitShape = UnitShape.Product(operands);
            return true;
        }

        unitShape = null!;
        return false;
    }

    /// <summary>Collects unit shapes from a grouped operation port surface.</summary>
    private static bool TryCollectOperationUnitShapes(
        IElement portSurface,
        List<UnitShape> unitShapes)
    {
        if (TryReadPortSurface(portSurface, out var port))
        {
            if (!TryReadPortUnitShape(port, out var unitShape))
            {
                return false;
            }

            unitShapes.Add(unitShape);
            return true;
        }

        if (portSurface is Relation portGroup)
        {
            return TryCollectOperationUnitShapes(portGroup.Left, unitShapes) &&
                TryCollectOperationUnitShapes(portGroup.Right, unitShapes);
        }

        return false;
    }

    /// <summary>Reads unit shape exposed by a port, with concrete target winning.</summary>
    private static bool TryReadPortUnitShape(Relation port, out UnitShape unitShape)
    {
        if (!TryReadFrame(port.Left, out var sourceFrame) ||
            !TryReadFrame(port.Right, out var targetFrame))
        {
            unitShape = null!;
            return false;
        }

        unitShape = targetFrame.Carrier.IsUnknown
            ? sourceFrame.Carrier.UnitShape
            : targetFrame.Carrier.UnitShape;
        return true;
    }

    /// <summary>Builds operation read output with target value and residue chain.</summary>
    private static Relation OperationResult(
        CarrierFrame targetFrame,
        long value,
        List<OperationOperand> operands) =>
        new(
            new CarrierLocation(
                targetFrame.Carrier,
                checked(targetFrame.Start + checked(value * targetFrame.Extent))),
            OperationResidue(operands));

    /// <summary>Chains operand residues in operation operand order.</summary>
    private static IElement OperationResidue(IReadOnlyList<OperationOperand> operands)
    {
        IElement residue = operands[^1].Residue;
        for (var index = operands.Count - 2; index >= 0; index--)
        {
            residue = new Relation(operands[index].Residue, residue);
        }

        return residue;
    }

    /// <summary>Finds the concrete structural coordinate retargeted by a surface.</summary>
    private static bool TryReadStructuralTarget(IElement element, out CarrierLocation targetSite)
    {
        if (element is CarrierLocation location)
        {
            targetSite = location;
            return true;
        }

        if (element is not Relation relation)
        {
            targetSite = default;
            return false;
        }

        if (relation.IsPoint(out var pointLocation, out _))
        {
            targetSite = pointLocation;
            return true;
        }

        if (relation.TryGetCarrierLocations(out _, out var right))
        {
            targetSite = right;
            return true;
        }

        if (relation.Left is Relation leftPoint &&
            relation.Right is Relation rightPoint &&
            leftPoint.IsPoint(out var leftPointLocation, out _) &&
            rightPoint.IsPoint(out var rightPointLocation, out _) &&
            leftPointLocation.Carrier.SameIdentityAs(rightPointLocation.Carrier))
        {
            targetSite = rightPointLocation;
            return true;
        }

        targetSite = default;
        return false;
    }

    /// <summary>Returns true when a frame accepts the requested concrete carrier.</summary>
    private static bool FrameAccepts(CarrierFrame frame, Carrier carrier) =>
        frame.Carrier.IsUnknown || carrier.SameIdentityAs(frame.Carrier);

    /// <summary>Replaces Unknown frame carrier with the concrete carrier for a read.</summary>
    private static CarrierFrame ResolveFrameCarrier(CarrierFrame frame, Carrier carrier) =>
        frame.Carrier.IsUnknown
            ? new CarrierFrame(carrier, frame.Start, frame.End)
            : frame;

    /// <summary>Resolves operation output frame, optionally binding it to the reader.</summary>
    private static bool TryResolveOperationTargetFrame(
        CarrierFrame frame,
        Carrier reader,
        bool bindOutputToReader,
        out CarrierFrame targetFrame)
    {
        if (bindOutputToReader)
        {
            targetFrame = new CarrierFrame(reader, frame.Start, frame.End);
            return true;
        }

        if (!FrameAccepts(frame, reader))
        {
            targetFrame = default;
            return false;
        }

        targetFrame = ResolveFrameCarrier(frame, reader);
        return true;
    }

    /// <summary>Allows scalar crystal value projection only for currently reducible shapes.</summary>
    private bool CanProjectOperationValueAsSingleCrystal()
    {
        var operands = new List<UnitShape>();
        if (!TryCollectOperationUnitShapes(Left, operands) ||
            operands.Count == 0)
        {
            return false;
        }

        if (TryReadPointFrame(Right, out var pointFrame) &&
            pointFrame.Extent != 0)
        {
            var first = operands[0];
            return operands.All(first.CompatibleWith);
        }

        return TryReadFrame(Right, out var segmentFrame) &&
            segmentFrame.Extent != 0;
    }

    #endregion

    #region Location Transforms and Projection Math

    /// <summary>Collects and commits relation-location updates as one structural change.</summary>
    private bool TryTransformLocations(Func<CarrierLocation, LocationTransform> transform)
    {
        var updates = new List<LocationUpdate>();
        if (!TryCollectLocationUpdates(transform, new HashSet<ulong>(), updates))
        {
            return false;
        }

        foreach (var update in updates)
        {
            if (update.Current.Carrier.SameIdentityAs(update.Next.Carrier))
            {
                update.Current.Tick.Set(update.Next.Ticks);
                continue;
            }

            if (update.IsLeft)
            {
                update.Relation.Left = update.Next;
            }
            else
            {
                update.Relation.Right = update.Next;
            }
        }

        return updates.Count != 0;
    }

    /// <summary>Recursively collects location updates while avoiding relation cycles.</summary>
    private bool TryCollectLocationUpdates(
        Func<CarrierLocation, LocationTransform> transform,
        HashSet<ulong> visited,
        List<LocationUpdate> updates)
    {
        if (!visited.Add(Id))
        {
            return true;
        }

        return TryCollect(Left, isLeft: true) &&
            TryCollect(Right, isLeft: false);

        bool TryCollect(IElement element, bool isLeft)
        {
            if (element is CarrierLocation location)
            {
                var transformed = transform(location);
                if (!transformed.CanTransform)
                {
                    return false;
                }

                if (!transformed.Location.Equals(location))
                {
                    updates.Add(new LocationUpdate(this, isLeft, location, transformed.Location));
                }

                return true;
            }

            return element is not Relation relation ||
                relation.TryCollectLocationUpdates(transform, visited, updates);
        }
    }

    /// <summary>Projects a location from a source frame into a target frame.</summary>
    private static Relation ProjectBetween(
        CarrierLocation location,
        Carrier reader,
        CarrierFrame source,
        CarrierFrame target)
    {
        var sourceBasis = source.Extent;
        var targetBasis = target.Extent;
        if (sourceBasis == 0 || targetBasis == 0)
        {
            return ProjectionResult(new CarrierLocation(reader, 0), location);
        }

        var relativeTicks = checked(location.Ticks - source.Start);
        if (relativeTicks == 0)
        {
            return ProjectionResult(new CarrierLocation(reader, target.Start), new CarrierLocation(location.Carrier, 0));
        }

        var sourceMagnitude = MagnitudeOf(sourceBasis);
        var targetMagnitude = MagnitudeOf(targetBasis);
        var locationMagnitude = MagnitudeOf(relativeTicks);
        var scaled = checked(locationMagnitude * targetMagnitude);
        var projectedMagnitude = scaled / sourceMagnitude;
        var residueMagnitude = scaled % sourceMagnitude;
        var sign = Math.Sign(relativeTicks) * Math.Sign(sourceBasis) * Math.Sign(targetBasis);

        return ProjectionResult(
            new CarrierLocation(reader, checked(target.Start + SignedTicks(projectedMagnitude, sign))),
            new CarrierLocation(location.Carrier, SignedTicks(residueMagnitude, Math.Sign(relativeTicks))));
    }

    /// <summary>Builds the standard projection result pair.</summary>
    private static Relation ProjectionResult(CarrierLocation projected, CarrierLocation residue) =>
        new(projected, residue);

    /// <summary>Returns positive magnitude of signed ticks.</summary>
    private static ulong MagnitudeOf(long ticks) =>
        ticks >= 0 ? checked((ulong)ticks) : checked((ulong)-ticks);

    /// <summary>Applies a sign to an unsigned tick magnitude.</summary>
    private static long SignedTicks(ulong ticks, int sign) =>
        sign < 0 ? -checked((long)ticks) : checked((long)ticks);

    /// <summary>Scales a location exactly between two basis extents.</summary>
    private static bool TryScale(
        CarrierLocation location,
        long sourceBasis,
        long targetBasis,
        out CarrierLocation scaled)
    {
        var sourceMagnitude = MagnitudeOf(sourceBasis);
        var targetMagnitude = MagnitudeOf(targetBasis);
        var locationMagnitude = MagnitudeOf(location.Ticks);
        var scaledMagnitude = checked(locationMagnitude * targetMagnitude);
        if (scaledMagnitude % sourceMagnitude != 0)
        {
            scaled = default;
            return false;
        }

        var sign = Math.Sign(location.Ticks) * Math.Sign(sourceBasis) * Math.Sign(targetBasis);
        scaled = new CarrierLocation(location.Carrier, SignedTicks(scaledMagnitude / sourceMagnitude, sign));
        return true;
    }

    #endregion

    #region Element Traversal Helpers

    /// <summary>Collects carrier locations from an element tree.</summary>
    private static CarrierLocation[] CarrierLocationsOf(IElement element) => element switch
    {
        CarrierLocation location => [location],
        Relation relation => relation.CarrierLocations(),
        _ => []
    };

    /// <summary>Collects carriers from an element tree.</summary>
    private static Carrier[] CarriersOf(IElement element) => element switch
    {
        Carrier carrier => [carrier],
        CarrierLocation location => [location.Carrier],
        Relation relation => relation.Carriers(),
        _ => []
    };

    #endregion

    #region Support Read Helpers

    /// <summary>Builds a reverse/forward slot pair on a reader carrier.</summary>
    private static Relation SlotPair(Carrier reader, ulong reverse, ulong forward) =>
        new(
            new CarrierLocation(reader, -checked((long)reverse)),
            new CarrierLocation(reader, forward));

    /// <summary>Reads nested state as reverse and forward support magnitudes.</summary>
    private (ulong Reverse, ulong Forward) ReadStatePair(Carrier reader)
    {
        if (TryGetCarrierLocations(out var left, out var right))
        {
            return ReadSlots(reader, left, right);
        }

        return Right is Relation state ? state.ReadStatePair(reader) : (0, 0);
    }

    /// <summary>Reads two direct slots as reverse/forward participation.</summary>
    private static (ulong Reverse, ulong Forward) ReadSlots(
        Carrier reader,
        CarrierLocation left,
        CarrierLocation right)
    {
        var reverse = 0UL;
        var forward = 0UL;
        AddSlot(left);
        AddSlot(right);
        return (reverse, forward);

        void AddSlot(CarrierLocation slot)
        {
            var read = slot.ReadFrom(reader);
            if (read.Direction > 0)
            {
                forward += read.Magnitude;
            }
            else if (read.Direction < 0)
            {
                reverse += read.Magnitude;
            }
        }
    }

    #endregion

    #region Settlement Helpers

    /// <summary>Applies same-carrier accounting for reset, wrap, and clamp shapes.</summary>
    private static bool AccountLocal(
        Carrier carrier,
        long source,
        long target)
    {
        if (source <= 0 || target < 0 || source < target)
        {
            return false;
        }

        var sourceTicks = checked((ulong)source);
        var targetTicks = checked((ulong)target);
        if (carrier.Value <= targetTicks)
        {
            return false;
        }

        var local = sourceTicks == targetTicks
            ? Math.Min(carrier.Value, sourceTicks)
            : checked(targetTicks + ((carrier.Value - targetTicks) % (sourceTicks - targetTicks)));
        return local != carrier.Value &&
            carrier.LeaveTension(local, checked((long)(carrier.Value - local)));
    }

    /// <summary>Routes positive source tension into an origin receiver relation.</summary>
    private static bool RoutePositiveTension(CarrierSites source, CarrierSites receiver)
    {
        var unit = source.Width == 0 ? source.Max : source.Width;
        if (unit <= 0)
        {
            return false;
        }

        var target = source.Width == 0 ? 0 : source.Min;
        var changed = AccountLocal(source.Carrier, source.Max, target);
        if (source.Carrier.Tension <= 0)
        {
            return changed;
        }

        var tension = checked((ulong)source.Carrier.Tension);
        var unitTicks = checked((ulong)unit);
        var received = tension / unitTicks;
        if (received == 0)
        {
            return changed;
        }

        changed |= source.Carrier.KeepTensionRemainder(checked((long)(tension % unitTicks)));
        receiver.Carrier.Receive(checked((long)received));
        return true;
    }

    /// <summary>Routes negative source tension into an inverse-origin receiver relation.</summary>
    private static bool RouteNegativeTension(CarrierSites source, CarrierSites receiver)
    {
        var unit = source.Width == 0 ? source.Max : source.Width;
        if (unit <= 0)
        {
            return false;
        }

        if (source.Carrier.Tension >= 0)
        {
            return false;
        }

        var tension = checked((ulong)-source.Carrier.Tension);
        var unitTicks = checked((ulong)unit);
        var received = tension / unitTicks;
        if (received == 0)
        {
            return false;
        }

        var changed = source.Carrier.KeepTensionRemainder(-checked((long)(tension % unitTicks)));
        receiver.Carrier.Receive(checked((long)received));
        return true;
    }

    /// <summary>Collects all carrier sites participating in settlement application.</summary>
    private static CarrierLocation[] CarrierSitesOf(IElement element) => element switch
    {
        CarrierLocation location => [location],
        Relation relation => [.. CarrierSitesOf(relation.Left), .. CarrierSitesOf(relation.Right)],
        _ => []
    };

    /// <summary>Reads a carrier-site set only when all sites share one carrier.</summary>
    private static bool TryReadOneCarrier(
        CarrierLocation[] sites,
        out CarrierSites carrierSites)
    {
        if (sites.Length == 0)
        {
            carrierSites = default;
            return false;
        }

        var carrier = sites[0].Carrier;
        if (sites.Any(site => !site.Carrier.SameIdentityAs(carrier)))
        {
            carrierSites = default;
            return false;
        }

        carrierSites = new(
            carrier,
            sites.Min(site => site.Ticks),
            sites.Max(site => site.Ticks));
        return true;
    }

    #endregion

    #region Local Data Shapes

    private readonly record struct CarrierSites(Carrier Carrier, long Min, long Max)
    {
        public long Width => Max - Min;

        public bool IsOrigin => Width == 0 && Min == 0;
    }

    private readonly record struct CarrierFrame(Carrier Carrier, long Start, long End)
    {
        public long Extent => End - Start;
    }

    private readonly record struct SurfaceProjection(CarrierLocation Target, CarrierLocation Residue);

    private readonly record struct OperationOperand(long Ticks, CarrierLocation Residue);

    private readonly record struct SlotImport(Carrier Target, long TargetTicks, CarrierLocation Residue)
    {
        public void Apply()
        {
            Target.Set(TargetTicks);
            Residue.Carrier.HoldResidueTension(Residue.Ticks);
        }
    }

    private readonly record struct StructuralSlotImport(
        CarrierLocation Target,
        long TargetTicks,
        CarrierLocation Residue)
    {
        public void Apply()
        {
            Target.Tick.Set(TargetTicks);
            Residue.Carrier.HoldResidueTension(Residue.Ticks);
        }
    }

    private readonly record struct LocationUpdate(
        Relation Relation,
        bool IsLeft,
        CarrierLocation Current,
        CarrierLocation Next);

    private readonly record struct LocationTransform(bool CanTransform, CarrierLocation Location)
    {
        public static LocationTransform Blocked => new(false, default);

        public static LocationTransform To(CarrierLocation location) => new(true, location);
    }

    /// <summary>Allocates the next physical relation identity.</summary>
    private static ulong NextId() => idCounter++;

    #endregion
}
