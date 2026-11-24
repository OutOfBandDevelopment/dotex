using System;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.System.Sandbox.Tests;

[TestClass]
public sealed class MessageRateLimiterTests
{
    public required TestContext TestContext { get; set; }

    [TestCategory("Unit")]
    [TestMethod]
    public void Allow_OverTheLimit_DropsAndCounts()
    {
        // Stage
        var limiter = new MessageRateLimiter(2, 3, new FakeTimeProvider());

        // Test
        var results = new[] { limiter.Allow(), limiter.Allow(), limiter.Allow(), limiter.Allow() };

        // Assert
        TestContext.WriteLine($"Results {string.Join(",", results)}, dropped {limiter.Dropped}");
        CollectionAssert.AreEqual(new[] { true, true, false, false }, results);
        Assert.AreEqual(2, limiter.Dropped);
        Assert.IsFalse(limiter.Sustained);
    }

    [TestCategory("Unit")]
    [TestMethod]
    public void Allow_OverInConsecutiveWindows_IsSustained()
    {
        // Stage
        var time = new FakeTimeProvider();
        var limiter = new MessageRateLimiter(1, 2, time);

        // Test
        for (var window = 0; window < 3; window++)
        {
            limiter.Allow();
            limiter.Allow();
            time.Advance(TimeSpan.FromMilliseconds(1100));
        }

        limiter.Allow();

        // Assert
        TestContext.WriteLine($"Dropped {limiter.Dropped}, sustained {limiter.Sustained}");
        Assert.IsTrue(limiter.Sustained);
    }

    [TestCategory("Unit")]
    [TestMethod]
    public void Allow_QuietWindowBetween_ResetsTheStreak()
    {
        // Stage
        var time = new FakeTimeProvider();
        var limiter = new MessageRateLimiter(1, 2, time);

        // Test
        limiter.Allow();
        limiter.Allow();
        time.Advance(TimeSpan.FromSeconds(5));
        limiter.Allow();
        limiter.Allow();
        time.Advance(TimeSpan.FromMilliseconds(1100));
        limiter.Allow();

        // Assert
        Assert.IsFalse(limiter.Sustained);
    }
}
