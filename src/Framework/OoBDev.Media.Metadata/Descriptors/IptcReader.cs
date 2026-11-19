using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

namespace OoBDev.Media.Metadata.Descriptors;

/// <summary>Reads the IPTC-IIM record 2 fields from the Photoshop APP13 segment of a JPEG. Malformed data gives what was read so far.</summary>
public static class IptcReader
{
    private static ReadOnlySpan<byte> Photoshop => "Photoshop 3.0\0"u8;

    /// <summary>Reads the descriptors of a JPEG file.</summary>
    /// <param name="jpeg">The file bytes.</param>
    /// <returns>The fields, or <see cref="ImageDescriptors.Empty"/>.</returns>
    public static ImageDescriptors Read(byte[] jpeg)
    {
        ArgumentNullException.ThrowIfNull(jpeg);
        string? title = null, creator = null, caption = null, created = null;
        var keywords = new List<string>();
        foreach (var (marker, payload) in JpegSegments.Read(jpeg))
        {
            if (marker != 0xED || payload.Length < 14 || !payload.AsSpan(0, 14).SequenceEqual(Photoshop))
                continue;
            var iim = FindIim(payload);
            var p = 0;
            while (p + 5 <= iim.Length && iim[p] == 0x1C)
            {
                var (record, dataset) = (iim[p + 1], iim[p + 2]);
                int length = BinaryPrimitives.ReadUInt16BigEndian(iim.AsSpan(p + 3));
                if ((length & 0x8000) != 0 || p + 5 + length > iim.Length)
                    break;
                if (record == 2)
                {
                    var text = Encoding.UTF8.GetString(iim, p + 5, length).Trim('\0', ' ');
                    if (text.Length > 0)
                    {
                        switch (dataset)
                        {
                            case 5:
                                title ??= text;
                                break;
                            case 25:
                                keywords.Add(text);
                                break;
                            case 55:
                                created ??= text;
                                break;
                            case 80:
                                creator ??= text;
                                break;
                            case 120:
                                caption ??= text;
                                break;
                        }
                    }
                }

                p += 5 + length;
            }
        }

        var found = new ImageDescriptors(title, creator, caption, keywords, null, created);
        return found.IsEmpty ? ImageDescriptors.Empty : found;
    }

    private static byte[] FindIim(byte[] payload)
    {
        var p = 14;
        while (p + 12 <= payload.Length && payload.AsSpan(p, 4).SequenceEqual("8BIM"u8))
        {
            var id = BinaryPrimitives.ReadUInt16BigEndian(payload.AsSpan(p + 4));
            var nameLength = payload[p + 6];
            var q = p + 7 + nameLength;
            if ((1 + nameLength) % 2 == 1)
                q++;
            if (q + 4 > payload.Length)
                break;
            var size = BinaryPrimitives.ReadUInt32BigEndian(payload.AsSpan(q));
            q += 4;
            if (size > payload.Length - q)
                break;
            if (id == 0x0404)
                return payload.AsSpan(q, (int)size).ToArray();
            p = q + (int)size + ((int)size & 1);
        }

        return [];
    }
}
