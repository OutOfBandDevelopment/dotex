namespace OoBDev.DevTerm.Logging;

/// <summary>What a <see cref="SessionLogRecord"/> describes — its <c>"type"</c> in the file. See docs/design/SessionLogging/session-logging.md.</summary>
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

    /// <summary><c>"checkpoint"</c> — a marker the recorder writes every so often. It carries no data; playback saves the presenters' state when it passes one, so a backward seek restarts from the nearest checkpoint. Older readers keep it as an unknown record and ignore it.</summary>
    Checkpoint,

    /// <summary>A record type this version doesn't know (written by a newer dev-term) — kept verbatim so trimming/annotating a log never drops it.</summary>
    Unknown,
}
