using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Onnx.SentenceEmbeddings;
using OoBDev.SBert.AllMiniLmL6V2;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Containers;

namespace OoBDev.Embeddings.Http.Tests;

/// <summary>
/// The in-process all-MiniLM-L6-v2 ONNX generator against the HTTP client pointed at the Ollama service of the test stack
/// (<c>/v1/embeddings</c>, model <c>all-minilm</c>), the bar of docs/design/ImageEmbeddings (cosine at least 0.995).
/// </summary>
[TestClass]
public class OnnxVersusHttpTests
{
    private const double Bar = 0.995;

    private static readonly string[] Sample =
    [
        "The quick brown fox jumps over the lazy dog.",
        "Invoices are archived for seven years.",
        "How do I reset my password?",
        "A scanned page with a table of quarterly results.",
        "Sunset over the harbour, long exposure.",
    ];

    public required TestContext TestContext { get; set; }

    [TestCategory(TestCategories.Integration)]
    [IntegrationTestMethod("oobd-test-ollama")]
    public async Task Compare_SameSentences_CosineAtLeastTheBarAsync()
    {
        // Stage
        var url = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("OLLAMA_URL"));
        var model = TestContext.GetPropertyOrDefault("OLLAMA_EMBEDDING_MODEL", "all-minilm");
        TestContext.WriteLine($"server: {url}, model: {model}, sentences: {Sample.Length}");
        var options = new OnnxSentenceEmbeddingOptions();
        AllMiniLmL6V2Model.ApplyDefaults(options);
        try
        {
            await AllMiniLmL6V2Model.EnsureAsync(options, NullLogger.Instance);
        }
        catch (HttpRequestException ex)
        {
            Assert.Inconclusive("The all-MiniLM-L6-v2 model could not be downloaded: " + ex.Message);
        }

        using var onnx = new OnnxSentenceEmbeddingGenerator(Options.Create(options), NullLogger<OnnxSentenceEmbeddingGenerator>.Instance);
        using var http = new HttpClient { BaseAddress = new Uri(url.TrimEnd('/') + "/"), Timeout = TimeSpan.FromMinutes(5) };
        var provider = new OpenAiCompatibleEmbeddingProvider(http, Options.Create(new HttpEmbeddingOptions { BaseUrl = url, Model = model, Dimensions = onnx.Dimensions, TimeoutSeconds = 300 }));

        // Test
        var local = await onnx.GenerateAsync(Sample);
        var remote = await provider.GenerateEmbeddingsAsync(Sample);

        // Assert
        Assert.HasCount(Sample.Length, remote);
        var worst = 1.0;
        for (var i = 0; i < Sample.Length; i++)
        {
            var cosine = Cosine(local[i].Vector.Span, remote[i].Span);
            worst = Math.Min(worst, cosine);
            TestContext.WriteLine($"{i}: cosine {cosine:F5}");
        }

        TestContext.WriteLine($"lowest cosine {worst:F5} (bar {Bar}) on {Environment.MachineName} {DateTime.UtcNow:yyyy-MM-dd}");
        Assert.IsGreaterThanOrEqualTo(Bar, worst);
    }

    private static double Cosine(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        Assert.AreEqual(a.Length, b.Length, "Vectors must have the same dimension.");
        double dot = 0, na = 0, nb = 0;
        for (var i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            na += a[i] * a[i];
            nb += b[i] * b[i];
        }

        return dot / (Math.Sqrt(na) * Math.Sqrt(nb));
    }
}
