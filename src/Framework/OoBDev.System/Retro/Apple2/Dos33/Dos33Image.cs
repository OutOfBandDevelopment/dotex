using System;
using System.Collections.Generic;
using System.IO;

namespace OoBDev.System.Retro.Apple2.Dos33;

/// <summary>
/// An in-memory copy of a DOS 3.3 disk image (DOS sector order, 256-byte sectors) with the sector bitmap and catalog operations the writer needs.
/// Changes stay in memory until the caller writes <see cref="Bytes"/> back, so a failed operation leaves the stream untouched.
/// </summary>
internal sealed class Dos33Image
{
    public const int SectorSize = 256;
    public const int EntrySize = 35;
    public const int EntriesPerSector = 7;
    public const int MaxPairsPerList = 122;
    public const int NameLength = 30;
    public const int MaxImageLength = 4 * 1024 * 1024;

    private const int _vtocTrack = 17;
    private const int _bitmapStart = 0x38;

    private Dos33Image(byte[] bytes, int tracks, int sectors)
    {
        Bytes = bytes;
        Tracks = tracks;
        Sectors = sectors;
    }

    public byte[] Bytes { get; }

    public int Tracks { get; }

    public int Sectors { get; }

    public static Dos33Image Load(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanWrite || !stream.CanSeek)
        {
            throw new ArgumentException("The disk image stream must be readable, writable and seekable.", nameof(stream));
        }

        if (stream.Length is < ((_vtocTrack + 1) * 16 * SectorSize) or > MaxImageLength)
        {
            throw new InvalidDataException("The stream is not a DOS 3.3 disk image size.");
        }

        var bytes = new byte[stream.Length];
        stream.Position = 0;
        stream.ReadExactly(bytes);

        var vtoc = (_vtocTrack * 16) * SectorSize;
        var tracks = bytes[vtoc + 0x34];
        var sectors = bytes[vtoc + 0x35];
        var sectorSize = bytes[vtoc + 0x36] | (bytes[vtoc + 0x37] << 8);
        if (sectorSize != SectorSize || sectors is < 1 or > 32 || tracks <= _vtocTrack || tracks > 50 || tracks * sectors * SectorSize > bytes.Length)
        {
            throw new InvalidDataException("The volume table of contents does not describe a DOS 3.3 disk.");
        }

        var image = new Dos33Image(bytes, tracks, sectors);
        if (image.VtocOffset + SectorSize > bytes.Length)
        {
            throw new InvalidDataException("The volume table of contents lies outside the image.");
        }

        return image;
    }

    public int VtocOffset => Offset(_vtocTrack, 0);

    public int Offset(int track, int sector)
    {
        if (track < 0 || track >= Tracks || sector < 0 || sector >= Sectors)
        {
            throw new InvalidDataException($"Track {track}, sector {sector} is outside the disk.");
        }

        return ((track * Sectors) + sector) * SectorSize;
    }

    public bool IsFree(int track, int sector)
    {
        var (index, bit) = BitmapPosition(track, sector);
        return (Bytes[index] & bit) != 0;
    }

    public void SetFree(int track, int sector, bool free)
    {
        var (index, bit) = BitmapPosition(track, sector);
        Bytes[index] = free ? (byte)(Bytes[index] | bit) : (byte)(Bytes[index] & ~bit);
    }

    public (int Track, int Sector)? Allocate()
    {
        for (var distance = 1; distance < Tracks; distance++)
        {
            foreach (var track in new[] { _vtocTrack + distance, _vtocTrack - distance })
            {
                if (track < 1 || track >= Tracks)
                {
                    continue;
                }

                for (var sector = Sectors - 1; sector >= 0; sector--)
                {
                    if (IsFree(track, sector))
                    {
                        SetFree(track, sector, false);
                        Bytes[VtocOffset + 0x30] = (byte)track;
                        return (track, sector);
                    }
                }
            }
        }

        return null;
    }

    public IEnumerable<int> CatalogSectorOffsets()
    {
        var track = Bytes[VtocOffset + 1];
        var sector = Bytes[VtocOffset + 2];
        for (var visited = 0; visited < Tracks * Sectors; visited++)
        {
            var offset = Offset(track, sector);
            yield return offset;
            track = Bytes[offset + 1];
            sector = Bytes[offset + 2];
            if (track == 0 && sector == 0)
            {
                yield break;
            }
        }

        throw new InvalidDataException("The catalog chain loops.");
    }

    public IEnumerable<int> EntryOffsets()
    {
        foreach (var sectorOffset in CatalogSectorOffsets())
        {
            for (var i = 0; i < EntriesPerSector; i++)
            {
                yield return sectorOffset + 0x0B + (i * EntrySize);
            }
        }
    }

    public IEnumerable<(int Track, int Sector)> ChainSectors(int track, int sector)
    {
        for (var visited = 0; visited < Tracks * Sectors; visited++)
        {
            var offset = Offset(track, sector);
            yield return (track, sector);
            for (var i = 0; i < MaxPairsPerList; i++)
            {
                var pair = offset + 0x0C + (i * 2);
                if (Bytes[pair] != 0 || Bytes[pair + 1] != 0)
                {
                    yield return (Bytes[pair], Bytes[pair + 1]);
                }
            }

            track = Bytes[offset + 1];
            sector = Bytes[offset + 2];
            if (track == 0 && sector == 0)
            {
                yield break;
            }
        }

        throw new InvalidDataException("A track/sector list chain loops.");
    }

    public static byte[] EncodeName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        var upper = name.ToUpperInvariant();
        if (upper.Length is 0 or > NameLength)
        {
            throw new ArgumentException($"A DOS 3.3 file name is 1 to {NameLength} characters.", nameof(name));
        }

        var encoded = new byte[NameLength];
        Array.Fill(encoded, (byte)0xA0);
        for (var i = 0; i < upper.Length; i++)
        {
            if (upper[i] is < ' ' or > '~' || upper[i] == ',')
            {
                throw new ArgumentException("A DOS 3.3 file name holds printable ASCII and no comma.", nameof(name));
            }

            encoded[i] = (byte)(upper[i] | 0x80);
        }

        return encoded;
    }

    public int FindEntry(byte[] encodedName)
    {
        foreach (var entry in EntryOffsets())
        {
            if (Bytes[entry] is 0 or 0xFF)
            {
                continue;
            }

            if (Bytes.AsSpan(entry + 3, NameLength).SequenceEqual(encodedName))
            {
                return entry;
            }
        }

        return -1;
    }

    private (int Index, int Bit) BitmapPosition(int track, int sector)
    {
        if (track < 0 || track >= Tracks || sector < 0 || sector >= Sectors)
        {
            throw new InvalidDataException($"Track {track}, sector {sector} is outside the disk.");
        }

        var baseIndex = VtocOffset + _bitmapStart + (track * 4);
        return sector < 8 ? (baseIndex + 1, 1 << sector) : (baseIndex, 1 << (sector - 8));
    }
}
