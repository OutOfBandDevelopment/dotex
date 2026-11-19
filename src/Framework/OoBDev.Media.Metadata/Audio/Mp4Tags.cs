using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OoBDev.Media.Metadata.Audio;

/// <summary>
/// Reads the iTunes style tags (<c>moov/udta/meta/ilst</c>) of M4A and MP4 files.
/// </summary>
internal static class Mp4Tags
{
    private static readonly Dictionary<string, string> Keys = new(StringComparer.Ordinal)
    {
        ["©nam"] = "title", ["©ART"] = "artist", ["©alb"] = "album", ["aART"] = "albumartist",
        ["©day"] = "date", ["©gen"] = "genre", ["©cmt"] = "comment", ["trkn"] = "track", ["disk"] = "disc",
    };

    public static AudioTags Read(ReadOnlySpan<byte> file)
    {
        var ilst = Find(file, ["moov", "udta", "meta", "ilst"]);
        if (ilst.IsEmpty)
            return AudioTags.Empty;
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (type, body) in Boxes(ilst))
        {
            if (!Keys.TryGetValue(type, out var key) || values.ContainsKey(key))
                continue;
            foreach (var (inner, data) in Boxes(body))
            {
                if (inner != "data" || data.Length < 8)
                    continue;
                var kind = data[3];
                var payload = data[8..];
                var text = kind == 0 && payload.Length >= 6
                    ? NumberPair(payload)
                    : kind == 1 ? Encoding.UTF8.GetString(payload).Trim() : string.Empty;
                if (text.Length > 0)
                    values[key] = text.Length > 500 ? text[..500] : text;
                break;
            }
        }

        return values.Count == 0 ? AudioTags.Empty : new AudioTags("mp4", values);
    }

    private static string NumberPair(ReadOnlySpan<byte> p)
    {
        var number = (p[2] << 8) | p[3];
        var total = (p[4] << 8) | p[5];
        if (number == 0)
            return string.Empty;
        return total == 0
            ? number.ToString(CultureInfo.InvariantCulture)
            : string.Create(CultureInfo.InvariantCulture, $"{number}/{total}");
    }

    private static ReadOnlySpan<byte> Find(ReadOnlySpan<byte> data, string[] path)
    {
        var current = data;
        foreach (var name in path)
        {
            var found = false;
            foreach (var (type, body) in Boxes(current))
            {
                if (type != name)
                    continue;
                // meta is a full box: four bytes of version and flags precede its children
                current = name == "meta" && body.Length >= 4 ? body[4..] : body;
                found = true;
                break;
            }

            if (!found)
                return default;
        }

        return current;
    }

    private static List<(string Type, byte[] Body)> Boxes(ReadOnlySpan<byte> data)
    {
        var list = new List<(string, byte[])>();
        var pos = 0;
        while (pos + 8 <= data.Length)
        {
            long size = (data[pos] << 24) | (data[pos + 1] << 16) | (data[pos + 2] << 8) | data[pos + 3];
            if (size < 8 || size > data.Length - pos)
                break;
            list.Add((Encoding.Latin1.GetString(data.Slice(pos + 4, 4)), data.Slice(pos + 8, (int)size - 8).ToArray()));
            pos += (int)size;
        }

        return list;
    }

}
