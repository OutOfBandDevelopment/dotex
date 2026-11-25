using OoBDev.Extensions;
using OoBDev.TestUtilities.Containers;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OllamaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OllamaSharp.Models.Chat;
using OoBDev.Extensions.Linq;

namespace OoBDev.Ollama.Tests;

/// <summary>
/// Tests for OllamaApiClient functionality.
/// Integration tests use TestContext properties (OLLAMA_URL, OLLAMA_MODEL) and run against Docker Ollama.
/// The vision, pull and delete tests fetch their models into the Docker Ollama on first run.
/// </summary>
[TestClass]
public class OllamaApiClientTests
{
    // The tests assert a non-empty answer, not its length: on the CPU-only test container generation time is per token.
    private const int MaxTokens = 24;

    public required TestContext TestContext { get; set; }

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        // Only initialize for Integration tests
        var isIntegrationTest = context.Properties.ContainsKey("OLLAMA_URL");
        if (!isIntegrationTest)
        {
            context.WriteLine("Skipping Ollama initialization - not running integration tests");
            return;
        }

        var url = TestInstanceGuard.Require(context.GetRequiredProperty<string>("OLLAMA_URL"));
        var model = context.GetPropertyOrDefault("OLLAMA_MODEL", "phi3");

        context.WriteLine($"Initializing Ollama at {url}");
        context.WriteLine($"Ensuring model '{model}' is available...");

