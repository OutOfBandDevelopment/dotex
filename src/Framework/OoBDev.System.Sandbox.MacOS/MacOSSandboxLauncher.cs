using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;

namespace OoBDev.System.Sandbox.MacOS;

/// <summary>Starts plugins under Seatbelt (<c>sandbox-exec</c>); unverified on a Mac.</summary>
/// <param name="pluginAssembly">The plugin assembly the launcher runs with <c>dotnet</c>.</param>
[SupportedOSPlatform("macos")]
public sealed class MacOSSandboxLauncher(string pluginAssembly) : ISandboxLauncher
{
    /// <inheritdoc />
    public string Mechanism => "Seatbelt";

    /// <inheritdoc />
    public bool IsAvailable => OperatingSystem.IsMacOS() && File.Exists("/usr/bin/sandbox-exec");

    /// <inheritdoc />
    public ISandboxedPlugin Start(string commandArguments, IReadOnlyCollection<string>? readDirectories = null, IReadOnlyCollection<string>? writeDirectories = null, JobLimits? limits = null, bool surviveCaller = false) =>
        surviveCaller ? throw new NotSupportedException("Survive is not built on macOS.") : new SeatbeltPlugin(pluginAssembly, commandArguments, readDirectories, writeDirectories);
}
