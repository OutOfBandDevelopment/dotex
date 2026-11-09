using System;

namespace OoBDev.System.IO.Tcp;

/// <summary>RFC 2217 NOTIFY-LINESTATE bitmask (UART line-status register bits).</summary>
[Flags]
internal enum Rfc2217LineState : byte
{
    None = 0,
    DataReady = 0x01,
    OverrunError = 0x02,
    ParityError = 0x04,
    FramingError = 0x08,
    BreakDetect = 0x10,
    TransmitHoldingRegisterEmpty = 0x20,
    TransmitShiftRegisterEmpty = 0x40,
    TimeoutError = 0x80,
}
