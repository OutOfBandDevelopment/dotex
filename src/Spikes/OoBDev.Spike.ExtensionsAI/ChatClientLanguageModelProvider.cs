using Microsoft.Extensions.AI;
using OoBDev.AI;
using OoBDev.AI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Spike.ExtensionsAI;

/// <summary>
/// Spike: the existing <see cref="ILanguageModelProvider"/> contract implemented once over any
/// <see cref="IChatClient"/> (and optionally an <see cref="IEmbeddingGenerator{TInput, TEmbedding}"/>),
/// so a vendor adapter only has to supply the platform client.
/// </summary>
public sealed class ChatClientLanguageModelProvider : ILanguageModelProvider
{
    private readonly IChatClient _chat;
    private readonly IEmbeddingGenerator<string, Embedding<float>>? _embeddings;

    public ChatClientLanguageModelProvider(
        IChatClient chat,
        IEmbeddingGenerator<string, Embedding<float>>? embeddings = null)
    {
        _chat = chat;
        _embeddings = embeddings;
    }

    public async Task<string> GetResponseAsync(string promptDetails, string userInput, CancellationToken cancellationToken = default)
    {
        var response = await _chat.GetResponseAsync(Build(promptDetails, [userInput], []), cancellationToken: cancellationToken);
        return response.Text;
    }

    public async IAsyncEnumerable<string> GetStreamedResponseAsync(string promptDetails, string userInput, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var update in _chat.GetStreamingResponseAsync(Build(promptDetails, [userInput], []), cancellationToken: cancellationToken))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                yield return update.Text;
            }
        }
    }

    public async IAsyncEnumerable<string> GetStreamedContextResponseAsync(string assistantConfinment, List<string> systemInteractions, List<string> userInput, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var update in _chat.GetStreamingResponseAsync(Build(assistantConfinment, userInput, systemInteractions), cancellationToken: cancellationToken))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                yield return update.Text;
            }
        }
    }

    public async Task<float[]> GetEmbeddedResponseAsync(string data, CancellationToken cancellationToken = default)
    {
        if (_embeddings is null)
        {
            throw new InvalidOperationException("No IEmbeddingGenerator was supplied.");
        }

        var result = await _embeddings.GenerateAsync([data], cancellationToken: cancellationToken);
        return result[0].Vector.ToArray();
    }

    public async Task<string> GetContextResponseAsync(string assistantConfinment, List<string> systemInteractions, List<string> userInput, CancellationToken cancellationToken = default)
    {
        var response = await _chat.GetResponseAsync(Build(assistantConfinment, userInput, systemInteractions), cancellationToken: cancellationToken);
        return response.Text;
    }

    public Task<string> GetRAGResponseAsync(string assistantConfinment, string ragData, string userInput, CancellationToken cancellationToken = default) =>
        GetResponseAsync($"{assistantConfinment}\n\nUse only this data:\n{ragData}", userInput, cancellationToken);

    public async IAsyncEnumerable<string> GetRAGResponseCitiationsAsync(List<KeyValuePairModel> ragData, string userQuery, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var sources = string.Join("\n", ragData.Select(r => $"[{r.Key}] {r.Value}"));
        await foreach (var text in GetStreamedResponseAsync(
            $"Answer using only these sources and cite them by their bracketed key:\n{sources}", userQuery, cancellationToken))
        {
            yield return text;
        }
    }

    private static List<ChatMessage> Build(string system, IEnumerable<string> user, IEnumerable<string> assistant)
    {
        var messages = new List<ChatMessage> { new(ChatRole.System, system) };
        messages.AddRange(assistant.Select(a => new ChatMessage(ChatRole.Assistant, a)));
        messages.AddRange(user.Select(u => new ChatMessage(ChatRole.User, u)));
        return messages;
    }
}
