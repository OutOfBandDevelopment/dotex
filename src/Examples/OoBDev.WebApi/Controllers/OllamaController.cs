using OoBDev.AI;
using OoBDev.AI.Models;
using OoBDev.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.WebApi.Controllers;

/// <summary>
/// Controller for AI-related operations.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class OllamaController : ControllerBase
{
    private readonly ILanguageModelProvider _llmProvider;
    private readonly IMessageCompletion _completion;
    private readonly IEmbeddingProvider _embedding;

    /// <summary>
    /// Initializes a new instance of the <see cref="AIController"/> class with the specified dependencies.
    /// </summary>
    /// <param name="llmProvider">The language model provider.</param>
    /// <param name="completion">The completion provider.</param>
    /// <param name="embedding">The embedding provider.</param>
    public OllamaController(
        [FromKeyedServices("OLLAMA")] ILanguageModelProvider llmProvider,
        [FromKeyedServices("OLLAMA")] IMessageCompletion completion,
        [FromKeyedServices("OLLAMA")] IEmbeddingProvider embedding
        )
    {
        _llmProvider = llmProvider;
        _completion = completion;
        _embedding = embedding;
    }

    /// <summary>
    /// Generate an LLM Response based on the prompt and user input
    /// </summary>
    /// <returns>The string response from the LLM</returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<string> GetResponseAsync([FromBody] GenerativeAiRequestModel model) =>
        await _llmProvider.GetResponseAsync(model.PromptDetails, model.UserInput);

    /// <summary>
    /// Generate an AbstractAI Streamed Response based on the prompt and user input
    /// </summary>
    /// <returns>The streamed string responses from the AbstractAI</returns>
    [HttpPost("Streamed")]
    [AllowAnonymous]
    public async IAsyncEnumerable<string> GetStreamedResponseAsync([FromBody] GenerativeAiRequestModel model)
    {
        await foreach (var response in _llmProvider.GetStreamedResponseAsync(model.PromptDetails, model.UserInput))
        {
            yield return response;
        };
    }

    /// <summary>
    /// Retrieves the embedding vector for the given text.
    /// </summary>
    /// <param name="text">The text for which to retrieve the embedding vector.</param>
    /// <param name="model">The model for which to retrieve the embedding vector.</param>
    /// <returns>The embedding vector.</returns>
    [HttpGet]
    public async Task<ReadOnlyMemory<float>> Embed(string text, string? model = default) =>
        await _embedding.GenerateEmbeddingAsync(text, model, default);

    /// <summary>
    /// executes a completion request
    /// </summary>
    /// <param name="model">completion request</param>
    /// <returns>completion result</returns>
    [HttpPost("Completion")]
    public async Task<CompletionResponse> Completion([FromBody] CompletionRequest model) =>
        await _completion.GetCompletionAsync(model);
}
