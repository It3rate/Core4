using Gojo.Core;

namespace Gojo.Reads;

public static class StructuralTransformReads
{
    public static Relation AddAndSettle(this Relation structure, CarrierLocation incoming) =>
        structure.AddAndSettle((IElement)incoming);

    public static Relation AddAndSettle(this Relation structure, IElement incoming)
    {
        var state = ContributionSurface.From(structure.Left);
        var relations = RelationsOf(structure.Right);
        var projected = new HashSet<Relation>();

        state.AddIncoming(incoming);
        ApplyUntilQuiet(state, relations, projected);

        return new(state.Surface(), structure.Right);
    }

    public static CarrierLocation PositionOn(this Relation structure, Carrier carrier) =>
        structure.Left.ContributionTotalOn(carrier);

    private static void ApplyUntilQuiet(
        ContributionSurface state,
        Relation[] relations,
        HashSet<Relation> projected)
    {
        while (true)
        {
            var before = state.Snapshot();
            foreach (var relation in relations)
            {
                ApplyRelation(state, relation, projected);
            }

            if (state.SameAs(before))
            {
                return;
            }
        }
    }

    private static void ApplyRelation(
        ContributionSurface state,
        Relation relation,
        HashSet<Relation> projected)
    {
        var left = CarrierSitesOf(relation.Left);
        var right = CarrierSitesOf(relation.Right);

        if (TryReadOneCarrier(left, out var leftSites) &&
            TryReadOneCarrier(right, out var rightSites))
        {
            if (leftSites.Key == rightSites.Key)
            {
                AccountLocal(state, leftSites.Carrier, leftSites.Max, rightSites.Min);
                return;
            }

            ProjectFreshExtent(state, relation, projected, leftSites, rightSites);
            if (rightSites.Width != 0)
            {
                return;
            }

            RouteTension(state, relation, leftSites, rightSites);
        }
    }

    private static void AccountLocal(
        ContributionSurface state,
        Carrier carrier,
        ulong source,
        ulong target)
    {
        if (source == 0 || source < target)
        {
            return;
        }

        var total = state.TotalOn(carrier);
        if (total <= target)
        {
            return;
        }

        var local = source == target
            ? Math.Min(total, source)
            : checked(target + ((total - target) % (source - target)));
        if (local == total)
        {
            return;
        }

        state.ReplaceCarrier(carrier, carrier, local);
        state.AddTension(carrier, total - local);
    }

    private static void RouteTension(
        ContributionSurface state,
        Relation relation,
        CarrierSites source,
        CarrierSites receiver)
    {
        var unit = source.Width == 0 ? source.Max : source.Width;
        if (unit == 0)
        {
            return;
        }

        var target = source.Width == 0 ? 0 : source.Min;
        AccountLocal(state, source.Carrier, source.Max, target);

        var tension = state.TensionOn(source.Carrier);
        var received = tension / unit;
        if (received == 0)
        {
            return;
        }

        state.SetTension(source.Carrier, tension % unit);
        state.Add(relation, new CarrierLocation(receiver.Carrier, received));
    }

    private static void ProjectFreshExtent(
        ContributionSurface state,
        Relation relation,
        HashSet<Relation> projected,
        CarrierSites source,
        CarrierSites receiver)
    {
        if (projected.Contains(relation) ||
            source.Width == 0 ||
            receiver.Width == 0)
        {
            return;
        }

        var fresh = state.FreshOn(source.Carrier);
        if (fresh == 0)
        {
            return;
        }

        var received = checked((fresh * receiver.Width) / source.Width);
        if (received == 0)
        {
            return;
        }

        projected.Add(relation);
        state.Add(relation, new CarrierLocation(receiver.Carrier, received));
    }

    private static Relation[] RelationsOf(IElement surface)
    {
        var relations = new List<Relation>();
        Visit(surface);
        return [.. relations];

        void Visit(IElement element)
        {
            if (element is not Relation relation)
            {
                return;
            }

            relations.Add(relation);
            Visit(relation.Left);
            Visit(relation.Right);
        }
    }

    private static CarrierLocation[] CarrierSitesOf(IElement element) => element switch
    {
        CarrierLocation location => [location],
        Relation relation => [.. CarrierSitesOf(relation.Left), .. CarrierSitesOf(relation.Right)],
        _ => []
    };

    private static bool TryReadOneCarrier(
        CarrierLocation[] sites,
        out CarrierSites carrierSites)
    {
        if (sites.Length == 0)
        {
            carrierSites = default;
            return false;
        }

        var key = CarrierKey.From(sites[0].Carrier);
        if (sites.Any(site => CarrierKey.From(site.Carrier) != key))
        {
            carrierSites = default;
            return false;
        }

        carrierSites = new(
            key,
            sites[0].Carrier,
            sites.Min(site => site.Ticks),
            sites.Max(site => site.Ticks));
        return true;
    }

    private readonly record struct CarrierSites(
        CarrierKey Key,
        Carrier Carrier,
        ulong Min,
        ulong Max)
    {
        public ulong Width => Max - Min;
    }

    private readonly record struct CarrierKey(ulong Id, ulong Lineage, long Flow)
    {
        public static CarrierKey From(Carrier carrier) =>
            new(carrier.Id, carrier.Lineage, carrier.Flow == 0 ? 1 : carrier.Flow);
    }
}
