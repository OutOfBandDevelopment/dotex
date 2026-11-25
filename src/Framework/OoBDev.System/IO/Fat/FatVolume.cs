using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OoBDev.System.IO.Fat;

/// <summary>
/// A read-only FAT12, FAT16 or FAT32 volume: boot sector, cluster chains, directories with long names and file content.
/// The image is untrusted input, so every size, cluster number and chain length is checked and corrupt or looping structures
/// raise <see cref="InvalidDataException"/> instead of reading on.
/// </summary>
public sealed class FatVolume
{
    private const int _entrySize = 32;
    private const int _maxDirectoryBytes = 65536 * _entrySize;
    private const int _maxFatBytes = 32 * 1024 * 1024;

    private readonly Stream _stream;
    private readonly byte[] _fat;
    private readonly int _bytesPerSector;
    private readonly int _sectorsPerCluster;
    private readonly long _rootSector;
    private readonly int _rootSectors;
    private readonly long _dataSector;
    private readonly uint _rootCluster;

    private FatVolume(Stream stream, byte[] fat, FatType type, int bytesPerSector, int sectorsPerCluster, long rootSector, int rootSectors, long dataSector, uint rootCluster, long clusterCount, string? bootLabel)
    {
        _stream = stream;
        _fat = fat;
        Type = type;
        _bytesPerSector = bytesPerSector;
        _sectorsPerCluster = sectorsPerCluster;
        _rootSector = rootSector;
        _rootSectors = rootSectors;
        _dataSector = dataSector;
        _rootCluster = rootCluster;
        ClusterCount = clusterCount;
        Label = bootLabel;
    }

    /// <summary>Gets the FAT variant.</summary>
    public FatType Type { get; }

    /// <summary>Gets the volume label: the label entry of the root directory when present, otherwise the boot sector label.</summary>
    public string? Label { get; private set; }

    /// <summary>Gets the number of data clusters.</summary>
    public long ClusterCount { get; }

    /// <summary>Gets the size of a cluster in bytes.</summary>
    public int BytesPerCluster => _bytesPerSector * _sectorsPerCluster;

    /// <summary>
    /// Opens a volume. The stream must stay open while the volume is used; the volume does not dispose it.
    /// </summary>
    /// <param name="stream">A readable, seekable stream over the whole volume (a partition image or a floppy image).</param>
    /// <returns>The volume.</returns>
    /// <exception cref="InvalidDataException">The stream is not a consistent FAT volume.</exception>
    public static FatVolume Open(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanSeek)
        {
            throw new ArgumentException("The volume stream must be readable and seekable.", nameof(stream));
        }

        var boot = new byte[512];
        ReadAt(stream, 0, boot);
        if (boot[510] != 0x55 || boot[511] != 0xAA)
        {
            throw new InvalidDataException("The boot sector signature is missing.");
        }

        var bytesPerSector = BinaryPrimitives.ReadUInt16LittleEndian(boot.AsSpan(11));
        int sectorsPerCluster = boot[13];
        int reserved = BinaryPrimitives.ReadUInt16LittleEndian(boot.AsSpan(14));
        int fatCount = boot[16];
        var rootEntries = BinaryPrimitives.ReadUInt16LittleEndian(boot.AsSpan(17));
        long totalSectors = BinaryPrimitives.ReadUInt16LittleEndian(boot.AsSpan(19));
        if (totalSectors == 0)
        {
            totalSectors = BinaryPrimitives.ReadUInt32LittleEndian(boot.AsSpan(32));
        }

        long fatSectors = BinaryPrimitives.ReadUInt16LittleEndian(boot.AsSpan(22));
        if (fatSectors == 0)
        {
            fatSectors = BinaryPrimitives.ReadUInt32LittleEndian(boot.AsSpan(36));
        }

        if (bytesPerSector is not (512 or 1024 or 2048 or 4096)
            || sectorsPerCluster is < 1 or > 128 || (sectorsPerCluster & (sectorsPerCluster - 1)) != 0
            || reserved < 1 || fatCount is < 1 or > 4 || fatSectors < 1 || totalSectors < 1)
        {
            throw new InvalidDataException("The boot sector does not describe a FAT volume.");
        }

        if (totalSectors * bytesPerSector > stream.Length)
        {
            throw new InvalidDataException("The image is shorter than the boot sector says.");
        }

        var rootSectors = ((rootEntries * _entrySize) + bytesPerSector - 1) / bytesPerSector;
        var dataSector = reserved + (fatCount * fatSectors) + rootSectors;
        if (dataSector >= totalSectors)
        {
            throw new InvalidDataException("The reserved area, tables and root directory fill the whole volume.");
        }

