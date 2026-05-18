using Gojo.Core;

namespace Gojo.Patterns;

/// <summary>
/// Small helpers for applying placeholder bindings to ordinary Gojo structure.
/// The helpers produce relation/carrier/location structure; they are not a
/// durable pattern or repeat object.
/// </summary>
public static class StructuralPattern
{
    /// <summary>Creates relation-shaped binding from a placeholder to an element.</summary>
    public static Relation Bind(Placeholder placeholder, IElement element) =>
        new(placeholder, element);

    /// <summary>Reads relation-shaped binding from a placeholder to an element.</summary>
    public static bool TryReadBinding(Relation relation, out Placeholder placeholder, out IElement element)
    {
        if (relation.Left is Placeholder slot)
        {
            placeholder = slot;
            element = relation.Right;
            return true;
        }

        placeholder = default!;
        element = default!;
        return false;
    }

    /// <summary>Applies bindings to every matching placeholder inside a pattern.</summary>
    public static IElement Apply(IElement pattern, params Relation[] bindings) =>
        Apply(pattern, (IReadOnlyList<Relation>)bindings);

    /// <summary>Applies bindings to every matching placeholder inside a pattern.</summary>
    public static IElement Apply(IElement pattern, IReadOnlyList<Relation> bindings)
    {
        var map = ReadBindings(bindings);
        return Apply(pattern, map);
    }

    /// <summary>Instantiates one pattern once per binding frame.</summary>
    public static IElement[] Repeat(IElement pattern, params Relation[][] frames) =>
        Repeat(pattern, (IReadOnlyList<IReadOnlyList<Relation>>)frames);

    /// <summary>Instantiates one pattern once per binding frame.</summary>
    public static IElement[] Repeat(IElement pattern, IReadOnlyList<IReadOnlyList<Relation>> frames)
    {
        var copies = new IElement[frames.Count];
        for (var index = 0; index < frames.Count; index++)
        {
            copies[index] = Apply(pattern, frames[index]);
        }

        return copies;
    }

    /// <summary>
    /// Computes the next binding frame by applying the current frame to
    /// relation-shaped transition bindings.
    /// </summary>
    public static Relation[] Advance(IReadOnlyList<Relation> currentFrame, params Relation[] transitions) =>
        Advance(currentFrame, (IReadOnlyList<Relation>)transitions);

    /// <summary>
    /// Computes the next binding frame by applying the current frame to
    /// relation-shaped transition bindings.
    /// </summary>
    public static Relation[] Advance(
        IReadOnlyList<Relation> currentFrame,
        IReadOnlyList<Relation> transitions)
    {
        var current = ReadBindings(currentFrame);
        var next = new List<Relation>();
        var seen = new HashSet<Placeholder>();
        foreach (var transition in transitions)
        {
            if (TryReadBinding(transition, out var placeholder, out var element) &&
                seen.Add(placeholder))
            {
                next.Add(Bind(placeholder, Apply(element, current)));
            }
        }

        return [.. next];
    }

    /// <summary>Chains repeated copies into one relation-shaped surface.</summary>
    public static IElement Chain(IReadOnlyList<IElement> elements)
    {
        if (elements.Count == 0)
        {
            return new Placeholder();
        }

        var surface = elements[0];
        for (var index = 1; index < elements.Count; index++)
        {
            surface = new Relation(surface, elements[index]);
        }

        return surface;
    }

    private static IElement Apply(IElement element, IReadOnlyDictionary<Placeholder, IElement> bindings) =>
        element switch
        {
            Placeholder placeholder when TryReadBoundPlaceholder(placeholder, bindings, out var bound) => bound,
            Relation relation => new Relation(
                Apply(relation.Left, bindings),
                Apply(relation.Right, bindings)),
            CarrierLocation location => new CarrierLocation(location.Carrier, location.Tick),
            _ => element
        };

    private static bool TryReadBoundPlaceholder(
        Placeholder placeholder,
        IReadOnlyDictionary<Placeholder, IElement> bindings,
        out IElement bound)
    {
        if (bindings.TryGetValue(placeholder, out bound!))
        {
            return true;
        }

        var dual = placeholder.Dual();
        if (bindings.TryGetValue(dual, out var dualBound))
        {
            bound = dualBound.Dual();
            return true;
        }

        bound = default!;
        return false;
    }

    private static Dictionary<Placeholder, IElement> ReadBindings(IReadOnlyList<Relation> bindings)
    {
        var map = new Dictionary<Placeholder, IElement>();
        foreach (var binding in bindings)
        {
            if (TryReadBinding(binding, out var placeholder, out var element) &&
                !map.ContainsKey(placeholder))
            {
                map.Add(placeholder, element);
            }
        }

        return map;
    }
}
