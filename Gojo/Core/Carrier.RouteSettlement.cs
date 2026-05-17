namespace Gojo.Core;

public sealed partial class Carrier
{
    #region Runtime Route Settlement

    private readonly record struct RuntimeRouteSites(Carrier Carrier, long Min, long Max)
    {
        public long Width => Max - Min;

        public bool IsOrigin => Width == 0 && Min == 0;
    }

    private readonly record struct RuntimeRouteGroupKey(
        Carrier Source,
        long SourceBoundary,
        long LocalTarget,
        long Unit);

    private readonly record struct RuntimeRouteFlow(
        Relation Relation,
        RuntimeRouteSites Source,
        RuntimeRouteSites Target,
        long SourceBoundary,
        long LocalTarget,
        long Unit);

    /// <summary>
    /// Coordinates direct positive tension routes that compete for the same
    /// source boundary during this settlement pass. Target absorption is read
    /// from existing local clamp structure; all route records here are runtime
    /// scratch, not durable Gojo structure.
    /// </summary>
    private static HashSet<ulong> ApplyRuntimeRouteSettlement(Carrier[] carriers)
    {
        var flows = carriers
            .SelectMany(carrier => carrier.relations)
            .GroupBy(relation => relation.Id)
            .Select(group => group.First())
            .SelectMany(relation =>
                TryReadPositiveRuntimeRouteFlow(relation, out var flow)
                    ? [flow]
                    : Array.Empty<RuntimeRouteFlow>())
            .GroupBy(flow => new RuntimeRouteGroupKey(
                flow.Source.Carrier,
                flow.SourceBoundary,
                flow.LocalTarget,
                flow.Unit));

        var handled = new HashSet<ulong>();
        foreach (var group in flows)
        {
            var groupFlows = group.OrderBy(flow => flow.Relation.Id).ToArray();
            if (groupFlows.Length < 2 && !HasFinitePositiveAbsorption(groupFlows[0].Target.Carrier))
            {
                continue;
            }

            ApplyRuntimeRouteGroup(groupFlows);
            foreach (var flow in groupFlows)
            {
                handled.Add(flow.Relation.Id);
            }
        }

        return handled;
    }

    /// <summary>Reads a direct positive route candidate from one relation.</summary>
    private static bool TryReadPositiveRuntimeRouteFlow(Relation relation, out RuntimeRouteFlow flow)
    {
        if (!TryReadRuntimeRouteSites(relation.Left, out var source) ||
            !TryReadRuntimeRouteSites(relation.Right, out var target) ||
            source.Carrier.SameIdentityAs(target.Carrier) ||
            source.Carrier.IsUnknown ||
            target.Carrier.IsUnknown ||
            source.Carrier.IsInverse != target.Carrier.IsInverse ||
            !target.IsOrigin)
        {
            flow = default;
            return false;
        }

        var unit = source.Width == 0 ? source.Max : source.Width;
        var localTarget = source.Width == 0 ? 0 : source.Min;
        if (unit <= 0 ||
            localTarget < 0 ||
            source.Max < localTarget)
        {
            flow = default;
            return false;
        }

        flow = new RuntimeRouteFlow(
            relation,
            source,
            target,
            source.Max,
            localTarget,
            unit);
        return true;
    }

    /// <summary>Allocates one source tension pool across equivalent direct routes.</summary>
    private static bool ApplyRuntimeRouteGroup(RuntimeRouteFlow[] flows)
    {
        if (flows.Length == 0)
        {
            return false;
        }

        var source = flows[0].Source.Carrier;
        var sourceBoundary = flows[0].SourceBoundary;
        var localTarget = flows[0].LocalTarget;
        var unit = flows[0].Unit;
        var changed = AccountRuntimeLocal(source, sourceBoundary, localTarget);
        if (source.Tension <= 0)
        {
            return changed;
        }

        var available = checked((ulong)source.Tension);
        var unitTicks = checked((ulong)unit);
        var received = available / unitTicks;
        if (received == 0)
        {
            return changed;
        }

        var allocations = AllocateRuntimeRouteTicks(flows, received);
        var allocated = allocations.Aggregate(0UL, (sum, ticks) => checked(sum + ticks));
        for (var index = 0; index < flows.Length; index++)
        {
            if (allocations[index] != 0)
            {
                flows[index].Target.Carrier.Receive(checked((long)allocations[index]));
            }
        }

        var rejected = checked(received - allocated);
        var remaining = checked((rejected * unitTicks) + (available % unitTicks));
        changed |= source.KeepTensionRemainder(checked((long)remaining));
        return true;
    }

    /// <summary>Splits carried ticks across targets while respecting local target clamps.</summary>
    private static ulong[] AllocateRuntimeRouteTicks(RuntimeRouteFlow[] flows, ulong available)
    {
        var allocations = new ulong[flows.Length];
        if (available == 0)
        {
            return allocations;
        }

        var remaining = available;
        var active = Enumerable.Range(0, flows.Length).ToList();
        while (remaining != 0 && active.Count != 0)
        {
            var count = checked((ulong)active.Count);
            var baseShare = remaining / count;
            var remainder = remaining % count;
            var moved = 0UL;

            for (var order = 0; order < active.Count; order++)
            {
                var index = active[order];
                var requested = baseShare + (checked((ulong)order) < remainder ? 1UL : 0UL);
                if (requested == 0)
                {
                    continue;
                }

                var alreadyAllocatedToTarget = AllocatedRuntimeRouteTicksTo(
                    flows,
                    allocations,
                    flows[index].Target.Carrier);
                var allowed = RuntimePositiveAbsorptionLeft(
                    flows[index].Target.Carrier,
                    alreadyAllocatedToTarget);
                var accepted = allowed.HasValue
                    ? Math.Min(requested, allowed.Value)
                    : requested;

                allocations[index] = checked(allocations[index] + accepted);
                moved = checked(moved + accepted);
            }

            if (moved == 0)
            {
                break;
            }

            remaining = checked(remaining - moved);
            active.RemoveAll(index =>
                RuntimePositiveAbsorptionLeft(
                    flows[index].Target.Carrier,
                    AllocatedRuntimeRouteTicksTo(flows, allocations, flows[index].Target.Carrier)) == 0);
        }

        return allocations;
    }

