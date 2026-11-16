using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OoBDev.AI;

namespace OoBDev.Embeddings.Http;

/// <summary>
/// <see cref="IEmbeddingProvider"/> over any server that speaks the OpenAI <c>/v1/embeddings</c> shape (Ollama, llama.cpp, vLLM, TEI, a container).
/// </summary>
public sealed class OpenAiCompatibleEmbeddingProvider(HttpClient client, IOptions<HttpEmbeddingOptions> options) : IEmbeddingProvider
{
    private readonly HttpEmbeddingOptions _options = options.Value;

    /// <inheritdoc />
    public int Length => _options.Dimensions;

    /// <inheritdoc />
    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string content, string? model, CancellationToken cancellationToken = default)
    {
        var all = await GenerateEmbeddingsAsync([content], model, cancellationToken).ConfigureAwait(false);
        return all[0];
    }

    /// <summary>
    /// Embeds many inputs, split into requests of <see cref="HttpEmbeddingOptions.BatchSize"/>; the result is in input order.
    /// </summary>
    /// <param name="contents">The inputs.</param>
    /// <param name="model">The model, or <see langword="null"/> for the configured one.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>One vector per input.</returns>
    public async Task<IReadOnlyList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IReadOnlyList<string> contents, string? model = default, CancellationToken cancellationToken = default)
    {
        var result = new List<ReadOnlyMemory<float>>(contents.Count);
        for (var offset = 0; offset < contents.Count; offset += _options.BatchSize)
        {
            var chunk = contents.Skip(offset).Take(_options.BatchSize).ToArray();
            result.AddRange(await SendAsync(chunk, model ?? _options.Model, cancellationToken).ConfigureAwait(false));
        }
        return result;
    }

    private async Task<IReadOnlyList<ReadOnlyMemory<float>>> SendAsync(string[] chunk, string model, CancellationToken cancellationToken)
    {
        using var response = await client.PostAsJsonAsync("v1/embeddings", new Request(model, chunk), cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            throw new EmbeddingServerException($"The embedding server answered {(int)response.StatusCode} {response.StatusCode}.", response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<Response>(cancellationToken).ConfigureAwait(false);
        if (body?.Data is null || body.Data.Count != chunk.Length)
            throw new EmbeddingServerException($"The embedding server returned {body?.Data?.Count ?? 0} vectors for {chunk.Length} inputs.");

        var ordered = body.Data.OrderBy(d => d.Index).Select(d => d.Embedding ?? []).ToArray();
        if (ordered.Any(v => v.Length != _options.Dimensions))
            throw new EmbeddingServerException($"The embedding server returned vectors that are not {_options.Dimensions} long.");
        return ordered.Select(v => (ReadOnlyMemory<float>)v).ToArray();
    }

    private sealed record Request([property: JsonPropertyName("model")] string Model, [property: JsonPropertyName("input")] string[] Input);

    private sealed record Response([property: JsonPropertyName("data")] List<Item>? Data);

    private sealed record Item([property: JsonPropertyName("index")] int Index, [property: JsonPropertyName("embedding")] float[]? Embedding);
}
