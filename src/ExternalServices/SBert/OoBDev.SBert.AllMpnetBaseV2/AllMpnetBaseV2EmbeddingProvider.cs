using OoBDev.AI;
using OoBDev.Onnx.SentenceEmbeddings;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.SBert.AllMpnetBaseV2;

/// <summary>
/// <see cref="IEmbeddingProvider"/> over the in-process all-mpnet-base-v2 ONNX generator.
/// </summary>
/// <remarks>Blank input returns a zero vector of <see cref="Length"/>.</remarks>
public class AllMpnetBaseV2EmbeddingProvider : IEmbeddingProvider
{
    private readonly OnnxSentenceEmbeddingGenerator _generator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllMpnetBaseV2EmbeddingProvider"/> class.
    /// </summary>
    /// <param name="generator">The generator that owns the model session.</param>
    public AllMpnetBaseV2EmbeddingProvider(OnnxSentenceEmbeddingGenerator generator) => _generator = generator;

    /// <inheritdoc />
    public int Length => _generator.Dimensions;

    /// <inheritdoc />
    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string content, string? model, CancellationToken cancellationToken = default)
    {
        var result = await _generator.GenerateAsync([content], cancellationToken: cancellationToken).ConfigureAwait(false);
        return result[0].Vector;
    }
}
