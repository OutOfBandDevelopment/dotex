namespace OoBDev.Midi;

/// <summary>
/// One message decoded from a live MIDI byte stream.
/// </summary>
/// <param name="Status">The status byte (for a channel message, with the channel in the low nibble).</param>
/// <param name="Data1">The first data byte, or zero.</param>
/// <param name="Data2">The second data byte, or zero.</param>
/// <param name="SysEx">For a system exclusive message the bytes between <c>F0</c> and <c>F7</c>; otherwise <see langword="null"/>.</param>
public sealed record MidiMessage(byte Status, byte Data1, byte Data2, byte[]? SysEx = null)
{
    /// <summary>Gets a value indicating whether this is a channel voice message.</summary>
    public bool IsChannelMessage => Status is >= 0x80 and < 0xF0;

    /// <summary>Gets the zero-based channel of a channel message, or -1.</summary>
    public int Channel => IsChannelMessage ? Status & 0x0F : -1;

    /// <summary>Gets the message type nibble (<c>8</c> to <c>E</c>) of a channel message, or the status byte itself for a system message.</summary>
    public byte Type => IsChannelMessage ? (byte)(Status >> 4) : Status;
}
