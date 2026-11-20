using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Threading;

namespace OoBDev.System.Sandbox.Windows;

/// <summary>
/// Runs this assembly as a plugin in an AppContainer (no network, no files beyond its grants, explicit environment, three inherited
/// handles) inside a job object with the limits of <see cref="JobLimits"/>, and speaks the binary frame protocol with it.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class SandboxedPlugin : ISandboxedPlugin
{
    private readonly AppContainerLauncher.Launched _launched;
    private readonly JobObject _job;
    private readonly FrameChannel _channel;
    private readonly NativeProcess _process;
    private readonly Thread _watcher;
    private readonly JobLimits _limits;
    private volatile int _limitHit;

    /// <summary>Starts the plugin; <paramref name="killOnClose"/> false lets it outlive the process that started it (Survive). <paramref name="readDirectories"/> and <paramref name="writeDirectories"/> are the only folders it can reach besides the system and its own assembly folder.</summary>
    public SandboxedPlugin(string pluginAssembly, string commandArguments, JobLimits limits, IReadOnlyCollection<string>? readDirectories = null, IReadOnlyCollection<string>? writeDirectories = null, FrameFlags flags = FrameFlags.Crc32 | FrameFlags.Hmac | FrameFlags.CounterSign, bool killOnClose = true)
    {
        ArgumentNullException.ThrowIfNull(limits);
        _limits = limits;
        var assemblyDirectory = Path.GetDirectoryName(pluginAssembly)!;
        AppContainerLauncher.Grant(assemblyDirectory, write: false);
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

        var key = RandomNumberGenerator.GetBytes(32);
        var dotnetRoot = Path.GetFullPath(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "..", "..", ".."));
        var dotnet = Path.Combine(dotnetRoot, "dotnet.exe");
        var scratch = firstWrite ?? assemblyDirectory;
        var environment = new Dictionary<string, string>
        {
            ["SystemRoot"] = Environment.GetEnvironmentVariable("SystemRoot") ?? @"C:\Windows",
            ["SystemDrive"] = Environment.GetEnvironmentVariable("SystemDrive") ?? "C:",
            ["windir"] = Environment.GetEnvironmentVariable("windir") ?? @"C:\Windows",
            ["ProgramData"] = Environment.GetEnvironmentVariable("ProgramData") ?? @"C:\ProgramData",
            ["LOCALAPPDATA"] = scratch,
            ["TEMP"] = scratch,
            ["TMP"] = scratch,
            ["DOTNET_ROOT"] = dotnetRoot,
            ["DOTNET_CLI_HOME"] = scratch,
            ["DOTNET_NOLOGO"] = "1",
            [SandboxEnvironment.KeyVariable] = Convert.ToHexString(key),
            [SandboxEnvironment.FlagsVariable] = ((byte)flags).ToString(),
        };

        _job = new JobObject(limits, killOnClose);
        var commandLine = $"\"{dotnet}\" \"{pluginAssembly}\" {commandArguments}";
        _launched = AppContainerLauncher.Start(dotnet, commandLine, assemblyDirectory, environment, _job);
        _process = new NativeProcess(_launched.Process);
        _channel = new FrameChannel(_launched.Output, _launched.Input, flags, key);
        _watcher = new Thread(Watch) { IsBackground = true, Name = "plugin-limit-watcher" };
        _watcher.Start();
    }

    /// <summary>The operating system process id (valid to read after disposal, to prove the process is gone).</summary>
    public int ProcessId => _launched.ProcessId;

    /// <summary>True once the plugin process has ended.</summary>
    public bool HasExited => _process.HasExited;

    /// <summary>The plugin's exit code.</summary>
    public int ExitCode => _process.ExitCode;

    /// <summary>The limit that ended the plugin, once it has ended.</summary>
    public LimitHit LimitHit => (LimitHit)_limitHit;

    /// <summary>Peak memory the job saw, in bytes.</summary>
    public long PeakMemoryBytes => _job.PeakProcessMemoryBytes();

    /// <summary>Everything the plugin wrote to standard error; call after it has ended.</summary>
    public string ReadStandardError() => new StreamReader(_launched.Error).ReadToEnd();

    /// <summary>Sends a frame with a JSON payload.</summary>
    public ulong Send(FrameType type, string topic, string json) => _channel.Send(type, topic, json);

    /// <summary>Answers <paramref name="request"/> with a JSON payload.</summary>
    public ulong Reply(ReceivedFrame request, FrameType type, string json) => _channel.Reply(request, type, global::System.Text.Encoding.UTF8.GetBytes(json));

    /// <summary>Reads one frame; a wire violation kills the plugin and is rethrown.</summary>
    public ReceivedFrame Receive()
    {
        try
        {
            return _channel.Read();
        }
        catch (FrameViolation)
        {
            Kill();
            throw;
        }
    }

    /// <summary>Kills the plugin and waits for it to end.</summary>
    public void Kill()
    {
        if (!_process.HasExited)
        {
            _process.Kill();
        }

        _process.WaitForExit(Timeout.Infinite);
    }

    /// <summary>Waits up to <paramref name="milliseconds"/> for the plugin to end and settles which limit, if any, ended it.</summary>
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
