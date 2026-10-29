using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace OoBDev.Nmea;

/// <summary>
/// GGA: time, position and fix data. A blank field is <see langword="null"/>.
/// </summary>
/// <param name="Time">UTC time of the fix.</param>
/// <param name="Latitude">Signed decimal degrees, south negative.</param>
/// <param name="Longitude">Signed decimal degrees, west negative.</param>
/// <param name="Quality">The fix quality.</param>
/// <param name="SatellitesUsed">Satellites used in the fix.</param>
/// <param name="Hdop">Horizontal dilution of precision.</param>
/// <param name="AltitudeMeters">Altitude above mean sea level.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct NmeaGga(
    TimeOnly? Time,
    double? Latitude,
    double? Longitude,
    NmeaFixQuality Quality,
    int? SatellitesUsed,
    double? Hdop,
    double? AltitudeMeters)
{
    /// <summary>Reads a GGA sentence.</summary>
    /// <param name="sentence">Any sentence.</param>
    /// <param name="gga">The typed view when the sentence is a GGA with at least nine fields.</param>
    /// <returns><see langword="true"/> when <paramref name="gga"/> is set.</returns>
    public static bool TryParse(NmeaSentence sentence, [NotNullWhen(true)] out NmeaGga? gga)
    {
        ArgumentNullException.ThrowIfNull(sentence);
        gga = null;
        if (sentence.SentenceType != "GGA" || sentence.Fields.Count < 9) return false;

        var f = sentence.Fields;
        gga = new NmeaGga(
            NmeaFields.Time(f[0]),
            NmeaFields.Degrees(f[1], f[2]),
            NmeaFields.Degrees(f[3], f[4]),
            f[5] switch
            {
                "" or "0" => NmeaFixQuality.NoFix,
                "1" => NmeaFixQuality.Gps,
                "2" => NmeaFixQuality.Dgps,
                "3" => NmeaFixQuality.Pps,
                "4" => NmeaFixQuality.RtkFixed,
                "5" => NmeaFixQuality.RtkFloat,
                "6" => NmeaFixQuality.Estimated,
                _ => NmeaFixQuality.Unknown,
            },
            NmeaFields.Int(f[6]),
            NmeaFields.Double(f[7]),
            NmeaFields.Double(f[8]));
        return true;
    }
}
