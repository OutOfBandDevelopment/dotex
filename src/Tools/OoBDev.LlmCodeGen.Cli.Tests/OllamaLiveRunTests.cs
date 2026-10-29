using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AI;
using OoBDev.Ollama;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.LlmCodeGen.Cli.Tests;

[TestClass]
public class OllamaLiveRunTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task RunAsync_SampleFolderAgainstOllama_WritesPromptAndResponseAsync()
    {
        // Stage
        var url = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("OLLAMA_URL"));
        var model = TestContext.GetPropertyOrDefault("OLLAMA_MODEL", "phi3");
        var root = Path.Combine(Path.GetTempPath(), "llmcodegen-live-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(Path.Combine(root, "in"));
            File.WriteAllText(Path.Combine(root, "in", "a.txt"), "alpha");
            var template = Path.Combine(root, "t.hbs");
            File.WriteAllText(template, "Reply with one short sentence about this text: {{#each files}}{{content}}{{/each}}");

            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OllamaApiClientOptions:Url"] = url,
                ["OllamaApiClientOptions:DefaultModel"] = model,
            }).Build();
            using var services = new ServiceCollection()
                .AddSingleton<IConfiguration>(config)
                .AddLogging()
                .TryAddOllamaServices(config, nameof(OllamaApiClientOptions))
                .BuildServiceProvider();
            var completion = LlmCodeGenService.ResolveCompletion(services, "ollama");
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
        finally
        {
            Directory.Delete(root, true);
        }
    }
}
