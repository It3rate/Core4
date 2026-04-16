using System.Collections.Immutable;

namespace Core4.Topology;

/// <summary>
/// A primitive connector reading for one site inside a connected topology. It combines local incidence and aligned
/// geometry without introducing symbolic connector kinds, so pass-through, T-like, and endpoint structures stay
/// readable as numeric counts and support spans.
/// </summary>
public readonly record struct SiteConnector(ConnectedSiteTopology Topology, Site Site)
{
    public SiteIncidence Incidence => Topology.IncidenceAt(Site);

    public Measure NeighborCount => new(Topology.NeighborsOf(Site).Length);

    public ImmutableArray<Carrier> Carriers => Incidence.Carriers();

    public ImmutableArray<AlignedSiteSpan> Geometries
    {
        get
        {
            var spans = ImmutableArray.CreateBuilder<AlignedSiteSpan>();

            foreach (var carrier in Carriers)
            {
                var geometry = Topology.GeometryAt(Site, carrier);

                if (geometry.IncomingCount != Measure.Zero || geometry.OutgoingCount != Measure.Zero)
                {
                    spans.Add(geometry);
                }
            }

            return spans.ToImmutable();
        }
    }

    public AlignedSiteSpan SiteCarrierGeometry => Topology.GeometryAt(Site, Site.Carrier);

    public Measure GeometryCarrierCount => new(Geometries.Length);

    public Measure InteriorCarrierCount => Count(geometry => geometry.IsInterior);

    public Measure EndpointCarrierCount => Count(geometry => geometry.IsStartEndpoint || geometry.IsEndEndpoint);

    public Measure CenteredCarrierCount => Count(geometry => geometry.IsCentered);

    public Measure AsymmetricCarrierCount => Count(geometry => geometry.HasGeometry && !geometry.IsCentered);

    public Measure IncomingTerminalCarrierCount => CountIncomingTerminalCarriers();

    public Measure OutgoingTerminalCarrierCount => CountOutgoingTerminalCarriers();

    public Ratio SiteCarrierPosition => SiteCarrierGeometry.Position;

    public Ratio SiteCarrierBalance => SiteCarrierGeometry.Balance;

    public Ratio SiteCarrierAsymmetry => SiteCarrierGeometry.Asymmetry;

    private Measure Count(Func<AlignedSiteSpan, bool> predicate)
    {
        long total = 0L;

        foreach (var geometry in Geometries)
        {
            if (predicate(geometry))
            {
                total++;
            }
        }

        return new Measure(total);
    }

    private Measure CountCarriers(Func<Carrier, bool> predicate)
    {
        long total = 0L;

        foreach (var carrier in Carriers)
        {
            if (predicate(carrier))
            {
                total++;
            }
        }

        return new Measure(total);
    }

    private Measure CountIncomingTerminalCarriers()
    {
        var incidence = Incidence;
        long total = 0L;

        foreach (var carrier in Carriers)
        {
            if (incidence.Terminates(carrier) && incidence.HasIncoming(carrier) && !incidence.HasOutgoing(carrier))
            {
                total++;
            }
        }

        return new Measure(total);
    }

    private Measure CountOutgoingTerminalCarriers()
    {
        var incidence = Incidence;
        long total = 0L;

        foreach (var carrier in Carriers)
        {
            if (incidence.Terminates(carrier) && incidence.HasOutgoing(carrier) && !incidence.HasIncoming(carrier))
            {
                total++;
            }
        }

        return new Measure(total);
    }
}
