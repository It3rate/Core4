using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Formatting;

internal static class ProbeSiteFormatting
{
    public static string CarrierLabel(Carrier carrier) => carrier.Id switch
    {
        1L => "x",
        2L => "y",
        3L => "z",
        4L => "w",
        _ => $"c{carrier.Id}"
    };

    public static string SiteLabel(Site site) => $"{CarrierLabel(site.Carrier)}{PositionLabel(site)}";

    public static string PlacementLabel(Placement placement) => $"{CarrierLabel(placement.PrimaryCarrier)}{PositionLabel(placement)}";

    public static string PositionLabel(Site site) => PositionLabel(site.On(site.Carrier));

    public static string PositionLabel(Placement placement) => PositionLabel(placement.On(placement.PrimaryCarrier));

    public static string PositionLabel(Ratio position)
    {
        var normalized = position.Normalize();
        return normalized.IsResolved && normalized.Remainder == Measure.Zero
            ? normalized.Whole.Raw.ToString()
            : normalized.ToString();
    }

    public static string PositionLabel(Measure position) => position.Raw.ToString();

    public static bool SameNode(Site left, Site right) =>
        left.Carrier == right.Carrier && left.On(left.Carrier).Normalize() == right.On(right.Carrier).Normalize();
}
