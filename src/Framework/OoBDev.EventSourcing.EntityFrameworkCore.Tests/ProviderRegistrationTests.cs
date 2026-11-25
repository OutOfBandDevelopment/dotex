using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.EventSourcing.Postgres;
using OoBDev.EventSourcing.SqlServer;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;

namespace OoBDev.EventSourcing.EntityFrameworkCore.Tests;

[TestClass]
public class ProviderRegistrationTests
{
    private static ServiceProvider Build(string key, Action<IServiceCollection> register)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            [EventSourcingGlobals.EventStorePath] = key,
            [EventSourcingGlobals.EntityStorePath] = key,
        }).Build());
        services.TryAddEventSourcing();
        register(services);
        return services.BuildServiceProvider();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void TryAddPostgresEventSourcing_Configured_ResolvesEfStores()
    {
        // Stage
        using var provider = Build(PostgresGlobals.ProviderKey, s => s.TryAddPostgresEventSourcing("Host=unused;Database=unused"));

        // Test / Assert
        Assert.IsInstanceOfType<EfEventStore>(provider.GetRequiredKeyedService<IEventStore>(EventSourcingGlobals.EventStoreSelectedKey));
        Assert.IsInstanceOfType<EfEntityStore>(provider.GetRequiredKeyedService<IEntityStore>(EventSourcingGlobals.EntityStoreSelectedKey));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void TryAddSqlServerEventSourcing_Configured_ResolvesEfStores()
    {
        // Stage
        using var provider = Build(SqlServerGlobals.ProviderKey, s => s.TryAddSqlServerEventSourcing("Server=unused;Database=unused"));

        // Test / Assert
        Assert.IsInstanceOfType<EfEventStore>(provider.GetRequiredKeyedService<IEventStore>(EventSourcingGlobals.EventStoreSelectedKey));
        Assert.IsInstanceOfType<EfEntityStore>(provider.GetRequiredKeyedService<IEntityStore>(EventSourcingGlobals.EntityStoreSelectedKey));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void TryAddSqlServerEventSourcing_BlankConnectionString_Throws()
    {
        // Test / Assert
        Assert.ThrowsExactly<ArgumentException>(() => new ServiceCollection().TryAddSqlServerEventSourcing(" "));
    }
}
