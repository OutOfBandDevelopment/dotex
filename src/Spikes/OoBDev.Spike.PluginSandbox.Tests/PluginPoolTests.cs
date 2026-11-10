using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Spike.PluginSandbox.Tests;

/// <summary>REQ-011: bound and shared plugin lifetimes.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class PluginPoolTests
{
    private sealed class FixedTime(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;

        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class Fake : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose() => Disposed = true;
    }

    private static readonly DateTimeOffset _start = new(2026, 11, 10, 12, 0, 0, TimeSpan.Zero);

    [TestMethod]
    public void Bound_EachLeaseGetsItsOwnInstanceAndDisposeEndsIt()
    {
        using var pool = new PluginPool<Fake>(new FixedTime(_start), TimeSpan.FromSeconds(30));

        var first = pool.Acquire("p", "g", PluginLifetime.Bound, () => new Fake());
        var second = pool.Acquire("p", "g", PluginLifetime.Bound, () => new Fake());

        Assert.AreNotSame(first.Plugin, second.Plugin);
        first.Dispose();
        Assert.IsTrue(first.Plugin.Disposed);
        Assert.IsFalse(second.Plugin.Disposed);
        Assert.AreEqual(0, pool.SharedCount);
    }

    [TestMethod]
    public void Shared_SameKey_ReusesOneInstanceAndGrantsSeparateThem()
    {
        using var pool = new PluginPool<Fake>(new FixedTime(_start), TimeSpan.FromSeconds(30));
        var creates = 0;
        Fake Make()
        {
            creates++;
            return new Fake();
        }

        using var a = pool.Acquire("p", "g1", PluginLifetime.Shared, Make);
        using var b = pool.Acquire("p", "g1", PluginLifetime.Shared, Make);
        using var c = pool.Acquire("p", "g2", PluginLifetime.Shared, Make);

        Assert.AreSame(a.Plugin, b.Plugin);
        Assert.AreNotSame(a.Plugin, c.Plugin);
        Assert.AreEqual(2, creates);
        Assert.AreEqual(2, pool.SharedCount);
    }

    [TestMethod]
    public void Shared_IdleAfterLastLease_EndsOnlyAfterTheTimeout()
    {
        var time = new FixedTime(_start);
        using var pool = new PluginPool<Fake>(time, TimeSpan.FromSeconds(30));
        var lease = pool.Acquire("p", "g", PluginLifetime.Shared, () => new Fake());
        var plugin = lease.Plugin;

        Assert.AreEqual(0, pool.Sweep(), "A leased instance is never swept.");
        lease.Dispose();
        time.Now = _start.AddSeconds(29);
        Assert.AreEqual(0, pool.Sweep());
        time.Now = _start.AddSeconds(30);
        Assert.AreEqual(1, pool.Sweep());
        Assert.IsTrue(plugin.Disposed);
        Assert.AreEqual(0, pool.SharedCount);
    }

    [TestMethod]
    public void Shared_LeaseAfterSweep_StartsAFreshInstance()
    {
        var time = new FixedTime(_start);
        using var pool = new PluginPool<Fake>(time, TimeSpan.FromSeconds(1));
        var first = pool.Acquire("p", "g", PluginLifetime.Shared, () => new Fake());
        first.Dispose();
        time.Now = _start.AddSeconds(2);
        pool.Sweep();

        using var second = pool.Acquire("p", "g", PluginLifetime.Shared, () => new Fake());

        Assert.AreNotSame(first.Plugin, second.Plugin);
    }

    [TestMethod]
    public void Lease_DisposedTwice_ReleasesOnce()
    {
        using var pool = new PluginPool<Fake>(new FixedTime(_start), TimeSpan.Zero);
        using var keep = pool.Acquire("p", "g", PluginLifetime.Shared, () => new Fake());
        var extra = pool.Acquire("p", "g", PluginLifetime.Shared, () => new Fake());

        extra.Dispose();
        extra.Dispose();

        Assert.AreEqual(0, pool.Sweep(), "The other lease still holds the instance.");
    }

    [TestMethod]
    public void Dispose_EndsSharedInstances()
    {
        var pool = new PluginPool<Fake>(new FixedTime(_start), TimeSpan.FromSeconds(30));
        var lease = pool.Acquire("p", "g", PluginLifetime.Shared, () => new Fake());

        pool.Dispose();

        Assert.IsTrue(lease.Plugin.Disposed);
    }
}
