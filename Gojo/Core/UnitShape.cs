namespace Gojo.Core;

/// <summary>
/// Stable relation-shaped description of what one carrier tick means. Unit
/// shape is immutable by construction; Surface exposes snapshots so callers
/// can inspect without mutating a crystallized basis.
/// </summary>
public sealed class UnitShape : IEquatable<UnitShape>
{
    private readonly IElement surface;

    private UnitShape(IElement surface)
    {
        this.surface = surface ?? throw new ArgumentNullException(nameof(surface));
    }

    #region Construction and Surface Reads

    /// <summary>Returns an inspectable snapshot of the unit surface.</summary>
    public IElement Surface => SnapshotOf(surface);

    /// <summary>Creates a fresh linear unit atom grounded on a carrier.</summary>
    public static UnitShape Atom(Carrier carrier) =>
        new(Point(carrier, 1));

    /// <summary>Creates a shape unit by pushing a source element into its orthogonal.</summary>
    public static UnitShape OrthogonalOf(IElement source) =>
        new(Relation.OrthogonalOutlet(SnapshotOf(source ?? throw new ArgumentNullException(nameof(source)))));

    /// <summary>Creates a circular or level-line unit shape from a point source.</summary>
    public static UnitShape CircularFrom(Relation point)
    {
        if (!IsUnitPoint(point))
        {
            throw new ArgumentException("Circular unit shapes require a point source.", nameof(point));
        }

        return OrthogonalOf(point);
    }

    /// <summary>Creates a linear ray-like unit shape from coincident segment points.</summary>
    public static UnitShape LinearFrom(Relation zeroLengthSegment)
    {
        if (!TryReadSegment(zeroLengthSegment, out var left, out var right) ||
            !left.Carrier.SameIdentityAs(right.Carrier) ||
            left.Ticks != right.Ticks)
        {
            throw new ArgumentException(
                "Linear orthogonal unit shapes require a zero-length segment source.",
                nameof(zeroLengthSegment));
        }

        return OrthogonalOf(zeroLengthSegment);
    }

    /// <summary>Creates a sheet or rectangle-like unit shape from a nonzero segment.</summary>
    public static UnitShape SheetFrom(Relation segment)
    {
        if (!TryReadSegment(segment, out var left, out var right) ||
            !left.Carrier.SameIdentityAs(right.Carrier) ||
            left.Ticks == right.Ticks)
        {
            throw new ArgumentException(
                "Sheet orthogonal unit shapes require a nonzero segment source.",
                nameof(segment));
        }

        return OrthogonalOf(segment);
    }

    /// <summary>Creates an additive unit shape from operands.</summary>
    public static UnitShape Additive(params UnitShape[] shapes) =>
        Additive((IReadOnlyList<UnitShape>)shapes);

    /// <summary>Collapses compatible additive operands or preserves them side-by-side.</summary>
    public static UnitShape Additive(IReadOnlyList<UnitShape> shapes)
    {
        if (shapes.Count == 0)
        {
            return Atom(Carrier.Unknown);
        }

        var operands = DistinctCompatible(shapes);
        if (operands.Count == 1)
        {
            return operands[0];
        }

        return new UnitShape(new Relation(Chain([.. operands.Select(shape => shape.surface)]), Point(Carrier.Unknown, 1)));
    }

    /// <summary>Creates a multiplicative unit shape from operands.</summary>
    public static UnitShape Product(params UnitShape[] shapes) =>
        Product((IReadOnlyList<UnitShape>)shapes);

    /// <summary>Creates a same-type calibrated product from aligned operands.</summary>
    public static UnitShape AlignedProduct(params UnitShape[] shapes) =>
        AlignedProduct((IReadOnlyList<UnitShape>)shapes);

    /// <summary>Collapses compatible aligned operands to their shared type.</summary>
    public static UnitShape AlignedProduct(IReadOnlyList<UnitShape> shapes) =>
        Additive(shapes);

    /// <summary>Creates a new product type from non-aligned operand participation.</summary>
    public static UnitShape OrthogonalProduct(params UnitShape[] shapes) =>
        OrthogonalProduct((IReadOnlyList<UnitShape>)shapes);

    /// <summary>Creates a new product type from non-aligned operand participation.</summary>
    public static UnitShape OrthogonalProduct(IReadOnlyList<UnitShape> shapes) =>
        Product(shapes);

    /// <summary>Preserves repeated product participation as relation structure.</summary>
    public static UnitShape Product(IReadOnlyList<UnitShape> shapes)
    {
        if (shapes.Count == 0)
        {
            return Atom(Carrier.Unknown);
        }

        if (shapes.Count == 1)
        {
            return shapes[0];
        }

        return new UnitShape(new Relation(Chain([.. shapes.Select(shape => shape.surface)]), Segment(Carrier.Unknown, 0, 1)));
    }

    #endregion

    #region Compatibility and Participation Reads

