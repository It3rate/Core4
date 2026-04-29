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
            scale.Carrier == location.Carrier)
        {
            contribution = new(relation.Left, location, scale);
            return true;
        }

        contribution = default;
        return false;
    }
}

internal sealed class ContributionSurface
{
    private readonly List<Contribution> contributions = [];

    public static ContributionSurface From(IElement surface)
    {
        var positions = new ContributionSurface();
        positions.AddSurface(surface);
        return positions;
    }

    public void AddSurface(IElement surface)
    {
        Visit(surface, allowBareLocation: true);

        void AddLocation(CarrierLocation location)
        {
            Add(new Relation(Point(location.Carrier, 0), location), location);
        }

        void AddPoint(Relation point, CarrierLocation location)
        {
            Add(point, location);
        }

        void Visit(IElement element, bool allowBareLocation)
        {
            if (allowBareLocation && element is CarrierLocation location)
            {
                AddLocation(location);
                return;
            }

            if (element is not Relation relation)
            {
                return;
            }

            if (Contribution.TryRead(relation, out var contribution))
            {
                Add(contribution);
                return;
            }

            if (TryGetPoint(relation, out var point))
            {
                AddPoint(relation, point);
                return;
            }

            Visit(relation.Left, allowBareLocation: false);
            Visit(relation.Right, allowBareLocation: false);
        }
    }

    public void Add(Contribution contribution) =>
        Add(contribution.Source, contribution.Location, contribution.Scale);

    public void Add(IElement source, CarrierLocation location)
    {
        Add(source, location, new CarrierLocation(location.Carrier, 1));
    }

    public void Set(IElement source, Carrier carrier, ulong ticks, ulong scale = 1)
    {
        contributions.RemoveAll(contribution =>
            Equals(contribution.Source, source) &&
            contribution.Location.Carrier == carrier);

        if (ticks != 0)
        {
            contributions.Add(new(
                source,
                new CarrierLocation(carrier, ticks),
                new CarrierLocation(carrier, scale)));
        }
    }

    public void ReplaceCarrier(Carrier carrier, IElement source, ulong ticks, ulong scale = 1)
    {
        contributions.RemoveAll(contribution => contribution.Location.Carrier == carrier);
        Set(source, carrier, ticks, scale);
    }

    public ulong TotalOn(Carrier carrier) => contributions
        .Where(contribution => contribution.Location.Carrier == carrier)
        .Aggregate(0UL, (total, contribution) => checked(total + contribution.Location.Ticks));

    public ulong TotalOn(Carrier carrier, IElement except) => contributions
        .Where(contribution =>
            contribution.Location.Carrier == carrier &&
            !Equals(contribution.Source, except))
        .Aggregate(0UL, (total, contribution) => checked(total + contribution.Location.Ticks));

    public ulong ScaleOn(Carrier carrier)
    {
        var scale = 1UL;
        foreach (var contribution in contributions)
        {
            if (contribution.Location.Carrier == carrier && contribution.Scale.Ticks > 1)
            {
                scale = checked(scale * contribution.Scale.Ticks);
            }
        }

        return scale;
    }

    public bool HasZeroScaleOn(Carrier carrier) => contributions.Any(contribution =>
        contribution.Location.Carrier == carrier &&
        contribution.Scale.Ticks == 0);

    public bool HasSource(IElement source) => contributions.Any(contribution =>
        Equals(contribution.Source, source));

    public void SetSurface(IElement source, IElement surface)
    {
        foreach (var group in CarrierLocationsOf(surface).GroupBy(location => location.Carrier))
        {
            Set(
                source,
                group.Key,
                group.Aggregate(0UL, (total, location) => checked(total + location.Ticks)));
        }
    }

    public Relation[] Relations() => [.. OrderedContributions()
        .Select(contribution => contribution.Surface)];

    public Relation[] RelationsOn(Carrier carrier) => [.. OrderedContributions()
        .Where(contribution => contribution.Location.Carrier == carrier)
        .Select(contribution => contribution.Surface)];

    public CarrierLocation[] ScalesOn(Carrier carrier) => [.. OrderedContributions()
        .Where(contribution => contribution.Location.Carrier == carrier)
        .Select(contribution => contribution.Scale)];

    public IElement Surface() => BuildSurface([.. OrderedContributions()
        .Select(contribution => (IElement)contribution.Surface)]);

    public string[] Snapshot() => [.. contributions
        .Select(contribution => ElementKey(contribution.Surface))
        .Order()];

    public bool SameAs(string[] snapshot) =>
        Snapshot().SequenceEqual(snapshot);

    private IOrderedEnumerable<Contribution> OrderedContributions() => contributions
        .OrderBy(contribution => contribution.Location.Carrier.Lineage)
        .ThenBy(contribution => contribution.Location.Carrier.Id)
        .ThenBy(contribution => contribution.Location.Carrier.Flow)
        .ThenBy(contribution => ElementKey(contribution.Source))
        .ThenBy(contribution => contribution.Location.Ticks)
        .ThenBy(contribution => contribution.Scale.Ticks);

    private void Add(IElement source, CarrierLocation location, CarrierLocation scale)
    {
        if (location.Ticks == 0)
        {
            return;
        }

        var index = contributions.FindIndex(contribution =>
            Equals(contribution.Source, source) &&
            contribution.Location.Carrier == location.Carrier);
        if (index < 0)
        {
            contributions.Add(new(source, location, scale));
            return;
        }

        var contribution = contributions[index];
        contributions[index] = contribution with
        {
            Location = new CarrierLocation(
                location.Carrier,
                checked(contribution.Location.Ticks + location.Ticks))
        };
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
            location.Carrier == carrier)
        {
            point = location;
            return true;
        }

        point = default;
        return false;
    }

    private static string ElementKey(IElement element) => element switch
    {
        Carrier carrier => $"C({carrier.Lineage},{carrier.Id},{carrier.Flow})",
        CarrierLocation location => $"L({location.Carrier.Lineage},{location.Carrier.Id},{location.Carrier.Flow},{location.Ticks})",
        Relation relation => $"R({ElementKey(relation.Left)}>{ElementKey(relation.Right)})",
        _ => element.GetType().FullName ?? string.Empty
    };
}
