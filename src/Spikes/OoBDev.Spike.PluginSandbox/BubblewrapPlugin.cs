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
/// frame protocol over stdio as the Windows launcher. With <c>sandboxed</c> false it starts the same plugin directly, as a positive control.
/// Not built on Linux yet: memory, CPU and process-count limits (the Windows job object has no bubblewrap equivalent; cgroups or rlimits are the route).
/// </summary>
[SupportedOSPlatform("linux")]
public sealed class BubblewrapPlugin : IDisposable
{
    private readonly Process _process;
    private readonly FrameChannel _channel;

    /// <summary>Starts the plugin; the folders are bound at the same path inside the sandbox.</summary>
    public BubblewrapPlugin(string commandArguments, IReadOnlyCollection<string>? readDirectories = null, IReadOnlyCollection<string>? writeDirectories = null, bool sandboxed = true, FrameFlags flags = FrameFlags.Crc32 | FrameFlags.Hmac | FrameFlags.CounterSign)
    {
        var key = RandomNumberGenerator.GetBytes(32);
        var dll = typeof(BubblewrapPlugin).Assembly.Location;
        var dotnetRoot = Path.GetFullPath(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "..", "..", ".."));
        var info = new ProcessStartInfo(sandboxed ? "bwrap" : Path.Combine(dotnetRoot, "dotnet"))
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        var keyHex = Convert.ToHexString(key);
        var flagText = ((byte)flags).ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (sandboxed)
        {
            foreach (var argument in Arguments(dll, dotnetRoot, keyHex, flagText, readDirectories ?? [], writeDirectories ?? [], commandArguments))
            {
                info.ArgumentList.Add(argument);
            }
        }
        else
        {
            info.ArgumentList.Add(dll);
            foreach (var part in commandArguments.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                info.ArgumentList.Add(part.Trim('"'));
            }

            info.Environment[Program.KeyVariable] = keyHex;
            info.Environment[Program.FlagsVariable] = flagText;
        }

        _process = Process.Start(info) ?? throw new InvalidOperationException("could not start the plugin");
        _channel = new FrameChannel(_process.StandardOutput.BaseStream, _process.StandardInput.BaseStream, flags, key);
    }

    /// <summary>The plugin's exit code once it has ended.</summary>
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
        foreach (var part in commandArguments.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            args.Add(part.Trim('"'));
        }

        return args;
    }

    /// <summary>Sends a frame with a JSON payload.</summary>
    public ulong Send(FrameType type, string topic, string json) => _channel.Send(type, topic, json);

    /// <summary>Reads one frame.</summary>
    public ReceivedFrame Receive() => _channel.Read();

    /// <summary>Waits up to <paramref name="milliseconds"/> for the plugin to end.</summary>
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
