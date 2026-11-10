using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Search.Semantic;
using OoBDev.Search.Tests.Semantic;
using OoBDev.TestUtilities;
using System.Linq;

namespace OoBDev.Search.Tests;

[TestClass]
public class ServiceCollectionExtensionsTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryAddSearchServices_Twice_RegistersOnce()
    {
        // Stage
        var services = new ServiceCollection();

        // Test
        services.TryAddSearchServices().TryAddSearchServices();

        // Assert
        Assert.AreEqual(1, services.Count(i => i.ServiceType == typeof(IVectorStoreFactory)));
        Assert.AreEqual(1, services.Count(i => i.ServiceType == typeof(IVectorStore<>)));
    }

    [TestMethod, TestCategory(TestCategories.Simulate)]
    public void TryAddSearchServices_ResolvesStoreOfT()
    {
        // Stage
        var fallback = new FakeVectorStoreProvider();
        var services = new ServiceCollection();
        services.AddSingleton<IVectorStoreProvider>(fallback);
        services.TryAddSearchServices();
        using var provider = services.BuildServiceProvider();

        // Test
        var store = provider.GetRequiredService<IVectorStore<ServiceCollectionExtensionsTests>>();

        // Assert
        Assert.IsNotNull(store);
        Assert.AreEqual(nameof(ServiceCollectionExtensionsTests), fallback.CollectionName);
    }
}
