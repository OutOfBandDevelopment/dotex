using Microsoft.Extensions.AI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using OoBDev.Vision.ClipVitB32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Onnx.ImageEmbeddings.Tests;

[TestClass]
public class ClipUnitTests
{
    // A tiny vocabulary: single bytes, a merged word and the specials.
    private static ClipTokenizer Tokenizer() => new(
        """{"a":0,"b":1,"c":2,"a</w>":3,"b</w>":4,"c</w>":5,"ab</w>":6,"abc</w>":7,"!</w>":8,"<|startoftext|>":49406,"<|endoftext|>":49407}""",
        ["#version: 0.2", "a b</w>", "ab c</w>"]);

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Encode_AddsStartAndEnd()
    {
        var ids = Tokenizer().Encode("a");
        CollectionAssert.AreEqual(new[] { ClipTokenizer.StartOfText, 3, ClipTokenizer.EndOfText }, ids);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Encode_AppliesMergesLowerCaseAndCollapsesWhitespace()
    {
        var tokenizer = Tokenizer();
        CollectionAssert.AreEqual(tokenizer.Encode("ab"), tokenizer.Encode("  AB \n"));
        CollectionAssert.AreEqual(new[] { ClipTokenizer.StartOfText, 6, ClipTokenizer.EndOfText }, tokenizer.Encode("ab"));
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Encode_TruncatesButKeepsEnd()
    {
        var ids = Tokenizer().Encode(string.Join(' ', Enumerable.Repeat("a", 50)), 10);
        Assert.HasCount(10, ids);
        Assert.AreEqual(ClipTokenizer.EndOfText, ids[^1]);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Encode_RejectsTinyMaxLength() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Tokenizer().Encode("a", 2));

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void TextOptions_Validate_RejectsBadValues()
    {
        new ClipTextOptions().Validate();
        Assert.Throws<ArgumentException>(() => new ClipTextOptions { MaxSequenceLength = 78 }.Validate());
        Assert.Throws<ArgumentException>(() => new ClipTextOptions { MaxBatchSize = 0 }.Validate());
        Assert.Throws<ArgumentException>(() => new ClipTextOptions { ModelPath = " " }.Validate());
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public async Task ZeroShot_RanksBySimilarityAndSumsToOne()
    {
        var classifier = new ZeroShotImageClassifier(new Fixed<DataContent>([1f, 0f]), new FixedText());
        var all = await classifier.ClassifyAsync(new DataContent(new byte[] { 1 }, "image/png"), ["x", "y", "z"], 0);
        Assert.HasCount(3, all);
        Assert.AreEqual("x", all[0].Label, string.Join(",", all.Select(l => l.Label + "=" + l.Probability)));
        Assert.AreEqual(1f, all.Sum(l => l.Probability), 1e-4f);
        Assert.HasCount(2, await classifier.ClassifyAsync(new DataContent(new byte[] { 1 }, "image/png"), ["x", "y", "z"], 2));
        await Assert.ThrowsAsync<ArgumentException>(() => classifier.ClassifyAsync(new DataContent(new byte[] { 1 }, "image/png"), []));
    }

    private sealed class Fixed<T>(float[] vector) : IEmbeddingGenerator<T, Embedding<float>>
    {
        public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<T> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(values.Select(_ => new Embedding<float>(vector))));
        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }

    /// <summary>Label "x" points the same way as the image; the others are orthogonal or opposite.</summary>
    private sealed class FixedText : IEmbeddingGenerator<string, Embedding<float>>
    {
        public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<string> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(values.Select(v => new Embedding<float>(v switch { "x" => new float[] { 1f, 0f }, "y" => new float[] { 0f, 1f }, _ => new float[] { -1f, 0f } }))));
        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }
}
