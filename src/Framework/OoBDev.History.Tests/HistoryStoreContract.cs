using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.History.Tests;

/// <summary>Behaviour every <see cref="IHistoryStore"/> provider must show.</summary>
public abstract class HistoryStoreContract
{
    protected static readonly DateTimeOffset T0 = new(2026, 11, 23, 10, 0, 0, TimeSpan.Zero);

    protected abstract Task<IHistoryStore> CreateAsync(HistoryOptions options);

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Raw_ReadsRangeInclusiveFromExclusiveTo_OldestFirst()
    {
        var store = await CreateAsync(new HistoryOptions());
        await store.AppendAsync("a", new HistoryPoint(T0.AddSeconds(2), 2));
        await store.AppendAsync("a", new HistoryPoint(T0, 0));
        await store.AppendAsync("a", new HistoryPoint(T0.AddSeconds(1), 1));
        await store.AppendAsync("b", new HistoryPoint(T0, 99));

        var points = await store.ReadRawAsync("a", T0, T0.AddSeconds(2));

        CollectionAssert.AreEqual(new[] { 0d, 1d }, points.Select(p => p.Value).ToArray());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Buckets_AggregateCountMinMaxSumLast()
    {
        var store = await CreateAsync(new HistoryOptions());
        await store.AppendAsync("a", new HistoryPoint(T0.AddSeconds(10), 5));
        await store.AppendAsync("a", new HistoryPoint(T0.AddSeconds(20), 1));
        await store.AppendAsync("a", new HistoryPoint(T0.AddSeconds(30), 9));
        await store.AppendAsync("a", new HistoryPoint(T0.AddSeconds(70), 4));

        var buckets = await store.ReadBucketsAsync("a", "1m", T0, T0.AddMinutes(5));

        Assert.AreEqual(2, buckets.Count);
        Assert.AreEqual(T0, buckets[0].Start);
        Assert.AreEqual(3, buckets[0].Count);
        Assert.AreEqual(1d, buckets[0].Minimum);
        Assert.AreEqual(9d, buckets[0].Maximum);
        Assert.AreEqual(15d, buckets[0].Sum);
        Assert.AreEqual(9d, buckets[0].Last);
        Assert.AreEqual(5d, buckets[0].Average);
        Assert.AreEqual(T0.AddMinutes(1), buckets[1].Start);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Buckets_LatePoint_UpdatesStatsButNotLast()
    {
        var store = await CreateAsync(new HistoryOptions());
        await store.AppendAsync("a", new HistoryPoint(T0.AddSeconds(30), 9));
        await store.AppendAsync("a", new HistoryPoint(T0.AddSeconds(5), 1));

        var bucket = (await store.ReadBucketsAsync("a", "1m", T0, T0.AddMinutes(1))).Single();

        Assert.AreEqual(2, bucket.Count);
        Assert.AreEqual(1d, bucket.Minimum);
        Assert.AreEqual(9d, bucket.Last);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Buckets_AlignToEpochInUtc_AndIgnoreOffset()
    {
        var store = await CreateAsync(new HistoryOptions());
        var local = new DateTimeOffset(2026, 11, 23, 5, 59, 30, TimeSpan.FromHours(-5));
        await store.AppendAsync("a", new HistoryPoint(local, 1));

        var bucket = (await store.ReadBucketsAsync("a", "1h", T0.AddHours(-1), T0.AddHours(2))).Single();

        Assert.AreEqual(new DateTimeOffset(2026, 11, 23, 10, 0, 0, TimeSpan.Zero), bucket.Start);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Buckets_UnknownTier_Throws()
    {
        var store = await CreateAsync(new HistoryOptions());

        await Assert.ThrowsExactlyAsync<ArgumentException>(() => store.ReadBucketsAsync("a", "nope", T0, T0.AddDays(1)));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Append_BlankSeries_Throws()
    {
        var store = await CreateAsync(new HistoryOptions());

        await Assert.ThrowsExactlyAsync<ArgumentException>(() => store.AppendAsync(" ", new HistoryPoint(T0, 1)));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Prune_RemovesRawAndBucketsPastRetention_KeepsNewer()
    {
        var options = new HistoryOptions { RawRetention = TimeSpan.FromHours(1) };
        var store = await CreateAsync(options);
        await store.AppendAsync("a", new HistoryPoint(T0, 1));
        await store.AppendAsync("a", new HistoryPoint(T0.AddHours(3), 2));

        var removed = await store.PruneAsync(T0.AddHours(3).AddMinutes(30));

        Assert.AreEqual(1, removed);
        var raw = await store.ReadRawAsync("a", T0.AddDays(-1), T0.AddDays(1));
        Assert.AreEqual(2d, raw.Single().Value);
        Assert.AreEqual(2, (await store.ReadBucketsAsync("a", "1m", T0.AddDays(-1), T0.AddDays(1))).Count);

        removed = await store.PruneAsync(T0.AddDays(8));
        Assert.IsTrue(removed > 0);
        Assert.AreEqual(0, (await store.ReadBucketsAsync("a", "1m", T0.AddDays(-1), T0.AddDays(1))).Count);
        Assert.AreEqual(2, (await store.ReadBucketsAsync("a", "1h", T0.AddDays(-1), T0.AddDays(1))).Count);
    }
}
