using Gojo.Core;

namespace Gojo.Reads;

internal readonly record struct Contribution(IElement Source, CarrierLocation Location, CarrierLocation Scale)
{
    public Relation Surface => new(Source, new Relation(Location, Scale));

    public static bool TryRead(Relation relation, out Contribution contribution)
    {
        if (relation.Right is Relation state &&
            state.Left is CarrierLocation location &&
            state.Right is CarrierLocation scale &&
            scale.Carrier.SameIdentityAs(location.Carrier))
        {
            contribution = new(relation.Left, location, scale);
            return true;
        }

        contribution = default;
        return false;
    }
}

internal readonly record struct CarrierTension(CarrierLocation Location)
{
    public Relation Surface => new(Point(Location.Carrier, 0), Location);

    public static bool TryRead(Relation relation, out CarrierTension tension)
    {
        if (relation.Right is CarrierLocation location &&
            TryGetPoint(relation.Left, out var point) &&
            point.Carrier.SameIdentityAs(location.Carrier) &&
            point.Ticks == 0)
        {
            tension = new(location);
            return true;
        }

        tension = default;
        return false;
    }

    private static Relation Point(Carrier carrier, ulong ticks) =>
        new(new CarrierLocation(carrier, ticks), carrier);

    private static bool TryGetPoint(IElement element, out CarrierLocation point)
    {
        if (element is Relation relation &&
            relation.Left is CarrierLocation location &&
            relation.Right is Carrier carrier &&
            location.Carrier.SameIdentityAs(carrier))
        {
            point = location;
            return true;
        }

        point = default;
        return false;
    }
}

internal sealed class ContributionSurface
{
    private readonly Dictionary<CarrierKey, Carrier> carriers = [];
    private readonly Dictionary<CarrierKey, ulong> freshValues = [];
    private readonly HashSet<IElement> frameSources = [];

    public static ContributionSurface From(IElement surface)
    {
        var positions = new ContributionSurface();
        positions.AddSurface(surface);
        return positions;
    }

    public void AddSurface(IElement surface) =>
        AddSurface(surface, isFresh: false);

    public void AddIncoming(IElement surface) =>
        AddSurface(surface, isFresh: true);

    public void AddSurface(IElement source, IElement surface)
    {
        frameSources.Add(source);
        foreach (var carrier in CarriersOf(surface))
        {
            AddCarrier(carrier, isFresh: true);
        }

        foreach (var group in CarrierLocationsOf(surface).GroupBy(location => CarrierKey.From(location.Carrier)))
        {
            var carrier = group.First().Carrier;
            AddValue(
                carrier,
                group.Aggregate(0UL, (total, location) => checked(total + location.Ticks)),
                isFresh: true);
        }
    }

    public void SetSurface(IElement source, IElement surface)
    {
        frameSources.Add(source);
        AddSurface(source, surface);
    }

    public void Add(Contribution contribution) =>
        Add(contribution.Source, contribution.Location, contribution.Scale);

    public void Add(IElement source, CarrierLocation location)
    {
        Add(source, location, new CarrierLocation(location.Carrier, 1));
    }

    public void Add(IElement source, CarrierLocation location, CarrierLocation scale)
    {
        frameSources.Add(source);
        AddValue(location.Carrier, location.Ticks, isFresh: true);
    }

    public void Set(IElement source, Carrier carrier, ulong ticks, ulong scale = 1)
    {
        frameSources.Add(source);
        AddValue(carrier, ticks, isFresh: true);
    }

    public void ReplaceCarrier(Carrier carrier, IElement source, ulong ticks, ulong scale = 1)
    {
        frameSources.Add(source);
        SetValue(carrier, ticks);
    }

    public ulong TotalOn(Carrier carrier) => StateOf(carrier).Value;

    public ulong TotalOn(Carrier carrier, IElement except) => TotalOn(carrier);

    public ulong FreshOn(Carrier carrier) =>
        freshValues.TryGetValue(CarrierKey.From(carrier), out var ticks) ? ticks : 0;

    public ulong TensionOn(Carrier carrier) => StateOf(carrier).Tension;

    public ulong ScaleOn(Carrier carrier) => 1;

    public bool HasZeroScaleOn(Carrier carrier) => false;

    public bool HasSource(IElement source) => frameSources.Contains(source);

    public void AddTension(Carrier carrier, ulong ticks)
    {
        if (ticks == 0)
        {
            return;
        }

        var state = StateOf(carrier);
        SetCarrier(state.WithTension(checked(state.Tension + ticks)));
    }

    public void SetTension(Carrier carrier, ulong ticks)
    {
        var state = StateOf(carrier);
        SetCarrier(state.WithTension(ticks));
    }

    public Relation[] Relations() => [.. OrderedCarriers()
        .Where(carrier => carrier.Value != 0)
        .Select(ContributionOf)];

    public Relation[] RelationsOn(Carrier carrier) => [.. OrderedCarriers()
        .Where(state => state.SameIdentityAs(carrier) && state.Value != 0)
        .Select(ContributionOf)];

    public CarrierLocation[] ScalesOn(Carrier carrier) =>
        TotalOn(carrier) == 0 ? [] : [new CarrierLocation(carrier, 1)];

    public IElement Surface() => BuildSurface([.. OrderedCarriers()
        .Where(carrier => carrier.HasState)
        .Select(carrier => (IElement)carrier)]);

    public string[] Snapshot() => [.. OrderedCarriers()
        .Where(carrier => carrier.HasState)
        .Select(carrier => ElementKey(carrier))
        .Order()];

