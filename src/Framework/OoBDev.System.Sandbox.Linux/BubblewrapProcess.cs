using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Versioning;

namespace OoBDev.System.Sandbox.Linux;

/// <summary>
/// Runs any program under bubblewrap with the isolation of <see cref="BubblewrapPlugin"/> (no network, read-only system, empty temp folder, explicit environment,
/// only the granted folders bound in) and hands back its plain standard streams.
/// </summary>
[SupportedOSPlatform("linux")]
public sealed class BubblewrapProcess : ISandboxedProcess
{
    private readonly Process _process;
    private readonly global::System.Threading.Timer? _wall;
    private readonly LinuxProcessWatcher? _watcher;
    private readonly JobLimits _limits;
    private volatile int _hit;

    /// <summary>Starts the program; the folders are bound at the same path inside the sandbox.</summary>
    public BubblewrapProcess(string fileName, IReadOnlyList<string> arguments, IReadOnlyCollection<string>? readDirectories = null, IReadOnlyCollection<string>? writeDirectories = null, JobLimits? limits = null)
    {
        _limits = limits ?? new JobLimits(MemoryMb: 256);
        var cpu = _limits.CpuSeconds;
        var info = new ProcessStartInfo(cpu > 0 ? "/bin/sh" : "bwrap")
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        if (cpu > 0)
        {
            info.ArgumentList.Add("-c");
            info.ArgumentList.Add("ulimit -t \"$0\"; exec bwrap \"$@\"");
            info.ArgumentList.Add(cpu.ToString(CultureInfo.InvariantCulture));
        }

        foreach (var argument in Arguments(fileName, arguments, readDirectories ?? [], writeDirectories ?? []))
        {
            info.ArgumentList.Add(argument);
        }

        _process = Process.Start(info) ?? throw new InvalidOperationException("could not start the program");
        _process.ErrorDataReceived += static (_, _) => { };
        _process.BeginErrorReadLine();
        if (_limits is { MaxProcesses: > 0 } or { CpuSeconds: > 0 } or { MemoryMb: > 0 })
        {
            _watcher = new LinuxProcessWatcher(_process.Id, _limits, hit => End(hit));
        }

        if (_limits.WallSeconds > 0)
        {
            _wall = new global::System.Threading.Timer(_ => End(LimitHit.Wall), null, TimeSpan.FromSeconds(_limits.WallSeconds), global::System.Threading.Timeout.InfiniteTimeSpan);
        }
    }

    /// <inheritdoc />
    public global::System.IO.Stream Input => _process.StandardInput.BaseStream;

    /// <inheritdoc />
    public global::System.IO.Stream Output => _process.StandardOutput.BaseStream;

    /// <inheritdoc />
    public bool HasExited => _process.HasExited;

    /// <inheritdoc />
    public int ExitCode => _process.ExitCode;

    /// <inheritdoc />
    public LimitHit LimitHit => (LimitHit)_hit;

    /// <summary>Builds the bubblewrap command line; public so a test can assert what is and is not mounted.</summary>
    public static IReadOnlyList<string> Arguments(string fileName, IReadOnlyList<string> arguments, IReadOnlyCollection<string> read, IReadOnlyCollection<string> write)
    {
        var args = new List<string>
        {
            "--unshare-all", "--new-session", "--clearenv", "--die-with-parent",
            "--ro-bind", "/usr", "/usr",
            "--symlink", "usr/lib", "/lib", "--symlink", "usr/lib64", "/lib64", "--symlink", "usr/bin", "/bin",
            "--proc", "/proc", "--dev", "/dev", "--tmpfs", "/tmp",
            "--setenv", "HOME", "/tmp", "--setenv", "TMPDIR", "/tmp", "--setenv", "PATH", "/usr/bin:/bin",
        };

        var bound = new HashSet<string>(StringComparer.Ordinal) { "/usr" };
        void Bind(string option, string path)
        {
            var full = Path.GetFullPath(path);
            if (!full.StartsWith("/usr/", StringComparison.Ordinal) && bound.Add(full))
            {
                args.AddRange([option, full, full]);
            }
        }

        foreach (var directory in read)
        {
            Bind("--ro-bind", directory);
        }

        foreach (var directory in write)
        {
            Bind("--bind", directory);
        }

        args.Add("--");
        args.Add(fileName);
        args.AddRange(arguments);
        return args;
    }

    /// <inheritdoc />
    public void Kill()
    {
        try
        {
            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
        }
    }

    /// <inheritdoc />
    public bool WaitForExit(int milliseconds) => _process.WaitForExit(milliseconds);

    /// <inheritdoc />
    public void Dispose()
    {
        Kill();
        _wall?.Dispose();
        _watcher?.Dispose();
        _process.WaitForExit();
        _process.Dispose();
    }

    private void End(LimitHit hit)
    {
        if (!_process.HasExited)
        {
            _hit = (int)hit;
        }

        Kill();
    }
}
