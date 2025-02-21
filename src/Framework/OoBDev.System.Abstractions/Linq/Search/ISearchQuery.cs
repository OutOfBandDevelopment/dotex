namespace OoBDev.System.Linq.Search;

/// <summary>
/// Represents a query object that combines page, sort, search term, and filter criteria for searching data.
/// </summary>
public interface ISearchQuery : IPageQuery, ISortQuery, ISearchTermQuery, IFilterQuery
{
}

/// <summary>
/// Represents a generic query object that combines page, sort, search term, and filter criteria for searching data of type <typeparamref name="TModel"/>.
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
public interface ISearchQuery<TModel> : ISearchQuery
{
}
