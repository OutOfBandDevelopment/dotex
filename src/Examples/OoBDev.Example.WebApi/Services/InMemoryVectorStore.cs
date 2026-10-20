using OoBDev.Search.Models;
using OoBDev.Search.Semantic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics.Tensors;
using System.Threading.Tasks;

namespace OoBDev.WebApi.Services;

/// <summary>
/// Keeps collections in process, ranked by cosine distance. The default when no SQL Server connection string is configured.
/// </summary>
public sealed class InMemoryVectorStoreProviderFactory : IVectorStoreProviderFactory
{
    private static readonly ConcurrentDictionary<string, List<(string Id, float[] Vector, IDictionary<string, object> Metadata)>> Collections = new(StringComparer.Ordinal);

    /// <inheritdoc/>
    public IVectorStoreProvider Create(string containerName) => new Provider(containerName, Collections.GetOrAdd(containerName, _ => []));

    private sealed class Provider(string name, List<(string Id, float[] Vector, IDictionary<string, object> Metadata)> rows) : IVectorStoreProvider
    {
        public string CollectionName { get; set; } = name;

        public Task<string[]> StoreVectorsAsync(IEnumerable<ReadOnlyMemory<float>> embeddings, IDictionary<string, object> metadata) =>
            StoreVectorsAsync(embeddings.Select(e => (e, (IDictionary<string, object>)new Dictionary<string, object>(StringComparer.Ordinal))), metadata);

        public Task<string[]> StoreVectorsAsync(IEnumerable<(ReadOnlyMemory<float> embedding, IDictionary<string, object> metadata)> items, IDictionary<string, object> metadata)
        {
            var ids = new List<string>();
            lock (rows)
            {
                foreach (var (embedding, own) in items)
                {
                    var id = (rows.Count + 1).ToString(CultureInfo.InvariantCulture);
                    var merged = new Dictionary<string, object>(metadata, StringComparer.Ordinal);
                    foreach (var p in own)
                        merged[p.Key] = p.Value;
                    rows.Add((id, embedding.ToArray(), merged));
                    ids.Add(id);
                }
            }
            return Task.FromResult(ids.ToArray());
        }

        private List<(string Id, float[] Vector, IDictionary<string, object> Metadata)> Snapshot()
        {
            lock (rows)
                return [.. rows];
        }

        public async IAsyncEnumerable<SearchResultModel> ListAsync()
        {
            await Task.CompletedTask.ConfigureAwait(false);
            foreach (var r in Snapshot())
                yield return new SearchResultModel { ItemId = r.Id, Score = 0, MetaData = r.Metadata };
        }

        public async IAsyncEnumerable<SearchResultModel> FindNeighborsAsync(ReadOnlyMemory<float> find)
        {
            await Task.CompletedTask.ConfigureAwait(false);
            var query = find.ToArray();
            foreach (var r in Snapshot().Where(r => r.Vector.Length == query.Length)
                .Select(r => (r, Distance: 1f - TensorPrimitives.CosineSimilarity(query, r.Vector)))
                .OrderBy(x => x.Distance))
                yield return new SearchResultModel { ItemId = r.r.Id, Score = r.Distance, MetaData = r.r.Metadata };
        }

        public async IAsyncEnumerable<SearchResultModel> FindNeighborsAsync(ReadOnlyMemory<float> find, string groupBy)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            await foreach (var hit in FindNeighborsAsync(find).ConfigureAwait(false))
            {
                var key = hit.MetaData is not null && hit.MetaData.TryGetValue(groupBy, out var v) ? Convert.ToString(v, CultureInfo.InvariantCulture) ?? "" : hit.ItemId;
                if (seen.Add(key))
                    yield return hit;
            }
        }
    }
}
