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

public class AllMiniLmL6V2Embedding : IEmbeddingProvider
{
    private readonly IEmbedder _embedder;
    private readonly IOptions<AllMiniLmL6V2EmbeddingOptions> _options;

    public AllMiniLmL6V2Embedding(
        IEmbedder embedder,
        IOptions<AllMiniLmL6V2EmbeddingOptions> options
        )
    {
        _embedder = embedder;
        _options = options;
    }

    private int? _length;
    public int Length => _length ??= GenerateEmbeddingAsync("hello world", default, default).Result.Length;

    public Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string content, string? model, CancellationToken cancellationToken = default)
    {
        ReadOnlyMemory<float> result =
            string.IsNullOrWhiteSpace(content) ?
            Array.Empty<float>() :
            _embedder.GenerateEmbedding(content).ToArray();

        return Task.FromResult(result);
    }

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
