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
        new(new Relation(
            SnapshotOf(source ?? throw new ArgumentNullException(nameof(source))),
            Segment(Carrier.Unknown, 0, 0)));

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

        var first = shapes[0];
        if (shapes.All(first.CompatibleWith))
        {
            return first;
        }

        return new UnitShape(new Relation(Chain([.. shapes.Select(shape => shape.surface)]), Point(Carrier.Unknown, 1)));
    }

    /// <summary>Creates a multiplicative unit shape from operands.</summary>
    public static UnitShape Product(params UnitShape[] shapes) =>
        Product((IReadOnlyList<UnitShape>)shapes);

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

    /// <summary>Flattens nested product operands while preserving point atoms.</summary>
    private static IElement[] Flatten(IElement element) =>
        element is Relation relation &&
            !IsUnitPoint(relation) &&
            !IsUnitSegment(relation) &&
            !IsOrthogonalSurface(relation)
            ? [.. Flatten(relation.Left), .. Flatten(relation.Right)]
            : [element];

    /// <summary>Returns true for an orthogonal shape-unit surface.</summary>
    private static bool IsOrthogonalSurface(IElement element) =>
        element is Relation relation &&
        IsZeroSegment(relation.Right);

    /// <summary>Returns true for the unit segment marker used by product shapes.</summary>
    private static bool IsUnitSegment(IElement element) =>
        element is Relation relation &&
        relation.Left is Relation leftPoint &&
        relation.Right is Relation rightPoint &&
        IsPoint(leftPoint, Carrier.Unknown, 0) &&
        IsPoint(rightPoint, Carrier.Unknown, 1);

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

    /// <summary>Matches a point relation at a specific carrier tick.</summary>
    private static bool IsPoint(Relation relation, Carrier carrier, long ticks) =>
        relation.Left is CarrierLocation location &&
        relation.Right is Carrier pointCarrier &&
        pointCarrier.SameIdentityAs(carrier) &&
        location.Carrier.SameIdentityAs(carrier) &&
        location.Ticks == ticks;

    /// <summary>Builds a point relation for unit-shape encoding.</summary>
    private static Relation Point(Carrier carrier, long ticks) =>
        new(new CarrierLocation(carrier, ticks), carrier);

    /// <summary>Builds a segment relation for unit-shape encoding.</summary>
    private static Relation Segment(Carrier carrier, long left, long right) =>
        new(Point(carrier, left), Point(carrier, right));

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
