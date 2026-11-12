using System;
using System.IO;
using System.Linq;

namespace OoBDev.System.Retro.Apple2.Dos33;

/// <summary>
/// Writes files into an Apple DOS 3.3 disk image. See <see cref="IDiskImageWriter"/>.
/// </summary>
public class DiskImageWriter : IDiskImageWriter
{
    /// <inheritdoc />
    public void AddFile(Stream diskImage, string name, AppleFileType fileType, ReadOnlySpan<byte> data, bool replace = false)
    {
        var encodedName = Dos33Image.EncodeName(name);
        var image = Dos33Image.Load(diskImage);

        var existing = image.FindEntry(encodedName);
        if (existing >= 0)
        {
            if (!replace)
            {
                throw new InvalidOperationException($"The disk already holds a file named '{name}'.");
            }

            Remove(image, existing);
        }

        var slot = image.EntryOffsets().Cast<int?>().FirstOrDefault(offset => image.Bytes[offset!.Value] is 0 or 0xFF)
            ?? throw new InvalidOperationException("The catalog is full.");

        var (track, sector, sectorCount) = Store(image, data);

        var entry = image.Bytes.AsSpan(slot, Dos33Image.EntrySize);
        entry.Clear();
        entry[0] = (byte)track;
        entry[1] = (byte)sector;
        entry[2] = (byte)fileType;
        encodedName.CopyTo(entry[3..]);
        entry[0x21] = (byte)sectorCount;
        entry[0x22] = (byte)(sectorCount >> 8);

        Save(diskImage, image);
    }

    /// <inheritdoc />
    public bool DeleteFile(Stream diskImage, string name)
    {
        var image = Dos33Image.Load(diskImage);
        var entry = image.FindEntry(Dos33Image.EncodeName(name));
        if (entry < 0)
        {
            return false;
        }

        Remove(image, entry);
        Save(diskImage, image);
        return true;
    }

    private static void Remove(Dos33Image image, int entry)
    {
        var track = image.Bytes[entry];
        var sector = image.Bytes[entry + 1];
        foreach (var (t, s) in image.ChainSectors(track, sector).ToArray())
        {
            image.SetFree(t, s, true);
        }

        image.Bytes[entry + 0x20] = track;
        image.Bytes[entry] = 0xFF;
    }

    private static (int Track, int Sector, int SectorCount) Store(Dos33Image image, ReadOnlySpan<byte> data)
    {
        var dataSectors = (data.Length + Dos33Image.SectorSize - 1) / Dos33Image.SectorSize;
        var lists = global::System.Math.Max(1, (dataSectors + Dos33Image.MaxPairsPerList - 1) / Dos33Image.MaxPairsPerList);
        var firstList = (Track: 0, Sector: 0);
        var previousListOffset = -1;
        var written = 0;

        for (var list = 0; list < lists; list++)
        {
            var listAt = Allocate(image);
            var listOffset = image.Offset(listAt.Track, listAt.Sector);
            image.Bytes.AsSpan(listOffset, Dos33Image.SectorSize).Clear();
            if (previousListOffset < 0)
            {
                firstList = listAt;
            }
            else
            {
                image.Bytes[previousListOffset + 1] = (byte)listAt.Track;
                image.Bytes[previousListOffset + 2] = (byte)listAt.Sector;
            }

            var offsetSectors = list * Dos33Image.MaxPairsPerList;
            image.Bytes[listOffset + 5] = (byte)offsetSectors;
            image.Bytes[listOffset + 6] = (byte)(offsetSectors >> 8);

            for (var i = 0; i < Dos33Image.MaxPairsPerList && written < dataSectors; i++, written++)
            {
                var at = Allocate(image);
                image.Bytes[listOffset + 0x0C + (i * 2)] = (byte)at.Track;
                image.Bytes[listOffset + 0x0D + (i * 2)] = (byte)at.Sector;
                var chunk = data[(written * Dos33Image.SectorSize)..];
                chunk = chunk[..global::System.Math.Min(chunk.Length, Dos33Image.SectorSize)];
                var target = image.Bytes.AsSpan(image.Offset(at.Track, at.Sector), Dos33Image.SectorSize);
                target.Clear();
                chunk.CopyTo(target);
            }

            previousListOffset = listOffset;
        }

        return (firstList.Track, firstList.Sector, dataSectors + lists);
    }

    private static (int Track, int Sector) Allocate(Dos33Image image) =>
        image.Allocate() ?? throw new InvalidOperationException("The disk is full.");

    private static void Save(Stream diskImage, Dos33Image image)
    {
        diskImage.Position = 0;
        diskImage.Write(image.Bytes);
        diskImage.Flush();
    }
}
