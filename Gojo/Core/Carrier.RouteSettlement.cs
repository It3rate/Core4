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
    /// source boundary during this settlement pass. This is runtime scratch,
    /// not durable Gojo structure.
    /// </summary>
    private static bool ApplyRuntimeRouteSettlement(Carrier[] carriers)
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
                flow.Unit))
            .Where(group => group.Count() > 1);

        var changed = false;
        foreach (var group in flows)
        {
            changed |= ApplyRuntimeRouteGroup([.. group.OrderBy(flow => flow.Relation.Id)]);
        }

        return changed;
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
        if (flows.Length < 2)
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

        var baseShare = received / checked((ulong)flows.Length);
        var remainder = received % checked((ulong)flows.Length);
        for (var index = 0; index < flows.Length; index++)
        {
            var share = baseShare + (checked((ulong)index) < remainder ? 1UL : 0UL);
            if (share != 0)
            {
                flows[index].Target.Carrier.Receive(checked((long)share));
            }
        }

        changed |= source.KeepTensionRemainder(checked((long)(available % unitTicks)));
        return true;
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
