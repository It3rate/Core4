using Gojo.Core;

namespace Gojo.GearDemo.Components;

internal sealed class GojoSlider
{
    private readonly TickRef positionTick;

    private GojoSlider(int trackMinimum, int trackMaximum, int outputMinimum, int outputMaximum)
    {
        if (trackMinimum < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(trackMinimum), "Slider carrier coordinates must be non-negative.");
        }

        if (outputMinimum < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(outputMinimum), "Slider output coordinates must be non-negative.");
        }

        if (trackMaximum < trackMinimum)
        {
            throw new ArgumentOutOfRangeException(nameof(trackMaximum), "Slider track maximum must be greater than or equal to minimum.");
        }

        if (outputMaximum < outputMinimum)
        {
            throw new ArgumentOutOfRangeException(nameof(outputMaximum), "Slider output maximum must be greater than or equal to minimum.");
        }

        Track = new Carrier();
        Output = new Carrier();
        positionTick = new TickRef(trackMinimum);
        TrackSegment = new Relation(
            new CarrierLocation(Track, trackMinimum),
            new CarrierLocation(Track, trackMaximum));
        PositionSegment = new Relation(
            new CarrierLocation(Track, trackMinimum),
            new CarrierLocation(Track, positionTick));
        OutputSegment = new Relation(
            new CarrierLocation(Output, outputMinimum),
            new CarrierLocation(Output, outputMaximum));
        OutputRelation = new Relation(TrackSegment, OutputSegment);
    }

    public Carrier Track { get; }

    public Carrier Output { get; }

    public Relation TrackSegment { get; }

    public Relation PositionSegment { get; }

    public Relation OutputSegment { get; }

    public Relation OutputRelation { get; }

    public long MinimumTicks => ReadTrackBounds().Minimum;

    public long MaximumTicks => ReadTrackBounds().Maximum;

    public long RangeTicks => TrackSegment.FlowOn(Track).Ticks;

    public static GojoSlider Create(int minimum, int maximum, int value)
    {
        return Create(minimum, maximum, minimum, maximum, value);
    }

    public static GojoSlider Create(
        int trackMinimum,
        int trackMaximum,
        int outputMinimum,
        int outputMaximum,
        int outputValue)
    {
        var slider = new GojoSlider(trackMinimum, trackMaximum, outputMinimum, outputMaximum);
        slider.SetOutput(outputValue);
        return slider;
    }

    public void SetOutput(int value)
    {
        var bounds = ReadOutputBounds();
        var clamped = Math.Clamp((long)value, bounds.Minimum, bounds.Maximum);
        Output.Set(clamped);
        PullOutputToPosition();
    }

    public void SetPosition(int ticks)
    {
        var bounds = ReadTrackBounds();
        var clamped = Math.Clamp((long)ticks, bounds.Minimum, bounds.Maximum);
        Track.Set(clamped);
        positionTick.Set(clamped);
        PushPositionToOutput();
    }

    public GojoSliderSnapshot Read()
    {
        var position = PositionSegment.FlowOn(Track);
        return new GojoSliderSnapshot(
            Track,
            Output,
            TrackSegment,
            PositionSegment,
            OutputSegment,
            OutputRelation,
            TrackSegment.FlowOn(Track),
            position,
            Track.ValueSlot,
            Output.ValueSlot,
            Track.TensionSlot,
            Output.TensionSlot);
    }

    private void PushPositionToOutput()
    {
        Output.TrySetFrom(OutputRelation, Track.ValueSlot);
    }

    private void PullOutputToPosition()
    {
        Track.TrySetFrom(OutputRelation, Output.ValueSlot);
        positionTick.Set(Track.ValueSlot.Ticks);
    }

    private (long Minimum, long Maximum) ReadTrackBounds() => ReadBounds(TrackSegment, Track);

    private (long Minimum, long Maximum) ReadOutputBounds() => ReadBounds(OutputSegment, Output);

    private static (long Minimum, long Maximum) ReadBounds(Relation segment, Carrier carrier)
    {
        var boundaries = segment.BoundariesOn(carrier);
        return (boundaries.LeftLocation.Ticks, boundaries.RightLocation.Ticks);
    }
}

internal readonly record struct GojoSliderSnapshot(
    Carrier Track,
    Carrier Output,
    Relation TrackSegment,
    Relation PositionSegment,
    Relation OutputSegment,
    Relation OutputRelation,
    CarrierLocation Range,
    CarrierLocation Position,
    CarrierLocation TrackValue,
    CarrierLocation OutputValue,
    CarrierLocation TrackTension,
    CarrierLocation OutputTension);
