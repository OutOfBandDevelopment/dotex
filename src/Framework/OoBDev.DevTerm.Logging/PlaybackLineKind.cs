using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Logging;

/// <summary>What a playback line shows — the front ends style by it (WPF colors, the TUI's text tags).</summary>
public enum PlaybackLineKind
{
    /// <summary>Received data as a presenter rendered it: <c>[ascii] text</c>.</summary>
    Received,

    /// <summary>Sent data: <c>[tx] text</c>.</summary>
    Sent,

    /// <summary>A connection event: <c>[dev-term] Connected.</c>.</summary>
    Status,

    /// <summary>A lost connection or a presenter failure: <c>[error] …</c>.</summary>
    Error,

    /// <summary>A playback annotation: <c>[note] …</c>.</summary>
    Note,
}
