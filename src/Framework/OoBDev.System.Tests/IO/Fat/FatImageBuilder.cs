using OoBDev.System.IO.Fat;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

namespace OoBDev.System.Tests.IO.Fat;

/// <summary>
/// Builds small FAT images for tests: root files, one level of subdirectories, optional long names, contiguous clusters.
/// </summary>
internal sealed class FatImageBuilder
{
    private const int _bytesPerSector = 512;

    private readonly FatType _type;
    private readonly int _sectorsPerCluster;
    private readonly int _reserved;
    private readonly int _rootEntries;
    private readonly int _totalSectors;
    private readonly int _fatSectors;
    private readonly List<Item> _items = [];

    public FatImageBuilder(FatType type)
    {
        _type = type;
        (_sectorsPerCluster, _reserved, _rootEntries, _totalSectors, _fatSectors) = type switch
        {
            FatType.Fat12 => (1, 1, 224, 2880, 9),
            FatType.Fat16 => (4, 1, 512, 40960, 40),
            _ => (1, 32, 0, 70000, 547),
        };
    }

    public int FatOffset => _reserved * _bytesPerSector;

    public int BytesPerCluster => _sectorsPerCluster * _bytesPerSector;

    public string? Label { get; set; }

    public int FirstCluster(string shortName) => _items.Find(i => i.ShortName == shortName)!.FirstCluster;

    public void AddFile(string? directory, string shortName, byte[] data, string? longName = null, bool deleted = false) =>
        _items.Add(new Item(directory, shortName, data, longName, false, deleted));

    public void AddDirectory(string shortName, string? longName = null) =>
        _items.Add(new Item(null, shortName, [], longName, true, false));

    public byte[] Build()
    {
        var image = new byte[_totalSectors * _bytesPerSector];
        WriteBoot(image);
        var fatStart = FatOffset;
        var next = 2;

        int Allocate(int bytes)
        {
            var count = global::System.Math.Max(1, (bytes + BytesPerCluster - 1) / BytesPerCluster);
            var first = next;
            for (var i = 0; i < count; i++)
            {
                SetFat(image, fatStart, next, i == count - 1 ? EndMarker : (uint)(next + 1));
                next++;
            }

            return first;
        }

        SetFat(image, fatStart, 0, 0x0FFFFFF8);
        SetFat(image, fatStart, 1, 0x0FFFFFFF);

        var rootCluster = 0;
        if (_type == FatType.Fat32)
        {
            rootCluster = Allocate(DirectoryBytesLength(null));
            WriteU32(image, 44, (uint)rootCluster);
        }

        foreach (var item in _items)
        {
            if (!item.IsDirectory)
            {
                item.FirstCluster = item.Data.Length == 0 ? 0 : Allocate(item.Data.Length);
            }
        }

        foreach (var item in _items)
        {
            if (item.IsDirectory)
            {
                item.FirstCluster = Allocate(DirectoryBytesLength(item.ShortName));
            }
        }

        foreach (var item in _items)
        {
            if (!item.IsDirectory && item.Data.Length > 0)
            {
                item.Data.CopyTo(image, DataOffset(item.FirstCluster));
            }
        }

        foreach (var directory in _items.FindAll(i => i.IsDirectory))
        {
            DirectoryBytes(directory.ShortName, directory.FirstCluster).CopyTo(image, DataOffset(directory.FirstCluster));
        }

        var rootBytes = DirectoryBytes(null, 0);
        if (_type == FatType.Fat32)
        {
            rootBytes.CopyTo(image, DataOffset(rootCluster));
        }
        else
        {
            rootBytes.CopyTo(image, (_reserved + (2 * _fatSectors)) * _bytesPerSector);
        }

        var second = fatStart + (_fatSectors * _bytesPerSector);
        Array.Copy(image, fatStart, image, second, _fatSectors * _bytesPerSector);
        return image;
    }

    private uint EndMarker => _type switch { FatType.Fat12 => 0xFFF, FatType.Fat16 => 0xFFFF, _ => 0x0FFFFFFF };

    private int DataOffset(int cluster)
    {
        var rootSectors = ((_rootEntries * 32) + _bytesPerSector - 1) / _bytesPerSector;
        var dataSector = _reserved + (2 * _fatSectors) + rootSectors;
        return (dataSector + ((cluster - 2) * _sectorsPerCluster)) * _bytesPerSector;
    }

