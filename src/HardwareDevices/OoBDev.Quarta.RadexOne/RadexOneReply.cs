using System;

namespace OoBDev.Quarta.RadexOne;

/// <summary>
/// A validated reply. The extension is kept whole and read through the typed accessors, because the shape depends on the command.
/// </summary>
/// <param name="PacketNumber">The packet number echoed from the request.</param>
/// <param name="Extension">The extension bytes, command code first.</param>
public sealed record RadexOneReply(ushort PacketNumber, byte[] Extension)
{
    /// <summary>The command code, or <see langword="null"/> when the extension is too short to hold one.</summary>
    public ushort? CommandCode => RadexOneExtensionCodec.TryReadCommandCode(Extension, out var code) ? code : null;

    /// <summary>Reads the values of a Read Data reply.</summary>
    /// <param name="data">The values.</param>
    /// <returns><see langword="true"/> when this is a Read Data reply with a valid inner checksum.</returns>
    public bool TryGetReadData(out RadexOneReadData data)
    {
        data = default;
        return CommandCode == RadexOneCommandCode.ReadData && RadexOneExtensionCodec.TryParseReadData(Extension, out data);
    }

    /// <summary>Reads the settings of a Read Settings reply.</summary>
    /// <param name="settings">The settings.</param>
    /// <returns><see langword="true"/> when this is a Read Settings reply with a valid inner checksum.</returns>
    public bool TryGetSettings(out RadexOneSettings settings)
    {
        settings = default;
        return CommandCode == RadexOneCommandCode.ReadSettings && RadexOneExtensionCodec.TryParseSettings(Extension, out settings);
    }

    /// <summary>Whether this is a valid acknowledgement of a Write Settings or Reset Accumulated request.</summary>
    public bool IsAcknowledgement =>
        CommandCode is RadexOneCommandCode.WriteSettings or RadexOneCommandCode.ResetAccumulated && RadexOneExtensionCodec.HasValidAck(Extension);

    /// <summary>The serial and version payload of a Read Serial Version reply as text, with non-printable bytes shown as <c>.</c>. The layout is unconfirmed.</summary>
    public string? SerialVersionText
    {
        get
        {
            if (CommandCode != RadexOneCommandCode.ReadSerialVersion) return null;
            var payload = RadexOneExtensionCodec.SerialVersionPayload(Extension);
            var chars = new char[payload.Length];
            for (var i = 0; i < payload.Length; i++)
                chars[i] = payload[i] is >= 0x20 and < 0x7F ? (char)payload[i] : '.';
            return new string(chars);
        }
    }
}
