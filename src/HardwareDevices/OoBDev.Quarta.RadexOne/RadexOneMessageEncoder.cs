using OoBDev.System.IO.Messages;
using System;

namespace OoBDev.Quarta.RadexOne;

/// <summary>
/// Encodes a <see cref="RadexOneRequest"/> as an outbound packet. Queries carry the word <c>0x000C</c>, except Reset Accumulated, which
/// carries <c>0x0001</c>.
/// </summary>
public sealed class RadexOneMessageEncoder : IMessageEncoder<RadexOneRequest>
{
    /// <inheritdoc/>
    public ReadOnlyMemory<byte> Encode(ref RadexOneRequest request) => RadexOneFramer.BuildRequest(request.PacketNumber, request.Kind switch
    {
        RadexOneRequestKind.ReadData => RadexOneExtensionCodec.BuildQuery(RadexOneCommandCode.ReadData, 0x000C),
        RadexOneRequestKind.ReadSerialVersion => RadexOneExtensionCodec.BuildQuery(RadexOneCommandCode.ReadSerialVersion, 0x000C),
        RadexOneRequestKind.ReadSettings => RadexOneExtensionCodec.BuildQuery(RadexOneCommandCode.ReadSettings, 0x000C),
        RadexOneRequestKind.ResetAccumulated => RadexOneExtensionCodec.BuildQuery(RadexOneCommandCode.ResetAccumulated, 0x0001),
        RadexOneRequestKind.WriteSettings => RadexOneExtensionCodec.BuildWriteSettings((byte)request.Settings.AlarmMode, request.Settings.Threshold),
        _ => throw new ArgumentOutOfRangeException(nameof(request), request.Kind, "Unknown Radex One request kind."),
    });
}
