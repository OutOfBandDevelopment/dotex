using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.MessageQueueing.Services;
using OoBDev.TestUtilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.EntityFrameworkCore.Tests;

[TestClass]
public class EfMessagingStoreTests
{
    public sealed record Ping(string Text);

    private SqliteMessagingDatabase database = null!;

    [TestInitialize]
    public void Initialize() => database = new SqliteMessagingDatabase();

    [TestCleanup]
    public async Task Cleanup() => await database.DisposeAsync();

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Inbox_TryBegin_SecondCallForSameMessageIsRejected()
    {
        var inbox = new EfInboxStore(database.Factory);

        Assert.IsTrue(await inbox.TryBeginAsync("c", "m1"));
        Assert.IsFalse(await inbox.TryBeginAsync("c", "m1"));
        Assert.IsTrue(await inbox.TryBeginAsync("other", "m1"));
        Assert.IsTrue(await inbox.TryBeginAsync("c", "m2"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Inbox_Release_AllowsRetry()
    {
        var inbox = new EfInboxStore(database.Factory);
        await inbox.TryBeginAsync("c", "m1");

        await inbox.ReleaseAsync("c", "m1");

        Assert.IsTrue(await inbox.TryBeginAsync("c", "m1"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Inbox_ConcurrentBegin_OnlyOneWins()
    {
        var inbox = new EfInboxStore(database.Factory);

        var results = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Task.Run(() => inbox.TryBeginAsync("c", "race"))));

        Assert.AreEqual(1, results.Count(r => r));
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Outbox_AddThenPending_RoundTripsTypedMessageInOrder()
    {
        var outbox = new EfOutboxStore(database.Factory, new OutboxMessageTypes().Add<Ping>());
        var now = new DateTimeOffset(2026, 11, 16, 10, 0, 0, TimeSpan.Zero);

        await outbox.AddAsync(new OutboxEntry("a", new Ping("one"), now));
        await outbox.AddAsync(new OutboxEntry("b", new Ping("two"), now.AddSeconds(-5)));

        var pending = await outbox.GetPendingAsync(10);

        CollectionAssert.AreEqual(new[] { "a", "b" }, pending.Select(p => p.Id).ToArray());
        Assert.AreEqual(new Ping("one"), pending[0].Message);
        Assert.AreEqual(now, pending[0].CreatedAt);
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Outbox_GetPending_HonoursMax()
    {
        var outbox = new EfOutboxStore(database.Factory, new OutboxMessageTypes().Add<Ping>());
        for (var i = 0; i < 3; i++)
        {
            await outbox.AddAsync(new OutboxEntry($"m{i}", new Ping("x"), DateTimeOffset.UtcNow));
        }

        Assert.AreEqual(2, (await outbox.GetPendingAsync(2)).Count);
        await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(() => outbox.GetPendingAsync(0));
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Outbox_MarkSent_RemovesTheEntry()
    {
        var outbox = new EfOutboxStore(database.Factory, new OutboxMessageTypes().Add<Ping>());
        await outbox.AddAsync(new OutboxEntry("a", new Ping("one"), DateTimeOffset.UtcNow));

        await outbox.MarkSentAsync("a");
        await outbox.MarkSentAsync("missing");

        Assert.AreEqual(0, (await outbox.GetPendingAsync(10)).Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Outbox_UnregisteredType_IsRejectedOnAdd()
    {
        var outbox = new EfOutboxStore(database.Factory, new OutboxMessageTypes());

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => outbox.AddAsync(new OutboxEntry("a", new Ping("one"), DateTimeOffset.UtcNow)));
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Outbox_RowWithUnknownTypeName_IsNotLoaded()
    {
        var outbox = new EfOutboxStore(database.Factory, new OutboxMessageTypes().Add<Ping>());
        await using (var db = await database.Factory.CreateDbContextAsync())
        {
            db.Outbox.Add(new OutboxRow { Id = "x", TypeName = "System.IO.FileInfo", Json = "{}", CreatedAtTicks = 1 });
            await db.SaveChangesAsync();
        }

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => outbox.GetPendingAsync(10));
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Outbox_DuplicateId_IsRejected()
    {
        var outbox = new EfOutboxStore(database.Factory, new OutboxMessageTypes().Add<Ping>());
        await outbox.AddAsync(new OutboxEntry("a", new Ping("one"), DateTimeOffset.UtcNow));

        await Assert.ThrowsExactlyAsync<DbUpdateException>(
            () => outbox.AddAsync(new OutboxEntry("a", new Ping("two"), DateTimeOffset.UtcNow)));
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Registration_ResolvesRelationalStoresAndSharesTypes()
    {
        var services = new ServiceCollection();
        services.TryAddEntityFrameworkMessaging(
            (_, options) => options.UseSqlite("Data Source=mq_reg;Mode=Memory;Cache=Shared"),
            types => types.Add<Ping>());
        await using var provider = services.BuildServiceProvider();

        Assert.IsInstanceOfType<EfInboxStore>(provider.GetRequiredService<IInboxStore>());
        Assert.IsInstanceOfType<EfOutboxStore>(provider.GetRequiredService<IOutboxStore>());
        Assert.AreSame(provider.GetRequiredService<OutboxMessageTypes>(), provider.GetRequiredService<OutboxMessageTypes>());
    }
}
