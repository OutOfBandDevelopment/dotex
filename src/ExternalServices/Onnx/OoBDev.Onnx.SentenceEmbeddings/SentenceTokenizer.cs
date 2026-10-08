using Microsoft.ML.Tokenizers;
using System;
using System.Collections.Generic;
using System.IO;

namespace OoBDev.Onnx.SentenceEmbeddings;

/// <summary>
/// WordPiece tokenizer for single sentences, built on <see cref="BertTokenizer"/>. Instances are immutable and safe to share.
/// </summary>
internal sealed class SentenceTokenizer
{
    private readonly BertTokenizer _tokenizer;
    private readonly int _maxSequenceLength;

    public SentenceTokenizer(string vocabPath, bool lowerCase, int maxSequenceLength)
    {
        using var stream = File.OpenRead(vocabPath);
        _tokenizer = BertTokenizer.Create(stream, new BertOptions { LowerCaseBeforeTokenization = lowerCase });
        _maxSequenceLength = maxSequenceLength;
    }

    /// <summary>
    /// Token ids for one sentence, including the start and end markers, at most <c>maxSequenceLength</c> long.
    /// </summary>
    public IReadOnlyList<int> Encode(string text) =>
        _tokenizer.EncodeToIds(text, _maxSequenceLength, out _, out _);
}