    private void SetFat(byte[] image, int start, int cluster, uint value)
    {
        switch (_type)
        {
            case FatType.Fat12:
                var offset = start + cluster + (cluster / 2);
                var pair = BinaryPrimitives.ReadUInt16LittleEndian(image.AsSpan(offset));
                pair = (cluster & 1) == 0 ? (ushort)((pair & 0xF000) | (value & 0xFFF)) : (ushort)((pair & 0x000F) | ((value & 0xFFF) << 4));
                BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(offset), pair);
                break;
            case FatType.Fat16:
                BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(start + (cluster * 2)), (ushort)value);
                break;
            default:
                BinaryPrimitives.WriteUInt32LittleEndian(image.AsSpan(start + (cluster * 4)), value);
                break;
        }
    }

    private void WriteBoot(byte[] image)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(11), _bytesPerSector);
        image[13] = (byte)_sectorsPerCluster;
        BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(14), (ushort)_reserved);
        image[16] = 2;
        BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(17), (ushort)_rootEntries);
        if (_totalSectors < 65536)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(19), (ushort)_totalSectors);
        }
        else
        {
            BinaryPrimitives.WriteUInt32LittleEndian(image.AsSpan(32), (uint)_totalSectors);
        }

        if (_type == FatType.Fat32)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(image.AsSpan(36), (uint)_fatSectors);
        }
        else
        {
            BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(22), (ushort)_fatSectors);
        }

        var labelAt = _type == FatType.Fat32 ? 71 : 43;
        Encoding.Latin1.GetBytes("NO NAME    ").CopyTo(image, labelAt);
        image[510] = 0x55;
        image[511] = 0xAA;
    }

    private static void WriteU32(byte[] image, int offset, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(image.AsSpan(offset), value);

    private int DirectoryBytesLength(string? directory)
    {
        if (directory is null && _type != FatType.Fat32)
        {
            return _rootEntries * 32;
        }

        var count = directory is null ? (Label is null ? 0 : 1) : 2;
        foreach (var item in _items)
        {
            if (item.Directory == directory)
            {
                count += 1 + (item.LongName is null ? 0 : (item.LongName.Length + 12) / 13);
            }
        }

        return RoundUp(global::System.Math.Max(32, count * 32));
    }

    private byte[] DirectoryBytes(string? directory, int ownCluster)
    {
        var bytes = new byte[DirectoryBytesLength(directory)];
        var at = 0;

        if (directory is null && Label is not null)
        {
            Entry(bytes, ref at, Pad(Label, 11), 0x08, 0, 0);
        }

        if (directory is not null)
        {
            Entry(bytes, ref at, Pad(".", 11), 0x10, ownCluster, 0);
            Entry(bytes, ref at, Pad("..", 11), 0x10, 0, 0);
        }

        foreach (var item in _items)
        {
            if (item.Directory != directory)
            {
                continue;
            }

            var shortName = Encoding.Latin1.GetBytes(item.ShortName);
            if (item.LongName is not null)
            {
                WriteLongName(bytes, ref at, item.LongName, shortName, item.Deleted);
            }

            Entry(bytes, ref at, shortName, item.IsDirectory ? (byte)0x10 : (byte)0x20, item.FirstCluster, item.Data.Length);
            if (item.Deleted)
            {
                bytes[at - 32] = 0xE5;
            }
        }

        return bytes;
    }

    private int RoundUp(int length) => (length + BytesPerCluster - 1) / BytesPerCluster * BytesPerCluster;

    private static byte[] Pad(string name, int length)
    {
        var raw = new byte[length];
        Array.Fill(raw, (byte)' ');
        Encoding.Latin1.GetBytes(name).CopyTo(raw, 0);
        return raw;
    }

    private static void Entry(byte[] bytes, ref int at, byte[] name, byte attributes, int cluster, int size)
    {
        name.CopyTo(bytes, at);
        bytes[at + 11] = attributes;
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(at + 20), (ushort)(cluster >> 16));
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(at + 26), (ushort)cluster);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(at + 28), (uint)size);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(at + 24), (ushort)(((2024 - 1980) << 9) | (3 << 5) | 15));
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(at + 22), (ushort)((13 << 11) | (45 << 5) | 15));
        at += 32;
    }

    private static void WriteLongName(byte[] bytes, ref int at, string longName, byte[] shortName, bool deleted)
    {
        byte sum = 0;
        foreach (var b in shortName)
        {
            sum = (byte)(((sum & 1) << 7) + (sum >> 1) + b);
        }

        var pieces = (longName.Length + 12) / 13;
        for (var piece = pieces; piece >= 1; piece--)
        {
            var span = bytes.AsSpan(at, 32);
            span[0] = (byte)(piece | (piece == pieces ? 0x40 : 0));
            span[11] = 0x0F;
            span[13] = sum;
            var offsets = new[] { 1, 3, 5, 7, 9, 14, 16, 18, 20, 22, 24, 28, 30 };
            for (var i = 0; i < 13; i++)
            {
                var index = ((piece - 1) * 13) + i;
                ushort ch = index < longName.Length ? longName[index] : index == longName.Length ? (ushort)0 : (ushort)0xFFFF;
                BinaryPrimitives.WriteUInt16LittleEndian(span[offsets[i]..], ch);
            }

            if (deleted)
            {
                span[0] = 0xE5;
            }

            at += 32;
        }
    }

    private sealed class Item(string? directory, string shortName, byte[] data, string? longName, bool isDirectory, bool deleted)
    {
        public string? Directory { get; } = directory;

        public string ShortName { get; } = shortName;

        public byte[] Data { get; } = data;

        public string? LongName { get; } = longName;

        public bool IsDirectory { get; } = isDirectory;

        public bool Deleted { get; } = deleted;

        public int FirstCluster { get; set; }
    }
}
