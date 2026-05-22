using Gojo.Core;

namespace Gojo.ConceptShapes;

/// <summary>
/// Example concept shape assemblies for manual exploration and future tests.
/// </summary>
public static class ConceptShapeExamples
{
    /// <summary>Corner seed with an open transition-radius slot.</summary>
    public static Relation CornerSeed()
    {
        var arm = new Carrier();
        var radiusSlot = new Placeholder();
        var topology = ConceptCompositions.Corner(arm, jointTicks: 0, armATicks: 5, armBTicks: 8, radiusSlot);
        var joint = ConceptPrimitives.Site(arm, 0);
        return ConceptShape.CreateSeed(new Carrier(), joint, topology);
    }

    /// <summary>Passage with a neck capacity read wired on the neck span.</summary>
    public static Relation ChokeCapableBridge()
    {
        var regionA = new Carrier();
        var regionB = new Carrier();
        var neck = new Carrier();
        var readTarget = new Carrier();
        var topology = ConceptCompositions.Passage(regionA, regionB, neck, neckEndTicks: 4);
        var neckSpan = ConceptPrimitives.DirectedExtent(neck, 0, 4);
        var port = new Relation(neckSpan, ConceptPrimitives.Site(readTarget, 0));
        var exposed = ConceptPrimitives.ExposedRead(neckSpan, port);
        return ConceptShape.Create(
            new Carrier(),
            selectedStructure: neckSpan,
            topologySurface: topology,
            flowSurface: ConceptShapeRoot.UnknownPoint(),
            parameterSurface: ConceptShapeRoot.UnknownPoint(),
            constraintSurface: ConceptShapeRoot.UnknownPoint(),
            exposedReadSurface: exposed,
            settlementWitness: ConceptShapeRoot.UnknownPoint(),
            refinementTimeline: ConceptShapeRoot.UnknownPoint());
    }

    /// <summary>Collector funnel topology without outlet (peninsula omission).</summary>
    public static Relation Peninsula()
    {
        var collector = new Carrier();
        var neck = new Carrier();
        var topology = ConceptCompositions.PeninsulaCollector(collector, neck, neckEndTicks: 3);
        return ConceptShape.CreateSeed(new Carrier(), ConceptPrimitives.Site(neck, 3), topology);
    }

    /// <summary>Collector funnel with open outlet attach at the neck end.</summary>
    public static Relation PeninsulaOpenOutlet()
    {
        var collector = new Carrier();
        var neck = new Carrier();
        var topology = ConceptCompositions.PeninsulaWithOpenOutlet(collector, neck, neckEndTicks: 3);
        return ConceptShape.CreateSeed(new Carrier(), ConceptPrimitives.Site(neck, 3), topology);
    }

    /// <summary>Full funnel with collector, neck, and outlet extent.</summary>
    public static Relation Funnel()
    {
        var collector = new Carrier();
        var neck = new Carrier();
        var outlet = new Carrier();
        var topology = ConceptCompositions.Funnel(collector, neck, 3, outlet, 6);
        return ConceptShape.CreateSeed(new Carrier(), ConceptPrimitives.Site(neck, 3), topology);
    }
}