    public bool SameAs(string[] snapshot) =>
        Snapshot().SequenceEqual(snapshot);

    private void AddSurface(IElement surface, bool isFresh)
    {
        Visit(surface, allowBareLocation: true);

        void Visit(IElement element, bool allowBareLocation)
        {
            switch (element)
            {
                case Carrier carrier:
                    AddCarrier(carrier, isFresh);
                    return;
                case CarrierLocation location when allowBareLocation:
                    AddValue(location.Carrier, location.Ticks, isFresh);
                    return;
                case not Relation:
                    return;
            }

            var relation = (Relation)element;
            if (Contribution.TryRead(relation, out var contribution))
            {
                AddValue(contribution.Location.Carrier, contribution.Location.Ticks, isFresh);
                return;
            }

            if (CarrierTension.TryRead(relation, out var tension))
            {
                var state = StateOf(tension.Location.Carrier);
                SetCarrier(state.WithTension(checked(state.Tension + tension.Location.Ticks)));
                return;
            }

            if (TryGetPoint(relation, out var point))
            {
                AddValue(point.Carrier, point.Ticks, isFresh);
                return;
            }

            Visit(relation.Left, allowBareLocation: false);
            Visit(relation.Right, allowBareLocation: false);
        }
    }

    private IEnumerable<Carrier> OrderedCarriers() => carriers.Values
        .OrderBy(carrier => carrier.Lineage)
        .ThenBy(carrier => carrier.Id)
        .ThenBy(carrier => carrier.Flow);

    private void AddCarrier(Carrier carrier, bool isFresh)
    {
        var state = StateOf(carrier);
        SetCarrier(state.WithState(
            checked(state.Value + carrier.Value),
            checked(state.Tension + carrier.Tension)));
        if (isFresh)
        {
            AddFresh(carrier, carrier.Value);
        }
    }

    private void AddValue(Carrier carrier, ulong ticks, bool isFresh)
    {
        if (ticks == 0)
        {
            EnsureCarrier(carrier);
            return;
        }

        var state = StateOf(carrier);
        SetCarrier(state.WithValue(checked(state.Value + ticks)));
        if (isFresh)
        {
            AddFresh(carrier, ticks);
        }
    }

    private void SetValue(Carrier carrier, ulong ticks)
    {
        var state = StateOf(carrier);
        SetCarrier(state.WithValue(ticks));
        freshValues.Remove(CarrierKey.From(carrier));
    }

    private void AddFresh(Carrier carrier, ulong ticks)
    {
        if (ticks == 0)
        {
            return;
        }

        var key = CarrierKey.From(carrier);
        freshValues[key] = checked((freshValues.TryGetValue(key, out var existing) ? existing : 0) + ticks);
    }

    private Carrier StateOf(Carrier carrier)
    {
        var key = CarrierKey.From(carrier);
        return carriers.TryGetValue(key, out var state)
            ? state
            : carrier.WithState(0, 0);
    }

    private void EnsureCarrier(Carrier carrier)
    {
        var key = CarrierKey.From(carrier);
        carriers.TryAdd(key, carrier.WithState(0, 0));
    }

    private void SetCarrier(Carrier carrier)
    {
        carriers[CarrierKey.From(carrier)] = carrier;
    }

    private static IElement BuildSurface(IElement[] elements)
    {
        return elements.Length switch
        {
            0 => new Relation(new CarrierLocation(Carrier.Unknown, 0), Carrier.Unknown),
            1 => elements[0],
            _ => new Relation(elements[0], BuildSurface(elements[1..]))
        };
    }

    private static Relation Point(Carrier carrier, ulong ticks) =>
        new(new CarrierLocation(carrier, ticks), carrier);

    private static Relation ContributionOf(Carrier carrier)
    {
        var basis = carrier.WithState(0, 0);
        return new Contribution(
            Point(basis, 0),
            new CarrierLocation(basis, carrier.Value),
            new CarrierLocation(basis, 1)).Surface;
    }

    private static Carrier[] CarriersOf(IElement element) => element switch
    {
        Carrier carrier => [carrier],
        Relation relation => [.. CarriersOf(relation.Left), .. CarriersOf(relation.Right)],
        _ => []
    };

    private static CarrierLocation[] CarrierLocationsOf(IElement element) => element switch
    {
        CarrierLocation location => [location],
        Relation relation => relation.CarrierLocations(),
        _ => []
    };

    private static bool TryGetPoint(Relation relation, out CarrierLocation point)
    {
        if (relation.Left is CarrierLocation location &&
            relation.Right is Carrier carrier &&
            location.Carrier.SameIdentityAs(carrier))
        {
            point = location;
            return true;
        }

        point = default;
        return false;
    }

    private static string ElementKey(IElement element) => element switch
    {
        Carrier carrier => $"C({carrier.Lineage},{carrier.Id},{carrier.Flow},{carrier.Value},{carrier.Tension})",
        CarrierLocation location => $"L({location.Carrier.Lineage},{location.Carrier.Id},{location.Carrier.Flow},{location.Ticks})",
        Relation relation => $"R({ElementKey(relation.Left)}>{ElementKey(relation.Right)})",
        _ => element.GetType().FullName ?? string.Empty
    };

    private readonly record struct CarrierKey(ulong Id, ulong Lineage, long Flow)
    {
        public static CarrierKey From(Carrier carrier) =>
            new(carrier.Id, carrier.Lineage, carrier.Flow == 0 ? 1 : carrier.Flow);
    }
}
