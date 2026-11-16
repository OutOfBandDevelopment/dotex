using System;
using System.Collections.Generic;
using System.Text;

namespace OoBDev.Media.Metadata.Raw;

/// <summary>Recognises camera RAW files from their first bytes: the Canon CR3 and Fuji RAF containers, Olympus and Panasonic headers, and TIFF-based files (CR2, NEF, ARW, DNG and others).</summary>
public static class RawDetector
{
    private const int TagMake = 271;
    private const int TagSubIfds = 330;
    private const int TagDngVersion = 50706;

    private static readonly string[] Makers =
    [
        "NIKON", "Canon", "SONY", "OLYMPUS", "PENTAX", "RICOH", "Panasonic", "LEICA", "FUJIFILM", "SAMSUNG", "Hasselblad", "Phase One",
    ];

    /// <summary>Tests the leading bytes of a file (a megabyte is plenty).</summary>
    /// <param name="head">The first bytes.</param>
    public static bool IsRaw(ReadOnlySpan<byte> head)
    {
        if (head.Length < 12)
        {
            return false;
        }

        if (head.StartsWith("FUJIFILMCCD-RAW"u8))
        {
            return true;
        }

        if (head[4] == 'f' && head[5] == 't' && head[6] == 'y' && head[7] == 'p' && head.Slice(8, 4).SequenceEqual("crx "u8))
        {
            return true;
        }

        if (head.StartsWith("IIRO"u8) || head.StartsWith("IIRS"u8) || head.StartsWith("MMOR"u8) || head.StartsWith("IIU\0"u8))
        {
            return true;
        }

        if (head[8] == 'C' && head[9] == 'R' && (head.StartsWith("II*\0"u8)))
        {
            return true;
        }

        var block = TiffBlock.Open(head);
        if (block is null)
        {
            return false;
        }

        var magic = block.U16(2);
        if (magic != 42)
        {
            return false;
        }

        var ifd0 = ReadIfd(block, block.FirstIfd, head.Length);
        if (TiffBlock.Find(ifd0, TagDngVersion) is not null)
        {
            return true;
        }

        if (TiffBlock.Find(ifd0, TagMake) is { } make && TiffBlock.Find(ifd0, TagSubIfds) is not null)
        {
            var text = Encoding.ASCII.GetString(head.Slice(make.At, Math.Min(make.Count, head.Length - make.At)));
            foreach (var maker in Makers)
            {
                if (text.Contains(maker, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static List<TiffBlock.Entry> ReadIfd(TiffBlock block, int offset, int length) =>
        offset > 0 && offset + 2 < length ? block.Ifd(offset) : [];
}
