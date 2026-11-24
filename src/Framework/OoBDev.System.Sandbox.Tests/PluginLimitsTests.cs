using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.System.Sandbox.Tests;

[TestClass]
public sealed class PluginLimitsTests
{
    [TestCategory("Unit")]
    [DataTestMethod]
    [DataRow(null, 200)]
    [DataRow(0, 200)]
    [DataRow(50, 50)]
    [DataRow(5000, 1000)]
    public void EffectiveMsgPerSec_Request_IsDefaultedAndClamped(int? asked, int expected)
    {
        // Stage
        var limits = new PluginLimits(asked);

        // Assert
        Assert.AreEqual(expected, limits.EffectiveMsgPerSec);
    }

    [TestCategory("Unit")]
    [TestMethod]
    public void ToJobLimits_AskedAndUnasked_AreClampedAndDefaulted()
    {
        // Stage
        var asked = new PluginLimits(MemoryMb: 4096, MaxProcesses: 500, CpuSeconds: 10, WallSeconds: 99999);

        // Test
        var clamped = asked.ToJobLimits();
        var defaults = new PluginLimits().ToJobLimits();

        // Assert
        Assert.AreEqual(new JobLimits(1024, 64, 10, 3600), clamped);
        Assert.AreEqual(new JobLimits(256, 0, 0, 0), defaults);
    }
}
