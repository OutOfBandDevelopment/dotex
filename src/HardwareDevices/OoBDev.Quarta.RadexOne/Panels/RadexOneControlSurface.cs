using System;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.UiDefinitions;

namespace OoBDev.Quarta.RadexOne.Panels;

/// <summary>Sends one <see cref="RadexOneRequest"/> per button. Write Settings is not offered: the device takes it only when sent three times in a row.</summary>
public sealed class RadexOneControlSurface : IControlSurface, ICommandPreview
{
    private readonly DeviceSend _send;
    private readonly RadexOneMessageEncoder _encoder = new();
    private ushort _packetNumber;

    /// <summary>Creates a surface that writes through <paramref name="send"/>.</summary>
    /// <param name="send">The device write.</param>
    public RadexOneControlSurface(DeviceSend send)
    {
        ArgumentNullException.ThrowIfNull(send);
        _send = send;
    }

    /// <inheritdoc />
    public Task InvokeAsync(string commandId, string? value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(commandId);
        var kind = Find(commandId) ?? throw new ArgumentException($"Unknown Radex One command '{commandId}'.", nameof(commandId));
        var request = new RadexOneRequest(kind, unchecked(++_packetNumber));
        return _send(_encoder.Encode(ref request), cancellationToken);
    }

    /// <inheritdoc />
    public string? PreviewCommand(string commandId, string? value)
    {
        ArgumentNullException.ThrowIfNull(commandId);
        if (Find(commandId) is not { } kind)
        {
            return null;
        }

        var request = new RadexOneRequest(kind, unchecked((ushort)(_packetNumber + 1)));
        return CommandPreviewFormat.ToHex(_encoder.Encode(ref request).ToArray());
    }

    private static RadexOneRequestKind? Find(string commandId) => commandId switch
    {
        RadexOneUiDefinition.ReadData => RadexOneRequestKind.ReadData,
        RadexOneUiDefinition.ReadSettings => RadexOneRequestKind.ReadSettings,
        RadexOneUiDefinition.ReadSerialVersion => RadexOneRequestKind.ReadSerialVersion,
        RadexOneUiDefinition.ResetAccumulated => RadexOneRequestKind.ResetAccumulated,
        _ => null,
    };
}
