using OoBDev.System.Sandbox.Linux;
using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.System.Sandbox.Tests;

/// <summary>Resource limits of the Linux launcher (REQ-009 on Linux): memory, CPU time and wall time end a runaway plugin.</summary>
[TestCategory("Integration")]
[TestClass]
public sealed class LinuxLimitTests
{
    [TestInitialize]
    public void RequireLinux()
    {
        if (!OperatingSystem.IsLinux() || !File.Exists("/usr/bin/bwrap"))
        {
            Assert.Inconclusive("Needs Linux with bubblewrap (/usr/bin/bwrap).");
        }
    }

    [TestMethod]
    public void Hog_MemoryOverTheLimit_PluginEndsWithAFailure()
    {
        using var plugin = new BubblewrapPlugin(TestPlugin.Dll, "hog", limits: new JobLimits(MemoryMb: 128));

        Assert.IsTrue(plugin.WaitForExit(30_000));
        Assert.AreNotEqual(0, plugin.ExitCode);
        Assert.AreEqual(LimitHit.Memory, plugin.LimitHit);
    }

    [TestMethod]
    public void Spin_CpuOverTheLimit_PluginIsKilled()
    {
        using var plugin = new BubblewrapPlugin(TestPlugin.Dll, "spin", limits: new JobLimits(CpuSeconds: 2));

        Assert.IsTrue(plugin.WaitForExit(30_000));
        Assert.AreNotEqual(0, plugin.ExitCode);
        Assert.AreEqual(LimitHit.Cpu, plugin.LimitHit);
    }

    [TestMethod]
    public void Sleep_WallTimeOverTheLimit_PluginIsKilled()
    {
        using var plugin = new BubblewrapPlugin(TestPlugin.Dll, "sleep", limits: new JobLimits(WallSeconds: 2));

        Assert.IsTrue(plugin.WaitForExit(30_000));
        Assert.AreEqual(LimitHit.Wall, plugin.LimitHit);
    }

    [TestMethod]
    public void Fork_MoreProcessesThanTheLimit_TheTreeIsKilledAndReported()
    {
        using var plugin = new BubblewrapPlugin(TestPlugin.Dll, "fork", limits: new JobLimits(MaxProcesses: 4));

        Assert.IsTrue(plugin.WaitForExit(30_000));
        Assert.AreEqual(LimitHit.Processes, plugin.LimitHit);
    }

    [TestMethod]
    public void Sleep_WithinEveryLimit_NoLimitIsReported()
    {
        using var plugin = new BubblewrapPlugin(TestPlugin.Dll, "echo", limits: new JobLimits(MemoryMb: 256, MaxProcesses: 8, CpuSeconds: 30, WallSeconds: 60));
        plugin.Receive();

        plugin.Send(FrameType.Shutdown, "lifecycle.shutdown", "{}");

        Assert.IsTrue(plugin.WaitForExit(10_000));
        Assert.AreEqual(LimitHit.None, plugin.LimitHit);
    }
}
