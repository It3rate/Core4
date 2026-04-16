using System.Collections.Immutable;
using Core4.Numerics;

namespace Core4.Structure;

/// <summary>
/// A support-independent collected sample group. This is not yet calibrated by a ruler; it is an ordered,
/// persistent grouping of distinguishable live sample sites that can later be attached to different measuring
/// scaffolds. Composite sample sets preserve child order while also carrying the flattened site order.
/// </summary>
public sealed class SampleSet
{
    private readonly ImmutableArray<Site> _sites;
    private readonly ImmutableArray<SampleSet> _inputs;

    private SampleSet(ImmutableArray<Site> sites, ImmutableArray<SampleSet> inputs)
    {
        _sites = sites;
        _inputs = inputs;
    }

    public bool IsAtomic => _inputs.IsEmpty;

    public bool IsComposite => !IsAtomic;

    public Measure SiteCount => new(_sites.Length);

    public ImmutableArray<Site> Sites => _sites;

    public ImmutableArray<SampleSet> Inputs => _inputs;

    public SampleSet Recessive => IsComposite ? _inputs[0] : this;

    public SampleSet Dominant => IsComposite ? _inputs[1] : this;

    public ImmutableArray<Measure> Measures => _sites.Select(site => site.Location).ToImmutableArray();

    public ImmutableArray<Placement> Placements => _sites.Select(site => site.Placement).ToImmutableArray();

    public static SampleSet Collect(params Site[] sites) => new([.. sites], ImmutableArray<SampleSet>.Empty);

    internal static SampleSet Collect(ImmutableArray<Site> sites) => new(sites, ImmutableArray<SampleSet>.Empty);

    public static SampleSet Fuse(SampleSet recessive, SampleSet dominant, params SampleSet[] remaining)
    {
        var inputs = new SampleSet[remaining.Length + 2];
        inputs[0] = recessive;
        inputs[1] = dominant;

        for (var index = 0; index < remaining.Length; index++)
        {
            inputs[index + 2] = remaining[index];
        }

        return Compose(inputs.ToImmutableArray());
    }

    private static SampleSet Compose(ImmutableArray<SampleSet> inputs)
    {
        var sites = ImmutableArray.CreateBuilder<Site>();

        foreach (var input in inputs)
        {
            sites.AddRange(input.Sites);
        }

        return new SampleSet(sites.ToImmutable(), inputs);
    }
}
