using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Spike.PluginSandbox.Tests;

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
        using var plugin = new BubblewrapPlugin("hog", limits: new JobLimits(MemoryMb: 128));

        Assert.IsTrue(plugin.WaitForExit(30_000));
        Assert.AreNotEqual(0, plugin.ExitCode);
    }

    [TestMethod]
    public void Spin_CpuOverTheLimit_PluginIsKilled()
    {
        using var plugin = new BubblewrapPlugin("spin", limits: new JobLimits(CpuSeconds: 2));

        Assert.IsTrue(plugin.WaitForExit(30_000));
        Assert.AreNotEqual(0, plugin.ExitCode);
    }

    [TestMethod]
    public void Sleep_WallTimeOverTheLimit_PluginIsKilled()
    {
        using var plugin = new BubblewrapPlugin("sleep", limits: new JobLimits(WallSeconds: 2));

        Assert.IsTrue(plugin.WaitForExit(30_000));
    }
}