    /// <summary>Counts ticks already allocated to one target carrier in this route group.</summary>
    private static ulong AllocatedRuntimeRouteTicksTo(
        RuntimeRouteFlow[] flows,
        ulong[] allocations,
        Carrier target)
    {
        var allocated = 0UL;
        for (var index = 0; index < flows.Length; index++)
        {
            if (flows[index].Target.Carrier.SameIdentityAs(target))
            {
                allocated = checked(allocated + allocations[index]);
            }
        }

        return allocated;
    }

    /// <summary>Returns true when target relation structure gives the target a finite positive intake.</summary>
    private static bool HasFinitePositiveAbsorption(Carrier carrier) =>
        RuntimePositiveAbsorptionLeft(carrier, alreadyAllocated: 0).HasValue;

    /// <summary>
    /// Reads remaining positive ticks a target can absorb before its own local
    /// clamp would leave positive tension. Null means the current structure
    /// does not express a finite intake limit.
    /// </summary>
    private static ulong? RuntimePositiveAbsorptionLeft(Carrier carrier, ulong alreadyAllocated)
    {
        var limit = RuntimePositiveClampLimit(carrier);
        if (!limit.HasValue)
        {
            return null;
        }

        var capacity = RuntimePositiveClampCapacity(carrier, limit.Value);
        return alreadyAllocated >= capacity
            ? 0UL
            : capacity - alreadyAllocated;
    }

    /// <summary>Reads the nearest local positive clamp point on this carrier.</summary>
    private static ulong? RuntimePositiveClampLimit(Carrier carrier)
    {
        ulong? limit = null;
        foreach (var relation in carrier.relations
            .GroupBy(relation => relation.Id)
            .Select(group => group.First()))
        {
            if (!TryReadRuntimeRouteSites(relation.Left, out var left) ||
                !TryReadRuntimeRouteSites(relation.Right, out var right) ||
                !left.Carrier.SameIdentityAs(carrier) ||
                !right.Carrier.SameIdentityAs(carrier) ||
                left.Width != 0 ||
                right.Width != 0 ||
                left.Max != right.Max ||
                left.Max < 0)
            {
                continue;
            }

            var ticks = checked((ulong)left.Max);
            limit = limit.HasValue
                ? Math.Min(limit.Value, ticks)
                : ticks;
        }

        return limit;
    }

    /// <summary>Computes clamp intake without mutating the target carrier.</summary>
    private static ulong RuntimePositiveClampCapacity(Carrier carrier, ulong limit)
    {
        var tensionDebt = carrier.Tension < 0
            ? checked((ulong)-carrier.Tension)
            : 0UL;

        if (carrier.Tension > 0 || carrier.Value >= limit)
        {
            return tensionDebt;
        }

        return checked(tensionDebt + (limit - carrier.Value));
    }

    /// <summary>Applies same-carrier local accounting without choosing a route target.</summary>
    private static bool AccountRuntimeLocal(Carrier carrier, long source, long target)
    {
        if (source <= 0 || target < 0 || source < target)
        {
            return false;
        }

        var sourceTicks = checked((ulong)source);
        var targetTicks = checked((ulong)target);
        if (carrier.Value <= targetTicks)
        {
            return false;
        }

        var local = sourceTicks == targetTicks
            ? Math.Min(carrier.Value, sourceTicks)
            : checked(targetTicks + ((carrier.Value - targetTicks) % (sourceTicks - targetTicks)));
        return local != carrier.Value &&
            carrier.LeaveTension(local, checked((long)(carrier.Value - local)));
    }

    /// <summary>Reads all sites in an element only when they share one carrier.</summary>
    private static bool TryReadRuntimeRouteSites(IElement element, out RuntimeRouteSites sites)
    {
        var locations = RuntimeRouteLocationsOf(element);
        if (locations.Length == 0)
        {
            sites = default;
            return false;
        }

        var carrier = locations[0].Carrier;
        if (locations.Any(location => !location.Carrier.SameIdentityAs(carrier)))
        {
            sites = default;
            return false;
        }

        sites = new RuntimeRouteSites(
            carrier,
            locations.Min(location => location.Ticks),
            locations.Max(location => location.Ticks));
        return true;
    }

    /// <summary>Collects carrier locations from an element tree for route discovery.</summary>
    private static CarrierLocation[] RuntimeRouteLocationsOf(IElement element) => element switch
    {
        CarrierLocation location => [location],
        Relation relation => [.. RuntimeRouteLocationsOf(relation.Left), .. RuntimeRouteLocationsOf(relation.Right)],
        _ => []
    };

    #endregion
}
