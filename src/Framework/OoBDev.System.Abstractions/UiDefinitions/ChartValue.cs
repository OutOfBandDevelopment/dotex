using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace OoBDev.UiDefinitions;

/// <summary>Reads a number out of a published value's text — tolerant of units or a prefix, since a raw device reply often carries them.</summary>
public static partial class ChartValue
{
    /// <summary>
    /// The whole text as an invariant-culture number (<c>"+1.2345E+00"</c>), else the first number
    /// found in it (<c>"DC VOLT 1.25 V"</c> → 1.25). False when there's none, or it's not finite.
    /// </summary>
    public static bool TryParse(string? text, out double value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        if (!double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        {
            var match = FirstNumber().Match(text);
            if (!match.Success || !double.TryParse(match.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                return false;
            }
        }

        return double.IsFinite(value);
    }

    /// <summary>A short display form: up to 3 significant decimals, invariant culture.</summary>
    public static string Format(double value, string? unit = null) =>
        value.ToString("0.###", CultureInfo.InvariantCulture) + (string.IsNullOrEmpty(unit) ? string.Empty : " " + unit);

    [GeneratedRegex(@"[-+]?(\d+\.?\d*|\.\d+)([eE][-+]?\d+)?", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex FirstNumber();
}
