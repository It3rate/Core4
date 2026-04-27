using Gojo.Core;

namespace Gojo.Reads;

public static class RouteSequenceReads
{
    public static Relation[] TransferThroughRoutes(this CarrierLocation source, Carrier reader, params Relation[] routes)
    {
        var remaining = source;
        var transfers = new List<Relation>();
        foreach (var route in routes)
        {
            if (!remaining.ReadFrom(reader).IsVisible)
            {
                break;
            }

            var transfer = remaining.TransferThrough(reader, route);
            transfers.Add(transfer);
            remaining = transfer.Left is Relation sourceRead
                ? sourceRead.RightLocation
                : new CarrierLocation(source.Carrier, 0);
        }

        return [.. transfers];
    }

    public static Relation[] TransferSourcesThrough(Carrier reader, Relation route, params CarrierLocation[] sources)
    {
        var currentRoute = route;
        var transfers = new List<Relation>();
        foreach (var source in sources)
        {
            var transfer = source.TransferThrough(reader, currentRoute);
            transfers.Add(transfer);
            currentRoute = new Relation(currentRoute.Left, transfer.Right);
        }

        return [.. transfers];
    }

    public static Relation[] ObserveThroughRoutes(this CarrierLocation source, Carrier reader, params Relation[] routes) =>
        routes
            .Select(route => source.ObserveThrough(reader, route))
            .ToArray();

    public static Relation[] ObserveSourcesThrough(Carrier reader, Relation route, params CarrierLocation[] sources)
    {
        var currentRoute = route;
        var observations = new List<Relation>();
        foreach (var source in sources)
        {
            var observation = source.ObserveThrough(reader, currentRoute);
            observations.Add(observation);
            currentRoute = new Relation(currentRoute.Left, observation.Right);
        }

        return [.. observations];
    }
}