        try
        {
            var client = new OllamaApiClient(new Uri(url));

            // Check if model already exists
            var models = await client.ListLocalModelsAsync();
            var modelExists = models.Any(m => m.Name.StartsWith(model));

            if (modelExists)
            {
                context.WriteLine($"✅ Model '{model}' already exists");
                return;
            }

            // Pull the model
            context.WriteLine($"Pulling model '{model}' (this may take several minutes on first run)...");
            var pullStarted = false;
            await foreach (var progress in client.PullModelAsync(model))
            {
                if (progress == null) continue;

                if (!pullStarted)
                {
                    pullStarted = true;
                    context.WriteLine($"Download started: {progress.Status}");
                }

                // Log progress at 10% intervals
                if (progress.Percent % 10 == 0)
                {
                    context.WriteLine($"Progress: {progress.Percent}% - {progress.Status}");
                }
            }

            context.WriteLine($"✅ Model '{model}' pulled successfully");
        }
        catch (Exception ex)
        {
            context.WriteLine($"⚠️ Failed to initialize Ollama: {ex.Message}");
            context.WriteLine("Tests may fail if model is not available");
            // Don't throw - let individual tests fail if needed
        }
    }

    private IOllamaApiClient Build(string url, string model)
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

        var factory = services.GetRequiredService<IOllamaApiClientFactory>();
        var client = factory.Build();
        return client;
    }

    [TestCategory(TestCategories.Integration)]
    [IntegrationTestMethod("oobd-test-ollama")]
    public async Task GenerateEmbeddingsDoubleTest()
    {
        var url = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("OLLAMA_URL"));
        var model = TestContext.GetPropertyOrDefault("OLLAMA_EMBEDDING_MODEL", "all-minilm");

        var client = Build(url, model);
        var embedding = await client.GetEmbeddingDoubleAsync("Hello World!", model);

        TestContext.WriteLine($"url: {url}");
        TestContext.WriteLine($"model: {model}");
        TestContext.WriteLine($"Length: {embedding.Length}");

        var preview = embedding.Length > 10 ? embedding.Slice(0, 10) : embedding;
        TestContext.WriteLine(string.Join(", ", preview.ToArray()));

        Assert.IsGreaterThan(0, embedding.Length, "Embedding should have elements");
    }

    [TestCategory(TestCategories.Integration)]
    [IntegrationTestMethod("oobd-test-ollama")]
    public async Task GenerateEmbeddingsSingleTest()
    {
        var url = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("OLLAMA_URL"));
        var model = TestContext.GetPropertyOrDefault("OLLAMA_EMBEDDING_MODEL", "all-minilm");

        var client = Build(url, model);
        var embedding = await client.GetEmbeddingSingleAsync("Hello World!", model);

        TestContext.WriteLine($"url: {url}");
        TestContext.WriteLine($"model: {model}");
        TestContext.WriteLine($"Length: {embedding.Length}");

        var preview = embedding.Length > 10 ? embedding[..10] : embedding;
        TestContext.WriteLine(string.Join(", ", preview.ToArray()));

        Assert.IsGreaterThan(0, embedding.Length, "Embedding should have elements");
    }

    [TestCategory(TestCategories.Integration)]
    [IntegrationTestMethod("oobd-test-ollama")]
    public async Task ChatAsyncTest()
    {
        var url = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("OLLAMA_URL"));
        var model = TestContext.GetPropertyOrDefault("OLLAMA_MODEL", "phi3");

        var client = Build(url, model);
        var responses = await client.ChatAsync(new()
        {
            Model = model,
            Messages = new[] { new Message(ChatRole.User, "Say 'Hello' in one word.") },
            Options = new OllamaSharp.Models.RequestOptions
            {
                Seed = 12345,
                NumPredict = MaxTokens,
                Temperature = 0.1f,
            },
        }).ToListAsync();

        TestContext.WriteLine($"url: {url}");
        TestContext.WriteLine($"model: {model}");
        TestContext.WriteLine($"Response count: {responses.Count}");

        var fullResponse = string.Join("", responses.Select(r => r?.Message.Content ?? ""));
        TestContext.WriteLine($"Full response: {fullResponse}");

        Assert.IsGreaterThan(0, responses.Count, "Should receive at least one response");
        Assert.IsFalse(string.IsNullOrWhiteSpace(fullResponse), "Response should not be empty");
    }

    [TestCategory(TestCategories.Integration)]
    [IntegrationTestMethod("oobd-test-ollama")]
    public async Task ChatAsyncTest_SamplingOptions_AnswersAsync()
    {
        var url = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("OLLAMA_URL"));
        var model = TestContext.GetPropertyOrDefault("OLLAMA_MODEL", "phi3");

        var client = Build(url, model);
        var responses = await client.ChatAsync(new()
        {
            Model = model,
            Messages = new[] { new Message(ChatRole.User, "Tell me a story about a cat in two sentences.") },
            Options = new OllamaSharp.Models.RequestOptions
            {
                Seed = 12312542,
                NumPredict = MaxTokens,
                TopK = 100,
                TopP = 0.9f,
                Temperature = 1f,
            },
        }).ToListAsync();

        var text = string.Join("", responses.Select(r => r?.Message.Content ?? ""));
        TestContext.WriteLine(string.Join(Environment.NewLine, text.SplitBy(50)));

        Assert.IsFalse(string.IsNullOrWhiteSpace(text), "Response should not be empty");
    }

    [TestCategory(TestCategories.Integration)]
    [IntegrationTestMethod("oobd-test-ollama")]
    [DataRow("describe the image", "LadyDancingWithDog.jpg")]
    [DataRow("describe the image in less than 10 words", "RobotsTalking.jpg")]
    public async Task ChatWithVisionTest(string prompt, string imageName)
    {
        var url = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("OLLAMA_URL"));
        var model = TestContext.GetPropertyOrDefault("OLLAMA_VISION_MODEL", "llava-phi3");
        using var img = GetType().Assembly.GetManifestResourceStream($"OoBDev.Ollama.Tests.TestData.{imageName}")!;
        using var ms = new MemoryStream();
        img.CopyTo(ms);
        var base64 = Convert.ToBase64String(ms.ToArray());

        var client = Build(url, model);
        await EnsureModelAsync(client, model);
        var responses = await client.ChatAsync(new()
        {
            Model = model,
            Messages = new[] { new Message(ChatRole.User, prompt, [base64]) },
            Options = new OllamaSharp.Models.RequestOptions { NumPredict = MaxTokens },
        }).ToListAsync();

        var text = string.Join("", responses.Select(r => r?.Message.Content ?? ""));
        TestContext.WriteLine($"model: {model}, image: {imageName}, prompt: {prompt}");
        TestContext.WriteLine(string.Join(Environment.NewLine, text.SplitBy(50)));

        Assert.IsFalse(string.IsNullOrWhiteSpace(text), "Response should not be empty");
    }

    [TestCategory(TestCategories.Integration)]
    [IntegrationTestMethod("oobd-test-ollama")]
    public async Task ListModelsTest()
    {
        var url = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("OLLAMA_URL"));
        var client = Build(url, TestContext.GetPropertyOrDefault("OLLAMA_MODEL", "phi3"));

        var models = await client.ListLocalModelsAsync();
        var modelsList = models.ToList();

        foreach (var localModel in modelsList)
            TestContext.WriteLine($"model: {localModel.Name} - {localModel.Size:#,##0} ({localModel.Digest})");

        Assert.IsGreaterThan(0, modelsList.Count, "At least one model should be available");
    }

    [TestCategory(TestCategories.Integration)]
    [IntegrationTestMethod("oobd-test-ollama")]
    public async Task PullModelTest_EmbeddingModel_IsListedAfterwardsAsync()
    {
        var url = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("OLLAMA_URL"));
        var model = TestContext.GetPropertyOrDefault("OLLAMA_EMBEDDING_MODEL", "all-minilm");
        var client = Build(url, model);

        await foreach (var _ in client.PullModelAsync(model))
        {
        }

        var models = await client.ListLocalModelsAsync();
        Assert.IsTrue(models.Any(m => m.Name.StartsWith(model, StringComparison.Ordinal)), $"{model} is listed");
    }

    [TestCategory(TestCategories.Integration)]
    [IntegrationTestMethod("oobd-test-ollama")]
    public async Task DeleteModelTest_CopyOfAModel_IsGoneAfterwardsAsync()
    {
        var url = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("OLLAMA_URL"));
        var source = TestContext.GetPropertyOrDefault("OLLAMA_EMBEDDING_MODEL", "all-minilm");
        var copy = $"test-model-{Guid.NewGuid():N}";
        var client = Build(url, source);
        await EnsureModelAsync(client, source);
        await client.CopyModelAsync(source, copy);

        await client.DeleteModelAsync(copy);

        var models = await client.ListLocalModelsAsync();
        Assert.IsFalse(models.Any(m => m.Name.StartsWith(copy, StringComparison.Ordinal)), "the copy was deleted");
    }

    private static async Task EnsureModelAsync(IOllamaApiClient client, string model)
    {
        var models = await client.ListLocalModelsAsync();
        if (models.Any(m => m.Name.StartsWith(model, StringComparison.Ordinal)))
        {
            return;
        }

        await foreach (var _ in client.PullModelAsync(model))
        {
        }
    }
}
