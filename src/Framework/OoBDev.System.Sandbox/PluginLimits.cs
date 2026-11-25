

namespace OoBDev.System.Sandbox;

/// <summary>The optional limits a manifest asks for; the host applies its own ceilings (protocol-and-manifest.md, manifest fields).</summary>
/// <param name="MsgPerSec">Frames per second the plugin may send; over it, frames are dropped.</param>
/// <param name="MemoryMb">Memory per process.</param>
/// <param name="MaxProcesses">Processes the plugin may run at once.</param>
/// <param name="CpuSeconds">CPU time before the plugin is stopped.</param>
/// <param name="WallSeconds">Wall clock time before the plugin is stopped.</param>
public sealed record PluginLimits(int? MsgPerSec = null, int? MemoryMb = null, int? MaxProcesses = null, int? CpuSeconds = null, int? WallSeconds = null)
{
    /// <summary>The most frames a second the host allows whatever the manifest asks for.</summary>
    public const int MaxMsgPerSecCeiling = 1000;

    /// <summary>The rate used when the manifest names none.</summary>
    public const int DefaultMsgPerSec = 200;

    /// <summary>The memory used when the manifest names none.</summary>
    public const int DefaultMemoryMb = 256;

    /// <summary>The most memory a plugin may ask for.</summary>
    public const int MemoryMbCeiling = 1024;

    /// <summary>The most processes a plugin may ask for.</summary>
    public const int MaxProcessesCeiling = 64;

    /// <summary>The longest CPU or wall time a plugin may ask for.</summary>
    public const int SecondsCeiling = 3600;

    /// <summary>The rate to enforce: the request clamped to the ceiling, or the default.</summary>
    public int EffectiveMsgPerSec => MsgPerSec is > 0 ? global::System.Math.Min(MsgPerSec.Value, MaxMsgPerSecCeiling) : DefaultMsgPerSec;

    /// <summary>The process limits to launch with: each request clamped to its ceiling, memory defaulted, the rest unlimited unless asked for.</summary>
    public JobLimits ToJobLimits() => new(
        MemoryMb is > 0 ? global::System.Math.Min(MemoryMb.Value, MemoryMbCeiling) : DefaultMemoryMb,
        MaxProcesses is > 0 ? global::System.Math.Min(MaxProcesses.Value, MaxProcessesCeiling) : 0,
        CpuSeconds is > 0 ? global::System.Math.Min(CpuSeconds.Value, SecondsCeiling) : 0,
        WallSeconds is > 0 ? global::System.Math.Min(WallSeconds.Value, SecondsCeiling) : 0);
}
