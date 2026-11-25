using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AspNetCore.Authorization;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.EntityFrameworkCore.Tests;

[TestClass]
public sealed class RightAuditTests
{
    public required TestContext TestContext { get; set; }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;

        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection keeper;

        public Fixture(RightAuditOptions? options = null)
        {
            var name = $"Data Source=audit{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
            keeper = new SqliteConnection(name);
            keeper.Open();
            var factory = new TestFactory(new DbContextOptionsBuilder<RightAuditDbContext>().UseSqlite(name).Options);
            using (var db = factory.CreateDbContext())
            {
                db.Database.EnsureCreated();
            }

            Options = Microsoft.Extensions.Options.Options.Create(options ?? new RightAuditOptions());
            Store = new EfRightAuditStore(factory);
            Sink = new BufferedRightAuditSink(Options);
            Clock = new FixedClock(new DateTimeOffset(2026, 11, 16, 12, 0, 0, TimeSpan.Zero));
            Writer = new RightAuditWriter(Sink, Store, Options, Clock, NullLogger<RightAuditWriter>.Instance);
        }

        public IOptions<RightAuditOptions> Options { get; }

        public EfRightAuditStore Store { get; }

        public BufferedRightAuditSink Sink { get; }

        public FixedClock Clock { get; }

        public RightAuditWriter Writer { get; }

        public async ValueTask DisposeAsync()
        {
            Writer.Dispose();
            await keeper.DisposeAsync();
        }
    }

    private sealed class TestFactory(DbContextOptions<RightAuditDbContext> options) : IDbContextFactory<RightAuditDbContext>
    {
        public RightAuditDbContext CreateDbContext() => new(options);
    }

    private static RightAuditEntry Entry(string subject, DateTimeOffset at, string result = "Allowed") => new(at, subject, "orders.read", result);

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Flush_WritesBufferedEntries_AndQueryReturnsNewestFirst()
    {
        await using var f = new Fixture();
        var t = f.Clock.Now;
        f.Sink.Record(Entry("u-1", t.AddMinutes(-2)));
        f.Sink.Record(Entry("u-2", t.AddMinutes(-1), "Forbidden"));

        var written = await f.Writer.FlushAsync();
        var all = await f.Store.QueryAsync();

        Assert.AreEqual(2, written);
        Assert.AreEqual("u-2", all[0].Subject);
        Assert.AreEqual("Forbidden", all[0].Result);
        Assert.AreEqual(t.AddMinutes(-2), all[1].At);
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Query_FiltersBySubjectAndFrom()
    {
        await using var f = new Fixture();
        var t = f.Clock.Now;
        await f.Store.AppendAsync([Entry("u-1", t.AddDays(-3)), Entry("u-1", t.AddDays(-1)), Entry("u-2", t.AddDays(-1))]);

        var mine = await f.Store.QueryAsync("u-1", t.AddDays(-2));

        Assert.HasCount(1, mine);
        Assert.AreEqual(t.AddDays(-1), mine[0].At);
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Query_ByFilter_AppliesResultAndUpperBoundThroughTheAbstraction()
    {
        await using var f = new Fixture();
        var t = f.Clock.Now;
        await f.Store.AppendAsync([Entry("u-1", t.AddDays(-3), "Forbidden"), Entry("u-1", t.AddDays(-2)), Entry("u-2", t.AddDays(-1), "Forbidden")]);
        IRightAuditQuery query = f.Store;

        var denied = await query.QueryAsync(new RightAuditFilter(Result: "Forbidden"));
        var before = await query.QueryAsync(new RightAuditFilter(To: t.AddDays(-2)));
        var one = await query.QueryAsync(new RightAuditFilter(MaxCount: 1));

        TestContext.WriteLine("denied {0}, before {1}, one {2}", denied.Count, before.Count, one.Count);
        Assert.HasCount(2, denied);
        Assert.HasCount(1, before);
        Assert.AreEqual(t.AddDays(-3), before[0].At);
        Assert.AreEqual("u-2", one[0].Subject);
        await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(() => query.QueryAsync(new RightAuditFilter(MaxCount: 0)));
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Flush_PurgesEntriesPastRetention_OncePerDay()
    {
        await using var f = new Fixture(new RightAuditOptions { RetentionDays = 30 });
        var t = f.Clock.Now;
        await f.Store.AppendAsync([Entry("old", t.AddDays(-31)), Entry("new", t.AddDays(-29))]);

        await f.Writer.FlushAsync();
        var kept = await f.Store.QueryAsync();

        Assert.HasCount(1, kept);
        Assert.AreEqual("new", kept[0].Subject);
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Flush_WritesInBatches()
    {
        await using var f = new Fixture(new RightAuditOptions { BatchSize = 2 });
        for (var i = 0; i < 5; i++)
        {
            f.Sink.Record(Entry("u", f.Clock.Now.AddSeconds(i)));
        }

        var written = await f.Writer.FlushAsync();

        Assert.AreEqual(5, written);
        Assert.HasCount(5, await f.Store.QueryAsync());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Record_WhenBufferFull_DropsAndCounts()
    {
        await using var f = new Fixture(new RightAuditOptions { QueueCapacity = 2 });

        for (var i = 0; i < 5; i++)
        {
            f.Sink.Record(Entry("u", f.Clock.Now));
        }

        Assert.AreEqual(3, f.Sink.Dropped);
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Append_ClipsOverlongValues()
    {
        await using var f = new Fixture();
        await f.Store.AppendAsync([new RightAuditEntry(f.Clock.Now, new string('s', 500), new string('r', 5000), "Allowed")]);

        var row = (await f.Store.QueryAsync()).Single();

        Assert.AreEqual(200, row.Subject.Length);
        Assert.AreEqual(1000, row.Rights.Length);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Checker_WithSink_RecordsEachCheck()
    {
        await using var f = new Fixture();
        var checker = new AuditingApplicationRightChecker(new ApplicationRightChecker(), NullLogger<AuditingApplicationRightChecker>.Instance, f.Sink, f.Clock);

        checker.Check(new ClaimsPrincipal(new ClaimsIdentity()), ["orders.read"]);
        await f.Writer.FlushAsync();

        var entry = (await f.Store.QueryAsync()).Single();
        Assert.AreEqual("anonymous", entry.Subject);
        Assert.AreEqual("orders.read", entry.Rights);
        Assert.AreNotEqual("Allowed", entry.Result);
    }

    private static ServiceProvider Build(string batchSize)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["OoBDev:RightAudit:BatchSize"] = batchSize }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.TryAddEntityFrameworkRightAudit(config, (_, o) => o.UseSqlite("Data Source=:memory:"));
        return services.BuildServiceProvider();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Registration_ResolvesSinkAndWriter()
    {
        await using var provider = Build("10");

        Assert.IsInstanceOfType<BufferedRightAuditSink>(provider.GetRequiredService<IRightAuditSink>());
        Assert.AreEqual(10, provider.GetRequiredService<IOptions<RightAuditOptions>>().Value.BatchSize);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Registration_BadOptions_FailValidation()
    {
        await using var provider = Build("0");

        Assert.ThrowsExactly<OptionsValidationException>(() => provider.GetRequiredService<IOptions<RightAuditOptions>>().Value);
    }
}
