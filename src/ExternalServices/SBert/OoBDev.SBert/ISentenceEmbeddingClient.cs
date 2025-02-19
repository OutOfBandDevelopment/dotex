using System;
using System.Threading.Tasks;

namespace OoBDev.SBert;

/// <summary>
/// Represents a client for generating sentence embeddings.
/// </summary>
public interface ISentenceEmbeddingClient
{
    /// <summary>
    /// Asynchronously generates a sentence embedding as an array of single-precision floating-point numbers.
    /// </summary>
    /// <param name="input">The input sentence to generate the embedding for.</param>
    /// <returns>An array of single-precision floating-point numbers representing the embedding of the input sentence.</returns>
    Task<ReadOnlyMemory<float>> GetEmbeddingAsync(string input);

    /// <summary>
    /// Asynchronously generates a sentence embedding as an array of double-precision floating-point numbers.
    /// </summary>
    /// <param name="input">The input sentence to generate the embedding for.</param>
    /// <returns>An array of double-precision floating-point numbers representing the embedding of the input sentence.</returns>
    Task<ReadOnlyMemory<double>> GetEmbeddingDoubleAsync(string input);

    /// <summary>
    /// Check the health status of the sentence embedding service
    /// </summary>
    /// <returns></returns>
    Task<string> GetHealthAsync();
}
