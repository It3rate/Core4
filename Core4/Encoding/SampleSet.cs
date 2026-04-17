using System.Collections.Immutable;

namespace Core4.Encoding;

/// <summary>
/// A support-independent collected sample group. This is not yet calibrated by a ruler; it is an ordered,
/// persistent grouping of distinguishable live sample sites that can later be attached to different measuring
/// scaffolds. Composite sample sets preserve child order while also carrying the flattened site order, but this
/// type intentionally stays close to raw sample storage rather than accumulating read/filter helpers directly.
/// Samples are observational/candidate structure for now rather than frozen topology.
/// </summary>
public sealed class SampleSet : IElement
{
    private readonly ImmutableArray<Site> _sites;
    private readonly ImmutableArray<SampleSet> _groups;

    private SampleSet(ImmutableArray<Site> sites, ImmutableArray<SampleSet> groups)
    {
        _sites = sites;
        _groups = groups;
    }

    public bool IsAtomic => _groups.IsEmpty;

    public bool IsComposite => !IsAtomic;

    public bool IsEmpty => _sites.IsEmpty;

    public Measure SiteCount => new(_sites.Length);

    public ImmutableArray<Site> Sites => _sites;

    public ImmutableArray<SampleSet> Groups => _groups;

    public SampleSet Recessive => IsComposite ? _groups[0] : this;

    public SampleSet Dominant => IsComposite ? _groups[1] : this;

    // TODO(core): Allow sample groups to harden/promote into connected frozen structure when commitment becomes
    // strong enough. That promotion should tolerate contradiction and partial/broken structure by surfacing tension
    // instead of requiring samples to already form a perfect topology.

    public static SampleSet Collect(params Site[] sites) => new([.. sites], ImmutableArray<SampleSet>.Empty);

    internal static SampleSet Collect(ImmutableArray<Site> sites) => new(sites, ImmutableArray<SampleSet>.Empty);

    public static SampleSet Fuse(SampleSet recessive, SampleSet dominant, params SampleSet[] remaining)
    {
        var groups = new SampleSet[remaining.Length + 2];
        groups[0] = recessive;
        groups[1] = dominant;

        for (var index = 0; index < remaining.Length; index++)
        {
            groups[index + 2] = remaining[index];
        }

        return Compose(groups.ToImmutableArray());
    }

    internal static SampleSet Compose(ImmutableArray<SampleSet> groups)
    {
        var sites = ImmutableArray.CreateBuilder<Site>();

        foreach (var group in groups)
        {
            sites.AddRange(group.Sites);
        }

        return new SampleSet(sites.ToImmutable(), groups);
    }
}

