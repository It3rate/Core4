using System.Collections.Immutable;

namespace Core4.Interpretation;

/// <summary>
/// Read and selection helpers over raw sample groups. This keeps <see cref="SampleSet"/> close to ordered sample
/// storage while exposing the current convenience reads and carrier-local extraction alongside the rest of the
/// interpretation layer.
/// </summary>
public static class SampleSetInterpretation
{
    public static ImmutableArray<Measure> Measures(this SampleSet samples) =>
        samples.Sites.Select(static site => site.Location).ToImmutableArray();

    public static ImmutableArray<Placement> Placements(this SampleSet samples) =>
        samples.Sites.Select(static site => site.Placement).ToImmutableArray();

    // TODO(core): This carrier-local selection is the springboard for richer sample operations such as filter,
    // sort, support-relative extraction, and measured regrouping. Later passes should likely move those behaviors
    // into their own sample-operations surface rather than growing this interpretation helper ad hoc.
    public static SampleSet Select(this SampleSet samples, Carrier carrier)
    {
        if (samples.IsAtomic)
        {
            return SampleSet.Collect(samples.Sites.Where(site => site.Carrier == carrier).ToImmutableArray());
        }

        var selectedInputs = ImmutableArray.CreateBuilder<SampleSet>();

        foreach (var input in samples.Inputs)
        {
            var selected = input.Select(carrier);
            if (!selected.IsEmpty)
            {
                selectedInputs.Add(selected);
            }
        }

        return selectedInputs.Count switch
        {
            0 => SampleSet.Collect(),
            1 => selectedInputs[0],
            _ => SampleSet.Compose(selectedInputs.ToImmutable())
        };
    }
}
