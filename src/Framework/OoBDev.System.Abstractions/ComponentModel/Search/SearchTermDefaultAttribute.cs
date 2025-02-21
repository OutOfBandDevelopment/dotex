using OoBDev.System.Linq.Search;
using System;

namespace OoBDev.System.ComponentModel.Search;

/// <summary>
/// provide the ability to control how search terms are handled if not wilded carded
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SearchTermDefaultAttribute(SearchTermDefaults @default) : Attribute, ISearchQueryIntercept
{
    /// <summary>
    /// rule to use for provided search term if not quoted
    /// </summary>
    public SearchTermDefaults Default { get; } = @default;

    /// <summary>
    /// use the `Default` to control pattern for searches without provided wild cards
    /// </summary>
    /// <param name="searchQuery"></param>
    /// <returns></returns>
    public ISearchQuery Intercept(ISearchQuery searchQuery)
    {
        if (searchQuery is SearchQuery query &&
            query.SearchTerm != null
            )
        {
            var searchTerm = query.SearchTerm;
            if (searchTerm.StartsWith('"') && searchTerm.EndsWith('"'))
            {
                return query with { SearchTerm = searchTerm[1..^1] };
            }

            if (!searchTerm.Contains('*'))
            {
                return query with
                {
                    SearchTerm = Default switch
                    {
                        SearchTermDefaults.StartsWith => searchTerm + "*",
                        SearchTermDefaults.EndsWith => "*" + searchTerm,
                        SearchTermDefaults.Contains => "*" + searchTerm + "*",
                        _ => searchTerm,
                    }
                };
            }
        }
        return searchQuery;
    }
}
