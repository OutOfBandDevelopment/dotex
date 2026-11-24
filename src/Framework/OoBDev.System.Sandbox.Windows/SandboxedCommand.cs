using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;

namespace OoBDev.System.Sandbox.Windows;

/// <summary>
/// Runs any program in an AppContainer (no network, no files beyond its grants and the program's own folder, explicit environment) inside a job object
/// with the limits of <see cref="JobLimits"/>, and hands back its plain standard streams.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class SandboxedCommand : ISandboxedProcess
{
    private readonly AppContainerLauncher.Launched _launched;
    private readonly JobObject _job;
    private readonly NativeProcess _process;
    private readonly Thread _watcher;
    private readonly JobLimits _limits;
    private volatile int _limitHit;

    /// <summary>Starts the program; <paramref name="fileName"/> must be a full path.</summary>
    public SandboxedCommand(string fileName, IReadOnlyList<string> arguments, JobLimits limits, IReadOnlyCollection<string>? readDirectories = null, IReadOnlyCollection<string>? writeDirectories = null)
    {
        ArgumentNullException.ThrowIfNull(limits);
        ArgumentNullException.ThrowIfNull(arguments);
        if (!Path.IsPathRooted(fileName))
        {
            throw new ArgumentException("The program needs a full path in an AppContainer.", nameof(fileName));
        }

        _limits = limits;
        var programDirectory = Path.GetDirectoryName(Path.GetFullPath(fileName))!;
        var windows = Environment.GetEnvironmentVariable("SystemRoot") ?? @"C:\Windows";
        if (!programDirectory.StartsWith(windows, StringComparison.OrdinalIgnoreCase))
        {
            AppContainerLauncher.Grant(programDirectory, write: false);
        }

        foreach (var directory in readDirectories ?? [])
        {
            AppContainerLauncher.Grant(directory, write: false);
        }

        var firstWrite = default(string);
        foreach (var directory in writeDirectories ?? [])
        {
            AppContainerLauncher.Grant(directory, write: true);
            firstWrite ??= Path.GetFullPath(directory);
        }

        var scratch = firstWrite ?? programDirectory;
        var environment = new Dictionary<string, string>
        {
            ["SystemRoot"] = Environment.GetEnvironmentVariable("SystemRoot") ?? @"C:\Windows",
            ["SystemDrive"] = Environment.GetEnvironmentVariable("SystemDrive") ?? "C:",
            ["windir"] = Environment.GetEnvironmentVariable("windir") ?? @"C:\Windows",
            ["LOCALAPPDATA"] = scratch,
            ["TEMP"] = scratch,
            ["TMP"] = scratch,
        };

        _job = new JobObject(limits);
        var commandLine = string.Join(' ', new[] { fileName }.Concat(arguments).Select(Quote));
        _launched = AppContainerLauncher.Start(fileName, commandLine, programDirectory, environment, _job);
        _process = new NativeProcess(_launched.Process);
        _watcher = new Thread(Watch) { IsBackground = true, Name = "command-limit-watcher" };
        _watcher.Start();
    }

    /// <inheritdoc />
    public Stream Input => _launched.Input;

    /// <inheritdoc />
    public Stream Output => _launched.Output;

    /// <inheritdoc />
    public bool HasExited => _process.HasExited;

    /// <inheritdoc />
    public int ExitCode => _process.ExitCode;

    /// <inheritdoc />
    public LimitHit LimitHit => (LimitHit)_limitHit;

    /// <summary>Quotes one argument for a Windows command line.</summary>
    public static string Quote(string argument)
    {
        ArgumentNullException.ThrowIfNull(argument);
        if (argument.Length > 0 && argument.IndexOfAny([' ', '\t', '"']) < 0)
        {
            return argument;
        }

        var text = new StringBuilder("\"");
        var backslashes = 0;
        foreach (var c in argument)
        {
            if (c == '\\')
            {
                backslashes++;
                continue;
            }

            if (c == '"')
            {
                text.Append('\\', backslashes * 2 + 1).Append('"');
            }
            else
            {
                text.Append('\\', backslashes).Append(c);
            }

            backslashes = 0;
        }

        return text.Append('\\', backslashes * 2).Append('"').ToString();
    }

    /// <inheritdoc />
    public void Kill()
    {
        if (!_process.HasExited)
        {
            _process.Kill();
        }

        _process.WaitForExit(Timeout.Infinite);
    }

    /// <inheritdoc />
    public bool WaitForExit(int milliseconds)
    {
        var exited = _process.WaitForExit(milliseconds);
        if (exited)
        {
            _watcher.Join();
            if (_limitHit == 0 && _limits.MemoryMb > 0 && _process.ExitCode != 0 && _job.PeakProcessMemoryBytes() >= _limits.MemoryMb * 1024L * 1024 * 2 / 3)
            {
                _limitHit = (int)LimitHit.Memory;
            }
        }

        return exited;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Kill();
        _watcher.Join();
        _launched.Dispose();
        _job.Dispose();
    }

    private void Watch()
    {
        var clock = Stopwatch.StartNew();
        while (!_process.WaitForExit(50))
        {
            if (_limits.WallSeconds > 0 && clock.Elapsed.TotalSeconds >= _limits.WallSeconds)
            {
                End(LimitHit.Wall);
                return;
            }

            if (_limits.CpuSeconds > 0 && _process.TotalProcessorTime.TotalSeconds >= _limits.CpuSeconds)
            {
                End(LimitHit.Cpu);
                return;
            }
        }
    }

    private void End(LimitHit hit)
    {
        _limitHit = (int)hit;
        _process.Kill();
    }
}
