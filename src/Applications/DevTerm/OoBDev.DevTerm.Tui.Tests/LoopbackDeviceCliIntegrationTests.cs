using OoBDev.TestUtilities.Containers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
namespace OoBDev.DevTerm.Tui.Tests;

/// <summary>
/// The command-line mode over the TCP transport against the <c>loopback-device</c> container of the integration stack
/// (<c>containers/testing</c>). It is the automated stand-in for the real-hardware TCP tests: same path, a scripted peer.
/// Endpoint keys <c>LOOPBACK_DEVICE_HOST</c> and <c>LOOPBACK_DEVICE_PORT</c> come from <c>src/.runsettings</c>; an
/// unreachable peer ends the test Inconclusive.
/// </summary>
[TestCategory("Integration")]
[TestClass]
public sealed class LoopbackDeviceCliIntegrationTests
{
    public TestContext TestContext { get; set; } = null!;

    private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(15);

    private string GetProperty(string name, string fallback) =>
        TestContext.Properties.TryGetValue(name, out var value) && value is string text && text.Length > 0 ? text : fallback;

    [IntegrationTestMethod("oobd-test-loopback-device")]
    [DataRow("hello", "From Loopback test")]
    [DataRow("STATUS?", "temp=")]
    [DataRow("nonsense", "ERR unknown command")]
    public async Task CliMode_AgainstTheLoopbackDevice_AnswersTheCommand(string command, string expectedReplySubstring)
    {
        // Stage
        var host = GetProperty("LOOPBACK_DEVICE_HOST", "127.0.0.1");
        var port = int.Parse(GetProperty("LOOPBACK_DEVICE_PORT", "7778"), CultureInfo.InvariantCulture);
        if (!await RealDeviceReachability.IsTcpReachableAsync(host, port, TestContext.CancellationToken))
        {
            Assert.Inconclusive($"The loopback-device container is not reachable at {host}:{port}: start the stack with containers/testing/scripts/integration-up.sh.");
            return;
        }

        var startInfo = new ProcessStartInfo(
            "dotnet",
            $"\"{Path.Combine(AppContext.BaseDirectory, "OoBDev.DevTerm.Tui.dll")}\" --transport tcp --host {host} --port {port} --presenter ascii --lineending Lf --cli true")
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var job = new ChildProcessJob();
        using var process = Process.Start(startInfo)!;
        job.Add(process);

        // Test
        await process.StandardInput.WriteLineAsync(command);
        await process.StandardInput.FlushAsync(TestContext.CancellationToken);

        string? line;
        do
        {
            line = await process.StandardOutput.ReadLineAsync(TestContext.CancellationToken).AsTask().WaitAsync(_timeout, TestContext.CancellationToken);
        }
        while (line is not null && !line.Contains("[ascii]", StringComparison.Ordinal));

        process.StandardInput.Close();
        await process.WaitForExitAsync(TestContext.CancellationToken).WaitAsync(_timeout, TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(line, "Expected a decoded reply from the loopback device.");
        Assert.Contains(expectedReplySubstring, line);
        Assert.AreEqual(0, process.ExitCode);
    }
}
