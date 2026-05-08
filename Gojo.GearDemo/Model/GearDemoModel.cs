using Gojo.Core;

namespace Gojo.GearDemo.Model;

internal sealed class GearDemoModel
{
    public const int TicksPerTooth = 8;

    private const int MinimumTeeth = 6;
    private const int MaximumTeeth = 48;
    private const int MaximumSpeedTicksPerSecond = 96;
    private const int ClockTicksPerSecond = 1000;
    private const int DiametralPitch = 24;
    private const int CenterClearanceRadiusTicks = 2;
    private const int ToothProfileRadiusTicks = 1;

    private readonly GearNumbers numbers;
    private readonly GojoSlider speedSlider;
    private readonly GojoSlider driverTeethSlider;
    private readonly GojoSlider drivenTeethSlider;
    private readonly Carrier driverInputTicks = new();
    private readonly Carrier clockDriveNumeratorResidue = new();
    private GearPair pair;
    private ClockRead lastClockRead;

    public GearDemoModel()
    {
        numbers = GearNumbers.Create(TicksPerTooth, DiametralPitch, ToothProfileRadiusTicks);
        speedSlider = GojoSlider.Create(0, MaximumSpeedTicksPerSecond, 3);
        driverTeethSlider = GojoSlider.Create(MinimumTeeth, MaximumTeeth, 24);
        drivenTeethSlider = GojoSlider.Create(MinimumTeeth, MaximumTeeth, 16);
        pair = GearPair.Create(driverTeethSlider.Output, drivenTeethSlider.Output, driverInputTicks, numbers);
    }

    public int DriverTeeth => pair.Driver.ToothCount;

    public int DrivenTeeth => pair.Driven.ToothCount;

    public void Drive(long ticks)
    {
        if (ticks <= 0)
        {
            return;
        }

        pair.Driver.Perimeter.Add(checked((ulong)ticks));
        driverInputTicks.Add(checked((ulong)ticks));
    }

    public void AdvanceClock(long elapsedMilliseconds)
    {
        if (elapsedMilliseconds <= 0)
        {
            return;
        }

        var speedTicks = checked((long)speedSlider.Output.Value);
        if (speedTicks <= 0)
        {
            clockDriveNumeratorResidue.Set(0);
            lastClockRead = new ClockRead(elapsedMilliseconds, 0, 0);
            return;
        }

        var frameMilliseconds = new Carrier();
        var driveTicks = new Carrier();
        frameMilliseconds.Set(elapsedMilliseconds);

        var clockProjection = new Relation(
            Relation.Segment(frameMilliseconds, 0, ClockTicksPerSecond),
            Relation.Segment(driveTicks, 0, speedTicks));
        var read = clockProjection.Project(frameMilliseconds.ValueSlot, driveTicks);
        var (frameTicks, frameNumeratorResidue) = ReadProjection(read);
        var numerator = checked(frameNumeratorResidue + checked((long)clockDriveNumeratorResidue.Value));
        var projectedTicks = checked(frameTicks + (numerator / ClockTicksPerSecond));
        var numeratorResidue = numerator % ClockTicksPerSecond;

        if (projectedTicks > 0)
        {
            Drive(projectedTicks);
        }

        clockDriveNumeratorResidue.Set(numeratorResidue);
        lastClockRead = new ClockRead(elapsedMilliseconds, projectedTicks, numeratorResidue);
    }

    public void Reset()
    {
        pair = GearPair.Create(driverTeethSlider.Output, drivenTeethSlider.Output, driverInputTicks, numbers);
        driverInputTicks.Set(0);
        clockDriveNumeratorResidue.Set(0);
        lastClockRead = default;
    }

    public void SetSpeedTicksPerSecond(int ticksPerSecond) =>
        speedSlider.SetOutput(ticksPerSecond);

    public void SetDriverTeeth(int toothCount)
    {
        driverTeethSlider.SetOutput(toothCount);
        RebuildPair();
    }

    public void SetDrivenTeeth(int toothCount)
    {
        drivenTeethSlider.SetOutput(toothCount);
        RebuildPair();
    }

