using OoBDev.AI;
using OoBDev.SBert.AllMiniLmL6V2;
using OoBDev.WebApi.Models;
using OoBDev.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics.Tensors;
using System.Threading.Tasks;

namespace OoBDev.WebApi.Controllers;

/// <summary>
/// Searches the given passages three ways: by keywords (BM25), by meaning (in-process embeddings) and by both fused.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class SearchController([FromKeyedServices(SBertGlobals.AllMiniLmL6V2Key)] IEmbeddingProvider embedding) : ControllerBase
{
    /// <summary>
    /// Runs the query in all three modes.
    /// </summary>
    /// <param name="model">The query and the passages.</param>
    /// <returns>The hits per mode, best first.</returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<SearchResponseModel>> Search([FromBody] SearchRequestModel model)
    {
        if (model.Passages.Count == 0 || string.IsNullOrWhiteSpace(model.Query))
        {
            return Problem(title: "Nothing to search", detail: "Give a query and at least one passage.", statusCode: StatusCodes.Status400BadRequest);
        }

        var limit = Math.Clamp(model.Limit, 1, 20);
        var question = (await embedding.GenerateEmbeddingAsync(model.Query, default, default)).ToArray();
        var scored = new List<SearchHitModel>();
        foreach (var passage in model.Passages)
        {
            var vector = (await embedding.GenerateEmbeddingAsync(passage.Text, default, default)).ToArray();
            scored.Add(new SearchHitModel { Id = passage.Id, Text = passage.Text, Score = TensorPrimitives.CosineSimilarity(question, vector) });
        }

        var semantic = scored.OrderByDescending(h => h.Score).ToList();
        var lexical = SearchScoring.Lexical(model.Query, model.Passages);
        var hybrid = SearchScoring.Fuse(lexical, semantic);

        return new SearchResponseModel
        {
            Lexical = lexical.Take(limit).ToList(),
            Semantic = semantic.Take(limit).ToList(),
            Hybrid = hybrid.Take(limit).ToList(),
        };
    }
}
