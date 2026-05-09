using Gojo.Core;
using Gojo.GearDemo.Adapters;

namespace Gojo.GearDemo.Model;

internal static class GearStructure
{
    public static Relation PointAt(Carrier carrier, TickRef ticks) =>
        new(new CarrierLocation(carrier, ticks), carrier);

    public static Relation SegmentFromZeroTo(Carrier carrier, TickRef end) =>
        new(PointAt(carrier, new TickRef(0)), PointAt(carrier, end));

    public static Relation AttachStructure(Carrier anchor, params IElement[] elements)
    {
        var structure = Gather(elements);
        anchor.Attach(structure);
        return structure;
    }

    private static Relation Gather(IReadOnlyList<IElement> elements)
    {
        var count = elements.Count;
        return count switch
        {
            0 => Relation.Point(Carrier.Unknown, 0),
            1 => new Relation(elements[0], Relation.Point(Carrier.Unknown, 0)),
            2 => new Relation(elements[0], elements[1]),
            _ => new Relation(elements[0], Gather([.. elements.Skip(1)]))
        };
    }
}

internal sealed record GearNumbers(
    Carrier TicksPerToothCarrier,
    Carrier DiametralPitch,
    Carrier RadiusTicksPerInch,
    Carrier ToothProfileRadiusTicks,
    Carrier HalfPitchTicks,
    Carrier QuarterPitchTicks,
    Relation DiametralPitchToRadiusTicks,
    Relation TicksPerToothToHalfPitch,
    Relation HalfPitchToQuarterPitch,
    Relation Structure)
{
    public int TicksPerToothTicks => GearDemoAdapters.ToInt(TicksPerToothCarrier);

    public int DiametralPitchTicks => GearDemoAdapters.ToInt(DiametralPitch);

    public int RadiusTicksPerInchTicks => GearDemoAdapters.ToInt(RadiusTicksPerInch);

    public int ToothProfileRadiusTicksValue => GearDemoAdapters.ToInt(ToothProfileRadiusTicks);

    public int HalfPitchTicksValue => GearDemoAdapters.ToInt(HalfPitchTicks);

    public int QuarterPitchTicksValue => GearDemoAdapters.ToInt(QuarterPitchTicks);

    public static GearNumbers Create(int ticksPerTooth, int diametralPitch, int toothProfileRadiusTicks)
    {
        var ticksPerToothCarrier = new Carrier();
        var diametralPitchCarrier = new Carrier();
        var radiusTicksPerInch = new Carrier();
        var toothProfileRadiusTicksCarrier = new Carrier();
        var halfPitchTicks = new Carrier();
        var quarterPitchTicks = new Carrier();
        ticksPerToothCarrier.Set(ticksPerTooth);
        diametralPitchCarrier.Set(diametralPitch);
        toothProfileRadiusTicksCarrier.Set(toothProfileRadiusTicks);

        var diametralPitchToRadiusTicks = new Relation(
            Relation.Segment(diametralPitchCarrier, 0, 1),
            Relation.Segment(radiusTicksPerInch, 0, 2));
        var radiusTicksOutput = ticksPerToothCarrier.AttachLiveOutput(
            diametralPitchCarrier.ValueSlot,
            diametralPitchToRadiusTicks);

        var ticksPerToothToHalfPitch = new Relation(
            Relation.Segment(ticksPerToothCarrier, 0, 2),
            Relation.Segment(halfPitchTicks, 0, 1));
        var halfPitchOutput = ticksPerToothCarrier.AttachLiveOutput(
            ticksPerToothCarrier.ValueSlot,
            ticksPerToothToHalfPitch);

        var halfPitchToQuarterPitch = new Relation(
            Relation.Segment(halfPitchTicks, 0, 2),
            Relation.Segment(quarterPitchTicks, 0, 1));
        var quarterPitchOutput = ticksPerToothCarrier.AttachLiveOutput(
            halfPitchTicks.ValueSlot,
            halfPitchToQuarterPitch);

        var structure = GearStructure.AttachStructure(
            ticksPerToothCarrier,
            Relation.Point(ticksPerToothCarrier, 0),
            Relation.Point(diametralPitchCarrier, 0),
            Relation.Point(toothProfileRadiusTicksCarrier, 0),
            radiusTicksOutput,
            halfPitchOutput,
            quarterPitchOutput,
            diametralPitchToRadiusTicks,
            ticksPerToothToHalfPitch,
            halfPitchToQuarterPitch);

        return new GearNumbers(
            ticksPerToothCarrier,
            diametralPitchCarrier,
            radiusTicksPerInch,
            toothProfileRadiusTicksCarrier,
            halfPitchTicks,
            quarterPitchTicks,
            diametralPitchToRadiusTicks,
            ticksPerToothToHalfPitch,
            halfPitchToQuarterPitch,
            structure);
    }
}

