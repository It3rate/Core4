using Gojo.Core;

namespace Gojo.Reads;

public static class ContributionReads
{
    public static Relation[] Contributions(this IElement surface) =>
        ContributionSurface.From(surface).Relations();

    public static Relation[] ContributionsOn(this IElement surface, Carrier carrier) =>
        ContributionSurface.From(surface).RelationsOn(carrier);

    public static CarrierLocation ContributionTotalOn(this IElement surface, Carrier carrier) =>
        new(carrier, ContributionSurface.From(surface).ValueOn(carrier));
}
