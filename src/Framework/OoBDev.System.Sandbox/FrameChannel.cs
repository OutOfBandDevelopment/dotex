using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Hashing;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace OoBDev.Spike.PluginSandbox;

/// <summary>
/// The binary wire between host and plugin (protocol-and-manifest.md, "Framing and encoding"). All integers are little-endian and packed.
/// <code>
/// off size field
///   0    1 version        (1)
///   1    1 flags          (Crc32=1, Hmac=2, CounterSign=4; other bits zero)
///   2    1 type           (FrameType)
///   3    1 format         (PayloadFormat)
///   4    4 payloadLength
///   8    8 sequence       (per direction, starts at 1, rises by exactly 1)
///  16    8 ack            (sequence answered, or 0)
///  24    2 topicLength    (0..256)
///  26    1 hops
///  27    1 reserved       (zero)
///  28    4 ttlMs          (0 = none)
///  32    topicLength      UTF-8 topic
///  ..    payloadLength    payload
///  ..    4                CRC-32 of everything before it, when Crc32
///  ..   32                HMAC-SHA256 of everything before the CRC, plus the answered request's HMAC for a counter-signed reply, when Hmac
/// </code>
/// </summary>
public sealed class FrameChannel
{
    /// <summary>The wire version this code speaks.</summary>
    public const byte Version = 1;

    /// <summary>The largest payload a frame may carry.</summary>
    public const int MaxPayload = 1_048_576;

    /// <summary>The longest topic.</summary>
    public const int MaxTopic = 256;

    /// <summary>The fixed header size.</summary>
    public const int HeaderSize = 32;

    private const int _macSize = 32;
    private const int _maxUnanswered = 64;
    private const int _knownFlagBits = 7;

    private readonly Stream _input;
    private readonly Stream _output;
    private readonly FrameFlags _required;
    private readonly byte[] _key;
    private readonly Dictionary<ulong, byte[]> _sentMacs = [];
    private ulong _sendSequence;
    private ulong _receiveSequence;

