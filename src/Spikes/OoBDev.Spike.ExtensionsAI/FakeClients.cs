using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Spike.ExtensionsAI;

internal sealed class FakeChatClient : IChatClient
{
    public List<ChatMessage> LastMessages { get; private set; } = [];

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        LastMessages = messages.ToList();
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "answer")));
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        LastMessages = messages.ToList();
        foreach (var part in new[] { "an", "swer" })
        {
            yield return new ChatResponseUpdate(ChatRole.Assistant, part);
            await Task.Yield();
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}

internal sealed class FakeEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    public string? LastModel { get; private set; }

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<string> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        LastModel = options?.ModelId;
        var result = new GeneratedEmbeddings<Embedding<float>>(
            values.Select(v => new Embedding<float>(new float[] { v.Length, 1f, 0f })));
        return Task.FromResult(result);
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
