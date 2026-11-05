using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Threading.Tasks;
using OoBDev.TestUtilities;

namespace OoBDev.Scpi.Tests;

/// <summary>Read-only queries against the Korad KA3005P and KA6003P supplies; nothing is set and the output is never switched on (see docs/how-tos/test-hardware.md).</summary>
[TestCategory("Hardware")]
[TestClass]
[TestCategory(TestCategories.Scpi)]
public sealed class KoradSupplyTests
{
    private static readonly TimeSpan _quiet = TimeSpan.FromMilliseconds(250);

    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [DataRow("KORAD_KA3005P_SERIAL_PORT", "KA3005P")]
    [DataRow("KORAD_KA6003P_SERIAL_PORT", "KA6003P")]
    public async Task Identify_KoradSupply_ReturnsTheModel(string portParameter, string model)
    {
        // Stage
        using var supply = BenchSerialInstrument.Require(TestContext, portParameter);

        // Test
        var reply = await supply.QueryAsync("*IDN?", _quiet);

        // Assert
        StringAssert.Contains(reply, "KORAD " + model);
    }

    [TestMethod]
    [DataRow("KORAD_KA3005P_SERIAL_PORT", "VSET1?", 30d)]
    [DataRow("KORAD_KA3005P_SERIAL_PORT", "ISET1?", 5d)]
    [DataRow("KORAD_KA6003P_SERIAL_PORT", "VSET1?", 60d)]
    [DataRow("KORAD_KA6003P_SERIAL_PORT", "ISET1?", 3d)]
    public async Task QuerySetpoint_KoradSupply_ReturnsANumberWithinTheRange(string portParameter, string query, double maximum)
    {
        // Stage
        using var supply = BenchSerialInstrument.Require(TestContext, portParameter);

        // Test
        var reply = await supply.QueryAsync(query, _quiet);

        // Assert
        Assert.IsTrue(double.TryParse(reply.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value), $"'{reply}' is not a number.");
        Assert.IsTrue(value >= 0 && value <= maximum, $"{value} is outside 0 to {maximum}.");
    }

    [TestMethod]
    [DataRow("KORAD_KA3005P_SERIAL_PORT")]
    [DataRow("KORAD_KA6003P_SERIAL_PORT")]
    public async Task QueryStatus_KoradSupply_ReturnsOneByte(string portParameter)
    {
        // Stage
        using var supply = BenchSerialInstrument.Require(TestContext, portParameter);

        // Test
        var reply = await supply.QueryAsync("STATUS?", _quiet);

        // Assert
        Assert.AreEqual(1, reply.Length, "STATUS? answers one raw status byte.");
    }
}
