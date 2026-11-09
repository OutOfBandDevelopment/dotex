using Microsoft.Extensions.Options;
using OllamaSharp;
using OoBDev.AI;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Ollama;

/// <summary>
/// <see cref="IEmbeddingProvider"/> over Ollama's embed endpoint, using <see cref="OllamaApiClientOptions.EmbeddingModel"/> (not the chat model).
/// </summary>
/// <remarks>The embedding model is pulled once per server when it is not installed yet.</remarks>
public class OllamaEmbeddingProvider : IEmbeddingProvider
{
    private static readonly ConcurrentDictionary<string, Lazy<Task>> Pulls = new(StringComparer.Ordinal);

    private readonly IOllamaApiClient _client;
    private readonly IOptions<OllamaApiClientOptions> _options;
    private int? _length;

    /// <summary>
    /// Initializes a new instance of the <see cref="OllamaEmbeddingProvider"/> class.
    /// </summary>
    /// <param name="client">The Ollama client.</param>
    /// <param name="options">The Ollama options holding the embedding model name.</param>
    public OllamaEmbeddingProvider(IOllamaApiClient client, IOptions<OllamaApiClientOptions> options)
    {
        _client = client;
        _options = options;
    }

    /// <inheritdoc />
    public int Length => _length ??= GenerateEmbeddingAsync("hello world", default, default).GetAwaiter().GetResult().Length;

    /// <inheritdoc />
    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string content, string? model, CancellationToken cancellationToken = default)
    {
        var name = model ?? _options.Value.EmbeddingModel;
        await EnsureModelAsync(name, cancellationToken).ConfigureAwait(false);
        var result = await _client.GetEmbeddingSingleAsync(content, name).ConfigureAwait(false);
        _length ??= result.Length;
        return result;
    }

    private Task EnsureModelAsync(string model, CancellationToken cancellationToken) =>
        Pulls.GetOrAdd($"{_client.Uri}|{model}", _ => new Lazy<Task>(() => PullIfMissingAsync(model, cancellationToken))).Value;

    private async Task PullIfMissingAsync(string model, CancellationToken cancellationToken)
    {
        var local = await _client.ListLocalModelsAsync(cancellationToken).ConfigureAwait(false);
        if (local.Any(m => m.Name.StartsWith(model, StringComparison.Ordinal)))
            return;
        await foreach (var _ in _client.PullModelAsync(model, cancellationToken).ConfigureAwait(false))
        {
        }
    }
}
