using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Data.DesiredState;

/// <summary>
/// Runs a script through any ADO.NET provider: the caller supplies the connection for a target (Npgsql, SqlClient, Microsoft.Data.Sqlite, ...),
/// so this project needs no driver. The whole script is sent as one command; split on <c>GO</c> yourself for SQL Server batches.
/// </summary>
public sealed class AdoScriptExecutor(Func<DatabaseTarget, DbConnection> connectionFactory) : IScriptExecutor
{
    public async Task ExecuteAsync(DatabaseTarget target, string scriptName, string scriptText, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(target);
        var connection = connectionFactory(target);
        await using (connection.ConfigureAwait(false))
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            var command = connection.CreateCommand();
            await using (command.ConfigureAwait(false))
            {
                command.CommandText = scriptText;
                try
                {
                    await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (DbException ex)
                {
                    throw new InvalidOperationException($"Ensure script '{scriptName}' failed: {ex.Message}", ex);
                }
            }
        }
    }
}