    public GearDemoSnapshot Read()
    {
        var driverPerimeterRead = pair.DriverInputToPerimeter.Project(driverInputTicks.ValueSlot, pair.Driver.Perimeter);
        var (driverPerimeterTicks, _) = ReadProjection(driverPerimeterRead);
        var totalRead = pair.Mesh.Project(new CarrierLocation(pair.Driver.Perimeter, driverPerimeterTicks), pair.Driven.Perimeter);
        var visibleRead = pair.Mesh.Project(pair.Driver.Perimeter.ValueSlot, pair.Driven.Perimeter);
        var (projectedTicks, projectionResidue) = ReadProjection(totalRead);
        var (visibleProjectedTicks, visibleResidue) = ReadProjection(visibleRead);
        var (driverToothPosition, _) = ReadProjection(pair.Driver.PerimeterToToothPosition.Project(pair.Driver.Perimeter.ValueSlot, pair.Driver.ToothPosition));
        var drivenProjectedLocation = new CarrierLocation(pair.Driven.Perimeter, projectedTicks);
        var (drivenProjectedToothPosition, _) = ReadProjection(pair.Driven.PerimeterToToothPosition.Project(drivenProjectedLocation, pair.Driven.ToothPosition));
        var (drivenProjectedRotations, _) = ReadProjection(pair.Driven.PerimeterToRotation.Project(drivenProjectedLocation, pair.Driven.Rotations));
        var driverCircularToothPosition = ReadCircularValue(pair.Driver.ToothPosition, driverToothPosition, pair.Driver.ToothCount);
        var drivenCircularToothPosition = ReadCircularValue(pair.Driven.ToothPosition, drivenProjectedToothPosition, pair.Driven.ToothCount);

        return new GearDemoSnapshot(
            speedSlider.Read(),
            driverTeethSlider.Read(),
            drivenTeethSlider.Read(),
            lastClockRead.ElapsedMilliseconds,
            lastClockRead.ProjectedDriveTicks,
            lastClockRead.DriveNumeratorResidue,
            pair.Driver.ToothCount,
            pair.Driven.ToothCount,
            pair.Driver.PerimeterTicks,
            pair.Driven.PerimeterTicks,
            checked((long)driverInputTicks.Value),
            pair.Driver.Perimeter.Value,
            driverCircularToothPosition,
            pair.Driver.Perimeter.Tension,
            pair.Driver.Rotations.Value,
            projectedTicks,
            drivenCircularToothPosition,
            drivenProjectedRotations,
            projectionResidue,
            visibleProjectedTicks,
            visibleResidue,
            pair.Driven.Perimeter.Value,
            pair.Driven.Perimeter.Tension,
            pair.Driven.Rotations.Value,
            pair.Driver.DiameterTwentyFourths,
            pair.Driven.DiameterTwentyFourths,
            pair.Driver.RadiusFortyEighths,
            pair.Driven.RadiusFortyEighths,
            pair.Driver.RootRadiusFortyEighths,
            pair.Driven.RootRadiusFortyEighths,
            pair.Driver.TipRadiusFortyEighths,
            pair.Driven.TipRadiusFortyEighths,
            pair.CenterLineDistanceFortyEighths,
            pair.Numbers.DiametralPitchTicks,
            pair.Numbers.RadiusTicksPerInchTicks,
            pair.Numbers.ToothProfileRadiusTicksValue,
            pair.Numbers.HalfPitchTicksValue,
            pair.Numbers.QuarterPitchTicksValue,
            pair.DriverMeshPhaseTicks,
            pair.DrivenMeshPhaseTicks,
            pair.CenterLineSegment,
            MeshLabel(pair.Numbers.HalfPitchTicksValue));
    }

    private void RebuildPair()
    {
        var ticks = checked((long)driverInputTicks.Value);
        pair = GearPair.Create(driverTeethSlider.Output, drivenTeethSlider.Output, driverInputTicks, numbers);
        pair.Driver.Perimeter.Add(checked((ulong)ticks));
    }

    private static (long ProjectedTicks, long ResidueTicks) ReadProjection(Relation read)
    {
        if (read.TryGetCarrierLocations(out var projected, out var residue))
        {
            return (projected.Ticks, residue.Ticks);
        }

        return (0, 0);
    }

    private static int ReadCircularValue(Carrier circularPrototype, long ticks, int cycleTicks)
    {
        if (cycleTicks <= 0)
        {
            return 0;
        }

        var circular = new Carrier(circularPrototype.UnitShape);
        circular.Attach(new Relation(
            Relation.Point(circular, cycleTicks),
            Relation.Point(circular, 0)));
        var positiveTicks = ticks;
        if (positiveTicks < 0)
        {
            var cycles = checked((-positiveTicks / cycleTicks) + 1);
            positiveTicks = checked(positiveTicks + checked(cycles * cycleTicks));
        }

        circular.Add(checked((ulong)positiveTicks));
        return checked((int)circular.Value);
    }

