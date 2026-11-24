using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using OoBDev.WebApi.Models;

namespace OoBDev.WebApi.Services;

/// <summary>Runs one checked SELECT against the configured instance with the read-only intent, a row cap and a timeout.</summary>
/// <param name="options">The runner settings.</param>
public class ReadOnlyQueryRunner(IOptions<DataAccessOptions> options)
{
    /// <summary>Gets a value indicating whether a connection string is configured.</summary>
    public virtual bool IsConfigured => !string.IsNullOrWhiteSpace(options.Value.ConnectionString);

    /// <summary>Runs the query.</summary>
    /// <param name="sql">The SELECT text; the caller has already checked it.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The columns and capped rows.</returns>
    public virtual async Task<DataQueryResultModel> RunAsync(string sql, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var builder = new SqlConnectionStringBuilder(settings.ConnectionString) { ApplicationIntent = ApplicationIntent.ReadOnly };
        var maxRows = Math.Clamp(settings.MaxRows, 1, 10_000);
        var clock = Stopwatch.StartNew();
        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandTimeout = Math.Clamp(settings.TimeoutSeconds, 1, 300);
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);

        var columns = new List<string>(reader.FieldCount);
        for (var i = 0; i < reader.FieldCount; i++)
        {
            columns.Add(reader.GetName(i));
        }

        var rows = new List<IReadOnlyList<string?>>();
        var truncated = false;
        while (await reader.ReadAsync(cancellationToken))
        {
            if (rows.Count >= maxRows)
            {
                truncated = true;
                break;
            }

            var row = new string?[columns.Count];
            for (var i = 0; i < row.Length; i++)
            {
                row[i] = await reader.IsDBNullAsync(i, cancellationToken) ? null : Convert.ToString(await reader.GetFieldValueAsync<object>(i, cancellationToken), CultureInfo.InvariantCulture);
            }

            rows.Add(row);
        }

        return new DataQueryResultModel { Columns = columns, Rows = rows, Truncated = truncated, ElapsedMs = clock.ElapsedMilliseconds };
    }
}
