using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OoBDev.Media.Metadata;

/// <summary>
/// A TIFF block with its byte order: the entries of an image file directory and the typed values they point at.
/// </summary>
internal sealed class TiffBlock
{
    private readonly byte[] _bytes;
    private readonly bool _little;

    private TiffBlock(byte[] bytes, bool little)
    {
        _bytes = bytes;
        _little = little;
    }

    /// <summary>One directory entry: tag, value type, value count and where the value bytes start.</summary>
    [StructLayout(LayoutKind.Auto)]
    internal readonly record struct Entry(int Tag, int Type, int Count, int At);

    /// <summary>Opens the block, or returns <see langword="null"/> when it is too short or has no byte order mark.</summary>
    public static TiffBlock? Open(ReadOnlySpan<byte> block)
    {
        if (block.Length < 8)
            return null;
        if (block[0] == 'I' && block[1] == 'I')
            return new TiffBlock(block.ToArray(), true);
        if (block[0] == 'M' && block[1] == 'M')
            return new TiffBlock(block.ToArray(), false);
        return null;
    }

    public ushort U16(int offset) => _little ? BinaryPrimitives.ReadUInt16LittleEndian(_bytes.AsSpan(offset)) : BinaryPrimitives.ReadUInt16BigEndian(_bytes.AsSpan(offset));

    public uint U32(int offset) => _little ? BinaryPrimitives.ReadUInt32LittleEndian(_bytes.AsSpan(offset)) : BinaryPrimitives.ReadUInt32BigEndian(_bytes.AsSpan(offset));

    public long Int64(int offset) => _little ? BinaryPrimitives.ReadInt64LittleEndian(_bytes.AsSpan(offset)) : BinaryPrimitives.ReadInt64BigEndian(_bytes.AsSpan(offset));

    public byte Byte(int offset) => _bytes[offset];

    public byte[] Bytes(int offset, int count) => _bytes.AsSpan(offset, count).ToArray();

    public string Utf8(int offset, int count) => System.Text.Encoding.UTF8.GetString(_bytes, offset, count);

    /// <summary>Where the first directory starts.</summary>
    public int FirstIfd => (int)U32(4);

    /// <summary>The entries of the directory at <paramref name="offset"/>; entries that do not fit in the block or have an unknown type are skipped.</summary>
    public List<Entry> Ifd(int offset) => Ifd(offset, out _);

    /// <summary>The entries of the directory at <paramref name="offset"/>, and how many entries were skipped or cut off.</summary>
    /// <param name="offset">Where the directory starts.</param>
    /// <param name="skipped">The number of entries with an unknown type, a bad size, data outside the block, or that the block ended before.</param>
    /// <returns>The readable entries.</returns>
    public List<Entry> Ifd(int offset, out int skipped)
    {
        var list = new List<Entry>();
        skipped = 0;
        if (offset <= 0 || offset + 2 > _bytes.Length)
            return list;
        var n = U16(offset);
        for (var i = 0; i < n; i++)
        {
            var e = offset + 2 + (i * 12);
            if (e + 12 > _bytes.Length)
            {
                skipped += n - i;
                break;
            }

            var type = U16(e + 2);
            var count = (int)U32(e + 4);
            var size = TypeSize(type) * (long)count;
            if (size <= 0)
            {
                skipped++;
                continue;
            }

            var at = size <= 4 ? e + 8 : (long)U32(e + 8);
            if (at + size > _bytes.Length)
            {
                skipped++;
                continue;
            }

            list.Add(new Entry(U16(e), type, count, (int)at));
        }

        return list;
    }

    /// <summary>The first entry with the tag, or <see langword="null"/>.</summary>
    public static Entry? Find(List<Entry> entries, int tag)
    {
        foreach (var entry in entries)
            if (entry.Tag == tag)
                return entry;
        return null;
    }

    /// <summary>An unsigned rational (type 5) as a double; a zero denominator gives zero.</summary>
    public double Rational(Entry entry, int index)
    {
        var num = U32(entry.At + (index * 8));
        var den = U32(entry.At + (index * 8) + 4);
        return den == 0 ? 0 : (double)num / den;
    }

    private static int TypeSize(int type) => type switch
    {
        1 or 2 or 6 or 7 => 1,
        3 or 8 => 2,
        4 or 9 or 11 => 4,
        5 or 10 or 12 => 8,
        _ => -1,
    };
}
