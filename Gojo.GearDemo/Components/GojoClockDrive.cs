using Gojo.Core;
using Gojo.GearDemo.Adapters;

namespace Gojo.GearDemo.Components;

internal sealed class GojoClockDrive
{
    private readonly Carrier clockTicksPerSecond = new();
    private readonly Carrier driveNumeratorResidue = new();
    private ClockDriveSnapshot lastRead;

    public GojoClockDrive(int ticksPerSecondResolution)
    {
        clockTicksPerSecond.Set(ticksPerSecondResolution);
    }

    public ClockDriveSnapshot Read() => lastRead;

    public void Reset()
    {
        driveNumeratorResidue.Set(0);
        lastRead = default;
    }

    public void Advance(long elapsedMilliseconds, Carrier speedTicksPerSecond, Action<long> drive)
    {
        if (elapsedMilliseconds <= 0)
        {
            return;
        }

        var speedTicks = GearDemoAdapters.ToLong(speedTicksPerSecond);
        if (speedTicks <= 0)
        {
            driveNumeratorResidue.Set(0);
            lastRead = new ClockDriveSnapshot(elapsedMilliseconds, 0, 0);
            return;
        }

        var frameMilliseconds = new Carrier();
        var driveTicks = new Carrier();
        frameMilliseconds.Set(elapsedMilliseconds);

        var resolutionTicks = GearDemoAdapters.ToLong(clockTicksPerSecond);
        var clockProjection = new Relation(
            Relation.Segment(frameMilliseconds, 0, resolutionTicks),
            Relation.Segment(driveTicks, 0, speedTicks));
        var read = clockProjection.Project(frameMilliseconds.ValueSlot, driveTicks);
        var (frameTicks, frameNumeratorResidue) = GearDemoAdapters.ReadProjection(read);
        var numerator = checked(frameNumeratorResidue + GearDemoAdapters.ToLong(driveNumeratorResidue));
        var projectedTicks = checked(frameTicks + (numerator / resolutionTicks));
        var numeratorResidue = numerator % resolutionTicks;

        if (projectedTicks > 0)
        {
            drive(projectedTicks);
        }

        driveNumeratorResidue.Set(numeratorResidue);
        lastRead = new ClockDriveSnapshot(elapsedMilliseconds, projectedTicks, numeratorResidue);
    }
}

internal readonly record struct ClockDriveSnapshot(
    long ElapsedMilliseconds,
    long ProjectedDriveTicks,
    long DriveNumeratorResidue);
