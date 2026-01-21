using OoBDev.Extensions;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OllamaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OllamaSharp.Models.Chat;
using OoBDev.Extensions.Linq;

namespace OoBDev.Ollama.Tests;

/// <summary>
/// Tests for OllamaApiClient functionality.
/// Integration tests use TestContext properties (OLLAMA_URL, OLLAMA_MODEL) and run against Docker Ollama.
/// DevLocal tests are for manual/exploratory testing with specific models and configurations.
/// </summary>
[TestClass]
public class OllamaApiClientTests
{
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

        var url = context.GetRequiredProperty<string>("OLLAMA_URL");
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
    [TestMethod]
    public async Task GenerateEmbeddingsDoubleTest()
    {
        var url = TestContext.GetRequiredProperty<string>("OLLAMA_URL");
        var model = TestContext.GetPropertyOrDefault("OLLAMA_MODEL", "phi3");

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
    [TestMethod]
    public async Task GenerateEmbeddingsSingleTest()
    {
        var url = TestContext.GetRequiredProperty<string>("OLLAMA_URL");
        var model = TestContext.GetPropertyOrDefault("OLLAMA_MODEL", "phi3");

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
    [TestMethod]
    public async Task ChatAsyncTest()
    {
        var url = TestContext.GetRequiredProperty<string>("OLLAMA_URL");
        var model = TestContext.GetPropertyOrDefault("OLLAMA_MODEL", "phi3");

        var client = Build(url, model);
        var responses = await client.ChatAsync(new()
        {
            Model = model,
            Messages = new[] { new Message(ChatRole.User, "Say 'Hello' in one word.") },
            Options = new OllamaSharp.Models.RequestOptions
            {
                Seed = 12345,
                Temperature = 0.1f,
            },
        }).ToListAsync();

        TestContext.WriteLine($"url: {url}");
        TestContext.WriteLine($"model: {model}");
        TestContext.WriteLine($"Response count: {responses.Count}");

        var fullResponse = string.Join("", responses.Select(r => r?.Message.Content ?? ""));
        TestContext.WriteLine($"Full response: {fullResponse}");

        Assert.IsTrue(responses.Any(), "Should receive at least one response");
        Assert.IsFalse(string.IsNullOrWhiteSpace(fullResponse), "Response should not be empty");
    }

    [TestCategory(TestCategories.DevLocal)]
    [TestMethod]
    [DataRow("http://192.168.1.170:11434", "llama2:7b", "tell me a story about a cat")]
    public async Task ChatAsyncTest_DevLocal(string hostName, string model, string prompt)
    {
        var client = Build(hostName, model);
        var responses = await client.ChatAsync(new()
        {
            Model = model,
            Messages = new[] { new Message(ChatRole.User, prompt) },
            Options = new OllamaSharp.Models.RequestOptions
            {
                Seed = 12312542,
                TopK = 100,
                TopP = 2,
                Temperature = 1f,
            },
        }).ToListAsync();

        TestContext.AddResult(responses);

        TestContext.WriteLine(
            string.Join(
                Environment.NewLine,
                responses.Select(s => s?.Message.Content?.SplitBy(50))
                ));
    }

    [TestCategory(TestCategories.DevLocal)]
    [TestMethod]
    [DataRow("http://192.168.1.170:11434", "llava-phi3", "describe the image", "LadyDancingWithDog.jpg")]
    [DataRow("http://192.168.1.170:11434", "llava-phi3", "describe the image", "RobotsTalking.jpg")]
    [DataRow("http://192.168.1.170:11434", "llava-phi3", "describe the image in less than 10 words", "RobotsTalking.jpg")]
    [DataRow("http://192.168.1.170:11434", "llava-phi3", "describe the image and what color are the characters", "RobotsTalking.jpg")]
    [DataRow("http://192.168.1.170:11434", "llava-phi3", "list items found in this image as json", "LadyDancingWithDog.jpg")]
    [DataRow("http://192.168.1.170:11434", "llava-phi3", "what is the left robot doing with its left hand", "RobotsTalking.jpg")]
    public async Task ChatWithVisionTest_DevLocal(string hostName, string model, string prompt, string imageName)
    {
        using var img = GetType().Assembly.GetManifestResourceStream($"OoBDev.Ollama.Tests.TestData.{imageName}")!;
        using var ms = new MemoryStream();
        img.CopyTo(ms);

        var base64 = Convert.ToBase64String(ms.ToArray());

        TestContext.WriteLine($"hostName: {hostName}");
        TestContext.WriteLine($"model: {model}");
        TestContext.WriteLine($"prompt: {prompt}");
        TestContext.WriteLine($"imageName: {imageName}");

        var client = Build(hostName, model);
        var responses = await client.ChatAsync(new()
        {
            Model = model,
            Messages = new[]
            {
                 new Message(ChatRole.User, prompt, [base64])
            },
            Options = new OllamaSharp.Models.RequestOptions(),
        }).ToListAsync();

        TestContext.WriteLine(new string('-', 80));
        TestContext.WriteLine(
            string.Join(
                Environment.NewLine,
                responses.Select(s => s?.Message.Content?.SplitBy(50))
                ));
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    public async Task ListModelsTest()
    {
        var url = TestContext.GetRequiredProperty<string>("OLLAMA_URL");
        var client = Build(url, "");

        var models = await client.ListLocalModelsAsync();
        var modelsList = models.ToList();

        foreach (var localModel in modelsList)
            TestContext.WriteLine($"model: {localModel.Name} - {localModel.Size:#,##0} ({localModel.Digest})");

        Assert.IsTrue(modelsList.Any(), "At least one model should be available");
    }

    [TestCategory(TestCategories.DevLocal)]
    [TestMethod]
    [DataRow("http://192.168.1.170:11434", "phi3")]
    [DataRow("http://192.168.1.170:11434", "llava-phi3")]
    [DataRow("http://192.168.1.170:11434", "all-minilm")]
    public async Task PullModelTest_DevLocal(string hostName, string model)
    {
        var client = Build(hostName, model);
        double? last = default;

        await foreach (var ps in client.PullModelAsync(model))
        {
            if (ps == null) continue;
            if (ps.Percent != last)
            {
                Debug.WriteLine($"{model}: Pulled: {ps.Percent}% / {ps.Status} / {ps.Total:#,##0}");
                last = ps.Percent;
            }
        }
    }

    [TestCategory(TestCategories.DevLocal)]
    [TestMethod]
    [DataRow("http://192.168.1.170:11434", "test-model")]
    public async Task DeleteModelTest_DevLocal(string hostName, string model)
    {
        var client = Build(hostName, model);
        await client.DeleteModelAsync(model);
    }
}
