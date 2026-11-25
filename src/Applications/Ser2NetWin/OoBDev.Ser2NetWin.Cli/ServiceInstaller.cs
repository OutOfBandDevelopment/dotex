using OoBDev.System.IO.Ports.Bridge;
using System.Diagnostics;
using System.Security.Principal;

namespace OoBDev.Ser2NetWin.Cli;

/// <summary>Registers and controls the Windows Service through sc.exe.</summary>
public static class ServiceInstaller
{
    public const string ServiceName = "ser2net-win";

    private const string DisplayName = "ser2net-win (serial to TCP bridge)";
    private const string Description = "Bridges Windows COM ports to raw TCP sockets.";

    /// <summary>Builds the sc.exe argument string for "create". The binPath value is quoted the way sc.exe expects.</summary>
    public static string BuildCreateArguments(string exePath, string? configPath)
    {
        var binPath = $"\\\"{exePath}\\\" run";
        if (configPath is not null)
        {
            binPath += $" --config \\\"{configPath}\\\"";
        }

        return $"create {ServiceName} binPath= \"{binPath}\" start= auto DisplayName= \"{DisplayName}\"";
    }

    public static bool IsAdministrator()
    {
        if (!OperatingSystem.IsWindows())
        {
            return false;
        }

        using var identity = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static bool Exists() => RunSc($"query {ServiceName}").Succeeded;

    public static bool IsRunning() => RunSc($"query {ServiceName}").Output.Contains("RUNNING", StringComparison.Ordinal);

    public static string? Install(string configPath, bool start)
    {
        var exe = Environment.ProcessPath;
        if (exe is null || Path.GetFileNameWithoutExtension(exe).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
        {
            return "Install needs the published ser2net-win.exe, not 'dotnet run'. Publish first (see README).";
        }

        var explicitConfig = string.Equals(Path.GetFullPath(configPath), ConfigStore.DefaultPath, StringComparison.OrdinalIgnoreCase)
            ? null
            : Path.GetFullPath(configPath);

        var create = RunSc(BuildCreateArguments(exe, explicitConfig));
        if (!create.Succeeded)
        {
            return create.Output.Trim();
        }

        RunSc($"description {ServiceName} \"{Description}\"");
        RunSc($"failure {ServiceName} reset= 86400 actions= restart/5000/restart/5000/restart/30000");

        return start ? Start() : null;
    }

    public static string? Uninstall()
    {
        Stop();
        var result = RunSc($"delete {ServiceName}");
        return result.Succeeded ? null : result.Output.Trim();
    }

    public static string? Start()
    {
        var result = RunSc($"start {ServiceName}");
        return result.Succeeded ? null : result.Output.Trim();
    }

    public static string? Stop()
    {
        var result = RunSc($"stop {ServiceName}");
        return result.Succeeded ? null : result.Output.Trim();
    }

    public static string? Restart()
    {
        Stop();
        for (var i = 0; i < 20 && IsRunning(); i++)
        {
            Thread.Sleep(500);
        }

        return Start();
    }

    public static string Status()
    {
        var result = RunSc($"query {ServiceName}");
        if (!result.Succeeded)
        {
            return "not installed";
        }

        foreach (var state in new[] { "RUNNING", "STOPPED", "START_PENDING", "STOP_PENDING", "PAUSED" })
        {
            if (result.Output.Contains(state, StringComparison.Ordinal))
            {
                return state.ToLowerInvariant().Replace('_', ' ');
            }
        }

        return "unknown";
    }

    private static ScResult RunSc(string arguments)
    {
        var startInfo = new ProcessStartInfo("sc.exe", arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(startInfo)!;
        var output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
        process.WaitForExit();
        return new ScResult(process.ExitCode, output);
    }
}
