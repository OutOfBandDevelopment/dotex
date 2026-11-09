using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.GroqCloud;
using OoBDev.System.IO;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.LlmCodeGen.Cli.Tests;

/// <summary>
/// Runs the folder prompt runner against the real Groq Cloud API. Manual execution only: needs <c>GROQ_API_KEY</c>
/// (user environment variable or run parameter) and may incur cost.
/// </summary>
[TestClass]
public class GroqLiveRunTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.LiveIntegration)]
    public async Task RunAsync_SampleFolderAgainstGroq_WritesPromptAndResponseAsync()
    {
        // Stage
        var apiKey = TestContext.GetPropertyOrDefault("GROQ_API_KEY", string.Empty);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Assert.Inconclusive("GROQ_API_KEY is not set; see README.GroqCloud.Tests.md.");
        }

        var model = TestContext.GetPropertyOrDefault("GROQ_MODEL", new GroqCloudApiClientOptions().Model);
        using var temp = new TempDirectoryHandle();
        var root = temp.DirectoryPath;
        Directory.CreateDirectory(Path.Combine(root, "in"));
        File.WriteAllText(Path.Combine(root, "in", "a.txt"), "alpha");
        var template = Path.Combine(root, "t.hbs");
        File.WriteAllText(template, "Reply with one short sentence about this text: {{#each files}}{{content}}{{/each}}");

        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"{nameof(GroqCloudApiClientOptions)}:{nameof(GroqCloudApiClientOptions.ApiKey)}"] = apiKey,
            [$"{nameof(GroqCloudApiClientOptions)}:{nameof(GroqCloudApiClientOptions.Model)}"] = model,
        }).Build();
        using var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(config)
            .AddLogging()
            .TryAddGroqCloudServices(config, nameof(GroqCloudApiClientOptions))
            .BuildServiceProvider();
        var completion = LlmCodeGenService.ResolveCompletion(services, "groq");
        var runner = new FolderPromptRunner(completion, NullLogger<FolderPromptRunner>.Instance);
        var options = new LlmCodeGenOptions
        {
            InputPath = Path.Combine(root, "in"),
            OutputPath = Path.Combine(root, "out"),
            Template = template,
            Model = model,
        };

        // Test
        var sent = await runner.RunAsync(options, CancellationToken.None);

        // Assert
        Assert.AreEqual(1, sent);
        Assert.IsFalse(string.IsNullOrWhiteSpace(File.ReadAllText(Path.Combine(root, "out", "t.response.md"))), "the model answered");
        StringAssert.Contains(File.ReadAllText(Path.Combine(root, "out", "t.prompt.md")), "alpha", StringComparison.Ordinal);
    }
}
