using System;

namespace OoBDev.System.IO.Tcp;

/// <summary>RFC 2217 NOTIFY-MODEMSTATE bitmask (UART modem-status register bits).</summary>
[Flags]
internal enum Rfc2217ModemState : byte
{
    None = 0,
    DeltaClearToSend = 0x01,
    DeltaDataSetReady = 0x02,
    TrailingEdgeRingIndicator = 0x04,
    DeltaReceiveLineSignalDetect = 0x08,
    ClearToSend = 0x10,
    DataSetReady = 0x20,
    RingIndicator = 0x40,
    ReceiveLineSignalDetect = 0x80,
}
