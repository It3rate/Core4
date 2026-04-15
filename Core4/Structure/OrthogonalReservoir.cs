using System.Collections.Immutable;
using Core4.Numerics;
namespace Core4.Structure;

/// <summary>
/// The unresolved orthogonal remainder around a site plus any articulated child flows that have attached to it.
/// The current engine stores these flows in one reservoir, but the intended interpretation is that foreign
/// publication and responsive tension are distinct directions sharing this frontier rather than one amorphous blob.
/// Example: <c>var reservoir = OrthogonalReservoir.Open.Resolve(Ratio.Tension(10)).Add(flow);</c>
/// </summary>
public readonly record struct OrthogonalReservoir(Ratio Residual, ImmutableArray<Flow> Flows)
{
    public static OrthogonalReservoir Open => new(Ratio.Zero(Measure.Zero), ImmutableArray<Flow>.Empty);

    public Measure Potential
    {
        get
        {
            var total = Residual.Potential;

            foreach (var flow in Flows)
            {
                total += flow.Potential;
            }

            return total;
        }
    }

    public Measure ConnectionCount => new(Flows.Length);

    public Measure OccupiedCount
    {
        get
        {
            var total = Residual.Potential != Measure.Zero || Residual.Dominant != Measure.Zero ? 1L : 0L;
            total += Flows.Length;
            return new Measure(total);
        }
    }

    public Measure BoundCount
    {
        get
        {
            var total = Residual.Dominant != Measure.Zero ? 1L : 0L;

            foreach (var flow in Flows)
            {
                if (flow.Value.Dominant != Measure.Zero)
                {
                    total++;
                }
            }

            return new Measure(total);
        }
    }

    public OrthogonalReservoir Resolve(Ratio residual) => new(residual.Normalize(), Flows);

    public OrthogonalReservoir Add(Flow flow) => new(Residual, Flows.Add(flow));

    public Ratio Incoming() => Aggregate(flow => flow.IsIncoming);

    public Ratio LocalIncoming() => Aggregate(flow => flow.IsIncoming && flow.Support is not Carrier);

    public Ratio Outgoing() => Aggregate(flow => flow.IsOutgoing);

    public Ratio LocalOutgoing() => Aggregate(flow => flow.IsOutgoing && flow.Support is not Carrier);

    public Ratio Oscillating() => Aggregate(flow => flow.IsOscillating);

    public Ratio LocalOscillating() => Aggregate(flow => flow.IsOscillating && flow.Support is not Carrier);

    private Ratio Aggregate(Func<Flow, bool> predicate)
    {
        var hasAny = false;
        var total = Ratio.Zero(Measure.Zero);

        foreach (var flow in Flows)
        {
            if (!predicate(flow))
            {
                continue;
            }

            total = hasAny ? total.Add(flow.Value) : flow.Value.Normalize();
            hasAny = true;
        }

        return hasAny ? total.Normalize() : Ratio.Zero(Measure.Zero);
    }
}
