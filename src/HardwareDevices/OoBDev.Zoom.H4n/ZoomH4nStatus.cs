using System;

namespace OoBDev.Zoom.H4n;

/// <summary>
/// The status byte the recorder sends unprompted on its remote port: a bitmask, each byte a complete snapshot. Bits 0x04 and 0x08 have no
/// documented meaning, and 0x80 is the wake signal of the init handshake, so none of them are named.
/// </summary>
[Flags]
public enum ZoomH4nStatus : byte
{
    /// <summary>No documented bit is set.</summary>
    None = 0,

    /// <summary>The record LED.</summary>
    Record = 0x01,

    /// <summary>The peak indicator.</summary>
    Peak = 0x02,

    /// <summary>The microphone LED.</summary>
    Mic = 0x10,

    /// <summary>LED 1.</summary>
    Led1 = 0x20,

    /// <summary>LED 2.</summary>
    Led2 = 0x40,
}
