using OoBDev.System.Providers;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace OoBDev.System.Tests.Providers;

[TestClass]
public class DateTimeProviderTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void NowTest()
    {
        var start = DateTimeOffset.Now;
        Thread.Sleep(50);
        var provider = new DateTimeProvider();
        var now = provider.Now;
        Assert.IsTrue(now >= start);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void UtcNowTest()
    {
        var start = DateTimeOffset.UtcNow;
        Thread.Sleep(50);
        var provider = new DateTimeProvider();
        var now = provider.UtcNow;
        Assert.IsTrue(now >= start);
    }
}
