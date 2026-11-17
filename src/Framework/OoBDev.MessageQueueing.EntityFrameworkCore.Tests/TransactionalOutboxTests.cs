using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.EntityFrameworkCore.Tests;

[TestClass]
public class TransactionalOutboxTests
{
    public sealed record OrderPlaced(string OrderId);

    public sealed class Order
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    private sealed class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
    {
        public DbSet<Order> Orders => Set<Order>();

        protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.AddOutbox();
    }

    private SqliteMessagingDatabase database = null!;
    private readonly OutboxMessageTypes types = new OutboxMessageTypes().Add<OrderPlaced>();

    [TestInitialize]
    public void Initialize()
    {
        database = new SqliteMessagingDatabase();
        using var db = Orders();
        db.Database.ExecuteSqlRaw("CREATE TABLE Orders (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL)");
    }

    [TestCleanup]
    public async Task Cleanup() => await database.DisposeAsync();

    private OrdersDbContext Orders() => new(new DbContextOptionsBuilder<OrdersDbContext>().UseSqlite(database.ConnectionString).Options);

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task StageOutbox_SavedWithBusinessChange_DispatcherStoreReadsIt()
    {
        await using (var db = Orders())
        {
            db.Orders.Add(new Order { Name = "first" });
            var id = db.StageOutbox(types, new OrderPlaced("o1"), id: "m1");
            await db.SaveChangesAsync();
            Assert.AreEqual("m1", id);
        }

        var pending = await new EfOutboxStore(database.Factory, types).GetPendingAsync(10);

        Assert.AreEqual(1, pending.Count);
        Assert.AreEqual(new OrderPlaced("o1"), pending[0].Message);
        await using var check = Orders();
        Assert.AreEqual(1, await check.Orders.CountAsync());
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task StageOutbox_TransactionRolledBack_NeitherRowIsStored()
    {
        await using (var db = Orders())
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            db.Orders.Add(new Order { Name = "lost" });
            db.StageOutbox(types, new OrderPlaced("o2"));
            await db.SaveChangesAsync();
            await transaction.RollbackAsync();
        }

        Assert.AreEqual(0, (await new EfOutboxStore(database.Factory, types).GetPendingAsync(10)).Count);
        await using var check = Orders();
        Assert.AreEqual(0, await check.Orders.CountAsync());
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task StageOutbox_UnregisteredType_Throws()
    {
        await using var db = Orders();

        Assert.ThrowsExactly<InvalidOperationException>(() => db.StageOutbox(types, new Order()));
    }
}
