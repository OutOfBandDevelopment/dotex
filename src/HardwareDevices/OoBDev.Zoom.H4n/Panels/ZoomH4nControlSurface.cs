using System;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.UiDefinitions;

namespace OoBDev.Zoom.H4n.Panels;

/// <summary>Runs the panel's buttons: each is sent as its press code and then the shared release code, as two writes like a physical remote. The caller owns the wake-up handshake (<see cref="ZoomH4nProtocol"/>).</summary>
public sealed class ZoomH4nControlSurface : IControlSurface, ICommandPreview
{
    private readonly DeviceSend _send;
    private readonly ZoomH4nMessageEncoder _encoder = new();

    /// <summary>Creates a surface that writes through <paramref name="send"/>.</summary>
    /// <param name="send">The device write.</param>
    public ZoomH4nControlSurface(DeviceSend send)
    {
        ArgumentNullException.ThrowIfNull(send);
        _send = send;
    }

    /// <inheritdoc />
    public async Task InvokeAsync(string commandId, string? value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(commandId);
        var button = Find(commandId) ?? throw new ArgumentException($"Unknown Zoom H4n command '{commandId}'.", nameof(commandId));
        var press = new ZoomH4nCommand(button);
        var release = ZoomH4nCommand.Release;
        await _send(_encoder.Encode(ref press), cancellationToken).ConfigureAwait(false);
        await _send(_encoder.Encode(ref release), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public string? PreviewCommand(string commandId, string? value)
    {
        ArgumentNullException.ThrowIfNull(commandId);
        if (Find(commandId) is not { } button)
        {
            return null;
        }

        var press = new ZoomH4nCommand(button);
        var release = ZoomH4nCommand.Release;
        return $"{CommandPreviewFormat.ToHex(_encoder.Encode(ref press).ToArray())} {CommandPreviewFormat.ToHex(_encoder.Encode(ref release).ToArray())}";
    }

    private static ZoomH4nButton? Find(string commandId)
    {
        foreach (var (id, _, button) in ZoomH4nUiDefinition.Buttons)
        {
            if (string.Equals(id, commandId, StringComparison.Ordinal))
            {
                return button;
            }
        }

        return null;
    }
}
