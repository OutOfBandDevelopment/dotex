using System.Collections.Generic;
using System.Globalization;

namespace OoBDev.Digi.XBee;

/// <summary>An I/O data sample from a remote radio: which digital lines are high and the raw analog readings.</summary>
/// <param name="Address64">The 64-bit source address.</param>
/// <param name="Address16">The 16-bit network address.</param>
/// <param name="Options">The receive options byte.</param>
/// <param name="DigitalMask">The digital lines that are sampled (bit n is line DIOn).</param>
/// <param name="DigitalValues">The state of the sampled digital lines (bit n set means high); 0 when none are sampled.</param>
/// <param name="Analog">The raw 10-bit reading of each sampled analog line, keyed by line number (0 for AD0).</param>
public sealed record XBeeIoSample(ulong Address64, ushort Address16, byte Options, ushort DigitalMask, ushort DigitalValues, IReadOnlyDictionary<int, ushort> Analog) : XBeeFrame(XBeeFrameTypes.IoDataSample)
{
    /// <summary>The source address as 16 hex digits.</summary>
    public string AddressText => Address64.ToString("X16", CultureInfo.InvariantCulture);

    /// <summary>Whether digital line <paramref name="line"/> is sampled and high.</summary>
    /// <param name="line">The line number.</param>
    /// <returns>True when sampled and high.</returns>
    public bool IsHigh(int line) => (DigitalMask & DigitalValues & (1 << line)) != 0;
}
