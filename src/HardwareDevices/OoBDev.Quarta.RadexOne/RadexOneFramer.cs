using System;
using System.Buffers.Binary;

namespace OoBDev.Quarta.RadexOne;

/// <summary>
/// Builds and parses the outer packet: a 10-byte header (prefix, type marker, extension length, packet number, two zero bytes), a
/// 16-bit checksum of the header, then the extension.
/// </summary>
public static class RadexOneFramer
{
    /// <summary>First byte of a reply.</summary>
    public const byte InboundPrefix0 = 0x7A;

    /// <summary>Second byte of a reply.</summary>
    public const byte InboundPrefix1 = 0xFF;

    /// <summary>The header including its checksum; the extension follows.</summary>
    public const int HeaderLength = 12;

    private const byte OutboundPrefix0 = 0x7B;
    private const byte OutboundPrefix1 = 0xFF;
    private const ushort OutboundTypeMarker = 0x0020;
    private const ushort InboundTypeMarker = 0x8020;
    private const int ChecksumOffset = 10;

    /// <summary>Builds a request packet.</summary>
    /// <param name="packetNumber">The packet number the device echoes in its reply.</param>
    /// <param name="extension">The extension bytes.</param>
    /// <returns>The packet.</returns>
    public static byte[] BuildRequest(ushort packetNumber, ReadOnlySpan<byte> extension) =>
        Build(OutboundPrefix0, OutboundPrefix1, OutboundTypeMarker, packetNumber, extension);

    /// <summary>Builds a reply packet, as the device would send it.</summary>
    /// <param name="packetNumber">The packet number.</param>
    /// <param name="extension">The extension bytes.</param>
    /// <returns>The packet.</returns>
    public static byte[] BuildReply(ushort packetNumber, ReadOnlySpan<byte> extension) =>
        Build(InboundPrefix0, InboundPrefix1, InboundTypeMarker, packetNumber, extension);

    /// <summary>Checks a reply header (prefix, type marker and checksum) and reads the extension length.</summary>
    /// <param name="header">At least <see cref="HeaderLength"/> bytes.</param>
    /// <param name="extensionLength">The declared extension length.</param>
    /// <returns><see langword="true"/> when the header is valid.</returns>
    public static bool TryValidateHeader(ReadOnlySpan<byte> header, out ushort extensionLength)
    {
        extensionLength = 0;
        if (header.Length < HeaderLength || header[0] != InboundPrefix0 || header[1] != InboundPrefix1) return false;
        if (BinaryPrimitives.ReadUInt16LittleEndian(header.Slice(2, 2)) != InboundTypeMarker) return false;
        if (BinaryPrimitives.ReadUInt16LittleEndian(header.Slice(ChecksumOffset, 2)) != ComputeChecksum(header[..ChecksumOffset])) return false;

        extensionLength = BinaryPrimitives.ReadUInt16LittleEndian(header.Slice(4, 2));
        return true;
    }

    /// <summary>Parses a whole reply packet. Bytes after the declared extension are ignored.</summary>
    /// <param name="buffer">The packet.</param>
    /// <param name="packetNumber">The packet number.</param>
    /// <param name="extension">The extension bytes.</param>
    /// <returns><see langword="true"/> when the packet is valid and complete.</returns>
    public static bool TryParseReply(ReadOnlySpan<byte> buffer, out ushort packetNumber, out byte[] extension)
    {
        packetNumber = 0;
        extension = [];
        if (!TryValidateHeader(buffer, out var length) || buffer.Length < HeaderLength + length) return false;

        packetNumber = BinaryPrimitives.ReadUInt16LittleEndian(buffer.Slice(6, 2));
        extension = buffer.Slice(HeaderLength, length).ToArray();
        return true;
    }

    /// <summary>Computes the protocol checksum: <c>0xFFFF - (sum of little-endian words % 0xFFFF)</c>.</summary>
    /// <param name="coveredBytes">The bytes covered, in whole words.</param>
    /// <returns>The checksum.</returns>
    public static ushort ComputeChecksum(ReadOnlySpan<byte> coveredBytes)
    {
        var sum = 0;
        for (var i = 0; i + 1 < coveredBytes.Length; i += 2)
            sum += BinaryPrimitives.ReadUInt16LittleEndian(coveredBytes.Slice(i, 2));
        return (ushort)(0xFFFF - (sum % 0xFFFF));
    }

    private static byte[] Build(byte prefix0, byte prefix1, ushort typeMarker, ushort packetNumber, ReadOnlySpan<byte> extension)
    {
        var packet = new byte[HeaderLength + extension.Length];
        packet[0] = prefix0;
        packet[1] = prefix1;
        BinaryPrimitives.WriteUInt16LittleEndian(packet.AsSpan(2), typeMarker);
        BinaryPrimitives.WriteUInt16LittleEndian(packet.AsSpan(4), (ushort)extension.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(packet.AsSpan(6), packetNumber);
        BinaryPrimitives.WriteUInt16LittleEndian(packet.AsSpan(ChecksumOffset), ComputeChecksum(packet.AsSpan(0, ChecksumOffset)));
        extension.CopyTo(packet.AsSpan(HeaderLength));
        return packet;
    }
}
