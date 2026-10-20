using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using OoBDev.Search.Models;
using OoBDev.Search.Semantic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OoBDev.Data.Vectors.SqlStore;

/// <summary>
/// <see cref="IVectorStoreProvider"/> over the <c>[embedding].[Vectors]</c> table: the collection is a row in
/// <c>[embedding].[Sources]</c>, the metadata is JSON in <c>[Metadata]</c> and ranking runs in SQL with <c>VectorF.Distance</c>.
/// </summary>
public class SqlVectorStoreProvider : IVectorStoreProvider
{
    private readonly IOptions<SqlVectorStoreOptions> _options;

    /// <inheritdoc/>
    public string CollectionName { get; set; }

    /// <summary>Creates a store bound to one collection.</summary>
    public SqlVectorStoreProvider(IOptions<SqlVectorStoreOptions> options, string collectionName)
    {
        _options = options;
        CollectionName = collectionName;
    }

    private SqlConnection Open() => new(_options.Value.ConnectionString);

    internal static string ToText(ReadOnlyMemory<float> v) =>
        string.Join(',', v.ToArray().Select(f => f.ToString("R", CultureInfo.InvariantCulture)));

    /// <inheritdoc/>
    public Task<string[]> StoreVectorsAsync(IEnumerable<ReadOnlyMemory<float>> embeddings, IDictionary<string, object> metadata) =>
        StoreVectorsAsync(embeddings.Select(e => (e, (IDictionary<string, object>)new Dictionary<string, object>(StringComparer.Ordinal))), metadata);

    /// <inheritdoc/>
    public async Task<string[]> StoreVectorsAsync(IEnumerable<(ReadOnlyMemory<float> embedding, IDictionary<string, object> metadata)> items, IDictionary<string, object> metadata)
    {
        var ids = new List<string>();
        await using var connection = Open();
        await connection.OpenAsync().ConfigureAwait(false);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync().ConfigureAwait(false);

        var sourceId = await ScalarAsync<int>(connection, transaction, """
            IF NOT EXISTS (SELECT 1 FROM [embedding].[Sources] WHERE [Name] = @name) INSERT INTO [embedding].[Sources] ([Name]) VALUES (@name);
            SELECT [SourceID] FROM [embedding].[Sources] WHERE [Name] = @name;
            """, ("@name", CollectionName)).ConfigureAwait(false);

        foreach (var (embedding, own) in items)
        {
            var merged = metadata.Concat(own).GroupBy(p => p.Key, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Last().Value, StringComparer.Ordinal);
            var id = await ScalarAsync<long>(connection, transaction, """
                INSERT INTO [embedding].[Vectors] ([OriginalID], [Value], [SourceID], [Metadata])
                OUTPUT inserted.[VectorID]
                VALUES (0, [embedding].[VectorF]::Parse(@vector), @source, @metadata);
                """, ("@vector", ToText(embedding)), ("@source", sourceId), ("@metadata", JsonSerializer.Serialize(merged))).ConfigureAwait(false);
            ids.Add(id.ToString(CultureInfo.InvariantCulture));
        }

        await transaction.CommitAsync().ConfigureAwait(false);
        return [.. ids];
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<SearchResultModel> ListAsync() =>
        QueryAsync("""
            SELECT TOP (@limit) v.[VectorID], CAST(0 AS float), v.[Metadata]
            FROM [embedding].[Vectors] v JOIN [embedding].[Sources] s ON s.[SourceID] = v.[SourceID]
            WHERE s.[Name] = @name ORDER BY v.[VectorID];
            """);

    /// <inheritdoc/>
    public IAsyncEnumerable<SearchResultModel> FindNeighborsAsync(ReadOnlyMemory<float> find) =>
        QueryAsync("""
            SELECT TOP (@limit) v.[VectorID], CAST(v.[Value].Distance([embedding].[VectorF]::Parse(@vector), @metric) AS float), v.[Metadata]
            FROM [embedding].[Vectors] v JOIN [embedding].[Sources] s ON s.[SourceID] = v.[SourceID]
            WHERE s.[Name] = @name AND v.[Length] = @length
            ORDER BY v.[Value].Distance([embedding].[VectorF]::Parse(@vector), @metric);
            """, ("@vector", ToText(find)), ("@length", find.Length), ("@metric", _options.Value.Metric));

    /// <inheritdoc/>
    public async IAsyncEnumerable<SearchResultModel> FindNeighborsAsync(ReadOnlyMemory<float> find, string groupBy)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        await foreach (var hit in FindNeighborsAsync(find).ConfigureAwait(false))
        {
            var key = hit.MetaData is not null && hit.MetaData.TryGetValue(groupBy, out var v)
                ? Convert.ToString(v, CultureInfo.InvariantCulture) ?? ""
                : hit.ItemId;
            if (seen.Add(key))
                yield return hit;
        }
    }

    private async IAsyncEnumerable<SearchResultModel> QueryAsync(string sql, params (string name, object value)[] parameters)
    {
        await using var connection = Open();
        await connection.OpenAsync().ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@name", CollectionName);
        command.Parameters.AddWithValue("@limit", _options.Value.Limit);
        foreach (var (name, value) in parameters)
            command.Parameters.AddWithValue(name, value);

        await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            yield return new SearchResultModel
            {
                ItemId = reader.GetInt64(0).ToString(CultureInfo.InvariantCulture),
                Score = (float)reader.GetDouble(1),
                MetaData = reader.IsDBNull(2) ? null : JsonToDictionary(reader.GetString(2)),
            };
        }
    }

    internal static Dictionary<string, object> JsonToDictionary(string json)
    {
        var result = new Dictionary<string, object>(StringComparer.Ordinal);
        using var doc = JsonDocument.Parse(json);
        foreach (var p in doc.RootElement.EnumerateObject())
        {
            object? value = p.Value.ValueKind switch
            {
                JsonValueKind.String => p.Value.GetString(),
                JsonValueKind.Number => p.Value.TryGetInt64(out var l) ? l : p.Value.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => p.Value.GetRawText(),
            };
            if (value is not null)
                result[p.Name] = value;
        }
        return result;
    }

    private static async Task<T> ScalarAsync<T>(SqlConnection connection, SqlTransaction transaction, string sql, params (string name, object value)[] parameters)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        foreach (var (name, value) in parameters)
            command.Parameters.AddWithValue(name, value);
        return (T)Convert.ChangeType((await command.ExecuteScalarAsync().ConfigureAwait(false))!, typeof(T), CultureInfo.InvariantCulture);
    }
}
