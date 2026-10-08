using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AllMiniLmL6V2Sharp;
using OoBDev.TestUtilities;
using System;
using System.IO;
using System.Linq;
using System.Numerics.Tensors;
using System.Threading.Tasks;
using ForkBertTokenizer = OoBDev.AllMiniLmL6V2Sharp.Tokenizer.BertTokenizer;

namespace OoBDev.Onnx.SentenceEmbeddings.Tests;

/// <summary>
/// Parity with the fork on plain text, where the fork is correct. The authority is the Hugging Face reference
/// (see ReferenceTests); the fork is wrong for accents, Korean, Polish, URLs, dates and bracketed special tokens.
/// Removed together with the fork.
/// </summary>
[TestClass]
public class ForkCompatibilityTests
{
    private static readonly string[] _corpus =
    [
        "The quick brown fox jumps over the lazy dog.",
        "hello 🙂 world",
        "punctuation!?;: (brackets) \"quotes\" - dashes",
        "UPPER lower MiXeD 12345 3.14",
        "a",
        "Sentence embeddings map text to vectors so that similar meaning is close together.",
    ];

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void TokenIds_MatchFork()
    {
        var folder = TestModel.RequireFolder();
        var vocab = Path.Combine(folder, "vocab.txt");
        var fork = new ForkBertTokenizer(vocab);
        var ours = new SentenceTokenizer(vocab, lowerCase: true, maxSequenceLength: 512);

        foreach (var text in _corpus)
        {
            var expected = fork.Encode(fork.Tokenize(text).Count(), text).Select(t => (int)t.InputIds).ToArray();
            CollectionAssert.AreEqual(expected, ours.Encode(text).ToArray(), text);
        }
    }

    /// <summary>
    /// Known difference 1 (better than the fork): the fork turns a run of CJK characters into one unknown token
    /// ([CLS], [UNK], [SEP]); the reference BERT tokenizer, and this implementation, split CJK per character.
    /// </summary>
    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void TokenIds_Cjk_AreSplitPerCharacterInsteadOfOneUnknown()
    {
        var vocab = Path.Combine(TestModel.RequireFolder(), "vocab.txt");
        var fork = new ForkBertTokenizer(vocab);
        var ours = new SentenceTokenizer(vocab, lowerCase: true, maxSequenceLength: 512);
        const string text = "東京は日本の首都です";

        CollectionAssert.AreEqual(new[] { 101, 100, 102 }, fork.Encode(fork.Tokenize(text).Count(), text).Select(t => (int)t.InputIds).ToArray());
        Assert.IsGreaterThan(5, ours.Encode(text).Count(id => id != 100));
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public async Task Embeddings_MatchFork()
    {
        var folder = TestModel.RequireFolder();
        using var fork = new AllMiniLmL6V2Embedder(Path.Combine(folder, "model.onnx"), new ForkBertTokenizer(Path.Combine(folder, "vocab.txt")));
        using var generator = TestModel.CreateGenerator();

        var actual = await generator.GenerateAsync(_corpus);

        for (var i = 0; i < _corpus.Length; i++)
        {
            var expected = fork.GenerateEmbedding(_corpus[i]).ToArray();
            var similarity = TensorPrimitives.CosineSimilarity(expected, actual[i].Vector.Span);
            Assert.IsGreaterThan(0.999f, similarity, _corpus[i]);
        }
    }
}
