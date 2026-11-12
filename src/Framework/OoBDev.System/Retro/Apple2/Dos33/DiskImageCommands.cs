using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OoBDev.System.Retro.Apple2.Dos33;

/// <summary>
/// set of commands for AppleSoft DOS 3.3 disk images.
/// </summary>
public class DiskImageCommands : IDiskImageCommands
{
    private const int _maxTrackSectorLists = 600;

    /// <summary>
    /// Retrieve Volume table of contents.
    /// </summary>
    /// <param name="diskImage"></param>
    /// <returns></returns>
    public VolumeTableOfContents GetVolumeTableOfContents(Stream diskImage)
    {
        //var diskTracks = 35;
        var trackSectors = 16;
        var sectorLength = 256;

        var trackNumber = 0x11;
        var sectorNumber = 0;

        var vtocLocation = trackNumber * trackSectors * sectorLength + sectorNumber * sectorLength;
        diskImage.Position = vtocLocation;
        var sector = new byte[sectorLength];
        Span<byte> sectorSpan = sector;
        var read = diskImage.Read(sectorSpan);

        var vtoc = new VolumeTableOfContents(sectorSpan[..read]);
        return vtoc;
    }

    /// <summary>
    /// Returns list of catalog entries with files
    /// </summary>
    /// <param name="diskImage"></param>
    /// <returns></returns>
    public IEnumerable<CatalogEntry> GetCatalogs(Stream diskImage)
    {
        var vtoc = GetVolumeTableOfContents(diskImage);

        var track = vtoc.FirstCatalogTrack;
        var sector = vtoc.FirstCatalogSector;
        var trackSectors = vtoc.NumberOfSectorsPerTrack;
        var sectorLength = vtoc.NumberOfBytesPerSector;

        var buffer = new byte[sectorLength];
    next:

        Span<byte> catalogSpan = buffer;
        var location = track * trackSectors * sectorLength + sector * sectorLength;

        diskImage.Position = location;
        var read = diskImage.Read(catalogSpan);
        var item = new CatalogEntry(catalogSpan[..read]);
        yield return item;

        if (item.NextCatalogTrack != 0 && item.NextSectorTrack != 0)
        {
            track = item.NextCatalogTrack;
            sector = item.NextSectorTrack;
            goto next;
        }
    }

    /// <summary>
    /// Returns list of populated track/sectors for given file
    /// </summary>
    /// <param name="diskImage"></param>
    /// <param name="fileEntry"></param>
    /// <returns></returns>
    public IEnumerable<TrackSectorList> GetTrackSectorListForFileEntry(Stream diskImage, FileEntry fileEntry)
    {
        var vtoc = GetVolumeTableOfContents(diskImage);

        var track = fileEntry.Track;
        var sector = fileEntry.Sector;
        var trackSectors = vtoc.NumberOfSectorsPerTrack;
        var sectorLength = vtoc.NumberOfBytesPerSector;

        var buffer = new byte[sectorLength];
        var lists = 0;
    next:

        Span<byte> span = buffer;
        var location = track * trackSectors * sectorLength + sector * sectorLength;

        diskImage.Position = location;
        var read = diskImage.Read(span);
        var item = new TrackSectorList(span[..read]);
        yield return item;

        if (++lists > _maxTrackSectorLists)
        {
            throw new InvalidDataException("The track/sector list chain does not end.");
        }

        if (item.NextTrack != 0 && item.NextSector != 0)
        {
            track = item.NextTrack;
            sector = item.NextSector;
            goto next;
        }
    }

    /// <summary>
    /// retrieve binary data for input file
    /// </summary>
    /// <param name="diskImage"></param>
    /// <param name="fileEntry"></param>
    /// <returns></returns>
    public Span<byte> GetDataFileEntry(Stream diskImage, FileEntry fileEntry)
    {
        var ts = (from trackSectorList in GetTrackSectorListForFileEntry(diskImage, fileEntry)
                  from entry in trackSectorList.TrackSectorPairs.Select((pair, index) => (pair, position: trackSectorList.SectorOffset + index))
                  where entry.pair.Track != 0 || entry.pair.Sector != 0
                  select entry).ToArray();

        var vtoc = GetVolumeTableOfContents(diskImage);
        var trackSectors = vtoc.NumberOfSectorsPerTrack;
        var sectorLength = vtoc.NumberOfBytesPerSector;

        var sectorCount = ts.Length == 0 ? 0 : ts.Max(e => e.position) + 1;
        if (sectorCount > trackSectors * 255)
        {
            throw new InvalidDataException("The track/sector lists describe more sectors than the disk holds.");
        }

        var outputBuffer = new byte[sectorLength * sectorCount];
        Span<byte> outputSpan = outputBuffer;

        foreach (var (pair, position) in ts)
        {
            var location = pair.Track * trackSectors * sectorLength + pair.Sector * sectorLength;
            diskImage.Position = location;
            diskImage.ReadExactly(outputSpan.Slice(position * sectorLength, sectorLength));
        }

        return outputSpan;
    }
}
