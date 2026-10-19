using OoBDev.AI;
using OoBDev.AI.Models;
using OoBDev.Extensions.Linq;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using System.Text;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Ollama;

/// <summary>
/// Represents a class responsible for generating message completions using the Ollama API.
/// </summary>
public class OllamaMessageCompletion : IMessageCompletion, IEmbeddingProvider, IStreamedCompletion
{
    private readonly IOllamaApiClient _client;
    private readonly IOllamaModelMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="OllamaMessageCompletion"/> class.
    /// </summary>
    /// <param name="client">The OllamaApiClient instance used for communication with the Ollama API.</param>
    /// <param name="mapper">Model mapper for Ollama request/response.</param>
    public OllamaMessageCompletion(
        IOllamaApiClient client,
        IOllamaModelMapper mapper
        )
    {
        _client = client;
        _mapper = mapper;
    }

    private int? _length;
    /// <summary>
    /// Gets the length of the embeddings.
    /// </summary>
    public int Length => _length ??= GenerateEmbeddingAsync("hello world", default, default).Result.Length;

    /// <summary>
    /// Retrieves the embedding vector for the given content.
    /// </summary>
    /// <param name="content">The content for which to retrieve the embedding.</param>
    /// <param name="model">The model for which to retrieve the embedding.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task representing the asynchronous operation. The task result contains the 
    /// embedding vector as an array of single-precision floats.</returns>
    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(
        string content,
#if DEBUG
        string? model,
        CancellationToken cancellationToken
#else
        string? model = default,
        CancellationToken cancellationToken = default
#endif
        ) =>
        await _client.GetEmbeddingSingleAsync(content, model ?? _client.SelectedModel).ConfigureAwait(false);

    /// <summary>
    /// Generates a completion for the given prompt using the specified model.
    /// </summary>
    /// <param name="modelName">The name of the model to use for completion.</param>
    /// <param name="prompt">The prompt for which completion is requested.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the completion response.</returns>
    public async Task<string> GetCompletionAsync(string modelName, string prompt)
    {
        var request = new ChatRequest
        {
            Model = modelName,
            Messages = new[] { new Message(ChatRole.User, prompt) }
        };

        var responses = await _client.ChatAsync(request).ToListAsync().ConfigureAwait(false);
        var result = string.Join("", responses.Select(r => r?.Message.Content ?? ""));
        return result;
    }

    /// <summary>
    /// Retrieves a completion for the given prompt from the specified model.
    /// </summary>
    /// <param name="model">Completion request model</param>
    /// <returns>Resulting response object</returns>
    public async Task<CompletionResponse> GetCompletionAsync(CompletionRequest model)
    {
        var text = new StringBuilder();
        long[] context = [];
        await foreach (var part in _client.GenerateAsync(_mapper.Map(model, _client.SelectedModel)).ConfigureAwait(false))
        {
            text.Append(part?.Response);
            if (part is GenerateDoneResponseStream done)
            {
#pragma warning disable CS0618 // Ollama deprecates generate context, but it is what carries the CompletionRequest.Context contract
                context = done.Context ?? [];
#pragma warning restore CS0618
            }
        }
        return new() { Context = context, Response = text.ToString() };
    }


    /// <summary>
    /// Gets a streamed response asynchronously based on the provided prompt details and user input.
    /// </summary>
    /// <param name="promptDetails">The details of the prompt.</param>
    /// <param name="userInput">The user input.</param>
    /// <param name="cancellationToken">The Cancellation Token.</param>
    /// <returns>An asynchronous enumerable of strings representing the streamed response.</returns>
    /// <inheritdoc />
    public async IAsyncEnumerable<string> GetStreamedResponseAsync(string promptDetails, string userInput, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = _mapper.Map(new() { Prompt = userInput, System = promptDetails }, _client.SelectedModel);
        await foreach (var part in _client.GenerateAsync(request, cancellationToken).ConfigureAwait(false))
        {
            if (!string.IsNullOrEmpty(part?.Response))
            {
                yield return part.Response;
            }
        }
    }
}
