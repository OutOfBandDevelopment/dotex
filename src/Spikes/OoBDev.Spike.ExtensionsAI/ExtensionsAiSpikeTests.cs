using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AI;
using OoBDev.AI.Models;
using OoBDev.TestUtilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Spike.ExtensionsAI;

[TestClass]
public class ExtensionsAiSpikeTests
{
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task GetResponseAsync_SystemAndUser_MapsRolesAndReturnsText()
    {
        var chat = new FakeChatClient();
        ILanguageModelProvider sut = new ChatClientLanguageModelProvider(chat);

        var text = await sut.GetResponseAsync("be brief", "hello");

        Assert.AreEqual("answer", text);
        CollectionAssert.AreEqual(
            new[] { ChatRole.System, ChatRole.User },
            chat.LastMessages.Select(m => m.Role).ToArray());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task GetStreamedResponseAsync_TwoUpdates_YieldsEachPart()
    {
        ILanguageModelProvider sut = new ChatClientLanguageModelProvider(new FakeChatClient());

        var parts = new List<string>();
        await foreach (var part in sut.GetStreamedResponseAsync("s", "u"))
        {
            parts.Add(part);
        }

        CollectionAssert.AreEqual(new[] { "an", "swer" }, parts);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task GetRAGResponseCitiationsAsync_Sources_AreInSystemPrompt()
    {
        var chat = new FakeChatClient();
        ILanguageModelProvider sut = new ChatClientLanguageModelProvider(chat);

        await foreach (var _ in sut.GetRAGResponseCitiationsAsync([new KeyValuePairModel { Key = "doc1", Value = "fact" }], "q"))
        {
        }

        StringAssert.Contains(chat.LastMessages[0].Text, "[doc1] fact");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task GenerateEmbeddingAsync_ModelSupplied_PassesModelAndReturnsVector()
    {
        var generator = new FakeEmbeddingGenerator();
        IEmbeddingProvider sut = new EmbeddingGeneratorProvider(generator, 3);

        var vector = await sut.GenerateEmbeddingAsync("abcd", "all-minilm", CancellationToken.None);

        Assert.AreEqual("all-minilm", generator.LastModel);
        Assert.AreEqual(3, sut.Length);
        Assert.AreEqual(4f, vector.Span[0]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task KeyedRegistration_ChatClientAndGenerator_ResolveThroughOwnerContracts()
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton<IChatClient>("ollama", new FakeChatClient());
        services.AddKeyedSingleton<IEmbeddingGenerator<string, Embedding<float>>>("ollama", new FakeEmbeddingGenerator());
        services.AddKeyedSingleton<ILanguageModelProvider>("ollama", (sp, key) => new ChatClientLanguageModelProvider(
            sp.GetRequiredKeyedService<IChatClient>(key),
            sp.GetRequiredKeyedService<IEmbeddingGenerator<string, Embedding<float>>>(key)));

        using var provider = services.BuildServiceProvider();
        var sut = provider.GetRequiredKeyedService<ILanguageModelProvider>("ollama");

        Assert.AreEqual(3, (await sut.GetEmbeddedResponseAsync("abc")).Length);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task UseFunctionInvocation_Pipeline_BuildsOverPlatformClient()
    {
        // Platform middleware (logging, caching, telemetry, tool calling) composes without our own proxies.
        using var client = new ChatClientBuilder(new FakeChatClient())
            .UseFunctionInvocation()
            .Build();

        var response = await client.GetResponseAsync("hi");

        Assert.AreEqual("answer", response.Text);
    }
}