    /// <summary>Counts this shape's product participation for a unit operand.</summary>
    public int ProductParticipationOf(UnitShape unit)
    {
        if (CompatibleWith(unit))
        {
            return 1;
        }

        if (!TryReadProductOperands(surface, out var operands))
        {
            return 0;
        }

        return operands.Sum(operand => new UnitShape(operand).ProductParticipationOf(unit));
    }

    /// <summary>Reads product operands when this shape represents orthogonal product participation.</summary>
    public bool TryReadOrthogonalProductOperands(out UnitShape[] operands)
    {
        if (TryReadProductOperands(surface, out var operandSurfaces))
        {
            operands = [.. operandSurfaces.Select(operand => new UnitShape(SnapshotOf(operand)))];
            return true;
        }

        operands = [];
        return false;
    }

    /// <summary>Reads unlike additive participation operands when this shape is a group.</summary>
    public bool TryReadAdditiveOperands(out UnitShape[] operands)
    {
        if (TryReadAdditiveOperands(surface, out var operandSurfaces))
        {
            operands = [.. operandSurfaces.Select(operand => new UnitShape(SnapshotOf(operand)))];
            return true;
        }

        operands = [];
        return false;
    }

    /// <summary>Reads the source element if this shape is an orthogonal push.</summary>
    public bool TryReadOrthogonalSource(out IElement source)
    {
        if (TryReadOrthogonalSourceCore(surface, out var orthogonalSource))
        {
            source = SnapshotOf(orthogonalSource);
            return true;
        }

        source = default!;
        return false;
    }

    /// <summary>Reads the source from an orthogonal shape-unit surface.</summary>
    public static bool TryReadOrthogonalSource(IElement element, out IElement source)
    {
        if (TryReadOrthogonalSourceCore(element, out var orthogonalSource))
        {
            source = SnapshotOf(orthogonalSource);
            return true;
        }

        source = default!;
        return false;
    }

    /// <summary>Reads the origin point of a circular/level-line orthogonal unit.</summary>
    public bool TryReadCircularSource(out CarrierLocation origin)
    {
        if (TryReadOrthogonalSourceCore(surface, out var source) &&
            source is Relation point &&
            IsPoint(point, out origin))
        {
            return true;
        }

        origin = default;
        return false;
    }

    /// <summary>Reads the collapsed origin segment of a ray/line orthogonal unit.</summary>
    public bool TryReadLinearSource(out CarrierLocation origin)
    {
        if (TryReadOrthogonalSourceCore(surface, out var source) &&
            source is Relation segment &&
            TryReadSegment(segment, out var left, out var right) &&
            left.Carrier.SameIdentityAs(right.Carrier) &&
            left.Ticks == right.Ticks)
        {
            origin = left;
            return true;
        }

        origin = default;
        return false;
    }

    /// <summary>Returns true when two unit shapes can collapse as the same unit.</summary>
    public bool CompatibleWith(UnitShape other) => Equals(other);

    /// <summary>Compares stable internal unit surfaces.</summary>
    public bool Equals(UnitShape? other) =>
        other is not null &&
        Equals(surface, other.surface);

    public override bool Equals(object? obj) => obj is UnitShape other && Equals(other);

    public override int GetHashCode() => surface.GetHashCode();

    public override string ToString() => surface.ToString() ?? string.Empty;

    #endregion

    #region Surface Helpers

    /// <summary>Copies relation/location structure so a read cannot mutate the basis.</summary>
    private static IElement SnapshotOf(IElement element) => element switch
    {
        CarrierLocation location => new CarrierLocation(location.Carrier, location.Ticks),
        Relation relation => new Relation(SnapshotOf(relation.Left), SnapshotOf(relation.Right)),
        _ => element
    };

    /// <summary>Reads product operands from the product-surface encoding.</summary>
    private static bool TryReadProductOperands(IElement surface, out IElement[] operands)
    {
        if (surface is Relation relation &&
            IsUnitSegment(relation.Right))
        {
            operands = Flatten(relation.Left);
            return true;
        }

        operands = [];
        return false;
    }

    /// <summary>Reads additive operands from the additive-surface encoding.</summary>
    private static bool TryReadAdditiveOperands(IElement surface, out IElement[] operands)
    {
        if (surface is Relation relation &&
            IsAdditiveMarker(relation.Right))
        {
            operands = FlattenAdditive(relation.Left);
            return true;
        }

        operands = [];
        return false;
    }

    /// <summary>Flattens nested product operands while preserving point atoms.</summary>
    private static IElement[] Flatten(IElement element) =>
        element is Relation relation &&
            !IsUnitPoint(relation) &&
            !IsUnitSegment(relation) &&
            !TryReadOrthogonalSourceCore(relation, out _)
            ? [.. Flatten(relation.Left), .. Flatten(relation.Right)]
            : [element];

