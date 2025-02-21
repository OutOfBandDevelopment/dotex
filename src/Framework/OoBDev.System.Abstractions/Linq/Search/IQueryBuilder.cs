using OoBDev.System.ResponseModel;
using System.Linq;

namespace OoBDev.System.Linq.Search;

/// <summary>
/// Represents a query builder for executing queries with search parameters.
/// </summary>
public interface IQueryBuilder
{
    /// <summary>
    /// Executes a query using the specified <paramref name="query"/> and <paramref name="searchQuery"/>.
    /// </summary>
    /// <param name="query">The queryable data source.</param>
    /// <param name="searchQuery">The search parameters for filtering and sorting the data.</param>
    /// <returns>An <see cref="IQueryResult"/> containing the result of the executed query.</returns>
    IQueryResult ExecuteBy(IQueryable query, ISearchQuery searchQuery);
}

/// <summary>
/// Represents a typed query builder for executing queries with search parameters.
/// </summary>
/// <typeparam name="TModel">The type of the model in the query.</typeparam>
public interface IQueryBuilder<TModel> : IQueryBuilder
{
    /// <summary>
    /// Executes a typed query using the specified <paramref name="query"/> and <paramref name="searchQuery"/>.
    /// </summary>
    /// <param name="query">The typed queryable data source.</param>
    /// <param name="searchQuery">The search parameters for filtering and sorting the data.</param>
    /// <returns>An <see cref="IQueryResult{TModel}"/> containing the result of the executed query.</returns>
    IQueryResult<TModel> ExecuteBy(IQueryable<TModel> query, ISearchQuery searchQuery);
}
