using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.SBert.AllMpnetBaseV2;
using OoBDev.SBert.NomicEmbedTextV1_5;
using OoBDev.TestUtilities;
using System;
using System.IO;
using System.Linq;
using System.Numerics.Tensors;
using System.Text.Json;
using System.Threading.Tasks;

namespace OoBDev.Onnx.SentenceEmbeddings.Tests;

/// <summary>
/// Compares the model presets with vectors from the original Hugging Face models
/// (scripts/embeddings/make-reference-vectors.py). The model files (hundreds of MB) are downloaded into the shared
/// Hugging Face cache on first run.
/// </summary>
[TestClass]
public class PresetReferenceTests
{
    public TestContext TestContext { get; set; } = null!;

    private sealed record Item(string Text, int Tokens, float[] Vector, float[]? Vector256);

    private static Item[] Load(string name)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", name + ".reference.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return [.. document.RootElement.GetProperty("items").EnumerateArray().Select(item => new Item(
            item.GetProperty("text").GetString()!,
            item.GetProperty("tokens").GetInt32(),
            [.. item.GetProperty("vector").EnumerateArray().Select(v => v.GetSingle())],
            item.TryGetProperty("vector256", out var small) ? [.. small.EnumerateArray().Select(v => v.GetSingle())] : null))];
    }

    private static string Describe(string text) => (text.Length > 40 ? text[..40] + "..." : text).Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ');

    private async Task AssertMatches(string reference, OnnxSentenceEmbeddingOptions options, Func<OnnxSentenceEmbeddingOptions, Task> ensure, Func<Item, float[]> expected, int dimensions)
    {
        try
        {
            await ensure(options);
        }
        catch (global::System.Net.Http.HttpRequestException ex)
        {
            Assert.Inconclusive("The model could not be downloaded: " + ex.Message);
        }

        var items = Load(reference);
        using var generator = new OnnxSentenceEmbeddingGenerator(Microsoft.Extensions.Options.Options.Create(options), NullLogger<OnnxSentenceEmbeddingGenerator>.Instance);
        Assert.AreEqual(dimensions, generator.Dimensions);

        var actual = await generator.GenerateAsync(items.Select(i => i.Text).ToArray());

        var scores = items.Select((item, i) => (item, similarity: TensorPrimitives.CosineSimilarity(expected(item), actual[i].Vector.Span))).ToArray();
        var failures = scores
            .Where(x => x.similarity < 0.999f)
            .Select(x => $"{Describe(x.item.Text)} tokens={x.item.Tokens} cos={x.similarity:F4}")
            .ToArray();
        TestContext.WriteLine($"items={items.Length} min cosine={scores.Min(x => x.similarity):F5} below 0.999: {failures.Length}");
        foreach (var failure in failures) TestContext.WriteLine(failure);
        Assert.IsEmpty(failures, string.Join("; ", failures));
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    public async Task AllMpnetBaseV2_MatchesReference()
    {
        var options = new OnnxSentenceEmbeddingOptions();
        AllMpnetBaseV2Model.ApplyDefaults(options);

        await AssertMatches("all-mpnet-base-v2", options, o => AllMpnetBaseV2Model.EnsureAsync(o, NullLogger.Instance), i => i.Vector, 768);
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    public async Task NomicEmbedTextV1_5_MatchesReference()
    {
        var options = new OnnxSentenceEmbeddingOptions();
        NomicEmbedTextV1_5Model.ApplyDefaults(options);
        options.MaxSequenceLength = 512; // the reference run cuts at 512 tokens

        await AssertMatches("nomic-embed-text-v1.5", options, o => NomicEmbedTextV1_5Model.EnsureAsync(o, NullLogger.Instance), i => i.Vector, 768);
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    public async Task NomicEmbedTextV1_5_Truncated256_MatchesReference()
    {
        var options = new OnnxSentenceEmbeddingOptions();
        NomicEmbedTextV1_5Model.ApplyDefaults(options);
        options.Dimensions = 256;

        await AssertMatches("nomic-embed-text-v1.5", options, o => NomicEmbedTextV1_5Model.EnsureAsync(o, NullLogger.Instance), i => i.Vector256!, 256);
    }
}
