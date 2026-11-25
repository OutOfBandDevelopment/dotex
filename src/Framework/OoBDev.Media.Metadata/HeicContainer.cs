using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OoBDev.Media.Metadata;

/// <summary>
/// Reads what a catalog needs from a HEIC or HEIF file without decoding it: the primary image's size (<c>ispe</c>, swapped for a quarter turn <c>irot</c>, as libheif applies
/// it) and the TIFF block of the Exif item. Anything unreadable gives null.
/// </summary>
public static class HeicContainer
{
    /// <summary>What was found in the container.</summary>
    /// <param name="Width">The primary image width after rotation.</param>
    /// <param name="Height">The primary image height after rotation.</param>
    /// <param name="Tiff">The TIFF block of the Exif item, or null when there is none.</param>
    public sealed record Info(int Width, int Height, byte[]? Tiff);

    private readonly record struct Box(string Type, int Start, int End);

    /// <summary>Whether the bytes start with an ISO base media <c>ftyp</c> box of a HEIF brand.</summary>
    /// <param name="file">The file bytes.</param>
    /// <returns>True for the brands heic, heix, hevc, hevx, heim, heis, mif1 and msf1.</returns>
    public static bool IsHeic(ReadOnlySpan<byte> file)
    {
        if (file.Length < 12 || !file.Slice(4, 4).SequenceEqual("ftyp"u8))
            return false;
        var brand = Encoding.ASCII.GetString(file.Slice(8, 4));
        return brand is "heic" or "heix" or "hevc" or "hevx" or "heim" or "heis" or "mif1" or "msf1";
    }

    /// <summary>Reads the size and the Exif item of a HEIC file.</summary>
    /// <param name="file">The file bytes.</param>
    /// <returns>The information, or null when the container cannot be read or has no image size.</returns>
    public static Info? Read(ReadOnlySpan<byte> file)
    {
        try
        {
            return Parse(file.ToArray());
        }
        catch (Exception e) when (e is ArgumentOutOfRangeException or IndexOutOfRangeException or OverflowException or InvalidDataException)
        {
            return null;
        }
    }

    private static Info? Parse(byte[] f)
    {
        if (Find(f, 0, f.Length, "meta") is not { } meta)
            return null;
        var (start, end) = (meta.Start + 4, meta.End); // meta is a full box: skip version and flags

        var primary = -1;
        if (Find(f, start, end, "pitm") is { } pitm)
            primary = f[pitm.Start] == 0 ? U16(f, pitm.Start + 4) : (int)U32(f, pitm.Start + 4);

        var exifItem = -1;
        if (Find(f, start, end, "iinf") is { } iinf)
        {
            var version = f[iinf.Start];
            var p = iinf.Start + 4 + (version == 0 ? 2 : 4);
            foreach (var infe in Children(f, p, iinf.End))
            {
                if (infe.Type != "infe" || f[infe.Start] < 2)
                    continue;
                var v = f[infe.Start];
                var q = infe.Start + 4;
                var id = v == 2 ? U16(f, q) : (int)U32(f, q);
                q += (v == 2 ? 2 : 4) + 2;
                if (Encoding.ASCII.GetString(f, q, 4) == "Exif")
                    exifItem = id;
            }
        }

        byte[]? tiff = null;
        if (exifItem >= 0 && Find(f, start, end, "iloc") is { } iloc)
            tiff = ExifBlock(f, iloc, exifItem);

        int width = 0, height = 0;
        if (Find(f, start, end, "iprp") is { } iprp && Find(f, iprp.Start, iprp.End, "ipco") is { } ipco)
        {
            var props = Children(f, ipco.Start, ipco.End).ToList();
            var rotate = false;
            foreach (var index in Associated(f, iprp, primary))
            {
                if (index < 1 || index > props.Count)
                    continue;
                var prop = props[index - 1];
                if (prop.Type == "ispe")
                    (width, height) = ((int)U32(f, prop.Start + 4), (int)U32(f, prop.Start + 8));
                else if (prop.Type == "irot")
                    rotate = (f[prop.Start] & 3) % 2 == 1;
            }

            if (rotate)
                (width, height) = (height, width);
        }

        return width > 0 && height > 0 ? new Info(width, height, tiff) : null;
    }

