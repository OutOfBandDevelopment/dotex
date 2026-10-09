using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AI;
using OoBDev.AI.Models;
using OoBDev.TestUtilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.GroqCloud.Tests;

/// <summary>
/// Calls the real Groq Cloud API. Manual execution only: needs <c>GROQ_API_KEY</c> and may incur cost.
/// See README.GroqCloud.Tests.md.
/// </summary>
[TestClass]
public class GroqCloudLiveTests
{
    public TestContext TestContext { get; set; } = null!;

    private ServiceProvider BuildServices()
    {
        var apiKey = TestContext.GetPropertyOrDefault("GROQ_API_KEY", string.Empty);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Assert.Inconclusive("GROQ_API_KEY is not set; see README.GroqCloud.Tests.md.");
        }

        var settings = new Dictionary<string, string?>
        {
            [$"{nameof(GroqCloudApiClientOptions)}:{nameof(GroqCloudApiClientOptions.ApiKey)}"] = apiKey,
        };
        var model = TestContext.GetPropertyOrDefault("GROQ_MODEL", string.Empty);
        if (!string.IsNullOrWhiteSpace(model))
        {
            settings[$"{nameof(GroqCloudApiClientOptions)}:{nameof(GroqCloudApiClientOptions.Model)}"] = model;
        }

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        return new ServiceCollection()
            .TryAddGroqCloudServices(configuration, nameof(GroqCloudApiClientOptions))
            .BuildServiceProvider();
    }

    [TestMethod]
    [TestCategory(TestCategories.LiveIntegration)]
    public async Task GetCompletionAsync_ShortPrompt_ReturnsText()
    {
        // Stage
        using var services = BuildServices();
        var completion = services.GetRequiredService<IMessageCompletion>();
        var model = TestContext.GetPropertyOrDefault("GROQ_MODEL", new GroqCloudApiClientOptions().Model);

        // Test
        var response = await completion.GetCompletionAsync(new CompletionRequest { Model = model, Prompt = "Reply with the single word: pong" });

        // Assert
        Assert.IsFalse(string.IsNullOrWhiteSpace(response.Response));
    }

    [TestMethod]
    [TestCategory(TestCategories.LiveIntegration)]
    public async Task HealthCheck_WithValidKey_IsHealthy()
    {
        // Stage
        using var services = BuildServices();
        var check = ActivatorUtilities.CreateInstance<GroqCloudHealthCheck>(services);

        // Test
        var result = await check.CheckHealthAsync(new HealthCheckContext());

        // Assert
        Assert.AreEqual(HealthStatus.Healthy, result.Status, result.Exception?.Message);
    }
}
