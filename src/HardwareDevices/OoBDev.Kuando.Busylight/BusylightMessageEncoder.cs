using OoBDev.System.IO.Messages;
using System;

namespace OoBDev.Kuando.Busylight;

/// <summary>
/// Encodes a <see cref="BusylightCommand"/> as the verified 9-byte write: report id <c>0x00</c>, then the 8-byte step record.
/// </summary>
public sealed class BusylightMessageEncoder : IMessageEncoder<BusylightCommand>
{
    /// <summary>The length of the encoded report.</summary>
    public const int ReportLength = 1 + BusylightCommand.Length;

    /// <inheritdoc/>
    public ReadOnlyMemory<byte> Encode(ref BusylightCommand request)
    {
        var report = new byte[ReportLength];
        request.WriteTo(report.AsSpan(1));
        return report;
    }
}
