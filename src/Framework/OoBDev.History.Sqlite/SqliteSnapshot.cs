using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace OoBDev.History.Sqlite;

/// <summary>Consistent copies of a running SQLite database (the event store, the queue or the history store).</summary>
public static class SqliteSnapshot
{
    /// <summary>
    /// Writes the database behind <paramref name="connection"/> to <paramref name="destinationPath"/> with <c>VACUUM INTO</c>, a single
    /// read transaction, so writers can continue and the copy holds no half-written transaction. The destination must not exist.
    /// </summary>
    public static async Task CreateAsync(SqliteConnection connection, string destinationPath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        if (File.Exists(destinationPath))
        {
            throw new IOException($"Snapshot target '{destinationPath}' already exists.");
        }

        await using var command = connection.CreateCommand();
        command.CommandText = "VACUUM INTO $path";
        command.Parameters.AddWithValue("$path", destinationPath);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Opens the snapshot read-only and runs <c>PRAGMA integrity_check</c>; true when it reports <c>ok</c>.</summary>
    public static async Task<bool> VerifyAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = path, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString());
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA integrity_check";
        return string.Equals((string?)await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false), "ok", StringComparison.Ordinal);
    }
}
