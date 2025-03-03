using AllMiniLmL6V2Sharp;
using Microsoft.Extensions.Options;
using OoBDev.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.SBert.AllMiniLML6v2Sharp;

/// <summary>
/// Provides embedding generation using the AllMiniLmL6V2 model.
/// </summary>
public class AllMiniLmL6V2Embedding : IEmbeddingProvider
{
    private readonly IEmbedder _embedder;
    private readonly IOptions<AllMiniLmL6V2EmbeddingOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllMiniLmL6V2Embedding"/> class.
    /// </summary>
    /// <param name="embedder">The embedder instance used for generating embeddings.</param>
    /// <param name="options">Configuration options for the embedding provider.</param>
    public AllMiniLmL6V2Embedding(
        IEmbedder embedder,
        IOptions<AllMiniLmL6V2EmbeddingOptions> options
        )
    {
        _embedder = embedder;
        _options = options;
    }

    private int? _length;

    /// <summary>
    /// Gets the length of the embedding vector.
    /// </summary>
    public int Length => _length ??= GenerateEmbeddingAsync("hello world", default, default).Result.Length;

    /// <summary>
    /// Generates an embedding for the given text input.
    /// </summary>
    /// <param name="content">The input text to generate an embedding for.</param>
    /// <param name="model">The optional model identifier (not used in this implementation).</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the embedding as a read-only memory block of floats.</returns>
    public Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string content, string? model, CancellationToken cancellationToken = default)
    {
        ReadOnlyMemory<float> result =
            string.IsNullOrWhiteSpace(content) ?
            Array.Empty<float>() :
            [.. _embedder.GenerateEmbedding(content)];

        return Task.FromResult(result);
    }

    /// <summary>
    /// Generates embeddings for a collection of text inputs asynchronously.
    /// </summary>
    /// <param name="inputs">A collection of input texts.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>An asynchronous stream of tuples containing the input text and its corresponding embedding.</returns>
    public async IAsyncEnumerable<(string input, float[]? embedding)> GenerateEmbeddingAsync(IEnumerable<string> inputs, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var results = _embedder.GenerateEmbeddings(inputs).ToArray();
        var zipped = inputs.Zip(results);
        await Task.Yield();
        foreach (var item in zipped)
        {
            var embedding = item.Second.ToArray();
            if (embedding.Length > 0)
            {
                yield return (item.First, embedding);
            }
        }
    }
}
