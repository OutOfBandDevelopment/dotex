using System;
using OoBDev.System.Sandbox;
using OoBDev.System.Sandbox.Linux;
using OoBDev.System.Sandbox.Windows;

namespace OoBDev.DevTerm;

/// <summary>Finds the sandbox launcher for this platform; null when none exists (macOS, or Linux without bubblewrap).</summary>
public static class PluginSandboxes
{
    private static ISandboxCommandLauncher? _override;
    private static bool _overridden;

    /// <summary>The launcher process plugins with <c>permissions</c> run in; tests may replace it with <see cref="Use"/>.</summary>
    public static ISandboxCommandLauncher? Launcher => _overridden ? _override : Detect();

    /// <summary>Replaces the detected launcher; null means no launcher at all (the refusal path). <see cref="Reset"/> restores detection.</summary>
    public static void Use(ISandboxCommandLauncher? launcher)
    {
        _override = launcher;
        _overridden = true;
    }

    /// <summary>Goes back to detecting the launcher from the operating system.</summary>
    public static void Reset()
    {
        _override = null;
        _overridden = false;
    }

    private static ISandboxCommandLauncher? Detect()
    {
        if (OperatingSystem.IsWindows())
        {
            return new WindowsSandboxLauncher(string.Empty);
        }

        if (OperatingSystem.IsLinux())
        {
            var linux = new LinuxSandboxLauncher(string.Empty);
            return linux.IsAvailable ? linux : null;
        }

        return null;
    }
}
