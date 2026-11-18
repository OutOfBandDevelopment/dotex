using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Npgsql;
using OoBDev.WorkQueue;

namespace OoBDev.PostgreSql.WorkQueue;

/// <summary>
/// The PostgreSQL <see cref="IWorkerEvents"/>: events are rows, each reader keeps a high-water mark row, and the retention
/// deletes by count and age. The table is the source of truth; no queue extension is needed.
/// </summary>
public sealed class PostgresWorkerEvents : IWorkerEvents, IAsyncDisposable
{
    private readonly WorkerEventsOptions _retention;
    private readonly TimeProvider _clock;
    private readonly string _events;
    private readonly string _readers;
    private readonly Lazy<NpgsqlDataSource> _source;

    /// <summary>Creates the event store; the connection pool is opened on first use.</summary>
    /// <param name="retention">The retention options.</param>
    /// <param name="postgres">The table names and connection string.</param>
    /// <param name="clock">The time source.</param>
    public PostgresWorkerEvents(IOptions<WorkerEventsOptions> retention, IOptions<PostgresWorkQueueOptions> postgres, TimeProvider clock)
    {
        _retention = retention.Value;
        _clock = clock;
        var settings = postgres.Value;
        var schema = PostgresWorkQueueSchema.Quote(settings.Schema);
        _events = $"{schema}.{PostgresWorkQueueSchema.Quote(settings.EventsTable)}";
        _readers = $"{schema}.{PostgresWorkQueueSchema.Quote(settings.EventReadersTable)}";
        var connectionString = settings.ConnectionString;
        _source = new Lazy<NpgsqlDataSource>(() => NpgsqlDataSource.Create(connectionString));
    }

    /// <summary>Creates the schema and tables when they do not exist yet.</summary>
    /// <param name="options">The names to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the script ran.</returns>
    public async Task MigrateAsync(PostgresWorkQueueOptions options, CancellationToken cancellationToken = default)
    {
        await using var command = _source.Value.CreateCommand(PostgresWorkQueueSchema.CreateScript(options));
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> PublishAsync(string source, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var command = _source.Value.CreateCommand($"INSERT INTO {_events} (at, source, message) VALUES (@at, @source, @message)");
            command.Parameters.AddWithValue("at", _clock.GetUtcNow());
            command.Parameters.AddWithValue("source", source);
            command.Parameters.AddWithValue("message", message);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (Exception e) when (e is NpgsqlException or InvalidOperationException)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WorkerEvent>> ReadAsync(string reader, int limit, bool fromStart = false, CancellationToken cancellationToken = default)
    {
        await using var connection = await _source.Value.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await using (var init = new NpgsqlCommand(
            $"INSERT INTO {_readers} (reader, last_id, updated) VALUES (@reader, CASE WHEN @start THEN 0 ELSE COALESCE((SELECT max(id) FROM {_events}), 0) END, @now) ON CONFLICT DO NOTHING",
            connection, transaction))
        {
            init.Parameters.AddWithValue("reader", reader);
            init.Parameters.AddWithValue("start", fromStart);
            init.Parameters.AddWithValue("now", _clock.GetUtcNow());
            await init.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        var list = new List<WorkerEvent>();
        await using (var query = new NpgsqlCommand(
            $"SELECT e.id, e.at, e.source, e.message FROM {_events} e WHERE e.id > (SELECT last_id FROM {_readers} WHERE reader = @reader FOR UPDATE) ORDER BY e.id LIMIT @limit",
            connection, transaction))
        {
            query.Parameters.AddWithValue("reader", reader);
            query.Parameters.AddWithValue("limit", limit);
            await using var rows = await query.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await rows.ReadAsync(cancellationToken).ConfigureAwait(false))
                list.Add(new WorkerEvent(rows.GetInt64(0), rows.GetFieldValue<DateTimeOffset>(1), rows.GetString(2), rows.GetString(3)));
        }

        await using (var mark = new NpgsqlCommand($"UPDATE {_readers} SET last_id = GREATEST(last_id, @id), updated = @now WHERE reader = @reader", connection, transaction))
        {
            mark.Parameters.AddWithValue("id", list.Count > 0 ? list[^1].Id : 0L);
            mark.Parameters.AddWithValue("now", _clock.GetUtcNow());
            mark.Parameters.AddWithValue("reader", reader);
            await mark.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return list;
    }

    /// <inheritdoc />
    public async Task<int> TrimAsync(CancellationToken cancellationToken = default)
    {
        var cut = _clock.GetUtcNow() - _retention.MaxAge;
        await using var connection = await _source.Value.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        int deleted;
        await using (var command = new NpgsqlCommand(
            $"DELETE FROM {_events} WHERE at < @cut OR id <= COALESCE((SELECT id FROM {_events} ORDER BY id DESC OFFSET @max LIMIT 1), 0)", connection))
        {
            command.Parameters.AddWithValue("cut", cut);
            command.Parameters.AddWithValue("max", _retention.MaxRecords);
            deleted = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await using var idle = new NpgsqlCommand($"DELETE FROM {_readers} WHERE updated < @cut", connection);
        idle.Parameters.AddWithValue("cut", cut);
        await idle.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return deleted;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_source.IsValueCreated)
            await _source.Value.DisposeAsync().ConfigureAwait(false);
    }
}