    private static List<int> Associated(byte[] f, Box iprp, int primary)
    {
        var result = new List<int>();
        if (Find(f, iprp.Start, iprp.End, "ipma") is not { } ipma)
            return result;
        var version = f[ipma.Start];
        var wide = (f[ipma.Start + 3] & 1) != 0;
        var count = U32(f, ipma.Start + 4);
        var p = ipma.Start + 8;
        for (uint i = 0; i < count; i++)
        {
            int id;
            if (version == 0)
            {
                id = U16(f, p);
                p += 2;
            }
            else
            {
                id = (int)U32(f, p);
                p += 4;
            }

            int n = f[p++];
            for (var k = 0; k < n; k++)
            {
                int index;
                if (wide)
                {
                    index = U16(f, p) & 0x7FFF;
                    p += 2;
                }
                else
                {
                    index = f[p] & 0x7F;
                    p++;
                }

                if (id == primary)
                    result.Add(index);
            }
        }

        return result;
    }

    private static byte[]? ExifBlock(byte[] f, Box iloc, int item)
    {
        var version = f[iloc.Start];
        int offsetSize = f[iloc.Start + 4] >> 4, lengthSize = f[iloc.Start + 4] & 15, baseSize = f[iloc.Start + 5] >> 4;
        var indexSize = version is 1 or 2 ? f[iloc.Start + 5] & 15 : 0;
        var p = iloc.Start + 6;
        long count;
        if (version < 2)
        {
            count = U16(f, p);
            p += 2;
        }
        else
        {
            count = U32(f, p);
            p += 4;
        }

        for (long i = 0; i < count; i++)
        {
            int id;
            if (version < 2)
            {
                id = U16(f, p);
                p += 2;
            }
            else
            {
                id = (int)U32(f, p);
                p += 4;
            }

            if (version is 1 or 2)
                p += 2; // construction method
            p += 2; // data reference index
            var baseOffset = Number(f, p, baseSize);
            p += baseSize;
            int extents = U16(f, p);
            p += 2;
            using var data = new MemoryStream();
            for (var e = 0; e < extents; e++)
            {
                p += indexSize;
                var offset = Number(f, p, offsetSize);
                var length = Number(f, p + offsetSize, lengthSize);
                p += offsetSize + lengthSize;
                if (id == item)
                    data.Write(f, checked((int)(baseOffset + offset)), checked((int)length));
            }

            if (id != item)
                continue;
            var d = data.ToArray();
            var skip = checked((int)BinaryPrimitives.ReadUInt32BigEndian(d));
            return 4 + skip <= d.Length ? d[(4 + skip)..] : null;
        }

        return null;
    }

    private static Box? Find(byte[] f, int from, int to, string type)
    {
        foreach (var box in Children(f, from, to))
        {
            if (box.Type == type)
                return box;
        }

        return null;
    }

    private static IEnumerable<Box> Children(byte[] f, int from, int to)
    {
        var p = from;
        while (p + 8 <= to)
        {
            long size = U32(f, p);
            var type = Encoding.ASCII.GetString(f, p + 4, 4);
            var header = 8;
            if (size == 1)
            {
                size = (long)BinaryPrimitives.ReadUInt64BigEndian(f.AsSpan(p + 8));
                header = 16;
            }
            else if (size == 0)
            {
                size = to - p;
            }

            if (size < header || p + size > to)
                yield break;
            yield return new Box(type, p + header, p + (int)size);
            p += (int)size;
        }
    }

    private static int U16(byte[] f, int o) => BinaryPrimitives.ReadUInt16BigEndian(f.AsSpan(o));

    private static uint U32(byte[] f, int o) => BinaryPrimitives.ReadUInt32BigEndian(f.AsSpan(o));

    private static long Number(byte[] f, int o, int size) => size switch
    {
        0 => 0,
        4 => U32(f, o),
        8 => (long)BinaryPrimitives.ReadUInt64BigEndian(f.AsSpan(o)),
        _ => throw new ArgumentOutOfRangeException(nameof(size)),
    };
}
