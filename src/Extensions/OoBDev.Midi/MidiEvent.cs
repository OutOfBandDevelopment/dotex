using System;

namespace OoBDev.Midi;

/// <summary>
/// One event of a standard MIDI file track.
/// </summary>
/// <param name="DeltaTicks">Ticks since the previous event in the track.</param>
/// <param name="Kind">The kind of event.</param>
/// <param name="Status">The status byte: <c>80</c> to <c>EF</c> for a channel event, <c>F0</c> for SysEx, <c>F7</c> for an escape, <c>FF</c> for meta.</param>
/// <param name="Data">The data bytes: one or two for a channel event, the payload for SysEx, escape and meta.</param>
/// <param name="MetaType">The meta event type (for example <c>51</c> for tempo and <c>2F</c> for end of track); zero for other kinds.</param>
/// <param name="UsedRunningStatus">Whether the file omitted the status byte of this channel event; kept so a file writes back byte for byte.</param>
public sealed record MidiEvent(
    long DeltaTicks,
    MidiEventKind Kind,
    byte Status,
    byte[] Data,
    byte MetaType = 0,
    bool UsedRunningStatus = false)
{
    /// <summary>Gets the zero-based channel of a channel event, or -1 for any other kind.</summary>
    public int Channel => Kind == MidiEventKind.Channel ? Status & 0x0F : -1;

    /// <summary>Gets the first data byte of a channel event, or zero when there is none.</summary>
    public byte Data1 => Data.Length > 0 ? Data[0] : (byte)0;

    /// <summary>Gets the second data byte of a channel event, or zero when there is none.</summary>
    public byte Data2 => Data.Length > 1 ? Data[1] : (byte)0;

    /// <summary>Gets the tempo in microseconds per quarter note when this is a tempo meta event; otherwise <see langword="null"/>.</summary>
    public int? MicrosecondsPerQuarterNote =>
        Kind == MidiEventKind.Meta && MetaType == 0x51 && Data.Length == 3 ? (Data[0] << 16) | (Data[1] << 8) | Data[2] : null;

    /// <inheritdoc />
    public bool Equals(MidiEvent? other) =>
        other is not null && DeltaTicks == other.DeltaTicks && Kind == other.Kind && Status == other.Status
        && MetaType == other.MetaType && UsedRunningStatus == other.UsedRunningStatus && Data.AsSpan().SequenceEqual(other.Data);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(DeltaTicks, Kind, Status, MetaType, Data.Length);
}
