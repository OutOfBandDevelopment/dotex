using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Search.Semantic;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;

namespace OoBDev.Microsoft.Azure.AISearch.Tests;

[TestClass]
public class ServiceCollectionExtensionsTests
{
    private static ServiceProvider Build(string? apiKey)
    {
        var values = new Dictionary<string, string?>
        {
            ["AI:Endpoint"] = "https://x.search.windows.net",
            ["AI:IndexName"] = "docs",
            ["AI:ApiKey"] = apiKey,
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        return new ServiceCollection().TryAddAzureAISearchServices(configuration, "AI").BuildServiceProvider();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void TryAddAzureAISearchServices_BindsOptionsAndFactory()
    {
        // Test
        using var provider = Build("key");

        // Assert
        Assert.AreEqual("docs", provider.GetRequiredService<IOptions<AzureAISearchOptions>>().Value.IndexName);
        Assert.IsInstanceOfType<AzureAISearchVectorStoreProviderFactory>(provider.GetRequiredService<IVectorStoreProviderFactory>());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Create_WithApiKey_ReturnsProviderForTheIndex()
    {
        // Stage
        using var provider = Build("key");

        // Test
        var store = provider.GetRequiredService<IVectorStoreProviderFactory>().Create("other");

        // Assert
        Assert.AreEqual("other", store.CollectionName);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Create_WithoutApiKey_UsesDefaultCredential()
    {
        // Stage
        using var provider = Build(null);

        // Test
        var store = provider.GetRequiredService<IVectorStoreProviderFactory>().Create("other");

        // Assert
        Assert.AreEqual("other", store.CollectionName);
    }
}
