using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;

namespace OoBDev.Spike.PluginSandbox;

/// <summary>
/// Runs this assembly as a plugin under bubblewrap on Linux (REQ-008): new user, PID, IPC, UTS and network namespaces (so no network),
/// a read-only system, an empty temp folder, an explicit environment, and only the granted folders bound in. It speaks the same binary
/// frame protocol over stdio as the other launchers.
/// Not built on Linux yet: memory, CPU and process-count limits (the Windows job object has no bubblewrap equivalent; cgroups or rlimits are the route).
/// </summary>
[SupportedOSPlatform("linux")]
public sealed class BubblewrapPlugin : ISandboxedPlugin
{
    private readonly Process _process;
    private readonly FrameChannel _channel;

    /// <summary>Starts the plugin; the folders are bound at the same path inside the sandbox.</summary>
    public BubblewrapPlugin(string commandArguments, IReadOnlyCollection<string>? readDirectories = null, IReadOnlyCollection<string>? writeDirectories = null, FrameFlags flags = FrameFlags.Crc32 | FrameFlags.Hmac | FrameFlags.CounterSign)
    {
        var key = RandomNumberGenerator.GetBytes(32);
        var dll = typeof(BubblewrapPlugin).Assembly.Location;
        var dotnetRoot = Path.GetFullPath(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "..", "..", ".."));
        var info = new ProcessStartInfo("bwrap")
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        var flagText = ((byte)flags).ToString(System.Globalization.CultureInfo.InvariantCulture);
        foreach (var argument in Arguments(dll, dotnetRoot, Convert.ToHexString(key), flagText, readDirectories ?? [], writeDirectories ?? [], commandArguments))
        {
            info.ArgumentList.Add(argument);
        }

        _process = Process.Start(info) ?? throw new InvalidOperationException("could not start the plugin");
        _channel = new FrameChannel(_process.StandardOutput.BaseStream, _process.StandardInput.BaseStream, flags, key);
    }

    /// <inheritdoc />
    public int ExitCode => _process.ExitCode;

    /// <summary>Builds the bubblewrap command line; public so a test can assert what is and is not mounted.</summary>
    public static IReadOnlyList<string> Arguments(string dll, string dotnetRoot, string keyHex, string flags, IReadOnlyCollection<string> read, IReadOnlyCollection<string> write, string commandArguments)
    {
        var args = new List<string>
        {
            "--unshare-all", "--die-with-parent", "--new-session", "--clearenv",
            "--ro-bind", "/usr", "/usr",
            "--symlink", "usr/lib", "/lib", "--symlink", "usr/lib64", "/lib64", "--symlink", "usr/bin", "/bin",
            "--proc", "/proc", "--dev", "/dev", "--tmpfs", "/tmp",
            "--setenv", "HOME", "/tmp", "--setenv", "TMPDIR", "/tmp", "--setenv", "DOTNET_ROOT", dotnetRoot, "--setenv", "DOTNET_NOLOGO", "1",
            "--setenv", Program.KeyVariable, keyHex, "--setenv", Program.FlagsVariable, flags,
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

    /// <inheritdoc />
    public ReceivedFrame Receive() => _channel.Read();

    /// <inheritdoc />
    public bool WaitForExit(int milliseconds) => _process.WaitForExit(milliseconds);

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_process.HasExited)
        {
            _process.Kill(entireProcessTree: true);
        }

        _process.WaitForExit();
        _process.Dispose();
    }
}
