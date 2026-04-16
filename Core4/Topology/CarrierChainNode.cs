using System.Collections.Immutable;

namespace Core4.Topology;

/// <summary>
/// One carrier-local node in an ordered chain. The node itself lives on the carrier, while attached sites record
/// which wider topology sites currently pin to this carrier position.
/// </summary>
/// <param name="Site">The carrier-local node for this chain position. This is always a site on the chain's own carrier.</param>
/// <param name="AttachedSites">The wider topology sites that resolve onto this carrier-local pin. These may live on
/// other carriers and contribute branch or join structure without becoming separate chain nodes on this carrier.</param>
public readonly record struct CarrierChainNode(Site Site, ImmutableArray<Site> AttachedSites)
{
    public Carrier Carrier => Site.Carrier;

    public Ratio Position => Site.On(Carrier).Normalize();

    public Measure Location => Position.Whole;

    public Measure AttachmentCount => new(AttachedSites.Length);

    public bool HasAttachments => AttachmentCount != Measure.Zero;
}
