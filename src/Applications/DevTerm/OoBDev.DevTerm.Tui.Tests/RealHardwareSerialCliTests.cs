using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.DevTerm.Tui.Tests;

/// <summary>
/// Hardware tests through the real command-line mode against the serial bench supplies. A Korad supply has no line terminator, so
/// the reply is read with the hex presenter (the ascii presenter would wait for a line end) and compared as hex. The query is
/// read-only. A blank or absent port, or one another program holds, ends the test Inconclusive.
/// </summary>
[TestClass]
[TestCategory(TestCategories.Hardware)]
[TestCategory(TestCategories.Serial)]
public sealed class RealHardwareSerialCliTests
{
    private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(15);

    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [TestCategory(TestCategories.Korad_Ka3005p)]
    [DataRow("KORAD_KA3005P_SERIAL_PORT", "4B4F524144204B4133303035")]
    [TestCategory(TestCategories.Korad_Ka6003p)]
    [DataRow("KORAD_KA6003P_SERIAL_PORT", "4B4F524144204B4136303033")]
    public async Task CliMode_AgainstKoradSupply_AnswersIdentityQuery(string portParameterName, string expectedHexPrefix)
    {
        var portName = TestContext.Properties.TryGetValue(portParameterName, out var value) ? value as string : null;
        if (string.IsNullOrWhiteSpace(portName) || !SerialPort.GetPortNames().Contains(portName, StringComparer.OrdinalIgnoreCase))
        {
            Assert.Inconclusive($"'{portParameterName}' is blank or not present: run with '--settings src/.runsettings' with the supply attached.");
            return;
        }

        var startInfo = new ProcessStartInfo(
            "dotnet",
            $"\"{Path.Combine(AppContext.BaseDirectory, "OoBDev.DevTerm.Tui.dll")}\" --transport serial --port {portName} --presenter hex --lineending None --cli true")
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

        await process.StandardInput.WriteLineAsync("*IDN?").ConfigureAwait(false);
        await process.StandardInput.FlushAsync(TestContext.CancellationToken).ConfigureAwait(false);

        string? line;
        do
        {
            line = await process.StandardOutput.ReadLineAsync(TestContext.CancellationToken).AsTask().WaitAsync(_timeout, TestContext.CancellationToken).ConfigureAwait(false);
        }
        while (line is not null && !line.Contains("[hex]", StringComparison.Ordinal) && !line.Contains("Access", StringComparison.Ordinal));

        TestContext.WriteLine($"Received: {line}");
        if (line is not null && line.Contains("Access", StringComparison.Ordinal))
        {
            Assert.Inconclusive($"{portName} is held by another program.");
        }

        Assert.IsNotNull(line, $"Expected a decoded reply from {portName}.");
        Assert.Contains(expectedHexPrefix, line);

        process.StandardInput.Close();
        await process.WaitForExitAsync(TestContext.CancellationToken).WaitAsync(_timeout, TestContext.CancellationToken).ConfigureAwait(false);
        Assert.AreEqual(0, process.ExitCode);
    }
}
