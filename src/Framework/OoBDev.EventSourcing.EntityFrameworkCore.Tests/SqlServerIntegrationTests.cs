using Microsoft.Data.SqlClient;
using OoBDev.TestUtilities.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SqlServer.Dac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.EventSourcing.SqlServer;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing.EntityFrameworkCore.Tests;

/// <summary>
/// Deploys the dacpac to a throwaway database on the Docker SQL Server and runs the stores against it.
/// </summary>
[TestClass]
public class SqlServerIntegrationTests
{
    private string _master = null!;
    private string _databaseName = null!;
    private ServiceProvider _provider = null!;

    public TestContext TestContext { get; set; } = null!;

    [TestInitialize]
    public async Task Initialize()
    {
        _master = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("SQLSERVER_CONNECTION_STRING"));
        _databaseName = $"es_test_{Guid.NewGuid():N}";
        await using (var connection = new SqlConnection(_master))
        {
            await connection.OpenAsync();
            await using var command = new SqlCommand($"CREATE DATABASE [{_databaseName}]", connection);
            await command.ExecuteNonQueryAsync();
        }

        using (var package = DacPackage.Load(FindDacpac()))
            new DacServices(_master).Deploy(package, _databaseName, upgradeExisting: true);

        var target = new SqlConnectionStringBuilder(_master) { InitialCatalog = _databaseName };
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            [EventSourcingGlobals.EventStorePath] = SqlServerGlobals.ProviderKey,
            [EventSourcingGlobals.EntityStorePath] = SqlServerGlobals.ProviderKey,
        }).Build());
        services.TryAddEventSourcing();
        services.TryAddSqlServerEventSourcing(target.ConnectionString);
        _provider = services.BuildServiceProvider();
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        if (_provider is null)
            return;
        await _provider.DisposeAsync();
        SqlConnection.ClearAllPools();
        await using var connection = new SqlConnection(_master);
        await connection.OpenAsync();
        await using var command = new SqlCommand($"IF DB_ID('{_databaseName}') IS NOT NULL BEGIN ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{_databaseName}]; END", connection);
        await command.ExecuteNonQueryAsync();
    }

    private static string FindDacpac()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var project = Path.Combine(directory.FullName, "OoBDev.EventSourcing.SqlServer.Database", "bin");
            if (Directory.Exists(project))
                return Directory.EnumerateFiles(project, "*.dacpac", SearchOption.AllDirectories)
                    .OrderByDescending(File.GetLastWriteTimeUtc).First();
            directory = directory.Parent;
        }
        throw new FileNotFoundException("The SQL Server dacpac was not built.");
    }

    [IntegrationTestMethod("oobd-test-sqlserver")]
    [TestCategory(TestCategories.Integration)]
    public async Task EventStore_AppendReadAndConflict_WorkOnTheDeployedDacpac()
    {
        // Stage
        var store = _provider.GetRequiredKeyedService<IEventStore>(EventSourcingGlobals.EventStoreSelectedKey);

        // Test
        var first = await store.AppendAsync("a", 0, [new NewEvent("Created", "{\"n\":1}"), new NewEvent("Renamed", "{\"n\":2}")]);
        var conflict = await store.AppendAsync("a", 0, [new NewEvent("Created", "{}")]);
        var stream = await store.ReadAsync("a");
        var all = await store.ReadAllAsync(0, 10);

        // Assert
        Assert.IsTrue(first.Succeeded);
        Assert.IsFalse(conflict.Succeeded);
        CollectionAssert.AreEqual(new long[] { 1, 2 }, stream.Select(e => e.Sequence).ToArray());
        Assert.AreEqual(2, all.Count);
        Assert.IsTrue(all[0].Position < all[1].Position);
    }

    [IntegrationTestMethod("oobd-test-sqlserver")]
    [TestCategory(TestCategories.Integration)]
    public async Task EntityStore_VersionedSaveAndDelete_WorkOnTheDeployedDacpac()
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
