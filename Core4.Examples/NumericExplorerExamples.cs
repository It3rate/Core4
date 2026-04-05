using System.Collections.Immutable;
using Core4.Dynamics;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.Examples;

/// <summary>
/// Reusable builders and presets for the numeric explorer UI.
/// </summary>
public static class NumericExplorerExamples
{
    public static ImmutableArray<NumericExplorerPreset> Presets() =>
    [
        new(
            "Dominant Only",
            "A one-sided basis that can read outward from its origin but cannot yet reveal the inverse side.",
            0,
            10,
            0,
            7,
            3,
            4,
            0,
            0),
        new(
            "Hidden Negative",
            "A dominant-only basis with a value on the unseen side, which becomes tension instead of a signed reading.",
            0,
            10,
            0,
            -4,
            5,
            2,
            1,
            0),
        new(
            "Centered Pair",
            "Adding the inverse unit opens the opposite side and lets the same value become a readable recessive projection.",
            0,
            10,
            10,
            -4,
            3,
            4,
            6,
            2),
        new(
            "Shifted Origin",
            "Origins are just frozen measures, so a basis at 100 behaves the same way structurally as one at 0.",
            100,
            12,
            8,
            5,
            2,
            3,
            2,
            1),
        new(
            "Orthogonal Branch",
            "Residual orthogonal tension plus several child channels shows how unknown carrier content can branch outward.",
            0,
            14,
            6,
            9,
            8,
            5,
            7,
            3)
    ];

    public static NumericExplorerSnapshot Build(NumericExplorerPreset preset)
    {
        var main = new Carrier(1);
        var dominantBasis = Basis.Freeze(main, preset.Origin, preset.Origin + preset.DominantSpan);
        var recessiveBasis = Basis.Freeze(main, preset.Origin, preset.Origin - preset.RecessiveSpan);
        var displayBasis = preset.RecessiveSpan != Measure.Zero
            ? Basis.Fuse(recessiveBasis, dominantBasis)
            : dominantBasis;

        var activeProjection = Project(preset, dominantBasis, recessiveBasis);
        var channels = Channels(preset);
        var source = CreateSourceSite(preset, channels);
        var resolved = source.Resolve(activeProjection.IsTension ? Ratio.Tension(activeProjection.Potential) : activeProjection);
        var settlement = resolved.Settle();

        return new NumericExplorerSnapshot(
            preset,
            channels,
            source,
            resolved,
            settlement,
            dominantBasis,
            recessiveBasis,
            displayBasis,
            activeProjection);
    }

    private static Site CreateSourceSite(
        NumericExplorerPreset preset,
        ImmutableArray<NumericExplorerChannel> channels)
    {
        var site = Site.Open(new Carrier(1), preset.Origin)
            .Anchor(new Ratio(Measure.Zero, Measure.One))
            .WithResidual(Ratio.Tension(preset.Residual));

        foreach (var channel in channels)
        {
            if (channel.Value.Potential == Measure.Zero && channel.Value.Dominant == Measure.Zero)
            {
                continue;
            }

            site = site.Connect(channel.ToFlow());
        }

        return site;
    }

    private static ImmutableArray<NumericExplorerChannel> Channels(NumericExplorerPreset preset) =>
    [
        new("Sensor", new Carrier(2), Ratio.Tension(preset.SensorInput), Provenance.OtherDriven(preset.SensorInput)),
        new("Switch", new Carrier(3), Ratio.Tension(preset.SwitchInput), Provenance.OtherDriven(preset.SwitchInput)),
        new("Feedback", new Carrier(4), Ratio.Tension(preset.Feedback), Provenance.Shared(preset.Feedback))
    ];

    private static Ratio Project(
        NumericExplorerPreset preset,
        Basis dominantBasis,
        Basis recessiveBasis)
    {
        var value = preset.Origin + preset.ValueOffset;

        if (preset.ValueOffset.Raw < 0)
        {
            return preset.RecessiveSpan == Measure.Zero
                ? Ratio.Tension(preset.ValueOffset.Absolute)
                : recessiveBasis.Project(value);
        }

        return dominantBasis.Project(value);
    }
}
