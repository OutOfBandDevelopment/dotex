using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading;

namespace OoBDev.Data.Common;

/// <summary>
/// Defines methods for executing database queries using specified database options.
/// </summary>
/// <typeparam name="TDbOptions">The type of the database options.</typeparam>
public interface IDatabaseQuery<TDbOptions>
{
    /// <summary>
    /// Executes a database function procedure asynchronously and retrieves the result
    /// </summary>
    /// <typeparam name="TResult">The type of the results returned by the stored procedure.</typeparam>
    /// <param name="function">The function to call</param>
    /// <param name="arguments">set of arguments to send to the function</param>
    /// <returns>An asynchronous stream of results from the stored procedure.</returns>
    Task<TResult?> ExecuteFunctionAsync<TResult>(string function, params object?[] arguments);

    /// <summary>
    /// Executes a stored procedure asynchronously and retrieves the results as an asynchronous stream.
    /// </summary>
    /// <typeparam name="T">The type of the query parameter.</typeparam>
    /// <typeparam name="TResult">The type of the results returned by the stored procedure.</typeparam>
    /// <param name="query">The query object containing the stored procedure parameters.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous stream of results from the stored procedure.</returns>
    /// <remarks>
    /// The <c>EnumeratorCancellation</c> attribute has no effect in this context but is included for compliance.
    /// </remarks>
#pragma warning disable CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
    IAsyncEnumerable<TResult> ExecuteStoredProcedureAsync<T, TResult>(T query, [EnumeratorCancellation] CancellationToken cancellationToken = default);
#pragma warning restore CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable

    /// <summary>
    /// Executes a stored procedure asynchronously, retrieves the results as an asynchronous stream,
    /// and provides progress updates via a callback.
    /// </summary>
    /// <typeparam name="T">The type of the query parameter.</typeparam>
    /// <typeparam name="TResult">The type of the results returned by the stored procedure.</typeparam>
    /// <param name="query">The query object containing the stored procedure parameters.</param>
    /// <param name="resultCallback">A callback action invoked with the result count for progress updates.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous stream of results from the stored procedure.</returns>
    /// <remarks>
    /// The <c>EnumeratorCancellation</c> attribute has no effect in this context but is included for compliance.
    /// </remarks>
#pragma warning disable CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
    IAsyncEnumerable<TResult> ExecuteStoredProcedureAsync<T, TResult>(T query, Action<int> resultCallback, [EnumeratorCancellation] CancellationToken cancellationToken = default);
#pragma warning restore CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
}
