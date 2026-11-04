using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
namespace OoBDev.DevTerm.Tui.Tests;

/// <summary>
/// Hardware tests through the real command-line mode against the network bench instruments (see
/// <c>docs/how-tos/test-hardware.md</c> for the hosts, terminators and byte delays). Each test reads its host and the
/// shared <c>TCP_DEVICE_PORT</c> from <c>src/.runsettings</c>; a blank value or an unreachable device ends the test
/// Inconclusive, so a run without the bench stays green.
/// </summary>
[TestCategory("Hardware")]
[TestClass]
public sealed class RealHardwareCliTests
{
    public TestContext TestContext { get; set; } = null!;

    private static readonly string _consoleAppDirectory = AppContext.BaseDirectory;

    private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(15);

    private string? GetProperty(string name) => TestContext.Properties.TryGetValue(name, out var value) ? value as string : null;

    [TestMethod]
    [DataRow("HP34401A_TCP_HOST", "*IDN?", "Lf", 50, "HEWLETT-PACKARD,34401A")]
    [DataRow("TEK2230_1_TCP_HOST", "ID?", "Cr", 175, "TEK/2230")]
    [DataRow("TEK2230_2_TCP_HOST", "ID?", "Cr", 175, "TEK/2230")]
    [DataRow("TDS2024_TCP_HOST", "*IDN?", "Lf", 50, "TEKTRONIX,TDS 2024")]
    public async Task CliMode_AgainstRealDevice_AnswersIdentityQuery(string hostParameterName, string query, string lineEnding, int byteDelayMs, string expectedReplySubstring)
    {
        var host = GetProperty(hostParameterName);
        var port = GetProperty("TCP_DEVICE_PORT");

        TestContext.WriteLine($"Host: {host}");
        TestContext.WriteLine($"Port: {port}");
        TestContext.WriteLine($"Query: {query}");

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port))
        {
            Assert.Inconclusive($"No '{hostParameterName}'/'TCP_DEVICE_PORT': run with '--settings src/.runsettings' to exercise this against real hardware.");
            return;
        }

        if (!int.TryParse(port, out var portNumber))
        {
            Assert.Inconclusive($"'TCP_DEVICE_PORT' value '{port}' is not a valid port number.");
            return;
        }

        if (!await RealDeviceReachability.IsTcpReachableAsync(host, portNumber, TestContext.CancellationToken))
        {
            Assert.Inconclusive($"Real device at {host}:{port} is not reachable (TCP connect attempt timed out/refused within {RealDeviceReachability.DefaultTimeout.TotalSeconds}s) — is it powered on and networked?");
            return;
        }

        var startInfo = new ProcessStartInfo(
            "dotnet",
            $"\"{Path.Combine(_consoleAppDirectory, "OoBDev.DevTerm.Tui.dll")}\" --transport tcp --host {host} --port {port} --presenter ascii --lineending {lineEnding} --writebytedelayms {byteDelayMs} --cli true")
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

        TestContext.WriteLine($"Sending: {query}");
        await process.StandardInput.WriteLineAsync(query);
        await process.StandardInput.FlushAsync(TestContext.CancellationToken);

        string? line;
        do
        {
            line = await process.StandardOutput.ReadLineAsync(TestContext.CancellationToken).AsTask().WaitAsync(_timeout, TestContext.CancellationToken);
        }
        while (line is not null && !line.Contains("[ascii]", StringComparison.Ordinal));

        TestContext.WriteLine($"Received: {line}");

        Assert.IsNotNull(line, $"Expected a decoded reply from the real device at {host}:{port} before the process ran out of output.");
        Assert.Contains(expectedReplySubstring, line);

        process.StandardInput.Close();
        await process.WaitForExitAsync(TestContext.CancellationToken).WaitAsync(_timeout, TestContext.CancellationToken);
        Assert.AreEqual(0, process.ExitCode);
    }
}
