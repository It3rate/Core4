using System;
using System.Globalization;
using Core4.Numerics;

namespace Core4.AvaloniaProbe.Rendering;

internal static class CircuitFormat
{
    public static string FormatRatio(Ratio value, bool fractionView)
    {
        var normalized = value.Canonicalize();

        if (normalized.IsTension)
        {
            return fractionView
                ? $"{normalized.Potential.Raw}/0"
                : $"{normalized.Potential.Raw.ToString("0.0", CultureInfo.InvariantCulture)}/0";
        }

        if (fractionView)
        {
            return normalized.Dominant == Measure.One
                ? normalized.Recessive.Raw.ToString(CultureInfo.InvariantCulture)
                : $"{normalized.Recessive.Raw}/{normalized.Dominant.Raw}";
        }

        return normalized.Approximate.ToString("0.0", CultureInfo.InvariantCulture);
    }

    public static string FormatResistanceLabel(Ratio value)
    {
        var magnitude = Math.Abs(value.Canonicalize().Approximate);
        var sign = value.Approximate < 0d ? "-" : string.Empty;
        var (scaled, suffix) = magnitude switch
        {
            >= 1_000_000_000d => (magnitude / 1_000_000_000d, "GΩ"),
            >= 1_000_000d => (magnitude / 1_000_000d, "MΩ"),
            >= 1_000d => (magnitude / 1_000d, "kΩ"),
            _ => (magnitude, "Ω")
        };

        var text = scaled.ToString(
            Math.Abs(scaled - Math.Round(scaled)) < 0.05d ? "0" : "0.#",
            CultureInfo.InvariantCulture);

        return $"{sign}{text}{suffix}";
    }
}
