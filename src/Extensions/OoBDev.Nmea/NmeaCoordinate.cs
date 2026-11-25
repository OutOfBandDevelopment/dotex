using System;
using System.Globalization;

namespace OoBDev.Nmea;

/// <summary>
/// One axis of a position (latitude or longitude) as signed decimal degrees, with the common display forms.
/// </summary>
/// <param name="Degrees">Signed decimal degrees; south and west are negative.</param>
/// <param name="IsLatitude"><see langword="true"/> for latitude (N or S), <see langword="false"/> for longitude (E or W).</param>
public readonly record struct NmeaCoordinate(double Degrees, bool IsLatitude)
{
    private const long ThousandthsPerDegreeInSeconds = 3_600_000;
    private const long ThousandthsPerDegreeInMinutes = 60_000;

    /// <summary>Gets the hemisphere letter: N, S, E or W.</summary>
    public char Hemisphere => IsLatitude ? (Degrees < 0 ? 'S' : 'N') : (Degrees < 0 ? 'W' : 'E');

    /// <summary>Gets the value rounded to four decimals, about 11 metres.</summary>
    public decimal DecimalDegrees => Math.Round((decimal)Degrees, 4);

    /// <summary>
    /// Reads the NMEA form <c>ddmm.mmmm</c> (latitude) or <c>dddmm.mmmm</c> (longitude) with its hemisphere letter.
    /// </summary>
    /// <param name="value">Degrees and decimal minutes, run together.</param>
    /// <param name="hemisphere"><c>N</c>, <c>S</c>, <c>E</c> or <c>W</c>.</param>
    /// <param name="isLatitude">Which axis the value is on.</param>
    /// <param name="coordinate">The coordinate when the value parses.</param>
    /// <returns><see langword="true"/> when <paramref name="coordinate"/> is set.</returns>
    public static bool TryParse(string? value, string? hemisphere, bool isLatitude, out NmeaCoordinate coordinate)
    {
        coordinate = default;
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(hemisphere)) return false;

        var allowed = isLatitude ? "NS" : "EW";
        if (!allowed.Contains(hemisphere[0])) return false;
        if (NmeaFields.Degrees(value, hemisphere) is not { } degrees) return false;

        coordinate = new NmeaCoordinate(degrees, isLatitude);
        return true;
    }

    /// <summary>Formats as degrees, minutes and seconds, for example <c>48°7'2.280"N</c>.</summary>
    /// <param name="ascii">Use <c>d</c>, <c>m</c> and <c>s</c> in place of the degree, minute and second marks.</param>
    public string ToDegreesMinutesSeconds(bool ascii = false)
    {
        var total = (long)Math.Round(Math.Abs(Degrees) * ThousandthsPerDegreeInSeconds, 0, MidpointRounding.AwayFromZero);
        var degrees = total / ThousandthsPerDegreeInSeconds;
        var minutes = total / 60_000 % 60;
        var seconds = total % 60_000 / 1000d;

        return ascii
            ? string.Create(CultureInfo.InvariantCulture, $"{degrees}d{minutes}m{seconds:0.000}s{Hemisphere}")
            : string.Create(CultureInfo.InvariantCulture, $"{degrees}°{minutes}'{seconds:0.000}\"{Hemisphere}");
    }

    /// <summary>Formats as degrees and decimal minutes, for example <c>48°7.038'N</c>.</summary>
    /// <param name="ascii">Use <c>d</c> and <c>m</c> in place of the degree and minute marks.</param>
    public string ToDegreesMinutes(bool ascii = false)
    {
        var total = (long)Math.Round(Math.Abs(Degrees) * ThousandthsPerDegreeInMinutes, 0, MidpointRounding.AwayFromZero);
        var degrees = total / ThousandthsPerDegreeInMinutes;
        var minutes = total % ThousandthsPerDegreeInMinutes / 1000d;

        return ascii
            ? string.Create(CultureInfo.InvariantCulture, $"{degrees}d{minutes:0.000}m{Hemisphere}")
            : string.Create(CultureInfo.InvariantCulture, $"{degrees}°{minutes:0.000}'{Hemisphere}");
    }

    /// <summary>Formats as degrees, minutes and seconds (see <see cref="ToDegreesMinutesSeconds"/>).</summary>
    public override string ToString() => ToDegreesMinutesSeconds();
}
