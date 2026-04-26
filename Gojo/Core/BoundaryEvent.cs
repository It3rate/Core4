namespace Gojo.Core;

public readonly record struct BoundaryEvent(CarrierLocation Side, CarrierLocation Travel)
{
    public static Carrier SideCarrier => new(101);

    public static Carrier TravelCarrier => new(102);

    public static BoundaryEvent PositiveOverflow => new(new CarrierLocation(SideCarrier, 1), new CarrierLocation(TravelCarrier, 1));

    public static BoundaryEvent PositiveRefill => new(new CarrierLocation(SideCarrier, 1), new CarrierLocation(TravelCarrier.Inverse(), 1));

    public static BoundaryEvent NegativeRefill => new(new CarrierLocation(SideCarrier.Inverse(), 1), new CarrierLocation(TravelCarrier, 1));

    public static BoundaryEvent NegativeOverflow => new(new CarrierLocation(SideCarrier.Inverse(), 1), new CarrierLocation(TravelCarrier.Inverse(), 1));

    public long SideDirection => SideCarrier.ReadSignOf(Side.Carrier);

    public long TravelDirection => TravelCarrier.ReadSignOf(Travel.Carrier);

    public long Alignment => Math.Sign(SideDirection * TravelDirection);
}
