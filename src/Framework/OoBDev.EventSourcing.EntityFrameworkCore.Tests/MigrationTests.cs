using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.EventSourcing.EntityFrameworkCore;
using OoBDev.EventSourcing.Postgres;
using OoBDev.EventSourcing.Sqlite;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing.EntityFrameworkCore.Tests;

[TestClass]
public class MigrationTests
{
    private static EventSourcingDbContext Create(Action<DbContextOptionsBuilder<EventSourcingDbContext>> configure)
    {
        var builder = new DbContextOptionsBuilder<EventSourcingDbContext>();
        configure(builder);
        return new EventSourcingDbContext(builder.Options);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void SqliteMigrations_MatchTheModel()
    {
        using var db = Create(b => b.UseSqlite("Data Source=unused.db", o => o.MigrationsAssembly(typeof(SqliteGlobals).Assembly.GetName().Name)));

        Assert.IsFalse(db.Database.HasPendingModelChanges());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void PostgresMigrations_MatchTheModel()
    {
        using var db = Create(b => b.UseNpgsql("Host=unused;Database=unused", o => o.MigrationsAssembly(typeof(PostgresGlobals).Assembly.GetName().Name)));

        Assert.IsFalse(db.Database.HasPendingModelChanges());
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task SqliteMigrate_CreatesTablesThatTheStoresUse()
    {
        var connectionString = $"Data Source=es_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        await using var keeper = new SqliteConnection(connectionString);
        await keeper.OpenAsync();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            [EventSourcingGlobals.EventStorePath] = SqliteGlobals.ProviderKey,
        }).Build());
        services.TryAddEventSourcing();
        services.TryAddSqliteEventSourcing(connectionString);
        await using var provider = services.BuildServiceProvider();

        await using (var db = await provider.GetRequiredService<IDbContextFactory<EventSourcingDbContext>>().CreateDbContextAsync())
            await db.Database.MigrateAsync();
        var store = provider.GetRequiredKeyedService<IEventStore>(EventSourcingGlobals.EventStoreSelectedKey);
        var result = await store.AppendAsync("a", 0, [new NewEvent("Created", "{}")]);

        Assert.IsInstanceOfType<EfEventStore>(store);
        Assert.IsTrue(result.Succeeded);
    }
}
