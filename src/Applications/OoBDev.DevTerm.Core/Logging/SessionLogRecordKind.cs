using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Logging;

/// <summary>What a <see cref="SessionLogRecord"/> describes — its <c>"type"</c> in the file. See docs/design/session-logging.md.</summary>
public enum SessionLogRecordKind
{
    /// <summary><c>"session"</c> — the logger attached to a session (logging started, or followed a profile switch): which connection, and whether it was already open.</summary>
    Session,

    /// <summary><c>"open"</c> — the connection opened.</summary>
    Open,

    /// <summary><c>"close"</c> — the connection was closed on request (File &gt; Disconnect, quitting, a profile switch).</summary>
    Close,

    /// <summary><c>"disconnect"</c> — the session closed itself: a read/send failure, or the device hanging up.</summary>
    Disconnect,

    /// <summary><c>"tx"</c> — bytes handed to the transport.</summary>
    Tx,

    /// <summary><c>"rx"</c> — one chunk received from the device, exactly as the presenters were handed it.</summary>
    Rx,

    /// <summary><c>"instrument"</c> — a SCPI instrument profile picked after logging started (Device panel); <see cref="SessionLogRecord.Profile"/> is its name. Playback applies it when the header names none.</summary>
    Instrument,

    /// <summary><c>"note"</c> — a text annotation added during playback (markup). Not a captured event, so it has no sequence number.</summary>
    Note,

    /// <summary>A record type this version doesn't know (written by a newer dev-term) — kept verbatim so trimming/annotating a log never drops it.</summary>
    Unknown,
}
