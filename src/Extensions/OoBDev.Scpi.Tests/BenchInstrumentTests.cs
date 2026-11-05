using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using OoBDev.TestUtilities;

namespace OoBDev.Scpi.Tests;

/// <summary>Identify queries against the network bench instruments (see docs/how-tos/test-hardware.md for the settings and quirks).</summary>
[TestCategory("Hardware")]
[TestClass]
[TestCategory(TestCategories.Scpi)]
public sealed class BenchInstrumentTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task Identify_Hp34401aThroughBridge_ReturnsTheModel()
    {
        // Stage
        using var instrument = await BenchTcpInstrument.RequireAsync(TestContext, "HP34401A_TCP_HOST", TimeSpan.FromMilliseconds(50));

        // Test
        var reply = await instrument.QueryAsync("*IDN?", "\n", TimeSpan.FromSeconds(2));

        // Assert
        StringAssert.Contains(reply, "HEWLETT-PACKARD,34401A");
    }

    [TestMethod]
    [DataRow("TEK2230_1_TCP_HOST")]
    [DataRow("TEK2230_2_TCP_HOST")]
    public async Task Identify_Tektronix2230ThroughBridge_ReturnsTheModel(string hostParameter)
    {
        // Stage
        using var instrument = await BenchTcpInstrument.RequireAsync(TestContext, hostParameter, TimeSpan.FromMilliseconds(175));

        // Test
        var reply = await instrument.QueryAsync("ID?", "\r", TimeSpan.FromSeconds(2));

        // Assert
        StringAssert.Contains(reply, "TEK/2230");
    }

    [TestMethod]
    public async Task Identify_Tds2024ThroughBridge_ReturnsTheModel()
    {
        // Stage
        using var instrument = await BenchTcpInstrument.RequireAsync(TestContext, "TDS2024_TCP_HOST", TimeSpan.FromMilliseconds(50));

        // Test
        var reply = await instrument.QueryAsync("*IDN?", "\n", TimeSpan.FromSeconds(2));

        // Assert
        StringAssert.Contains(reply, "TEKTRONIX,TDS 2024");
    }
}
