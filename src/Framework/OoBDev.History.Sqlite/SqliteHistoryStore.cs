using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace OoBDev.History.Sqlite;

/// <summary>
/// History store on one SQLite database. Create it with <see cref="OpenAsync"/>; the connection stays open for the life of the store
/// and all calls are serialised, which matches SQLite's single writer.
/// </summary>
public sealed class SqliteHistoryStore : IHistoryStore, IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly HistoryOptions _options;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private SqliteHistoryStore(SqliteConnection connection, HistoryOptions options)
    {
        _connection = connection;
        _options = options;
    }

    public static async Task<SqliteHistoryStore> OpenAsync(string connectionString, HistoryOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var connection = new SqliteConnection(connectionString);
        try
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            await ExecuteAsync(connection, """
                PRAGMA journal_mode = WAL;
                CREATE TABLE IF NOT EXISTS HistoryRaw (Series TEXT NOT NULL, At INTEGER NOT NULL, Value REAL NOT NULL);
                CREATE INDEX IF NOT EXISTS IX_HistoryRaw ON HistoryRaw (Series, At);
                CREATE TABLE IF NOT EXISTS HistoryBucket (
                    Series TEXT NOT NULL, Tier TEXT NOT NULL, Start INTEGER NOT NULL,
                    Count INTEGER NOT NULL, Minimum REAL NOT NULL, Maximum REAL NOT NULL, Sum REAL NOT NULL, Last REAL NOT NULL, LastAt INTEGER NOT NULL,
                    PRIMARY KEY (Series, Tier, Start));
                """, cancellationToken).ConfigureAwait(false);
            return new SqliteHistoryStore(connection, options);
        }
        catch
        {
            await connection.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    public async Task AppendAsync(string series, HistoryPoint point, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(series);
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await using var transaction = (SqliteTransaction)await _connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            await ExecuteAsync(_connection, "INSERT INTO HistoryRaw (Series, At, Value) VALUES ($s, $at, $v)", cancellationToken, transaction, ("$s", series), ("$at", point.Timestamp.UtcTicks), ("$v", point.Value)).ConfigureAwait(false);
            foreach (var tier in _options.Tiers)
            {
                await ExecuteAsync(_connection, """
                    INSERT INTO HistoryBucket (Series, Tier, Start, Count, Minimum, Maximum, Sum, Last, LastAt)
                    VALUES ($s, $t, $start, 1, $v, $v, $v, $v, $at)
                    ON CONFLICT (Series, Tier, Start) DO UPDATE SET
                        Count = Count + 1,
                        Minimum = MIN(Minimum, $v),
                        Maximum = MAX(Maximum, $v),
                        Sum = Sum + $v,
                        Last = CASE WHEN $at >= LastAt THEN $v ELSE Last END,
                        LastAt = MAX(LastAt, $at)
                    """, cancellationToken, transaction, ("$s", series), ("$t", tier.Name), ("$start", tier.StartOf(point.Timestamp).UtcTicks), ("$v", point.Value), ("$at", point.Timestamp.UtcTicks)).ConfigureAwait(false);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyList<HistoryPoint>> ReadRawAsync(string series, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await using var command = Command("SELECT At, Value FROM HistoryRaw WHERE Series = $s AND At >= $from AND At < $to ORDER BY At, rowid", null, ("$s", series), ("$from", from.UtcTicks), ("$to", to.UtcTicks));
            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            var result = new List<HistoryPoint>();
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                result.Add(new HistoryPoint(new DateTimeOffset(reader.GetInt64(0), TimeSpan.Zero), reader.GetDouble(1)));
            }

            return result;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyList<HistoryBucket>> ReadBucketsAsync(string series, string tier, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default)
    {
        var name = _options.GetTier(tier).Name;
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await using var command = Command("SELECT Start, Count, Minimum, Maximum, Sum, Last FROM HistoryBucket WHERE Series = $s AND Tier = $t AND Start >= $from AND Start < $to ORDER BY Start", null, ("$s", series), ("$t", name), ("$from", from.UtcTicks), ("$to", to.UtcTicks));
            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            var result = new List<HistoryBucket>();
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                result.Add(new HistoryBucket(new DateTimeOffset(reader.GetInt64(0), TimeSpan.Zero), reader.GetInt64(1), reader.GetDouble(2), reader.GetDouble(3), reader.GetDouble(4), reader.GetDouble(5)));
            }

            return result;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<int> PruneAsync(DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await using var transaction = (SqliteTransaction)await _connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            var removed = await ExecuteAsync(_connection, "DELETE FROM HistoryRaw WHERE At < $cutoff", cancellationToken, transaction, ("$cutoff", (now - _options.RawRetention).UtcTicks)).ConfigureAwait(false);
            foreach (var tier in _options.Tiers)
            {
                removed += await ExecuteAsync(_connection, "DELETE FROM HistoryBucket WHERE Tier = $t AND Start < $cutoff", cancellationToken, transaction, ("$t", tier.Name), ("$cutoff", (now - tier.Retention).UtcTicks)).ConfigureAwait(false);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return removed;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync().ConfigureAwait(false);
        _gate.Dispose();
    }

    private SqliteCommand Command(string sql, SqliteTransaction? transaction, params (string Name, object Value)[] parameters) => Build(_connection, sql, transaction, parameters);

    private static async Task<int> ExecuteAsync(SqliteConnection connection, string sql, CancellationToken cancellationToken, SqliteTransaction? transaction = null, params (string Name, object Value)[] parameters)
    {
        await using var command = Build(connection, sql, transaction, parameters);
        return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static SqliteCommand Build(SqliteConnection connection, string sql, SqliteTransaction? transaction, (string Name, object Value)[] parameters)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Transaction = transaction;
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        return command;
    }
}
