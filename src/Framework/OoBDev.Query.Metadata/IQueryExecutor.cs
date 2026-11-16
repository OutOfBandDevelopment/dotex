using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Query.Metadata;

/// <summary>
/// Runs a compiled statement against a database. The adapter owns the driver; the engine stays database-free.
/// </summary>
public interface IQueryExecutor
{
    /// <summary>Runs a statement and reads every row.</summary>
    /// <param name="sql">The statement with <c>@name</c> placeholders.</param>
    /// <param name="parameters">The bound values by name, without the <c>@</c>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The rows as column name to value; database nulls are <see langword="null"/>.</returns>
    Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ReadAsync(string sql, IReadOnlyDictionary<string, object?> parameters, CancellationToken cancellationToken);
}
