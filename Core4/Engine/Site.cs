namespace Core4.Engine;

/// <summary>
/// A local place where connection can happen. A site has a placement, a fixed aligned kernel, and an orthogonal
/// reservoir that can hold unresolved remainder plus articulated child flows.
/// Example: <c>var site = Site.Open(x, 0).WithResidual(Ratio.Tension(10));</c>
/// </summary>
public readonly record struct Site(Placement Placement, AlignedKernel Kernel, OrthogonalReservoir Orthogonal)
{
    public static Site Open(Carrier carrier, Measure location) => new(carrier.At(location), AlignedKernel.Open, OrthogonalReservoir.Open);

    public static Site Open(Placement placement) => new(placement, AlignedKernel.Open, OrthogonalReservoir.Open);

    public static Site Tension(Carrier carrier, Measure location, Measure energy) => Open(carrier, location).Focus(energy);

    public Carrier Carrier => Placement.PrimaryCarrier;

    public Ratio Position => Placement.Ratio;

    public Ratio State => Kernel.Dominant;

    public Measure Potential => Kernel.Potential + Orthogonal.Potential;

    public bool IsOpen => Kernel.IsOpen && Orthogonal.OccupiedCount == Measure.Zero;

    public bool IsTension => Kernel.BoundCount + Orthogonal.BoundCount == Measure.Zero && Potential != Measure.Zero;

    public Ratio On(Carrier carrier) => Placement.On(carrier);

    public Measure Location => On(Carrier).Whole;

    public Site Place(Placement placement) => new(placement, Kernel, Orthogonal);

    public Site Anchor(Ratio state) => new(Placement, Kernel.Anchor(state), Orthogonal);

    public Site Focus(Measure energy) => new(Placement, Kernel.Focus(energy), Orthogonal);

    public Site Resolve(Ratio state) => new(Placement, Kernel.Assert(state), Orthogonal);

    public Site WithResidual(Ratio residual) => new(Placement, Kernel, Orthogonal.Resolve(residual));

    public Site Connect(Flow flow) => new(Placement, Kernel, Orthogonal.Add(flow));

    public Site Publish(ISupport support, Ratio value) => Connect(new Flow(support, value.Normalize(), Provenance.SelfDriven(value.Potential)));

    public Site Request(ISupport support, Ratio value) => Connect(new Flow(support, value.Normalize(), Provenance.OtherDriven(value.Potential)));

    public Site Exchange(ISupport support, Ratio value, Provenance provenance) => Connect(new Flow(support, value.Normalize(), provenance));

    public SiteSettlement Settle()
    {
        var anchor = Kernel.Recessive.Normalize();
        var incoming = Orthogonal.Incoming();

        if (Orthogonal.Residual.Potential != Measure.Zero || Orthogonal.Residual.Dominant != Measure.Zero)
        {
            incoming = incoming.Potential == Measure.Zero && incoming.Dominant == Measure.Zero
                ? Orthogonal.Residual.Normalize()
                : incoming.Add(Orthogonal.Residual).Normalize();
        }

        var provisional = anchor.Add(incoming).Normalize();
        var published = provisional.Add(Kernel.Dominant).Normalize();
        var feedback = Orthogonal.Outgoing().Add(Orthogonal.Oscillating()).Normalize();
        var settled = published.Subtract(feedback).Normalize();
        var remainder = incoming.Subtract(feedback).Normalize();
        var next = Resolve(settled).WithResidual(remainder);

        return new SiteSettlement(this, anchor, incoming, provisional, published, feedback, settled, remainder, next);
    }

    public Relation Relate(Site anchor) => new(Carrier, this, anchor);

    public Relation Relate(Carrier carrier, Site anchor) => new(carrier, this, anchor);
}
