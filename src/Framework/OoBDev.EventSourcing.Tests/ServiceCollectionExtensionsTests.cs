using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.MessageQueueing;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;

namespace OoBDev.EventSourcing.Tests;

[TestClass]
public class ServiceCollectionExtensionsTests
{
    private static ServiceProvider Build(Dictionary<string, string?> settings)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(settings).Build());
        services.AddSingleton(Mock.Of<IMessageQueueSender<EventsChannel>>());
        services.TryAddEventSourcing();
        return services.BuildServiceProvider();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void TryAddEventSourcing_WithoutConfiguration_UsesInMemoryStores()
    {
        using var provider = Build([]);

        Assert.IsInstanceOfType<InMemoryEventStore>(provider.GetRequiredKeyedService<IEventStore>(EventSourcingGlobals.EventStoreSelectedKey));
        Assert.IsInstanceOfType<InMemoryEntityStore>(provider.GetRequiredKeyedService<IEntityStore>(EventSourcingGlobals.EntityStoreSelectedKey));
        Assert.IsNotNull(provider.GetRequiredService<EventRelay>());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void TryAddEventSourcing_InMemoryKey_Resolves()
    {
        using var provider = Build(new() { [EventSourcingGlobals.EventStorePath] = EventSourcingGlobals.InMemoryKey });

        Assert.IsInstanceOfType<InMemoryEventStore>(provider.GetRequiredKeyedService<IEventStore>(EventSourcingGlobals.EventStoreSelectedKey));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void TryAddEventSourcing_UnknownKey_Throws()
    {
        using var provider = Build(new() { [EventSourcingGlobals.EntityStorePath] = "nope" });

        Assert.ThrowsExactly<InvalidOperationException>(() => provider.GetRequiredKeyedService<IEntityStore>(EventSourcingGlobals.EntityStoreSelectedKey));
    }
}
