using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OoBDev.System.Sandbox;

namespace OoBDev.DevTerm;

/// <summary>
/// The optional <c>permissions</c> block of a process plugin's <c>plugin.json</c>:
/// <c>{ "read": ["{folder}/data"], "write": ["{folder}/out"], "memoryMb": 256, "cpuSeconds": 0, "wallSeconds": 0, "maxProcesses": 0 }</c>.
/// A plugin with the block runs in the platform isolation (no network, only these folders); where no isolation exists it is refused, never run plain.
/// </summary>
public sealed class PluginPermissions
{
    public IList<string> Read { get; set; } = [];

    public IList<string> Write { get; set; } = [];

    public int MemoryMb { get; set; } = 256;

    public int CpuSeconds { get; set; }

    public int WallSeconds { get; set; }

    public int MaxProcesses { get; set; }

    /// <summary>The isolation for the plugin in <paramref name="folder"/>; <c>{folder}</c> in a path becomes the folder, and the folder itself is always readable.</summary>
    public PluginSandbox ToSandbox(ISandboxCommandLauncher launcher, string folder)
    {
        ArgumentNullException.ThrowIfNull(launcher);
        var root = Path.GetFullPath(folder);
        string Expand(string path) => Path.GetFullPath(path.Replace("{folder}", root, StringComparison.Ordinal));
        var read = new[] { root }.Concat(Read.Select(Expand)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var write = Write.Select(Expand).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        return new PluginSandbox(launcher, read, write, new JobLimits(Math.Max(0, MemoryMb), Math.Max(0, MaxProcesses), Math.Max(0, CpuSeconds), Math.Max(0, WallSeconds)));
    }
}