internal sealed record GearCarrier(
    Carrier ToothCountCarrier,
    Carrier ShapeCenter,
    Carrier Perimeter,
    Carrier Rotations,
    Carrier ToothPosition,
    Carrier PerimeterTicksCarrier,
    Carrier DiameterTwentyFourthsCarrier,
    Carrier RadiusFortyEighthsCarrier,
    Carrier RootRadiusFortyEighthsCarrier,
    Carrier TipRadiusFortyEighthsCarrier,
    Carrier PitchCircle,
    Carrier RootCircle,
    Carrier TipCircle,
    Relation PerimeterSource,
    Relation PitchCircleSource,
    Relation RootCircleSource,
    Relation TipCircleSource,
    Relation Partition,
    Relation ToothPartition,
    Relation PerimeterToToothPosition,
    Relation PerimeterToRotation,
    Relation TeethToPerimeter,
    Relation TeethToDiameter,
    Relation DiameterToRadius,
    Relation RootRadiusOperation,
    Relation TipRadiusOperation,
    Relation PitchCircleAttachment,
    Relation RootCircleAttachment,
    Relation TipCircleAttachment,
    Relation Structure)
{
    public int ToothCount => GearDemoAdapters.ToInt(ToothCountCarrier);

    public int PerimeterTicks => GearDemoAdapters.ToInt(PerimeterTicksCarrier);

    public int DiameterTwentyFourths => GearDemoAdapters.ToInt(DiameterTwentyFourthsCarrier);

    public int RadiusFortyEighths => GearDemoAdapters.ToInt(RadiusFortyEighthsCarrier);

    public int RootRadiusFortyEighths => GearDemoAdapters.ToInt(RootRadiusFortyEighthsCarrier);

    public int TipRadiusFortyEighths => GearDemoAdapters.ToInt(TipRadiusFortyEighthsCarrier);

    public static GearCarrier Create(Carrier toothCountCarrier, GearNumbers numbers)
    {
        var shapeCenter = new Carrier();
        var rotations = new Carrier();
        var perimeterTicksCarrier = new Carrier();
        var diameterTwentyFourths = new Carrier();
        var radiusFortyEighths = new Carrier();
        var rootRadiusFortyEighths = new Carrier();
        var tipRadiusFortyEighths = new Carrier();

        var toothCountPort = new Relation(
            Relation.Point(toothCountCarrier, 1),
            Relation.Point(Carrier.Unknown, 1));
        var ticksPerToothPort = new Relation(
            Relation.Point(numbers.TicksPerToothCarrier, 1),
            Relation.Point(Carrier.Unknown, 1));
        var teethToPerimeter = new Relation(
            new Relation(toothCountPort, ticksPerToothPort),
            Relation.Segment(perimeterTicksCarrier, 0, 1));
        var perimeterTicksOutput = toothCountCarrier.AttachLiveOutput(
            new Relation(toothCountCarrier.ValueSlot, numbers.TicksPerToothCarrier.ValueSlot),
            teethToPerimeter);

        var teethToDiameter = new Relation(
            Relation.Point(toothCountCarrier, 1),
            Relation.Point(diameterTwentyFourths, 1));
        var diameterOutput = toothCountCarrier.AttachLiveOutput(
            toothCountCarrier.ValueSlot,
            teethToDiameter);

        var diameterToRadius = new Relation(
            Relation.Point(diameterTwentyFourths, 1),
            Relation.Point(radiusFortyEighths, 1));
        var radiusOutput = toothCountCarrier.AttachLiveOutput(
            diameterTwentyFourths.ValueSlot,
            diameterToRadius);

        var pitchRadiusPort = new Relation(
            Relation.Point(radiusFortyEighths, 1),
            Relation.Point(Carrier.Unknown, 1));
        var addProfilePort = new Relation(
            Relation.Point(numbers.ToothProfileRadiusTicks, 1),
            Relation.Point(Carrier.Unknown, 1));
        var subtractProfilePort = new Relation(
            Relation.Point(numbers.ToothProfileRadiusTicks, -1),
            Relation.Point(Carrier.Unknown, 1));
        var tipRadiusOperation = new Relation(
            new Relation(pitchRadiusPort, addProfilePort),
            Relation.Point(tipRadiusFortyEighths, 1));
        var rootRadiusOperation = new Relation(
            new Relation(pitchRadiusPort, subtractProfilePort),
            Relation.Point(rootRadiusFortyEighths, 1));
        var radiusInput = new Relation(radiusFortyEighths.ValueSlot, numbers.ToothProfileRadiusTicks.ValueSlot);
        var tipRadiusOutput = toothCountCarrier.AttachLiveOutput(radiusInput, tipRadiusOperation);
        var rootRadiusOutput = toothCountCarrier.AttachLiveOutput(radiusInput, rootRadiusOperation);

        var perimeterSource = Relation.Point(shapeCenter, 0);
        var pitchCircleSource = GearStructure.PointAt(shapeCenter, radiusFortyEighths.ValueSlot.Tick);
        var rootCircleSource = GearStructure.PointAt(shapeCenter, rootRadiusFortyEighths.ValueSlot.Tick);
        var tipCircleSource = GearStructure.PointAt(shapeCenter, tipRadiusFortyEighths.ValueSlot.Tick);
        var perimeter = new Carrier(UnitShape.CircularFrom(pitchCircleSource));
        var toothPosition = new Carrier(UnitShape.CircularFrom(pitchCircleSource));
        var rootCircle = new Carrier(UnitShape.CircularFrom(rootCircleSource));
        var tipCircle = new Carrier(UnitShape.CircularFrom(tipCircleSource));
        var pitchCircle = perimeter;

        var partition = perimeter.Attach(new Relation(
            GearStructure.PointAt(perimeter, perimeterTicksCarrier.ValueSlot.Tick),
            Relation.Point(rotations, 0)));
        var toothPartition = toothPosition.Attach(new Relation(
            GearStructure.PointAt(toothPosition, toothCountCarrier.ValueSlot.Tick),
            Relation.Point(toothPosition, 0)));
        var perimeterToToothPosition = new Relation(
            GearStructure.SegmentFromZeroTo(perimeter, numbers.TicksPerToothCarrier.ValueSlot.Tick),
            Relation.Segment(toothPosition, 0, 1));
        var perimeterToRotation = new Relation(
            GearStructure.SegmentFromZeroTo(perimeter, perimeterTicksCarrier.ValueSlot.Tick),
            Relation.Segment(rotations, 0, 1));
        var pitchCircleAttachment = new Relation(pitchCircleSource, Relation.Point(pitchCircle, 0));
        var rootCircleAttachment = new Relation(rootCircleSource, Relation.Point(rootCircle, 0));
        var tipCircleAttachment = new Relation(tipCircleSource, Relation.Point(tipCircle, 0));

        var structure = GearStructure.AttachStructure(
            toothCountCarrier,
            Relation.Point(toothCountCarrier, 0),
            Relation.Point(shapeCenter, 0),
            perimeterSource,
            pitchCircleSource,
            rootCircleSource,
            tipCircleSource,
            perimeterTicksOutput,
            diameterOutput,
            radiusOutput,
            rootRadiusOutput,
            tipRadiusOutput,
            teethToPerimeter,
            teethToDiameter,
            diameterToRadius,
            rootRadiusOperation,
            tipRadiusOperation,
            partition,
            toothPartition,
            perimeterToToothPosition,
            perimeterToRotation,
            pitchCircleAttachment,
            rootCircleAttachment,
            tipCircleAttachment);

        return new GearCarrier(
            toothCountCarrier,
            shapeCenter,
            perimeter,
            rotations,
            toothPosition,
            perimeterTicksCarrier,
            diameterTwentyFourths,
            radiusFortyEighths,
            rootRadiusFortyEighths,
            tipRadiusFortyEighths,
            pitchCircle,
            rootCircle,
            tipCircle,
            perimeterSource,
            pitchCircleSource,
            rootCircleSource,
            tipCircleSource,
            partition,
            toothPartition,
            perimeterToToothPosition,
            perimeterToRotation,
            teethToPerimeter,
            teethToDiameter,
            diameterToRadius,
            rootRadiusOperation,
            tipRadiusOperation,
            pitchCircleAttachment,
            rootCircleAttachment,
            tipCircleAttachment,
            structure);
    }
}

