using OoBDev.AI;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.Ollama.Tests;

[TestClass]
public class OllamaMessageCompletionTests
{
    public required TestContext TestContext { get; set; }

    private T Build<T>(string url, string model) where T : notnull
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "OllamaApiClientOptions:Url", url},
                { "OllamaApiClientOptions:DefaultModel", model},
            })
            .Build();

        var services = new ServiceCollection()
            .AddSingleton(config)
            .TryAddOllamaServices(config, nameof(OllamaApiClientOptions))
            .BuildServiceProvider()
            ;

        var client = services.GetRequiredService<T>();
        return client;
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    public async Task IMessageCompletion_GetCompletionAsyncTest()
    {
        var url = TestContext.GetRequiredProperty<string>("OLLAMA_URL");
        var model = TestContext.GetPropertyOrDefault("OLLAMA_MODEL", "phi3");

        var client = Build<IMessageCompletion>(url, model);
        var response = await client.GetCompletionAsync(model, "Hello World!");

        TestContext.WriteLine($"url: {url}");
        TestContext.WriteLine($"model: {model}");
        TestContext.WriteLine($"Response: {response}");

        Assert.IsFalse(string.IsNullOrWhiteSpace(response));
    }

    [TestCategory(TestCategories.DevLocal)]
    [TestMethod]
    [DataRow("http://127.0.0.1:11434", "phi", "Hello World!")]
    public async Task IMessageCompletion_GetCompletionAsyncTest_DevLocal(string hostName, string model, string prompt)
    {
        var client = Build<IMessageCompletion>(hostName, model);
        var embedding = await client.GetCompletionAsync(model, prompt);
        TestContext.WriteLine(embedding);

        Assert.IsFalse(string.IsNullOrWhiteSpace(embedding));
    }

}
