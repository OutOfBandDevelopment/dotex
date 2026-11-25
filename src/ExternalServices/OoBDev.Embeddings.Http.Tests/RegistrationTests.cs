using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AI;
using OoBDev.TestUtilities;

namespace OoBDev.Embeddings.Http.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class RegistrationTests
{
    private static ServiceProvider Build(Dictionary<string, string?> values)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.TryAddHttpEmbeddings(config);
        return services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task TryAddHttpEmbeddings_ResolvesPlainAndKeyedProvider()
    {
        await using var provider = Build(new()
        {
            ["Embeddings:Http:BaseUrl"] = "http://localhost:11435/",
            ["Embeddings:Http:Model"] = "m",
            ["Embeddings:Http:Dimensions"] = "384",
            ["Embeddings:Http:ApiKey"] = "k",
        });

        Assert.IsInstanceOfType<OpenAiCompatibleEmbeddingProvider>(provider.GetRequiredService<IEmbeddingProvider>());
        Assert.AreEqual(384, provider.GetRequiredKeyedService<IEmbeddingProvider>(EmbeddingsHttpGlobals.ProviderKey).Length);
    }

    [TestMethod]
    public async Task TryAddHttpEmbeddings_MissingDimensions_FailsValidation()
    {
        await using var provider = Build(new() { ["Embeddings:Http:BaseUrl"] = "http://x", ["Embeddings:Http:Model"] = "m" });

        Assert.ThrowsExactly<OptionsValidationException>(() => provider.GetRequiredService<IEmbeddingProvider>());
    }
}
