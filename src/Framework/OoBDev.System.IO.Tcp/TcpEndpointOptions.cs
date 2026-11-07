using OoBDev.System.IO.Pipelines;
using OoBDev.System.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Tcp;

/// <summary>Configuration for a TCP session, in either direction. See docs/design/Transports/transports.md.</summary>
public sealed class TcpEndpointOptions
{
    public TcpEndpointMode Mode { get; set; } = TcpEndpointMode.Client;

    /// <summary>Required in <see cref="TcpEndpointMode.Client"/> mode. Ignored (bind-any) if unset in Listener mode.</summary>
    public string? Host { get; set; }

    /// <summary>The remote port to connect to (Client mode) or the local port to bind (Listener mode).</summary>
    public int Port { get; set; }

    /// <summary>
    /// Milliseconds a single <see cref="TcpEndpointDeviceAdapter.WriteAsync"/> call may block before it's abandoned with a
    /// <see cref="TimeoutException"/> - a peer that stops reading (a full TCP send window) would otherwise block
    /// the caller (often the UI thread, via Session.SendAsync) forever. See
    /// docs/bugs/resolved/029-tcp-write-blocks-no-timeout.md.
    /// </summary>
    public int WriteTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Opt-in software flow control (XON/XOFF) - off by default, since stripping 0x11/0x13 from a
    /// stream that isn't actually using them for flow control would silently eat legitimate device
    /// data. Some serial-to-Ethernet bridges forward the attached serial port's XON/XOFF bytes over
    /// the wire rather than honoring them locally; enabling this strips those bytes out of the
    /// decoded output and pauses/resumes dev-term's own writes on them instead. Only the initial
    /// value for a new <see cref="TcpEndpointDeviceAdapter"/> - once open, <see cref="TcpEndpointDeviceAdapter.SoftwareFlowControl"/>
    /// is the live, settable switch (a front end can toggle it without reconnecting). See
    /// docs/design/Transports/transports.md.
    /// </summary>
    public bool SoftwareFlowControl { get; set; }

    /// <summary>
    /// Milliseconds paced between each byte written, or -1 (the default) to disable pacing and
    /// write the buffer as a single, unpaced call. See <see cref="WriteDelayStream"/>.
    /// </summary>
    public int WriteByteDelayMs { get; set; } = -1;
}