internal sealed record GearPair(
    GearCarrier Driver,
    GearCarrier Driven,
    GearNumbers Numbers,
    Carrier CenterLineSource,
    Carrier CenterLine,
    Carrier CenterDistance,
    Carrier CenterClearance,
    Carrier DriverMeshPhase,
    Carrier DrivenMeshPhase,
    Relation CenterDistanceOperation,
    Relation CenterLineSegment,
    Relation DrivenMeshPhaseOffset,
    Relation DriverInputToPerimeter,
    Relation Mesh,
    Relation Structure)
{
    public int CenterLineDistanceFortyEighths => GearDemoAdapters.ToInt(CenterDistance);

    public long DriverMeshPhaseTicks => GearDemoAdapters.ToLong(DriverMeshPhase);

    public long DrivenMeshPhaseTicks => GearDemoAdapters.ToLong(DrivenMeshPhase);

    public static GearPair Create(
        Carrier driverToothCount,
        Carrier drivenToothCount,
        Carrier driverInputTicks,
        GearNumbers numbers,
        long centerClearanceRadiusTicks)
    {
        var driver = GearCarrier.Create(driverToothCount, numbers);
        var driven = GearCarrier.Create(drivenToothCount, numbers);
        var centerLineSource = new Carrier();
        var centerLineSeed = Relation.Segment(centerLineSource, 0, 0);
        var centerLine = new Carrier(UnitShape.LinearFrom(centerLineSeed));
        var centerDistance = new Carrier();
        var centerClearance = new Carrier();
        var driverMeshPhase = new Carrier();
        var drivenMeshPhase = new Carrier();
        centerClearance.Set(centerClearanceRadiusTicks);
        driverMeshPhase.Set(0);

        var drivenMeshPhaseOffset = new Relation(
            Relation.Segment(numbers.HalfPitchTicks, 0, 1),
            Relation.Segment(drivenMeshPhase, 0, 1));
        var drivenMeshPhaseOutput = driverInputTicks.AttachLiveOutput(
            numbers.HalfPitchTicks.ValueSlot,
            drivenMeshPhaseOffset);

        var driverRadiusPort = new Relation(
            Relation.Point(driver.RadiusFortyEighthsCarrier, 1),
            Relation.Point(Carrier.Unknown, 1));
        var drivenRadiusPort = new Relation(
            Relation.Point(driven.RadiusFortyEighthsCarrier, 1),
            Relation.Point(Carrier.Unknown, 1));
        var clearancePort = new Relation(
            Relation.Point(centerClearance, 1),
            Relation.Point(Carrier.Unknown, 1));
        var centerDistanceOperation = new Relation(
            new Relation(new Relation(driverRadiusPort, drivenRadiusPort), clearancePort),
            Relation.Point(centerDistance, 1));
        var centerDistanceInput = new Relation(
            new Relation(driver.RadiusFortyEighthsCarrier.ValueSlot, driven.RadiusFortyEighthsCarrier.ValueSlot),
            centerClearance.ValueSlot);
        var centerDistanceOutput = driverInputTicks.AttachLiveOutput(
            centerDistanceInput,
            centerDistanceOperation);

        var centerLineSegment = GearStructure.SegmentFromZeroTo(centerLine, centerDistance.ValueSlot.Tick);
        var driverInputToPerimeter = new Relation(
            Relation.Segment(driverInputTicks, 0, 1),
            Relation.Segment(driver.Perimeter, 0, 1));
        var mesh = new Relation(
            Relation.Segment(driver.Perimeter, 0, 1),
            Relation.Segment(driven.Perimeter, 0, -1));
        var structure = GearStructure.AttachStructure(
            driverInputTicks,
            numbers.Structure,
            driver.Structure,
            driven.Structure,
            Relation.Point(driverInputTicks, 0),
            centerLineSeed,
            centerLineSegment,
            drivenMeshPhaseOutput,
            centerDistanceOutput,
            drivenMeshPhaseOffset,
            centerDistanceOperation,
            driverInputToPerimeter,
            mesh);

        return new GearPair(
            driver,
            driven,
            numbers,
            centerLineSource,
            centerLine,
            centerDistance,
            centerClearance,
            driverMeshPhase,
            drivenMeshPhase,
            centerDistanceOperation,
            centerLineSegment,
            drivenMeshPhaseOffset,
            driverInputToPerimeter,
            mesh,
            structure);
    }
}
