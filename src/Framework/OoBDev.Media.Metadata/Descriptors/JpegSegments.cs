using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace OoBDev.Media.Metadata.Descriptors;

internal static class JpegSegments
{
    /// <summary>Yields the payload (after the length) of every marker segment before the image data.</summary>
    public static IEnumerable<(byte Marker, byte[] Payload)> Read(byte[] file)
    {
        if (file.Length < 4 || file[0] != 0xFF || file[1] != 0xD8)
            yield break;
        var i = 2;
        while (i + 4 <= file.Length && file[i] == 0xFF)
        {
            var marker = file[i + 1];
            if (marker is 0xD9 or 0xDA)
                yield break;
            var length = BinaryPrimitives.ReadUInt16BigEndian(file.AsSpan(i + 2));
            if (length < 2 || i + 2 + length > file.Length)
                yield break;
            yield return (marker, file.AsSpan(i + 4, length - 2).ToArray());
            i += 2 + length;
        }
    }
}
