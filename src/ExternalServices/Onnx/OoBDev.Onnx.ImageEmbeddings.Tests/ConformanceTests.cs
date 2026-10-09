using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Onnx.ImageEmbeddings.Skia;
using OoBDev.TestUtilities;
using OoBDev.Vision.ClipVitB32;
using OoBDev.Vision.Dinov2Small;
using OoBDev.Vision.VitBasePatch16;
using System;
using System.IO;
using System.Linq;
using System.Numerics.Tensors;
using System.Threading.Tasks;

namespace OoBDev.Onnx.ImageEmbeddings.Tests;

/// <summary>
/// Behavior every image preset must show: repeatable, batch equals single, batches above the model batch size,
/// parallel callers, and clear errors for bad input. Models come from the shared Hugging Face cache.
/// </summary>
[TestClass]
public class ConformanceTests
{
    private static DataContent Image(string file) =>
        new(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "TestData", "images", file)), file.EndsWith(".jpg", StringComparison.Ordinal) ? "image/jpeg" : "image/png");

    private static readonly string[] Files = [.. Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "TestData", "images")).Select(Path.GetFileName).Cast<string>().Order()];

    private static async Task<OnnxImageEmbeddingGenerator> CreateAsync(string preset, Action<OnnxImageEmbeddingOptions>? tweak = null)
    {
        var options = new OnnxImageEmbeddingOptions();
        switch (preset)
        {
            case "dinov2": Dinov2SmallModel.ApplyDefaults(options); break;
            case "clip": ClipVitB32Model.ApplyDefaults(options); break;
            default: throw new ArgumentOutOfRangeException(nameof(preset));
        }
        options.MaxBatchSize = 4; // below the 18 test images
        tweak?.Invoke(options);
        try
        {
            await OnnxImageEmbeddingGenerator.EnsureModelAsync(options, NullLogger.Instance);
        }
        catch (global::System.Net.Http.HttpRequestException ex)
        {
            Assert.Inconclusive("The model could not be downloaded: " + ex.Message);
        }
        return new OnnxImageEmbeddingGenerator(Microsoft.Extensions.Options.Options.Create(options), new SkiaImageDecoder(), NullLogger<OnnxImageEmbeddingGenerator>.Instance);
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    [DataRow("dinov2")]
    [DataRow("clip")]
    public async Task Generate_BatchEqualsSingleAndIsRepeatable(string preset)
    {
        using var generator = await CreateAsync(preset);
        var images = Files.Select(Image).ToArray();

        var batch = await generator.GenerateAsync(images);
        var again = await generator.GenerateAsync(images);
        Assert.HasCount(images.Length, batch);
        for (var i = 0; i < images.Length; i++)
        {
            var single = (await generator.GenerateAsync([images[i]]))[0].Vector;
            Assert.AreEqual(generator.Dimensions, batch[i].Vector.Length);
            Assert.IsGreaterThan(0.9999f, TensorPrimitives.CosineSimilarity(batch[i].Vector.Span, single.Span), Files[i]);
            Assert.IsGreaterThan(0.99999f, TensorPrimitives.CosineSimilarity(batch[i].Vector.Span, again[i].Vector.Span), Files[i]);
        }
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    [DataRow("dinov2")]
    [DataRow("clip")]
    public async Task Generate_ParallelCallersGetTheirOwnResults(string preset)
    {
        using var generator = await CreateAsync(preset, o => o.MaxConcurrentInferences = 2);
        var expected = await generator.GenerateAsync(Files.Take(6).Select(Image).ToArray());

        var results = await Task.WhenAll(Enumerable.Range(0, 8).Select(n => Task.Run(async () =>
        {
            var index = n % 6;
            return (index, vector: (await generator.GenerateAsync([Image(Files[index])]))[0].Vector);
        })));
        foreach (var (index, vector) in results)
            Assert.IsGreaterThan(0.9999f, TensorPrimitives.CosineSimilarity(expected[index].Vector.Span, vector.Span), Files[index]);
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    public async Task Generate_EmptyInputGivesEmptyResult()
    {
        using var generator = await CreateAsync("dinov2");
        Assert.IsEmpty(await generator.GenerateAsync(Array.Empty<DataContent>()));
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    public async Task Generate_RejectsBadInput()
    {
        using var generator = await CreateAsync("dinov2");
        await Assert.ThrowsAsync<ArgumentException>(() => generator.GenerateAsync([new DataContent(new byte[] { 1, 2, 3 }, "text/plain")]));
        await Assert.ThrowsAsync<ArgumentException>(() => generator.GenerateAsync([new DataContent(ReadOnlyMemory<byte>.Empty, "image/png")]));
        await Assert.ThrowsAsync<InvalidDataException>(() => generator.GenerateAsync([new DataContent(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, "image/png")]));
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    public async Task Generate_EnforcesSizeLimits()
    {
        using var small = await CreateAsync("dinov2", o => o.MaxPixels = 1000);
        await Assert.ThrowsAsync<InvalidDataException>(() => small.GenerateAsync([Image("scene-640x480.png")]));

        using var bytes = await CreateAsync("dinov2", o => o.MaxImageBytes = 100);
        await Assert.ThrowsAsync<ArgumentException>(() => bytes.GenerateAsync([Image("scene-640x480.png")]));
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    public async Task Generate_HonorsCancellation()
    {
        using var generator = await CreateAsync("dinov2");
        using var cts = new global::System.Threading.CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => generator.GenerateAsync([Image("scene-640x480.png")], cancellationToken: cts.Token));
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    public async Task Classifier_ProbabilitiesAreOrderedAndBounded()
    {
        var options = new OnnxImageEmbeddingOptions();
        VitBasePatch16Model.ApplyDefaults(options);
        string folder;
        try
        {
            folder = await VitBasePatch16Model.EnsureAsync(options, NullLogger.Instance);
        }
        catch (global::System.Net.Http.HttpRequestException ex)
        {
            Assert.Inconclusive("The model could not be downloaded: " + ex.Message);
            return;
        }
        using var classifier = new OnnxImageClassifier(Microsoft.Extensions.Options.Options.Create(options), VitBasePatch16Model.LoadLabels(folder), new SkiaImageDecoder(), NullLogger<OnnxImageClassifier>.Instance);

        var top = await classifier.ClassifyAsync(Image("scene-640x480.png"), 5);
        Assert.HasCount(5, top);
        CollectionAssert.AreEqual(top.OrderByDescending(t => t.Probability).ToArray(), top.ToArray());
        Assert.IsTrue(top.All(t => t.Probability is >= 0f and <= 1f));
        Assert.IsLessThanOrEqualTo(1.0001f, top.Sum(t => t.Probability));
        await Assert.ThrowsAsync<ArgumentException>(() => classifier.ClassifyAsync(new DataContent(new byte[] { 1 }, "text/plain")));
    }
}
