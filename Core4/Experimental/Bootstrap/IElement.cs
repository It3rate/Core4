namespace Core4.Experimental.Bootstrap;

public interface IElement
{
    IElement Numerator { get; }

    IElement Denominator { get; }

    long PartitionCount { get; }

    IEnumerable<PartitionStep> Partitions();
}

public readonly record struct PartitionStep(long Start, long Width);
