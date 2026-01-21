using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System;

namespace OoBDev.Data.Common;

/// <summary>
/// Provides mapping functionality between application objects and database commands, connections, and parameters.
/// </summary>
public interface IDatabaseMapper
{
    /// <summary>
    /// Gets the command parameters from the specified query object.
    /// </summary>
    /// <typeparam name="T">The type of the query object.</typeparam>
    /// <param name="query">The query object containing parameter values.</param>
    /// <returns>A collection of <see cref="IDataParameter"/> objects representing the command parameters.</returns>
    IEnumerable<IDataParameter> GetCommandParameters<T>(T query);

    /// <summary>
    /// Gets the connection string for the specified database options type.
    /// </summary>
    /// <typeparam name="TDbOptions">The type of the database options configuration.</typeparam>
    /// <returns>The connection string configured for the specified database options type.</returns>
    string GetConnectionString<TDbOptions>();

    /// <summary>
    /// Creates and returns a database connection for the specified database options type.
    /// </summary>
    /// <typeparam name="TDbOptions">The type of the database options configuration.</typeparam>
    /// <returns>A <see cref="DbConnection"/> instance configured for the specified database options type.</returns>
    DbConnection GetConnection<TDbOptions>();

    /// <summary>
    /// Gets the command timeout value for the specified database options type.
    /// </summary>
    /// <typeparam name="TDbOptions">The type of the database options configuration.</typeparam>
    /// <returns>The command timeout in seconds, or <c>null</c> to use the default timeout.</returns>
    int? GetCommandTimeout<TDbOptions>();

    /// <summary>
    /// Gets the stored procedure name for the specified query type.
    /// </summary>
    /// <typeparam name="T">The type of the query object.</typeparam>
    /// <returns>The name of the stored procedure associated with the query type.</returns>
    string GetStoredProcedureName<T>();

    /// <summary>
    /// Creates a database command configured to execute the stored procedure for the specified query.
    /// </summary>
    /// <typeparam name="T">The type of the query object.</typeparam>
    /// <param name="sqlConnection">The database connection to use for the command.</param>
    /// <param name="query">The query object containing parameter values.</param>
    /// <returns>A <see cref="DbCommand"/> configured to execute the stored procedure with the specified parameters.</returns>
    DbCommand GetStoredProcedure<T>(DbConnection sqlConnection, T query);

    /// <summary>
    /// Gets a function that maps rows from a <see cref="DbDataReader"/> to result objects.
    /// </summary>
    /// <typeparam name="TResult">The type of the result object.</typeparam>
    /// <param name="reader">The data reader to create the mapper for.</param>
    /// <returns>A function that takes a <see cref="DbDataReader"/> and returns a mapped <typeparamref name="TResult"/> object.</returns>
    Func<DbDataReader, TResult> GetReaderMapper<TResult>(DbDataReader reader);
}
