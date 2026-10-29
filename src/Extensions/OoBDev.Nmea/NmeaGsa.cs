using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;

namespace OoBDev.Nmea;

/// <summary>
/// GSA: dilution of precision and the satellites used. A blank field is <see langword="null"/>.
/// </summary>
/// <param name="FixType">No fix, 2D or 3D.</param>
/// <param name="SatellitesUsed">How many of the twelve satellite slots are filled.</param>
/// <param name="Pdop">Position dilution of precision.</param>
/// <param name="Hdop">Horizontal dilution of precision.</param>
/// <param name="Vdop">Vertical dilution of precision.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct NmeaGsa(NmeaFixType FixType, int SatellitesUsed, double? Pdop, double? Hdop, double? Vdop)
{
    /// <summary>Reads a GSA sentence.</summary>
    /// <param name="sentence">Any sentence.</param>
    /// <param name="gsa">The typed view when the sentence is a GSA with at least seventeen fields.</param>
    /// <returns><see langword="true"/> when <paramref name="gsa"/> is set.</returns>
    public static bool TryParse(NmeaSentence sentence, [NotNullWhen(true)] out NmeaGsa? gsa)
    {
        ArgumentNullException.ThrowIfNull(sentence);
        gsa = null;
        if (sentence.SentenceType != "GSA" || sentence.Fields.Count < 17) return false;

        var f = sentence.Fields;
        gsa = new NmeaGsa(
            f[1] switch { "1" => NmeaFixType.NoFix, "2" => NmeaFixType.Fix2D, "3" => NmeaFixType.Fix3D, _ => NmeaFixType.Unknown },
            f.Skip(2).Take(12).Count(prn => prn.Length > 0),
            NmeaFields.Double(f[14]),
            NmeaFields.Double(f[15]),
            NmeaFields.Double(f[16]));
        return true;
    }
}
