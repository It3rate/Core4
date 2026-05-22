using Gojo.Core;
using Gojo.Patterns;

namespace Gojo.ConceptShapes;

/// <summary>
/// Packages a concept shape as an ordered Gojo group. Meaning remains in the
/// relation structure; this helper only creates and reads the skeleton.
/// </summary>
public static class ConceptShape
{
    public const int SurfaceElementCount = 8;

    /// <summary>
    /// Creates a concept shape record:
    /// selected, topology, flow, parameters, constraints, exposed reads,
    /// settlement witness, refinement timeline.
    /// </summary>
    public static Relation Create(
        Carrier? root,
        IElement selectedStructure,
        IElement topologySurface,
        IElement flowSurface,
        IElement parameterSurface,
        IElement constraintSurface,
        IElement exposedReadSurface,
        IElement settlementWitness,
        IElement refinementTimeline)
    {
        return Relation.OrderedGroup(
            ConceptShapeRoot.RootOrNew(root),
            selectedStructure ?? ConceptShapeRoot.UnknownPoint(),
            topologySurface ?? ConceptShapeRoot.UnknownPoint(),
            flowSurface ?? ConceptShapeRoot.UnknownPoint(),
            parameterSurface ?? ConceptShapeRoot.UnknownPoint(),
            constraintSurface ?? ConceptShapeRoot.UnknownPoint(),
            exposedReadSurface ?? ConceptShapeRoot.UnknownPoint(),
            settlementWitness ?? ConceptShapeRoot.UnknownPoint(),
            refinementTimeline ?? ConceptShapeRoot.UnknownPoint());
    }

    /// <summary>Creates a minimal seed shape with only selected and topology surfaces.</summary>
    public static Relation CreateSeed(
        Carrier? root,
        IElement selectedStructure,
        IElement topologySurface) =>
        Create(
            root,
            selectedStructure,
            topologySurface,
            ConceptShapeRoot.UnknownPoint(),
            ConceptShapeRoot.UnknownPoint(),
            ConceptShapeRoot.UnknownPoint(),
            ConceptShapeRoot.UnknownPoint(),
            ConceptShapeRoot.UnknownPoint(),
            ConceptShapeRoot.UnknownPoint());

    /// <summary>Reads a concept shape created by <see cref="Create"/>.</summary>
    public static bool TryRead(
        Relation conceptShape,
        out Carrier holder,
        out IElement selectedStructure,
        out IElement topologySurface,
        out IElement flowSurface,
        out IElement parameterSurface,
        out IElement constraintSurface,
        out IElement exposedReadSurface,
        out IElement settlementWitness,
        out IElement refinementTimeline)
    {
        selectedStructure = default!;
        topologySurface = default!;
        flowSurface = default!;
        parameterSurface = default!;
        constraintSurface = default!;
        exposedReadSurface = default!;
        settlementWitness = default!;
        refinementTimeline = default!;

        if (!ConceptShapeRoot.TryReadOrderedElements(conceptShape, out holder, out var elements) ||
            elements.Length < SurfaceElementCount)
        {
            holder = default!;
            return false;
        }

        selectedStructure = elements[0];
        topologySurface = elements[1];
        flowSurface = elements[2];
        parameterSurface = elements[3];
        constraintSurface = elements[4];
        exposedReadSurface = elements[5];
        settlementWitness = elements[6];
        refinementTimeline = elements[7];
        return true;
    }

    /// <summary>Reads only the topology surface.</summary>
    public static bool TryReadTopology(Relation conceptShape, out IElement topologySurface) =>
        TryReadElementAt(conceptShape, 1, out topologySurface);

    /// <summary>Reads only the exposed read surface.</summary>
    public static bool TryReadExposedReads(Relation conceptShape, out IElement exposedReadSurface) =>
        TryReadElementAt(conceptShape, 5, out exposedReadSurface);

    /// <summary>Reads only the parameter surface.</summary>
    public static bool TryReadParameters(Relation conceptShape, out IElement parameterSurface) =>
        TryReadElementAt(conceptShape, 3, out parameterSurface);

    /// <summary>Reads only the refinement timeline surface.</summary>
    public static bool TryReadTimeline(Relation conceptShape, out IElement refinementTimeline) =>
        TryReadElementAt(conceptShape, 7, out refinementTimeline);

    /// <summary>Reads open placeholder bindings from the parameter surface.</summary>
    public static Relation[] ReadOpenSlotBindings(IElement parameterSurface) =>
        StructuralPattern.ReadSlotBindings(CollectBindingRelations(parameterSurface));

    /// <summary>Reads live-output exposures from the exposed read surface.</summary>
    public static int CountExposedReads(IElement exposedReadSurface) =>
        CountLiveOutputs(exposedReadSurface);

    /// <summary>True when the topology contains an open attach pattern.</summary>
    public static bool TryFindOpenAttach(IElement topologySurface, out Relation openAttach)
    {
        foreach (var relation in CollectBindingRelations(topologySurface))
        {
            if (TryReadOpenAttachGroup(relation, out openAttach))
            {
                return true;
            }
        }

        openAttach = null!;
        return false;
    }

    private static bool TryReadElementAt(Relation conceptShape, int index, out IElement surface)
    {
        surface = default!;
        if (!ConceptShapeRoot.TryReadOrderedElements(conceptShape, out _, out var elements) ||
            index < 0 ||
            index >= elements.Length)
        {
            return false;
        }

        surface = elements[index];
        return true;
    }

    private static bool TryReadOpenAttachGroup(Relation relation, out Relation openAttach)
    {
        if (!ConceptShapeRoot.TryReadOrderedElements(relation, out _, out var elements) ||
            elements.Length < 2)
        {
            openAttach = null!;
            return false;
        }

        var hasOutlet = elements.Any(IsOrthogonalOutlet);
        var hasSlot = elements.Any(static e => e is Relation r && StructuralPattern.TryReadSlotBinding(r, out _, out _));
        if (!hasOutlet || !hasSlot)
        {
            openAttach = null!;
            return false;
        }

        openAttach = relation;
        return true;
    }

    private static bool IsOrthogonalOutlet(IElement element) =>
        element is Relation relation &&
        UnitShape.TryReadOrthogonalSource(relation, out _);

    private static List<Relation> CollectBindingRelations(IElement element)
    {
        var relations = new List<Relation>();
        CollectRelations(element, relations);
        return relations;
    }

    private static void CollectRelations(IElement element, List<Relation> relations)
    {
        if (element is Relation relation)
        {
            relations.Add(relation);
            CollectRelations(relation.Left, relations);
            CollectRelations(relation.Right, relations);
        }
    }

    private static int CountLiveOutputs(IElement element)
    {
        var count = 0;
        Walk(element, relation =>
        {
            if (relation.Right is Relation)
            {
                count++;
            }
        });
        return count;
    }

    private static void Walk(IElement element, Action<Relation> visit)
    {
        if (element is not Relation relation)
        {
            return;
        }

        visit(relation);
        Walk(relation.Left, visit);
        Walk(relation.Right, visit);
    }
}
