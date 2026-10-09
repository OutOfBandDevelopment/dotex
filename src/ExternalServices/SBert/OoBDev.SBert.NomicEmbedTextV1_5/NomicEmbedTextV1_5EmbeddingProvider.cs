using OoBDev.AI;
using OoBDev.Onnx.SentenceEmbeddings;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.SBert.NomicEmbedTextV1_5;

/// <summary>
/// <see cref="IEmbeddingProvider"/> over the in-process nomic-embed-text-v1.5 ONNX generator.
/// </summary>
/// <remarks>Blank input returns a zero vector of <see cref="Length"/>.</remarks>
public class NomicEmbedTextV1_5EmbeddingProvider : IEmbeddingProvider
{
    private readonly OnnxSentenceEmbeddingGenerator _generator;

    /// <summary>
    /// Initializes a new instance of the <see cref="NomicEmbedTextV1_5EmbeddingProvider"/> class.
    /// </summary>
    /// <param name="generator">The generator that owns the model session.</param>
    public NomicEmbedTextV1_5EmbeddingProvider(OnnxSentenceEmbeddingGenerator generator) => _generator = generator;

    /// <inheritdoc />
    public int Length => _generator.Dimensions;

    /// <inheritdoc />
    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string content, string? model, CancellationToken cancellationToken = default)
    {
        var result = await _generator.GenerateAsync([content], cancellationToken: cancellationToken).ConfigureAwait(false);
        return result[0].Vector;
    }
}
