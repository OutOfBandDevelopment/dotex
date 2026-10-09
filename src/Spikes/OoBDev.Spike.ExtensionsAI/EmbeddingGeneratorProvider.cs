using Microsoft.Extensions.AI;
using OoBDev.AI;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Spike.ExtensionsAI;

/// <summary>
/// Spike: <see cref="IEmbeddingProvider"/> over <see cref="IEmbeddingGenerator{TInput, TEmbedding}"/>.
/// </summary>
public sealed class EmbeddingGeneratorProvider : IEmbeddingProvider
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;

    public EmbeddingGeneratorProvider(IEmbeddingGenerator<string, Embedding<float>> generator, int length)
    {
        _generator = generator;
        Length = length;
    }

    public int Length { get; }

    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string content, string? model = default, CancellationToken cancellationToken = default)
    {
        var options = model is null ? null : new EmbeddingGenerationOptions { ModelId = model };
        var result = await _generator.GenerateAsync([content], options, cancellationToken);
        return result[0].Vector;
    }
}
