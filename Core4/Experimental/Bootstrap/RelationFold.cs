namespace Core4.Experimental.Bootstrap;

public static class RelationFold
{
    public static (long Numerator, long Denominator) Fold(IElement left, IElement right)
    {
        if (left is Measure leftMeasure && right is Measure rightMeasure)
        {
            return FoldMeasures(leftMeasure, rightMeasure);
        }

        return Smear(Read(left), Read(right));
    }

    private static (long Numerator, long Denominator) Read(IElement element) =>
        element is Relation relation
            ? Fold(relation.Left, relation.Right)
            : (element is Measure measure ? (measure.Raw, 0) : (0, 0));

    private static (long Numerator, long Denominator) FoldMeasures(Measure left, Measure right)
    {
        var extent = right.Raw - left.Raw;
        var direction = Math.Sign(extent);
        var partitions = Math.Abs(extent);
        long filled = 0;

        for (long tick = 0; tick < partitions; tick++)
        {
            filled += direction;
        }

        return (filled, extent);
    }

    private static (long Numerator, long Denominator) Smear(
        (long Numerator, long Denominator) left,
        (long Numerator, long Denominator) right)
    {
        var leftPartitionCount = PartitionCountOf(left);
        var rightPartitionCount = PartitionCountOf(right);
        var host = leftPartitionCount >= rightPartitionCount ? left : right;
        var partner = leftPartitionCount >= rightPartitionCount ? right : left;
        var hostPartitionCount = PartitionCountOf(host);
        var partnerPartitionCount = PartitionCountOf(partner);

        if (host.Denominator == 0 && partner.Denominator == 0)
        {
            long combined = 0;
            for (long hostTick = 0; hostTick < hostPartitionCount; hostTick++)
            {
                combined += host.Numerator;
            }

            for (long partnerTick = 0; partnerTick < partnerPartitionCount; partnerTick++)
            {
                combined += partner.Numerator;
            }

            return (combined, 0);
        }

        if (partner.Denominator == 0)
        {
            long filled = 0;
            for (long hostTick = 0; hostTick < hostPartitionCount; hostTick++)
            {
                filled += partner.Numerator;
            }

            return (filled / hostPartitionCount, host.Denominator);
        }

        var hostPiece = host.Numerator / hostPartitionCount;
        var partnerPiece = partner.Numerator / partnerPartitionCount;
        long scaled = 0;

        for (long hostTick = 0; hostTick < hostPartitionCount; hostTick++)
        {
            for (long partnerTick = 0; partnerTick < partnerPartitionCount; partnerTick++)
            {
                scaled += hostPiece * partnerPiece;
            }
        }

        return (scaled / partnerPartitionCount, host.Denominator);
    }

    private static long PartitionCountOf((long Numerator, long Denominator) value) =>
        value.Denominator == 0 ? 1 : Math.Abs(value.Denominator);
}
