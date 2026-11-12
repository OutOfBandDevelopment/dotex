using System;
using System.IO;

namespace OoBDev.Midi;

/// <summary>
/// Writes a <see cref="MidiFile"/> as standard MIDI file bytes. A channel event marked <see cref="MidiEvent.UsedRunningStatus"/>
/// omits its status byte when the previous event had the same status, so a parsed file writes back byte for byte.
/// </summary>
public static class MidiFileWriter
{
    /// <summary>
    /// Writes the file.
    /// </summary>
    /// <param name="file">The file to write.</param>
    /// <returns>The file bytes.</returns>
    public static byte[] Write(MidiFile file)
    {
        ArgumentNullException.ThrowIfNull(file);
        using var output = new MemoryStream();
        output.Write("MThd"u8);
        WriteUInt32(output, 6);
        WriteUInt16(output, file.Format);
        WriteUInt16(output, (ushort)file.Tracks.Count);
        WriteUInt16(output, file.Division);
        foreach (var track in file.Tracks)
        {
            var body = WriteTrack(track);
            output.Write("MTrk"u8);
            WriteUInt32(output, (uint)body.Length);
            output.Write(body);
        }

        return output.ToArray();
    }

    private static byte[] WriteTrack(MidiTrack track)
    {
        using var body = new MemoryStream();
        byte last = 0;
        foreach (var e in track.Events)
        {
            WriteVariableLength(body, e.DeltaTicks);
            if (e.Kind == MidiEventKind.Channel)
            {
                if (!(e.UsedRunningStatus && last == e.Status))
                {
                    body.WriteByte(e.Status);
                }

                body.Write(e.Data);
                last = e.Status;
                continue;
            }

            last = 0;
            body.WriteByte(e.Status);
            if (e.Kind == MidiEventKind.Meta)
            {
                body.WriteByte(e.MetaType);
            }

            WriteVariableLength(body, e.Data.Length);
            body.Write(e.Data);
        }

        return body.ToArray();
    }

    private static void WriteVariableLength(Stream output, long value)
    {
        if (value is < 0 or > 0x0FFFFFFF)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "A variable-length value must be between 0 and 268435455.");
        }

        Span<byte> buffer = stackalloc byte[4];
        var count = 0;
        buffer[count++] = (byte)(value & 0x7F);
        while ((value >>= 7) > 0)
        {
            buffer[count++] = (byte)((value & 0x7F) | 0x80);
        }

        for (var i = count - 1; i >= 0; i--)
        {
            output.WriteByte(buffer[i]);
        }
    }

    private static void WriteUInt16(Stream output, ushort value)
    {
        output.WriteByte((byte)(value >> 8));
        output.WriteByte((byte)value);
    }

    private static void WriteUInt32(Stream output, uint value)
    {
        WriteUInt16(output, (ushort)(value >> 16));
        WriteUInt16(output, (ushort)value);
    }
}
