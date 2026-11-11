using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using OoBDev.EventSourcing.EntityFrameworkCore;
using OoBDev.EventSourcing.Postgres;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing.EntityFrameworkCore.Tests;

[TestClass]
public class PostgresIntegrationTests
{
    private NpgsqlConnectionStringBuilder _admin = null!;
    private string _databaseName = null!;
    private ServiceProvider _provider = null!;

    public TestContext TestContext { get; set; } = null!;

    [TestInitialize]
    public async Task Initialize()
    {
        _admin = new NpgsqlConnectionStringBuilder(TestContext.GetRequiredProperty<string>("POSTGRES_CONNECTION_STRING"));
        Assert.IsTrue(_admin.Database!.Contains("test", StringComparison.OrdinalIgnoreCase), "Refusing to run against a database that is not a test instance.");

        _databaseName = $"es_test_{Guid.NewGuid():N}";
        await using (var connection = new NpgsqlConnection(_admin.ConnectionString))
        {
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand($"CREATE DATABASE \"{_databaseName}\"", connection);
            await command.ExecuteNonQueryAsync();
        }

        var target = new NpgsqlConnectionStringBuilder(_admin.ConnectionString) { Database = _databaseName };
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            [EventSourcingGlobals.EventStorePath] = PostgresGlobals.ProviderKey,
            [EventSourcingGlobals.EntityStorePath] = PostgresGlobals.ProviderKey,
        }).Build());
        services.TryAddEventSourcing();
        services.TryAddPostgresEventSourcing(target.ConnectionString);
        _provider = services.BuildServiceProvider();
        await using var db = await _provider.GetRequiredService<IDbContextFactory<EventSourcingDbContext>>().CreateDbContextAsync();
        await db.Database.MigrateAsync();
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await _provider.DisposeAsync();
        NpgsqlConnection.ClearAllPools();
        await using var connection = new NpgsqlConnection(_admin.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{_databaseName}\" WITH (FORCE)", connection);
        await command.ExecuteNonQueryAsync();
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task EventStore_AppendReadAndConflict_WorkOnPostgreSql()
    {
        // Stage
        var store = _provider.GetRequiredKeyedService<IEventStore>(EventSourcingGlobals.EventStoreSelectedKey);

        // Test
        var first = await store.AppendAsync("a", 0, [new NewEvent("Created", "{\"n\":1}"), new NewEvent("Renamed", "{\"n\":2}")]);
        var conflict = await store.AppendAsync("a", 0, [new NewEvent("Created", "{}")]);
        var stream = await store.ReadAsync("a");
        var all = await store.ReadAllAsync(0, 10);

        // Assert
        Assert.IsInstanceOfType<EfEventStore>(store);
        Assert.IsTrue(first.Succeeded);
        Assert.IsFalse(conflict.Succeeded);
        CollectionAssert.AreEqual(new long[] { 1, 2 }, stream.Select(e => e.Sequence).ToArray());
        Assert.AreEqual(2, all.Count);
        Assert.IsTrue(all[0].Position < all[1].Position);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task EntityStore_VersionedSaveAndDelete_WorkOnPostgreSql()
    {
        // Stage
        var store = _provider.GetRequiredKeyedService<IEntityStore>(EventSourcingGlobals.EntityStoreSelectedKey);

        // Test
        var created = await store.SaveAsync("t", "k", "{\"v\":1}", 0);
        var stale = await store.SaveAsync("t", "k", "{\"v\":2}", 0);
        var updated = await store.SaveAsync("t", "k", "{\"v\":2}", 1);
        var deleted = await store.DeleteAsync("t", "k", 2);

        // Assert
        Assert.AreEqual(1, created!.Version);
        Assert.IsNull(stale);
        Assert.AreEqual(2, updated!.Version);
        Assert.IsTrue(deleted);
        Assert.IsNull(await store.GetAsync("t", "k"));
    }
}
