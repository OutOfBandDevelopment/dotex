using System;
using System.Collections.Generic;
using System.Globalization;
using OoBDev.UiDefinitions;

namespace OoBDev.Examples.Controls;

/// <summary>
/// The live displays of the control examples and the simulated device that feeds them: a sine wave
/// and a ramp drive an indicator (their sum), a bar graph and a strip chart. Every front end shows
/// the same controls and publishes <see cref="Next"/> to them on its own timer.
/// </summary>
public sealed class LiveSampleFeed
{
    private static readonly ChartChannel[] _channels =
    [
        new ChartChannel { Id = "wave", Label = "Wave" },
        new ChartChannel { Id = "ramp", Label = "Ramp" },
    ];

    private int _tick;

    /// <summary>How often a front end publishes the next sample.</summary>
    public static TimeSpan Interval { get; } = TimeSpan.FromMilliseconds(500);

    /// <summary>The indicator, bar graph and strip chart, in display order.</summary>
    public static IReadOnlyList<UiControl> Controls { get; } =
    [
        new IndicatorControl { Id = "sum", Label = "Wave + ramp", Expression = "wave + ramp", Unit = "V" },
        new BarGraphControl { Id = "bars", Label = "Channels", Minimum = 0, Maximum = 10, Unit = "V", Channels = [.. _channels] },
        new StripChartControl { Id = "strip", Label = "Trend", Minimum = 0, Maximum = 10, HistoryLength = 60, Unit = "V", Channels = [.. _channels] },
    ];

    /// <summary>The next simulated sample as published values (<c>wave</c> and <c>ramp</c>).</summary>
    public KeyValuePair<string, string>[] Next()
    {
        var wave = (5 + (4 * Math.Sin(_tick / 5.0))).ToString("0.###", CultureInfo.InvariantCulture);
        var ramp = ((_tick % 20) / 2.0).ToString("0.###", CultureInfo.InvariantCulture);
        _tick++;
        return [new("wave", wave), new("ramp", ramp)];
    }
}
