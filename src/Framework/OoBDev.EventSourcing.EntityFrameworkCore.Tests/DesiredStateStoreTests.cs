using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Data.DesiredState;
using OoBDev.EventSourcing.Sqlite;
using OoBDev.System.IO;
using OoBDev.TestUtilities;

namespace OoBDev.EventSourcing.EntityFrameworkCore.Tests;

/// <summary>The event store reaches its final schema through the desired-state provider and the stores then work on it.</summary>
[TestClass]
public class DesiredStateStoreTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task SqliteEnsure_CreatesTablesThatTheStoresUse()
    {
        // Stage
        if (!EventStoreFinalStateTests.OnPath("sqlite3def"))
        {
            Assert.Inconclusive("sqlite3def (sqldef, MIT) is not on PATH.");
        }

        using var folder = new TempDirectoryHandle();
        var file = Path.Combine(folder.DirectoryPath, "events.db");
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            [EventSourcingGlobals.EventStorePath] = SqliteGlobals.ProviderKey,
        }).Build());
        services.AddDesiredStateDatabase();
        services.TryAddEventSourcing();
        services.TryAddSqliteEventSourcing($"Data Source={file};Pooling=False");
        await using var provider = services.BuildServiceProvider();
        TestContext.WriteLine($"database file: {file}");

        // Test
        EfDesiredStateResult first;
        EfDesiredStateResult second;
        await using (var db = await provider.GetRequiredService<IDbContextFactory<EventSourcingDbContext>>().CreateDbContextAsync(TestContext.CancellationToken))
        {
            first = await provider.GetRequiredService<EfDesiredStateDeployer>().EnsureAsync(db, cancellationToken: TestContext.CancellationToken);
            second = await provider.GetRequiredService<EfDesiredStateDeployer>().EnsureAsync(db, cancellationToken: TestContext.CancellationToken);
        }

        var store = provider.GetRequiredKeyedService<IEventStore>(EventSourcingGlobals.EventStoreSelectedKey);
        var result = await store.AppendAsync("a", 0, [new NewEvent("Created", "{}")]);
        TestContext.WriteLine($"first plan={first.Plan.Statements.Count} second plan={second.Plan.Statements.Count}");

        // Assert
        Assert.IsFalse(first.Plan.IsEmpty);
        Assert.IsTrue(second.Plan.IsEmpty);
        Assert.IsInstanceOfType<EfEventStore>(store);
        Assert.IsTrue(result.Succeeded);
    }
}
