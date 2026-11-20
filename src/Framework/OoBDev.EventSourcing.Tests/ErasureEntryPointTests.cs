using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing.Tests;

/// <summary>The erasure entry point records a request that the compaction job then completes.</summary>
[TestClass]
public class ErasureEntryPointTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Request_Subject_ReturnsRequestIdThatCompactionCompletes()
    {
        // Stage
        var store = new InMemoryEventStore(TimeProvider.System);
        var service = new ErasureService(store, store, new EventTypeRegistry(), []);
        IErasureEntryPoint entry = new ErasureEntryPoint(service);

        // Test
        var id = await entry.RequestAsync("c9", ["backup"]);
        var other = await entry.RequestAsync("c10");
        var completed = await service.CompactAsync();

        // Assert
        TestContext.WriteLine($"Request ids {id}, {other}; completed {completed}");
        Assert.AreNotEqual(id, other);
        Assert.AreEqual(2, completed);
        StringAssert.Contains((await store.ReadAsync(ErasureService.StreamOf("c9")))[0].Payload, id);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Request_BlankSubject_IsRefused()
    {
        var store = new InMemoryEventStore(TimeProvider.System);
        IErasureEntryPoint entry = new ErasureEntryPoint(new ErasureService(store, store, new EventTypeRegistry(), []));

        await Assert.ThrowsAsync<ArgumentException>(() => entry.RequestAsync(" "));
    }
}
