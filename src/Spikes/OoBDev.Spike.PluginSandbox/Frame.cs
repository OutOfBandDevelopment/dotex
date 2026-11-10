using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;
using System.Text.Json;

namespace OoBDev.Spike.PluginSandbox;

/// <summary>One message: a 4-byte little-endian length, then that many bytes of UTF-8 JSON (protocol-and-manifest.md, "Framing and encoding").</summary>
public static class Frame
{
    /// <summary>The largest body a frame may carry.</summary>
    public const int MaxBody = 1_048_576;

    /// <summary>Writes <paramref name="json"/> as one frame.</summary>
    public static void Write(Stream stream, string json)
    {
        var body = Encoding.UTF8.GetBytes(json);
        var header = new byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(header, (uint)body.Length);
        stream.Write(header);
        stream.Write(body);
        stream.Flush();
    }

    /// <summary>Reads one frame; throws <see cref="FrameViolation"/> for a bad length or body, <see cref="EndOfStreamException"/> at the end of input.</summary>
    public static JsonDocument Read(Stream stream)
    {
        var header = new byte[4];
        stream.ReadExactly(header);
        var length = BinaryPrimitives.ReadUInt32LittleEndian(header);
        if (length is 0 or > MaxBody)
        {
            throw new FrameViolation($"frame length {length} is outside 1..{MaxBody}");
        }

        var body = new byte[length];
        stream.ReadExactly(body);
        try
        {
            return JsonDocument.Parse(new UTF8Encoding(false, throwOnInvalidBytes: true).GetString(body));
        }
        catch (Exception ex) when (ex is JsonException or DecoderFallbackException)
        {
            throw new FrameViolation("body is not valid UTF-8 JSON", ex);
        }
    }
}
