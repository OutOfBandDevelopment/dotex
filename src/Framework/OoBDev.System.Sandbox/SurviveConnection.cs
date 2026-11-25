using System;
using System.IO;
using System.IO.Pipes;
using OoBDev.System.Sandbox;

namespace OoBDev.System.Sandbox;

/// <summary>
/// The caller's side of a plugin that outlives its caller (<c>onCallerExit: Survive</c>). The plugin listens on a named pipe that only the same user can open;
/// every connection starts with the new 32-byte session key (from <see cref="ReconnectRegistry"/>), after which frames flow as on stdio with sequences restarting at 1.
/// The plugin answers the first frame with <c>lifecycle.reconnected</c>.
/// </summary>
public sealed class SurviveConnection : IDisposable
{
    private readonly NamedPipeClientStream _pipe;
    private readonly FrameChannel _channel;

    /// <summary>Connects to <paramref name="pipeName"/> within <paramref name="timeout"/> and sends <paramref name="sessionKey"/>.</summary>
    public SurviveConnection(string pipeName, byte[] sessionKey, FrameFlags flags, TimeSpan timeout)
    {
        ArgumentException.ThrowIfNullOrEmpty(pipeName);
        ArgumentNullException.ThrowIfNull(sessionKey);
        _pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.CurrentUserOnly);
        _pipe.Connect((int)timeout.TotalMilliseconds);
        _pipe.Write(sessionKey);
        _pipe.Flush();
        _channel = new FrameChannel(_pipe, _pipe, flags, sessionKey);
    }

    /// <summary>Sends a request frame and returns its sequence.</summary>
    public ulong Request(string topic, string json) => _channel.Send(FrameType.Request, topic, json);

    /// <summary>Reads the next frame the plugin sent.</summary>
    public ReceivedFrame Receive() => _channel.Read();

    /// <inheritdoc />
    public void Dispose() => _pipe.Dispose();
}
