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

        var occurrences = new List<ProjectionOccurrence>();
        CollectProjectionOccurrences(input, occurrences);
        var expandedSurface = ExpandSurface(surface);

        if (!MatchesSurface(expandedSurface, occurrences))
        {
            projected = null!;
            return false;
        }

        var value = 1d;
        for (var i = 0; i < expandedSurface.Length; i++)
        {
            var entry = expandedSurface[i];
            var occurrence = occurrences[i];
            if (entry.SignedRepeatCount > 0)
            {
                value *= occurrence.Value;
            }
            else if (occurrence.Value != 0)
            {
                value /= occurrence.Value;
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
        var hasExplicitSurface = surface.Length != 0;
        var totalSurface = hasExplicitSurface ? surface : BuildTickSurface(source);
        return new(
            source,
            variable,
            frozen,
            totalSurface,
            BuildSignedTickSurface(variable, totalSurface, hasExplicitSurface),
            frozen is null ? Array.Empty<TickSurfaceEntry>() : BuildSignedTickSurface(frozen, totalSurface, hasExplicitSurface));
    }

    private static TickSurfaceEntry[] BuildTickSurface(IElement source)
    {
        var surface = new List<TickSurfaceEntry>();
        CollectTickSurface(source, surface);
        return surface.ToArray();
    }

    private static TickSurfaceEntry[] BuildSignedTickSurface(IElement source, TickSurfaceEntry[] totalSurface, bool compressRepeats)
    {
        var localSurface = BuildTickSurface(source);
        var expandedSurface = ExpandSurface(totalSurface);
        var used = new bool[expandedSurface.Length];
        for (var i = 0; i < localSurface.Length; i++)
        {
            localSurface[i] = localSurface[i] with
            {
                SignedRepeatCount = ResolveSignedRepeatCount(localSurface[i].Carrier, used, expandedSurface)
            };
        }

        return compressRepeats ? CompressExactRepeats(localSurface) : localSurface;
    }

    private static void CollectTickSurface(IElement element, List<TickSurfaceEntry> surface)
    {
        switch (element)
        {
            case Measure measure when measure.Carrier != Unbound:
                surface.Add(new(measure.Carrier));
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

    private static long ResolveSignedRepeatCount(Carrier carrier, bool[] used, TickSurfaceEntry[] expandedSurface)
    {
        for (var i = 0; i < expandedSurface.Length; i++)
        {
            if (!used[i] && expandedSurface[i].Carrier == carrier)
            {
                used[i] = true;
                return expandedSurface[i].SignedRepeatCount < 0 ? -1 : 1;
            }
        }

        return 1;
    }

    private static TickSurfaceEntry[] CompressExactRepeats(TickSurfaceEntry[] surface)
    {
        if (surface.Length == 0)
        {
            return surface;
        }

        var compressed = new List<TickSurfaceEntry> { surface[0] };
        for (var i = 1; i < surface.Length; i++)
        {
            var current = surface[i];
            var lastIndex = compressed.Count - 1;
            var last = compressed[lastIndex];
            if (last.Carrier == current.Carrier &&
                Math.Sign(last.SignedRepeatCount) == Math.Sign(current.SignedRepeatCount))
            {
                compressed[lastIndex] = last with
                {
                    SignedRepeatCount = last.SignedRepeatCount + current.SignedRepeatCount
                };
            }
            else
            {
                compressed.Add(current);
            }
        }

        return compressed.ToArray();
    }

    private static TickSurfaceEntry[] ExpandSurface(TickSurfaceEntry[] surface)
    {
        var expanded = new List<TickSurfaceEntry>();
        foreach (var entry in surface)
        {
            var sign = entry.SignedRepeatCount < 0 ? -1L : 1L;
            for (var i = 0L; i < entry.RepeatCount; i++)
            {
                expanded.Add(new(entry.Carrier, sign));
            }
        }

        return expanded.ToArray();
    }

    private static void CollectProjectionOccurrences(IElement element, List<ProjectionOccurrence> occurrences)
    {
        switch (element)
        {
            case Measure measure when measure.Carrier != Unbound:
                occurrences.Add(new(measure.Carrier, measure.Raw));
                break;

            case Relation relation:
                CollectProjectionOccurrences(relation.Left, occurrences);
                CollectProjectionOccurrences(relation.Right, occurrences);
                break;

            case RelationFold fold:
                CollectProjectionOccurrences(fold.Left, occurrences);
                CollectProjectionOccurrences(fold.Right, occurrences);
                break;
        }
    }

    private static bool MatchesSurface(TickSurfaceEntry[] surface, List<ProjectionOccurrence> occurrences)
    {
        if (surface.Length != occurrences.Count)
        {
            return false;
        }

        for (var i = 0; i < surface.Length; i++)
        {
            if (surface[i].Carrier != occurrences[i].Carrier)
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

    private readonly record struct ProjectionOccurrence(Carrier Carrier, long Value);
}