    private static string MeshLabel(int drivenPhaseTicks) =>
        $"{TicksPerTooth} ticks/tooth, driven phase +{drivenPhaseTicks}";

    private sealed record GearNumbers(
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
        public int TicksPerToothTicks => checked((int)TicksPerToothCarrier.Value);

        public int DiametralPitchTicks => checked((int)DiametralPitch.Value);

        public int RadiusTicksPerInchTicks => checked((int)RadiusTicksPerInch.Value);

        public int ToothProfileRadiusTicksValue => checked((int)ToothProfileRadiusTicks.Value);

        public int HalfPitchTicksValue => checked((int)HalfPitchTicks.Value);

        public int QuarterPitchTicksValue => checked((int)QuarterPitchTicks.Value);

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
            var (radiusTicks, _) = ReadProjection(diametralPitchToRadiusTicks.Project(diametralPitchCarrier.ValueSlot, radiusTicksPerInch));
            radiusTicksPerInch.Set(radiusTicks);

            var ticksPerToothToHalfPitch = new Relation(
                Relation.Segment(ticksPerToothCarrier, 0, 2),
                Relation.Segment(halfPitchTicks, 0, 1));
            var (halfPitch, _) = ReadProjection(ticksPerToothToHalfPitch.Project(ticksPerToothCarrier.ValueSlot, halfPitchTicks));
            halfPitchTicks.Set(halfPitch);

            var ticksPerToothToQuarterPitch = new Relation(
                Relation.Segment(ticksPerToothCarrier, 0, 4),
                Relation.Segment(quarterPitchTicks, 0, 1));
            var (quarterPitch, _) = ReadProjection(ticksPerToothToQuarterPitch.Project(ticksPerToothCarrier.ValueSlot, quarterPitchTicks));
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

    private sealed class GojoSlider
    {
        private GojoSlider(int minimum, int maximum)
        {
            Minimum = new Carrier();
            Maximum = new Carrier();
            Range = new Carrier();
            Position = new Carrier();
            Output = new Carrier();
            Minimum.Set(minimum);
            Maximum.Set(maximum);
            var maximumPort = new Relation(
                Relation.Point(Maximum, 1),
                Relation.Point(Carrier.Unknown, 1));
            var minimumPort = new Relation(
                Relation.Point(Minimum, -1),
                Relation.Point(Carrier.Unknown, 1));
            RangeOperation = new Relation(
                new Relation(maximumPort, minimumPort),
                Relation.Point(Range, 1));
            ReadRange();
            Position.Attach(new Relation(
                Relation.Point(Position, RangeTicks),
                Relation.Point(Position, RangeTicks)));
            Projection = new Relation(
                Relation.Segment(Position, 0, RangeTicks),
                Relation.Segment(Output, MinimumTicks, MaximumTicks));
        }

        public Carrier Minimum { get; }

        public Carrier Maximum { get; }

        public Carrier Range { get; }

        public Carrier Position { get; }

        public Carrier Output { get; }

        public Relation Projection { get; }

        public Relation RangeOperation { get; }

        public long MinimumTicks => checked((long)Minimum.Value);

        public long MaximumTicks => checked((long)Maximum.Value);

        public long RangeTicks => checked((long)Range.Value);

        public static GojoSlider Create(int minimum, int maximum, int value)
        {
            var slider = new GojoSlider(minimum, maximum);
            slider.SetOutput(value);
            return slider;
        }

        public void SetOutput(int value)
        {
            var clamped = Math.Clamp((long)value, MinimumTicks, MaximumTicks);
            var projected = Projection.Project(new CarrierLocation(Output, clamped), Position);
            var (position, _) = ReadProjection(projected);
            Position.Set(position);
            ProjectOutput();
        }

        public GojoSliderSnapshot Read()
        {
            ProjectOutput();
            return new GojoSliderSnapshot(
                MinimumTicks,
                MaximumTicks,
                RangeTicks,
                checked((long)Position.Value),
                checked((long)Output.Value),
                Position.Tension,
                RangeOperation,
                Projection);
        }

        private void ReadRange()
        {
            if (!RangeOperation.TryReadOperationFrom(
                    new Relation(Maximum.ValueSlot, Minimum.ValueSlot),
                    Range,
                    out var rangeRead) ||
                rangeRead.Left is not CarrierLocation rangeLocation)
            {
                throw new InvalidOperationException("Slider range operation did not produce a Gojo carrier location.");
            }

            Range.Set(rangeLocation.Ticks);
        }

        private void ProjectOutput()
        {
            var projected = Projection.Project(Position.ValueSlot, Output);
            var (output, _) = ReadProjection(projected);
            Output.Set(output);
        }
    }

    private sealed record GearCarrier(
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
        public int ToothCount => checked((int)ToothCountCarrier.Value);

        public int PerimeterTicks => checked((int)PerimeterTicksCarrier.Value);

        public int DiameterTwentyFourths => checked((int)DiameterTwentyFourthsCarrier.Value);

        public int RadiusFortyEighths => checked((int)RadiusFortyEighthsCarrier.Value);

        public int RootRadiusFortyEighths => checked((int)RootRadiusFortyEighthsCarrier.Value);

        public int TipRadiusFortyEighths => checked((int)TipRadiusFortyEighthsCarrier.Value);

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
            var perimeterTicks = checked((int)perimeterTicksCarrier.Value);
            var partition = perimeter.Attach(new Relation(
                Relation.Point(perimeter, perimeterTicks),
                Relation.Point(rotations, 0)));
            toothPosition.Attach(new Relation(
                Relation.Point(toothPosition, checked((long)toothCountCarrier.Value)),
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
            var (diameterTicks, _) = ReadProjection(teethToDiameter.Project(toothCountCarrier.ValueSlot, diameterTwentyFourths));
            diameterTwentyFourths.Set(diameterTicks);
            var (radiusTicks, _) = ReadProjection(diameterToRadius.Project(diameterTwentyFourths.ValueSlot, radiusFortyEighths));
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
            var pitchCircleSource = Relation.Point(shapeCenter, checked((long)radiusFortyEighths.Value));
            var rootCircleSource = Relation.Point(shapeCenter, checked((long)rootRadiusFortyEighths.Value));
            var tipCircleSource = Relation.Point(shapeCenter, checked((long)tipRadiusFortyEighths.Value));
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

    private sealed record GearPair(
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
        public int CenterLineDistanceFortyEighths => checked((int)CenterDistance.Value);

        public long DriverMeshPhaseTicks => checked((long)DriverMeshPhase.Value);

        public long DrivenMeshPhaseTicks => checked((long)DrivenMeshPhase.Value);

        public static GearPair Create(Carrier driverToothCount, Carrier drivenToothCount, Carrier driverInputTicks, GearNumbers numbers)
        {
            var driver = GearCarrier.Create(driverToothCount, numbers);
            var driven = GearCarrier.Create(drivenToothCount, numbers);
            var centerLine = new Carrier(UnitShape.LinearFrom(Relation.Segment(new Carrier(), 0, 0)));
            var centerDistance = new Carrier();
            var centerClearance = new Carrier();
            var driverMeshPhase = new Carrier();
            var drivenMeshPhase = new Carrier();
            centerClearance.Set(CenterClearanceRadiusTicks);
            driverMeshPhase.Set(0);
            var drivenMeshPhaseOffset = new Relation(
                Relation.Segment(numbers.HalfPitchTicks, 0, 1),
                Relation.Segment(drivenMeshPhase, 0, 1));
            var (drivenPhaseTicks, _) = ReadProjection(drivenMeshPhaseOffset.Project(numbers.HalfPitchTicks.ValueSlot, drivenMeshPhase));
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
            var drivenCenter = checked((int)centerDistance.Value);
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

    private readonly record struct ClockRead(
        long ElapsedMilliseconds,
        long ProjectedDriveTicks,
        long DriveNumeratorResidue);
}

internal readonly record struct GojoSliderSnapshot(
    long Minimum,
    long Maximum,
    long Range,
    long PositionTicks,
    long ValueTicks,
    long PositionTension,
    Relation RangeOperation,
    Relation Projection);

internal readonly record struct GearDemoSnapshot(
    GojoSliderSnapshot SpeedSlider,
    GojoSliderSnapshot DriverTeethSlider,
    GojoSliderSnapshot DrivenTeethSlider,
    long ClockFrameMilliseconds,
    long ClockProjectedDriveTicks,
    long ClockDriveNumeratorResidue,
    int DriverTeeth,
    int DrivenTeeth,
    int DriverPerimeterTicks,
    int DrivenPerimeterTicks,
    long DriverInputTicks,
    ulong DriverValue,
    int DriverToothPosition,
    long DriverTension,
    ulong DriverRotations,
    long DrivenProjectedTicks,
    int DrivenProjectedToothPosition,
    long DrivenProjectedRotations,
    long ProjectionResidueTicks,
    long VisibleDrivenProjectedTicks,
    long VisibleProjectionResidueTicks,
    ulong DrivenValue,
    long DrivenTension,
    ulong DrivenRotations,
    int DriverDiameterTwentyFourths,
    int DrivenDiameterTwentyFourths,
    int DriverRadiusFortyEighths,
    int DrivenRadiusFortyEighths,
    int DriverRootRadiusFortyEighths,
    int DrivenRootRadiusFortyEighths,
    int DriverTipRadiusFortyEighths,
    int DrivenTipRadiusFortyEighths,
    int CenterLineDistanceFortyEighths,
    int DiametralPitchTicks,
    int RadiusTicksPerInchTicks,
    int ToothProfileRadiusTicks,
    int HalfPitchTicks,
    int QuarterPitchTicks,
    long DriverMeshPhaseTicks,
    long DrivenMeshPhaseTicks,
    Relation CenterLineSegment,
    string MeshLabel)
{
    public const int TicksPerTooth = GearDemoModel.TicksPerTooth;

    public int SpeedTicksPerSecond => checked((int)SpeedSlider.ValueTicks);

    public double DriverTurns => DriverPerimeterTicks == 0 ? 0d : DriverInputTicks / (double)DriverPerimeterTicks;

    public double DrivenProjectedTurns => DrivenPerimeterTicks == 0 ? 0d : DrivenProjectedTicks / (double)DrivenPerimeterTicks;

    public double VisibleDrivenTurns => DrivenPerimeterTicks == 0 ? 0d : VisibleDrivenProjectedTicks / (double)DrivenPerimeterTicks;

    public double DriverDiameterInches => DriverDiameterTwentyFourths / (double)DiametralPitchTicks;

    public double DrivenDiameterInches => DrivenDiameterTwentyFourths / (double)DiametralPitchTicks;

    public double DriverRadiusInches => DriverRadiusFortyEighths / (double)RadiusTicksPerInchTicks;

    public double DrivenRadiusInches => DrivenRadiusFortyEighths / (double)RadiusTicksPerInchTicks;

    public double DriverRootRadiusInches => DriverRootRadiusFortyEighths / (double)RadiusTicksPerInchTicks;

    public double DrivenRootRadiusInches => DrivenRootRadiusFortyEighths / (double)RadiusTicksPerInchTicks;

    public double DriverTipRadiusInches => DriverTipRadiusFortyEighths / (double)RadiusTicksPerInchTicks;

    public double DrivenTipRadiusInches => DrivenTipRadiusFortyEighths / (double)RadiusTicksPerInchTicks;

    public double CenterLineDistanceInches => CenterLineDistanceFortyEighths / (double)RadiusTicksPerInchTicks;

    public double ToothProfileInches => ToothProfileRadiusTicks / (double)RadiusTicksPerInchTicks;

    public string DriverDiameterText => FractionalInches(DriverDiameterTwentyFourths, DiametralPitchTicks);

    public string DrivenDiameterText => FractionalInches(DrivenDiameterTwentyFourths, DiametralPitchTicks);

    public string ClockResidueText => FractionalTicks(ClockDriveNumeratorResidue, 1000, "tick");

    private static string FractionalInches(int numerator, int denominator) =>
        FractionalTicks(numerator, denominator, "in");

    private static string FractionalTicks(long numerator, int denominator, string suffix)
    {
        var sign = numerator < 0 ? "-" : string.Empty;
        var magnitude = Math.Abs(numerator);
        var whole = magnitude / denominator;
        var remainder = magnitude % denominator;
        if (remainder == 0)
        {
            return $"{sign}{whole} {suffix}";
        }

        var divisor = GreatestCommonDivisor(checked((int)remainder), denominator);
        var fraction = $"{remainder / divisor}/{denominator / divisor}";
        return whole == 0
            ? $"{sign}{fraction} {suffix}"
            : $"{sign}{whole} {fraction} {suffix}";
    }

    private static int GreatestCommonDivisor(int left, int right)
    {
        left = Math.Abs(left);
        right = Math.Abs(right);
        while (right != 0)
        {
            var next = left % right;
            left = right;
            right = next;
        }

        return left == 0 ? 1 : left;
    }
}
