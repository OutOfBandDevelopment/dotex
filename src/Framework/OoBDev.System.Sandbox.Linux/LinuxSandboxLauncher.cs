using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;

namespace OoBDev.System.Sandbox.Linux;

/// <summary>Starts plugins under bubblewrap (needs <c>/usr/bin/bwrap</c>).</summary>
/// <param name="pluginAssembly">The plugin assembly the launcher runs with <c>dotnet</c>.</param>
[SupportedOSPlatform("linux")]
public sealed class LinuxSandboxLauncher(string pluginAssembly) : ISandboxLauncher
{
    /// <inheritdoc />
    public string Mechanism => "bubblewrap";

    /// <inheritdoc />
    public bool IsAvailable => OperatingSystem.IsLinux() && File.Exists("/usr/bin/bwrap");

    /// <inheritdoc />
    public ISandboxedPlugin Start(string commandArguments, IReadOnlyCollection<string>? readDirectories = null, IReadOnlyCollection<string>? writeDirectories = null, JobLimits? limits = null) =>
        new BubblewrapPlugin(pluginAssembly, commandArguments, readDirectories, writeDirectories, limits: limits ?? new JobLimits(MemoryMb: 256));
}
