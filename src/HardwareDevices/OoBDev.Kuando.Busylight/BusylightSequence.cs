using System;
using System.Collections.Generic;

namespace OoBDev.Kuando.Busylight;

/// <summary>
/// The 64-byte program report: up to seven step records, a commit record <c>06 04 55 FF FF FF</c> and a checksum. The checksum is
/// the plain sum of the 62 bytes before it, stored big-endian (checked against four recorded frames). The reference traces show
/// the device accepting this report without visibly acting on it, so treat it as built to specification and not yet proven.
/// </summary>
public static class BusylightSequence
{
    /// <summary>The most steps one report holds.</summary>
    public const int MaxSteps = 7;

    /// <summary>The report length, without the leading report id.</summary>
    public const int Length = 64;

    private static readonly byte[] _commit = [0x06, 0x04, 0x55, 0xFF, 0xFF, 0xFF];

    /// <summary>
    /// Builds the program report, preceded by the report id byte <c>0x00</c> as the write needs it.
    /// </summary>
    /// <param name="steps">One to seven steps.</param>
    /// <returns>65 bytes.</returns>
    public static byte[] Encode(IReadOnlyList<BusylightCommand> steps)
    {
        ArgumentNullException.ThrowIfNull(steps);
        if (steps.Count is 0 or > MaxSteps)
            throw new ArgumentOutOfRangeException(nameof(steps), steps.Count, $"A report holds 1 to {MaxSteps} steps.");

        var report = new byte[1 + Length];
        for (var i = 0; i < steps.Count; i++)
            steps[i].WriteTo(report.AsSpan(1 + (i * BusylightCommand.Length)));

        _commit.CopyTo(report, 1 + (MaxSteps * BusylightCommand.Length));
        WriteChecksum(report);
        return report;
    }

    /// <summary>
    /// The poll request that makes the device answer with its identification: <c>8F</c>, zeros, the commit record and its checksum,
    /// preceded by the report id byte.
    /// </summary>
    /// <returns>65 bytes.</returns>
    public static byte[] Poll()
    {
        var report = new byte[1 + Length];
        report[1] = 0x8F;
        _commit.CopyTo(report, 1 + (MaxSteps * BusylightCommand.Length));
        WriteChecksum(report);
        return report;
    }

    private static void WriteChecksum(byte[] report)
    {
        var sum = 0;
        for (var i = 1; i < 1 + Length - 2; i++) sum += report[i];
        report[^2] = (byte)(sum >> 8);
        report[^1] = (byte)sum;
    }
}
