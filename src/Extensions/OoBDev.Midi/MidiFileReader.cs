using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OoBDev.Midi;

/// <summary>
/// Parses standard MIDI file bytes. The input is untrusted: chunk lengths are checked against the data and the event count of a track is bounded.
/// </summary>
public static class MidiFileReader
{
    /// <summary>The default bound on events in one track.</summary>
    public const int DefaultMaxEventsPerTrack = 1_000_000;

    /// <summary>
    /// Parses a file. Chunks other than <c>MThd</c> and <c>MTrk</c> are skipped.
    /// </summary>
    /// <param name="data">The file bytes.</param>
    /// <param name="maxEventsPerTrack">The most events one track may hold.</param>
    /// <returns>The parsed file.</returns>
    /// <exception cref="InvalidDataException">The bytes are not a well-formed MIDI file or exceed the bound.</exception>
    public static MidiFile Parse(byte[] data, int maxEventsPerTrack = DefaultMaxEventsPerTrack)
    {
        ArgumentNullException.ThrowIfNull(data);
        var pos = 0;
        var (headerStart, headerLength) = ReadChunkHeader(data, ref pos, "MThd");
        if (headerLength < 6)
        {
            throw new InvalidDataException("The MThd chunk is shorter than 6 bytes.");
        }

        var format = (ushort)((data[headerStart] << 8) | data[headerStart + 1]);
        var trackCount = (data[headerStart + 2] << 8) | data[headerStart + 3];
        var division = (ushort)((data[headerStart + 4] << 8) | data[headerStart + 5]);
        pos = headerStart + headerLength;

        var tracks = new List<MidiTrack>();
        while (pos + 8 <= data.Length)
        {
            var id = Encoding.ASCII.GetString(data, pos, 4);
            var (start, length) = ReadChunkHeader(data, ref pos, id);
            if (id == "MTrk")
            {
                tracks.Add(new MidiTrack(ParseTrack(data, start, start + length, maxEventsPerTrack)));
            }

            pos = start + length;
        }

        if (tracks.Count != trackCount)
        {
            throw new InvalidDataException($"The header declares {trackCount} track(s) but {tracks.Count} were found.");
        }

        return new MidiFile(format, division, tracks);
    }

    private static (int Start, int Length) ReadChunkHeader(byte[] data, ref int pos, string expected)
    {
        if (pos + 8 > data.Length)
        {
            throw new InvalidDataException("The data ends inside a chunk header.");
        }

        if (Encoding.ASCII.GetString(data, pos, 4) != expected)
        {
            throw new InvalidDataException($"Expected a '{expected}' chunk at offset {pos}.");
        }

        var length = (long)((uint)(data[pos + 4] << 24) | (uint)(data[pos + 5] << 16) | (uint)(data[pos + 6] << 8) | data[pos + 7]);
        var start = pos + 8;
        if (start + length > data.Length)
        {
            throw new InvalidDataException($"The '{expected}' chunk at offset {pos} runs past the end of the data.");
        }

        return (start, (int)length);
    }

    private static List<MidiEvent> ParseTrack(byte[] d, int pos, int end, int maxEvents)
    {
        var events = new List<MidiEvent>();
        byte running = 0;
        while (pos < end)
        {
            if (events.Count >= maxEvents)
            {
                throw new InvalidDataException($"A track holds more than {maxEvents} events.");
            }

            var delta = ReadVariableLength(d, ref pos, end);
            if (pos >= end)
            {
                throw new InvalidDataException("A track ends after a delta time.");
            }

            events.Add(ParseEvent(d, ref pos, end, delta, ref running));
        }

        return events;
    }

    private static MidiEvent ParseEvent(byte[] d, ref int pos, int end, long delta, ref byte running)
    {
        var usedRunning = d[pos] < 0x80;
        if (usedRunning && running == 0)
        {
            throw new InvalidDataException($"Running status used with no previous status at offset {pos}.");
        }

        var status = usedRunning ? running : d[pos++];
        if (status == 0xFF)
        {
            running = 0;
            if (pos >= end)
            {
                throw new InvalidDataException("A meta event has no type.");
            }

            var type = d[pos++];
            return new MidiEvent(delta, MidiEventKind.Meta, status, ReadPayload(d, ref pos, end), type);
        }

        if (status is 0xF0 or 0xF7)
        {
            running = 0;
            return new MidiEvent(delta, status == 0xF0 ? MidiEventKind.SysEx : MidiEventKind.Escape, status, ReadPayload(d, ref pos, end));
        }

        if (status >= 0xF0)
        {
            throw new InvalidDataException($"Status byte {status:X2} is not valid in a file (offset {pos}).");
        }

        running = status;
        var count = (status >> 4) is 0xC or 0xD ? 1 : 2;
        if (pos + count > end)
        {
            throw new InvalidDataException("A channel event is cut short.");
        }

        var bytes = d[pos..(pos + count)];
        pos += count;
        foreach (var b in bytes)
        {
            if (b >= 0x80)
            {
                throw new InvalidDataException($"A channel event data byte has its top bit set (offset {pos}).");
            }
        }

        return new MidiEvent(delta, MidiEventKind.Channel, status, bytes, UsedRunningStatus: usedRunning);
    }

    private static byte[] ReadPayload(byte[] d, ref int pos, int end)
    {
        var length = ReadVariableLength(d, ref pos, end);
        if (pos + length > end)
        {
            throw new InvalidDataException("An event payload runs past the end of its track.");
        }

        var payload = d[pos..(pos + (int)length)];
        pos += (int)length;
        return payload;
    }

    private static long ReadVariableLength(byte[] d, ref int pos, int end)
    {
        long value = 0;
        for (var i = 0; i < 4; i++)
        {
            if (pos >= end)
            {
                throw new InvalidDataException("A variable-length value is cut short.");
            }

            var b = d[pos++];
            value = (value << 7) | (uint)(b & 0x7F);
            if (b < 0x80)
            {
                return value;
            }
        }

        throw new InvalidDataException("A variable-length value is longer than 4 bytes.");
    }
}