    /// <summary>Creates the channel. <paramref name="key"/> is required with <see cref="FrameFlags.Hmac"/>: 16 or more random bytes per launch.</summary>
    public FrameChannel(Stream input, Stream output, FrameFlags required, byte[]? key)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);
        if (required.HasFlag(FrameFlags.CounterSign) && !required.HasFlag(FrameFlags.Hmac))
        {
            throw new ArgumentException("CounterSign needs Hmac.", nameof(required));
        }

        if (required.HasFlag(FrameFlags.Hmac) && key is not { Length: >= 16 })
        {
            throw new ArgumentException("Hmac needs a key of at least 16 bytes.", nameof(key));
        }

        _input = input;
        _output = output;
        _required = required;
        _key = key ?? [];
    }

    /// <summary>Sends a frame that answers nothing and returns its sequence number.</summary>
    public ulong Send(FrameType type, string topic, ReadOnlySpan<byte> payload, PayloadFormat format = PayloadFormat.Json, uint ttlMs = 0, byte hops = 0)
        => Write(type, topic, payload, format, 0, [], ttlMs, hops);

    /// <summary>Sends a JSON text payload.</summary>
    public ulong Send(FrameType type, string topic, string json, uint ttlMs = 0)
        => Write(type, topic, Encoding.UTF8.GetBytes(json), PayloadFormat.Json, 0, [], ttlMs, 0);

    /// <summary>Answers <paramref name="request"/>. With <see cref="FrameFlags.CounterSign"/> the reply is bound to that request's HMAC.</summary>
    public ulong Reply(ReceivedFrame request, FrameType type, ReadOnlySpan<byte> payload, PayloadFormat format = PayloadFormat.Json)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Write(type, request.Topic, payload, format, request.Sequence, request.Mac, 0, 0);
    }

    /// <summary>Reads and verifies one frame. Any breach throws <see cref="FrameViolation"/>; end of input throws <see cref="EndOfStreamException"/>.</summary>
    public ReceivedFrame Read()
    {
        var header = new byte[HeaderSize];
        _input.ReadExactly(header);
        if (header[0] != Version)
        {
            throw new FrameViolation($"frame version {header[0]} is not {Version}");
        }

        var flags = (FrameFlags)header[1];
        if ((header[1] & ~_knownFlagBits) != 0 || header[27] != 0)
        {
            throw new FrameViolation("unknown flag bits or non-zero reserved byte");
        }

        if ((flags & _required) != _required)
        {
            throw new FrameViolation($"frame flags {flags} lack the required {_required}");
        }

        var type = (FrameType)header[2];
        var format = (PayloadFormat)header[3];
        if (!Enum.IsDefined(type) || !Enum.IsDefined(format))
        {
            throw new FrameViolation($"unknown frame type {header[2]} or format {header[3]}");
        }

        var length = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(4));
        var sequence = BinaryPrimitives.ReadUInt64LittleEndian(header.AsSpan(8));
        var ack = BinaryPrimitives.ReadUInt64LittleEndian(header.AsSpan(16));
        var topicLength = BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(24));
        var hops = header[26];
        var ttl = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(28));
        if (length > MaxPayload || topicLength > MaxTopic || (format == PayloadFormat.Json && length == 0))
        {
            throw new FrameViolation($"payload length {length} or topic length {topicLength} is outside the limits");
        }

        var content = new byte[HeaderSize + topicLength + length];
        header.CopyTo(content, 0);
        _input.ReadExactly(content.AsSpan(HeaderSize));
        if (flags.HasFlag(FrameFlags.Crc32))
        {
            var crc = new byte[4];
            _input.ReadExactly(crc);
            if (BinaryPrimitives.ReadUInt32LittleEndian(crc) != Crc(content))
            {
                throw new FrameViolation("CRC-32 mismatch");
            }
        }

        var mac = Array.Empty<byte>();
        if (flags.HasFlag(FrameFlags.Hmac))
        {
            mac = new byte[_macSize];
            _input.ReadExactly(mac);
            byte[] answered = [];
            if (ack != 0 && (!flags.HasFlag(FrameFlags.CounterSign) || !_sentMacs.Remove(ack, out answered!)))
            {
                throw new FrameViolation($"ack {ack} answers no request of ours");
            }

            if (!CryptographicOperations.FixedTimeEquals(mac, Mac(content, answered)))
            {
                throw new FrameViolation("HMAC mismatch");
            }
        }
        else if (ack != 0)
        {
            throw new FrameViolation("ack without HMAC");
        }

        if (sequence != _receiveSequence + 1)
        {
            throw new FrameViolation($"sequence {sequence} after {_receiveSequence}");
        }

        string topic;
        var payload = content.AsSpan(HeaderSize + topicLength).ToArray();
        try
        {
            var strict = new UTF8Encoding(false, throwOnInvalidBytes: true);
            topic = strict.GetString(content, HeaderSize, topicLength);
            if (format == PayloadFormat.Text)
            {
                strict.GetString(payload);
            }
            else if (format == PayloadFormat.Json)
            {
                strict.GetString(payload);
                JsonDocument.Parse(payload).Dispose();
            }
        }
        catch (Exception ex) when (ex is JsonException or DecoderFallbackException)
        {
            throw new FrameViolation("topic or payload is not valid for its declared format", ex);
        }

        _receiveSequence = sequence;
        return new ReceivedFrame { Type = type, Format = format, Sequence = sequence, Ack = ack, Topic = topic, Hops = hops, TtlMs = ttl, Payload = payload, Mac = mac };
    }

    private ulong Write(FrameType type, string topic, ReadOnlySpan<byte> payload, PayloadFormat format, ulong ack, byte[] answeredMac, uint ttlMs, byte hops)
    {
        var topicBytes = Encoding.UTF8.GetBytes(topic);
        if (topicBytes.Length > MaxTopic || payload.Length > MaxPayload)
        {
            throw new ArgumentException("topic or payload over the limit");
        }

        var flags = ack != 0 && _required.HasFlag(FrameFlags.Hmac) ? _required | FrameFlags.CounterSign : _required;
        var sequence = ++_sendSequence;
        var content = new byte[HeaderSize + topicBytes.Length + payload.Length];
        content[0] = Version;
        content[1] = (byte)flags;
        content[2] = (byte)type;
        content[3] = (byte)format;
        BinaryPrimitives.WriteUInt32LittleEndian(content.AsSpan(4), (uint)payload.Length);
        BinaryPrimitives.WriteUInt64LittleEndian(content.AsSpan(8), sequence);
        BinaryPrimitives.WriteUInt64LittleEndian(content.AsSpan(16), ack);
        BinaryPrimitives.WriteUInt16LittleEndian(content.AsSpan(24), (ushort)topicBytes.Length);
        content[26] = hops;
        BinaryPrimitives.WriteUInt32LittleEndian(content.AsSpan(28), ttlMs);
        topicBytes.CopyTo(content, HeaderSize);
        payload.CopyTo(content.AsSpan(HeaderSize + topicBytes.Length));

        using var frame = new MemoryStream();
        frame.Write(content);
        if (flags.HasFlag(FrameFlags.Crc32))
        {
            var crc = new byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(crc, Crc(content));
            frame.Write(crc);
        }

        if (flags.HasFlag(FrameFlags.Hmac))
        {
            var mac = Mac(content, ack != 0 ? answeredMac : []);
            frame.Write(mac);
            if (ack == 0)
            {
                if (_sentMacs.Count >= _maxUnanswered)
                {
                    throw new InvalidOperationException("too many unanswered frames");
                }

                _sentMacs[sequence] = mac;
            }
        }

        _output.Write(frame.GetBuffer(), 0, (int)frame.Length);
        _output.Flush();
        return sequence;
    }

    private static uint Crc(byte[] content) => Crc32.HashToUInt32(content);

    private byte[] Mac(byte[] content, byte[] answered)
    {
        using var hmac = new HMACSHA256(_key);
        hmac.TransformBlock(content, 0, content.Length, null, 0);
        hmac.TransformFinalBlock(answered, 0, answered.Length);
        return hmac.Hash!;
    }
}
