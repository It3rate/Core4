namespace Gojo.Core;

public readonly record struct LayerTransform(CarrierLocation Value)
{
    public static LayerTransform Additive => new(new CarrierLocation(Carrier.Unknown, 0));

    public static LayerTransform Expansive(ulong magnitude) => new(new CarrierLocation(Carrier.Unknown, magnitude));

    public static LayerTransform Contractive(ulong magnitude) => new(new CarrierLocation(Carrier.Unknown.Inverse(), magnitude));

    public bool IsAdditive => Value.Ticks == 0;

    public bool IsExpansive => !IsAdditive && Carrier.Unknown.ReadSignOf(Value.Carrier) > 0;

    public bool IsContractive => !IsAdditive && Carrier.Unknown.ReadSignOf(Value.Carrier) < 0;

    public ulong Magnitude => Value.Ticks;
}
