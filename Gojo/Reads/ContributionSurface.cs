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

    internal static ContributionSurface From(IElement surface)
    {
        var positions = new ContributionSurface();
        positions.AddSurface(surface, isFresh: false);
        return positions;
    }

    internal void AddIncoming(IElement surface) =>
        AddSurface(surface, isFresh: true);

    internal ulong ValueOn(Carrier carrier) => StateOf(carrier).Value;

    internal ulong FreshValueOn(Carrier carrier) =>
        freshValues.TryGetValue(carrier.Id, out var ticks) ? ticks : 0;

    internal ulong OverflowOn(Carrier carrier) => StateOf(carrier).Overflow;

    internal ulong UnderflowOn(Carrier carrier) => StateOf(carrier).Underflow;

    internal void AccountOverflow(Carrier carrier, ulong value, ulong overflow)
    {
        var state = StateOf(carrier);
        state.SetValue(value);
        state.AddOverflow(overflow);
        freshValues.Remove(carrier.Id);
    }

    internal void KeepOverflowRemainder(Carrier carrier, ulong ticks) =>
        StateOf(carrier).SetOverflow(ticks);

    internal void KeepUnderflowRemainder(Carrier carrier, ulong ticks) =>
        StateOf(carrier).SetUnderflow(ticks);

    internal void Receive(IElement source, CarrierLocation location)
    {
        _ = source;
        AddValue(location.Carrier, location.Ticks, isFresh: true);
    }

    internal void BalanceOpposedValues(Carrier first, Carrier second)
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

    internal Relation[] Relations() => [.. OrderedCarriers()
        .Where(carrier => carrier.Value != 0)
        .Select(ContributionOf)];

    internal Relation[] RelationsOn(Carrier carrier) => [.. OrderedCarriers()
        .Where(state => state.SameIdentityAs(carrier) && state.Value != 0)
        .Select(ContributionOf)];

    internal IElement Surface() => BuildSurface([.. OrderedCarriers()
        .Where(carrier => carrier.HasState)
        .Select(carrier => (IElement)carrier)]);

    internal string[] Snapshot() => [.. OrderedCarriers()
        .Where(carrier => carrier.HasState)
        .Select(ElementKey)
        .Order()];

    internal bool SameAs(string[] snapshot) =>
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
                StateOf(tension.Location.Carrier).AddOverflow(tension.Location.Ticks);
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
