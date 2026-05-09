using Gojo.Core;

namespace Gojo.GearDemo.Components;

internal sealed class GojoSlider
{
    private GojoSlider(Carrier carrier, long minimum, long maximum)
    {
        var minimumTick = new TickRef(minimum);
        Track = carrier;
        TrackSegment = new Relation(
            new CarrierLocation(Track, minimumTick),
            new CarrierLocation(Track, new TickRef(maximum)));
        PositionSegment = new Relation(
            new CarrierLocation(Track, minimumTick),
            Track.ValueSlot);
    }

    public Carrier Track { get; }

    public Relation TrackSegment { get; }

    public Relation PositionSegment { get; }

    public long MinimumTicks => TrackSegment.BoundariesOn(Track).LeftLocation.Ticks;

    public long MaximumTicks => TrackSegment.BoundariesOn(Track).RightLocation.Ticks;

    public long RangeTicks => TrackSegment.FlowOn(Track).Ticks;

    public static GojoSlider Create(int minimum, int maximum, int value)
    {
        var carrier = new Carrier();
        var slider = Create(carrier, minimum, maximum);
        slider.SetPosition(value);
        return slider;
    }

    public static GojoSlider Create(Carrier carrier, long minimum, long maximum) =>
        new(carrier, minimum, maximum);

    public void SetPosition(long ticks) => Track.Set(ticks);

    public GojoSliderSnapshot Read() =>
        new(
            Track,
            TrackSegment,
            PositionSegment,
            TrackSegment.FlowOn(Track),
            PositionSegment.FlowOn(Track),
            Track.ValueSlot,
            Track.TensionSlot);
}

internal readonly record struct GojoSliderSnapshot(
    Carrier Track,
    Relation TrackSegment,
    Relation PositionSegment,
    CarrierLocation Range,
    CarrierLocation Position,
    CarrierLocation Value,
    CarrierLocation Tension);
