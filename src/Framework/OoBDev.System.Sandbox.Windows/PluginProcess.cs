using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Security.Cryptography;

namespace OoBDev.System.Sandbox.Windows;

/// <summary>Launches this assembly as a plugin in a limited job and speaks the framed protocol with it.</summary>
[SupportedOSPlatform("windows")]
public sealed class PluginProcess : IDisposable
{
    private readonly Process _process;
    private readonly JobObject _job;
    private readonly FrameChannel _channel;

    /// <summary>Starts the plugin in <paramref name="mode"/> with a per-process <paramref name="memoryMb"/> cap.</summary>
    public PluginProcess(string pluginAssembly, string mode, int memoryMb, FrameFlags flags = FrameFlags.Crc32 | FrameFlags.Hmac | FrameFlags.CounterSign)
    {
        _job = new JobObject(memoryMb);
        var key = RandomNumberGenerator.GetBytes(32);
        var info = new ProcessStartInfo("dotnet", $"\"{pluginAssembly}\" {mode}")
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        info.Environment[SandboxEnvironment.KeyVariable] = Convert.ToHexString(key);
        info.Environment[SandboxEnvironment.FlagsVariable] = ((byte)flags).ToString();
        _process = Process.Start(info) ?? throw new InvalidOperationException("could not start the plugin");
        _job.Assign(_process);
        _channel = new FrameChannel(_process.StandardOutput.BaseStream, _process.StandardInput.BaseStream, flags, key);
    }

    /// <summary>True once the plugin process has exited.</summary>
    public bool HasExited => _process.HasExited;

    /// <summary>The exit code of the finished plugin.</summary>
    public int ExitCode => _process.ExitCode;

    /// <summary>Sends one JSON message.</summary>
    public ulong Send(FrameType type, string topic, string json) => _channel.Send(type, topic, json);

    /// <summary>Reads one message. A wire violation kills the plugin (Table 8) and is rethrown.</summary>
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
            _process.Kill(entireProcessTree: true);
        }

        _process.WaitForExit();
    }

    /// <summary>Waits up to <paramref name="milliseconds"/> for the plugin to end.</summary>
    public bool WaitForExit(int milliseconds) => _process.WaitForExit(milliseconds);

    /// <inheritdoc />
    public void Dispose()
    {
        _job.Dispose();
        _process.Dispose();
    }
}
