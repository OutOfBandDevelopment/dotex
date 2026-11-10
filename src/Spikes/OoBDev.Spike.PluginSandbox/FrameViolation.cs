using System;

namespace OoBDev.Spike.PluginSandbox;

/// <summary>A frame broke the wire rules; the host's reaction is to disconnect and kill the plugin.</summary>
public sealed class FrameViolation : Exception
{
    /// <summary>Creates the violation.</summary>
    public FrameViolation(string message) : base(message)
    {
    }

    /// <summary>Creates the violation with its cause.</summary>
    public FrameViolation(string message, Exception inner) : base(message, inner)
    {
    }
}