    /// <summary>Flattens additive chains while preserving product and orthogonal operands.</summary>
    private static IElement[] FlattenAdditive(IElement element)
    {
        if (element is not Relation relation)
        {
            return [element];
        }

        if (IsAdditiveMarker(relation.Right))
        {
            return FlattenAdditive(relation.Left);
        }

        if (IsUnitPoint(relation) ||
            IsUnitSegment(relation) ||
            IsProductSurface(relation) ||
            TryReadOrthogonalSourceCore(relation, out _))
        {
            return [element];
        }

        return [.. FlattenAdditive(relation.Left), .. FlattenAdditive(relation.Right)];
    }

    /// <summary>Combines compatible additive operands while preserving first-seen order.</summary>
    private static List<UnitShape> DistinctCompatible(IReadOnlyList<UnitShape> shapes)
    {
        var operands = new List<UnitShape>();
        foreach (var shape in shapes)
        {
            if (!operands.Any(operand => operand.CompatibleWith(shape)))
            {
                operands.Add(shape);
            }
        }

        return operands;
    }

    /// <summary>Reads the source from an orthogonal shape-unit surface.</summary>
    private static bool TryReadOrthogonalSourceCore(IElement element, out IElement source)
    {
        if (element is Relation relation &&
            IsZeroSegment(relation.Right))
        {
            source = relation.Left;
            return true;
        }

        source = default!;
        return false;
    }

    /// <summary>Returns true for the unit segment marker used by product shapes.</summary>
    private static bool IsUnitSegment(IElement element) =>
        element is Relation relation &&
        relation.Left is Relation leftPoint &&
        relation.Right is Relation rightPoint &&
            IsPoint(leftPoint, Carrier.Unknown, 0) &&
            IsPoint(rightPoint, Carrier.Unknown, 1);

    /// <summary>Returns true for a product shape surface.</summary>
    private static bool IsProductSurface(IElement element) =>
        element is Relation relation &&
        IsUnitSegment(relation.Right);

    /// <summary>Returns true for the unit point marker used by additive group shapes.</summary>
    private static bool IsAdditiveMarker(IElement element) =>
        element is Relation relation &&
        IsPoint(relation, Carrier.Unknown, 1);

    /// <summary>Returns true for a carrier-grounded point atom.</summary>
    private static bool IsUnitPoint(IElement element) =>
        element is Relation relation &&
        relation.Left is CarrierLocation location &&
        relation.Right is Carrier carrier &&
        location.Carrier.SameIdentityAs(carrier);

    /// <summary>Returns true for the zero segment marker used by orthogonal shapes.</summary>
    private static bool IsZeroSegment(IElement element) =>
        element is Relation relation &&
        relation.Left is Relation leftPoint &&
        relation.Right is Relation rightPoint &&
        IsPoint(leftPoint, Carrier.Unknown, 0) &&
        IsPoint(rightPoint, Carrier.Unknown, 0);

    /// <summary>Reads a same-carrier segment relation built from point surfaces.</summary>
    private static bool TryReadSegment(
        Relation relation,
        out CarrierLocation left,
        out CarrierLocation right)
    {
        if (relation.Left is Relation leftPoint &&
            relation.Right is Relation rightPoint &&
            IsPoint(leftPoint, out left) &&
            IsPoint(rightPoint, out right) &&
            left.Carrier.SameIdentityAs(right.Carrier))
        {
            return true;
        }

        left = default;
        right = default;
        return false;
    }

    /// <summary>Reads a point relation without constraining its tick value.</summary>
    private static bool IsPoint(Relation relation, out CarrierLocation location)
    {
        if (relation.Left is CarrierLocation pointLocation &&
            relation.Right is Carrier carrier &&
            pointLocation.Carrier.SameIdentityAs(carrier))
        {
            location = pointLocation;
            return true;
        }

        location = default;
        return false;
    }

    /// <summary>Matches a point relation at a specific carrier tick.</summary>
    private static bool IsPoint(Relation relation, Carrier carrier, long ticks) =>
        IsPoint(relation, out var location) &&
        location.Carrier.SameIdentityAs(carrier) &&
        location.Ticks == ticks;

    /// <summary>Builds a point relation for unit-shape encoding.</summary>
    private static Relation Point(Carrier carrier, long ticks) =>
        Relation.Point(carrier, ticks);

    /// <summary>Builds a segment relation for unit-shape encoding.</summary>
    private static Relation Segment(Carrier carrier, long left, long right) =>
        Relation.Segment(carrier, left, right);

    /// <summary>Chains operands as visible relation-shaped participation.</summary>
    private static IElement Chain(IReadOnlyList<IElement> elements)
    {
        if (elements.Count == 0)
        {
            return Point(Carrier.Unknown, 1);
        }

        var surface = elements[0];
        for (var index = 1; index < elements.Count; index++)
        {
            surface = new Relation(surface, elements[index]);
        }

        return surface;
    }

    #endregion
}
