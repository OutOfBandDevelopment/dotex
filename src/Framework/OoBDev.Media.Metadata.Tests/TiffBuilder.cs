using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OoBDev.Media.Metadata.Tests;

/// <summary>Builds small TIFF blocks in memory, in either byte order, and wraps them as JPEG, PNG and WebP files.</summary>
internal sealed class TiffBuilder(bool little = true)
{
    private sealed record RawEntry(int Tag, int Type, int Count, byte[] Value);

    private readonly List<RawEntry> _entries = [];
    private readonly List<(int Tag, TiffBuilder Child)> _children = [];

    public TiffBuilder Child(int pointerTag, TiffBuilder child)
    {
        _children.Add((pointerTag, child));
        return this;
    }

    public TiffBuilder Ascii(int tag, string text)
    {
        var bytes = Encoding.ASCII.GetBytes(text + "\0");
        _entries.Add(new RawEntry(tag, 2, bytes.Length, bytes));
        return this;
    }

    public TiffBuilder Shorts(int tag, params ushort[] values) => Add(tag, 3, values.Length, 2, (s, v) => Write16(s, (ushort)v), values.Select(v => (long)v));

    public TiffBuilder SignedShorts(int tag, params short[] values) => Add(tag, 8, values.Length, 2, (s, v) => Write16(s, (ushort)(short)v), values.Select(v => (long)v));

    public TiffBuilder Longs(int tag, params uint[] values) => Add(tag, 4, values.Length, 4, (s, v) => Write32(s, (uint)v), values.Select(v => (long)v));

    public TiffBuilder SignedLongs(int tag, params int[] values) => Add(tag, 9, values.Length, 4, (s, v) => Write32(s, (uint)(int)v), values.Select(v => (long)v));

    public TiffBuilder Floats(int tag, params float[] values) => Add(tag, 11, values.Length, 4, (s, v) => Write32(s, (uint)(int)v), values.Select(v => (long)BitConverter.SingleToInt32Bits(v)));

    public TiffBuilder Doubles(int tag, params double[] values)
    {
        var bytes = new byte[values.Length * 8];
        for (var i = 0; i < values.Length; i++)
        {
            var bits = BitConverter.DoubleToInt64Bits(values[i]);
            if (little)
                BinaryPrimitives.WriteInt64LittleEndian(bytes.AsSpan(i * 8), bits);
            else
                BinaryPrimitives.WriteInt64BigEndian(bytes.AsSpan(i * 8), bits);
        }

        _entries.Add(new RawEntry(tag, 12, values.Length, bytes));
        return this;
    }

    public TiffBuilder Bytes(int tag, byte[] value, int type = 1)
    {
        _entries.Add(new RawEntry(tag, type, value.Length, value));
        return this;
    }

    public TiffBuilder Rationals(int tag, params (uint Num, uint Den)[] values) => RationalBytes(tag, 5, values.Select(v => (v.Num, v.Den)));

    public TiffBuilder SignedRationals(int tag, params (int Num, int Den)[] values) => RationalBytes(tag, 10, values.Select(v => ((uint)v.Num, (uint)v.Den)));

    private TiffBuilder RationalBytes(int tag, int type, IEnumerable<(uint Num, uint Den)> values)
    {
        var list = values.ToList();
        var bytes = new byte[list.Count * 8];
        for (var i = 0; i < list.Count; i++)
        {
            Write32(bytes.AsSpan(i * 8), list[i].Num);
            Write32(bytes.AsSpan((i * 8) + 4), list[i].Den);
        }

        _entries.Add(new RawEntry(tag, type, list.Count, bytes));
        return this;
    }

    private TiffBuilder Add(int tag, int type, int count, int size, Action<Span<byte>, long> write, IEnumerable<long> values)
    {
        var list = values.ToList();
        var bytes = new byte[count * size];
        for (var i = 0; i < count; i++)
            write(bytes.AsSpan(i * size), list[i]);
        _entries.Add(new RawEntry(tag, type, count, bytes));
        return this;
    }

