using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Tcp;

/// <summary>
/// RFC 2217 SET-CONTROL wire values — one shared subnegotiation command covering flow control and
/// the BREAK/DTR/RTS modem-control lines. Only the values dev-term's client actually needs to send
/// (DTR/RTS/BREAK set + "no flow control") plus the full request/ack set the codec must still be
/// able to decode when a server echoes one back.
/// </summary>
internal enum Rfc2217ControlValue : byte
{
    RequestOutboundFlowControl = 0,
    UseNoFlowControl = 1,
    UseXonXoffFlowControl = 2,
    UseHardwareFlowControl = 3,
    RequestBreakState = 4,
    SetBreakStateOn = 5,
    SetBreakStateOff = 6,
    RequestDtrState = 7,
    SetDtrStateOn = 8,
    SetDtrStateOff = 9,
    RequestRtsState = 10,
    SetRtsStateOn = 11,
    SetRtsStateOff = 12,
    RequestInboundFlowControl = 13,
    UseNoFlowControlInbound = 14,
    UseXonXoffFlowControlInbound = 15,
    UseHardwareFlowControlInbound = 16,
    UseDcdFlowControl = 17,
    UseDtrFlowControlInbound = 18,
    UseDsrFlowControl = 19,
}
