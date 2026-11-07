using System;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.UiDefinitions;

namespace OoBDev.LanC.Panels;

/// <summary>Writes the two command bytes of a button to the LANC bridge.</summary>
public sealed class LanCControlSurface : IControlSurface, ICommandPreview
{
    private readonly DeviceSend _send;

    /// <summary>Creates a surface that writes through <paramref name="send"/>.</summary>
    /// <param name="send">The device write.</param>
    public LanCControlSurface(DeviceSend send)
    {
        ArgumentNullException.ThrowIfNull(send);
        _send = send;
    }

    /// <inheritdoc />
    public Task InvokeAsync(string commandId, string? value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(commandId);
        var bytes = Find(commandId) ?? throw new ArgumentException($"Unknown LANC command '{commandId}'.", nameof(commandId));
        return _send(bytes, cancellationToken);
    }

    /// <inheritdoc />
    public string? PreviewCommand(string commandId, string? value)
    {
        ArgumentNullException.ThrowIfNull(commandId);
        return Find(commandId) is { } bytes ? CommandPreviewFormat.ToHex(bytes) : null;
    }

    private static byte[]? Find(string commandId)
    {
        foreach (var (id, _, table, code) in LanCUiDefinition.Buttons)
        {
            if (string.Equals(id, commandId, StringComparison.Ordinal))
            {
                return [(byte)table, code];
            }
        }

        return null;
    }
}
