using OoBDev.System.IO.Messages;
using System;

namespace OoBDev.Zoom.H4n;

/// <summary>
/// Encodes a <see cref="ZoomH4nCommand"/> as its two-byte press or release code. Send the press, then the release, as two writes like
/// a physical remote does.
/// </summary>
public sealed class ZoomH4nMessageEncoder : IMessageEncoder<ZoomH4nCommand>
{
    /// <inheritdoc/>
    public ReadOnlyMemory<byte> Encode(ref ZoomH4nCommand request) => request.Action switch
    {
        ZoomH4nAction.Press => ZoomH4nProtocol.PressCode(request.Button),
        ZoomH4nAction.Release => ZoomH4nProtocol.ReleaseCode,
        _ => throw new ArgumentOutOfRangeException(nameof(request), request.Action, "Unknown Zoom H4n action."),
    };
}
