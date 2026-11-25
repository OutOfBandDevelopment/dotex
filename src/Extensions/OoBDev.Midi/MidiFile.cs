using System.Collections.Generic;

namespace OoBDev.Midi;

/// <summary>
/// A standard MIDI file: the header fields and the tracks.
/// </summary>
/// <param name="Format">0 (one track), 1 (simultaneous tracks) or 2 (independent sequences).</param>
/// <param name="Division">The raw time division: ticks per quarter note when the top bit is clear, otherwise SMPTE.</param>
/// <param name="Tracks">The tracks in file order.</param>
public sealed record MidiFile(ushort Format, ushort Division, IReadOnlyList<MidiTrack> Tracks)
{
    /// <summary>Gets the ticks per quarter note, or <see langword="null"/> when the file uses SMPTE time.</summary>
    public int? TicksPerQuarterNote => (Division & 0x8000) == 0 ? Division : null;
}
