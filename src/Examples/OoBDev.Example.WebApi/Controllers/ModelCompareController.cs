using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.AI;
using OoBDev.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics.Tensors;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.WebApi.Controllers;

/// <summary>
/// Embeds the same two texts with different in-process sentence models so their similarity scores can be compared.
/// Each model is downloaded the first time it is used.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class ModelCompareController(IServiceProvider services) : ControllerBase
{
    private static readonly string[] Models =
    [
        OoBDev.SBert.AllMiniLmL6V2.SBertGlobals.AllMiniLmL6V2Key,
        OoBDev.SBert.AllMpnetBaseV2.SBertGlobals.AllMpnetBaseV2Key,
        OoBDev.SBert.NomicEmbedTextV1_5.SBertGlobals.NomicEmbedTextV1_5Key,
    ];

    /// <summary>Lists the model keys that can be compared.</summary>
    /// <returns>The model keys.</returns>
    [HttpGet("models")]
    public ActionResult<string[]> GetModels() => Models;

    /// <summary>Scores the similarity of two texts with the chosen models.</summary>
    /// <param name="request">The texts and the models to use.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>One score per model.</returns>
    [HttpPost("similarity")]
    public async Task<ActionResult<List<ModelScoreModel>>> Similarity(ModelCompareRequestModel request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.A) || string.IsNullOrWhiteSpace(request.B))
            return Problem(title: "Invalid texts", detail: "Give both texts.", statusCode: StatusCodes.Status400BadRequest);
        var unknown = request.Models.Where(m => !Models.Contains(m)).ToList();
        if (request.Models.Count == 0 || unknown.Count > 0)
            return Problem(title: "Invalid models", detail: $"Choose from: {string.Join(", ", Models)}.", statusCode: StatusCodes.Status400BadRequest);

        var scores = new List<ModelScoreModel>();
        foreach (var model in request.Models.Distinct())
        {
            try
            {
                var provider = services.GetRequiredKeyedService<IEmbeddingProvider>(model);
                var started = DateTimeOffset.UtcNow;
                var a = await provider.GenerateEmbeddingAsync(request.A, default, cancellationToken);
                var b = await provider.GenerateEmbeddingAsync(request.B, default, cancellationToken);
                scores.Add(new ModelScoreModel
                {
                    Model = model,
                    Length = a.Length,
                    Similarity = TensorPrimitives.CosineSimilarity(a.Span, b.Span),
                    Milliseconds = (DateTimeOffset.UtcNow - started).TotalMilliseconds,
                });
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return Problem(title: $"Model {model} failed", detail: ex.Message, statusCode: StatusCodes.Status422UnprocessableEntity);
            }
        }
        return scores;
    }
}
