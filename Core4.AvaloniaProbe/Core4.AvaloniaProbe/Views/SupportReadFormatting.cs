using Core4.Topology;

namespace Core4.AvaloniaProbe.Views;

internal static class SupportReadFormatting
{
    public static string DescribeEncounter(SiteBasisEncounter encounter)
    {
        var kind = encounter.IsExplicitPinnedBasis
            ? "explicit"
            : encounter.IsBilateral
                ? "bilateral"
                : "local";
        var sidedness = encounter.HasRecessiveCalibration && encounter.HasDominantCalibration
            ? "dual"
            : encounter.HasDominantCalibration
                ? "dominant"
                : encounter.HasRecessiveCalibration
                    ? "recessive"
                    : "blocked";
        return $"{kind} {sidedness} f {encounter.Fold} c {encounter.Commitment}";
    }

    public static string DescribeAgreement(BasisEncounterAgreement agreement)
    {
        var continuation = agreement.PreservesDirection
            ? "preserve"
            : agreement.InvertsDirection
                ? "invert"
                : "block";
        var capability = agreement.IsBilateral ? "bilateral" : agreement.IsOneSided ? "one-sided" : "none";
        return $"{continuation} {capability} f {agreement.Calibration} r {agreement.Resolution} c {agreement.Commitment}";
    }
}
