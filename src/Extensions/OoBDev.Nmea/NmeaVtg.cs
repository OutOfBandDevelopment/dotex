using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace OoBDev.Nmea;

/// <summary>
/// VTG: course and speed over ground. A blank field is <see langword="null"/>.
/// </summary>
/// <param name="CourseTrue">Course, degrees true.</param>
/// <param name="SpeedKnots">Speed in knots.</param>
/// <param name="SpeedKmh">Speed in kilometres per hour.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct NmeaVtg(double? CourseTrue, double? SpeedKnots, double? SpeedKmh)
{
    /// <summary>Reads a VTG sentence.</summary>
    /// <param name="sentence">Any sentence.</param>
    /// <param name="vtg">The typed view when the sentence is a VTG with at least eight fields.</param>
    /// <returns><see langword="true"/> when <paramref name="vtg"/> is set.</returns>
    public static bool TryParse(NmeaSentence sentence, [NotNullWhen(true)] out NmeaVtg? vtg)
    {
        ArgumentNullException.ThrowIfNull(sentence);
        vtg = null;
        if (sentence.SentenceType != "VTG" || sentence.Fields.Count < 8) return false;

        var f = sentence.Fields;
        vtg = new NmeaVtg(NmeaFields.Double(f[0]), NmeaFields.Double(f[4]), NmeaFields.Double(f[6]));
        return true;
    }
}
