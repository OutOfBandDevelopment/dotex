using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OoBDev.Data.Common;
using Microsoft.Extensions.Configuration;
using OoBDev.System.Text.Json.Serialization;
using OoBDev.System.ComponentModel;
using Microsoft.Data.SqlClient;

namespace OoBDev.Microsoft.SqlServer.Server;

public class SqlDatabaseMapper : IDatabaseMapper
{
    private readonly IConfiguration _configuration;
    private readonly IJsonSerializer _serializer;

    public SqlDatabaseMapper(
        IConfiguration configuration,
        IJsonSerializer serializer
        )
    {
        _configuration = configuration;
        _serializer = serializer;
    }

    public IEnumerable<IDataParameter> GetCommandParameters<T>(T query)
    {
        foreach (var property in typeof(T).GetProperties())
        {
            var attribute = property.GetCustomAttribute<QueryParameterAttribute>();
            if (attribute == null) continue;
            var name = '@' + (attribute?.Name ?? property.Name).TrimStart('@');

            var value = property.GetValue(query);
            if (value is Array array)
            {
                value = _serializer.Serialize(array);
            }
            else if ((attribute?.IsJson ?? false) && value != null)
            {
                value = _serializer.Serialize(value);
            }

            yield return new SqlParameter(name, value ?? DBNull.Value);
        }
    }

    public string GetConnectionString<T>()
    {
        var attribute = typeof(T).GetCustomAttribute<ConnectionStringNameAttribute>() ??
            throw new NotSupportedException($"Missing {nameof(ConnectionStringNameAttribute)} on type {typeof(T)}");
        return _configuration.GetConnectionString(attribute.ConnectionStringName) ??
            throw new ApplicationException($"Missing Connection string for {attribute.ConnectionStringName}");
    }

    public int? GetCommandTimeout<T>()
    {
        var attribute = typeof(T).GetCustomAttribute<ConnectionStringNameAttribute>() ??
            throw new NotSupportedException($"Missing {nameof(ConnectionStringNameAttribute)} on type {typeof(T)}");

        return int.TryParse(_configuration[$"CommandTimeouts:{attribute.ConnectionStringName}"], out var value) ? value : null;
    }

    public DbConnection GetConnection<T>()
    {
        var connectionString = GetConnectionString<T>();
        var sqlConnection = new SqlConnection(connectionString);
        return sqlConnection;
    }

    public string GetStoredProcedureName<T>()
    {
        var attribute = typeof(T).GetCustomAttribute<StoredProcedureAttribute>() ??
            throw new NotSupportedException($"Missing {nameof(StoredProcedureAttribute)} on type {typeof(T)}");
        return attribute.StoredProcedureName;
    }

    public DbCommand GetStoredProcedure<T>(DbConnection sqlConnection, T query)
    {
        var commandString = GetStoredProcedureName<T>();
        var sqlCommand = sqlConnection.CreateCommand();
        sqlCommand.CommandText = commandString;
        sqlCommand.CommandType = CommandType.StoredProcedure;

        foreach (var parameter in GetCommandParameters(query))
            sqlCommand.Parameters.Add(parameter);

        return sqlCommand;
    }

    public Func<DbDataReader, TResult> GetReaderMapper<TResult>(DbDataReader reader)
    {
        var columns = reader.GetColumnSchema();
        var members = from member in typeof(TResult).GetProperties()
                      let mapping = member.GetCustomAttribute<QueryResultAttribute>()
                      let name = mapping?.Name ?? member.Name
                      let position = mapping?.Position ?? QueryResultAttribute.UndefinedPosition
                      select new
                      {
                          Info = member,
                          Name = name,
                          Position = position,
                      };

        var indexerProperty = typeof(DbDataReader).GetProperty("Item", typeof(object), new[] { typeof(int) })?.GetGetMethod()
            ?? throw new NotSupportedException("reader is missing integer indexer");

        var readerParameter = Expression.Parameter(typeof(DbDataReader), "reader");

        var mappingMethod = this.GetType().GetMethod(nameof(MakeSafe), BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new NotSupportedException($"Local static  method {nameof(MakeSafe)} not found");

        var ctor = typeof(TResult).GetConstructor(Type.EmptyTypes)
                ?? throw new NotSupportedException($"Default constructor for {typeof(TResult)} not found");

        var valueMap = from column in columns
                       from member in members
                       where column.ColumnName == member.Name || column.ColumnOrdinal == member.Position
                       select new
                       {
                           column.ColumnOrdinal,
                           MemberInfo = member.Info,

                           value = Expression.Call(null,
                               mappingMethod.MakeGenericMethod(member.Info.PropertyType),
                               readerParameter, Expression.Constant(column.ColumnOrdinal)),
                       };

        var bindings = from item in valueMap
                       select Expression.Bind(item.MemberInfo, item.value);

        var lambda = Expression.Lambda<Func<DbDataReader, TResult>>(
            Expression.MemberInit(Expression.New(ctor), bindings),
            readerParameter
        );

        return lambda.Compile();
    }

    internal static T? MakeSafe<T>(IDataRecord reader, int index) =>
        reader[index] is T mapped ? mapped : default;
}