    public byte[] Build()
    {
        var header = new byte[8];
        header[0] = header[1] = (byte)(little ? 'I' : 'M');
        if (little)
            header[2] = 42;
        else
            header[3] = 42;
        Write32(header.AsSpan(4), 8);
        return [.. header, .. Emit(8)];
    }

    private byte[] Emit(int start)
    {
        var n = _entries.Count + _children.Count;
        var ifdSize = 2 + (12 * n) + 4;
        var data = new List<byte>();
        var childOffsets = new List<int>();
        var ifd = new byte[ifdSize];
        Write16(ifd.AsSpan(0), (ushort)n);
        var slot = 0;

        foreach (var e in _entries)
        {
            var at = 2 + (12 * slot++);
            Write16(ifd.AsSpan(at), (ushort)e.Tag);
            Write16(ifd.AsSpan(at + 2), (ushort)e.Type);
            Write32(ifd.AsSpan(at + 4), (uint)e.Count);
            if (e.Value.Length <= 4)
            {
                e.Value.CopyTo(ifd.AsSpan(at + 8));
            }
            else
            {
                Write32(ifd.AsSpan(at + 8), (uint)(start + ifdSize + data.Count));
                data.AddRange(e.Value);
                if (data.Count % 2 == 1)
                    data.Add(0);
            }
        }

        var cursor = start + ifdSize + data.Count;
        var childBytes = new List<byte>();
        foreach (var (tag, child) in _children)
        {
            var at = 2 + (12 * slot++);
            Write16(ifd.AsSpan(at), (ushort)tag);
            Write16(ifd.AsSpan(at + 2), 4);
            Write32(ifd.AsSpan(at + 4), 1);
            Write32(ifd.AsSpan(at + 8), (uint)cursor);
            var emitted = child.Emit(cursor);
            childOffsets.Add(cursor);
            cursor += emitted.Length;
            childBytes.AddRange(emitted);
        }

        return [.. ifd, .. data, .. childBytes];
    }

    private void Write16(Span<byte> target, ushort value)
    {
        if (little)
            BinaryPrimitives.WriteUInt16LittleEndian(target, value);
        else
            BinaryPrimitives.WriteUInt16BigEndian(target, value);
    }

    private void Write32(Span<byte> target, uint value)
    {
        if (little)
            BinaryPrimitives.WriteUInt32LittleEndian(target, value);
        else
            BinaryPrimitives.WriteUInt32BigEndian(target, value);
    }

    public static byte[] AsJpeg(byte[] tiff)
    {
        var payload = new List<byte>(Encoding.ASCII.GetBytes("Exif\0\0"));
        payload.AddRange(tiff);
        var length = payload.Count + 2;
        return [0xFF, 0xD8, 0xFF, 0xE1, (byte)(length >> 8), (byte)length, .. payload, 0xFF, 0xD9];
    }

    public static byte[] AsPng(byte[] tiff) =>
    [
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
        0, 0, 0, 0, (byte)'I', (byte)'E', (byte)'N', (byte)'D', 0, 0, 0, 0,
        .. BigEndian(tiff.Length), .. "eXIf"u8.ToArray(), .. tiff, 0, 0, 0, 0,
    ];

    public static byte[] AsWebP(byte[] tiff, bool withHeader = false)
    {
        var blob = withHeader ? [.. "Exif\0\0"u8.ToArray(), .. tiff] : tiff;
        var chunk = new List<byte>();
        chunk.AddRange("EXIF"u8.ToArray());
        chunk.AddRange(BitConverter.GetBytes(blob.Length));
        chunk.AddRange(blob);
        if (blob.Length % 2 == 1)
            chunk.Add(0);
        return [.. "RIFF"u8.ToArray(), .. BitConverter.GetBytes(4 + chunk.Count), .. "WEBP"u8.ToArray(), .. chunk];
    }

    private static byte[] BigEndian(int value) => [(byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value];
}
