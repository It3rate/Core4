using System.Collections.Concurrent;
using System.Threading;

namespace Core4.Experimental.Bootstrap;

public readonly record struct TickSurfaceEntry(Carrier Carrier, long SignedRepeatCount = 1)
{
    public long RepeatCount => Math.Abs(SignedRepeatCount);

    // TODO: Revisit repeat semantics before surface interpretation hardens.
    // Today sign is being used for expansive/contractive participation. Later we
    // may also need explicit inverse-repeat or root-like surface reads without
    // collapsing those meanings together accidentally.
}

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

    public static Carrier Crystallize(IElement source, params TickSurfaceEntry[] surface)
    {
        var lineage = FirstCarrierOf(source);
        var crystallization = SplitSource(source, surface);
        var crystallized = new Carrier(Interlocked.Decrement(ref nextCrystallizedId), lineage.Id);
        crystallizedSources[crystallized] = crystallization;
        return crystallized;
    }

    public static Carrier Crystallize(IElement source, IElement variable, IElement? frozen, params TickSurfaceEntry[] surface)
    {
        var lineage = FirstCarrierOf(source);
        var crystallization = BuildCrystallization(source, variable, frozen, surface);
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

    public static bool TryGetTickSurface(Carrier carrier, out TickSurfaceEntry[] surface)
    {
        if (crystallizedSources.TryGetValue(carrier, out var crystallization))
        {
            surface = crystallization.Surface;
            return true;
        }

        surface = Array.Empty<TickSurfaceEntry>();
        return false;
    }

    public static bool TryGetVariableTickSurface(Carrier carrier, out TickSurfaceEntry[] surface)
    {
        if (crystallizedSources.TryGetValue(carrier, out var crystallization))
        {
            surface = crystallization.VariableSurface;
            return true;
        }

        surface = Array.Empty<TickSurfaceEntry>();
        return false;
    }

    public static bool TryGetFrozenTickSurface(Carrier carrier, out TickSurfaceEntry[] surface)
    {
        if (crystallizedSources.TryGetValue(carrier, out var crystallization))
        {
            surface = crystallization.FrozenSurface;
            return true;
        }

        surface = Array.Empty<TickSurfaceEntry>();
        return false;
    }

    public static bool TryProject(Carrier carrier, IElement input, out Measure projected)
    {
        if (!TryGetTickSurface(carrier, out var surface))
        {
            projected = null!;
            return false;
        }

        var factors = new Dictionary<Carrier, ProjectionFactor>();
        CollectProjectionFactors(input, factors);

        if (!MatchesSurface(surface, factors))
        {
            projected = null!;
            return false;
        }

        var value = 1d;
        foreach (var entry in surface)
        {
            var factor = factors[entry.Carrier];
            if (entry.SignedRepeatCount > 0)
            {
                value *= factor.Product;
            }
            else if (factor.Product != 0)
            {
                value /= factor.Product;
            }
            else
            {
                projected = null!;
                return false;
            }
        }

        projected = Measure.Fixed(ToLong(value), carrier);
        return true;
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

    private static Crystallization SplitSource(IElement source) =>
        SplitSource(source, BuildTickSurface(source));

    private static Crystallization SplitSource(IElement source, TickSurfaceEntry[] surface)
    {
        return source switch
        {
            Relation relation => BuildCrystallization(source, relation.Left, relation.Right, surface),
            RelationFold fold => BuildCrystallization(source, fold.Left, fold.Right, surface),
            _ => BuildCrystallization(source, source, null, surface)
        };
    }

    private static Crystallization BuildCrystallization(IElement source, IElement variable, IElement? frozen, TickSurfaceEntry[] surface)
    {
        var totalSurface = surface.Length == 0 ? BuildTickSurface(source) : surface;
        return new(
            source,
            variable,
            frozen,
            totalSurface,
            BuildSignedTickSurface(variable, totalSurface),
            frozen is null ? Array.Empty<TickSurfaceEntry>() : BuildSignedTickSurface(frozen, totalSurface));
    }

    private static TickSurfaceEntry[] BuildTickSurface(IElement source)
    {
        var surface = new List<TickSurfaceEntry>();
        CollectTickSurface(source, surface);
        return surface.ToArray();
    }

    private static TickSurfaceEntry[] BuildSignedTickSurface(IElement source, TickSurfaceEntry[] totalSurface)
    {
        var localSurface = BuildTickSurface(source);
        for (var i = 0; i < localSurface.Length; i++)
        {
            localSurface[i] = localSurface[i] with
            {
                SignedRepeatCount = ResolveSignedRepeatCount(localSurface[i].Carrier, localSurface[i].RepeatCount, totalSurface)
            };
        }

        return localSurface;
    }

    private static void CollectTickSurface(IElement element, List<TickSurfaceEntry> surface)
    {
        switch (element)
        {
            case Measure measure when measure.Carrier != Unbound:
                AddTickSurfaceEntry(surface, measure.Carrier);
                break;

            case Relation relation:
                CollectTickSurface(relation.Left, surface);
                CollectTickSurface(relation.Right, surface);
                break;

            case RelationFold fold:
                CollectTickSurface(fold.Left, surface);
                CollectTickSurface(fold.Right, surface);
                break;
        }
    }

    private static void AddTickSurfaceEntry(List<TickSurfaceEntry> surface, Carrier carrier)
    {
        for (var i = 0; i < surface.Count; i++)
        {
            if (surface[i].Carrier == carrier)
            {
                surface[i] = surface[i] with { SignedRepeatCount = surface[i].SignedRepeatCount + 1 };
                return;
            }
        }

        surface.Add(new(carrier));
    }

    private static long ResolveSignedRepeatCount(Carrier carrier, long repeatCount, TickSurfaceEntry[] totalSurface)
    {
        foreach (var entry in totalSurface)
        {
            if (entry.Carrier == carrier)
            {
                return entry.SignedRepeatCount < 0 ? -repeatCount : repeatCount;
            }
        }

        return repeatCount;
    }

    private static void CollectProjectionFactors(IElement element, Dictionary<Carrier, ProjectionFactor> factors)
    {
        switch (element)
        {
            case Measure measure when measure.Carrier != Unbound:
                AddProjectionFactor(factors, measure.Carrier, measure.Raw);
                break;

            case Relation relation:
                CollectProjectionFactors(relation.Left, factors);
                CollectProjectionFactors(relation.Right, factors);
                break;

            case RelationFold fold:
                CollectProjectionFactors(fold.Left, factors);
                CollectProjectionFactors(fold.Right, factors);
                break;
        }
    }

    private static void AddProjectionFactor(Dictionary<Carrier, ProjectionFactor> factors, Carrier carrier, long value)
    {
        if (factors.TryGetValue(carrier, out var factor))
        {
            factors[carrier] = factor with
            {
                Count = factor.Count + 1,
                Product = factor.Product * value
            };
            return;
        }

        factors[carrier] = new(1, value);
    }

    private static bool MatchesSurface(TickSurfaceEntry[] surface, Dictionary<Carrier, ProjectionFactor> factors)
    {
        if (surface.Length != factors.Count)
        {
            return false;
        }

        foreach (var entry in surface)
        {
            if (!factors.TryGetValue(entry.Carrier, out var factor) ||
                factor.Count != entry.RepeatCount)
            {
                return false;
            }
        }

        return true;
    }

    private static long ToLong(double value) => (long)Math.Round(value, MidpointRounding.AwayFromZero);

    private sealed record Crystallization(
        IElement Source,
        IElement Variable,
        IElement? Frozen,
        TickSurfaceEntry[] Surface,
        TickSurfaceEntry[] VariableSurface,
        TickSurfaceEntry[] FrozenSurface);

    private readonly record struct ProjectionFactor(int Count, double Product);
}
