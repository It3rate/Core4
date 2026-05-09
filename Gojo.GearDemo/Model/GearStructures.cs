using Gojo.Core;
using Gojo.GearDemo.Adapters;

namespace Gojo.GearDemo.Model;

internal sealed record GearNumbers(
    Carrier TicksPerToothCarrier,
    Carrier DiametralPitch,
    Carrier RadiusTicksPerInch,
    Carrier ToothProfileRadiusTicks,
    Carrier HalfPitchTicks,
    Carrier QuarterPitchTicks,
    Relation DiametralPitchToRadiusTicks,
    Relation TicksPerToothToHalfPitch,
    Relation TicksPerToothToQuarterPitch)
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
        var (radiusTicks, _) = GearDemoAdapters.ReadProjection(diametralPitchToRadiusTicks.Project(diametralPitchCarrier.ValueSlot, radiusTicksPerInch));
        radiusTicksPerInch.Set(radiusTicks);

        var ticksPerToothToHalfPitch = new Relation(
            Relation.Segment(ticksPerToothCarrier, 0, 2),
            Relation.Segment(halfPitchTicks, 0, 1));
        var (halfPitch, _) = GearDemoAdapters.ReadProjection(ticksPerToothToHalfPitch.Project(ticksPerToothCarrier.ValueSlot, halfPitchTicks));
        halfPitchTicks.Set(halfPitch);

        var ticksPerToothToQuarterPitch = new Relation(
            Relation.Segment(ticksPerToothCarrier, 0, 4),
            Relation.Segment(quarterPitchTicks, 0, 1));
        var (quarterPitch, _) = GearDemoAdapters.ReadProjection(ticksPerToothToQuarterPitch.Project(ticksPerToothCarrier.ValueSlot, quarterPitchTicks));
        quarterPitchTicks.Set(quarterPitch);

        return new GearNumbers(
            ticksPerToothCarrier,
            diametralPitchCarrier,
            radiusTicksPerInch,
            toothProfileRadiusTicksCarrier,
            halfPitchTicks,
            quarterPitchTicks,
            diametralPitchToRadiusTicks,
            ticksPerToothToHalfPitch,
            ticksPerToothToQuarterPitch);
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
    Relation Partition,
    Relation PerimeterToToothPosition,
    Relation PerimeterToRotation,
    Relation TeethToPerimeter,
    Relation TeethToDiameter,
    Relation DiameterToRadius,
    Relation RootRadiusOperation,
    Relation TipRadiusOperation,
    Relation PitchCircleSource,
    Relation RootCircleSource,
    Relation TipCircleSource)
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
        var perimeter = new Carrier(UnitShape.CircularFrom(Relation.Point(shapeCenter, 0)));
        var rotations = new Carrier();
        var toothPositionCenter = new Carrier();
        var toothPosition = new Carrier(UnitShape.CircularFrom(Relation.Point(toothPositionCenter, 0)));
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
        if (!teethToPerimeter.TryReadOperationFrom(
                new Relation(toothCountCarrier.ValueSlot, numbers.TicksPerToothCarrier.ValueSlot),
                perimeterTicksCarrier,
                out var perimeterRead) ||
            perimeterRead.Left is not CarrierLocation perimeterLocation)
        {
            throw new InvalidOperationException("Gear perimeter operation did not produce a Gojo carrier location.");
        }

        perimeterTicksCarrier.Set(perimeterLocation.Ticks);
        var perimeterTicks = GearDemoAdapters.ToInt(perimeterTicksCarrier);
        var partition = perimeter.Attach(new Relation(
            Relation.Point(perimeter, perimeterTicks),
            Relation.Point(rotations, 0)));
        toothPosition.Attach(new Relation(
            Relation.Point(toothPosition, GearDemoAdapters.ToLong(toothCountCarrier)),
            Relation.Point(toothPosition, 0)));
        var perimeterToToothPosition = new Relation(
            Relation.Segment(perimeter, 0, numbers.TicksPerToothTicks),
            Relation.Segment(toothPosition, 0, 1));
        var perimeterToRotation = new Relation(
            Relation.Segment(perimeter, 0, perimeterTicks),
            Relation.Segment(rotations, 0, 1));
        var teethToDiameter = new Relation(
            Relation.Point(toothCountCarrier, 1),
            Relation.Point(diameterTwentyFourths, 1));
        var diameterToRadius = new Relation(
            Relation.Point(diameterTwentyFourths, 1),
            Relation.Point(radiusFortyEighths, 1));
        var (diameterTicks, _) = GearDemoAdapters.ReadProjection(teethToDiameter.Project(toothCountCarrier.ValueSlot, diameterTwentyFourths));
        diameterTwentyFourths.Set(diameterTicks);
        var (radiusTicks, _) = GearDemoAdapters.ReadProjection(diameterToRadius.Project(diameterTwentyFourths.ValueSlot, radiusFortyEighths));
        radiusFortyEighths.Set(radiusTicks);

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
        if (!tipRadiusOperation.TryReadOperationFrom(radiusInput, tipRadiusFortyEighths, out var tipRead) ||
            tipRead.Left is not CarrierLocation tipLocation)
        {
            throw new InvalidOperationException("Gear tip radius operation did not produce a Gojo carrier location.");
        }

        if (!rootRadiusOperation.TryReadOperationFrom(radiusInput, rootRadiusFortyEighths, out var rootRead) ||
            rootRead.Left is not CarrierLocation rootLocation)
        {
            throw new InvalidOperationException("Gear root radius operation did not produce a Gojo carrier location.");
        }

        tipRadiusFortyEighths.Set(tipLocation.Ticks);
        rootRadiusFortyEighths.Set(rootLocation.Ticks);
        var pitchCircleSource = Relation.Point(shapeCenter, GearDemoAdapters.ToLong(radiusFortyEighths));
        var rootCircleSource = Relation.Point(shapeCenter, GearDemoAdapters.ToLong(rootRadiusFortyEighths));
        var tipCircleSource = Relation.Point(shapeCenter, GearDemoAdapters.ToLong(tipRadiusFortyEighths));
        var pitchCircle = new Carrier(UnitShape.CircularFrom(pitchCircleSource));
        var rootCircle = new Carrier(UnitShape.CircularFrom(rootCircleSource));
        var tipCircle = new Carrier(UnitShape.CircularFrom(tipCircleSource));

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
            partition,
            perimeterToToothPosition,
            perimeterToRotation,
            teethToPerimeter,
            teethToDiameter,
            diameterToRadius,
            rootRadiusOperation,
            tipRadiusOperation,
            pitchCircleSource,
            rootCircleSource,
            tipCircleSource);
    }
}

