namespace Core4.Experimental.Bootstrap;

public interface IElement
{
    long PartitionCount { get; }

    IEnumerable<PartitionStep> Partitions();
}

public readonly record struct PartitionStep(long Start, long Width);
