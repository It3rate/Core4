using System.Collections.Immutable;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.Topology;

/// <summary>
/// One carrier-local node in an ordered chain. The node itself lives on the carrier, while attached sites record
/// which wider topology sites currently pin to this carrier position.
/// </summary>
public readonly record struct CarrierChainNode(Site Site, ImmutableArray<Site> AttachedSites)
{
    public Carrier Carrier => Site.Carrier;

    public Ratio Position => Site.On(Carrier).Normalize();

    public Measure Location => Position.Whole;

    public Measure AttachmentCount => new(AttachedSites.Length);

    public bool HasAttachments => AttachmentCount != Measure.Zero;
}
