using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace OoBDev.Nmea;

/// <summary>
/// RMC: recommended minimum navigation data. A blank field is <see langword="null"/>.
/// </summary>
/// <param name="Time">UTC time.</param>
/// <param name="Active"><see langword="true"/> for status <c>A</c> (valid), <see langword="false"/> for <c>V</c> (void).</param>
/// <param name="Latitude">Signed decimal degrees, south negative.</param>
/// <param name="Longitude">Signed decimal degrees, west negative.</param>
/// <param name="SpeedKnots">Speed over ground.</param>
/// <param name="CourseTrue">Course over ground, degrees true.</param>
/// <param name="Date">UTC date, read as the 21st century.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct NmeaRmc(
    TimeOnly? Time,
    bool Active,
    double? Latitude,
    double? Longitude,
    double? SpeedKnots,
    double? CourseTrue,
    DateOnly? Date)
{
    /// <summary>Reads an RMC sentence.</summary>
    /// <param name="sentence">Any sentence.</param>
    /// <param name="rmc">The typed view when the sentence is an RMC with at least nine fields.</param>
    /// <returns><see langword="true"/> when <paramref name="rmc"/> is set.</returns>
    public static bool TryParse(NmeaSentence sentence, [NotNullWhen(true)] out NmeaRmc? rmc)
    {
        ArgumentNullException.ThrowIfNull(sentence);
        rmc = null;
        if (sentence.SentenceType != "RMC" || sentence.Fields.Count < 9) return false;

        var f = sentence.Fields;
        rmc = new NmeaRmc(
            NmeaFields.Time(f[0]),
            f[1] == "A",
            NmeaFields.Degrees(f[2], f[3]),
            NmeaFields.Degrees(f[4], f[5]),
            NmeaFields.Double(f[6]),
            NmeaFields.Double(f[7]),
            NmeaFields.Date(f[8]));
        return true;
    }
}
