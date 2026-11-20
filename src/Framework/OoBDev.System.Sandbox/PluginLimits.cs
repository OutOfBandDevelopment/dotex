namespace OoBDev.System.Sandbox;

/// <summary>The optional limits a manifest asks for; the host applies its own ceilings (protocol-and-manifest.md, manifest fields).</summary>
/// <param name="MsgPerSec">Frames per second the plugin may send; over it, frames are dropped.</param>
public sealed record PluginLimits(int? MsgPerSec = null)
{
    /// <summary>The most frames a second the host allows whatever the manifest asks for.</summary>
    public const int MaxMsgPerSecCeiling = 1000;

    /// <summary>The rate used when the manifest names none.</summary>
    public const int DefaultMsgPerSec = 200;

    /// <summary>The rate to enforce: the request clamped to the ceiling, or the default.</summary>
    public int EffectiveMsgPerSec => MsgPerSec is > 0 ? global::System.Math.Min(MsgPerSec.Value, MaxMsgPerSecCeiling) : DefaultMsgPerSec;
}