        var clusterCount = (totalSectors - dataSector) / sectorsPerCluster;
        var type = clusterCount < 4085 ? FatType.Fat12 : clusterCount < 65525 ? FatType.Fat16 : FatType.Fat32;
        if ((type == FatType.Fat32) != (rootEntries == 0))
        {
            throw new InvalidDataException("The root directory size does not match the FAT type.");
        }

        var fatBytes = fatSectors * bytesPerSector;
        var needed = type switch
        {
            FatType.Fat12 => (((clusterCount + 2) * 3) + 1) / 2,
            FatType.Fat16 => (clusterCount + 2) * 2,
            _ => (clusterCount + 2) * 4,
        };
        if (fatBytes > _maxFatBytes || fatBytes < needed)
        {
            throw new InvalidDataException("The allocation table size does not fit the cluster count.");
        }

        uint rootCluster = 0;
        if (type == FatType.Fat32)
        {
            rootCluster = BinaryPrimitives.ReadUInt32LittleEndian(boot.AsSpan(44));
            if (rootCluster < 2 || rootCluster >= clusterCount + 2)
            {
                throw new InvalidDataException("The root directory cluster is outside the volume.");
            }
        }

        var fat = new byte[fatBytes];
        ReadAt(stream, (long)reserved * bytesPerSector, fat);

