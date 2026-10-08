using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.IO;
using System.Linq;
using System.Numerics.Tensors;
using System.Text.Json;
using System.Threading.Tasks;

namespace OoBDev.Onnx.SentenceEmbeddings.Tests;

/// <summary>
/// Compares against vectors produced by the reference implementation (sentence-transformers/all-MiniLM-L6-v2 on
/// Hugging Face, see TestData/README.md).
/// </summary>
[TestClass]
public class ReferenceTests
{
    public TestContext TestContext { get; set; } = null!;

    private sealed record Item(string Text, int Tokens, float[] Vector);

    private static Item[] Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", "all-minilm-l6-v2.reference.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return [.. document.RootElement.GetProperty("items").EnumerateArray().Select(item => new Item(
            item.GetProperty("text").GetString()!,
            item.GetProperty("tokens").GetInt32(),
            [.. item.GetProperty("vector").EnumerateArray().Select(v => v.GetSingle())]))];
    }

    private static string Describe(string text) => (text.Length > 40 ? text[..40] + "..." : text).Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ');

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public async Task InProcessGenerator_MatchesReference()
    {
        var items = Load();
        using var generator = TestModel.CreateGenerator();

        var actual = await generator.GenerateAsync(items.Select(i => i.Text).ToArray());

        var failures = items
            .Select((item, i) => (item, similarity: TensorPrimitives.CosineSimilarity(item.Vector, actual[i].Vector.Span)))
            .Where(x => x.similarity < 0.999f)
            .Select(x => $"{Describe(x.item.Text)} tokens={x.item.Tokens} cos={x.similarity:F4}")
            .ToArray();
        TestContext.WriteLine($"items={items.Length} below 0.999: {failures.Length}");
        foreach (var failure in failures) TestContext.WriteLine(failure);
        Assert.IsEmpty(failures, string.Join("; ", failures));
    }
}
