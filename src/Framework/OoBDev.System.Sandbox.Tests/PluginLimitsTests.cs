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
}
