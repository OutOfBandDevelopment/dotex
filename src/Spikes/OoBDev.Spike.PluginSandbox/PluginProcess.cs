using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text.Json;

namespace OoBDev.Spike.PluginSandbox;

/// <summary>Launches this assembly as a plugin in a limited job and speaks the framed protocol with it.</summary>
[SupportedOSPlatform("windows")]
public sealed class PluginProcess : IDisposable
{
    private readonly Process _process;
    private readonly JobObject _job;

    /// <summary>Starts the plugin in <paramref name="mode"/> with a per-process <paramref name="memoryMb"/> cap.</summary>
    public PluginProcess(string mode, int memoryMb)
    {
        _job = new JobObject(memoryMb);
        var info = new ProcessStartInfo("dotnet", $"\"{typeof(PluginProcess).Assembly.Location}\" {mode}")
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        _process = Process.Start(info) ?? throw new InvalidOperationException("could not start the plugin");
        _job.Assign(_process);
    }

    /// <summary>True once the plugin process has exited.</summary>
    public bool HasExited => _process.HasExited;

    /// <summary>The exit code of the finished plugin.</summary>
    public int ExitCode => _process.ExitCode;

    /// <summary>Sends one JSON message.</summary>
    public void Send(string json) => Frame.Write(_process.StandardInput.BaseStream, json);

    /// <summary>Reads one message. A wire violation kills the plugin (Table 8) and is rethrown.</summary>
    public JsonDocument Receive()
    {
        try
        {
            return Frame.Read(_process.StandardOutput.BaseStream);
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
