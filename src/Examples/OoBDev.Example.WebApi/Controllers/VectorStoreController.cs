using OoBDev.AI;
using OoBDev.Search.Semantic;
using OoBDev.SBert.AllMiniLmL6V2;
using OoBDev.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.WebApi.Controllers;

/// <summary>
/// Stores text embeddings in a vector store collection and finds the nearest ones. The store is SQL Server when
/// <c>SqlVectorStoreOptions:ConnectionString</c> is configured, otherwise in memory.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class VectorStoreController(
    [FromKeyedServices(SBertGlobals.AllMiniLmL6V2Key)] IEmbeddingProvider embedding,
    IVectorStoreProviderFactory stores
    ) : ControllerBase
{
    private const int MaxTexts = 100;

    /// <summary>Embeds the texts and stores them in the collection.</summary>
    /// <param name="model">The collection and texts.</param>
    /// <returns>The stored ids.</returns>
    [HttpPost("index")]
    [AllowAnonymous]
    public async Task<ActionResult<VectorIndexResponseModel>> Index([FromBody] VectorIndexRequestModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Collection) || model.Texts.Count == 0 || model.Texts.Count > MaxTexts)
            return Problem(title: "Invalid request", detail: $"Give a collection and 1 to {MaxTexts} texts.", statusCode: StatusCodes.Status400BadRequest);

        var items = new List<(ReadOnlyMemory<float>, IDictionary<string, object>)>();
        foreach (var text in model.Texts)
        {
            var vector = (await embedding.GenerateEmbeddingAsync(text, default, default)).ToArray();
            items.Add((vector, new Dictionary<string, object> { ["text"] = text }));
        }

        var ids = await stores.Create(model.Collection).StoreVectorsAsync(items, new Dictionary<string, object>());
        return new VectorIndexResponseModel { Ids = ids, Store = stores is Services.InMemoryVectorStoreProviderFactory ? "memory" : "sql" };
    }

    /// <summary>Finds the stored texts closest to the query.</summary>
    /// <param name="model">The collection, query and limit.</param>
    /// <returns>The nearest hits, closest first.</returns>
    [HttpPost("query")]
    [AllowAnonymous]
    public async Task<ActionResult<List<VectorHitModel>>> Query([FromBody] VectorQueryRequestModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Collection) || string.IsNullOrWhiteSpace(model.Query))
            return Problem(title: "Invalid request", detail: "Give a collection and a query.", statusCode: StatusCodes.Status400BadRequest);

        var vector = (await embedding.GenerateEmbeddingAsync(model.Query, default, default)).ToArray();
        var hits = new List<VectorHitModel>();
        await foreach (var hit in stores.Create(model.Collection).FindNeighborsAsync(vector))
        {
            hits.Add(ToHit(hit));
            if (hits.Count >= Math.Clamp(model.Limit, 1, 50))
                break;
        }
        return hits;
    }

    /// <summary>Lists what is stored in the collection.</summary>
    /// <param name="collection">The collection name.</param>
    /// <returns>The stored items.</returns>
    [HttpGet("{collection}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<VectorHitModel>>> List(string collection)
    {
        var hits = new List<VectorHitModel>();
        await foreach (var hit in stores.Create(collection).ListAsync())
            hits.Add(ToHit(hit));
        return hits;
    }

    private static VectorHitModel ToHit(OoBDev.Search.Models.SearchResultModel hit) => new()
    {
        Id = hit.ItemId,
        Score = hit.Score,
        Text = hit.MetaData is not null && hit.MetaData.TryGetValue("text", out var t) ? t?.ToString() : null,
    };
}
