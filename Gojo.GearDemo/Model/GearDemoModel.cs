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
    private const int RadiusTicksPerInch = DiametralPitch * 2;
    private const int CenterClearanceRadiusTicks = 2;
    private const int ToothProfileRadiusTicks = 1;

    private readonly GojoSlider speedSlider = GojoSlider.Create(0, MaximumSpeedTicksPerSecond, 3);
    private readonly GojoSlider driverTeethSlider = GojoSlider.Create(MinimumTeeth, MaximumTeeth, 24);
    private readonly GojoSlider drivenTeethSlider = GojoSlider.Create(MinimumTeeth, MaximumTeeth, 16);
    private readonly Carrier driverInputTicks = new();
    private readonly Carrier clockDriveNumeratorResidue = new();
    private GearPair pair;
    private ClockRead lastClockRead;

    public GearDemoModel()
    {
        pair = GearPair.Create(driverTeethSlider.Output, drivenTeethSlider.Output, driverInputTicks);
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
        pair = GearPair.Create(driverTeethSlider.Output, drivenTeethSlider.Output, driverInputTicks);
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
            pair.Driver.Perimeter.Tension,
            pair.Driver.Rotations.Value,
            projectedTicks,
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
            pair.CenterLineDistanceFortyEighths,
            pair.CenterLineSegment,
            MeshLabel());
    }

    private void RebuildPair()
    {
        var ticks = checked((long)driverInputTicks.Value);
        pair = GearPair.Create(driverTeethSlider.Output, drivenTeethSlider.Output, driverInputTicks);
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

    private static string MeshLabel() =>
        $"{TicksPerTooth} ticks/tooth, driver tick -> -driven tick";

    private sealed class GojoSlider
    {
        private GojoSlider(int minimum, int maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
            Range = checked(maximum - minimum);
            Position = new Carrier();
            Output = new Carrier();
            Position.Attach(new Relation(
                Relation.Point(Position, Range),
                Relation.Point(Position, Range)));
            Projection = new Relation(
                Relation.Segment(Position, 0, Range),
                Relation.Segment(Output, minimum, maximum));
        }

        public int Minimum { get; }

        public int Maximum { get; }

        public int Range { get; }

        public Carrier Position { get; }

        public Carrier Output { get; }

        public Relation Projection { get; }

        public static GojoSlider Create(int minimum, int maximum, int value)
        {
            var slider = new GojoSlider(minimum, maximum);
            slider.SetOutput(value);
            return slider;
        }

        public void SetOutput(int value)
        {
            var clamped = Math.Clamp(value, Minimum, Maximum);
            var projected = Projection.Project(new CarrierLocation(Output, clamped), Position);
            var (position, _) = ReadProjection(projected);
            Position.Set(position);
            ProjectOutput();
        }

        public GojoSliderSnapshot Read()
        {
            ProjectOutput();
            return new GojoSliderSnapshot(
                Minimum,
                Maximum,
                Range,
                checked((long)Position.Value),
                checked((long)Output.Value),
                Position.Tension,
                Projection);
        }

        private void ProjectOutput()
        {
            var projected = Projection.Project(Position.ValueSlot, Output);
            var (output, _) = ReadProjection(projected);
            Output.Set(output);
        }
    }

    private sealed record GearCarrier(
        int ToothCount,
        int PerimeterTicks,
        Carrier ToothCountCarrier,
        Carrier ShapeCenter,
        Carrier Perimeter,
        Carrier Rotations,
        Carrier DiameterTwentyFourthsCarrier,
        Carrier RadiusFortyEighthsCarrier,
        int DiameterTwentyFourths,
        int RadiusFortyEighths,
        Relation Partition,
        Relation TeethToDiameter,
        Relation DiameterToRadius)
    {
        public static GearCarrier Create(Carrier toothCountCarrier)
        {
            var toothCount = checked((int)toothCountCarrier.Value);
            var perimeterTicks = checked(toothCount * TicksPerTooth);
            var shapeCenter = new Carrier();
            var perimeter = new Carrier(UnitShape.CircularFrom(Relation.Point(shapeCenter, 0)));
            var rotations = new Carrier();
            var diameterTwentyFourths = new Carrier();
            var radiusFortyEighths = new Carrier();
            var partition = perimeter.Attach(new Relation(
                Relation.Point(perimeter, perimeterTicks),
                Relation.Point(rotations, 0)));
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

            return new GearCarrier(
                toothCount,
                perimeterTicks,
                toothCountCarrier,
                shapeCenter,
                perimeter,
                rotations,
                diameterTwentyFourths,
                radiusFortyEighths,
                checked((int)diameterTwentyFourths.Value),
                checked((int)radiusFortyEighths.Value),
                partition,
                teethToDiameter,
                diameterToRadius);
        }
    }

    private sealed record GearPair(
        GearCarrier Driver,
        GearCarrier Driven,
        Carrier CenterLine,
        Carrier CenterDistance,
        int DriverCenterFortyEighths,
        int DrivenCenterFortyEighths,
        Carrier CenterClearance,
        Relation CenterDistanceOperation,
        Relation CenterLineSegment,
        Relation DriverInputToPerimeter,
        Relation Mesh)
    {
        public int CenterLineDistanceFortyEighths => DrivenCenterFortyEighths - DriverCenterFortyEighths;

        public static GearPair Create(Carrier driverToothCount, Carrier drivenToothCount, Carrier driverInputTicks)
        {
            var driver = GearCarrier.Create(driverToothCount);
            var driven = GearCarrier.Create(drivenToothCount);
            var centerLine = new Carrier(UnitShape.LinearFrom(Relation.Segment(new Carrier(), 0, 0)));
            var centerDistance = new Carrier();
            var centerClearance = new Carrier();
            centerClearance.Set(CenterClearanceRadiusTicks);
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
                centerLine,
                centerDistance,
                0,
                drivenCenter,
                centerClearance,
                centerDistanceOperation,
                centerLineSegment,
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
    int Minimum,
    int Maximum,
    int Range,
    long PositionTicks,
    long ValueTicks,
    long PositionTension,
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
    long DriverTension,
    ulong DriverRotations,
    long DrivenProjectedTicks,
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
    int CenterLineDistanceFortyEighths,
    Relation CenterLineSegment,
    string MeshLabel)
{
    private const int DiametralPitch = 24;
    private const int RadiusTicksPerInch = DiametralPitch * 2;
    public const int TicksPerTooth = GearDemoModel.TicksPerTooth;
    public const int ToothProfileRadiusTicks = 1;

    public int SpeedTicksPerSecond => checked((int)SpeedSlider.ValueTicks);

    public int DriverToothPosition => PositiveModulo(checked((long)DriverValue / TicksPerTooth), DriverTeeth);

    public int DrivenProjectedToothPosition => PositiveModulo(DrivenProjectedTicks / TicksPerTooth, DrivenTeeth);

    public long DrivenProjectedRotations => DrivenProjectedTicks / DrivenPerimeterTicks;

    public double DriverTurns => DriverPerimeterTicks == 0 ? 0d : DriverInputTicks / (double)DriverPerimeterTicks;

    public double DrivenProjectedTurns => DrivenPerimeterTicks == 0 ? 0d : DrivenProjectedTicks / (double)DrivenPerimeterTicks;

    public double VisibleDrivenTurns => DrivenPerimeterTicks == 0 ? 0d : VisibleDrivenProjectedTicks / (double)DrivenPerimeterTicks;

    public double DriverDiameterInches => DriverDiameterTwentyFourths / (double)DiametralPitch;

    public double DrivenDiameterInches => DrivenDiameterTwentyFourths / (double)DiametralPitch;

    public double DriverRadiusInches => DriverRadiusFortyEighths / (double)RadiusTicksPerInch;

    public double DrivenRadiusInches => DrivenRadiusFortyEighths / (double)RadiusTicksPerInch;

    public double CenterLineDistanceInches => CenterLineDistanceFortyEighths / (double)RadiusTicksPerInch;

    public double ToothProfileInches => ToothProfileRadiusTicks / (double)RadiusTicksPerInch;

    public string DriverDiameterText => FractionalInches(DriverDiameterTwentyFourths, DiametralPitch);

    public string DrivenDiameterText => FractionalInches(DrivenDiameterTwentyFourths, DiametralPitch);

    public string ClockResidueText => FractionalTicks(ClockDriveNumeratorResidue, 1000, "tick");

    private static int PositiveModulo(long value, int modulus)
    {
        var remainder = value % modulus;
        return (int)(remainder < 0 ? remainder + modulus : remainder);
    }

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
