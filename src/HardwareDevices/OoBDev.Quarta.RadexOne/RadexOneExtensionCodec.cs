using System;
using System.Buffers.Binary;

namespace OoBDev.Quarta.RadexOne;

internal static class RadexOneExtensionCodec
{
    public const int ReadDataLength = 22;
    public const int SettingsLength = 16;

    public static byte[] BuildQuery(ushort commandCode, ushort word)
    {
        var extension = new byte[6];
        BinaryPrimitives.WriteUInt16LittleEndian(extension, commandCode);
        BinaryPrimitives.WriteUInt16LittleEndian(extension.AsSpan(2), word);
        BinaryPrimitives.WriteUInt16LittleEndian(extension.AsSpan(4), RadexOneFramer.ComputeChecksum(extension.AsSpan(0, 4)));
        return extension;
    }

    public static byte[] BuildWriteSettings(byte alarmMode, ushort threshold)
    {
        var extension = new byte[SettingsLength];
        BinaryPrimitives.WriteUInt16LittleEndian(extension, RadexOneCommandCode.WriteSettings);
        BinaryPrimitives.WriteUInt16LittleEndian(extension.AsSpan(2), 0x000E);
        BinaryPrimitives.WriteUInt16LittleEndian(extension.AsSpan(4), 0x0005);
        extension[8] = alarmMode;
        BinaryPrimitives.WriteUInt16LittleEndian(extension.AsSpan(9), threshold);
        BinaryPrimitives.WriteUInt16LittleEndian(extension.AsSpan(14), RadexOneFramer.ComputeChecksum(extension.AsSpan(0, 14)));
        return extension;
    }

    public static bool TryReadCommandCode(ReadOnlySpan<byte> extension, out ushort commandCode)
    {
        commandCode = extension.Length < 2 ? (ushort)0 : BinaryPrimitives.ReadUInt16LittleEndian(extension);
        return extension.Length >= 2;
    }

    public static bool TryParseReadData(ReadOnlySpan<byte> extension, out RadexOneReadData data)
    {
        data = default;
        if (extension.Length < ReadDataLength || !HasValidChecksum(extension, 20)) return false;

        data = new RadexOneReadData(
            BinaryPrimitives.ReadUInt16LittleEndian(extension[8..]),
            BinaryPrimitives.ReadUInt16LittleEndian(extension[12..]),
            BinaryPrimitives.ReadUInt16LittleEndian(extension[16..]));
        return true;
    }

    public static bool TryParseSettings(ReadOnlySpan<byte> extension, out RadexOneSettings settings)
    {
        settings = default;
        if (extension.Length < SettingsLength || !HasValidChecksum(extension, 14)) return false;

        settings = new RadexOneSettings((RadexOneAlarmMode)extension[8], BinaryPrimitives.ReadUInt16LittleEndian(extension[9..]));
        return true;
    }

    public static bool HasValidAck(ReadOnlySpan<byte> extension) => extension.Length >= 6 && HasValidChecksum(extension, 4);

    public static ReadOnlySpan<byte> SerialVersionPayload(ReadOnlySpan<byte> extension) => extension.Length < 6 ? [] : extension[4..^2];

    private static bool HasValidChecksum(ReadOnlySpan<byte> extension, int coveredLength) =>
        BinaryPrimitives.ReadUInt16LittleEndian(extension.Slice(coveredLength, 2)) == RadexOneFramer.ComputeChecksum(extension[..coveredLength]);
}
