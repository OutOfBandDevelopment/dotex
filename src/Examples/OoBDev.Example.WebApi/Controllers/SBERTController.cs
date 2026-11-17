using OoBDev.SBert.AllMpnetBaseV2;
using OoBDev.AspNetCore.Mvc.Filters;
using OoBDev.AI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace OoBDev.WebApi.Controllers;

/// <summary>
/// Embeds text with the in-process all-mpnet-base-v2 ONNX model (the same model the Python SBERT container hosts).
/// </summary>
[Route("api/[controller]")]
[ApiController]
[ApplicationRight("ai.invoke")]
public class SBERTController : ControllerBase
{
    private readonly IEmbeddingProvider _embedding;

    /// <summary>
    /// Initializes a new instance of the <see cref="SBERTController"/> class with the specified dependencies.
    /// </summary>
    /// <param name="embedding">The embedding provider.</param>
    public SBERTController(
        [FromKeyedServices(SBertGlobals.AllMpnetBaseV2Key)] IEmbeddingProvider embedding
        ) => _embedding = embedding;

    /// <summary>
    /// Retrieves the embedding vector for the given text.
    /// </summary>
    /// <param name="text">The text for which to retrieve the embedding vector.</param>
    /// <returns>The embedding vector.</returns>
    [HttpGet]
    public async Task<ReadOnlyMemory<float>> Embed(string text) =>
        await _embedding.GenerateEmbeddingAsync(text, default, default);
}
