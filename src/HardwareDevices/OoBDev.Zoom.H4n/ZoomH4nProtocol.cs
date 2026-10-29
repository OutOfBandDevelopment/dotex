using System;

namespace OoBDev.Zoom.H4n;

/// <summary>
/// Constants of the RC04 / RC2 remote protocol (serial, 2400 baud, 8-N-1, no handshake). The recorder ignores everything until the init
/// handshake has run: send <see cref="ProbeByte"/> about every <see cref="HandshakeAttemptTimeout"/> (at most
/// <see cref="MaxHandshakeAttempts"/> times) until a reply byte with the high bit set arrives (<see cref="IsWakeByte"/>), then send
/// <see cref="WakeSequence"/>. Running it is the caller's job, because only the caller owns the port.
/// </summary>
public static class ZoomH4nProtocol
{
    /// <summary>The baud rate of the remote port.</summary>
    public const int BaudRate = 2400;

    /// <summary>The byte repeated until the recorder answers.</summary>
    public const byte ProbeByte = 0x00;

    /// <summary>The most probes to send before sending the wake sequence anyway.</summary>
    public const int MaxHandshakeAttempts = 1024;

    /// <summary>How long to wait for a reply after each probe.</summary>
    public static readonly TimeSpan HandshakeAttemptTimeout = TimeSpan.FromMilliseconds(30);

    /// <summary>The bytes that end the handshake.</summary>
    public static ReadOnlyMemory<byte> WakeSequence { get; } = new byte[] { 0xA1, 0x80, 0x00 };

    /// <summary>Whether a received byte is the handshake reply (high bit set).</summary>
    /// <param name="value">A received byte.</param>
    /// <returns><see langword="true"/> when the recorder has woken.</returns>
    public static bool IsWakeByte(byte value) => (value & 0x80) != 0;

    internal static ReadOnlyMemory<byte> ReleaseCode { get; } = new byte[] { 0x80, 0x00 };

    internal static ReadOnlyMemory<byte> PressCode(ZoomH4nButton button) => button switch
    {
        ZoomH4nButton.Record => new byte[] { 0x81, 0x00 },
        ZoomH4nButton.Play => new byte[] { 0x82, 0x00 },
        ZoomH4nButton.Stop => new byte[] { 0x84, 0x00 },
        ZoomH4nButton.FastForward => new byte[] { 0x88, 0x00 },
        ZoomH4nButton.Rewind => new byte[] { 0x90, 0x00 },
        ZoomH4nButton.VolumeUp => new byte[] { 0x80, 0x08 },
        ZoomH4nButton.VolumeDown => new byte[] { 0x80, 0x10 },
        ZoomH4nButton.RecordLevelUp => new byte[] { 0x80, 0x20 },
        ZoomH4nButton.RecordLevelDown => new byte[] { 0x80, 0x40 },
        ZoomH4nButton.Mic => new byte[] { 0x80, 0x01 },
        ZoomH4nButton.Channel1 => new byte[] { 0x80, 0x02 },
        ZoomH4nButton.Channel2 => new byte[] { 0x80, 0x04 },
        _ => throw new ArgumentOutOfRangeException(nameof(button), button, "Unknown Zoom H4n button."),
    };
}
