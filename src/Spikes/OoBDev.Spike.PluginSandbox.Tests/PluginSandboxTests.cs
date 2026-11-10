using System.Runtime.Versioning;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: SupportedOSPlatform("windows")]

namespace OoBDev.Spike.PluginSandbox.Tests;

/// <summary>Proof of concept for phase 2 of the plugin sandbox: framing, violation reactions and job limits on Windows. They start real child processes, so they are Integration-tier.</summary>
[TestCategory("Integration")]
[TestClass]
public sealed class PluginSandboxTests
{
    [TestMethod]
    public void Echo_ReadyThenRequest_ReturnsResponseWithCorrelationId()
    {
        using var plugin = new PluginProcess("echo", 256);

        using var ready = plugin.Receive();
        plugin.Send("""{"v":1,"type":"Request","requestId":"r1","topic":"echo","payload":{"n":7}}""");
        using var response = plugin.Receive();
        plugin.Send("""{"v":1,"type":"Shutdown","requestId":"r2"}""");

        Assert.AreEqual("lifecycle.ready", ready.RootElement.GetProperty("topic").GetString());
        Assert.AreEqual("r1", response.RootElement.GetProperty("correlationId").GetString());
        Assert.AreEqual(7, response.RootElement.GetProperty("payload").GetProperty("n").GetInt32());
        Assert.IsTrue(plugin.WaitForExit(10_000));
        Assert.AreEqual(0, plugin.ExitCode);
    }

    [TestMethod]
    public void Oversize_FrameLengthOverTheCap_HostKillsThePlugin()
    {
        using var plugin = new PluginProcess("oversize", 256);

        Assert.ThrowsExactly<FrameViolation>(() => plugin.Receive());

        Assert.IsTrue(plugin.HasExited);
    }

    [TestMethod]
    public void Hog_MemoryOverTheJobLimit_TheJobKillsIt()
    {
        using var plugin = new PluginProcess("hog", 128);

        Assert.IsTrue(plugin.WaitForExit(30_000), "The job's per-process memory limit should end the plugin.");
        Assert.AreNotEqual(0, plugin.ExitCode);
    }
}
