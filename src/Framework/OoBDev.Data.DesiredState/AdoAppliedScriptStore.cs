using System;
using System.Data.Common;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Data.DesiredState;

/// <summary>
/// Keeps the applied-script and refactor records in the target database (<c>__desired_state_history</c>), so they survive the process.
/// Works with any ADO.NET provider that accepts <c>@name</c> parameters (Npgsql, Microsoft.Data.Sqlite). The table is created on first use. When a plan may drop
/// (<see cref="ApplyOptions.AllowDataLoss"/>), add <see cref="SchemaScript"/> to the package schema so the table is part of the desired state.
/// </summary>
public sealed class AdoAppliedScriptStore(Func<DatabaseTarget, DbConnection> connectionFactory, TimeProvider? time = null) : IAppliedScriptStore
{
    public const string TableName = "__desired_state_history";

    /// <summary>The table as a schema script (no <c>IF NOT EXISTS</c>), identical to what the store creates.</summary>
    public static string SchemaScript { get; } =
        $"CREATE TABLE \"{TableName}\" (\"name\" varchar(400) NOT NULL, \"hash\" varchar(128) NOT NULL, \"applied_at\" varchar(40) NOT NULL, PRIMARY KEY (\"name\", \"hash\"));";

    public async Task<bool> HasAppliedAsync(DatabaseTarget target, string scriptName, string hash, CancellationToken cancellationToken = default)
    {
        var result = await RunAsync(target, $"SELECT COUNT(*) FROM \"{TableName}\" WHERE \"name\" = @name AND \"hash\" = @hash", scriptName, hash, null, cancellationToken).ConfigureAwait(false);
        return Convert.ToInt64(result, CultureInfo.InvariantCulture) > 0;
    }

    public async Task RecordAsync(DatabaseTarget target, string scriptName, string hash, CancellationToken cancellationToken = default)
    {
        var now = (time ?? TimeProvider.System).GetUtcNow().ToString("O", CultureInfo.InvariantCulture);
        await RunAsync(target, $"INSERT INTO \"{TableName}\" (\"name\", \"hash\", \"applied_at\") VALUES (@name, @hash, @at)", scriptName, hash, now, cancellationToken).ConfigureAwait(false);
    }

    private async Task<object?> RunAsync(DatabaseTarget target, string sql, string name, string hash, string? at, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(target);
        var connection = connectionFactory(target);
        await using (connection.ConfigureAwait(false))
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            await Execute(connection, SchemaScript.Replace("CREATE TABLE", "CREATE TABLE IF NOT EXISTS", StringComparison.Ordinal), null, cancellationToken).ConfigureAwait(false);
            return await Execute(connection, sql, (name, hash, at), cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task<object?> Execute(DbConnection connection, string sql, (string Name, string Hash, string? At)? values, CancellationToken cancellationToken)
    {
        var command = connection.CreateCommand();
        await using (command.ConfigureAwait(false))
        {
            command.CommandText = sql;
            if (values is { } v)
            {
                Add(command, "@name", v.Name);
                Add(command, "@hash", v.Hash);
                if (v.At is not null)
                {
                    Add(command, "@at", v.At);
                }
            }

            return sql.StartsWith("SELECT", StringComparison.Ordinal)
                ? await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false)
                : await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static void Add(DbCommand command, string name, string value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
