using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AI.Models;
using OoBDev.TestUtilities;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AI.Tests;

[TestClass]
public sealed class LlmUsageTests
{
    private sealed class FakeCompletion : IMessageCompletion, IStreamedCompletion
    {
        public Task<string> GetCompletionAsync(string modelName, string prompt) => Task.FromResult("12345678");

        public Task<CompletionResponse> GetCompletionAsync(CompletionRequest model) =>
            Task.FromResult(new CompletionResponse { Context = [], Response = "abcd" });

        public async IAsyncEnumerable<string> GetStreamedResponseAsync(string promptDetails, string userInput, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return "abcd";
            await Task.Yield();
            yield return "efgh";
            yield return "i";
        }
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(null, 0)]
    [DataRow("", 0)]
    [DataRow("a", 1)]
    [DataRow("abcd", 1)]
    [DataRow("abcde", 2)]
    public void Of_RoundsCharactersOverFourUp(string? text, long expected) =>
        Assert.AreEqual(expected, TokenEstimate.Of(text));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task MessageCompletion_RecordsPerModel()
    {
        var meter = new LlmUsageMeter();
        var sut = new EstimatingMessageCompletion(new FakeCompletion(), meter);

        await sut.GetCompletionAsync("phi3", "12345678");
        await sut.GetCompletionAsync("phi3", "1234");
        await sut.GetCompletionAsync(new CompletionRequest { Prompt = "abcd", System = "ab" });

        var usage = meter.Snapshot();
        Assert.AreEqual(2, usage.Count);
        var phi = usage.Single(u => u.Model == "phi3");
        Assert.AreEqual(2, phi.Calls);
        Assert.AreEqual(3, phi.EstimatedPromptTokens);
        Assert.AreEqual(4, phi.EstimatedCompletionTokens);
        var unnamed = usage.Single(u => u.Model == "(default)");
        Assert.AreEqual(2, unnamed.EstimatedPromptTokens);
        Assert.AreEqual(1, unnamed.EstimatedCompletionTokens);
        Assert.AreEqual(3, unnamed.EstimatedTotalTokens);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task StreamedCompletion_RecordsWhenTheStreamEnds()
    {
        var meter = new LlmUsageMeter();
        var sut = new EstimatingStreamedCompletion(new FakeCompletion(), meter);

        var chunks = new List<string>();
        await foreach (var chunk in sut.GetStreamedResponseAsync("sys", "user"))
        {
            chunks.Add(chunk);
        }

        Assert.AreEqual(3, chunks.Count);
        var usage = meter.Snapshot().Single();
        Assert.AreEqual(1, usage.Calls);
        Assert.AreEqual(2, usage.EstimatedPromptTokens);
        Assert.AreEqual(3, usage.EstimatedCompletionTokens);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task StreamedCompletion_Abandoned_RecordsWhatArrived()
    {
        var meter = new LlmUsageMeter();
        var sut = new EstimatingStreamedCompletion(new FakeCompletion(), meter);

        await foreach (var _ in sut.GetStreamedResponseAsync("", ""))
        {
            break;
        }

        Assert.AreEqual(1, meter.Snapshot().Single().EstimatedCompletionTokens);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task TryAddEstimatedLlmUsage_WrapsRegisteredServicesOnceAndSharesTheMeter()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IMessageCompletion, FakeCompletion>();
        services.AddSingleton<IStreamedCompletion>(new FakeCompletion());
        services.TryAddEstimatedLlmUsage().TryAddEstimatedLlmUsage();

        using var provider = services.BuildServiceProvider();
        var completion = provider.GetRequiredService<IMessageCompletion>();
        await completion.GetCompletionAsync("m", "abcd");

        Assert.IsInstanceOfType<EstimatingMessageCompletion>(completion);
        Assert.IsInstanceOfType<EstimatingStreamedCompletion>(provider.GetRequiredService<IStreamedCompletion>());
        Assert.AreEqual(1, provider.GetRequiredService<LlmUsageMeter>().Snapshot().Single().Calls);
        Assert.AreEqual(1, services.Count(d => d.ServiceType == typeof(IMessageCompletion)));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryAddEstimatedLlmUsage_NothingRegistered_StillRegistersTheMeter()
    {
        var services = new ServiceCollection();
        services.TryAddEstimatedLlmUsage();

        using var provider = services.BuildServiceProvider();
        Assert.IsNotNull(provider.GetService<LlmUsageMeter>());
        Assert.IsNull(provider.GetService<IMessageCompletion>());
    }
}
