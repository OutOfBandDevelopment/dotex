using OoBDev.AI;
using OoBDev.AI.Models;
using OoBDev.Ollama;
using OoBDev.SBert.AllMiniLmL6V2;
using OoBDev.WebApi.Models;
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
/// Retrieval augmented generation: embeds the passages and the question in process, gives the closest passages
/// to the local model as context, and returns the answer with its sources.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class RagController(
    [FromKeyedServices(SBertGlobals.AllMiniLmL6V2Key)] IEmbeddingProvider embedding,
    [FromKeyedServices(OllamaGlobals.ProviderKey)] IMessageCompletion completion
    ) : ControllerBase
{
    /// <summary>
    /// Answers the question from the closest passages.
    /// </summary>
    /// <param name="model">The question and the passages.</param>
    /// <returns>The answer and the sources used.</returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<RagResponseModel>> Ask([FromBody] RagRequestModel model)
    {
        if (model.Passages.Count == 0)
        {
            return Problem(title: "No passages", detail: "Give at least one passage to search.", statusCode: StatusCodes.Status400BadRequest);
        }

        var question = (await embedding.GenerateEmbeddingAsync(model.Question, default, default)).ToArray();
        var scored = new List<RagSource>();
        foreach (var passage in model.Passages)
        {
            var vector = (await embedding.GenerateEmbeddingAsync(passage.Text, default, default)).ToArray();
            scored.Add(new RagSource { Id = passage.Id, Text = passage.Text, Score = TensorPrimitives.CosineSimilarity(question, vector) });
        }

        var sources = scored.OrderByDescending(s => s.Score).Take(Math.Clamp(model.TopK, 1, 10)).ToList();
        var context = string.Join("\n\n", sources.Select(s => $"[{s.Id}] {s.Text}"));
        var answer = await completion.GetCompletionAsync(new CompletionRequest
        {
            System = "Answer the question using only the context. If the context does not contain the answer, say so. Name the passage ids you used.",
            Prompt = $"Context:\n{context}\n\nQuestion: {model.Question}",
        });

        return new RagResponseModel { Answer = answer.Response, Sources = sources };
    }
}
