using Microsoft.Extensions.Configuration;
using OoBDev.TestUtilities.Containers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AI;
using OoBDev.TestUtilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.Ollama.Tests;

[TestClass]
public class OllamaEmbeddingProviderTests
{
    public TestContext TestContext { get; set; } = null!;

    private static ServiceProvider Build(string url, string? embeddingModel = null)
    {
        var settings = new Dictionary<string, string?>
        {
            ["OllamaApiClientOptions:Url"] = url,
            ["OllamaApiClientOptions:DefaultModel"] = "phi3",
        };
        if (embeddingModel != null)
            settings["OllamaApiClientOptions:EmbeddingModel"] = embeddingModel;
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        return new ServiceCollection()
            .AddLogging()
            .TryAddOllamaServices(configuration, nameof(OllamaApiClientOptions))
            .BuildServiceProvider();
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Registration_KeyedEmbeddingProvider_IsTheEmbeddingProviderNotTheChatClass()
    {
        using var provider = Build("http://localhost:11434");

        var embedding = provider.GetRequiredKeyedService<IEmbeddingProvider>(OllamaGlobals.ProviderKey);

        Assert.IsInstanceOfType<OllamaEmbeddingProvider>(embedding);
        Assert.IsNull(provider.GetService<IEmbeddingProvider>(), "the default embedding provider is left to the host");
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Options_EmbeddingModel_DefaultsToAllMiniLm()
    {
        Assert.AreEqual("all-minilm", new OllamaApiClientOptions { Url = "http://localhost", DefaultModel = "phi3" }.EmbeddingModel);
    }

    [TestCategory(TestCategories.Integration)]
    [IntegrationTestMethod("oobd-test-ollama")]
    public async Task GenerateEmbeddingAsync_DockerOllama_PullsTheEmbeddingModelAndReturnsAVectorAsync()
    {
        var url = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("OLLAMA_URL"));
        using var provider = Build(url, TestContext.GetPropertyOrDefault("OLLAMA_EMBEDDING_MODEL", "all-minilm"));
        var embedding = provider.GetRequiredKeyedService<IEmbeddingProvider>(OllamaGlobals.ProviderKey);

        var vector = await embedding.GenerateEmbeddingAsync("Hello World!", default, TestContext.CancellationToken);

        Assert.IsGreaterThan(0, vector.Length);
        Assert.AreEqual(vector.Length, embedding.Length);
    }
}
