using System;
using System.IO;

namespace OoBDev.System.Retro.Apple2.Dos33;

/// <summary>
/// Detects and converts the sector order of a 16-sector Apple II disk image. <c>.dsk</c> and <c>.do</c> images hold the
/// sectors in DOS order, which the DOS 3.3 reader and writer expect; <c>.po</c> images hold them in ProDOS order.
/// </summary>
public static class DiskImageOrder
{
    private const int _sectorSize = 256;
    private const int _sectorsPerTrack = 16;
    private const int _vtocTrack = 17;

    // The same table converts either way: sector 0 and 15 stay, the others mirror (1 <-> 14, 2 <-> 13, ...).
    private static int Swap(int sector) => sector is 0 or 15 ? sector : 15 - sector;

    /// <summary>
    /// Tells whether the image reads as a DOS 3.3 disk only after its sectors are put in DOS order.
    /// </summary>
    /// <param name="diskImage">A readable, seekable image of whole 16-sector tracks.</param>
    /// <returns><see langword="true"/> when the catalog chain is longer read in ProDOS order than in DOS order (DOS chains all 15 catalog sectors).</returns>
    public static bool IsProDosOrder(Stream diskImage)
    {
        ArgumentNullException.ThrowIfNull(diskImage);
        if (!HasTracks(diskImage))
        {
            return false;
        }

        var bytes = new byte[diskImage.Length];
        diskImage.Position = 0;
        diskImage.ReadExactly(bytes);
        return CatalogLength(bytes, true) > CatalogLength(bytes, false);
    }

    /// <summary>
    /// Copies an image with its sectors moved between DOS order and ProDOS order; applying it twice returns the original.
    /// </summary>
    /// <param name="diskImage">A readable, seekable image of whole 16-sector tracks.</param>
    /// <returns>A new stream positioned at the start.</returns>
    /// <exception cref="InvalidDataException">The image is not a whole number of 16-sector tracks.</exception>
    public static MemoryStream ConvertSectorOrder(Stream diskImage)
    {
        ArgumentNullException.ThrowIfNull(diskImage);
        if (!HasTracks(diskImage))
        {
            throw new InvalidDataException("The image is not a whole number of 16-sector tracks.");
        }

        var source = new byte[diskImage.Length];
        diskImage.Position = 0;
        diskImage.ReadExactly(source);
        var target = new byte[source.Length];
        for (var track = 0; track < source.Length / (_sectorsPerTrack * _sectorSize); track++)
        {
            for (var sector = 0; sector < _sectorsPerTrack; sector++)
            {
                Buffer.BlockCopy(source, Offset(track, sector), target, Offset(track, Swap(sector)), _sectorSize);
            }
        }

        return new MemoryStream(target) { Position = 0 };
    }

    private static bool HasTracks(Stream stream) =>
        stream.Length >= (_vtocTrack + 1) * _sectorsPerTrack * _sectorSize && stream.Length % (_sectorsPerTrack * _sectorSize) == 0;

    private static int Offset(int track, int sector) => ((track * _sectorsPerTrack) + sector) * _sectorSize;

    private static int CatalogLength(byte[] bytes, bool proDosOrder)
    {
        int Read(int sector) => Offset(_vtocTrack, proDosOrder ? Swap(sector) : sector);

        var vtoc = Read(0);
        int track = bytes[vtoc + 1], sector = bytes[vtoc + 2];
        for (var length = 1; length <= _sectorsPerTrack; length++)
        {
            if (track != _vtocTrack || sector is < 1 or >= _sectorsPerTrack)
            {
                return 0;
            }

            var at = Read(sector);
            if (bytes[at] != 0)
            {
                return 0;
            }

            track = bytes[at + 1];
            sector = bytes[at + 2];
            if (track == 0 && sector == 0)
            {
                return length;
            }
        }

        return 0;
    }
}