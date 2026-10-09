using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Onnx.ImageEmbeddings.Skia;
using OoBDev.TestUtilities;
using OoBDev.Vision.Dinov2Small;
using OoBDev.Vision.VitBasePatch16;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics.Tensors;
using System.Text.Json;
using System.Threading.Tasks;

namespace OoBDev.Onnx.ImageEmbeddings.Tests;

/// <summary>
/// Compares the presets with vectors and labels from the original Python models
/// (scripts/embeddings/make-reference-images.py). Model files are downloaded into the shared Hugging Face cache on first run.
/// </summary>
[TestClass]
public class ReferenceTests
{
    public TestContext TestContext { get; set; } = null!;

    private static JsonElement[] Items(string model, out JsonElement root)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", model + ".reference.json");
        var document = JsonDocument.Parse(File.ReadAllText(path));
        root = document.RootElement;
        return [.. root.GetProperty("items").EnumerateArray()];
    }

    private static DataContent Image(string file)
    {
        var bytes = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "TestData", "images", file));
        return new DataContent(bytes, file.EndsWith(".jpg", StringComparison.Ordinal) ? "image/jpeg" : "image/png");
    }

    private static async Task Ensure(Func<Task> download)
    {
        try
        {
            await download();
        }
        catch (global::System.Net.Http.HttpRequestException ex)
        {
            Assert.Inconclusive("The model could not be downloaded: " + ex.Message);
        }
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    public async Task Dinov2Small_MatchesReference()
    {
        var options = new OnnxImageEmbeddingOptions();
        Dinov2SmallModel.ApplyDefaults(options);
        await Ensure(() => Dinov2SmallModel.EnsureAsync(options, NullLogger.Instance));

        var items = Items("dinov2-small", out _);
        using var generator = new OnnxImageEmbeddingGenerator(Microsoft.Extensions.Options.Options.Create(options), new SkiaImageDecoder(), NullLogger<OnnxImageEmbeddingGenerator>.Instance);
        Assert.AreEqual(384, generator.Dimensions);

        var actual = await generator.GenerateAsync(items.Select(i => Image(i.GetProperty("file").GetString()!)).ToArray());

        var scores = items.Select((item, i) =>
        {
            var expected = item.GetProperty("vector").EnumerateArray().Select(v => v.GetSingle()).ToArray();
            return (file: item.GetProperty("file").GetString()!, similarity: TensorPrimitives.CosineSimilarity(expected, actual[i].Vector.Span));
        }).ToArray();
        foreach (var score in scores) TestContext.WriteLine($"{score.file}: cos={score.similarity:F5}");
        // 0.995, not 0.999: the same ONNX file run in Python on the Hugging Face processor's pixels gives identical
        // cosines (0.9975 to 0.9999), so the gap is the Xenova ONNX export versus PyTorch, not this pipeline.
        var failures = scores.Where(s => s.similarity < 0.995f).Select(s => $"{s.file} cos={s.similarity:F4}").ToArray();
        Assert.IsEmpty(failures, string.Join("; ", failures));
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    public async Task VitBasePatch16_MatchesReferenceLabels()
    {
        var options = new OnnxImageEmbeddingOptions();
        VitBasePatch16Model.ApplyDefaults(options);
        string folder = string.Empty;
        await Ensure(async () => folder = await VitBasePatch16Model.EnsureAsync(options, NullLogger.Instance));

        var items = Items("vit-base-patch16-224", out _);
        using var classifier = new OnnxImageClassifier(Microsoft.Extensions.Options.Options.Create(options), VitBasePatch16Model.LoadLabels(folder), new SkiaImageDecoder(), NullLogger<OnnxImageClassifier>.Instance);
        Assert.HasCount(1000, classifier.Labels);

        var failures = new List<string>();
        foreach (var item in items)
        {
            var file = item.GetProperty("file").GetString()!;
            var expected = item.GetProperty("top").EnumerateArray()
                .Select(t => (Label: t.GetProperty("label").GetString()!, Probability: t.GetProperty("probability").GetSingle()))
                .ToArray();
            var actual = await classifier.ClassifyAsync(Image(file), 5);

            var maxDifference = expected.Max(e => Math.Abs(e.Probability - (actual.FirstOrDefault(a => a.Label == e.Label)?.Probability ?? 0f)));
            TestContext.WriteLine($"{file}: top={actual[0].Label} ({actual[0].Probability:F3}) expected={expected[0].Label} ({expected[0].Probability:F3}) max diff={maxDifference:F4}");

            if (actual[0].Label != expected[0].Label) failures.Add($"{file}: top-1 {actual[0].Label} != {expected[0].Label}");
            if (!actual.Select(a => a.Label).Order().SequenceEqual(expected.Select(e => e.Label).Order())) failures.Add($"{file}: top-5 set differs");
            if (maxDifference > 0.01f) failures.Add($"{file}: probability differs by {maxDifference:F4}");
        }
        Assert.IsEmpty(failures, string.Join("; ", failures));
    }
}
