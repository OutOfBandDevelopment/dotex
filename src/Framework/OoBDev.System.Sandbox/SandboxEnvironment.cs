namespace OoBDev.System.Sandbox;

/// <summary>The environment variables that carry the frame key and flags from the host to the plugin.</summary>
public static class SandboxEnvironment
{
    /// <summary>The per-plugin HMAC key, hex.</summary>
    public const string KeyVariable = "PLUGIN_FRAME_KEY";

    /// <summary>The negotiated <see cref="FrameFlags"/>, as a number.</summary>
    public const string FlagsVariable = "PLUGIN_FRAME_FLAGS";
}
