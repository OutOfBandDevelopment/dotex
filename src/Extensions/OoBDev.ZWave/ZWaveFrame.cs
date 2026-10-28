using OoBDev.System.IO.Functions;
using System;

namespace OoBDev.ZWave;

/// <summary>
/// One unit of the Z-Wave Serial API byte stream. For <see cref="ZWaveFrameKind.Data"/> the accessors below are valid.
/// </summary>
/// <param name="Kind">What the bytes are.</param>
/// <param name="Raw">The bytes exactly as they travel on the wire.</param>
public sealed record ZWaveFrame(ZWaveFrameKind Kind, byte[] Raw)
{
    /// <summary>The leading byte of a data frame.</summary>
    public const byte StartOfFrame = 0x01;

    /// <summary>The lone byte that accepts a data frame.</summary>
    public const byte AckByte = 0x06;

    /// <summary>The lone byte that rejects a data frame.</summary>
    public const byte NakByte = 0x15;

    /// <summary>The lone byte that drops a data frame because the receiver is busy or out of sync.</summary>
    public const byte CanByte = 0x18;

    /// <summary>The frame type of a request, or an unsolicited callback.</summary>
    public const byte Request = 0x00;

    /// <summary>The frame type of the reply to a request.</summary>
    public const byte Response = 0x01;

    private static readonly ChecksumCalculator _checksum = new();

    /// <summary>Gets a lone ACK, sent for every data frame the controller sends.</summary>
    public static ZWaveFrame Ack { get; } = new(ZWaveFrameKind.Ack, [AckByte]);

    /// <summary>Gets the frame type: <see cref="Request"/> or <see cref="Response"/>.</summary>
    public byte Type => Raw[2];

    /// <summary>Gets a value indicating whether this is the reply to a request.</summary>
    public bool IsResponse => Kind == ZWaveFrameKind.Data && Type == Response;

    /// <summary>Gets the Serial API function id, for example <c>0x15</c> for GetVersion.</summary>
    public byte Function => Raw[3];

    /// <summary>Gets the function's parameters, between the function id and the checksum.</summary>
    public ReadOnlySpan<byte> Payload => Raw.AsSpan(4, Raw.Length - 5);

    /// <summary>
    /// Builds a complete data frame: <c>01 len type function payload checksum</c>.
    /// </summary>
    /// <param name="type">The frame type, <see cref="Request"/> or <see cref="Response"/>.</param>
    /// <param name="function">The Serial API function id.</param>
    /// <param name="payload">The function's parameters.</param>
    /// <returns>The data frame, checksum included.</returns>
    public static ZWaveFrame Create(byte type, byte function, ReadOnlySpan<byte> payload)
    {
        var raw = new byte[payload.Length + 5];
        raw[0] = StartOfFrame;
        raw[1] = (byte)(payload.Length + 3);
        raw[2] = type;
        raw[3] = function;
        payload.CopyTo(raw.AsSpan(4));
        raw[^1] = Checksum(raw.AsSpan(1, raw.Length - 2));
        return new ZWaveFrame(ZWaveFrameKind.Data, raw);
    }

    /// <summary>
    /// Calculates the frame checksum: <c>0xFF</c> exclusive-or every byte from the length byte through the last payload byte.
    /// </summary>
    /// <param name="lengthThroughPayload">The bytes between the start-of-frame byte and the checksum byte.</param>
    /// <returns>The checksum byte.</returns>
    public static byte Checksum(ReadOnlySpan<byte> lengthThroughPayload) =>
        (byte)(0xFF ^ _checksum.Xor8(lengthThroughPayload));
}
