using System.Collections.Concurrent;
using System.Threading;

namespace Core4.Experimental.Bootstrap;

// A bootstrap carrier is the current accessible floor of resolution.
// It may be primitive like distance, or already condensed like speed.
public readonly record struct Carrier(long Id, long Domain = 0)
{
    private static readonly ConcurrentDictionary<Carrier, IElement> crystallizedSources = new();
    private static long nextCrystallizedId;

    public static Carrier Unbound => new(0);

    public static Carrier Primary => new(1);

    public static Carrier Crystallize(IElement source)
    {
        var lineage = FirstCarrierOf(source);
        var crystallized = new Carrier(Interlocked.Decrement(ref nextCrystallizedId), lineage.Id);
        crystallizedSources[crystallized] = source;
        return crystallized;
    }

    public static bool TryGetSource(Carrier carrier, out IElement source) =>
        crystallizedSources.TryGetValue(carrier, out source!);

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
}
