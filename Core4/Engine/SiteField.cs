using System.Collections.Immutable;

namespace Core4.Engine;

/// <summary>
/// The finite local field at one site. For the current two-carrier engine this exposes four slots:
/// aligned recessive, aligned dominant, orthogonal recessive, and orthogonal dominant.
/// Slots stay numeric, so open <c>0/0</c>, unresolved <c>m/0</c>, and committed <c>m/u</c> all share one form.
/// Example: <c>var field = SiteField.Open.Add(1, Ratio.Tension(10));</c>
/// </summary>
public readonly record struct SiteField(
    Ratio AlignedRecessive,
    Ratio AlignedDominant,
    Ratio OrthogonalRecessive,
    Ratio OrthogonalDominant)
{
    public static SiteField Open => new(
        Ratio.Zero(Measure.Zero),
        Ratio.Zero(Measure.Zero),
        Ratio.Zero(Measure.Zero),
        Ratio.Zero(Measure.Zero));

    public Measure SlotCount => new(4);

    public Measure Potential => AlignedRecessive.Potential
        + AlignedDominant.Potential
        + OrthogonalRecessive.Potential
        + OrthogonalDominant.Potential;

    public Measure OpenCount => Count(slot => slot.Potential == Measure.Zero && slot.Dominant == Measure.Zero);

    public Measure OccupiedCount => Count(slot => slot.Potential != Measure.Zero || slot.Dominant != Measure.Zero);

    public Measure BoundCount => Count(slot => slot.Dominant != Measure.Zero);

    public ImmutableArray<Ratio> Slots() =>
    [
        AlignedRecessive,
        AlignedDominant,
        OrthogonalRecessive,
        OrthogonalDominant
    ];

    public Ratio Slot(Measure code) => code.Raw switch
    {
        0 => AlignedRecessive,
        1 => AlignedDominant,
        2 => OrthogonalRecessive,
        3 => OrthogonalDominant,
        _ => Ratio.Tension(code)
    };

    public SiteField Replace(Measure code, Ratio value) => code.Raw switch
    {
        0 => this with { AlignedRecessive = value.Normalize() },
        1 => this with { AlignedDominant = value.Normalize() },
        2 => this with { OrthogonalRecessive = value.Normalize() },
        3 => this with { OrthogonalDominant = value.Normalize() },
        _ => this
    };

    public SiteField Add(Measure code, Ratio value) => Replace(code, Slot(code).Add(value));

    public SiteField FocusAlignedRecessive(Measure energy) => Replace(0, Ratio.Tension(energy));

    public SiteField FocusAlignedDominant(Measure energy) => Replace(1, Ratio.Tension(energy));

    public SiteField FocusOrthogonalRecessive(Measure energy) => Replace(2, Ratio.Tension(energy));

    public SiteField FocusOrthogonalDominant(Measure energy) => Replace(3, Ratio.Tension(energy));

    private Measure Count(Func<Ratio, bool> predicate)
    {
        var total = 0L;

        foreach (var slot in Slots())
        {
            if (predicate(slot))
            {
                total++;
            }
        }

        return new Measure(total);
    }
}
