using System;
using System.Collections.Immutable;
using Core4.Calibration;
using Core4.Numerics;
using Core4.Structure;

namespace Core4.AvaloniaProbe.Models;

public static class CalibrationDemoCatalog
{
    public static ImmutableArray<CalibrationDataset> Datasets { get; } =
    [
        new(
            "gear-sizes",
            "Gear Sizes",
            "Mixed common imperial diametral-pitch values and metric module values, all scaled by 1000. This is deliberately awkward, so later calibration passes can discover that the stream is discrete and unit-mixed before trying to smooth it.",
            "Wikipedia gear standards: common diametral pitches and preferred standard metric modules",
            "https://en.wikipedia.org/wiki/Gear",
            "gear",
            new Carrier(4100),
            ImmutableArray.Create<Measure>(
                3000, 4000, 5000, 6000, 8000, 10000, 12000, 16000, 20000, 24000,
                32000, 48000, 64000, 100, 200, 300, 400, 500, 600, 800,
                1000, 1250, 1500, 2000, 2500, 3000, 4000, 5000, 6000, 8000,
                10000, 12000, 16000, 20000, 25000, 32000, 40000, 50000),
            FormatGearScale),
        new(
            "winnipeg-april-2024",
            "Winnipeg April Highs",
            "Daily high temperatures in Winnipeg, scaled by 100 so the stream keeps its raw seasonal shape instead of arriving pre-calibrated in coarse whole degrees.",
            "ExtremeWeatherWatch Winnipeg 2024 daily highs",
            "https://www.extremeweatherwatch.com/cities/winnipeg/year-2024",
            "degC",
            new Carrier(4101),
            ImmutableArray.Create<Measure>(
                620, 200, 790, 1010, 1250, 1410, 1560, 1100, 1380, 1620,
                1400, 1270, 1990, 1510, 1470, 1410, 870, 500, 0, 1070,
                1710, 1920, 990, 2020, 2430, 1660, 1020, 1170, 550),
            FormatTemperature),
        new(
            "nhl-roster-weights",
            "People Weights",
            "Thirty real roster weights from the Jets and Wild game notes, scaled by 300. The point here is not the scale itself, but that the stream comes from real people and already has a human-sized band the calibrater can try to inhabit.",
            "NHL game notes roster weights for Winnipeg and Minnesota",
            "https://media.d3.nhle.com/image/private/t_document/prd/jrwwdtzkw9styonnw1lq.pdf",
            "weight",
            new Carrier(4102),
            ImmutableArray.Create<Measure>(
                57000, 58200, 57000, 67500, 60000, 54300, 60300, 64800, 54000, 63000,
                60300, 52500, 66000, 62100, 58500, 62100, 64800, 62100, 65400, 69300,
                62100, 54900, 55500, 65700, 60000, 54900, 60300, 60300, 62100, 67800),
            FormatWeight),
        new(
            "gdp-countries",
            "Country GDP Sizes",
            "Thirty-five nominal GDP values kept as long whole numbers in millions of US dollars. The range is huge on purpose, so later calibration work can discover billion-scale units instead of receiving them pre-normalized.",
            "Wikipedia GDP table compiling IMF and World Bank country values",
            "https://en.wikipedia.org/wiki/List_of_countries_by_GDP_(nominal)",
            "usd",
            new Carrier(4103),
            ImmutableArray.Create<Measure>(
                31821293, 20650754, 5328184, 4515629, 4463634, 4225639, 3558562, 2701544, 2509421,
                2420837, 2292690, 2041830, 2030999, 1948227, 1936617, 1576110, 1550236, 1413076,
                1316250, 1109962, 1074588, 971448, 761174, 750112, 711503, 667922, 666407, 606225,
                604198, 601163, 561509, 547693, 533922, 519288, 511059),
            FormatGdpMillions)
    ];

    public static ImmutableArray<CalibrationFrame> BuildFrames(CalibrationDataset dataset)
    {
        var calibrater = Calibrater.Start(dataset.Carrier);
        var frames = ImmutableArray.CreateBuilder<CalibrationFrame>(dataset.Values.Length);

        for (var index = 0; index < dataset.Values.Length; index++)
        {
            var value = dataset.Values[index];
            calibrater = calibrater.Input(value);
            frames.Add(new CalibrationFrame(index + 1, value, calibrater));
        }

        return frames.MoveToImmutable();
    }

    private static string FormatGearScale(Measure value)
        => value.Raw >= 1000L
            ? $"dp {value.Raw / 1000d:0.##}"
            : $"m {value.Raw / 1000d:0.###}";

    private static string FormatTemperature(Measure value) => $"{value.Raw / 100d:0.00}C";

    private static string FormatWeight(Measure value) => $"{value.Raw / 300d:0.#}lb";

    private static string FormatGdpMillions(Measure value) => $"${value.Raw / 1000d:0,0}B";
}

public sealed record CalibrationDataset(
    string Key,
    string Label,
    string Summary,
    string SourceLabel,
    string SourceUrl,
    string UnitLabel,
    Carrier Carrier,
    ImmutableArray<Measure> Values,
    Func<Measure, string> Format);

public readonly record struct CalibrationFrame(int Index, Measure Sample, Calibrater Calibrater);
