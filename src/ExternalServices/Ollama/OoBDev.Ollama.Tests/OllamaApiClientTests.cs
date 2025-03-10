﻿using OoBDev.Extensions;
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

[TestClass]
public class OllamaApiClientTests
{
    public required TestContext TestContext { get; set; }

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

    [TestCategory(TestCategories.DevLocal)]
    [DataTestMethod]
    [DataRow("http://127.0.0.1:11434", "phi")]
    [DataRow("http://192.168.1.170:11434", "phi3")]
    [DataRow("http://192.168.1.170:11434", "all-minilm")]
    public async Task GenerateEmbeddingsDoubleTest(string hostName, string model)
    {
        var client = Build(hostName, model);
        var embedding = await client.GetEmbeddingDoubleAsync("Hello World!", model);
        TestContext.WriteLine($"hostName: {hostName}");
        TestContext.WriteLine($"model: {model}");
        TestContext.WriteLine($"Length: {embedding.Length}");
        TestContext.WriteLine(string.Join(", ", embedding));
    }

    [TestCategory(TestCategories.DevLocal)]
    [DataTestMethod]
    [DataRow("http://127.0.0.1:11434", "phi")]
    public async Task GenerateEmbeddingsSingleTest(string hostName, string model)
    {
        var client = Build(hostName, model);
        var embedding = await client.GetEmbeddingSingleAsync("Hello World!", model);

        TestContext.WriteLine(string.Join(", ", embedding));
    }

    [TestCategory(TestCategories.DevLocal)]
    [DataTestMethod]
    //[DataRow("http://127.0.0.1:11434", "phi", "Create a hello world script for windows command prompt")]
    [DataRow("http://192.168.1.170:11434", "llama2:7b", "tell me a story about a cat")]

    public async Task GetCompletionTest(string hostName, string model, string prompt)
    {
        var client = Build(hostName, model);
        var embedding = await client.ChatAsync(new()
        {
            Model = model,
            Messages = new[] { new Message(null, prompt) },

            Options = new OllamaSharp.Models.RequestOptions
            {
                Seed = 12312542,

                TopK = 100,
                TopP = 2,

                Temperature = 1f,
            },
        }).ToListAsync();

        TestContext.AddResult(embedding);

        TestContext.WriteLine(
            string.Join(
                Environment.NewLine,
                embedding.Select(s => s?.Message.Content?.SplitBy(50))
                ));
    }

    [TestCategory(TestCategories.DevLocal)]
    [DataTestMethod]
    //[DataRow("http://127.0.0.1:11434", "llava-phi3", "describe the image", "LadyDancingWithDog.jpg")]
    //[DataRow("http://127.0.0.1:11434", "llava-phi3", "describe the image", "RobotsTalking.jpg")]

    //[DataRow("http://192.168.1.170:11434", "llava:7b", "describe the image", "LadyDancingWithDog.jpg")]
    //[DataRow("http://192.168.1.170:11434", "llava:7b", "describe the image", "RobotsTalking.jpg")]
    //[DataRow("http://192.168.1.170:11434", "llava:7b", "describe the image in less than 10 words", "RobotsTalking.jpg")]
    //[DataRow("http://192.168.1.170:11434", "llava:7b", "describe the image and what color are the characters", "RobotsTalking.jpg")]
    //[DataRow("http://192.168.1.170:11434", "llava:7b", "list items found in this image as json", "LadyDancingWithDog.jpg")]
    //[DataRow("http://192.168.1.170:11434", "llava:7b", "what is the left robot doing with its left hand", "RobotsTalking.jpg")]

    [DataRow("http://192.168.1.170:11434", "llava-phi3", "describe the image", "LadyDancingWithDog.jpg")]
    [DataRow("http://192.168.1.170:11434", "llava-phi3", "describe the image", "RobotsTalking.jpg")]
    [DataRow("http://192.168.1.170:11434", "llava-phi3", "describe the image in less than 10 words", "RobotsTalking.jpg")]
    [DataRow("http://192.168.1.170:11434", "llava-phi3", "describe the image and what color are the characters", "RobotsTalking.jpg")]
    [DataRow("http://192.168.1.170:11434", "llava-phi3", "list items found in this image as json", "LadyDancingWithDog.jpg")]
    [DataRow("http://192.168.1.170:11434", "llava-phi3", "what is the left robot doing with its left hand", "RobotsTalking.jpg")]

    //[DataRow("http://192.168.1.170:11434", "bakllava", "describe the image", "LadyDancingWithDog.jpg")]
    //[DataRow("http://192.168.1.170:11434", "bakllava", "describe the image", "RobotsTalking.jpg")]
    //[DataRow("http://192.168.1.170:11434", "bakllava", "describe the image in less than 10 words", "RobotsTalking.jpg")]
    //[DataRow("http://192.168.1.170:11434", "bakllava", "describe the image and what color are the characters", "RobotsTalking.jpg")]
    //[DataRow("http://192.168.1.170:11434", "bakllava", "list items found in this image as json", "LadyDancingWithDog.jpg")]
    //[DataRow("http://192.168.1.170:11434", "bakllava", "what is the left robot doing with its left hand", "RobotsTalking.jpg")]

