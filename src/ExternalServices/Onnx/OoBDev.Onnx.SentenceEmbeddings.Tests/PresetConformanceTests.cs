using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.SBert.AllMpnetBaseV2;
using OoBDev.SBert.NomicEmbedTextV1_5;
using OoBDev.TestUtilities;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Numerics.Tensors;
using System.Threading.Tasks;

namespace OoBDev.Onnx.SentenceEmbeddings.Tests;

/// <summary>
/// Behaviour every model preset must show, whatever the model: vector size and scale, repeatability, batch and padding
/// independence, blank and empty input, truncation, concurrency and sensible similarity ordering.
/// The model files (hundreds of MB) are downloaded into the shared Hugging Face cache on first run.
/// </summary>
[TestClass]
public class PresetConformanceTests
{
    private const string Mpnet = "mpnet";
    private const string Nomic = "nomic";

    private static readonly ConcurrentDictionary<string, Lazy<OnnxSentenceEmbeddingGenerator>> _shared = new();

    private static OnnxSentenceEmbeddingOptions Options(string model)
    {
        var options = new OnnxSentenceEmbeddingOptions();
        if (model == Mpnet) AllMpnetBaseV2Model.ApplyDefaults(options);
        else NomicEmbedTextV1_5Model.ApplyDefaults(options);
        return options;
    }

    private static OnnxSentenceEmbeddingGenerator Create(string model, Action<OnnxSentenceEmbeddingOptions>? configure = null)
    {
        var options = Options(model);
        configure?.Invoke(options);
        try
        {
            if (model == Mpnet) AllMpnetBaseV2Model.EnsureAsync(options, NullLogger.Instance).GetAwaiter().GetResult();
            else NomicEmbedTextV1_5Model.EnsureAsync(options, NullLogger.Instance).GetAwaiter().GetResult();
        }
        catch (global::System.Net.Http.HttpRequestException ex)
        {
            Assert.Inconclusive("The model could not be downloaded: " + ex.Message);
        }
        return new OnnxSentenceEmbeddingGenerator(Microsoft.Extensions.Options.Options.Create(options), NullLogger<OnnxSentenceEmbeddingGenerator>.Instance);
    }

    /// <summary>One generator per model for the checks that do not change options (the model session is expensive to load).</summary>
    private static OnnxSentenceEmbeddingGenerator Shared(string model) =>
        _shared.GetOrAdd(model, key => new Lazy<OnnxSentenceEmbeddingGenerator>(() => Create(key))).Value;

    [ClassCleanup]
    public static void Cleanup()
    {
        foreach (var generator in _shared.Values.Where(l => l.IsValueCreated)) generator.Value.Dispose();
        _shared.Clear();
    }

