using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AI;

/// <summary>
/// Represents a provider for word embeddings.
/// </summary>
public interface IEmbeddingProvider
{
    /// <summary>
    /// Gets the length of the embeddings.
    /// </summary>
    int Length { get; }

    /// <summary>
    /// Retrieves the embedding vector for the given content.
    /// </summary>
    /// <param name="content">The content for which to retrieve the embedding.</param>
    /// <param name="model">The model for which to retrieve the embedding.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the embedding vector as an array of single-precision floats.</returns>
    Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(
        string content,
#if DEBUG
        string? model,
        CancellationToken cancellationToken
#else
        string? model = default,
        CancellationToken cancellationToken = default
#endif
        );
}
