using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace OoBDev.Nmea;

/// <summary>
/// GSV: the header of a satellites-in-view message. The per-satellite blocks are not read.
/// </summary>
/// <param name="TotalMessages">How many GSV messages make up the cycle.</param>
/// <param name="MessageNumber">This message within the cycle.</param>
/// <param name="SatellitesInView">Satellites in view.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct NmeaGsv(int? TotalMessages, int? MessageNumber, int? SatellitesInView)
{
    /// <summary>Reads a GSV sentence.</summary>
    /// <param name="sentence">Any sentence.</param>
    /// <param name="gsv">The typed view when the sentence is a GSV with at least three fields.</param>
    /// <returns><see langword="true"/> when <paramref name="gsv"/> is set.</returns>
    public static bool TryParse(NmeaSentence sentence, [NotNullWhen(true)] out NmeaGsv? gsv)
    {
        ArgumentNullException.ThrowIfNull(sentence);
        gsv = null;
        if (sentence.SentenceType != "GSV" || sentence.Fields.Count < 3) return false;

        var f = sentence.Fields;
        gsv = new NmeaGsv(NmeaFields.Int(f[0]), NmeaFields.Int(f[1]), NmeaFields.Int(f[2]));
        return true;
    }
}
