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

public class DatabaseQuery<TDbOptions> : IDatabaseQuery<TDbOptions>
{
    private readonly IDatabaseMapper _mapper;
    private readonly ILogger _logger;

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

    public DbConnection GetConnection() => _mapper.GetConnection<TDbOptions>();

    public IAsyncEnumerable<TResult> ExecuteStoredProcedureAsync<T, TResult>(
        T query,
#pragma warning disable CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
        [EnumeratorCancellation] CancellationToken cancellationToken = default
#pragma warning restore CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
        ) =>
        ExecuteStoredProcedureAsync<T, TResult>(query, _ => { }, cancellationToken);

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
