using System;
using System.Collections.Generic;

namespace OoBDev.Spike.PluginSandbox;

/// <summary>Starts a plugin in the sandbox of the current operating system: AppContainer and job object on Windows, bubblewrap on Linux, Seatbelt on macOS.</summary>
public static class SandboxLauncher
{
    /// <summary>The name of the isolation in use, for logs and test output.</summary>
    public static string Mechanism => OperatingSystem.IsWindows() ? "AppContainer + job object" : OperatingSystem.IsLinux() ? "bubblewrap" : OperatingSystem.IsMacOS() ? "Seatbelt" : "none";

    /// <summary>True when the sandbox for this platform can run here (on Linux it needs bubblewrap installed).</summary>
    public static bool IsAvailable => OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() && System.IO.File.Exists("/usr/bin/sandbox-exec") || OperatingSystem.IsLinux() && System.IO.File.Exists("/usr/bin/bwrap");

    /// <summary>Starts the plugin with only the given folders reachable (read-only and read-write).</summary>
    public static ISandboxedPlugin Start(string commandArguments, IReadOnlyCollection<string>? readDirectories = null, IReadOnlyCollection<string>? writeDirectories = null)
    {
        if (OperatingSystem.IsWindows())
        {
            return new SandboxedPlugin(commandArguments, new JobLimits(MemoryMb: 256), readDirectories, writeDirectories);
        }

        if (OperatingSystem.IsLinux())
        {
            return new BubblewrapPlugin(commandArguments, readDirectories, writeDirectories, limits: new JobLimits(MemoryMb: 256));
        }

        if (OperatingSystem.IsMacOS())
        {
            return new SeatbeltPlugin(commandArguments, readDirectories, writeDirectories);
        }

        throw new PlatformNotSupportedException("No sandbox is built for this platform.");
    }
}
