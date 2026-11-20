using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;

namespace OoBDev.System.Sandbox.Linux;

/// <summary>
/// Runs this assembly as a plugin under bubblewrap on Linux (REQ-008): new user, PID, IPC, UTS and network namespaces (so no network),
/// a read-only system, an empty temp folder, an explicit environment, and only the granted folders bound in. It speaks the same binary
/// frame protocol over stdio as the other launchers.
/// Limits: memory through the .NET GC hard limit, CPU time through ulimit -t, wall time through a kill timer. A process-count limit needs cgroup delegation and is not built.
/// </summary>
[SupportedOSPlatform("linux")]
public sealed class BubblewrapPlugin : ISandboxedPlugin
{
    private readonly Process _process;
    private readonly FrameChannel _channel;
    private readonly global::System.Threading.Timer? _wall;
    private readonly LinuxProcessWatcher? _watcher;
    private readonly JobLimits? _limits;
    private volatile int _hit;

    /// <summary>Starts the plugin; the folders are bound at the same path inside the sandbox.</summary>
    public BubblewrapPlugin(string pluginAssembly, string commandArguments, IReadOnlyCollection<string>? readDirectories = null, IReadOnlyCollection<string>? writeDirectories = null, FrameFlags flags = FrameFlags.Crc32 | FrameFlags.Hmac | FrameFlags.CounterSign, JobLimits? limits = null)
    {
        var key = RandomNumberGenerator.GetBytes(32);
        var dll = pluginAssembly;
        var dotnetRoot = Path.GetFullPath(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "..", "..", ".."));
        var cpu = limits?.CpuSeconds ?? 0;
        var info = new ProcessStartInfo(cpu > 0 ? "/bin/sh" : "bwrap")
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        var flagText = ((byte)flags).ToString(global::System.Globalization.CultureInfo.InvariantCulture);
        if (cpu > 0)
        {
            info.ArgumentList.Add("-c");
            info.ArgumentList.Add("ulimit -t \"$0\"; exec bwrap \"$@\"");
            info.ArgumentList.Add(cpu.ToString(global::System.Globalization.CultureInfo.InvariantCulture));
        }

        foreach (var argument in Arguments(dll, dotnetRoot, Convert.ToHexString(key), flagText, readDirectories ?? [], writeDirectories ?? [], commandArguments, limits?.MemoryMb ?? 0))
        {
            info.ArgumentList.Add(argument);
        }

        _process = Process.Start(info) ?? throw new InvalidOperationException("could not start the plugin");
        _channel = new FrameChannel(_process.StandardOutput.BaseStream, _process.StandardInput.BaseStream, flags, key);
        _limits = limits;
        if (limits is { MaxProcesses: > 0 } or { CpuSeconds: > 0 } or { MemoryMb: > 0 })
        {
            _watcher = new LinuxProcessWatcher(_process.Id, limits, hit => End(hit));
        }

        if (limits?.WallSeconds > 0)
        {
            _wall = new global::System.Threading.Timer(_ => End(LimitHit.Wall), null, TimeSpan.FromSeconds(limits.WallSeconds), global::System.Threading.Timeout.InfiniteTimeSpan);
        }
    }

    /// <summary>The process id of the bubblewrap process that holds the sandbox.</summary>
    public int ProcessId => _process.Id;

    /// <inheritdoc />
    public int ExitCode => _process.ExitCode;

    /// <inheritdoc />
    public LimitHit LimitHit
    {
        get
        {
            if (_hit != 0)
            {
                return (LimitHit)_hit;
            }

            if (!_process.HasExited || _process.ExitCode == 0 || _limits is null || _watcher is null)
            {
                return LimitHit.None;
            }

            if (_limits.CpuSeconds > 0 && _watcher.CpuSeconds >= _limits.CpuSeconds * 0.8)
            {
                return LimitHit.Cpu;
            }

            return _limits.MemoryMb > 0 && _watcher.PeakMemoryMb >= _limits.MemoryMb * 2.0 / 3 ? LimitHit.Memory : LimitHit.None;
        }
    }

    /// <summary>Builds the bubblewrap command line; public so a test can assert what is and is not mounted.</summary>
    public static IReadOnlyList<string> Arguments(string dll, string dotnetRoot, string keyHex, string flags, IReadOnlyCollection<string> read, IReadOnlyCollection<string> write, string commandArguments, int memoryMb = 0)
    {
        var args = new List<string>
        {
            "--unshare-all", "--die-with-parent", "--new-session", "--clearenv",
            "--ro-bind", "/usr", "/usr",
            "--symlink", "usr/lib", "/lib", "--symlink", "usr/lib64", "/lib64", "--symlink", "usr/bin", "/bin",
            "--proc", "/proc", "--dev", "/dev", "--tmpfs", "/tmp",
            "--setenv", "HOME", "/tmp", "--setenv", "TMPDIR", "/tmp", "--setenv", "DOTNET_ROOT", dotnetRoot, "--setenv", "DOTNET_NOLOGO", "1",
            "--setenv", SandboxEnvironment.KeyVariable, keyHex, "--setenv", SandboxEnvironment.FlagsVariable, flags,
        };

        if (memoryMb > 0)
        {
            args.AddRange(["--setenv", "DOTNET_GCHeapHardLimit", ((long)memoryMb * 1024 * 1024).ToString("X", global::System.Globalization.CultureInfo.InvariantCulture)]);
        }

        var bound = new HashSet<string>(StringComparer.Ordinal) { "/usr" };
        void Bind(string option, string path)
        {
            var full = Path.GetFullPath(path);
            if (!full.StartsWith("/usr/", StringComparison.Ordinal) && bound.Add(full))
            {
                args.AddRange([option, full, full]);
            }
        }

        Bind("--ro-bind", dotnetRoot);
        Bind("--ro-bind", Path.GetDirectoryName(dll)!);
        foreach (var directory in read)
        {
            Bind("--ro-bind", directory);
        }

        foreach (var directory in write)
        {
            Bind("--bind", directory);
        }

        args.AddRange(["--", Path.Combine(dotnetRoot, "dotnet"), dll]);
        args.AddRange(CommandLine.Split(commandArguments));
        return args;
    }

    /// <inheritdoc />
    public ulong Send(FrameType type, string topic, string json) => _channel.Send(type, topic, json);

    /// <summary>Answers <paramref name="request"/> with a JSON payload.</summary>
    public ulong Reply(ReceivedFrame request, FrameType type, string json) => _channel.Reply(request, type, global::System.Text.Encoding.UTF8.GetBytes(json));

    /// <inheritdoc />
    public ReceivedFrame Receive() => _channel.Read();

    /// <inheritdoc />
    public bool WaitForExit(int milliseconds) => _process.WaitForExit(milliseconds);

    private void End(LimitHit hit)
    {
        if (!_process.HasExited)
        {
            _hit = (int)hit;
        }

        Kill();
    }

    private void Kill()
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
    public void Dispose()
    {
        if (!_process.HasExited)
        {
            _process.Kill(entireProcessTree: true);
        }

        _wall?.Dispose();
        _watcher?.Dispose();
        _process.WaitForExit();
        _process.Dispose();
    }
}
