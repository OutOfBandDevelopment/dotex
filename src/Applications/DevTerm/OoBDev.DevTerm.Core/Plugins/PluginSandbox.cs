using System.Collections.Generic;
using OoBDev.System.Sandbox;

namespace OoBDev.DevTerm;

/// <summary>How to isolate one process plugin: the launcher, the folders it may reach and its limits.</summary>
/// <param name="Launcher">Starts the program in the platform isolation.</param>
/// <param name="ReadDirectories">Folders the plugin may read (its own folder is always included).</param>
/// <param name="WriteDirectories">Folders the plugin may read and write.</param>
/// <param name="Limits">Memory, process, CPU and wall time limits.</param>
public sealed record PluginSandbox(ISandboxCommandLauncher Launcher, IReadOnlyCollection<string> ReadDirectories, IReadOnlyCollection<string> WriteDirectories, JobLimits Limits);
