namespace Gojo.Core;

public readonly struct CarrierLocation : IElement, IEquatable<CarrierLocation>
{
    public CarrierLocation(Carrier carrier, long ticks)
        : this(carrier, new TickRef(ticks))
    {
    }

    public CarrierLocation(Carrier carrier, ulong ticks)
        : this(carrier, checked((long)ticks))
    {
    }

    public CarrierLocation(Carrier carrier, TickRef tick)
    {
        Carrier = carrier ?? throw new ArgumentNullException(nameof(carrier));
        Tick = tick ?? throw new ArgumentNullException(nameof(tick));
    }

    public Carrier Carrier { get; }

    public TickRef Tick { get; }

    public long Ticks => Tick?.Ticks ?? 0;

    public CarrierLocation Dual() => new(Carrier, checked(-Ticks));

    public CarrierRead ReadFrom(Carrier reader) => CarrierRead.From(this, reader);

    public bool SameTickRefAs(CarrierLocation other) => ReferenceEquals(Tick, other.Tick);

    public bool Equals(CarrierLocation other)
    {
        if (Carrier is null || other.Carrier is null)
        {
            return ReferenceEquals(Carrier, other.Carrier) && Ticks == other.Ticks;
        }

        return Carrier.SameIdentityAs(other.Carrier) && Ticks == other.Ticks;
    }

    public override bool Equals(object? obj) => obj is CarrierLocation other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Carrier?.Id ?? 0, Ticks);

    public static bool operator ==(CarrierLocation left, CarrierLocation right) => left.Equals(right);

    public static bool operator !=(CarrierLocation left, CarrierLocation right) => !left.Equals(right);

    public override string ToString() => $"{Carrier}:{Ticks}";

    IElement IElement.Dual() => Dual();
}
