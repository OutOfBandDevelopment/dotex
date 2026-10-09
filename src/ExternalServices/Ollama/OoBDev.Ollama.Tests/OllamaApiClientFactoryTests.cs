using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OllamaSharp;
using OoBDev.TestUtilities;
using System.Linq;

namespace OoBDev.Ollama.Tests;

[TestClass]
public class OllamaApiClientFactoryTests
{
    private static OllamaApiClient Build(string? apiKey)
    {
        var options = Options.Create(new OllamaApiClientOptions
        {
            Url = "http://localhost:11434",
            DefaultModel = "phi3",
            ApiKey = apiKey,
        });

        return (OllamaApiClient)new OllamaApiClientFactory(options).Build();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Build_ApiKey_AddsBearerAuthorizationHeader()
    {
        var client = Build("secret");

        Assert.AreEqual("Bearer secret", client.DefaultRequestHeaders["Authorization"]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("  ")]
    public void Build_NoApiKey_AddsNoAuthorizationHeader(string? apiKey)
    {
        var client = Build(apiKey);

        Assert.IsFalse(client.DefaultRequestHeaders.Keys.Contains("Authorization"));
    }
}
