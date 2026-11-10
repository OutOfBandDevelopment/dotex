using System;

namespace OoBDev.Spike.PluginSandbox;

/// <summary>A plugin process on any platform, speaking the framed protocol over stdio; the launcher behind it decides how it is isolated.</summary>
public interface ISandboxedPlugin : IDisposable
{
    /// <summary>The plugin's exit code once it has ended.</summary>
    int ExitCode { get; }

    /// <summary>Sends a frame with a JSON payload.</summary>
    ulong Send(FrameType type, string topic, string json);

    /// <summary>Reads one frame.</summary>
    ReceivedFrame Receive();

    /// <summary>Waits up to <paramref name="milliseconds"/> for the plugin to end.</summary>
    bool WaitForExit(int milliseconds);
}
