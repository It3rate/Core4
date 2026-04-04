using System.Collections.Immutable;
using Core4.Engine;

namespace Core4.Sessions;

public static class SiteExamples
{
    public static ImmutableArray<(string Name, Site Site)> Create()
    {
        var x = new Carrier(1);
        var open = Site.Open(x, 0);
        var data = open
            .Load(1, Ratio.Tension(2))
            .Load(1, Ratio.Tension(2))
            .Load(1, Ratio.Tension(2))
            .Load(1, Ratio.Tension(2))
            .Load(1, Ratio.Tension(2));
        var committed = data.Resolve(1, new Ratio(10, 5));

        return
        [
            ("open", open),
            ("data", data),
            ("committed", committed)
        ];
    }

    public static ImmutableArray<string> Describe()
    {
        var examples = Create();
        var descriptions = ImmutableArray.CreateBuilder<string>(examples.Length);

        foreach (var (name, site) in examples)
        {
            descriptions.Add(
                $"{name}: position={site.Position.Normalize()}, onCarrier={site.On(site.Carrier).Normalize()}, " +
                $"potential={site.Potential.Raw}, open={site.Field.OpenCount.Raw}, occupied={site.Field.OccupiedCount.Raw}, bound={site.Field.BoundCount.Raw}, " +
                $"slots=[{string.Join(", ", site.Field.Slots().Select(slot => slot.Normalize().ToString()))}]");
        }

        return descriptions.MoveToImmutable();
    }
}
