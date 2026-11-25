using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using OoBDev.System.Sandbox;

namespace OoBDev.DevTerm;

/// <summary>A running plugin program with its standard streams, whether plain or sandboxed.</summary>
internal abstract class PluginChild : IDisposable
{
    public abstract TextWriter Input { get; }

    public abstract TextReader Output { get; }

    public abstract void Kill();

    public abstract void Dispose();

    /// <summary>Starts the program; with <paramref name="sandbox"/> it runs inside the platform isolation, otherwise as a plain child process.</summary>
    /// <exception cref="InvalidOperationException">The program could not start.</exception>
    public static PluginChild Start(string fileName, IEnumerable<string> arguments, PluginSandbox? sandbox)
    {
        var list = new List<string>(arguments);
        try
        {
            return sandbox is null ? new Plain(fileName, list) : new Isolated(fileName, list, sandbox);
        }
        catch (Exception ex) when (ex is Win32Exception or IOException or InvalidOperationException or UnauthorizedAccessException or ArgumentException)
        {
            throw new InvalidOperationException($"Could not start plugin '{fileName}': {ex.Message}", ex);
        }
    }

    private sealed class Plain : PluginChild
    {
        private readonly Process _process;

        public Plain(string fileName, List<string> arguments)
        {
            var info = new ProcessStartInfo(fileName)
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            foreach (var argument in arguments)
            {
                info.ArgumentList.Add(argument);
            }

            _process = Process.Start(info) ?? throw new InvalidOperationException("The plugin process did not start.");
            // stderr is the plugin's own log; drain it so a chatty plugin can never fill the pipe and block.
            _process.ErrorDataReceived += static (_, _) => { };
            _process.BeginErrorReadLine();
        }

        public override TextWriter Input => _process.StandardInput;

        public override TextReader Output => _process.StandardOutput;

        public override void Kill()
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
                // already gone
            }
        }

        public override void Dispose()
        {
            Kill();
            _process.Dispose();
        }
    }

    private sealed class Isolated : PluginChild
    {
        private readonly ISandboxedProcess _process;
        private readonly StreamWriter _input;
        private readonly StreamReader _output;

        public Isolated(string fileName, List<string> arguments, PluginSandbox sandbox)
        {
            _process = sandbox.Launcher.StartCommand(fileName, arguments, sandbox.ReadDirectories, sandbox.WriteDirectories, sandbox.Limits);
            _input = new StreamWriter(_process.Input, new UTF8Encoding(false)) { NewLine = "\n" };
            _output = new StreamReader(_process.Output, Encoding.UTF8);
        }

        public override TextWriter Input => _input;

        public override TextReader Output => _output;

        public override void Kill() => _process.Kill();

        public override void Dispose() => _process.Dispose();
    }
}
