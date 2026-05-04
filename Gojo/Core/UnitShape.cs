namespace Gojo.Core;

public sealed class UnitShape : IEquatable<UnitShape>
{
    private UnitShape(IElement surface)
    {
        Surface = surface ?? throw new ArgumentNullException(nameof(surface));
    }

    public IElement Surface { get; }

    public static UnitShape Atom(Carrier carrier) =>
        new(Point(carrier, 1));

    public static UnitShape Additive(params UnitShape[] shapes) =>
        Additive((IReadOnlyList<UnitShape>)shapes);

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

        return new UnitShape(new Relation(Chain([.. shapes.Select(shape => shape.Surface)]), Point(Carrier.Unknown, 1)));
    }

    public static UnitShape Product(params UnitShape[] shapes) =>
        Product((IReadOnlyList<UnitShape>)shapes);

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

        return new UnitShape(new Relation(Chain([.. shapes.Select(shape => shape.Surface)]), Segment(Carrier.Unknown, 0, 1)));
    }

    public int ProductParticipationOf(UnitShape unit)
    {
        if (CompatibleWith(unit))
        {
            return 1;
        }

        if (!TryReadProductOperands(Surface, out var operands))
        {
            return 0;
        }

        return operands.Sum(operand => new UnitShape(operand).ProductParticipationOf(unit));
    }

    public bool CompatibleWith(UnitShape other) => Equals(other);

    public bool Equals(UnitShape? other) =>
        other is not null &&
        Equals(Surface, other.Surface);

    public override bool Equals(object? obj) => obj is UnitShape other && Equals(other);

    public override int GetHashCode() => Surface.GetHashCode();

    public override string ToString() => Surface.ToString() ?? string.Empty;

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

    private static IElement[] Flatten(IElement element) =>
        element is Relation relation && !IsUnitPoint(relation) && !IsUnitSegment(relation)
            ? [.. Flatten(relation.Left), .. Flatten(relation.Right)]
            : [element];

    private static bool IsUnitSegment(IElement element) =>
        element is Relation relation &&
        relation.Left is Relation leftPoint &&
        relation.Right is Relation rightPoint &&
        IsPoint(leftPoint, Carrier.Unknown, 0) &&
        IsPoint(rightPoint, Carrier.Unknown, 1);

    private static bool IsUnitPoint(IElement element) =>
        element is Relation relation &&
        relation.Left is CarrierLocation location &&
        relation.Right is Carrier carrier &&
        location.Carrier.SameIdentityAs(carrier);

    private static bool IsPoint(Relation relation, Carrier carrier, long ticks) =>
        relation.Left is CarrierLocation location &&
        relation.Right is Carrier pointCarrier &&
        pointCarrier.SameIdentityAs(carrier) &&
        location.Carrier.SameIdentityAs(carrier) &&
        location.Ticks == ticks;

    private static Relation Point(Carrier carrier, long ticks) =>
        new(new CarrierLocation(carrier, ticks), carrier);

    private static Relation Segment(Carrier carrier, long left, long right) =>
        new(Point(carrier, left), Point(carrier, right));

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
}
