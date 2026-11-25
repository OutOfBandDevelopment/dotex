using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Time;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.System.Tests.Time;

[TestClass]
public class HybridLogicalClockGeneratorTests
{
    private sealed class SettableTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;

        public override DateTimeOffset GetUtcNow() => Now;
    }

    private static readonly DateTimeOffset Start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Next_SameTick_IncrementsCounter()
    {
        var generator = new HybridLogicalClockGenerator(new FakeTimeProvider(Start));
        var first = generator.Next();
        var second = generator.Next();
        Assert.AreEqual(Start.UtcTicks, first.PhysicalTicks);
        Assert.AreEqual(0, first.LogicalCounter);
        Assert.AreEqual(1, second.LogicalCounter);
        Assert.IsTrue(second > first);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Next_TimeAdvances_ResetsCounter()
    {
        var time = new FakeTimeProvider(Start);
        var generator = new HybridLogicalClockGenerator(time);
        generator.Next();
        generator.Next();
        time.Advance(TimeSpan.FromSeconds(1));
        var next = generator.Next();
        Assert.AreEqual(0, next.LogicalCounter);
        Assert.AreEqual(time.GetUtcNow().UtcTicks, next.PhysicalTicks);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Next_ClockMovesBackwards_StaysMonotonic()
    {
        var time = new SettableTimeProvider(Start);
        var generator = new HybridLogicalClockGenerator(time);
        var first = generator.Next();
        time.Now = Start.AddMinutes(-5);
        var second = generator.Next();
        Assert.IsTrue(second > first);
        Assert.AreEqual(first.PhysicalTicks, second.PhysicalTicks);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Merge_EarlierReceived_StillAdvances()
    {
        var generator = new HybridLogicalClockGenerator(new FakeTimeProvider(Start));
        var local = generator.Next();
        var merged = generator.Merge(new HybridLogicalClock(local.PhysicalTicks - 10, 7));
        Assert.IsTrue(merged > local);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Merge_LaterReceived_AdvancesPastIt()
    {
        var generator = new HybridLogicalClockGenerator(new FakeTimeProvider(Start));
        var received = new HybridLogicalClock(Start.AddMinutes(5).UtcTicks, 3);
        var merged = generator.Merge(received);
        Assert.IsTrue(merged > received);
        Assert.AreEqual(received.PhysicalTicks, merged.PhysicalTicks);
        Assert.AreEqual(4, merged.LogicalCounter);
        Assert.IsTrue(generator.Next() > merged);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Merge_EqualPhysical_TakesLargerCounterPlusOne()
    {
        var generator = new HybridLogicalClockGenerator(new FakeTimeProvider(Start));
        generator.Next();
        generator.Next();
        var merged = generator.Merge(new HybridLogicalClock(Start.UtcTicks, 5));
        Assert.AreEqual(6, merged.LogicalCounter);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Merge_LocalTimeLeads_ResetsCounter()
    {
        var time = new FakeTimeProvider(Start);
        var generator = new HybridLogicalClockGenerator(time);
        generator.Next();
        time.Advance(TimeSpan.FromSeconds(10));
        var merged = generator.Merge(new HybridLogicalClock(Start.UtcTicks, 9));
        Assert.AreEqual(0, merged.LogicalCounter);
        Assert.AreEqual(time.GetUtcNow().UtcTicks, merged.PhysicalTicks);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Merge_BeyondMaxDrift_Throws()
    {
        var generator = new HybridLogicalClockGenerator(new FakeTimeProvider(Start), TimeSpan.FromMinutes(1));
        var far = new HybridLogicalClock(Start.AddMinutes(2).UtcTicks, 0);
        Assert.ThrowsExactly<ArgumentException>(() => generator.Merge(far));
        var near = new HybridLogicalClock(Start.AddSeconds(30).UtcTicks, 0);
        Assert.IsTrue(generator.Merge(near) > near);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Next_CounterOverflow_Throws()
    {
        var generator = new HybridLogicalClockGenerator(new FakeTimeProvider(Start));
        generator.Merge(new HybridLogicalClock(Start.UtcTicks, int.MaxValue - 1));
        Assert.ThrowsExactly<OverflowException>(() => generator.Next());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void CompareTo_OrdersByPhysicalThenCounter()
    {
        var a = new HybridLogicalClock(10, 5);
        var b = new HybridLogicalClock(11, 0);
        var c = new HybridLogicalClock(10, 6);
        Assert.IsTrue(a < b);
        Assert.IsTrue(a < c);
        Assert.IsTrue(b > c);
        Assert.IsTrue(a <= a);
        Assert.IsTrue(a >= a);
        Assert.AreEqual(0, a.CompareTo(new HybridLogicalClock(10, 5)));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Next_Concurrent_ProducesUniqueValues()
    {
        var generator = new HybridLogicalClockGenerator(new FakeTimeProvider(Start));
        var values = new HybridLogicalClock[400];
        Parallel.For(0, values.Length, i => values[i] = generator.Next());
        Assert.AreEqual(values.Length, new HashSet<HybridLogicalClock>(values).Count);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Constructor_NoArguments_UsesSystemTime()
    {
        var generator = new HybridLogicalClockGenerator();
        Assert.IsTrue(generator.Next() < generator.Next());
    }
}
