using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Npgsql;
using OoBDev.WorkQueue;

namespace OoBDev.PostgreSql.WorkQueue;

/// <summary>
/// The PostgreSQL <see cref="IWorkQueue"/>. Claims use <c>FOR UPDATE SKIP LOCKED</c>, every later call is fenced by
/// status, attempt and worker, and all times come from the <see cref="TimeProvider"/> so tests can drive them.
/// </summary>
public sealed class PostgresWorkQueue : IWorkQueue, IAsyncDisposable
{
    private const int ErrorLength = 500;

    private readonly WorkQueueOptions _queue;
    private readonly IPostgresClaimOrder _order;
    private readonly TimeProvider _clock;
    private readonly string _tasks;
    private readonly string _dead;
    private readonly Lazy<NpgsqlDataSource> _source;

    /// <summary>Creates the queue; the connection pool is opened on first use.</summary>
    /// <param name="queue">The queue options (lease, attempts, back-off).</param>
    /// <param name="postgres">The table names and connection string.</param>
    /// <param name="order">The claim order.</param>
    /// <param name="clock">The time source.</param>
    public PostgresWorkQueue(
        IOptions<WorkQueueOptions> queue,
        IOptions<PostgresWorkQueueOptions> postgres,
        IPostgresClaimOrder order,
        TimeProvider clock)
    {
        _queue = queue.Value;
        _order = order;
        _clock = clock;
        var settings = postgres.Value;
        var schema = PostgresWorkQueueSchema.Quote(settings.Schema);
        _tasks = $"{schema}.{PostgresWorkQueueSchema.Quote(settings.TasksTable)}";
        _dead = $"{schema}.{PostgresWorkQueueSchema.Quote(settings.DeadLettersTable)}";
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

    private string Due => """
        ((status = 'pending' AND (next_attempt_at IS NULL OR next_attempt_at <= @now))
         OR (status = 'running' AND (COALESCE(lease_until, updated + @lease) <= @now OR claimed_at + @runtime <= @now)))
        """;

    private const string Held = "stage = @stage AND item_key = @key AND status = 'running' AND attempts = @attempt AND claimed_by = @worker";

    /// <inheritdoc />
    public async Task<bool> EnqueueAsync(string stage, string key, CancellationToken cancellationToken = default)
    {
        var now = _clock.GetUtcNow();
        await using var command = _source.Value.CreateCommand($"""
            INSERT INTO {_tasks} (stage, item_key, status, attempts, created, updated)
            VALUES (@stage, @key, 'pending', 0, @now, @now)
            ON CONFLICT (stage, item_key) DO NOTHING
            """);
        command.Parameters.AddWithValue("stage", stage);
        command.Parameters.AddWithValue("key", key);
        command.Parameters.AddWithValue("now", now);
        return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) > 0;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WorkItem>> ClaimAsync(string stage, string worker, int batch, CancellationToken cancellationToken = default)
    {
        var now = _clock.GetUtcNow();
        var sla = _queue.For(stage);
        await using var connection = await _source.Value.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        await using (var sweep = new NpgsqlCommand($"""
            WITH swept AS (
                UPDATE {_tasks}
                SET status = CASE WHEN status = 'running' THEN 'parked' ELSE 'dead' END,
                    lease_until = NULL,
                    detail = CASE WHEN status = 'running'
                                  THEN 'parked: hung, lease expired after ' || attempts || ' attempts'
                                  ELSE 'dead: attempts exhausted' END,
                    updated = @now
                WHERE stage = @stage AND attempts >= @max AND {Due}
                RETURNING item_key, attempts, claimed_by, status)
            INSERT INTO {_dead} (stage, item_key, attempts, reason, last_error, claimed_by, died_at)
            SELECT @stage, item_key, attempts,
                   CASE WHEN status = 'parked' THEN 'lease expired' ELSE 'attempts exhausted' END,
                   NULL, claimed_by, @now
            FROM swept
            """, connection, transaction))
        {
            AddTiming(sweep, now, sla);
            sweep.Parameters.AddWithValue("stage", stage);
            sweep.Parameters.AddWithValue("max", sla.MaxAttempts);
            await sweep.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        var items = new List<WorkItem>();
        await using (var claim = new NpgsqlCommand($"""
            WITH picked AS (
                SELECT item_key FROM {_tasks}
                WHERE stage = @stage AND attempts < @max AND {Due}
                ORDER BY {_order.OrderBy(stage)}
                FOR UPDATE SKIP LOCKED
                LIMIT @batch)
            UPDATE {_tasks} t
            SET status = 'running', attempts = t.attempts + 1, claimed_by = @worker, claimed_at = @now,
                lease_until = @now + @lease, next_attempt_at = NULL, updated = @now
            FROM picked
            WHERE t.stage = @stage AND t.item_key = picked.item_key
            RETURNING t.item_key, t.attempts, t.lease_until
            """, connection, transaction))
        {
            AddTiming(claim, now, sla);
            claim.Parameters.AddWithValue("stage", stage);
            claim.Parameters.AddWithValue("max", sla.MaxAttempts);
            claim.Parameters.AddWithValue("batch", batch);
            claim.Parameters.AddWithValue("worker", worker);
            await using var reader = await claim.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                items.Add(new WorkItem(stage, reader.GetString(0), reader.GetInt32(1), worker, reader.GetFieldValue<DateTimeOffset>(2)));
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return items;
    }

    /// <inheritdoc />
    public async Task<bool> HeartbeatAsync(WorkItem item, CancellationToken cancellationToken = default)
    {
        var now = _clock.GetUtcNow();
        var sla = _queue.For(item.Stage);
        await using var command = _source.Value.CreateCommand($"""
            UPDATE {_tasks} SET lease_until = @now + @lease, updated = @now
            WHERE {Held} AND claimed_at + @runtime > @now
            """);
        AddTiming(command, now, sla);
        AddFence(command, item);
        return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) > 0;
    }

    /// <inheritdoc />
    public async Task<bool> CompleteAsync(WorkItem item, string status = WorkStatus.Done, string? detail = null, CancellationToken cancellationToken = default)
    {
        if (status is not (WorkStatus.Done or WorkStatus.Skipped))
            throw new ArgumentOutOfRangeException(nameof(status));

        await using var command = _source.Value.CreateCommand($"""
            UPDATE {_tasks} SET status = @status, detail = @detail, lease_until = NULL, updated = @now
            WHERE {Held}
            """);
        AddFence(command, item);
        command.Parameters.AddWithValue("status", status);
        command.Parameters.AddWithValue("detail", NpgsqlTypes.NpgsqlDbType.Text, (object?)detail ?? DBNull.Value);
        command.Parameters.AddWithValue("now", _clock.GetUtcNow());
        return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) > 0;
    }

    /// <inheritdoc />
    public async Task<string?> FailAsync(WorkItem item, string error, bool retryable = true, CancellationToken cancellationToken = default)
    {
        var now = _clock.GetUtcNow();
        var dies = !retryable || item.Attempt >= _queue.For(item.Stage).MaxAttempts;
        var text = error.Length <= ErrorLength ? error : error[..ErrorLength];

        await using var command = _source.Value.CreateCommand($"""
            WITH failed AS (
                UPDATE {_tasks}
                SET status = CASE WHEN @dies THEN 'dead' ELSE 'pending' END,
                    detail = @error, lease_until = NULL,
                    next_attempt_at = CASE WHEN @dies THEN NULL ELSE @now + @backoff END,
                    updated = @now
                WHERE {Held}
                RETURNING status),
            buried AS (
                INSERT INTO {_dead} (stage, item_key, attempts, reason, last_error, claimed_by, died_at)
                SELECT @stage, @key, @attempt, @reason, @error, @worker, @now
                WHERE EXISTS (SELECT 1 FROM failed WHERE status = 'dead'))
            SELECT status FROM failed
            """);
        AddFence(command, item);
        command.Parameters.AddWithValue("dies", dies);
        command.Parameters.AddWithValue("error", text);
        command.Parameters.AddWithValue("now", now);
        command.Parameters.AddWithValue("backoff", _queue.Backoff(item.Attempt));
        command.Parameters.AddWithValue("reason", retryable ? "attempts exhausted" : "non-retryable");
        return await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false) as string;
    }

