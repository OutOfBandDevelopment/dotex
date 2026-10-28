namespace OoBDev.ZWave;

/// <summary>
/// What one unit of the Z-Wave Serial API byte stream is.
/// </summary>
public enum ZWaveFrameKind
{
    /// <summary>A lone <c>0x06</c>: the receiver accepted the last data frame.</summary>
    Ack,

    /// <summary>A lone <c>0x15</c>: the receiver rejected the last data frame (bad checksum).</summary>
    Nak,

    /// <summary>A lone <c>0x18</c>: the receiver was busy or out of sync, so the frame was dropped.</summary>
    Can,

    /// <summary>A complete, checksum-valid <c>SOF len type function payload checksum</c> frame.</summary>
    Data,

    /// <summary>Bytes that are not a valid frame: a stray byte, or a frame whose checksum is wrong.</summary>
    Invalid,
}
