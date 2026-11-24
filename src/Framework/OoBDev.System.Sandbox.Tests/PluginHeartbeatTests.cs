using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.System.Sandbox.Tests;

[TestClass]
public sealed class PluginHeartbeatTests
{
    public required TestContext TestContext { get; set; }

    [TestCategory("Unit")]
    [TestMethod]
    public void Check_PluginAnswers_ReturnsTrue()
    {
        // Stage
        using var plugin = new PlainPlugin(TestPlugin.Dll, "echo");
        plugin.Receive();

        // Test
        var alive = PluginHeartbeat.Check(plugin, TimeSpan.FromSeconds(20));

        // Assert
        TestContext.WriteLine($"Answered {alive}");
        Assert.IsTrue(alive);
    }

    [TestCategory("Unit")]
    [TestMethod]
    public void Check_PluginIgnoresIt_ReturnsFalseAndStopsThePlugin()
    {
        // Stage
        using var plugin = new PlainPlugin(TestPlugin.Dll, "sleep");

        // Test
        var alive = PluginHeartbeat.Check(plugin, TimeSpan.FromSeconds(2));

        // Assert
        TestContext.WriteLine($"Answered {alive}");
        Assert.IsFalse(alive);
    }
}
