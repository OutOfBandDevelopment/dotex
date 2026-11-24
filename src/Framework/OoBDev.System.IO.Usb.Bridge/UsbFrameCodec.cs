using System;
using System.Buffers.Binary;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Usb.Bridge;

/// <summary>
/// Reads and writes frames: <c>u32 length</c> (the bytes after it), <c>u8 op</c>, <c>u32 id</c>, payload; big-endian.
/// </summary>
public static class UsbFrameCodec
{
    /// <summary>The protocol version carried in Hello.</summary>
    public const byte Version = 1;

    /// <summary>The largest frame accepted (the bytes after the length field).</summary>
    public const int MaxFrameBytes = 16 * 1024 * 1024;

    private const int HeaderAfterLength = 5;

    /// <summary>Encodes a frame into one buffer.</summary>
    public static byte[] Encode(UsbFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        var length = HeaderAfterLength + frame.Payload.Length;
        if (length > MaxFrameBytes)
            throw new InvalidDataException($"A frame of {length} bytes is over the {MaxFrameBytes} byte limit.");
        var buffer = new byte[4 + length];
        BinaryPrimitives.WriteUInt32BigEndian(buffer, (uint)length);
        buffer[4] = (byte)frame.Op;
        BinaryPrimitives.WriteUInt32BigEndian(buffer.AsSpan(5), frame.Id);
        frame.Payload.Span.CopyTo(buffer.AsSpan(9));
        return buffer;
    }

    /// <summary>Writes a frame and flushes.</summary>
    public static async Task WriteAsync(Stream stream, UsbFrame frame, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stream);
        await stream.WriteAsync(Encode(frame), cancellationToken).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Reads one frame, or null when the stream ended cleanly before a frame started.</summary>
    public static async Task<UsbFrame?> ReadAsync(Stream stream, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var head = new byte[4];
        if (!await FillAsync(stream, head, allowEmpty: true, cancellationToken).ConfigureAwait(false))
            return null;
        var length = BinaryPrimitives.ReadUInt32BigEndian(head);
        if (length < HeaderAfterLength || length > MaxFrameBytes)
            throw new InvalidDataException($"Bad frame length {length}.");
        var body = new byte[length];
        await FillAsync(stream, body, allowEmpty: false, cancellationToken).ConfigureAwait(false);
        return new UsbFrame((UsbBridgeOp)body[0], BinaryPrimitives.ReadUInt32BigEndian(body.AsSpan(1)), body.AsMemory(HeaderAfterLength));
    }

    private static async Task<bool> FillAsync(Stream stream, byte[] buffer, bool allowEmpty, CancellationToken cancellationToken)
    {
        var read = 0;
        while (read < buffer.Length)
        {
            var count = await stream.ReadAsync(buffer.AsMemory(read), cancellationToken).ConfigureAwait(false);
            if (count == 0)
            {
                if (read == 0 && allowEmpty)
                    return false;
                throw new EndOfStreamException("The connection closed in the middle of a frame.");
            }

            read += count;
        }

        return true;
    }
}
