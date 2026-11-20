using System;
using System.Buffers.Binary;
using System.Text;

namespace OoBDev.Digi.XBee;

/// <summary>Builds the outgoing frames, enveloped and ready to write to the radio.</summary>
public static class XBeeFrames
{
    /// <summary>The address that reaches every radio on the network (<c>000000000000FFFF</c>).</summary>
    public const ulong BroadcastAddress = 0xFFFFUL;

    /// <summary>The 16-bit address to send when it is not known.</summary>
    public const ushort UnknownAddress16 = 0xFFFE;

    /// <summary>An AT command for the local radio.</summary>
    /// <param name="frameId">The frame id (1 to 255); 0 asks for no response.</param>
    /// <param name="command">The two-letter command, for example <c>NJ</c>.</param>
    /// <param name="parameter">The parameter bytes; empty to read the value.</param>
    /// <param name="escaped">True for API mode 2.</param>
    /// <returns>The encoded frame.</returns>
    /// <exception cref="ArgumentException">The command is not two ASCII characters.</exception>
    public static byte[] AtCommand(byte frameId, string command, ReadOnlySpan<byte> parameter = default, bool escaped = false)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.Length != 2 || !char.IsAscii(command[0]) || !char.IsAscii(command[1]))
            throw new ArgumentException("An AT command is two ASCII characters.", nameof(command));

        var data = new byte[4 + parameter.Length];
        data[0] = XBeeFrameTypes.AtCommand;
        data[1] = frameId;
        Encoding.ASCII.GetBytes(command, data.AsSpan(2));
        parameter.CopyTo(data.AsSpan(4));
        return XBeeFraming.Encode(data, escaped);
    }

    /// <summary>A transmit request.</summary>
    /// <param name="frameId">The frame id (1 to 255); 0 asks for no status reply.</param>
    /// <param name="address64">The destination, or <see cref="BroadcastAddress"/>.</param>
    /// <param name="payload">The data to send.</param>
    /// <param name="address16">The 16-bit address when known.</param>
    /// <param name="escaped">True for API mode 2.</param>
    /// <returns>The encoded frame.</returns>
    public static byte[] TransmitRequest(byte frameId, ulong address64, ReadOnlySpan<byte> payload, ushort address16 = UnknownAddress16, bool escaped = false)
    {
        var data = new byte[14 + payload.Length];
        data[0] = XBeeFrameTypes.TransmitRequest;
        data[1] = frameId;
        BinaryPrimitives.WriteUInt64BigEndian(data.AsSpan(2), address64);
        BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(10), address16);
        payload.CopyTo(data.AsSpan(14));
        return XBeeFraming.Encode(data, escaped);
    }
}
