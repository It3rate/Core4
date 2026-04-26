namespace Gojo.Core;

public readonly record struct CarrierLocation(Carrier Carrier, ulong Ticks)
{
    public CarrierRead ReadFrom(Carrier reader) => CarrierRead.From(this, reader);

    public CarrierLocation NewTensionAfter(CarrierLocation accounted, Carrier reader) =>
        NewTensionAfter(reader, accounted);

    public CarrierLocation NewTensionAfter(Carrier reader, params CarrierLocation[] accounted)
    {
        var current = ReadFrom(reader);
        if (!current.IsVisible)
        {
            return new(Carrier, 0);
        }

        var coveredTicks = 0UL;
        foreach (var account in accounted)
        {
            var covered = account.ReadFrom(reader);
            if (!covered.IsVisible || covered.Direction != current.Direction)
            {
                continue;
            }

            var remaining = Ticks - coveredTicks;
            if (covered.Magnitude >= remaining)
            {
                return new(Carrier, 0);
            }

            coveredTicks += covered.Magnitude;
        }

        return new(Carrier, Ticks - coveredTicks);
    }
}
