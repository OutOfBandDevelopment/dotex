using System;
using System.Collections.Generic;
using System.Globalization;

namespace OoBDev.DerEE.De5000;

/// <summary>
/// Turns a <see cref="De5000Frame"/> into readable text and named values. Pure: no state, no I/O.
/// </summary>
public static class De5000Interpreter
{
    /// <summary>
    /// Describes one packet.
    /// </summary>
    /// <param name="frame">The decoded packet.</param>
    /// <returns>The line of text and the values.</returns>
    public static De5000Reading Interpret(De5000Frame frame)
    {
        var primary = FormatMeasurement(frame.PrimaryQuantity, frame.PrimaryValue, frame.PrimaryUnit, frame.PrimaryStatus);
        var secondary = FormatMeasurement(frame.SecondaryQuantity, frame.SecondaryValue, frame.SecondaryUnit, frame.SecondaryStatus);

        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["primary"] = primary,
            ["secondary"] = secondary,
            ["frequency"] = frame.Frequency ?? string.Empty,
            ["hold"] = Flag(frame.Hold),
            ["delta"] = Flag(frame.Delta),
            ["referenceShown"] = Flag(frame.ReferenceShown),
            ["calibration"] = Flag(frame.Calibration),
            ["sorting"] = Flag(frame.Sorting),
            ["lcrAuto"] = Flag(frame.LcrAuto),
            ["autoRange"] = Flag(frame.AutoRange),
            ["parallel"] = Flag(frame.Parallel),
        };

        var parts = new List<string> { primary, secondary };
        if (frame.Frequency is not null) parts.Add($"@{frame.Frequency}");

        var flags = new List<string>();
        if (frame.Hold) flags.Add("HOLD");
        if (frame.Delta) flags.Add("REL");
        if (frame.Calibration) flags.Add("CAL");
        if (frame.Sorting) flags.Add(frame.Tolerance is null ? "SORT" : $"SORT {frame.Tolerance}");
        if (frame.LcrAuto) flags.Add("AUTO-LCR");
        if (frame.AutoRange) flags.Add("AUTO-RNG");
        if (flags.Count > 0) parts.Add($"[{string.Join(' ', flags)}]");

        return new De5000Reading(string.Join(' ', parts), values);
    }

    /// <summary>
    /// Formats one measurement.
    /// </summary>
    /// <param name="quantity">The quantity label, or <see langword="null"/>.</param>
    /// <param name="value">The value.</param>
    /// <param name="unit">The unit, or <see langword="null"/>.</param>
    /// <param name="status">The status, or <see langword="null"/>.</param>
    /// <returns>For example <c>Ls=1.234mH</c> or <c>Cp=OL</c>.</returns>
    public static string FormatMeasurement(string? quantity, double value, string? unit, string? status)
    {
        var label = quantity ?? "?";
        if (status is "blank" or "----" or "OL") return $"{label}={status}";

        var text = $"{label}={value.ToString("G6", CultureInfo.InvariantCulture)}{unit ?? string.Empty}";
        return status is "normal" or null ? text : $"{text} ({status})";
    }

    private static string Flag(bool on) => on ? "1" : "0";
}
