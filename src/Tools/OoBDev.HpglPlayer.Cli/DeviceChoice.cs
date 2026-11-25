using System;
using System.Collections.Generic;

namespace OoBDev.HpglPlayer.Cli;

/// <summary>Picks an output device from the names the system reports.</summary>
public static class DeviceChoice
{
    /// <summary>The index of the device named by <paramref name="spec"/>: a number is the list position, otherwise the first name that contains the text (case-insensitive).</summary>
    /// <exception cref="ArgumentException">Nothing matches, or the text matches nothing in the list.</exception>
    public static int Pick(IReadOnlyList<string> names, string spec)
    {
        ArgumentNullException.ThrowIfNull(names);
        ArgumentException.ThrowIfNullOrWhiteSpace(spec);
        if (int.TryParse(spec, global::System.Globalization.NumberStyles.None, global::System.Globalization.CultureInfo.InvariantCulture, out var index))
        {
            return index < names.Count ? index : throw new ArgumentException($"There is no output device number {index} (0 to {names.Count - 1}); --list-devices shows them.", nameof(spec));
        }

        for (var i = 0; i < names.Count; i++)
        {
            if (names[i].Contains(spec, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        throw new ArgumentException($"No output device name contains '{spec}'; --list-devices shows them.", nameof(spec));
    }
}
