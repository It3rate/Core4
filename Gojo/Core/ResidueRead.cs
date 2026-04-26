namespace Gojo.Core;

public readonly record struct ResidueRead(ulong WholeTicks, ulong ResidueTicks, ulong BoundaryTicks)
{
    public static ResidueRead FromTicks(ulong ticks, ulong boundaryTicks)
    {
        ArgumentOutOfRangeException.ThrowIfZero(boundaryTicks);
        return new(ticks / boundaryTicks, ticks % boundaryTicks, boundaryTicks);
    }

    public bool HasResidue => ResidueTicks != 0;

    public override string ToString() =>
        HasResidue ? $"{WholeTicks} {ResidueTicks}|{BoundaryTicks}" : WholeTicks.ToString();
}
