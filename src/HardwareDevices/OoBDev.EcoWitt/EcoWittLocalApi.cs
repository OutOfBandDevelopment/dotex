using System;
using System.IO;

namespace OoBDev.EcoWitt;

/// <summary>
/// Framing of the gateway's local binary API (TCP port 45000): <c>FF FF</c>, command, size, data, checksum. The size counts from
/// the command byte to the checksum; the checksum is the sum of every byte after the header, modulo 256. The live data command
/// answers with a two-byte size, every other command with one byte.
/// </summary>
public static class EcoWittLocalApi
{
    /// <summary>The TCP port the gateway listens on.</summary>
    public const int DefaultPort = 45000;

    /// <summary>The command that reads all current sensor values.</summary>
    public const byte LiveDataCommand = 0x27;

    private const int _headerLength = 2;

    /// <summary>
    /// Builds the request for a command that takes no data, for example <see cref="LiveDataCommand"/>.
    /// </summary>
    /// <param name="command">The command byte.</param>
    /// <returns>The five request bytes.</returns>
    public static byte[] BuildRequest(byte command)
    {
        byte[] packet = [0xFF, 0xFF, command, 0x03, 0];
        packet[4] = Checksum(packet.AsSpan(_headerLength, 2));
        return packet;
    }

    /// <summary>
    /// Checks the framing of a response and returns its data.
    /// </summary>
    /// <param name="packet">One complete response.</param>
    /// <param name="command">The command byte of the response.</param>
    /// <returns>The data between the size field and the checksum.</returns>
    /// <exception cref="InvalidDataException">The header, size or checksum is wrong, or the packet is truncated.</exception>
    public static ReadOnlySpan<byte> ReadPayload(ReadOnlySpan<byte> packet, out byte command)
    {
        if (packet.Length < 5 || packet[0] != 0xFF || packet[1] != 0xFF)
        {
            throw new InvalidDataException("The response does not start with the FF FF header.");
        }

        command = packet[2];
        var sizeBytes = command == LiveDataCommand ? 2 : 1;
        var size = sizeBytes == 2 ? (packet[3] << 8) | packet[4] : packet[3];
        var total = _headerLength + size;
        if (packet.Length < _headerLength + 1 + sizeBytes + 1 || size < 1 + sizeBytes + 1 || packet.Length != total)
        {
            throw new InvalidDataException($"The size field says {size} bytes after the header, the response holds {packet.Length - _headerLength}.");
        }

        if (Checksum(packet[_headerLength..(total - 1)]) != packet[total - 1])
        {
            throw new InvalidDataException("The checksum does not match.");
        }

        return packet[(_headerLength + 1 + sizeBytes)..(total - 1)];
    }

    /// <summary>Computes the checksum of the bytes after the header: their sum modulo 256.</summary>
    /// <param name="bytes">The command, size and data bytes.</param>
    /// <returns>The checksum byte.</returns>
    public static byte Checksum(ReadOnlySpan<byte> bytes)
    {
        byte sum = 0;
        foreach (var b in bytes)
        {
            sum += b;
        }

        return sum;
    }
}
