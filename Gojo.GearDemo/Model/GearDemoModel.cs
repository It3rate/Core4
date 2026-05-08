using Gojo.Core;

namespace Gojo.GearDemo.Model;

internal sealed class GearDemoModel
{
    public const int TicksPerTooth = 8;

    private GearPair pair;
    private long driverInputTicks;

    public GearDemoModel()
    {
        pair = GearPair.Create(driverTeeth: 24, drivenTeeth: 16);
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
        driverInputTicks = checked(driverInputTicks + ticks);
    }

    public void Reset()
    {
        pair = GearPair.Create(DriverTeeth, DrivenTeeth);
        driverInputTicks = 0;
    }

    public void SetDriverTeeth(int toothCount) =>
        Rebuild(Math.Clamp(toothCount, 6, 48), DrivenTeeth);

    public void SetDrivenTeeth(int toothCount) =>
        Rebuild(DriverTeeth, Math.Clamp(toothCount, 6, 48));

    public GearDemoSnapshot Read()
    {
        var totalRead = pair.Mesh.Project(new CarrierLocation(pair.Driver.Perimeter, driverInputTicks), pair.Driven.Perimeter);
        var visibleRead = pair.Mesh.Project(pair.Driver.Perimeter.ValueSlot, pair.Driven.Perimeter);
        var (projectedTicks, projectionResidue) = ReadProjection(totalRead);
        var (visibleProjectedTicks, visibleResidue) = ReadProjection(visibleRead);

        return new GearDemoSnapshot(
            pair.Driver.ToothCount,
            pair.Driven.ToothCount,
            driverInputTicks,
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
            MeshLabel(pair.Driver.ToothCount, pair.Driven.ToothCount));
    }

    private void Rebuild(int driverTeeth, int drivenTeeth)
    {
        var ticks = driverInputTicks;
        pair = GearPair.Create(driverTeeth, drivenTeeth);
        driverInputTicks = 0;
        Drive(ticks);
    }

    private static (long ProjectedTicks, long ResidueTicks) ReadProjection(Relation read)
    {
        if (read.TryGetCarrierLocations(out var projected, out var residue))
        {
            return (projected.Ticks, residue.Ticks);
        }

        return (0, 0);
    }

    private static string MeshLabel(int driverTeeth, int drivenTeeth) =>
        $"{TicksPerTooth} ticks/tooth, driver tick -> -driven tick";

    private sealed record GearCarrier(
        int ToothCount,
        int PerimeterTicks,
        Carrier Center,
        Carrier Perimeter,
        Carrier Rotations,
        Relation Partition)
    {
        public static GearCarrier Create(int toothCount)
        {
            var perimeterTicks = checked(toothCount * TicksPerTooth);
            var center = new Carrier();
            var perimeter = new Carrier(UnitShape.CircularFrom(Relation.Point(center, 0)));
            var rotations = new Carrier();
            var partition = perimeter.Attach(new Relation(
                Relation.Point(perimeter, perimeterTicks),
                Relation.Point(rotations, 0)));

            return new GearCarrier(toothCount, perimeterTicks, center, perimeter, rotations, partition);
        }
    }

    private sealed record GearPair(GearCarrier Driver, GearCarrier Driven, Relation Mesh)
    {
        public static GearPair Create(int driverTeeth, int drivenTeeth)
        {
            var driver = GearCarrier.Create(driverTeeth);
            var driven = GearCarrier.Create(drivenTeeth);
            var mesh = new Relation(
                Relation.Segment(driver.Perimeter, 0, 1),
                Relation.Segment(driven.Perimeter, 0, -1));

            return new GearPair(driver, driven, mesh);
        }
    }
}

internal readonly record struct GearDemoSnapshot(
    int DriverTeeth,
    int DrivenTeeth,
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
    string MeshLabel)
{
    private const int DiametralPitch = 24;
    public const int TicksPerTooth = GearDemoModel.TicksPerTooth;

    public int DriverPerimeterTicks => checked(DriverTeeth * TicksPerTooth);

    public int DrivenPerimeterTicks => checked(DrivenTeeth * TicksPerTooth);

    public int DriverToothPosition => PositiveModulo(checked((long)DriverValue / TicksPerTooth), DriverTeeth);

    public int DrivenProjectedToothPosition => PositiveModulo(DrivenProjectedTicks / TicksPerTooth, DrivenTeeth);

    public long DrivenProjectedRotations => DrivenProjectedTicks / DrivenPerimeterTicks;

    public double DriverTurns => DriverPerimeterTicks == 0 ? 0d : DriverInputTicks / (double)DriverPerimeterTicks;

    public double DrivenProjectedTurns => DrivenPerimeterTicks == 0 ? 0d : DrivenProjectedTicks / (double)DrivenPerimeterTicks;

    public double VisibleDrivenTurns => DrivenPerimeterTicks == 0 ? 0d : VisibleDrivenProjectedTicks / (double)DrivenPerimeterTicks;

    public double DriverDiameterInches => DriverTeeth / (double)DiametralPitch;

    public double DrivenDiameterInches => DrivenTeeth / (double)DiametralPitch;

    public string DriverDiameterText => FractionalInches(DriverTeeth, DiametralPitch);

    public string DrivenDiameterText => FractionalInches(DrivenTeeth, DiametralPitch);

    private static int PositiveModulo(long value, int modulus)
    {
        var remainder = value % modulus;
        return (int)(remainder < 0 ? remainder + modulus : remainder);
    }

    private static string FractionalInches(int numerator, int denominator)
    {
        var whole = numerator / denominator;
        var remainder = numerator % denominator;
        if (remainder == 0)
        {
            return $"{whole} in";
        }

        var divisor = GreatestCommonDivisor(remainder, denominator);
        var fraction = $"{remainder / divisor}/{denominator / divisor}";
        return whole == 0
            ? $"{fraction} in"
            : $"{whole} {fraction} in";
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