        var bootLabel = DecodeLabel(boot.AsSpan(type == FatType.Fat32 ? 71 : 43, 11));
        var volume = new FatVolume(stream, fat, type, bytesPerSector, sectorsPerCluster, reserved + (fatCount * fatSectors), rootSectors, dataSector, rootCluster, clusterCount, bootLabel);
        volume.ParseDirectory(volume.ReadRoot(), out var rootLabel);
        volume.Label = rootLabel ?? bootLabel;
        return volume;
    }

    /// <summary>
    /// Lists a directory: the root when <paramref name="directory"/> is <see langword="null"/>. The <c>.</c> and <c>..</c> entries, deleted entries and the volume label are not returned.
    /// </summary>
    /// <param name="directory">A directory entry from an earlier listing, or <see langword="null"/> for the root.</param>
    /// <returns>The entries in directory order.</returns>
    /// <exception cref="InvalidDataException">The directory or its cluster chain is corrupt.</exception>
    public IReadOnlyList<FatDirectoryEntry> GetEntries(FatDirectoryEntry? directory = null)
    {
        if (directory is not null && !directory.IsDirectory)
        {
            throw new ArgumentException("The entry is not a directory.", nameof(directory));
        }

        var bytes = directory is null || directory.FirstCluster == 0 ? ReadRoot() : ReadChain(directory.FirstCluster, _maxDirectoryBytes);
        return ParseDirectory(bytes, out _);
    }

    /// <summary>
    /// Finds an entry by path, comparing long and short names without regard to case. Both <c>/</c> and <c>\</c> separate names.
    /// </summary>
    /// <param name="path">The path from the root, for example <c>DOCS/readme.txt</c>.</param>
    /// <returns>The entry, or <see langword="null"/> when no entry matches.</returns>
    public FatDirectoryEntry? Find(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        FatDirectoryEntry? current = null;
        foreach (var part in path.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries))
        {
            if (current is not null && !current.IsDirectory)
            {
                return null;
            }

            var parent = current;
            current = null;
            foreach (var entry in GetEntries(parent))
            {
                if (string.Equals(entry.Name, part, StringComparison.OrdinalIgnoreCase) || string.Equals(entry.ShortName, part, StringComparison.OrdinalIgnoreCase))
                {
                    current = entry;
                    break;
                }
            }

            if (current is null)
            {
                return null;
            }
        }

        return current;
    }

    /// <summary>
    /// Reads a file completely into a read-only stream.
    /// </summary>
    /// <param name="file">A file entry.</param>
    /// <returns>A stream over the file content.</returns>
    /// <exception cref="InvalidDataException">The size is larger than the volume, or the cluster chain is corrupt or ends early.</exception>
    public Stream OpenRead(FatDirectoryEntry file)
    {
        ArgumentNullException.ThrowIfNull(file);
        if (file.IsDirectory)
        {
            throw new ArgumentException("The entry is a directory.", nameof(file));
        }

        if (file.Size > ClusterCount * BytesPerCluster)
        {
            throw new InvalidDataException("The file size is larger than the volume.");
        }

        if (file.Size > Array.MaxLength)
        {
            throw new NotSupportedException("Files over 2 GiB cannot be read into memory.");
        }

        if (file.Size == 0)
        {
            return new MemoryStream([], false);
        }

        return new MemoryStream(ReadChain(file.FirstCluster, (int)file.Size, exact: true), false);
    }

    private static string? DecodeLabel(ReadOnlySpan<byte> raw)
    {
        var label = Encoding.Latin1.GetString(raw).TrimEnd();
        return label.Length == 0 || label == "NO NAME" ? null : label;
    }

    private static void ReadAt(Stream stream, long offset, Span<byte> buffer)
    {
        stream.Position = offset;
        if (stream.ReadAtLeast(buffer, buffer.Length, throwOnEndOfStream: false) < buffer.Length)
        {
            throw new InvalidDataException("The image ends before the structure being read.");
        }
    }

    private static DateTime? ToDateTime(ushort date, ushort time, int tenths)
    {
        var day = date & 0x1F;
        var month = (date >> 5) & 0xF;
        var year = 1980 + (date >> 9);
        if (date == 0 || month is < 1 or > 12 || day < 1 || day > DateTime.DaysInMonth(year, month))
        {
            return null;
        }

        var hour = time >> 11;
        var minute = (time >> 5) & 0x3F;
        var second = (time & 0x1F) * 2;
        if (tenths is < 0 or > 199)
        {
            tenths = 0;
        }

        second += tenths / 100;
        if (hour > 23 || minute > 59 || second > 59)
        {
            return null;
        }

        return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Unspecified).AddMilliseconds(tenths % 100 * 10);
    }

    private static byte Checksum(ReadOnlySpan<byte> shortName)
    {
        byte sum = 0;
        foreach (var b in shortName)
        {
            sum = (byte)(((sum & 1) << 7) + (sum >> 1) + b);
        }

        return sum;
    }

    private static string LongNamePiece(ReadOnlySpan<byte> entry)
    {
        var text = new StringBuilder(13);
        foreach (var offset in new[] { 1, 3, 5, 7, 9, 14, 16, 18, 20, 22, 24, 28, 30 })
        {
            var ch = BinaryPrimitives.ReadUInt16LittleEndian(entry[offset..]);
            if (ch == 0)
            {
                break;
            }

            if (ch != 0xFFFF)
            {
                text.Append((char)ch);
            }
        }

        return text.ToString();
    }

    private byte[] ReadRoot()
    {
        if (Type == FatType.Fat32)
        {
            return ReadChain(_rootCluster, _maxDirectoryBytes);
        }

        var bytes = new byte[_rootSectors * _bytesPerSector];
        ReadAt(_stream, _rootSector * _bytesPerSector, bytes);
        return bytes;
    }

    private List<FatDirectoryEntry> ParseDirectory(byte[] data, out string? label)
    {
        label = null;
        var entries = new List<FatDirectoryEntry>();
        var pieces = new List<string>();
        var expected = 0;
        byte checksum = 0;

        for (var at = 0; at + _entrySize <= data.Length; at += _entrySize)
        {
            var raw = data.AsSpan(at, _entrySize);
            if (raw[0] == 0)
            {
                break;
            }

            if (raw[0] == 0xE5)
            {
                pieces.Clear();
                expected = 0;
                continue;
            }

            var attributes = (FatAttributes)raw[11];
            if (((byte)attributes & 0x3F) == 0x0F)
            {
                var order = raw[0];
                if ((order & 0x40) != 0)
                {
                    pieces.Clear();
                    expected = order & 0x3F;
                    checksum = raw[13];
                }
                else if (expected == 0 || (order & 0x3F) != expected - 1 || raw[13] != checksum)
                {
                    pieces.Clear();
                    expected = 0;
                    continue;
                }
                else
                {
                    expected--;
                }

                if (expected is < 1 or > 20)
                {
                    pieces.Clear();
                    expected = 0;
                    continue;
                }

                pieces.Add(LongNamePiece(raw));
                expected = (order & 0x3F) == 1 ? -1 : expected;
                continue;
            }

            var isLongNameComplete = expected == -1 && pieces.Count > 0 && Checksum(raw[..11]) == checksum;
            var longName = isLongNameComplete ? string.Concat(Reversed(pieces)) : null;
            pieces.Clear();
            expected = 0;

            if ((attributes & FatAttributes.VolumeLabel) != 0 && (attributes & FatAttributes.Directory) == 0)
            {
                label = DecodeLabel(raw[..11]);
                continue;
            }

            if (raw[0] == (byte)'.' && (raw[1] == (byte)'.' || raw[1] == (byte)' '))
            {
                continue;
            }

            var shortName = ShortName(raw);
            var isDirectory = (attributes & FatAttributes.Directory) != 0;
            var cluster = ((uint)BinaryPrimitives.ReadUInt16LittleEndian(raw[20..]) << 16) | BinaryPrimitives.ReadUInt16LittleEndian(raw[26..]);
            if (Type != FatType.Fat32)
            {
                cluster &= 0xFFFF;
            }

            entries.Add(new FatDirectoryEntry(
                string.IsNullOrEmpty(longName) ? shortName : longName,
                shortName,
                attributes,
                isDirectory ? 0 : BinaryPrimitives.ReadUInt32LittleEndian(raw[28..]),
                cluster,
                ToDateTime(BinaryPrimitives.ReadUInt16LittleEndian(raw[16..]), BinaryPrimitives.ReadUInt16LittleEndian(raw[14..]), raw[13]),
                ToDateTime(BinaryPrimitives.ReadUInt16LittleEndian(raw[24..]), BinaryPrimitives.ReadUInt16LittleEndian(raw[22..]), 0),
                ToDateTime(BinaryPrimitives.ReadUInt16LittleEndian(raw[18..]), 0, 0)));
        }

        return entries;
    }

    private static IEnumerable<string> Reversed(List<string> pieces)
    {
        for (var i = pieces.Count - 1; i >= 0; i--)
        {
            yield return pieces[i];
        }
    }

    private static string ShortName(ReadOnlySpan<byte> raw)
    {
        Span<byte> name = stackalloc byte[8];
        raw[..8].CopyTo(name);
        if (name[0] == 0x05)
        {
            name[0] = 0xE5;
        }

        var baseName = Encoding.Latin1.GetString(name).TrimEnd();
        var extension = Encoding.Latin1.GetString(raw.Slice(8, 3)).TrimEnd();
        if ((raw[12] & 0x08) != 0)
        {
            baseName = baseName.ToLowerInvariant();
        }

        if ((raw[12] & 0x10) != 0)
        {
            extension = extension.ToLowerInvariant();
        }

        return extension.Length > 0 ? baseName + "." + extension : baseName;
    }

    private byte[] ReadChain(uint first, int limit, bool exact = false)
    {
        var bytes = new List<byte[]>();
        var total = 0;
        foreach (var cluster in Chain(first))
        {
            var take = BytesPerCluster;
            if (exact)
            {
                take = global::System.Math.Min(take, limit - total);
            }
            else if (total + take > limit)
            {
                throw new InvalidDataException("The directory is larger than a FAT directory can be.");
            }

            var part = new byte[take];
            ReadAt(_stream, (_dataSector + ((long)(cluster - 2) * _sectorsPerCluster)) * _bytesPerSector, part);
            bytes.Add(part);
            total += take;
            if (exact && total >= limit)
            {
                break;
            }
        }

        if (exact && total < limit)
        {
            throw new InvalidDataException("The cluster chain ends before the file size.");
        }

        var result = new byte[total];
        var offset = 0;
        foreach (var part in bytes)
        {
            part.CopyTo(result, offset);
            offset += part.Length;
        }

        return result;
    }

    private IEnumerable<uint> Chain(uint first)
    {
        var cluster = first;
        long count = 0;
        while (true)
        {
            if (cluster < 2 || cluster >= ClusterCount + 2)
            {
                throw new InvalidDataException($"The cluster chain reaches cluster {cluster}, which is free, bad or outside the volume.");
            }

            if (++count > ClusterCount)
            {
                throw new InvalidDataException("The cluster chain loops.");
            }

            yield return cluster;
            var next = Next(cluster);
            if (IsEnd(next))
            {
                yield break;
            }

            cluster = next;
        }
    }

    private uint Next(uint cluster)
    {
        switch (Type)
        {
            case FatType.Fat12:
                var offset12 = (int)(cluster + (cluster / 2));
                var pair = BinaryPrimitives.ReadUInt16LittleEndian(_fat.AsSpan(offset12));
                return (cluster & 1) == 0 ? (uint)(pair & 0xFFF) : (uint)(pair >> 4);
            case FatType.Fat16:
                return BinaryPrimitives.ReadUInt16LittleEndian(_fat.AsSpan((int)cluster * 2));
            default:
                return BinaryPrimitives.ReadUInt32LittleEndian(_fat.AsSpan((int)cluster * 4)) & 0x0FFFFFFF;
        }
    }

    private bool IsEnd(uint value) => Type switch
    {
        FatType.Fat12 => value >= 0xFF8,
        FatType.Fat16 => value >= 0xFFF8,
        _ => value >= 0x0FFFFFF8,
    };
}
