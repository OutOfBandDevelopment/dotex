using System.Globalization;

namespace OoBDev.Digi.XBee;

/// <summary>A data packet received from a remote radio.</summary>
/// <param name="Address64">The 64-bit source address.</param>
/// <param name="Address16">The 16-bit network address.</param>
/// <param name="Options">The receive options byte (bit 0 acknowledged, bit 1 broadcast).</param>
/// <param name="Payload">The received data.</param>
public sealed record XBeeReceivePacket(ulong Address64, ushort Address16, byte Options, byte[] Payload) : XBeeFrame(XBeeFrameTypes.ReceivePacket)
{
    /// <summary>The source address as 16 hex digits, the form used in topic names.</summary>
    public string AddressText => Address64.ToString("X16", CultureInfo.InvariantCulture);
}
