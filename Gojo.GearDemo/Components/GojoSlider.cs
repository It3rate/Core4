using Gojo.Core;

namespace Gojo.GearDemo.Components;

internal sealed class GojoSlider
{
    private GojoSlider(int minimum, int maximum)
    {
        if (minimum < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimum), "Slider carrier coordinates must be non-negative.");
        }

        if (maximum < minimum)
        {
            throw new ArgumentOutOfRangeException(nameof(maximum), "Slider maximum must be greater than or equal to minimum.");
        }

        Track = new Carrier();
        Minimum = new CarrierLocation(Track, minimum);
        Maximum = new CarrierLocation(Track, maximum);
        TrackSegment = Relation.Segment(Track, minimum, maximum);
        RangeRelation = new Relation(Minimum, Maximum);
    }

    public Carrier Track { get; }

    public Carrier Output => Track;

    public CarrierLocation Minimum { get; }

    public CarrierLocation Maximum { get; }

    public Relation TrackSegment { get; }

    public Relation RangeRelation { get; }

    public long MinimumTicks => Minimum.Ticks;

    public long MaximumTicks => Maximum.Ticks;

    public long RangeTicks => RangeRelation.FlowOn(Track).Ticks;

    public static GojoSlider Create(int minimum, int maximum, int value)
    {
        var slider = new GojoSlider(minimum, maximum);
        slider.SetOutput(value);
        return slider;
    }

    public void SetOutput(int value)
    {
        var clamped = Math.Clamp((long)value, MinimumTicks, MaximumTicks);
        Track.Set(clamped);
    }

    public GojoSliderSnapshot Read()
    {
        var value = Track.ValueSlot;
        var position = new Relation(Minimum, value).FlowOn(Track);
        return new GojoSliderSnapshot(
            Track,
            Minimum,
            Maximum,
            RangeRelation.FlowOn(Track),
            position,
            value,
            Track.TensionSlot,
            TrackSegment,
            RangeRelation);
    }
}

internal readonly record struct GojoSliderSnapshot(
    Carrier Track,
    CarrierLocation Minimum,
    CarrierLocation Maximum,
    CarrierLocation Range,
    CarrierLocation Position,
    CarrierLocation Value,
    CarrierLocation Tension,
    Relation TrackSegment,
    Relation RangeRelation);
