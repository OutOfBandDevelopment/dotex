using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Documents.Guides;

/// <summary>Runs a command-line tool and captures its output as a <see cref="CliExample"/>.</summary>
public static class CliToolRunner
{
    /// <summary>Runs <paramref name="fileName"/> with <paramref name="arguments"/> and waits for it to exit.</summary>
    /// <param name="title">Example heading.</param>
    /// <param name="description">Example description.</param>
    /// <param name="fileName">Executable to start (for example <c>dotnet</c>).</param>
    /// <param name="arguments">Arguments passed to the executable.</param>
    /// <param name="displayCommandLine">The command line shown in the guide, so machine-specific paths stay out of it.</param>
    /// <param name="workingDirectory">Working directory, or null for the current one.</param>
    /// <param name="cancellationToken">Cancels the wait and kills the process.</param>
    public static async Task<CliExample> RunAsync(
        string title,
        string description,
        string fileName,
        IReadOnlyList<string> arguments,
        string displayCommandLine,
        string? workingDirectory = null,
        CancellationToken cancellationToken = default)
    {
        var info = new ProcessStartInfo(fileName)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };
        if (workingDirectory is not null)
        {
            info.WorkingDirectory = workingDirectory;
        }

        foreach (var argument in arguments)
        {
            info.ArgumentList.Add(argument);
        }

        var output = new StringBuilder();
        var gate = new object();
        using var process = new Process { StartInfo = info };
        process.OutputDataReceived += (_, e) => Append(output, gate, e.Data);
        process.ErrorDataReceived += (_, e) => Append(output, gate, e.Data);
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        try
        {
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw;
        }

        process.WaitForExit();
        return new CliExample(title, description, displayCommandLine, output.ToString(), process.ExitCode);
    }

    private static void Append(StringBuilder output, object gate, string? line)
    {
        if (line is null)
        {
            return;
        }

        lock (gate)
        {
            output.Append(line).Append('\n');
        }
    }
}