internal sealed record GearPair(
    GearCarrier Driver,
    GearCarrier Driven,
    GearNumbers Numbers,
    Carrier CenterLine,
    Carrier CenterDistance,
    Carrier CenterClearance,
    Carrier DriverMeshPhase,
    Carrier DrivenMeshPhase,
    Relation CenterDistanceOperation,
    Relation CenterLineSegment,
    Relation DrivenMeshPhaseOffset,
    Relation DriverInputToPerimeter,
    Relation Mesh)
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
        var centerLine = new Carrier(UnitShape.LinearFrom(Relation.Segment(new Carrier(), 0, 0)));
        var centerDistance = new Carrier();
        var centerClearance = new Carrier();
        var driverMeshPhase = new Carrier();
        var drivenMeshPhase = new Carrier();
        centerClearance.Set(centerClearanceRadiusTicks);
        driverMeshPhase.Set(0);

        var drivenMeshPhaseOffset = new Relation(
            Relation.Segment(numbers.HalfPitchTicks, 0, 1),
            Relation.Segment(drivenMeshPhase, 0, 1));
        var (drivenPhaseTicks, _) = GearDemoAdapters.ReadProjection(drivenMeshPhaseOffset.Project(numbers.HalfPitchTicks.ValueSlot, drivenMeshPhase));
        drivenMeshPhase.Set(drivenPhaseTicks);

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
        if (!centerDistanceOperation.TryReadOperationFrom(centerDistanceInput, centerDistance, out var centerDistanceRead) ||
            centerDistanceRead.Left is not CarrierLocation centerDistanceTarget)
        {
            throw new InvalidOperationException("Center distance operation did not produce a Gojo carrier location.");
        }

        centerDistance.Set(centerDistanceTarget.Ticks);
        var drivenCenter = GearDemoAdapters.ToInt(centerDistance);
        var centerLineSegment = Relation.Segment(centerLine, 0, drivenCenter);
        var driverInputToPerimeter = new Relation(
            Relation.Segment(driverInputTicks, 0, 1),
            Relation.Segment(driver.Perimeter, 0, 1));
        var mesh = new Relation(
            Relation.Segment(driver.Perimeter, 0, 1),
            Relation.Segment(driven.Perimeter, 0, -1));

        return new GearPair(
            driver,
            driven,
            numbers,
            centerLine,
            centerDistance,
            centerClearance,
            driverMeshPhase,
            drivenMeshPhase,
            centerDistanceOperation,
            centerLineSegment,
            drivenMeshPhaseOffset,
            driverInputToPerimeter,
            mesh);
    }
}
