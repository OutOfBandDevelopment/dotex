using OoBDev.System.Sandbox.Windows;
using System;
using System.Runtime.Versioning;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: SupportedOSPlatform("windows")]

namespace OoBDev.System.Sandbox.Tests;

/// <summary>Proof of concept for phase 2 of the plugin sandbox: framing, violation reactions and job limits on Windows. They start real child processes, so they are Integration-tier.</summary>
[TestCategory("Integration")]
[TestClass]
public sealed class PluginSandboxTests
{
    [TestInitialize]
    public void RequireWindows()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Windows launcher (AppContainer and job object); see CrossPlatformEscapeTests for Linux and macOS.");
        }
    }

    [TestMethod]
    public void Echo_ReadyThenRequest_ReturnsCounterSignedResponse()
    {
        using var plugin = new PluginProcess(TestPlugin.Dll, "echo", 256);

        var ready = plugin.Receive();
        var requestSequence = plugin.Send(FrameType.Request, "echo", """{"n":7}""");
        var response = plugin.Receive();
        plugin.Send(FrameType.Shutdown, "lifecycle.shutdown", """{}""");

        Assert.AreEqual("lifecycle.ready", ready.Topic);
        Assert.AreEqual(requestSequence, response.Ack);
        Assert.AreEqual(7, response.ParseJson().RootElement.GetProperty("n").GetInt32());
        Assert.IsTrue(plugin.WaitForExit(10_000));
        Assert.AreEqual(0, plugin.ExitCode);
    }

    [TestMethod]
    public void Oversize_FrameLengthOverTheCap_HostKillsThePlugin()
    {
        using var plugin = new PluginProcess(TestPlugin.Dll, "oversize", 256);

        Assert.ThrowsExactly<FrameViolation>(() => plugin.Receive());

        Assert.IsTrue(plugin.HasExited);
    }

    [TestMethod]
    public void Hog_MemoryOverTheJobLimit_TheJobKillsIt()
    {
        using var plugin = new PluginProcess(TestPlugin.Dll, "hog", 128);

        Assert.IsTrue(plugin.WaitForExit(30_000), "The job's per-process memory limit should end the plugin.");
        Assert.AreNotEqual(0, plugin.ExitCode);
    }
}
