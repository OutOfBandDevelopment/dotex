using System;
using System.Collections.Generic;
using System.Text;

namespace OoBDev.Media.Metadata.Audio;

/// <summary>
/// Reads the title, artist, album and similar tags from MP3 (ID3v2 and ID3v1) and FLAC (Vorbis comments) bytes without a third-party package.
/// A missing or damaged tag gives fewer values, never an exception.
/// </summary>
public static class AudioTagReader
{
    private const int MaxValueLength = 500;

    private static readonly Dictionary<string, string> Id3Frames = new(StringComparer.Ordinal)
    {
        ["TIT2"] = "title", ["TPE1"] = "artist", ["TALB"] = "album", ["TPE2"] = "albumartist",
        ["TRCK"] = "track", ["TPOS"] = "disc", ["TDRC"] = "date", ["TYER"] = "date", ["TCON"] = "genre", ["COMM"] = "comment",
    };

    private static readonly Dictionary<string, string> VorbisKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["TITLE"] = "title", ["ARTIST"] = "artist", ["ALBUM"] = "album", ["ALBUMARTIST"] = "albumartist",
        ["TRACKNUMBER"] = "track", ["DISCNUMBER"] = "disc", ["DATE"] = "date", ["GENRE"] = "genre", ["COMMENT"] = "comment",
    };

    private static readonly string[] Id3v1Genres =
    [
        "Blues", "Classic Rock", "Country", "Dance", "Disco", "Funk", "Grunge", "Hip-Hop", "Jazz", "Metal", "New Age", "Oldies", "Other", "Pop", "R&B", "Rap",
        "Reggae", "Rock", "Techno", "Industrial", "Alternative", "Ska", "Death Metal", "Pranks", "Soundtrack", "Euro-Techno", "Ambient", "Trip-Hop", "Vocal",
        "Jazz+Funk", "Fusion", "Trance", "Classical", "Instrumental", "Acid", "House", "Game", "Sound Clip", "Gospel", "Noise", "AlternRock", "Bass", "Soul",
        "Punk", "Space", "Meditative", "Instrumental Pop", "Instrumental Rock", "Ethnic", "Gothic", "Darkwave", "Techno-Industrial", "Electronic", "Pop-Folk",
        "Eurodance", "Dream", "Southern Rock", "Comedy", "Cult", "Gangsta", "Top 40", "Christian Rap", "Pop/Funk", "Jungle", "Native American", "Cabaret",
        "New Wave", "Psychadelic", "Rave", "Showtunes", "Trailer", "Lo-Fi", "Tribal", "Acid Punk", "Acid Jazz", "Polka", "Retro", "Musical", "Rock & Roll", "Hard Rock",
    ];

    /// <summary>Reads the tags of an MP3 or FLAC file; ID3v2 wins over ID3v1 when both exist.</summary>
    /// <param name="file">The file content (at least the first and last 128 bytes for MP3).</param>
    /// <returns>The tags, or <see cref="AudioTags.Empty"/>.</returns>
    public static AudioTags Read(ReadOnlySpan<byte> file)
    {
        if (file.Length >= 4 && file[..4].SequenceEqual("fLaC"u8))
            return ReadFlac(file);
        if (file.Length >= 4 && file[..4].SequenceEqual("OggS"u8))
            return ReadOgg(file);
        if (file.Length >= 12 && file.Slice(4, 4).SequenceEqual("ftyp"u8))
            return Mp4Tags.Read(file);
        if (file.Length >= 10 && file[..3].SequenceEqual("ID3"u8))
        {
            var v2 = ReadId3v2(file);
            if (v2.Values.Count > 0)
                return v2;
        }

        return ReadId3v1(file);
    }

    private static AudioTags ReadId3v2(ReadOnlySpan<byte> file)
    {
        var version = file[3];
        if (version is < 3 or > 4)
            return AudioTags.Empty;
        var flags = file[5];
        var size = SyncSafe(file.Slice(6, 4));
        var end = (int)Math.Min((long)size + 10, file.Length);
        var pos = 10;
        if ((flags & 0x40) != 0 && pos + 4 <= end)
        {
            var ext = version == 4 ? SyncSafe(file.Slice(pos, 4)) : BigEndian(file.Slice(pos, 4)) + 4;
            pos += (int)Math.Min(ext, end);
        }

        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        while (pos + 10 <= end && file[pos] != 0)
        {
            var id = Encoding.ASCII.GetString(file.Slice(pos, 4));
            var frameSize = version == 4 ? SyncSafe(file.Slice(pos + 4, 4)) : BigEndian(file.Slice(pos + 4, 4));
            pos += 10;
            if (frameSize > (uint)(end - pos))
                break;
            if (Id3Frames.TryGetValue(id, out var key) && !values.ContainsKey(key))
            {
                var text = id == "COMM" ? DecodeComment(file.Slice(pos, (int)frameSize)) : DecodeText(file.Slice(pos, (int)frameSize));
                if (text.Length > 0)
                    values[key] = key == "genre" ? ResolveGenre(text) : text;
            }

            pos += (int)frameSize;
        }

        return values.Count == 0 ? AudioTags.Empty : new AudioTags("id3v2", values);
    }

    private static AudioTags ReadId3v1(ReadOnlySpan<byte> file)
    {
        if (file.Length < 128 || !file.Slice(file.Length - 128, 3).SequenceEqual("TAG"u8))
            return AudioTags.Empty;
        var tag = file[^128..];
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        Add(values, "title", Latin1(tag.Slice(3, 30)));
        Add(values, "artist", Latin1(tag.Slice(33, 30)));
        Add(values, "album", Latin1(tag.Slice(63, 30)));
        Add(values, "date", Latin1(tag.Slice(93, 4)));
        if (tag[125] == 0 && tag[126] != 0)
        {
            Add(values, "comment", Latin1(tag.Slice(97, 28)));
            values["track"] = tag[126].ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
        else
        {
            Add(values, "comment", Latin1(tag.Slice(97, 30)));
        }

        if (tag[127] < Id3v1Genres.Length)
            values["genre"] = Id3v1Genres[tag[127]];
        return values.Count == 0 ? AudioTags.Empty : new AudioTags("id3v1", values);
    }

    private static AudioTags ReadFlac(ReadOnlySpan<byte> file)
    {
        var pos = 4;
        while (pos + 4 <= file.Length)
        {
            var last = (file[pos] & 0x80) != 0;
            var type = file[pos] & 0x7F;
            var length = (file[pos + 1] << 16) | (file[pos + 2] << 8) | file[pos + 3];
            pos += 4;
            if (length > file.Length - pos)
                break;
            if (type == 4)
                return ParseVorbisComments(file.Slice(pos, length));
            if (last)
                break;
            pos += length;
        }

        return AudioTags.Empty;
    }

    private static AudioTags ReadOgg(ReadOnlySpan<byte> file)
    {
        // The comment packet follows the identification packet in the first pages; a packet split by a page header is cut short and yields what was read.
        var head = file[..Math.Min(file.Length, 65536)];
        foreach (var marker in new[] { "\u0003vorbis"u8.ToArray(), "OpusTags"u8.ToArray() })
        {
            var at = head.IndexOf(marker);
            if (at >= 0)
                return ParseVorbisComments(head[(at + marker.Length)..]);
        }

        return AudioTags.Empty;
    }

    internal static AudioTags ParseVorbisComments(ReadOnlySpan<byte> block)
    {
        var pos = 0;
        if (!TryLittleEndian(block, ref pos, out var vendor) || vendor > block.Length - pos)
            return AudioTags.Empty;
        pos += (int)vendor;
        if (!TryLittleEndian(block, ref pos, out var count))
            return AudioTags.Empty;
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var i = 0u; i < count && TryLittleEndian(block, ref pos, out var length); i++)
        {
            if (length > block.Length - pos)
                break;
            var entry = Encoding.UTF8.GetString(block.Slice(pos, (int)length));
            pos += (int)length;
            var eq = entry.IndexOf('=', StringComparison.Ordinal);
            if (eq > 0 && VorbisKeys.TryGetValue(entry[..eq], out var key) && !values.ContainsKey(key))
                Add(values, key, entry[(eq + 1)..]);
        }

        return values.Count == 0 ? AudioTags.Empty : new AudioTags("vorbis", values);
    }

    private static string DecodeText(ReadOnlySpan<byte> frame)
    {
        if (frame.Length < 2)
            return string.Empty;
        return Clean(Decode(frame[0], frame[1..]));
    }

    private static string DecodeComment(ReadOnlySpan<byte> frame)
    {
        // encoding, 3 byte language, short description (terminated), then the text
        if (frame.Length < 5)
            return string.Empty;
        var encoding = frame[0];
        var body = frame[4..];
        var wide = encoding is 1 or 2;
        var step = wide ? 2 : 1;
        for (var i = 0; i + step <= body.Length; i += step)
        {
            if (body[i] == 0 && (!wide || body[i + 1] == 0))
                return Clean(Decode(encoding, body[(i + step)..]));
        }

        return string.Empty;
    }

    private static string Decode(byte encoding, ReadOnlySpan<byte> bytes) => encoding switch
    {
        1 => DecodeUtf16WithBom(bytes),
        2 => Encoding.BigEndianUnicode.GetString(bytes),
        3 => Encoding.UTF8.GetString(bytes),
        _ => Encoding.Latin1.GetString(bytes),
    };

    private static string DecodeUtf16WithBom(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            return Encoding.Unicode.GetString(bytes[2..]);
        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            return Encoding.BigEndianUnicode.GetString(bytes[2..]);
        return Encoding.Unicode.GetString(bytes);
    }

    private static string ResolveGenre(string text)
    {
        // "(13)" or "13" is an ID3v1 genre number
        var digits = text.Trim('(', ')');
        if (int.TryParse(digits, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var n) && n < Id3v1Genres.Length)
            return Id3v1Genres[n];
        return text;
    }

    private static string Latin1(ReadOnlySpan<byte> bytes) => Clean(Encoding.Latin1.GetString(bytes));

    private static string Clean(string text)
    {
        // a text frame may hold several values separated by NUL; keep the first, and drop padding
        var nul = text.IndexOf('\0', StringComparison.Ordinal);
        if (nul >= 0)
            text = text[..nul];
        text = text.Trim();
        return text.Length > MaxValueLength ? text[..MaxValueLength] : text;
    }

    private static void Add(Dictionary<string, string> values, string key, string text)
    {
        var clean = Clean(text);
        if (clean.Length > 0)
            values[key] = clean;
    }

    private static uint SyncSafe(ReadOnlySpan<byte> b) => ((uint)(b[0] & 0x7F) << 21) | ((uint)(b[1] & 0x7F) << 14) | ((uint)(b[2] & 0x7F) << 7) | (uint)(b[3] & 0x7F);

    private static uint BigEndian(ReadOnlySpan<byte> b) => ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3];

    private static bool TryLittleEndian(ReadOnlySpan<byte> b, ref int pos, out uint value)
    {
        if (pos + 4 > b.Length)
        {
            value = 0;
            return false;
        }

        value = (uint)(b[pos] | (b[pos + 1] << 8) | (b[pos + 2] << 16) | (b[pos + 3] << 24));
        pos += 4;
        return true;
    }
}
