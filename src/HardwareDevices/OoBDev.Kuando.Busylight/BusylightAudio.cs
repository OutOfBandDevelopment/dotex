namespace OoBDev.Kuando.Busylight;

/// <summary>
/// The packed audio byte: bit 7 plays, bits 6 to 3 select the track, bits 2 to 0 set the volume.
/// </summary>
/// <param name="Play">Play the track (otherwise muted).</param>
/// <param name="Track">The track, 0 to 15.</param>
/// <param name="Volume">The volume, 0 to 7.</param>
public readonly record struct BusylightAudio(bool Play, byte Track, byte Volume)
{
    /// <summary>The state the verified single command sends: play flag set, track 0, volume 0.</summary>
    public static BusylightAudio Default { get; } = new(true, 0, 0);

    /// <summary>Muted, as the sequence frames of the reference traces send it.</summary>
    public static BusylightAudio Muted { get; } = new(false, 0, 0);

    /// <summary>Packs the fields into the wire byte; out-of-range track and volume bits are masked.</summary>
    /// <returns>The audio byte.</returns>
    public byte ToByte() => (byte)((Play ? 0x80 : 0x00) | ((Track & 0x0F) << 3) | (Volume & 0x07));

    /// <summary>Unpacks a wire byte.</summary>
    /// <param name="value">The audio byte.</param>
    /// <returns>The fields.</returns>
    public static BusylightAudio FromByte(byte value) => new((value & 0x80) != 0, (byte)((value >> 3) & 0x0F), (byte)(value & 0x07));
}
