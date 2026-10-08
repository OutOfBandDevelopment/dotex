using Microsoft.Extensions.AI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Linq;
using System.Numerics.Tensors;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Onnx.SentenceEmbeddings.Tests;

/// <summary>
/// Run the real all-MiniLM-L6-v2 model from the checked-out submodule; inconclusive when it is absent.
/// </summary>
[TestClass]
public class GeneratorTests
{
    private static readonly string[] _corpus =
    [
        "The quick brown fox jumps over the lazy dog.",
        "Café déjà vu naïve résumé",
        "東京は日本の首都です",
        "emoji 🙂 and punctuation!?;:",
        "a",
        string.Join(' ', Enumerable.Repeat("long", 600)),
    ];

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public async Task Generate_Single_Returns384UnitVector()
    {
        using var generator = TestModel.CreateGenerator();

        var result = await generator.GenerateAsync(["hello world"]);

        Assert.AreEqual(384, generator.Dimensions);
        Assert.HasCount(1, result);
        Assert.AreEqual(384, result[0].Vector.Length);
        Assert.AreEqual(1f, TensorPrimitives.Norm(result[0].Vector.Span), 1e-4f);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public async Task Generate_Blank_ReturnsZeroVectorOfFullLength()
    {
        using var generator = TestModel.CreateGenerator();

        var result = await generator.GenerateAsync(["", "   ", "text"]);

        Assert.HasCount(3, result);
        Assert.AreEqual(384, result[0].Vector.Length);
        Assert.AreEqual(0f, TensorPrimitives.Norm(result[0].Vector.Span));
        Assert.AreEqual(0f, TensorPrimitives.Norm(result[1].Vector.Span));
        Assert.AreEqual(1f, TensorPrimitives.Norm(result[2].Vector.Span), 1e-4f);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public async Task Generate_InBatchOrAlone_GivesSameVector()
    {
        using var generator = TestModel.CreateGenerator();

        var batch = await generator.GenerateAsync(_corpus);
        for (var i = 0; i < _corpus.Length; i++)
        {
            var alone = await generator.GenerateAsync([_corpus[i]]);
            var similarity = TensorPrimitives.CosineSimilarity(alone[0].Vector.Span, batch[i].Vector.Span);
            Assert.IsGreaterThan(0.9999f, similarity, $"corpus item {i}");
        }
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public async Task Generate_SmallBatchSize_GivesSameResultAsLargeBatchSize()
    {
        using var large = TestModel.CreateGenerator();
        using var small = TestModel.CreateGenerator(o => o.MaxBatchSize = 2);

        var expected = await large.GenerateAsync(_corpus);
        var actual = await small.GenerateAsync(_corpus);

        for (var i = 0; i < _corpus.Length; i++)
            Assert.IsGreaterThan(0.9999f, TensorPrimitives.CosineSimilarity(expected[i].Vector.Span, actual[i].Vector.Span));
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public async Task Generate_ManyThreads_AllGetTheSerialResult()
    {
        using var generator = TestModel.CreateGenerator(o => o.MaxConcurrentInferences = 4);
        var serial = await generator.GenerateAsync(_corpus);

        var runs = await Task.WhenAll(Enumerable.Range(0, 32).Select(_ => Task.Run(() => generator.GenerateAsync(_corpus))));

        foreach (var run in runs)
            for (var i = 0; i < _corpus.Length; i++)
                Assert.IsGreaterThan(0.99999f, TensorPrimitives.CosineSimilarity(serial[i].Vector.Span, run[i].Vector.Span));
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public async Task Generate_Cancelled_Throws()
    {
        using var generator = TestModel.CreateGenerator();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() => generator.GenerateAsync(["text"], cancellationToken: cts.Token));
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void GetService_ReportsMetadata()
    {
        using var generator = TestModel.CreateGenerator();

        var metadata = generator.GetService(typeof(EmbeddingGeneratorMetadata)) as EmbeddingGeneratorMetadata;

        Assert.IsNotNull(metadata);
        Assert.AreEqual(384, metadata.DefaultModelDimensions);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Create_DimensionsWithoutMatryoshka_Throws() =>
        Assert.Throws<ArgumentException>(() => TestModel.CreateGenerator(o => o.Dimensions = 128));

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public async Task Generate_TruncatedDimensions_AreReNormalised()
    {
        using var generator = TestModel.CreateGenerator(o => { o.SupportsDimensionTruncation = true; o.Dimensions = 128; });

        var result = await generator.GenerateAsync(["hello world"]);

        Assert.AreEqual(128, result[0].Vector.Length);
        Assert.AreEqual(1f, TensorPrimitives.Norm(result[0].Vector.Span), 1e-4f);
    }
}
