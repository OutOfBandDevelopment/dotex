using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace OoBDev.System.Sandbox.Windows;

/// <summary>Starts plugins in an AppContainer inside a job object.</summary>
/// <param name="pluginAssembly">The plugin assembly the launcher runs with <c>dotnet</c>.</param>
[SupportedOSPlatform("windows")]
public sealed class WindowsSandboxLauncher(string pluginAssembly) : ISandboxLauncher
{
    /// <inheritdoc />
    public string Mechanism => "AppContainer + job object";

    /// <inheritdoc />
    public bool IsAvailable => OperatingSystem.IsWindows();

    /// <inheritdoc />
    public ISandboxedPlugin Start(string commandArguments, IReadOnlyCollection<string>? readDirectories = null, IReadOnlyCollection<string>? writeDirectories = null) =>
        new SandboxedPlugin(pluginAssembly, commandArguments, new JobLimits(MemoryMb: 256), readDirectories, writeDirectories);
}
