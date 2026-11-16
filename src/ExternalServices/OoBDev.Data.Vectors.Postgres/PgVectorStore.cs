using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using OoBDev.Search;
using OoBDev.Search.Models;
using Pgvector;
using Pgvector.Npgsql;

namespace OoBDev.Data.Vectors.Postgres;

/// <summary>
/// <see cref="IVectorStore"/> on PostgreSQL with pgvector. An item owns one or more vectors (one per page, for example);
/// searches return the best vector per item and <see cref="FindNeighborsWithExplanationAsync"/> says which one matched.
/// </summary>
/// <remarks>
/// <see cref="StoreVectorsAsync(IEnumerable{ReadOnlyMemory{float}}, IDictionary{string, object})"/> stores the vectors as ONE item and returns its id;
/// the items overload stores one item per entry and returns their ids. Metadata is JSON, so values read back are <see cref="JsonElement"/>.
/// Nothing creates a schema or an index implicitly: call <see cref="EnsureCreatedAsync"/> and <see cref="CreateHnswIndexAsync"/>.
/// </remarks>
/// <param name="options">The options.</param>
public sealed class PgVectorStore(IOptions<PgVectorOptions> options) : IVectorStore, IAsyncDisposable
{
    private readonly PgVectorOptions _options = options.Value;
    private readonly PgVectorSql _sql = new(options.Value);
    private readonly Lazy<NpgsqlDataSource> _source = new(() =>
    {
        var builder = new NpgsqlDataSourceBuilder(options.Value.ConnectionString);
        builder.UseVector();
        return builder.Build();
    }, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>Creates the extension, table and item index when missing. Never creates the HNSW index.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        await Execute(_sql.Extension, cancellationToken).ConfigureAwait(false);
        await using (var connection = await _source.Value.OpenConnectionAsync(cancellationToken).ConfigureAwait(false))
            await connection.ReloadTypesAsync(cancellationToken).ConfigureAwait(false);
        await Execute(_sql.CreateTable, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Creates the HNSW index. Building one on a large table is slow, so this is only ever called explicitly.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task CreateHnswIndexAsync(CancellationToken cancellationToken = default) => Execute(_sql.CreateHnswIndex, cancellationToken);

    /// <inheritdoc />
    public async Task<string[]> StoreVectorsAsync(IEnumerable<ReadOnlyMemory<float>> embeddings, IDictionary<string, object> metadata)
    {
        var itemId = NewId();
        await InsertAsync([(itemId, embeddings.ToArray(), metadata)]).ConfigureAwait(false);
        return [itemId];
    }

    /// <inheritdoc />
    public async Task<string[]> StoreVectorsAsync(IEnumerable<(ReadOnlyMemory<float> embedding, IDictionary<string, object> metadata)> items, IDictionary<string, object> metadata)
    {
        var rows = items.Select(i => (Id: NewId(), Vectors: new[] { i.embedding }, Metadata: (IDictionary<string, object>)Merge(metadata, i.metadata))).ToArray();
        await InsertAsync(rows).ConfigureAwait(false);
        return rows.Select(r => r.Id).ToArray();
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<SearchResultModel> ListAsync()
    {
        await using var command = _source.Value.CreateCommand(_sql.List);
        command.CommandTimeout = _options.CommandTimeoutSeconds;
        await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false))
            yield return new SearchResultModel { Score = 0, ItemId = reader.GetString(0), MetaData = ReadMetadata(reader.GetString(1)) };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<SearchResultModel> FindNeighborsAsync(ReadOnlyMemory<float> find)
    {
        await foreach (var match in QueryAsync(_sql.NeighboursBestPerItem, find, null, default).ConfigureAwait(false))
            yield return match.Result;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<SearchResultModel> FindNeighborsAsync(ReadOnlyMemory<float> find, string groupBy)
    {
        await foreach (var match in QueryAsync(_sql.NeighboursBestPerGroup, find, groupBy, default).ConfigureAwait(false))
            yield return match.Result;
    }

    /// <summary>The best vector of each item nearest to <paramref name="find"/>, with the index of the vector that matched and its distance.</summary>
    /// <param name="find">The query vector.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Matches, nearest first.</returns>
    public IAsyncEnumerable<VectorMatch> FindNeighborsWithExplanationAsync(ReadOnlyMemory<float> find, CancellationToken cancellationToken = default) =>
        QueryAsync(_sql.NeighboursBestPerItem, find, null, cancellationToken);

    /// <inheritdoc />
    public ValueTask DisposeAsync() => _source.IsValueCreated ? _source.Value.DisposeAsync() : ValueTask.CompletedTask;

    private async IAsyncEnumerable<VectorMatch> QueryAsync(string sql, ReadOnlyMemory<float> find, string? group, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var command = _source.Value.CreateCommand(sql);
        command.CommandTimeout = _options.CommandTimeoutSeconds;
        command.Parameters.Add(new NpgsqlParameter("query", Embedding(find)));
        command.Parameters.AddWithValue("limit", _options.DefaultLimit);
        if (group is not null)
            command.Parameters.AddWithValue("group", group);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var distance = reader.GetDouble(3);
            var result = new SearchResultModel { Score = (float)_sql.Score(distance), ItemId = reader.GetString(0), MetaData = ReadMetadata(reader.GetString(2)) };
            yield return new VectorMatch(result, reader.GetInt32(1), distance);
        }
    }

    private async Task InsertAsync((string Id, ReadOnlyMemory<float>[] Vectors, IDictionary<string, object> Metadata)[] items)
    {
        foreach (var vector in items.SelectMany(i => i.Vectors))
            if (vector.Length != _options.Dimensions)
                throw new ArgumentException($"A vector has {vector.Length} dimensions; the store is configured for {_options.Dimensions}.");

        await using var connection = await _source.Value.OpenConnectionAsync().ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync().ConfigureAwait(false);
        foreach (var (id, vectors, metadata) in items)
        {
            var json = JsonSerializer.Serialize(metadata);
            for (var index = 0; index < vectors.Length; index++)
            {
                await using var command = new NpgsqlCommand(_sql.Insert, connection, transaction) { CommandTimeout = _options.CommandTimeoutSeconds };
                command.Parameters.AddWithValue("id", Guid.NewGuid());
                command.Parameters.AddWithValue("item", id);
                command.Parameters.AddWithValue("idx", index);
                command.Parameters.Add(new NpgsqlParameter("embedding", Embedding(vectors[index])));
                command.Parameters.Add(new NpgsqlParameter("metadata", NpgsqlDbType.Jsonb) { Value = json });
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        await transaction.CommitAsync().ConfigureAwait(false);
    }

    private object Embedding(ReadOnlyMemory<float> vector) => _options.UseHalfVector
        ? new HalfVector(vector.ToArray().Select(f => (Half)f).ToArray())
        : new Vector(vector);

    private async Task Execute(string sql, CancellationToken cancellationToken)
    {
        await using var command = _source.Value.CreateCommand(sql);
        command.CommandTimeout = _options.CommandTimeoutSeconds;
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string NewId() => Guid.NewGuid().ToString("N");

    private static Dictionary<string, object> Merge(IDictionary<string, object> shared, IDictionary<string, object> own)
    {
        var merged = new Dictionary<string, object>(shared, StringComparer.Ordinal);
        foreach (var (key, value) in own)
            merged[key] = value;
        return merged;
    }

    private static IDictionary<string, object>? ReadMetadata(string json) => JsonSerializer.Deserialize<Dictionary<string, object>>(json);
}
