namespace OoBDev.Midi;

/// <summary>
/// The kind of an event in a standard MIDI file track.
/// </summary>
public enum MidiEventKind
{
    /// <summary>A channel voice message (note, controller, program, pressure, pitch bend).</summary>
    Channel,

    /// <summary>A system exclusive message that starts with <c>F0</c>.</summary>
    SysEx,

    /// <summary>An escape or continuation packet that starts with <c>F7</c>.</summary>
    Escape,

    /// <summary>A meta event (<c>FF</c>): tempo, time signature, text, end of track.</summary>
    Meta,
}
