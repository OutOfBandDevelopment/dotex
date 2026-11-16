using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Npgsql;
using OoBDev.Query.Metadata;

namespace OoBDev.PostgreSql.Query;

/// <summary>
/// Runs compiled statements through a lazily created <see cref="NpgsqlDataSource"/>. Values are always sent as parameters.
/// </summary>
/// <param name="options">The options.</param>
public sealed class NpgsqlQueryExecutor(IOptions<PostgresQueryOptions> options) : IQueryExecutor, IAsyncDisposable
{
    private readonly Lazy<NpgsqlDataSource> _source = new(() => NpgsqlDataSource.Create(options.Value.ConnectionString), LazyThreadSafetyMode.ExecutionAndPublication);

    /// <inheritdoc />
    public async Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ReadAsync(string sql, IReadOnlyDictionary<string, object?> parameters, CancellationToken cancellationToken)
    {
        await using var command = _source.Value.CreateCommand(sql);
        command.CommandTimeout = options.Value.CommandTimeoutSeconds;
        foreach (var (name, value) in parameters)
            command.Parameters.Add(new NpgsqlParameter(name, value ?? DBNull.Value));

        var rows = new List<IReadOnlyDictionary<string, object?>>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var row = new Dictionary<string, object?>(reader.FieldCount, StringComparer.Ordinal);
            for (var i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = await reader.IsDBNullAsync(i, cancellationToken).ConfigureAwait(false) ? null : reader.GetValue(i);
            rows.Add(row);
        }

        return rows;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => _source.IsValueCreated ? _source.Value.DisposeAsync() : ValueTask.CompletedTask;
}