    private static async Task<float[]> Embed(OnnxSentenceEmbeddingGenerator generator, string text) =>
        (await generator.GenerateAsync([text]))[0].Vector.ToArray();

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    [DataRow(Mpnet)]
    [DataRow(Nomic)]
    public async Task Vector_HasModelSizeAndUnitLength(string model)
    {
        var generator = Shared(model);

        var vector = await Embed(generator, "The quick brown fox jumps over the lazy dog.");

        Assert.AreEqual(768, vector.Length);
        Assert.AreEqual(768, generator.Dimensions);
        Assert.AreEqual(1f, TensorPrimitives.Norm(vector), 1e-4f);
        Assert.IsTrue(vector.All(float.IsFinite));
        Assert.AreEqual(768, generator.GetService<EmbeddingGeneratorMetadata>()!.DefaultModelDimensions);
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    [DataRow(Mpnet)]
    [DataRow(Nomic)]
    public async Task Vector_IsRepeatable(string model)
    {
        var generator = Shared(model);

        var first = await Embed(generator, "How do I reset my password?");
        var second = await Embed(generator, "How do I reset my password?");

        Assert.IsGreaterThan(0.99999f, TensorPrimitives.CosineSimilarity(first, second));
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    [DataRow(Mpnet)]
    [DataRow(Nomic)]
    public async Task Batch_MatchesSingleCalls_RegardlessOfPadding(string model)
    {
        var generator = Shared(model);
        string[] texts =
        [
            "a",
            "A man is eating a piece of bread.",
            string.Join(' ', Enumerable.Repeat("Sentence embeddings map text to dense vectors.", 40)),
            "Café déjà vu naïve résumé",
            "東京は日本の首都です",
        ];

        var batch = await generator.GenerateAsync(texts);

        for (var i = 0; i < texts.Length; i++)
        {
            var single = await Embed(generator, texts[i]);
            var similarity = TensorPrimitives.CosineSimilarity(single, batch[i].Vector.Span);
            Assert.IsGreaterThan(0.9999f, similarity, $"item {i} cos={similarity:F5}");
        }
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    [DataRow(Mpnet)]
    [DataRow(Nomic)]
    public async Task Batch_LargerThanMaxBatchSize_KeepsOrder(string model)
    {
        var generator = Create(model, o => o.MaxBatchSize = 2);
        using var _ = generator;
        string[] texts = ["cats", "the stock market fell", "baking sourdough bread", "a football match", "quantum computing"];

        var batch = await generator.GenerateAsync(texts);
        var reference = await Shared(model).GenerateAsync(texts);

        Assert.HasCount(texts.Length, batch);
        for (var i = 0; i < texts.Length; i++)
            Assert.IsGreaterThan(0.9999f, TensorPrimitives.CosineSimilarity(reference[i].Vector.Span, batch[i].Vector.Span), $"item {i}");
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    [DataRow(Mpnet)]
    [DataRow(Nomic)]
    public async Task BlankInput_ReturnsZeroVectorOfFullLength(string model)
    {
        var generator = Shared(model);

        var result = await generator.GenerateAsync(["   ", ""]);

        foreach (var embedding in result)
        {
            Assert.AreEqual(768, embedding.Vector.Length);
            Assert.AreEqual(0f, TensorPrimitives.Norm(embedding.Vector.Span));
        }
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    [DataRow(Mpnet)]
    [DataRow(Nomic)]
    public async Task EmptyBatch_ReturnsNothing(string model)
    {
        var result = await Shared(model).GenerateAsync([]);

        Assert.IsEmpty(result);
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    [DataRow(Mpnet)]
    [DataRow(Nomic)]
    public async Task LongInput_IsTruncatedWithoutFailing(string model)
    {
        var generator = Shared(model);
        var longText = string.Join(' ', Enumerable.Repeat("Sentence embeddings power semantic search and retrieval.", 2000));

        var vector = await Embed(generator, longText);

        Assert.AreEqual(1f, TensorPrimitives.Norm(vector), 1e-4f);
        Assert.IsTrue(vector.All(float.IsFinite));
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    [DataRow(Mpnet)]
    [DataRow(Nomic)]
    public async Task ModelSpecialTokensInText_DoNotFail(string model)
    {
        var generator = Shared(model);

        var vector = await Embed(generator, "Markers like <s> </s> <pad> <mask> [CLS] [SEP] [UNK] [PAD] [MASK] are plain text to the caller.");

        Assert.AreEqual(1f, TensorPrimitives.Norm(vector), 1e-4f);
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    [DataRow(Mpnet)]
    [DataRow(Nomic)]
    public async Task ParallelCalls_ReturnTheSameVectors(string model)
    {
        var generator = Shared(model);
        string[] texts = ["alpha", "bravo charlie", "delta echo foxtrot", "golf", "hotel india juliet kilo"];
        var expected = await generator.GenerateAsync(texts);

        var results = await Task.WhenAll(Enumerable.Range(0, 24).Select(i => generator.GenerateAsync([texts[i % texts.Length]])));

        for (var i = 0; i < results.Length; i++)
            Assert.IsGreaterThan(0.9999f, TensorPrimitives.CosineSimilarity(expected[i % texts.Length].Vector.Span, results[i][0].Vector.Span), $"call {i}");
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    [DataRow(Mpnet)]
    [DataRow(Nomic)]
    public async Task Similarity_RelatedTextsScoreHigherThanUnrelated(string model)
    {
        var generator = Shared(model);

        var result = await generator.GenerateAsync(
        [
            "A man is eating food.",
            "A man is eating a piece of bread.",
            "A cheetah is running behind its prey.",
        ]);

        var related = TensorPrimitives.CosineSimilarity(result[0].Vector.Span, result[1].Vector.Span);
        var unrelated = TensorPrimitives.CosineSimilarity(result[0].Vector.Span, result[2].Vector.Span);
        Assert.IsGreaterThan(unrelated + 0.1f, related, $"related={related:F3} unrelated={unrelated:F3}");
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    public async Task Nomic_TaskPrefixChangesTheVector_AndQueryFindsItsDocument()
    {
        using var query = Create(Nomic, o => o.Prefix = "search_query: ");
        var document = Shared(Nomic);
        const string text = "Paris is the capital and most populous city of France.";

        var asDocument = await Embed(document, text);
        var asQuery = await Embed(query, text);
        var question = await Embed(query, "What is the capital of France?");
        var other = await Embed(document, "A cheetah is running behind its prey.");

        Assert.IsLessThan(0.9999f, TensorPrimitives.CosineSimilarity(asDocument, asQuery), "the prefix must reach the model");
        Assert.IsGreaterThan(
            TensorPrimitives.CosineSimilarity(question, other),
            TensorPrimitives.CosineSimilarity(question, asDocument));
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    [DataRow(512)]
    [DataRow(256)]
    [DataRow(128)]
    [DataRow(64)]
    public async Task Nomic_MatryoshkaSizes_AreUnitLengthPrefixesOfTheFullVector(int dimensions)
    {
        using var small = Create(Nomic, o => o.Dimensions = dimensions);
        const string text = "Matryoshka embeddings can be cut to fewer dimensions.";

        var full = await Embed(Shared(Nomic), text);
        var cut = await Embed(small, text);

        Assert.AreEqual(dimensions, cut.Length);
        Assert.AreEqual(1f, TensorPrimitives.Norm(cut), 1e-4f);
        // layer norm happens before the cut and scaling cancels in cosine, so the cut equals the prefix of the full vector
        Assert.IsGreaterThan(0.9999f, TensorPrimitives.CosineSimilarity(full.AsSpan(0, dimensions), cut));
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    public void Mpnet_DimensionsOption_IsRejectedBecauseNotMatryoshka()
    {
        var options = Options(Mpnet);
        options.Dimensions = 256;

        var ex = Assert.Throws<ArgumentException>(options.Validate);

        Assert.AreEqual(nameof(OnnxSentenceEmbeddingOptions.Dimensions), ex.ParamName);
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    public void Mpnet_SequenceLimit_DefaultsToTheOriginals384()
    {
        Assert.AreEqual(384, Options(Mpnet).MaxSequenceLength);
        Assert.AreEqual(512, Options(Nomic).MaxSequenceLength);
    }
}
