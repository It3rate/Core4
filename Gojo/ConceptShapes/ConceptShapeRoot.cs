using Gojo.Core;

namespace Gojo.ConceptShapes;

/// <summary>
/// Shared read helpers for ordered-group concept shape surfaces.
/// </summary>
internal static class ConceptShapeRoot
{
    public static Relation UnknownPoint() =>
        Relation.Point(Carrier.Unknown, 0);

    public static Carrier RootOrNew(Carrier? carrier) =>
        carrier ?? new Carrier();

    public static bool TryReadOrderedElements(Relation structure, out Carrier holder, out IElement[] elements)
    {
        holder = default!;
        elements = [];

        if (structure.Right is Carrier groupHolder &&
            structure.TryReadOrderedElementsOn(groupHolder, out var ordered))
        {
            holder = groupHolder;
            elements = ordered;
            return true;
        }

        return false;
    }
}
