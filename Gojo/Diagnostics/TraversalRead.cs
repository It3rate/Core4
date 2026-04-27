using Gojo.Core;
using Gojo.Reads;

namespace Gojo.Diagnostics;

public readonly struct TraversalRead
{
    private readonly Relation[] relations;

    public TraversalRead(Carrier reader, Relation travel, params Relation[] relations)
    {
        Reader = reader;
        Travel = travel;
        this.relations = [.. relations];
    }

    public Carrier Reader { get; }

    public Relation Travel { get; }

    public CarrierLocation[] BoundaryPoints =>
        RelationReads.TraversedBoundaryPointsOn(Reader, Travel, relations);

    public Relation[] BoundaryStates =>
        RelationReads.TraversedBoundaryStatesOn(Reader, Travel, relations);

    public Relation[] Spans =>
        RelationReads.TraversedSpansOn(Reader, Travel, relations);

    public Relation[] SpanStates =>
        RelationReads.TraversedSpanStatesOn(Reader, Travel, relations);

    public Relation[] SupportStates =>
        RelationReads.TraversedSupportStatesOn(Reader, Travel, relations);

    public Relation[] Support =>
        RelationReads.TraversedSupportOn(Reader, Travel, relations);

    public Relation[] Tension =>
        RelationReads.TraversedTensionOn(Reader, Travel, relations);

    public Relation[] Exclusive =>
        RelationReads.TraversedExclusiveOn(Reader, Travel, relations);

    public Relation[] Pass =>
        RelationReads.TraversedPassOn(Reader, Travel, relations);

    public Relation[] Block =>
        RelationReads.TraversedBlockOn(Reader, Travel, relations);
}
