using System;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Data.DesiredState;

/// <summary>
/// Runs a script through any ADO.NET provider: the caller supplies the connection for a target (Npgsql, SqlClient, Microsoft.Data.Sqlite, ...),
/// so this project needs no driver. A script is sent as one command, except that lines holding only <c>GO</c> split it into batches (SQL Server).
/// </summary>
public sealed partial class AdoScriptExecutor(Func<DatabaseTarget, DbConnection> connectionFactory) : IScriptExecutor
{
    /// <summary>Splits a script into batches on lines that hold only <c>GO</c>; empty batches are dropped.</summary>
    public static string[] SplitBatches(string scriptText)
    {
        ArgumentNullException.ThrowIfNull(scriptText);
        return [.. GoLine().Split(scriptText).Where(b => b.Trim().Length > 0)];
    }

    public async Task ExecuteAsync(DatabaseTarget target, string scriptName, string scriptText, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(target);
        var connection = connectionFactory(target);
        await using (connection.ConfigureAwait(false))
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            foreach (var batch in SplitBatches(scriptText))
            {
                var command = connection.CreateCommand();
                await using (command.ConfigureAwait(false))
                {
                    command.CommandText = batch;
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

    [GeneratedRegex(@"^[ \t]*GO[ \t]*\r?$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant, 200)]
    private static partial Regex GoLine();
}
