using Gojo.Core;

namespace Gojo.Reads;

public static class CarrierLocationReads
{
    public static CarrierLocation NewTensionAfter(
        this CarrierLocation location,
        CarrierLocation accounted,
        Carrier reader) =>
        location.NewTensionAfter(reader, accounted);

    public static Relation AccountedBy(
        this CarrierLocation location,
        Carrier reader,
        Relation accounted) =>
        location.AccountedBy(reader, accounted.CarrierLocations());

    public static Relation AccountedBy(
        this CarrierLocation location,
        Carrier reader,
        Relation firstAccounted,
        params Relation[] remainingAccounted) =>
        location.AccountedBy(reader, Flatten(firstAccounted, remainingAccounted));

    public static Relation AccountedBy(
        this CarrierLocation location,
        Carrier reader,
        params CarrierLocation[] accounted)
    {
        if (!location.ReadFrom(reader).IsVisible)
        {
            var zero = new CarrierLocation(location.Carrier, 0);
            return new(zero, zero);
        }

        var tension = location.NewTensionAfter(reader, accounted);
        return new(new CarrierLocation(location.Carrier, location.Ticks - tension.Ticks), tension);
    }

    public static CarrierLocation NewTensionAfter(
        this CarrierLocation location,
        Carrier reader,
        Relation accounted) =>
        location.NewTensionAfter(reader, accounted.CarrierLocations());

    public static CarrierLocation NewTensionAfter(
        this CarrierLocation location,
        Carrier reader,
        Relation firstAccounted,
        params Relation[] remainingAccounted) =>
        location.NewTensionAfter(reader, Flatten(firstAccounted, remainingAccounted));

    public static CarrierLocation NewTensionAfter(
        this CarrierLocation location,
        Carrier reader,
        params CarrierLocation[] accounted)
    {
        var current = location.ReadFrom(reader);
        if (!current.IsVisible)
        {
            return new(location.Carrier, 0);
        }

        var coveredTicks = 0UL;
        foreach (var account in accounted)
        {
            var covered = account.ReadFrom(reader);
            if (!covered.IsVisible || covered.Direction != current.Direction)
            {
                continue;
            }

            var remaining = location.Ticks - coveredTicks;
            if (covered.Magnitude >= remaining)
            {
                return new(location.Carrier, 0);
            }

            coveredTicks += covered.Magnitude;
        }

        return new(location.Carrier, location.Ticks - coveredTicks);
    }

    private static CarrierLocation[] Flatten(Relation first, Relation[] rest)
    {
        var locations = new List<CarrierLocation>(first.CarrierLocations());
        foreach (var relation in rest)
        {
            locations.AddRange(relation.CarrierLocations());
        }

        return [.. locations];
    }
}
