using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Data.Common;

/// <summary>
/// Provides database query execution functionality with mapping support.
/// </summary>
/// <typeparam name="TDbOptions">The type of database options for connection configuration.</typeparam>
public class DatabaseQuery<TDbOptions> : IDatabaseQuery<TDbOptions>
{
    private readonly IDatabaseMapper _mapper;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the DatabaseQuery class.
    /// </summary>
    /// <param name="mapper">The database mapper for connection and command creation.</param>
    /// <param name="logger">The logger for command logging.</param>
    public DatabaseQuery(
        IDatabaseMapper mapper,
        ILogger<DatabaseQuery<TDbOptions>> logger
        )
    {
        _mapper = mapper;
        _logger = logger;
    }

    private void LogCommand(IDbCommand command)
    {
        _logger.LogInformation("{commandType}: {command}", command.CommandType, command.CommandText);

        _logger.LogDebug(
            "{commandType}: {command} ({parameters})",
            command.CommandType,
            command.CommandText,
            string.Join(Environment.NewLine + "\t", command.Parameters.OfType<IDbDataParameter>().Select(p => $"{p.ParameterName}={p.Value}"))
            );
    }

    /// <summary>
    /// Gets a database connection for the configured database options type.
    /// </summary>
    /// <returns>A database connection instance.</returns>
    public DbConnection GetConnection() => _mapper.GetConnection<TDbOptions>();

    /// <summary>
    /// Executes a stored procedure asynchronously and yields results as they are read.
    /// </summary>
    /// <typeparam name="T">The type of the query object.</typeparam>
    /// <typeparam name="TResult">The type of result objects to return.</typeparam>
    /// <param name="query">The query object containing stored procedure parameters.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Async enumerable of result objects.</returns>
    public IAsyncEnumerable<TResult> ExecuteStoredProcedureAsync<T, TResult>(
        T query,
#pragma warning disable CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
        [EnumeratorCancellation] CancellationToken cancellationToken = default
#pragma warning restore CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
        ) =>
        ExecuteStoredProcedureAsync<T, TResult>(query, _ => { }, cancellationToken);

    /// <summary>
    /// Executes a stored procedure asynchronously and yields results as they are read.
    /// Invokes a callback with the stored procedure return value.
    /// </summary>
    /// <typeparam name="T">The type of the query object.</typeparam>
    /// <typeparam name="TResult">The type of result objects to return.</typeparam>
    /// <param name="query">The query object containing stored procedure parameters.</param>
    /// <param name="resultCallback">Callback invoked with the stored procedure return value.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Async enumerable of result objects.</returns>
    public async IAsyncEnumerable<TResult> ExecuteStoredProcedureAsync<T, TResult>(T query, Action<int> resultCallback, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();
        using var command = _mapper.GetStoredProcedure(connection, query);

        var timeout = _mapper.GetCommandTimeout<TDbOptions>();
        if (timeout.HasValue)
            command.CommandTimeout = timeout.Value;

        LogCommand(command);

        var resultParameter = command.CreateParameter();
        resultParameter.ParameterName = "@RETURN_VALUE";
        resultParameter.DbType = DbType.Int64;
        resultParameter.Direction = ParameterDirection.ReturnValue;

        command.Parameters.Add(resultParameter);

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        Func<DbDataReader, TResult>? itemMap = null;
        while (!cancellationToken.IsCancellationRequested && await reader.ReadAsync(cancellationToken))
        {
            itemMap ??= _mapper.GetReaderMapper<TResult>(reader);
            yield return itemMap(reader);
        }

        await reader.NextResultAsync(cancellationToken);

        if (resultParameter.Value is int result)
            resultCallback(result);
    }

    /// <summary>
    /// Executes a database function asynchronously and returns the scalar result.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="function">The name of the database function to execute.</param>
    /// <param name="arguments">Optional arguments to pass to the function.</param>
    /// <returns>The function result, or default(T) if the result cannot be cast to type T.</returns>
    public async Task<T?> ExecuteFunctionAsync<T>(string function, params object?[] arguments)
    {
        using var connection = _mapper.GetConnection<TDbOptions>();

        using var command = connection.CreateCommand();

        var timeout = _mapper.GetCommandTimeout<TDbOptions>();
        if (timeout.HasValue)
            command.CommandTimeout = timeout.Value;

        command.CommandText = $"SELECT {function}({string.Join(", ", Enumerable.Range(0, arguments.Length).Select(p => $"@p{p}"))})";

        command.CommandType = CommandType.Text;
        for (var p = 0; p < arguments.Length; p++)
        {

            var resultParameter = command.CreateParameter();
            resultParameter.ParameterName = $"@p{p}";
            resultParameter.Value = arguments[p] ?? DBNull.Value;
            resultParameter.Direction = ParameterDirection.Input;
            command.Parameters.Add(resultParameter);
        }

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var result = await command.ExecuteScalarAsync();
        return result is T output ? output : default;
    }
}
