using OoBDev.Search.Models;
using OoBDev.Search.Semantic;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.Search.Tests.Semantic;

internal sealed class FakeVectorStoreProvider : IVectorStoreProvider
{
    public string CollectionName { get; set; } = string.Empty;
    public List<string> Calls { get; } = [];

    public Task<string[]> StoreVectorsAsync(IEnumerable<ReadOnlyMemory<float>> embeddings, IDictionary<string, object> metadata)
    {
        Calls.Add(nameof(StoreVectorsAsync) + ":embeddings");
        return Task.FromResult(new[] { "a" });
    }

    public Task<string[]> StoreVectorsAsync(IEnumerable<(ReadOnlyMemory<float> embedding, IDictionary<string, object> metadata)> items, IDictionary<string, object> metadata)
    {
        Calls.Add(nameof(StoreVectorsAsync) + ":items");
        return Task.FromResult(new[] { "b" });
    }

    public Task DeleteAsync(IEnumerable<string> ids)
    {
        Calls.Add(nameof(DeleteAsync) + ":" + string.Join(",", ids));
        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<SearchResultModel> ListAsync()
    {
        Calls.Add(nameof(ListAsync));
        await Task.Yield();
        yield return new SearchResultModel { Score = 1, ItemId = "listed" };
    }

    public async IAsyncEnumerable<SearchResultModel> FindNeighborsAsync(ReadOnlyMemory<float> find)
    {
        Calls.Add(nameof(FindNeighborsAsync));
        await Task.Yield();
        yield return new SearchResultModel { Score = 0.5f, ItemId = "near" };
    }

    public async IAsyncEnumerable<SearchResultModel> FindNeighborsAsync(ReadOnlyMemory<float> find, string groupBy)
    {
        Calls.Add(nameof(FindNeighborsAsync) + ":" + groupBy);
        await Task.Yield();
        yield return new SearchResultModel { Score = 0.25f, ItemId = "grouped" };
    }
}

internal sealed class FakeVectorStoreProviderFactory(IVectorStoreProvider? result) : IVectorStoreProviderFactory
{
    public string? LastName { get; private set; }

    public IVectorStoreProvider Create(string containerName)
    {
        LastName = containerName;
        return result!;
    }
}
