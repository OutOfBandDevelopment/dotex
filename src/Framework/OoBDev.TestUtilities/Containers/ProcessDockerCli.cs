using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.TestUtilities.Containers;

/// <summary>Runs the <c>docker</c> command line.</summary>
public sealed class ProcessDockerCli : IDockerCli
{
    /// <inheritdoc />
    public async Task<ContainerState?> InspectAsync(string container, CancellationToken cancellationToken)
    {
        var (code, output) = await RunAsync(["inspect", "-f", "{{.State.Running}}|{{if .State.Health}}{{.State.Health.Status}}{{end}}", container], cancellationToken);
        if (code != 0)
            return null;
        var parts = output.Trim().Split('|');
        return new ContainerState(string.Equals(parts[0], "true", StringComparison.OrdinalIgnoreCase), parts.Length > 1 ? parts[1] : string.Empty);
    }

    /// <inheritdoc />
    public async Task StartAsync(string container, CancellationToken cancellationToken) =>
        await RunAsync(["start", container], cancellationToken);

    /// <inheritdoc />
    public async Task ComposeUpAsync(string composeFile, string service, CancellationToken cancellationToken) =>
        await RunAsync(["compose", "-f", composeFile, "up", "-d", "--no-deps", service], cancellationToken);

    /// <inheritdoc />
    public async Task<bool> LogContainsAsync(string container, string text, CancellationToken cancellationToken)
    {
        var (inspected, startedAt) = await RunAsync(["inspect", "-f", "{{.State.StartedAt}}", container], cancellationToken);
        if (inspected != 0)
            return false;

        var (code, output) = await RunAsync(["logs", "--since", startedAt.Trim(), container], cancellationToken, includeError: true);
        return code == 0 && output.Contains(text, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public async Task<bool?> NoneRunningAsync(string namePrefix, CancellationToken cancellationToken)
    {
        var (code, output) = await RunAsync(["ps", "-q", "--filter", "name=" + namePrefix], cancellationToken);
        return code != 0 ? null : string.IsNullOrWhiteSpace(output);
    }

    /// <inheritdoc />
    public async Task<bool> StartWavesAsync(string scriptPath, CancellationToken cancellationToken)
    {
        var (code, _) = await RunAsync(["-NoProfile", "-File", scriptPath], cancellationToken, fileName: "pwsh");
        return code == 0;
    }

    /// <inheritdoc />
    public async Task StopAsync(string container, TimeSpan grace, CancellationToken cancellationToken) =>
        await RunAsync(["stop", "-t", Math.Max(1, (int)grace.TotalSeconds).ToString(CultureInfo.InvariantCulture), container], cancellationToken);

    private static async Task<(int Code, string Output)> RunAsync(string[] arguments, CancellationToken cancellationToken, bool includeError = false, string fileName = "docker")
    {
        var info = new ProcessStartInfo(fileName) { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
        foreach (var argument in arguments)
            info.ArgumentList.Add(argument);

        Process process;
        try
        {
            process = Process.Start(info) ?? throw new InvalidOperationException(fileName + " did not start.");
        }
        catch (Win32Exception)
        {
            return (-1, string.Empty);
        }

        using (process)
        {
            var output = process.StandardOutput.ReadToEndAsync(CancellationToken.None);
            var error = process.StandardError.ReadToEndAsync(CancellationToken.None);
            try
            {
                await process.WaitForExitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                process.Kill(true);
                throw;
            }

            return (process.ExitCode, includeError ? await output + await error : await output);
        }
    }
}