    /// <inheritdoc />
    public async Task<int> RedriveAsync(string stage, string? key = null, CancellationToken cancellationToken = default)
    {
        await using var command = _source.Value.CreateCommand($"""
            WITH back AS (
                UPDATE {_tasks}
                SET status = 'pending', attempts = 0, lease_until = NULL, next_attempt_at = NULL, claimed_by = NULL,
                    detail = 'redriven from dead letters', updated = @now
                WHERE stage = @stage AND status IN ('dead', 'parked') AND (@key::text IS NULL OR item_key = @key)
                RETURNING item_key),
            marked AS (
                UPDATE {_dead} SET redriven_at = @now
                WHERE stage = @stage AND redriven_at IS NULL AND (@key::text IS NULL OR item_key = @key))
            SELECT count(*) FROM back
            """);
        command.Parameters.AddWithValue("stage", stage);
        command.Parameters.AddWithValue("key", NpgsqlTypes.NpgsqlDbType.Text, (object?)key ?? DBNull.Value);
        command.Parameters.AddWithValue("now", _clock.GetUtcNow());
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false), global::System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public async Task<StageStats> StatsAsync(string stage, CancellationToken cancellationToken = default)
    {
        var sla = _queue.For(stage);
        await using var command = _source.Value.CreateCommand($"""
            SELECT count(*) FILTER (WHERE attempts < @max AND {Due}),
                   count(*) FILTER (WHERE status = 'running'),
                   count(*) FILTER (WHERE status = 'running'
                                    AND (COALESCE(lease_until, updated + @lease) <= @now OR claimed_at + @runtime <= @now)),
                   count(*) FILTER (WHERE status = 'parked'),
                   count(*) FILTER (WHERE status = 'dead')
            FROM {_tasks} WHERE stage = @stage
            """);
        AddTiming(command, _clock.GetUtcNow(), sla);
        command.Parameters.AddWithValue("stage", stage);
        command.Parameters.AddWithValue("max", sla.MaxAttempts);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
        return new StageStats((int)reader.GetInt64(0), (int)reader.GetInt64(1), (int)reader.GetInt64(2), (int)reader.GetInt64(3), (int)reader.GetInt64(4));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DeadLetter>> DeadLettersAsync(string stage, CancellationToken cancellationToken = default)
    {
        await using var command = _source.Value.CreateCommand($"""
            SELECT item_key, attempts, reason, last_error, claimed_by, died_at, redriven_at
            FROM {_dead} WHERE stage = @stage ORDER BY died_at DESC, id DESC
            """);
        command.Parameters.AddWithValue("stage", stage);
        var list = new List<DeadLetter>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            list.Add(new DeadLetter(
                stage,
                reader.GetString(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.GetFieldValue<DateTimeOffset>(5),
                reader.IsDBNull(6) ? null : reader.GetFieldValue<DateTimeOffset>(6)));
        }

        return list;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => _source.IsValueCreated ? _source.Value.DisposeAsync() : ValueTask.CompletedTask;

    private static void AddTiming(NpgsqlCommand command, DateTimeOffset now, StageSla sla)
    {
        command.Parameters.AddWithValue("now", now);
        command.Parameters.AddWithValue("lease", sla.Lease);
        command.Parameters.AddWithValue("runtime", sla.MaxRuntime);
    }

    private static void AddFence(NpgsqlCommand command, WorkItem item)
    {
        command.Parameters.AddWithValue("stage", item.Stage);
        command.Parameters.AddWithValue("key", item.Key);
        command.Parameters.AddWithValue("attempt", item.Attempt);
        command.Parameters.AddWithValue("worker", item.Worker);
    }
}
