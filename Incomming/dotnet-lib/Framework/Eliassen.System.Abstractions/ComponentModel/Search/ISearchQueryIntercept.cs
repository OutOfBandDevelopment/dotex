using Eliassen.System.Linq.Search;

namespace Eliassen.System.ComponentModel.Search;

/// <summary>
/// Provide entry point to commonly intercept and override search definitions.
/// 
/// Example <seealso cref="SearchTermDefaultAttribute"/>
/// </summary>
public interface ISearchQueryIntercept
{
    /// <summary>
    /// modify or pass though search query before processing.
    /// </summary>
    /// <param name="searchQuery"></param>
    /// <returns></returns>
    ISearchQuery Intercept(ISearchQuery searchQuery);
}
