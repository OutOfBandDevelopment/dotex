using System;

namespace OoBDev.System.Sandbox;

/// <summary>A plugin process on any platform, speaking the framed protocol over stdio; the launcher behind it decides how it is isolated.</summary>
public interface ISandboxedPlugin : IDisposable
{
    /// <summary>Which limit ended the plugin, or none; reported where the platform can tell (Windows, Linux).</summary>
    LimitHit LimitHit { get; }

    /// <summary>The plugin's exit code once it has ended.</summary>
    int ExitCode { get; }

    /// <summary>Sends a frame with a JSON payload.</summary>
    ulong Send(FrameType type, string topic, string json);

    /// <summary>Answers <paramref name="request"/> with a JSON payload.</summary>
    ulong Reply(ReceivedFrame request, FrameType type, string json);

    /// <summary>Reads one frame.</summary>
    ReceivedFrame Receive();

    /// <summary>Waits up to <paramref name="milliseconds"/> for the plugin to end.</summary>
    bool WaitForExit(int milliseconds);

    /// <summary>Lets go of a plugin started with <c>surviveCaller</c>: the host stops watching it and closes its handles while the plugin keeps running for a later caller.</summary>
    void Detach() => throw new NotSupportedException("This launcher cannot detach a plugin.");
}
