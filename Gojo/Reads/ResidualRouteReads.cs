using Gojo.Core;

namespace Gojo.Reads;

public static class ResidualRouteReads
{
    public static Relation[] TransferResidualsThrough(this Relation result, Carrier reader, Relation route) =>
        RouteSequenceReads.TransferSourcesThrough(reader, route, result.ResidualsOn(reader));

    public static Relation[] ObserveResidualsThrough(this Relation result, Carrier reader, Relation route) =>
        RouteSequenceReads.ObserveSourcesThrough(reader, route, result.ResidualsOn(reader));

    public static Relation[] GatherResidualsThrough(this Relation result, Carrier reader, Relation route) =>
        result.ResidualsOn(reader)
            .Select(residual => residual.GatherThrough(reader, route))
            .ToArray();
}
