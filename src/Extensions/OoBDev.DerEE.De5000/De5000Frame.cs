using System;

namespace OoBDev.DerEE.De5000;

/// <summary>
/// One decoded DE-5000 measurement packet. A string field is <see langword="null"/> when its source value has no defined meaning
/// at that position (for example an out-of-range status nibble), so "decoded, no meaning" differs from "not decoded".
/// </summary>
/// <param name="Hold">The display is held.</param>
/// <param name="ReferenceShown">The reference value is shown.</param>
/// <param name="Delta">Relative (delta) mode.</param>
/// <param name="Calibration">Calibration mode.</param>
/// <param name="Sorting">Sorting mode.</param>
/// <param name="LcrAuto">Automatic L/C/R selection.</param>
/// <param name="AutoRange">Automatic range.</param>
/// <param name="Parallel">Parallel equivalent circuit (otherwise series).</param>
/// <param name="Frequency">The test frequency.</param>
/// <param name="Tolerance">The sorting tolerance.</param>
/// <param name="PrimaryQuantity">The primary quantity (Ls, Cp, DCR ...).</param>
/// <param name="PrimaryValue">The primary value.</param>
/// <param name="PrimaryUnit">The primary unit.</param>
/// <param name="PrimaryStatus">The primary status (normal, OL, PASS ...).</param>
/// <param name="SecondaryQuantity">The secondary quantity (D, Q, ESR, Theta).</param>
/// <param name="SecondaryValue">The secondary value.</param>
/// <param name="SecondaryUnit">The secondary unit.</param>
/// <param name="SecondaryStatus">The secondary status.</param>
public readonly record struct De5000Frame(
    bool Hold,
    bool ReferenceShown,
    bool Delta,
    bool Calibration,
    bool Sorting,
    bool LcrAuto,
    bool AutoRange,
    bool Parallel,
    string? Frequency,
    string? Tolerance,
    string? PrimaryQuantity,
    double PrimaryValue,
    string? PrimaryUnit,
    string? PrimaryStatus,
    string? SecondaryQuantity,
    double SecondaryValue,
    string? SecondaryUnit,
    string? SecondaryStatus)
{
    /// <summary>The packet length in bytes.</summary>
    public const int Length = 17;

    internal const byte Header0 = 0x00;
    internal const byte Header1 = 0x0D;
    internal const byte Footer0 = 0x0D;
    internal const byte Footer1 = 0x0A;

    // Which byte-5 table applies depends on the Parallel flag: one quantity is "series" or "parallel" by circuit mode.
    private static readonly string?[] _seriesQuantities = [null, "Ls", "Cs", "Rs", "DCR"];
    private static readonly string?[] _parallelQuantities = [null, "Lp", "Cp", "Rp", "DCR"];
    private static readonly string?[] _secondaryQuantities = [null, "D", "Q", "ESR", "Theta"];

    // Index 0 is the genuine empty string (a dimensionless reading); only index 4 is a gap.
    private static readonly string?[] _units =
        ["", "Ohm", "kOhm", "MOhm", null, "uH", "mH", "H", "kH", "pF", "nF", "uF", "mF", "%", "deg"];

    private static readonly string?[] _frequencies = ["100 Hz", "120 Hz", "1 kHz", "10 kHz", "100 kHz", "DC"];

    private static readonly string?[] _tolerances =
        [null, null, null, "+-0.25%", "+-0.5%", "+-1%", "+-2%", "+-5%", "+-10%", "+-20%", "-20%/+80%"];

    // Primary status uses 4 bits, secondary only 3, so FAIL/OPEn/Srt never occur on the secondary reading (an asymmetry of the chipset).
    private static readonly string?[] _status =
        ["normal", "blank", "----", "OL", null, null, null, "PASS", "FAIL", "OPEn", "Srt"];

    /// <summary>
    /// Parses exactly <see cref="Length"/> bytes as one measurement packet (Cyrustek ES51919 layout; the footer is <c>0D 0A</c> and a
    /// 16-bit value is <c>MSB * 0x100 + LSB</c>, as the reference decoder does rather than as its README says).
    /// </summary>
    /// <param name="packet">The bytes.</param>
    /// <param name="parsed">The decoded frame.</param>
    /// <returns><see langword="false"/> for a wrong length, header or footer.</returns>
    public static bool TryParse(ReadOnlySpan<byte> packet, out De5000Frame parsed)
    {
        parsed = default;
        if (packet.Length != Length
            || packet[0] != Header0 || packet[1] != Header1
            || packet[15] != Footer0 || packet[16] != Footer1)
            return false;

        var flags = packet[2];
        var parallel = (flags & 0b1000_0000) != 0;

        var primaryValue = ((packet[6] * 0x100) + packet[7]) * Math.Pow(10, -(packet[8] & 0b0000_0111));
        var secondaryUnit = LookUp(_units, (byte)((packet[13] & 0b1111_1000) >> 3));

        // Only % and deg can go negative, so only they are sign-extended.
        var secondaryRaw = (packet[11] * 0x100) + packet[12];
        double secondaryMagnitude = secondaryUnit is "%" or "deg" ? (short)secondaryRaw : secondaryRaw;

        parsed = new De5000Frame(
            Hold: (flags & 0b0000_0001) != 0,
            ReferenceShown: (flags & 0b0000_0010) != 0,
            Delta: (flags & 0b0000_0100) != 0,
            Calibration: (flags & 0b0000_1000) != 0,
            Sorting: (flags & 0b0001_0000) != 0,
            LcrAuto: (flags & 0b0010_0000) != 0,
            AutoRange: (flags & 0b0100_0000) != 0,
            Parallel: parallel,
            Frequency: LookUp(_frequencies, (byte)((packet[3] & 0b1110_0000) >> 5)),
            Tolerance: LookUp(_tolerances, packet[4]),
            PrimaryQuantity: LookUp(parallel ? _parallelQuantities : _seriesQuantities, packet[5]),
            PrimaryValue: primaryValue,
            PrimaryUnit: LookUp(_units, (byte)((packet[8] & 0b1111_1000) >> 3)),
            PrimaryStatus: LookUp(_status, (byte)(packet[9] & 0b0000_1111)),
            SecondaryQuantity: LookUp(_secondaryQuantities, packet[10]),
            SecondaryValue: secondaryMagnitude * Math.Pow(10, -(packet[13] & 0b0000_0111)),
            SecondaryUnit: secondaryUnit,
            SecondaryStatus: LookUp(_status, (byte)(packet[14] & 0b0000_0111)));
        return true;
    }

    private static string? LookUp(string?[] table, byte index) => index < table.Length ? table[index] : null;
}
