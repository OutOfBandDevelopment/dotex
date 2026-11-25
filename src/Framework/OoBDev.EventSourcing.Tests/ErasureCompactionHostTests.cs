using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.EventSourcing.Hosting;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing.Tests;

[TestClass]
public class ErasureCompactionHostTests
{
    public required TestContext TestContext { get; set; }

    [EventType("HostCustomerRegistered")]
    [PersonalData("CustomerId", "Name")]
    private sealed record HostCustomerRegistered(string CustomerId, string Name);

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task StartAsync_OpenRequest_IsCompactedWithoutACommand()
    {
        // Stage
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OoBDev:EventSourcing:Erasure:PollInterval"] = "00:00:00.020",
        }).Build());
        services.TryAddErasureCompactionHost();
        await using var provider = services.BuildServiceProvider();
        _ = provider.GetRequiredService<EventTypeRegistry>().Register<HostCustomerRegistered>();
        var store = provider.GetRequiredKeyedService<IEventStore>(EventSourcingGlobals.EventStoreSelectedKey);
        await store.AppendAsync("c1", 0, [new NewEvent("HostCustomerRegistered", JsonSerializer.Serialize(new HostCustomerRegistered("c1", "Ann Lee")))]);
        await provider.GetRequiredService<ErasureService>().RequestAsync("c1", "r1");
        var host = provider.GetServices<IHostedService>().OfType<ErasureCompactionHost>().Single();

        // Test
        await host.StartAsync(CancellationToken.None);
        var blanked = false;
        for (var i = 0; i < 200 && !blanked; i++)
        {
            blanked = (await store.ReadAsync("c1"))[0].Payload.Contains("[erased]", StringComparison.Ordinal);
            if (!blanked)
                await Task.Delay(50);
        }

        await host.StopAsync(CancellationToken.None);

        // Assert
        TestContext.WriteLine($"blanked={blanked}");
        Assert.IsTrue(blanked);
        Assert.IsTrue((await store.ReadAsync(ErasureService.StreamOf("c1"))).Any(e => e.Type == "ErasureCompleted"));
    }
}
