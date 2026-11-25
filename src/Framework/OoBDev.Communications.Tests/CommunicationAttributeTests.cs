using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Communications.Abstractions;
using OoBDev.TestUtilities;

namespace OoBDev.Communications.Tests;

[TestClass]
public class CommunicationAttributeTests
{
    public required TestContext TestContext { get; set; }

    [Communication(MessageType = "order.confirmation")]
    public sealed class OrderEnhancer : IDataEnhancer
    {
        public bool AppliesTo(string messageType) => true;

        public Task EnhanceAsync(CommunicationRequest request, IDictionary<string, object?> data, CancellationToken cancellationToken = default)
        {
            data["Order"] = "ok";
            return Task.CompletedTask;
        }
    }

    [Communication]
    public sealed class EveryTypeEnhancer : IDataEnhancer
    {
        public bool AppliesTo(string messageType) => true;

        public Task EnhanceAsync(CommunicationRequest request, IDictionary<string, object?> data, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    public sealed class Unmarked : IDataEnhancer
    {
        public bool AppliesTo(string messageType) => true;

        public Task EnhanceAsync(CommunicationRequest request, IDictionary<string, object?> data, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void TryAddCommunicationEnhancers_FindsMarkedClassesOnly_AndFiltersByMessageType()
    {
        // Stage
        var services = new ServiceCollection();

        // Test
        services.TryAddCommunicationEnhancers(typeof(CommunicationAttributeTests).Assembly);
        using var provider = services.BuildServiceProvider();
        var enhancers = provider.GetServices<IDataEnhancer>().ToList();
        TestContext.WriteLine($"found {enhancers.Count}");

        // Assert
        Assert.AreEqual(2, enhancers.Count);
        Assert.AreEqual(1, enhancers.Count(e => e.AppliesTo("order.confirmation") && !e.AppliesTo("welcome")));
        Assert.AreEqual(1, enhancers.Count(e => e.AppliesTo("welcome")));
    }
}