    public async Task GetCompletionWithImageTest(string hostName, string model, string prompt, string imageName)
    {
        using var img = GetType().Assembly.GetManifestResourceStream($"OoBDev.Ollama.Tests.TestData.{imageName}")!;
        using var ms = new MemoryStream();
        img.CopyTo(ms);

        var base64 = Convert.ToBase64String(ms.ToArray());

        TestContext.WriteLine($"hostName: {hostName}");
        TestContext.WriteLine($"model: {model}");
        TestContext.WriteLine($"prompt: {prompt}");
        TestContext.WriteLine($"imageName: {imageName}");

        //this.TestContext.WriteLine(new string('-', 80));
        //this.TestContext.WriteLine(string.Join(
        //        Environment.NewLine,
        //        base54.SplitBy(80)
        //        ));

        //TODO: fix this !
        var client = Build(hostName, model);
        var embedding = await client.ChatAsync(new()
        {
            Model = model,
            Messages = new[]
            {
                 new Message(ChatRole.User, prompt, [base64]){}
            },
            Options = new OllamaSharp.Models.RequestOptions
            {
            },
        }).ToListAsync();

        TestContext.WriteLine(new string('-', 80));
        TestContext.WriteLine(
            string.Join(
                Environment.NewLine,
                embedding.Select(s => s?.Message.Content?.SplitBy(50))
                ));
    }

    [TestCategory(TestCategories.DevLocal)]
    [DataTestMethod]
    [DataRow("http://127.0.0.1:11434")]
    [DataRow("http://192.168.1.170:11434")]
    public async Task ListModelsTest(string hostName)
    {
        var client = Build(hostName, "");
        foreach (var localModel in await client.ListLocalModelsAsync())
            TestContext.WriteLine($"model: {localModel.Name} - {localModel.Size:#,##0} ({localModel.Digest})");
    }

    [TestCategory(TestCategories.DevLocal)]
    [DataTestMethod]
    //[DataRow("http://127.0.0.1:11434", "phi")]
    [DataRow("http://127.0.0.1:11434", "phi3")]
    [DataRow("http://127.0.0.1:11434", "llava-phi3")]
    [DataRow("http://127.0.0.1:11434", "all-minilm")]

    [DataRow("http://192.168.1.170:11434", "llava-phi3")]
    //[DataRow("http://192.168.1.170:11434", "llava-llama3")]
    [DataRow("http://192.168.1.170:11434", "bakllava")]
    //[DataRow("http://192.168.1.170:11434", "llava:13b")]
    [DataRow("http://192.168.1.170:11434", "llava:7b")]

    //[DataRow("http://192.168.1.170:11434", "phi")]
    [DataRow("http://192.168.1.170:11434", "phi3")]
    [DataRow("http://192.168.1.170:11434", "llama3:8b")]
    [DataRow("http://192.168.1.170:11434", "llama2:7b")]
    //[DataRow("http://192.168.1.170:11434", "llama2:13b")]
    [DataRow("http://192.168.1.170:11434", "dolphin-llama3")]

    [DataRow("http://192.168.1.170:11434", "all-minilm")]
    public async Task PullModelTest(string hostName, string model)
    {
        var client = Build(hostName, model);
        double? last = default;

        await foreach(var ps in client.PullModelAsync(model))
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
    [DataTestMethod]
    //[DataRow("http://127.0.0.1:11434", "phi")]
    //[DataRow("http://127.0.0.1:11434", "phi3")]
    //[DataRow("http://127.0.0.1:11434", "llava-phi3")]
    //[DataRow("http://127.0.0.1:11434", "all-minilm")]

    //[DataRow("http://192.168.1.170:11434", "llava-phi3")]
    [DataRow("http://192.168.1.170:11434", "llava-llama3")]
    //[DataRow("http://192.168.1.170:11434", "bakllava")]
    [DataRow("http://192.168.1.170:11434", "llava:13b")]
    //[DataRow("http://192.168.1.170:11434", "llava:7b")]

    [DataRow("http://192.168.1.170:11434", "phi")]
    //[DataRow("http://192.168.1.170:11434", "phi3")]
    //[DataRow("http://192.168.1.170:11434", "llama3:8b")]
    //[DataRow("http://192.168.1.170:11434", "llama2:7b")]
    [DataRow("http://192.168.1.170:11434", "llama2:13b")]
    //[DataRow("http://192.168.1.170:11434", "dolphin-llama3")]

    [DataRow("http://192.168.1.170:11434", "all-minilm")]
    public async Task DeleteModel(string hostName, string model)
    {
        var client = Build(hostName, model);
        await client.DeleteModelAsync(model);
    }
}
