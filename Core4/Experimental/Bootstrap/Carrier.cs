using System.Collections.Concurrent;
using System.Threading;

namespace Core4.Experimental.Bootstrap;

// A bootstrap carrier is the current accessible floor of resolution.
// It may be primitive like distance, or already condensed like speed.
// TODO: Keep carrier identity itself small and stable. Usage-local surface such
// as free/frozen role, expansive/contractive role, and repeated participation
// likely belongs to the crystallized tick surface rather than to Carrier
// globally. Some carriers may remain intentionally opaque and expose only
// identity plus optional provenance.
public readonly record struct Carrier(long Id, long Domain = 0)
{
    private static readonly ConcurrentDictionary<Carrier, Crystallization> crystallizedSources = new();
    private static long nextCrystallizedId;

    public static Carrier Unbound => new(0);

    public static Carrier Primary => new(1);

    public static Carrier Crystallize(IElement source)
    {
        var lineage = FirstCarrierOf(source);
        var crystallization = SplitSource(source);
        var crystallized = new Carrier(Interlocked.Decrement(ref nextCrystallizedId), lineage.Id);
        crystallizedSources[crystallized] = crystallization;
        return crystallized;
    }

    public static bool TryGetSource(Carrier carrier, out IElement source)
    {
        if (crystallizedSources.TryGetValue(carrier, out var crystallization))
        {
            source = crystallization.Source;
            return true;
        }

        source = null!;
        return false;
    }

    public static bool TryGetVariableSource(Carrier carrier, out IElement variable)
    {
        if (crystallizedSources.TryGetValue(carrier, out var crystallization))
        {
            variable = crystallization.Variable;
            return true;
        }

        variable = null!;
        return false;
    }

    public static bool TryGetFrozenSource(Carrier carrier, out IElement frozen)
    {
        if (crystallizedSources.TryGetValue(carrier, out var crystallization) &&
            crystallization.Frozen is { } frozenSource)
        {
            frozen = frozenSource;
            return true;
        }

        frozen = null!;
        return false;
    }

    public static Carrier FirstCarrierOf(IElement element) => element switch
    {
        Measure measure => measure.Carrier,
        Relation relation => FirstCarrierOf(relation.Left, relation.Right),
        RelationFold fold => FirstCarrierOf(fold.Left, fold.Right),
        _ => Unbound
    };

    public override string ToString() => Domain == 0 ? Id.ToString() : $"{Domain}:{Id}";

    private static Carrier FirstCarrierOf(IElement left, IElement right)
    {
        var leftCarrier = FirstCarrierOf(left);
        if (leftCarrier != Unbound)
        {
            return leftCarrier;
        }

        return FirstCarrierOf(right);
    }

    private static Crystallization SplitSource(IElement source) => source switch
    {
        Relation relation => new(source, relation.Left, relation.Right),
        RelationFold fold => new(source, fold.Left, fold.Right),
        _ => new(source, source, null)
    };

    private sealed record Crystallization(IElement Source, IElement Variable, IElement? Frozen);
}
