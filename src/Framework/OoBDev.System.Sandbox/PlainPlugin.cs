using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace OoBDev.Spike.PluginSandbox;

/// <summary>
/// Runs the plugin as an ordinary child process with the host's environment and rights. It exists as the positive control for the escape tests
/// (the same attempt must work here and be refused in a sandbox) and is never a way to run a real plugin.
/// </summary>
public sealed class PlainPlugin : ISandboxedPlugin
{
    private readonly Process _process;
    private readonly FrameChannel _channel;

    /// <summary>Starts <paramref name="commandArguments"/> with no isolation.</summary>
    public PlainPlugin(string commandArguments, FrameFlags flags = FrameFlags.Crc32 | FrameFlags.Hmac | FrameFlags.CounterSign)
    {
        var key = RandomNumberGenerator.GetBytes(32);
        var dotnetRoot = Path.GetFullPath(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "..", "..", ".."));
        var info = new ProcessStartInfo(Path.Combine(dotnetRoot, OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet"))
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        info.ArgumentList.Add(typeof(PlainPlugin).Assembly.Location);
        foreach (var part in CommandLine.Split(commandArguments))
        {
            info.ArgumentList.Add(part);
        }

        info.Environment[Program.KeyVariable] = Convert.ToHexString(key);
        info.Environment[Program.FlagsVariable] = ((byte)flags).ToString(global::System.Globalization.CultureInfo.InvariantCulture);
        _process = Process.Start(info) ?? throw new InvalidOperationException("could not start the plugin");
        _channel = new FrameChannel(_process.StandardOutput.BaseStream, _process.StandardInput.BaseStream, flags, key);
    }

    /// <inheritdoc />
    /// <inheritdoc />
    public LimitHit LimitHit => LimitHit.None;

    public int ExitCode => _process.ExitCode;

    /// <inheritdoc />
    public ulong Send(FrameType type, string topic, string json) => _channel.Send(type, topic, json);

    /// <summary>Answers <paramref name="request"/> with a JSON payload.</summary>
    public ulong Reply(ReceivedFrame request, FrameType type, string json) => _channel.Reply(request, type, global::System.Text.Encoding.UTF8.GetBytes(json));

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
