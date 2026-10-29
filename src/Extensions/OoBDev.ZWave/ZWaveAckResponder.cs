using System;

namespace OoBDev.ZWave;

/// <summary>
/// Chooses the one-byte answer the Serial API wants for each received unit: an ACK for a checksum-valid data frame, a NAK for a
/// frame that starts correctly but fails its checksum, and nothing for lone ACK, NAK and CAN bytes or stray bytes. Without the ACK
/// the stick retransmits and then answers CAN.
/// </summary>
public static class ZWaveAckResponder
{
    /// <summary>Gets the answer to send for <paramref name="received"/>, or <see langword="null"/> when none is due.</summary>
    /// <param name="received">A unit decoded from the stream.</param>
    /// <returns><see cref="ZWaveFrame.Ack"/>, <see cref="ZWaveFrame.Nak"/> or <see langword="null"/>.</returns>
    public static ZWaveFrame? Respond(ZWaveFrame received)
    {
        ArgumentNullException.ThrowIfNull(received);

        return received.Kind switch
        {
            ZWaveFrameKind.Data => ZWaveFrame.Ack,
            ZWaveFrameKind.Invalid when received.Raw.Length > 1 && received.Raw[0] == ZWaveFrame.StartOfFrame => ZWaveFrame.Nak,
            _ => null,
        };
    }
}
