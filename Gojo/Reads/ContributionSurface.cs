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
    private readonly Dictionary<ulong, Carrier> carriers = [];
    private readonly Dictionary<ulong, ulong> freshValues = [];
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

        foreach (var group in CarrierLocationsOf(surface).GroupBy(location => location.Carrier.Id))
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
        freshValues.TryGetValue(carrier.Id, out var ticks) ? ticks : 0;

    public ulong TensionOn(Carrier carrier) => OverflowOn(carrier);

    public ulong OverflowOn(Carrier carrier) => StateOf(carrier).Overflow;

    public ulong UnderflowOn(Carrier carrier) => StateOf(carrier).Underflow;

    public ulong ScaleOn(Carrier carrier) => 1;

    public bool HasZeroScaleOn(Carrier carrier) => false;

    public bool HasSource(IElement source) => frameSources.Contains(source);

    public void AddTension(Carrier carrier, ulong ticks) =>
        AddOverflow(carrier, ticks);

    public void AddOverflow(Carrier carrier, ulong ticks)
    {
        if (ticks == 0)
        {
            EnsureCarrier(carrier);
            return;
        }

        StateOf(carrier).AddOverflow(ticks);
    }

    public void SetTension(Carrier carrier, ulong ticks) =>
        SetOverflow(carrier, ticks);

    public void SetOverflow(Carrier carrier, ulong ticks) =>
        StateOf(carrier).SetOverflow(ticks);

    public void AddUnderflow(Carrier carrier, ulong ticks)
    {
        if (ticks == 0)
        {
            EnsureCarrier(carrier);
            return;
        }

        StateOf(carrier).AddUnderflow(ticks);
    }

    public void SetUnderflow(Carrier carrier, ulong ticks) =>
        StateOf(carrier).SetUnderflow(ticks);

    public void RemoveValue(Carrier carrier, ulong ticks)
    {
        StateOf(carrier).Remove(ticks);
        freshValues.Remove(carrier.Id);
    }

    public void BalanceValues(Carrier first, Carrier second)
    {
        var firstState = StateOf(first);
        var secondState = StateOf(second);
        var shared = Math.Min(firstState.Value, secondState.Value);
        if (shared == 0)
        {
            return;
        }

        firstState.SetValue(firstState.Value - shared);
        secondState.SetValue(secondState.Value - shared);
        ReduceFresh(first, shared);
        ReduceFresh(second, shared);
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
        .Select(ElementKey)
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
                AddOverflow(tension.Location.Carrier, tension.Location.Ticks);
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
        .OrderBy(carrier => carrier.Id);

    private void AddCarrier(Carrier carrier, bool isFresh)
    {
        EnsureCarrier(carrier);
        if (isFresh)
        {
            AddFresh(carrier, carrier.Value);
        }
    }

    private void AddValue(Carrier carrier, ulong ticks, bool isFresh)
    {
        EnsureCarrier(carrier);
        if (ticks == 0)
        {
            return;
        }

        carrier.Add(ticks);
        if (isFresh)
        {
            AddFresh(carrier, ticks);
        }
    }

    private void SetValue(Carrier carrier, ulong ticks)
    {
        StateOf(carrier).SetValue(ticks);
        freshValues.Remove(carrier.Id);
    }

    private void AddFresh(Carrier carrier, ulong ticks)
    {
        if (ticks == 0)
        {
            return;
        }

        freshValues[carrier.Id] = checked((freshValues.TryGetValue(carrier.Id, out var existing) ? existing : 0) + ticks);
    }

    private void ReduceFresh(Carrier carrier, ulong ticks)
    {
        if (!freshValues.TryGetValue(carrier.Id, out var fresh))
        {
            return;
        }

        if (fresh <= ticks)
        {
            freshValues.Remove(carrier.Id);
            return;
        }

        freshValues[carrier.Id] = fresh - ticks;
    }

    private Carrier StateOf(Carrier carrier)
    {
        EnsureCarrier(carrier);
        return carriers[carrier.Id];
    }

    private void EnsureCarrier(Carrier carrier)
    {
        carriers.TryAdd(carrier.Id, carrier);
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

    private static Relation ContributionOf(Carrier carrier) =>
        new Contribution(
            Point(carrier, 0),
            new CarrierLocation(carrier, carrier.Value),
            new CarrierLocation(carrier, 1)).Surface;

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
        Carrier carrier => $"C({carrier.Id},{carrier.IsInverse},{carrier.Value},{carrier.Overflow},{carrier.Underflow})",
        CarrierLocation location => $"L({location.Carrier.Id},{location.Carrier.IsInverse},{location.Ticks})",
        Relation relation => $"R({ElementKey(relation.Left)}>{ElementKey(relation.Right)})",
        _ => element.GetType().FullName ?? string.Empty
    };
}
