using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AllMiniLmL6V2Sharp;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics.Tensors;
using System.Threading.Tasks;
using ForkBertTokenizer = OoBDev.AllMiniLmL6V2Sharp.Tokenizer.BertTokenizer;

namespace OoBDev.Onnx.SentenceEmbeddings.Tests;

/// <summary>
/// Wide comparison against the fork: a deterministic corpus built from the model vocabulary, short to long.
/// </summary>
[TestClass]
public class ForkCorpusTests
{
    public TestContext TestContext { get; set; } = null!;

    private static string[] BuildCorpus(string vocabFile, int count)
    {
        var words = File.ReadAllLines(vocabFile)
            .Where(w => w.Length > 1 && w.All(char.IsLetter))
            .ToArray();
        var random = new Random(12345);
        return [.. Enumerable.Range(0, count).Select(i =>
        {
            var length = i % 4 == 0 ? random.Next(150, 220) : random.Next(2, 60);
            return string.Join(' ', Enumerable.Range(0, length).Select(_ => words[random.Next(words.Length)]));
        })];
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public async Task Corpus_AllVectorsMatchFork_AndReportSpeed()
    {
        var folder = TestModel.RequireFolder();
        var corpus = BuildCorpus(Path.Combine(folder, "vocab.txt"), 300);
        using var fork = new AllMiniLmL6V2Embedder(Path.Combine(folder, "model.onnx"), new ForkBertTokenizer(Path.Combine(folder, "vocab.txt")));
        using var generator = TestModel.CreateGenerator(o => o.MaxSequenceLength = 512);

        var watch = Stopwatch.StartNew();
        var expected = corpus.Select(text => fork.GenerateEmbedding(text).ToArray()).ToArray();
        var forkTime = watch.Elapsed;

        watch.Restart();
        var actual = await generator.GenerateAsync(corpus);
        var ourTime = watch.Elapsed;

        var similarities = expected.Select((e, i) => TensorPrimitives.CosineSimilarity(e, actual[i].Vector.Span)).ToArray();
        TestContext.WriteLine($"items={corpus.Length} min={similarities.Min():F6} avg={similarities.Average():F6} fork={forkTime.TotalSeconds:F2}s ours={ourTime.TotalSeconds:F2}s");

        var worst = similarities.Select((s, i) => (s, i)).OrderBy(x => x.s).Take(3).Select(x => $"#{x.i}={x.s:F4}");
        Assert.IsGreaterThan(0.999f, similarities.Min(), "worst: " + string.Join(", ", worst));
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public async Task DefaultMaxLength_DiffersFromForkOnlyBeyond256Tokens()
    {
        var folder = TestModel.RequireFolder();
        var corpus = BuildCorpus(Path.Combine(folder, "vocab.txt"), 300);
        var tokenizer = new ForkBertTokenizer(Path.Combine(folder, "vocab.txt"));
        using var fork = new AllMiniLmL6V2Embedder(Path.Combine(folder, "model.onnx"), tokenizer);
        using var generator = TestModel.CreateGenerator();

        var actual = await generator.GenerateAsync(corpus);

        var mismatches = new List<string>();
        for (var i = 0; i < corpus.Length; i++)
        {
            var tokens = tokenizer.Tokenize(corpus[i]).Count();
            var similarity = TensorPrimitives.CosineSimilarity(fork.GenerateEmbedding(corpus[i]).ToArray(), actual[i].Vector.Span);
            if (similarity < 0.999f && tokens <= 256) mismatches.Add($"#{i} tokens={tokens} cos={similarity:F4}");
        }
        TestContext.WriteLine($"mismatches within 256 tokens: {mismatches.Count}");
        Assert.IsEmpty(mismatches, string.Join("; ", mismatches));
    }
}
