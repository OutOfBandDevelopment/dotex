using System;
using System.Collections.Generic;

namespace OoBDev.DevTerm.Logging;

/// <summary>
/// One line of a session log after the header. Captured records (<see cref="SessionLogRecordKind.Session"/>
/// through <see cref="SessionLogRecordKind.Rx"/>) carry a <see cref="Sequence"/> number, assigned in
/// capture order; notes don't. See docs/design/SessionLogging/session-logging.md for the on-disk shape.
/// </summary>
public sealed record SessionLogRecord
{
    public required SessionLogRecordKind Kind { get; init; }

    /// <summary>Capture order, starting at 1 and increasing by 1 per captured record; <see langword="null"/> for notes. A trimmed log keeps the original numbers, so gaps are expected.</summary>
    public long? Sequence { get; init; }

    /// <summary>When it happened (UTC). Captured timestamps come from a monotonic clock anchored at the log's start, so they never go backwards.</summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>The raw bytes of a <see cref="SessionLogRecordKind.Tx"/>/<see cref="SessionLogRecordKind.Rx"/> record; empty otherwise.</summary>
    public ReadOnlyMemory<byte> Data { get; init; }

    /// <summary>A note's text, or a <see cref="SessionLogRecordKind.Disconnect"/>'s error message (<see langword="null"/> for a clean hang-up).</summary>
    public string? Text { get; init; }

    /// <summary>A <see cref="SessionLogRecordKind.Session"/> record's connection definition (<c>tcp://192.168.0.107:23</c>).</summary>
    public string? Connection { get; init; }

    /// <summary>A <see cref="SessionLogRecordKind.Session"/> record's saved-profile name, when the connection is one; an <see cref="SessionLogRecordKind.Instrument"/> record's SCPI profile name.</summary>
    public string? Profile { get; init; }

    /// <summary>A <see cref="SessionLogRecordKind.Session"/> record's connection state when the logger attached (<c>open</c>/<c>closed</c>).</summary>
    public string? State { get; init; }

    /// <summary>For <see cref="SessionLogRecordKind.Unknown"/>: the record's JSON line exactly as read, written back unchanged.</summary>
    public string? RawJson { get; init; }

    /// <summary>For a <see cref="SessionLogRecordKind.Note"/> edited or deleted during playback: its earlier texts, oldest first, so the log stays auditable (null for a note never revised).</summary>
    public IReadOnlyList<string>? PreviousTexts { get; init; }

    /// <summary>For a <see cref="SessionLogRecordKind.Note"/>: it was deleted during playback. The record stays in the file (with its last text and history) but plays as a deletion marker.</summary>
    public bool Deleted { get; init; }

    /// <summary>This note with new text, the current text added to its history.</summary>
    public SessionLogRecord Revised(string text) =>
        this with { Text = text, PreviousTexts = [.. PreviousTexts ?? [], Text ?? string.Empty], Deleted = false };

    /// <summary>This note marked deleted, keeping its text and history.</summary>
    public SessionLogRecord AsDeleted() => this with { Deleted = true };

    public static SessionLogRecord Note(DateTimeOffset timestamp, string text) =>
        new() { Kind = SessionLogRecordKind.Note, Timestamp = timestamp, Text = text };
}
